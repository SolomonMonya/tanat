using System;
using System.Runtime.CompilerServices;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class PlayerStore : Store<Player>
	{
		public delegate void OnKill(Player _victim, int _killerId);

		private int mSelfPlayerId;

		private int mSelfTeam = -1;

		private HeroStore mHeroes;

		private BattleTimer mTimer;

		private CtrlAvatarStore mCtrlAvatarStore;

		private OnKill mOnKill = delegate
		{
		};

		private HandlerManager<BattlePacket, BattleCmdId> mHandlerMgr;

		[CompilerGenerated]
		private static OnKill _003C_003E9__CachedAnonymousMethodDelegate1;

		public PlayerStore(int _selfPlayerId, HeroStore _heroes, BattleTimer _timer, CtrlAvatarStore _avatarStore)
			: base("Players")
		{
			if (_heroes == null)
			{
				throw new ArgumentNullException("_heroes");
			}
			if (_timer == null)
			{
				throw new ArgumentNullException("_timer");
			}
			mSelfPlayerId = _selfPlayerId;
			mHeroes = _heroes;
			mTimer = _timer;
			mCtrlAvatarStore = _avatarStore;
		}

		public void Subscribe(HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			_handlerMgr.Subscribe<RegPlayerArg>(BattleCmdId.PLAYER_REG, null, null, OnReg);
			_handlerMgr.Subscribe<UnregPlayerArg>(BattleCmdId.PLAYER_UNREG, null, null, OnUnreg);
			_handlerMgr.Subscribe<PlayerStatsArg>(BattleCmdId.PLAYER_STATS, null, null, OnStatus);
			_handlerMgr.Subscribe<RespawnArg>(BattleCmdId.RESPAWN, null, null, OnRespawn);
			_handlerMgr.Subscribe<OnlineArg>(BattleCmdId.PLAYER_ONLINE, null, null, OnOnline);
			_handlerMgr.Subscribe<BuffArg>(BattleCmdId.ADD_BUFF, null, null, OnBuffAdded);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
			mSelfTeam = -1;
		}

		public void SubscribeOnKill(OnKill _callback)
		{
			mOnKill = (OnKill)Delegate.Combine(mOnKill, _callback);
		}

		public void UnsubscribeOnKill(OnKill _callback)
		{
			mOnKill = (OnKill)Delegate.Remove(mOnKill, _callback);
		}

		private void OnReg(RegPlayerArg _arg)
		{
			Log.Debug("id: " + _arg.mPlayerId + ", name: " + _arg.mName + " _arg.mTeam: " + _arg.mTeam + " _arg.mAvatar " + _arg.mAvatar);
			bool flag = _arg.mPlayerId == mSelfPlayerId;
			if (flag)
			{
				mSelfTeam = _arg.mTeam;
			}
			Player player = new Player(_arg.mPlayerId, _arg.mName, _arg.mTeam, flag);
			player.SetAvatarData(mCtrlAvatarStore.TryGet(_arg.mAvatar));
			player.SetTimer(mTimer);
			mHeroes.AddByPlayerId(_arg.mPlayerId);
			player.SetHeroProvider(mHeroes.GetContentProvider());
			Add(player);
		}

		private void OnUnreg(UnregPlayerArg _arg)
		{
			Log.Debug("id: " + _arg.mPlayerId);
			if (_arg.mPlayerId == mSelfPlayerId)
			{
				mSelfTeam = -1;
			}
			Player player = Get(_arg.mPlayerId);
			if (player != null)
			{
				if (player.Team > 0)
				{
					UserLog.AddAction(UserActionType.NOTHER_LEAVE, player.Team, string.Format("{0}({1}) team:{2}", player.Name, _arg.mPlayerId, (player.Team == mSelfTeam) ? "Friend" : "Enemy"));
				}
				player.Avatar?.Data.UnbindPlayer();
				Remove(_arg.mPlayerId);
			}
		}

		private void OnStatus(PlayerStatsArg _arg)
		{
			Player player = Get(_arg.mPlayerId);
			if (player == null)
			{
				return;
			}
			player.KillsCount = _arg.mKillsCnt;
			player.AssistsCount = _arg.mAssistsCnt;
			player.Level = _arg.mLevel;
			if (_arg.mDeathsCnt != player.DeathsCount)
			{
				int deathsCount = player.DeathsCount;
				player.DeathsCount = _arg.mDeathsCnt;
				if (player.DeathsCount > deathsCount)
				{
					mOnKill(player, _arg.mLastKiller);
				}
			}
		}

		private void OnRespawn(RespawnArg _arg)
		{
			Player player = Get(_arg.mPlayerId);
			if (player == null)
			{
				return;
			}
			if (_arg.mContainsTime)
			{
				player.SetRespTimeData(_arg.mTime);
				if (_arg.mContainsCost)
				{
					if (_arg.mRealCost > 0)
					{
						player.SetRespCostData(_arg.mRealCost, Currency.REAL);
					}
					else if (_arg.mVirtualCost > 0)
					{
						player.SetRespCostData(_arg.mVirtualCost, Currency.VIRTUAL);
					}
				}
			}
			else
			{
				player.PerformRespawn();
			}
		}

		private void OnOnline(OnlineArg _arg)
		{
			Player player = Get(_arg.mPlayerId);
			if (player != null)
			{
				player.IsOnline = _arg.mIsOnline;
			}
		}

		private void OnBuffAdded(BuffArg _arg)
		{
			Player player = Get(_arg.mId);
			if (player == null)
			{
				Log.Warning("Player not found");
			}
			else
			{
				player.Hero.GameInfo.mBuffs[_arg.mBuff] = _arg.mEndTime;
			}
		}
	}
}
