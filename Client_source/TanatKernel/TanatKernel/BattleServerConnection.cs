using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class BattleServerConnection : NetSystem.ConnectionListener
	{
		public delegate void DisconnectCallback(bool _state, string _host, string _port);

		private class State
		{
			public BattleServerData mSrvData;

			public BattleMapData mMapData;
		}

		private const double mTimeRequestDt = 5.0;

		private const double mPingUpdDt = 1.0;

		private DisconnectCallback mFullDisconnectCallback = delegate
		{
		};

		private DisconnectCallback mDisconnectCallback = delegate
		{
		};

		private bool mWaitReconnect;

		private State mCurState;

		private State mInProgressState;

		private bool mDeferredReconnect;

		private int mBattleId;

		private BattlePacketManager mPacketMgr = new BattlePacketManager();

		private NetSystem mNetSys;

		private IBattleHolder mBattleHolder;

		private UserNetData mUserData;

		private IMapManager mMapMgr;

		private BattleTimer mTimer;

		private DateTime mPrevTimeSending;

		public bool mIsObserver;

		private DateTime mPrevPingUpdTime;

		public static bool mIsReady;

		[CompilerGenerated]
		private static DisconnectCallback _003C_003E9__CachedAnonymousMethodDelegate2;

		[CompilerGenerated]
		private static DisconnectCallback _003C_003E9__CachedAnonymousMethodDelegate3;

		[CompilerGenerated]
		private static DisconnectCallback _003C_003E9__CachedAnonymousMethodDelegate5;

		[CompilerGenerated]
		private static DisconnectCallback _003C_003E9__CachedAnonymousMethodDelegate7;

		public BattlePacketManager PacketMgr => mPacketMgr;

		public int BattleId => mBattleId;

		public BattleServerConnection(NetSystem _netSys, UserNetData _userData, IBattleHolder _battleHoder)
		{
			if (_netSys == null)
			{
				throw new ArgumentNullException("_netSys");
			}
			if (_battleHoder == null)
			{
				throw new ArgumentNullException("_battleHoder");
			}
			if (_userData == null)
			{
				throw new ArgumentNullException("_userData");
			}
			mNetSys = _netSys;
			mBattleHolder = _battleHoder;
			mUserData = _userData;
			mPacketMgr.HandlerMgr.Subscribe<ConnectArg>(BattleCmdId.CONNECT, null, OnConnectFailed, OnConnect);
			mPacketMgr.HandlerMgr.Subscribe(BattleCmdId.READY, OnReady, OnReadyFailed);
			mPacketMgr.HandlerMgr.Subscribe(BattleCmdId.ENTER, OnEntered, OnReadyFailed);
		}

		public void SetMapManager(IMapManager _mapMgr)
		{
			mMapMgr = _mapMgr;
		}

		public void SetTimer(BattleTimer _timer)
		{
			mTimer = _timer;
		}

		public void Update()
		{
			if (base.IsConnectedMode)
			{
				if (mTimer != null)
				{
					if (DateTime.Now.Subtract(mPrevTimeSending).TotalSeconds > 5.0)
					{
						SendTimeRequest();
						mPrevTimeSending = DateTime.Now;
					}
					mTimer.Update();
				}
				mPacketMgr.Update();
				mPacketMgr.SendOutcomings(mNetSys, base.ConnectionId);
				if (DateTime.Now.Subtract(mPrevPingUpdTime).TotalSeconds > 1.0)
				{
					mPacketMgr.UpdatePing();
					mPrevPingUpdTime = DateTime.Now;
				}
			}
			if (mDeferredReconnect && mUserData.Inited)
			{
				bool flag = mNetSys.IsConnected(base.ConnectionId);
				Log.Debug("current connection state: " + flag);
				if (flag)
				{
					Disconnect();
				}
				else
				{
					Connect();
				}
				mDeferredReconnect = false;
			}
		}

		public override void ConnectionComplete(int _connectionId, int _port)
		{
			base.ConnectionComplete(_connectionId, _port);
			mCurState = mInProgressState;
			mInProgressState = null;
			SendConnect();
		}

		public override void DisconnectionComplete()
		{
			base.DisconnectionComplete();
			Log.Debug(mUserData.UserName + " disconnected");
			mTimer = null;
			if (mBattleHolder.IsBattleCreated())
			{
				mBattleHolder.DisableBattle();
				mBattleHolder.DestroyBattle();
			}
			mPacketMgr.Clear();
			mDisconnectCallback(_state: true, base.EndPoint.Address.ToString(), base.EndPoint.Port.ToString());
			mDisconnectCallback = delegate
			{
			};
			if (mInProgressState != null)
			{
				Connect();
			}
			else if (!mWaitReconnect && !mReconnecting)
			{
				if (mNeedMicroReconnect)
				{
					mReconnecting = true;
					ReconnectStart(0);
					mInProgressState = mCurState;
					Connect();
				}
				else
				{
					CallFullDisconnectCallback(_state: true);
				}
			}
		}

		public override void ConnectionFailed()
		{
			base.ConnectionFailed();
			if (mReconnecting && mCurrentAttempt < mAttemptMaxCount)
			{
				Thread.Sleep(mAttemptTime);
				mCurrentAttempt++;
				ReconnectStart(mCurrentAttempt);
				Log.Info("MicroReconnect attempt #" + mCurrentAttempt);
				Connect();
			}
			else
			{
				mInProgressState = null;
				Log.Warning(mUserData.UserName + " connection failed");
				CallFullDisconnectCallback(_state: false);
			}
		}

		public override void Parse(byte[] _buffer, int _offset, int _size)
		{
			mPacketMgr.Parse(_buffer, _offset, _size);
		}

		public void Connect(BattleServerData _battleSrvData, BattleMapData _battleMapData)
		{
			mWaitReconnect = false;
			if (_battleSrvData == null)
			{
				throw new ArgumentNullException();
			}
			Log.Debug(_battleSrvData.ToString());
			if (_battleMapData != null)
			{
				Log.Debug(_battleMapData.ToString());
			}
			if (mInProgressState != null)
			{
				Log.Warning("another connection process hasn't been released");
				return;
			}
			mInProgressState = new State();
			mInProgressState.mSrvData = _battleSrvData;
			mInProgressState.mMapData = _battleMapData;
			if (mLastPort != -1)
			{
				Log.Debug("Has already used port : " + mLastPort);
				List<int> list = new List<int>(mInProgressState.mSrvData.mPorts);
				if (list.Contains(mLastPort))
				{
					list.Remove(mLastPort);
					list.Insert(0, mLastPort);
					mInProgressState.mSrvData.mPorts = list.ToArray();
				}
			}
			mDeferredReconnect = true;
		}

		public void Disconnect()
		{
			mNetSys.Disconnect(base.ConnectionId);
		}

		private void Connect()
		{
			mNetSys.Connect(mInProgressState.mSrvData.mHost, mInProgressState.mSrvData.mPorts, this);
		}

		private void OnConnect(ConnectArg _arg)
		{
			mIsReady = false;
			SendTimeRequest();
			Log.Notice("battle id: " + _arg.mBattleId);
			mBattleId = _arg.mBattleId;
			mBattleHolder.CreateBattle(mPacketMgr, _arg.mSelfPlayerId);
			if (mMapMgr != null)
			{
				mMapMgr.LoadBattleMap(mCurState.mMapData, mIsObserver, new Notifier<IMapManager, object>(OnMapLoaded, null));
				mIsObserver = false;
			}
			else
			{
				OnMapLoaded(_success: true, null, null);
			}
		}

		private void OnConnectFailed(int _errorCode)
		{
			Log.Error("battle login failed");
			Disconnect();
		}

		private void OnMapLoaded(bool _success, IMapManager _mapMgr, object _data)
		{
			if (_success)
			{
				Log.Debug("map loaded");
				mPacketMgr.Send(BattleCmdId.ENTER);
				mBattleHolder.StartBattle();
			}
			else
			{
				Disconnect();
			}
		}

		private void OnReady()
		{
			mIsReady = true;
			Log.Debug("ready complete");
		}

		private void OnReadyFailed(int _errorCode)
		{
			Log.Error("battle ready failed");
			Disconnect();
		}

		private void OnEntered()
		{
			Log.Debug("ready");
			mPacketMgr.Send(BattleCmdId.READY);
			SendTimeRequest();
		}

		private void SendConnect()
		{
			NamedVar namedVar = new NamedVar("clientId", mUserData.UserId);
			BattleServerData mSrvData = mCurState.mSrvData;
			NamedVar namedVar2 = new NamedVar("pass", mSrvData.mPasswd);
			if (mSrvData.mDebugJoin == null)
			{
				mPacketMgr.Send(BattleCmdId.CONNECT, namedVar, namedVar2);
				return;
			}
			MixedArray mixedArray = new MixedArray();
			mixedArray["name"] = mUserData.UserName;
			mixedArray["battle"] = mSrvData.mDebugJoin.mBattleId;
			mixedArray["map"] = mSrvData.mDebugJoin.mMapId;
			mixedArray["team"] = mSrvData.mDebugJoin.mTeam;
			mixedArray["avatar"] = mSrvData.mDebugJoin.mAvatarPrefab;
			mixedArray["avatar_params"] = mSrvData.mDebugJoin.mAvatarParams;
			mixedArray["money"] = 100500;
			mixedArray["money_real"] = 100500;
			mPacketMgr.Send(BattleCmdId.CONNECT, namedVar, namedVar2, new NamedVar("debug", mixedArray));
		}

		public void SendReady()
		{
			mPacketMgr.Send(BattleCmdId.READY);
		}

		public void SendTimeRequest()
		{
			Log.Debug("SendTimeRequest");
			mPacketMgr.Send(BattleCmdId.GET_TIME);
		}

		public void SendMovePlayer(float _x, float _y, bool _rel)
		{
			if (base.IsConnectedMode)
			{
				MixedArray mixedArray = new MixedArray();
				mixedArray["x"] = _x;
				mixedArray["y"] = _y;
				mPacketMgr.Send(BattleCmdId.MOVE_PLAYER, new NamedVar("targetPos", mixedArray), new NamedVar("rel", _rel));
			}
		}

		public void SendMovePlayer(float _x, float _y)
		{
			SendMovePlayer(_x, _y, _rel: true);
		}

		public void SendStopPlayer(bool _stop)
		{
			if (base.IsConnectedMode)
			{
				mPacketMgr.Send(BattleCmdId.STOP_PLAYER, new NamedVar("stop", _stop));
			}
		}

		public void SendPlayerState(int _stateId, bool _isPet)
		{
			mPacketMgr.Send(BattleCmdId.SET_STATE, new NamedVar("state", _stateId), new NamedVar("pet", _isPet));
		}

		public void SendDoAction(int _obj, int _action, int _target, float _targetX, float _targetY)
		{
			MixedArray mixedArray = new MixedArray();
			mixedArray["x"] = _targetX;
			mixedArray["y"] = _targetY;
			mPacketMgr.Send(BattleCmdId.DO_ACTION, new NamedVar("id", _obj), new NamedVar("action", _action), new NamedVar("target", _target), new NamedVar("targetPos", mixedArray));
		}

		public void SendUpgradeSkill(int _skillId, int _points)
		{
			mPacketMgr.Send(BattleCmdId.UPGRADE_SKILL, new NamedVar("id", _skillId), new NamedVar("points", _points));
		}

		public void SendBuy(int _shopId, int _sellerId, int _itemId, int _cnt)
		{
			mPacketMgr.Send(BattleCmdId.BUY, new NamedVar("shopId", _shopId), new NamedVar("sellerId", _sellerId), new NamedVar("itemId", _itemId), new NamedVar("count", _cnt));
		}

		public void SendEquip(int _itemId)
		{
			mPacketMgr.Send(BattleCmdId.EQUIP_ITEM, new NamedVar("id", _itemId));
		}

		public void SendUse(int _objId)
		{
			mPacketMgr.Send(BattleCmdId.USE_OBJECT, new NamedVar("id", _objId));
		}

		public void SendGetDropInfo(int _objId)
		{
			mPacketMgr.Send(BattleCmdId.GET_DROP_INFO, new NamedVar("id", _objId));
		}

		public void SendPickUp(int _objId)
		{
			mPacketMgr.Send(BattleCmdId.PICK_UP, new NamedVar("id", _objId));
		}

		public void SendDropItem(int _objId, int _count)
		{
			mPacketMgr.Send(BattleCmdId.DROP_ITEM, new NamedVar("id", _objId), new NamedVar("count", _count));
		}

		public void SendForceRespawn()
		{
			mPacketMgr.Send(BattleCmdId.FORCE_RESPAWN);
		}

		public void SendSetBeacon(float _x, float _y)
		{
			MixedArray mixedArray = new MixedArray();
			mixedArray.Set(new NamedVar("x", _x));
			mixedArray.Set(new NamedVar("y", _y));
			mPacketMgr.Send(BattleCmdId.SET_BEACON, new NamedVar("pos", mixedArray));
		}

		public void SubscribeDisconnect(DisconnectCallback _callback)
		{
			mDisconnectCallback = (DisconnectCallback)Delegate.Combine(mDisconnectCallback, _callback);
		}

		public void SubscribeFullDisconnect(DisconnectCallback _callback)
		{
			mFullDisconnectCallback = (DisconnectCallback)Delegate.Combine(mFullDisconnectCallback, _callback);
		}

		private void CallFullDisconnectCallback(bool _state)
		{
			mFullDisconnectCallback(_state, base.EndPoint.Address.ToString(), base.EndPoint.Port.ToString());
			mFullDisconnectCallback = delegate
			{
			};
		}

		public void WaitReconnect()
		{
			mWaitReconnect = true;
		}

		public void StopWaitReconnect()
		{
			mWaitReconnect = false;
		}
	}
}
