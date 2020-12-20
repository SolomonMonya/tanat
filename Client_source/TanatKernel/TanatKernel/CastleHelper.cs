using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class CastleHelper
	{
		public delegate void LaunchCallback(BattleServerData _battleSrvData, BattleMapData _mapData);

		private LaunchCallback mLaunchCallback = delegate
		{
		};

		private BattleServerData mBattleSrvData;

		private BattleMapData mMapData;

		private CastleSender mSender;

		private ICastleJoinGui mGui;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		private CastleStartBattleInfoArg mStartTime;

		private FightHelper mFightHelper;

		private Clan mClan;

		private RequestHistory mHistory = new RequestHistory();

		[CompilerGenerated]
		private static LaunchCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public CastleStartBattleInfoArg StartTime
		{
			get
			{
				return mStartTime;
			}
			private set
			{
				mStartTime = value;
			}
		}

		public int SelfUserId => mClan.SelfMemberId;

		public int SelfClanId => mClan.Id;

		public void BindGui(ICastleJoinGui _gui)
		{
			if (_gui == null)
			{
				throw new ArgumentNullException("_gui");
			}
			mGui = _gui;
			mGui.Init();
			mHistory.BindGui(mGui);
		}

		public void UnbindGui()
		{
			mGui = null;
		}

		public CastleHelper(CastleSender _sender, FightHelper _helper, Clan _clan)
		{
			if (_sender == null)
			{
				throw new ArgumentNullException("_sender");
			}
			if (_helper == null)
			{
				throw new ArgumentNullException("_helper");
			}
			if (_clan == null)
			{
				throw new ArgumentNullException("_clan");
			}
			mSender = _sender;
			mFightHelper = _helper;
			mClan = _clan;
			Clan clan = mClan;
			clan.mOnSelfUpdated = (Clan.Action)Delegate.Combine(clan.mOnSelfUpdated, new Clan.Action(OnSelfUpdated));
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<CastleListArg>(CtrlCmdId.castle.list, null, ShowMessage, OnCastleList);
			mHandlerMgr.Subscribe<CastleMembersArg>(CtrlCmdId.castle.info, null, ShowMessage, OnCastleMembers);
			mHandlerMgr.Subscribe<CastleBattleInfoArg>(CtrlCmdId.castle.battle_info, null, ShowMessage, OnCastleBattleInfo);
			mHandlerMgr.Subscribe(CtrlCmdId.castle.desert, OnDeserted, ShowMessage);
			mHandlerMgr.Subscribe<CastleFighterListArg>(CtrlCmdId.castle.fighters, null, OnFightersError, OnFightersRequest);
			mHandlerMgr.Subscribe<CastleHistoryArg>(CtrlCmdId.castle.history, null, ShowMessage, OnCastleHistory);
			mHandlerMgr.Subscribe(CtrlCmdId.castle.set_fighters, OnJoined, OnSetFightersError);
			mHandlerMgr.Subscribe<DeferredMessageArg>(CtrlCmdId.common.message_mpd, null, null, OnMessage);
			mHandlerMgr.Subscribe<CastleStartBattleInfoArg>(CtrlCmdId.castle.start_battle_info_mpd, null, null, OnTimerInfo);
			mHandlerMgr.Subscribe<FightJoinArg>(CtrlCmdId.fight.join, null, null, OnMapQueueJoined);
			mHandlerMgr.Subscribe<RemoveUserMpdArg>(CtrlCmdId.clan.remove_user_mpd, null, null, OnUserRemovedBroadcast);
			mHandlerMgr.Subscribe(CtrlCmdId.clan.remove, OnClanRemoved, null);
			mHandlerMgr.Subscribe<CastleStartRequestArg>(CtrlCmdId.castle.start_request_mpd, null, ShowMessage, OnStartRequest);
			mHandlerMgr.Subscribe<CastleSelectAvatarTimerArg>(CtrlCmdId.castle.select_avatar_timer_mpd, null, ShowMessage, OnAvatarTimer);
			mHandlerMgr.Subscribe<CastleSelectAvatarArg>(CtrlCmdId.castle.select_avatar_mpd, null, ShowMessage, OnAvatarSelected);
			mHandlerMgr.Subscribe<CastleReadyArg>(CtrlCmdId.castle.ready_mpd, null, ShowMessage, OnReady);
			mHandlerMgr.Subscribe<CastleDesertArg>(CtrlCmdId.castle.desert_battle_mpd, null, ShowMessage, OnPlayerDeserted);
			mHandlerMgr.Subscribe<CastleLaunchArg>(CtrlCmdId.castle.launch_mpd, null, ShowMessage, OnLaunched);
			mHandlerMgr.Subscribe(CtrlCmdId.castle.early_won_mpd, OnEarlyWon, ShowMessage);
			mHandlerMgr.Subscribe(CtrlCmdId.castle.select_avatar, null, OnAvatarNotSelected);
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
			mClan = null;
			mHandlerMgr = null;
			mSender = null;
			mFightHelper = null;
		}

		public void CastleListRequest()
		{
			mSender.CastleListRequest();
		}

		public void RefreshSelfClanInfo()
		{
			mClan.RefreshInfo(_showInfo: false);
		}

		public void SetFighters(int _castleId, Dictionary<int, int> _fighters)
		{
			mSender.SetFighters(_castleId, _fighters);
		}

		public void GetBattleInfo(int _castleId, int _battleId)
		{
			mSender.GetBattleInfo(_castleId, _battleId);
		}

		public void GetCurrentBattleInfo(int _castleId)
		{
			mSender.GetCurrentBattleInfo(_castleId);
		}

		public void GetFighters(int _castleId)
		{
			mSender.GetFighters(_castleId);
		}

		public void GetCastleInfo(int _castleId)
		{
			mSender.GetCastleInfo(_castleId);
		}

		public void DesertCastleRequest(int _castleId)
		{
			mSender.DesertCastlerequest(_castleId);
		}

		public void SelectAvatar(int _id)
		{
			mSender.SelectAvatar(_id);
		}

		public void SetReady()
		{
			mSender.SetReady();
		}

		public void DesertBattle()
		{
			mHistory.Clear();
			mSender.DesertBattle();
		}

		public void GetClanInfo(int _clanId)
		{
			mClan.GetClanInfo(_clanId.ToString(), "", "");
		}

		public void GetCastleHistory(int _castleId)
		{
			mSender.GetCastleHistory(_castleId);
		}

		private void ShowMessage(int _error)
		{
			mGui.ShowMessage(_error);
		}

		private void OnAvatarNotSelected(int _error)
		{
			if (mGui != null)
			{
				mGui.SelfAvatarError();
			}
		}

		private void OnMessage(DeferredMessageArg _arg)
		{
			if (_arg.mType == 2)
			{
				StartTime = null;
			}
			if (mGui != null)
			{
				mGui.ShowDeferredMessage(_arg);
			}
		}

		private void OnFightersError(int _error)
		{
			if (mGui != null)
			{
				mGui.FightersErrorOccured(_error);
			}
		}

		private bool IsCanJoin()
		{
			if (mClan.Id <= 0)
			{
				return false;
			}
			Clan.ClanMember selfMember = mClan.SelfMember;
			if (selfMember == null)
			{
				return false;
			}
			if (selfMember.Role >= Clan.Role.COMMANDER)
			{
				return true;
			}
			return false;
		}

		private void OnCastleList(CastleListArg _arg)
		{
			foreach (CastleInfo mCastle in _arg.mCastles)
			{
				if (mCastle.mOwnerId == mClan.Id && mClan.Id > 0)
				{
					mCastle.mSelfOwner = true;
				}
			}
			if (mGui != null)
			{
				mGui.SetCastleList(_arg);
			}
		}

		private void OnCastleMembers(CastleMembersArg _arg)
		{
			if (mGui != null)
			{
				mGui.SetCastleMembers(_arg);
			}
		}

		private void OnCastleBattleInfo(CastleBattleInfoArg _arg)
		{
			if (mGui != null)
			{
				mGui.SetCastleBattleInfo(_arg);
			}
		}

		private void OnEarlyWon()
		{
			mHistory.Clear();
			if (mGui != null)
			{
				mGui.Won();
			}
		}

		private void OnJoined()
		{
			if (mGui != null)
			{
				mGui.Notify("GUI_CASTLE_IN_BATTLE");
				mSender.GetFighters(mGui.LastRequest);
			}
		}

		private void OnSetFightersError(int _error)
		{
			ShowMessage(_error);
			if (mGui != null)
			{
				mSender.GetFighters(mGui.LastRequest);
			}
		}

		private void OnDeserted()
		{
			if (mGui != null)
			{
				mGui.Desert();
			}
		}

		private void OnSelfUpdated()
		{
			if (mGui != null)
			{
				mGui.ClanInfoUpdated();
			}
		}

		private void OnCastleHistory(CastleHistoryArg _arg)
		{
			if (mGui != null)
			{
				mGui.SetCastleHistory(_arg.mHistory);
			}
		}

		private void OnFightersRequest(CastleFighterListArg _arg)
		{
			Dictionary<int, List<Clan.ClanMember>> dictionary = new Dictionary<int, List<Clan.ClanMember>>();
			foreach (Clan.ClanMember member in mClan.Members)
			{
				if (_arg.mFighters.ContainsKey(member.Id))
				{
					if (!dictionary.ContainsKey(_arg.mFighters[member.Id]))
					{
						dictionary[_arg.mFighters[member.Id]] = new List<Clan.ClanMember>();
					}
					dictionary[_arg.mFighters[member.Id]].Add(member);
				}
			}
			if (mGui != null)
			{
				mGui.SetFighters(dictionary, mClan.Tag, _arg.mCanLeave);
			}
		}

		private void OnTimerInfo(CastleStartBattleInfoArg _arg)
		{
			StartTime = _arg;
			if (mGui != null)
			{
				mGui.SetTimerMessage(_arg);
			}
		}

		private void OnMapQueueJoined(FightJoinArg _arg)
		{
			int mMapId = _arg.mMapId;
			if (mMapId < 0)
			{
				return;
			}
			MapData mapData = mFightHelper.FindMapById(mMapId);
			if (mapData != null)
			{
				MapType mType = (MapType)mapData.mType;
				if (mGui != null && (mType == MapType.DOTA || mType == MapType.DM) && StartTime != null && StartTime.mStartTime > DateTime.Now)
				{
					mGui.Notify("Castle_Warning_Text");
				}
			}
		}

		private void OnUserRemovedBroadcast(RemoveUserMpdArg _arg)
		{
			if (_arg.mUserId == mClan.SelfMemberId)
			{
				OnTimerInfo(null);
			}
		}

		private void OnClanRemoved()
		{
			OnTimerInfo(null);
		}

		private void OnStartRequest(CastleStartRequestArg _arg)
		{
			mHistory.SetRequest(_arg);
			mFightHelper.ClearHistory();
			mFightHelper.CloseAutoAcceptTimer();
			if (mGui != null)
			{
				mGui.SetTimerMessage(null);
				mGui.StartCastleBattle(_arg);
			}
		}

		private void OnAvatarTimer(CastleSelectAvatarTimerArg _arg)
		{
			mHistory.SetTimer(_arg);
			if (mGui != null)
			{
				mGui.SetAvatarTimer(_arg.mTimer, _arg.mFightersId);
			}
		}

		private void OnAvatarSelected(CastleSelectAvatarArg _arg)
		{
			mHistory.AvatarSelected(_arg.mId, _arg.mAvatar);
			if (mGui != null)
			{
				mGui.SetAvatar(_arg.mId, _arg.mAvatar);
			}
		}

		private void OnReady(CastleReadyArg _arg)
		{
			mHistory.SetReady(_arg.mId);
			if (mGui != null)
			{
				mGui.SetReadyStatus(_arg.mId);
			}
		}

		private void OnPlayerDeserted(CastleDesertArg _arg)
		{
			mHistory.SetDesert(_arg.mId);
			if (mGui != null)
			{
				mGui.PlayerDeserted(_arg.mId);
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

		private void OnLaunched(CastleLaunchArg _arg)
		{
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
			mGui.SetTimer(num);
			Timer timer = new Timer();
			timer.AutoReset = false;
			timer.Interval = num * 1000;
			timer.Enabled = true;
			timer.Elapsed += OnLaunchTime;
		}

		private void OnLaunchTime(object sender, ElapsedEventArgs e)
		{
			Log.Debug("OnLaunchTime");
			mHistory.Clear();
			mGui.SetTimerMessage(null);
			StartTime = null;
			UnbindGui();
			mLaunchCallback(mBattleSrvData, mMapData);
		}
	}
}
