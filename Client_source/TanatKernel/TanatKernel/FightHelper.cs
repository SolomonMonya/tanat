using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class FightHelper
	{
		public delegate void LaunchCallback(BattleServerData _battleSrvData, BattleMapData _mapData);

		private const double mRequestTimeLimit = 5.0;

		private LaunchCallback mLaunchCallback = delegate
		{
		};

		private BattleServerData mBattleSrvData;

		private BattleMapData mMapData;

		private FightSender mSender;

		private IFightJoinGui mGui;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		private RequestHistory mHistory = new RequestHistory();

		private Dictionary<int, MapData> mMaps = new Dictionary<int, MapData>();

		private Timer mAutoAcceptTimer;

		private readonly int mAcceptTime = 20;

		private Dictionary<int, DateTime> mJoined = new Dictionary<int, DateTime>();

		private int mCurrentJoin = -1;

		private DateTime mLastRequestTime;

		private bool mIsTutor;

		private int mCurrentDesert = -1;

		private int mSelectedAvatar = -1;

		private FightStartSelectAvatarMpdArg mFightData;

		private DateTime mTimeToStart;

		private bool mWeInLobby;

		private List<int> mInLobby = new List<int>();

		private Dictionary<int, int> mAvatars = new Dictionary<int, int>();

		private List<int> mReady = new List<int>();

		[CompilerGenerated]
		private static LaunchCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public void BindGui(IFightJoinGui _gui)
		{
			mCurrentDesert = -1;
			mCurrentJoin = -1;
			mLastRequestTime = DateTime.Now.AddSeconds(-5.0);
			if (_gui == null)
			{
				throw new ArgumentNullException("_gui");
			}
			mGui = _gui;
			if (mFightData != null)
			{
				OnAvatarLobby(mFightData);
			}
			else
			{
				foreach (KeyValuePair<int, DateTime> item in mJoined)
				{
					JoinedQueue joinedQueue = new JoinedQueue();
					joinedQueue.MapData = FindMapById(item.Key);
					joinedQueue.StartTime = item.Value;
					mGui.Joined(joinedQueue, isNew: false);
				}
			}
			mGui.Binded();
			RefreshMapStatus();
		}

		public void UnbindGui()
		{
			mGui = null;
		}

		public void MapsInfo()
		{
			mSender.MapsInfo();
		}

		public FightHelper(FightSender _sender)
		{
			if (_sender == null)
			{
				throw new ArgumentNullException("_sender");
			}
			mSender = _sender;
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<FightJoinArg>(CtrlCmdId.fight.join, null, OnJoinError, OnJoined);
			mHandlerMgr.Subscribe<FightInviteMpdArg>(CtrlCmdId.fight.invite_mpd, null, OnCommonError, OnInviteRequest);
			mHandlerMgr.Subscribe<FightSelectAvatarMpdArg>(CtrlCmdId.fight.select_avatar_mpd, null, OnCommonError, OnAvatarSelected);
			mHandlerMgr.Subscribe<FightDesertMpdArg>(CtrlCmdId.fight.desert_mpd, null, OnCommonError, OnDeserted);
			mHandlerMgr.Subscribe<FightInRequestMpd>(CtrlCmdId.fight.in_request_mpd, null, OnCommonError, OnRequestJoined);
			mHandlerMgr.Subscribe<FightReadyMpdArg>(CtrlCmdId.fight.ready_mpd, null, OnCommonError, OnPlayerReady);
			mHandlerMgr.Subscribe<FightLaunchArg>(CtrlCmdId.fight.launch_mpd, null, OnCommonError, OnBattleLaunched);
			mHandlerMgr.Subscribe<FightStartSelectAvatarMpdArg>(CtrlCmdId.fight.start_select_avatar_mpd, null, OnCommonError, OnAvatarLobby);
			mHandlerMgr.Subscribe<FightDesertArg>(CtrlCmdId.fight.desert, null, OnFightExitError, OnFightExit);
			mHandlerMgr.Subscribe<FightAnswerArg>(CtrlCmdId.fight.answer, null, OnCommonError, OnAnswer);
			mHandlerMgr.Subscribe(CtrlCmdId.fight.in_request, OnInRequest, OnCommonError);
			mHandlerMgr.Subscribe<MapInfoArg>(CtrlCmdId.arena.get_maps_info, null, null, OnMapsInfo);
			mHandlerMgr.Subscribe(CtrlCmdId.fight.select_avatar, null, OnSelfAvatarError);
			mHandlerMgr.Subscribe<HuntJoinArg>(CtrlCmdId.hunt.join, null, OnCommonError, OnHuntJoin);
			mHandlerMgr.Subscribe<HuntReadyArg>(CtrlCmdId.hunt.ready, null, OnCommonError, OnHuntReady);
		}

		private void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
			}
			mHandlerMgr = null;
		}

		public void Clear()
		{
			Unsubscribe();
			mHandlerMgr = null;
			mSender = null;
		}

		private void RefreshMapStatus()
		{
			List<MapData> list = new List<MapData>();
			foreach (KeyValuePair<int, MapData> mMap in mMaps)
			{
				MapData value = mMap.Value;
				value.mUsed = false;
				if (mJoined.ContainsKey(mMap.Key))
				{
					value.mUsed = true;
				}
				list.Add(value);
			}
			mGui.ShowMapsDesc(list);
		}

		private void OnSelfAvatarError(int _error)
		{
			mGui.SelfAvatarError();
		}

		private void OnJoinError(int _error)
		{
			mCurrentJoin = -1;
			OnCommonError(_error);
		}

		private void OnJoined(FightJoinArg _arg)
		{
			mCurrentJoin = -1;
			if (_arg.mTime > 0)
			{
				mGui.BanMessage(_arg.mTime);
			}
			else if (!BattleReady())
			{
				mJoined[_arg.mMapId] = DateTime.Now;
				JoinedQueue joinedQueue = new JoinedQueue();
				joinedQueue.MapData = FindMapById(_arg.mMapId);
				joinedQueue.StartTime = DateTime.Now;
				if (mGui != null)
				{
					mGui.Joined(joinedQueue, isNew: true);
				}
				RefreshMapStatus();
			}
		}

		private void OnInviteRequest(FightInviteMpdArg _arg)
		{
			FightDesertArg fightDesertArg = new FightDesertArg();
			fightDesertArg.mMapId = _arg.mMapId;
			OnFightExit(fightDesertArg);
			if (mGui != null)
			{
				mGui.FriendInvite(_arg.mNick, _arg.mMapId, _arg.mIsLeave);
			}
		}

		private void OnAvatarSelected(FightSelectAvatarMpdArg _arg)
		{
			mAvatars[_arg.mUserId] = _arg.mAvatarId;
			if (mGui != null)
			{
				mGui.SetAvatar(_arg.mUserId, _arg.mAvatarId);
			}
		}

		private void OnDeserted(FightDesertMpdArg _arg)
		{
			UserLog.AddAction(UserActionType.FIGHT_BROKEN);
			StopAutoAcceptTimer();
			ClearHistory();
			mJoined[_arg.mMapId] = DateTime.Now;
			if (mGui != null)
			{
				RefreshMapStatus();
				JoinedQueue joinedQueue = new JoinedQueue();
				joinedQueue.MapData = FindMapById(_arg.mMapId);
				joinedQueue.StartTime = DateTime.Now;
				mGui.PlayerDeserted();
				mGui.Joined(joinedQueue, isNew: false);
			}
			else
			{
				mWeInLobby = false;
			}
		}

		private void OnRequestJoined(FightInRequestMpd _arg)
		{
			string text = string.Empty;
			foreach (Fighter item in mFightData.mTeam1)
			{
				if (item.mId == _arg.mUserId)
				{
					text = (string.IsNullOrEmpty(item.mTag) ? item.mNick : ("[" + item.mTag + "]" + item.mNick));
					text = $"{text} Level: {item.mLevel}. Team: {item.mTeam}";
				}
			}
			foreach (Fighter item2 in mFightData.mTeam2)
			{
				if (item2.mId == _arg.mUserId)
				{
					text = (string.IsNullOrEmpty(item2.mTag) ? item2.mNick : ("[" + item2.mTag + "]" + item2.mNick));
					text = $"{text} Level: {item2.mLevel}. Team: {item2.mTeam}";
				}
			}
			UserLog.AddAction(UserActionType.PLAYER_IN_LOBBY, _arg.mUserId, text);
			mInLobby.Add(_arg.mUserId);
			if (mGui != null)
			{
				mGui.PlayerJoined(_arg.mUserId);
			}
		}

		private void OnPlayerReady(FightReadyMpdArg _arg)
		{
			mReady.Add(_arg.mUserId);
			if (mGui != null)
			{
				mGui.SetReady(_arg.mUserId);
			}
		}

		public void SubscribeLaunch(LaunchCallback _callback)
		{
			mLaunchCallback = (LaunchCallback)Delegate.Combine(mLaunchCallback, _callback);
		}

		public void UnsubscribeLaunch(LaunchCallback _callback)
		{
			mLaunchCallback = (LaunchCallback)Delegate.Remove(mLaunchCallback, _callback);
		}

		private void OnBattleLaunched(FightLaunchArg _arg)
		{
			ClearHistory();
			mWeInLobby = false;
			BattleServerData battleServerData = new BattleServerData();
			battleServerData.mHost = _arg.mHost;
			battleServerData.mPorts = _arg.mPorts;
			battleServerData.mPasswd = _arg.mPasswd;
			Log.Debug("Host:" + _arg.mHost);
			BattleMapData battleMapData = new BattleMapData();
			battleMapData.mMapName = _arg.mMap;
			mBattleSrvData = battleServerData;
			mMapData = battleMapData;
			int num = 5;
			double totalSeconds = (mTimeToStart - DateTime.Now).TotalSeconds;
			bool flag = true;
			if (_arg.mMapId > 0)
			{
				flag = 4 != FindMapById(_arg.mMapId).mType;
			}
			if (mIsTutor || (totalSeconds <= 0.0 && flag))
			{
				mIsTutor = false;
				Log.Debug("Start without timer");
				OnLaunchTime(null, null);
				return;
			}
			Log.Debug("Start 5 second timer");
			mGui.SetTimer(num);
			Timer timer = new Timer();
			timer.AutoReset = false;
			timer.Interval = num * 1000;
			timer.Enabled = true;
			timer.Elapsed += OnLaunchTime;
		}

		private void OnAutoAccept(object sender, ElapsedEventArgs e)
		{
			JoinRequest();
		}

		private void StopAutoAcceptTimer()
		{
			if (mAutoAcceptTimer != null)
			{
				mAutoAcceptTimer.Enabled = false;
				mAutoAcceptTimer.Close();
				mAutoAcceptTimer = null;
			}
		}

		private void OnLaunchTime(object sender, ElapsedEventArgs e)
		{
			Log.Debug("OnLaunchTime");
			mHistory.Clear();
			UnbindGui();
			mLaunchCallback(mBattleSrvData, mMapData);
		}

		private void OnAvatarLobby(FightStartSelectAvatarMpdArg _arg)
		{
			UserLog.AddAction(UserActionType.FIGHT_START);
			FightStartSelectAvatarMpdArg fightStartSelectAvatarMpdArg = mFightData;
			mFightData = _arg;
			if (FindMapById(mFightData.mMapId).mType != 4)
			{
				mJoined = new Dictionary<int, DateTime>();
				RefreshMapStatus();
			}
			mTimeToStart = DateTime.Now.AddSeconds(_arg.mWaitTime);
			if (mGui == null)
			{
				return;
			}
			if (mWeInLobby)
			{
				mSender.JoinRequest();
				return;
			}
			Log.Debug("PlayersFound");
			bool needClear = false;
			if (fightStartSelectAvatarMpdArg != null && FindMapById(fightStartSelectAvatarMpdArg.mMapId).mType == 4)
			{
				needClear = true;
			}
			mGui.PlayersFound(_arg.mMapId, _arg.mWaitTime, needClear);
			int num = _arg.mWaitTime - mAcceptTime;
			if (num > 0)
			{
				mAutoAcceptTimer = new Timer();
				mAutoAcceptTimer.AutoReset = false;
				mAutoAcceptTimer.Interval = num * 1000;
				mAutoAcceptTimer.Enabled = true;
				mAutoAcceptTimer.Elapsed += OnAutoAccept;
			}
			else
			{
				OnAutoAccept(null, null);
			}
		}

		private void OnHuntJoin(HuntJoinArg _arg)
		{
			mFightData = new FightStartSelectAvatarMpdArg();
			mFightData.mAddStats = _arg.mAddStats;
			mFightData.mAvatars = _arg.mAvatars;
			mFightData.mMapId = _arg.mMapId;
			mFightData.mTeam1.Add(_arg.mFighter);
			UserLog.AddAction(UserActionType.SELECT_AVATAR_SCREEN_HUNT);
			mGui.SetBattleInfo(mFightData, 0);
			mGui.PlayerJoined(_arg.mFighter.mId);
		}

		private void OnHuntReady(HuntReadyArg _arg)
		{
			OnBattleLaunched(_arg.mLaunchArg);
		}

		private void OnFightExitError(int _error)
		{
			mCurrentDesert = -1;
			OnCommonError(_error);
		}

		private void OnFightExit(FightDesertArg _arg)
		{
			mCurrentDesert = -1;
			if (mGui != null)
			{
				mGui.ExitFromQueue(_arg.mMapId);
			}
			if (mJoined.ContainsKey(_arg.mMapId))
			{
				mJoined.Remove(_arg.mMapId);
			}
			RefreshMapStatus();
		}

		private void OnAnswer(FightAnswerArg _arg)
		{
			if (_arg.mAnswer)
			{
				FightJoinArg fightJoinArg = new FightJoinArg();
				fightJoinArg.mMapId = _arg.mMapId;
				OnJoined(fightJoinArg);
			}
			else
			{
				FightDesertArg fightDesertArg = new FightDesertArg();
				fightDesertArg.mMapId = _arg.mMapId;
				OnFightExit(fightDesertArg);
			}
		}

		private void OnMapsInfo(MapInfoArg _arg)
		{
			foreach (MapDataDesc item in _arg.mMapsDesc)
			{
				mMaps[item.mMapId] = item;
			}
			RefreshMapStatus();
		}

		private void OnCommonError(int _error)
		{
			if (mGui != null)
			{
				mGui.ShowError(_error);
			}
		}

		public bool JoinBattle(int _mapId)
		{
			if (BattleReady())
			{
				OnCommonError(8011);
				return false;
			}
			MapData mapData = FindMapById(_mapId);
			if (mapData == null)
			{
				Log.Error("cannot find map " + _mapId);
				return false;
			}
			if (mapData.mType == 4)
			{
				mSender.JoinHunt(_mapId);
			}
			else
			{
				if ((DateTime.Now - mLastRequestTime).TotalSeconds <= 5.0)
				{
					mGui.ShowMessage("GUI_JOIN_OFTEN");
					return false;
				}
				if (!IsBanned() && mCurrentJoin < 0)
				{
					mSender.Join(_mapId);
					mCurrentJoin = _mapId;
					mLastRequestTime = DateTime.Now;
				}
			}
			return true;
		}

		public void Desert(int _mapId)
		{
			if (mCurrentDesert < 0)
			{
				mSender.Desert(_mapId);
				mCurrentDesert = _mapId;
			}
		}

		public void InviteAnswer(int _mapId, bool _answer)
		{
			if (FindMapById(_mapId).mType == 4)
			{
				if (_answer)
				{
					mSender.JoinHunt(_mapId);
				}
			}
			else
			{
				mSender.GroupAnswer(_mapId, _answer);
			}
		}

		private bool IsHunt()
		{
			return FindMapById(mFightData.mMapId).mType == 4;
		}

		public void SelectAvatar(int _avatarId)
		{
			if (IsHunt())
			{
				mSelectedAvatar = _avatarId;
				mGui.SetAvatar(mFightData.mTeam1[0].mId, _avatarId);
			}
			else
			{
				mSender.SelectAvatar(_avatarId);
			}
		}

		private void OnInRequest()
		{
			int num = (int)(mTimeToStart - DateTime.Now).TotalSeconds;
			UserLog.AddAction(UserActionType.SELECT_AVATAR_SCREEN, "wait time: " + (mFightData.mWaitTime - num));
			mGui.SetBattleInfo(mFightData, num);
		}

		public void JoinRequest()
		{
			if (!BattleReady())
			{
				Log.Error("Try to set lobby for empty battle");
				return;
			}
			mWeInLobby = true;
			mSender.JoinRequest();
			StopAutoAcceptTimer();
		}

		public void SetReady(int _id)
		{
			if (IsHunt())
			{
				mSender.HuntReady(mFightData.mMapId, mSelectedAvatar);
				mGui.SetReady(_id);
			}
			else
			{
				mSender.Ready();
			}
		}

		public void TutorialHuntReady(int _mapId, int _avatarId)
		{
			mIsTutor = true;
			mSender.HuntReady(_mapId, _avatarId);
		}

		public bool IsBanned()
		{
			return false;
		}

		public MapData FindMapById(int _mapId)
		{
			if (mMaps.ContainsKey(_mapId))
			{
				return mMaps[_mapId];
			}
			return null;
		}

		public void CloseAutoAcceptTimer()
		{
			if (mAutoAcceptTimer != null)
			{
				mAutoAcceptTimer.Enabled = false;
				mAutoAcceptTimer.Close();
				mAutoAcceptTimer = null;
			}
		}

		public void ExitFromLobby()
		{
			mWeInLobby = false;
			ClearHistory();
		}

		public bool BattleReady()
		{
			return mFightData != null;
		}

		public IEnumerable<int> PlayersInLobby()
		{
			return mInLobby.AsReadOnly();
		}

		public IEnumerable<int> PlayersReady()
		{
			return mReady.AsReadOnly();
		}

		public IEnumerable<KeyValuePair<int, int>> SelectedAvatars()
		{
			return mAvatars;
		}

		public bool IsMapJoined(int _mapId)
		{
			return mJoined.ContainsKey(_mapId);
		}

		public void ClearHistory()
		{
			mInLobby.Clear();
			mAvatars = new Dictionary<int, int>();
			mReady.Clear();
			mFightData = null;
		}

		public void MicroReconnected()
		{
			CloseAutoAcceptTimer();
			ClearHistory();
			mJoined = new Dictionary<int, DateTime>();
			mWeInLobby = false;
		}
	}
}
