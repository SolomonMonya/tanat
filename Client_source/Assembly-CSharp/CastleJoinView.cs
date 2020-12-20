using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;
using TanatKernel;

public class CastleJoinView : ICastleJoinGui
{
	private enum Text
	{
		IN_BATTLE = 1,
		DESERTED = 2,
		NOT_ENOUGH_PEOPLE = 3,
		ERROR = 9000,
		WRONG_PARAMETERS = 9001,
		USER_NOT_IN_CLAN = 9002,
		NO_PERMISSIONS = 9003,
		CLAN_NOT_IN_QUEUE = 9004,
		WRONG_FIGHTERS = 9005,
		WRONG_LEVEL = 9006
	}

	public delegate void Started();

	private CastleMenu mCastleMenu;

	private WorldMapMenu mWorldMapMenu;

	private CastleRequestMenu mCastleRequestMenu;

	private MapJoinQueueMenu mJoinMenu;

	private SelectAvatarWindow mSelectAvatar;

	private ScreenManager mScreenMgr;

	private CastleHistoryMenu mCastleHistoryMenu;

	public Started mOnStarted;

	public CastleHelper.LaunchCallback mObserverCallback = delegate
	{
	};

	private CastleInfo mCurrentCastleInfo;

	private CastleInfo mCurrentRequest;

	private bool mIsRightLevel;

	private int mBanCount;

	private int mLastRequest;

	private CastleHelper mHelper;

	private OkDialog mOkDialog;

	public int LastRequest => mLastRequest;

	public CastleJoinView(CastleHelper _helper)
	{
		if (_helper == null)
		{
			throw new ArgumentNullException("_helper");
		}
		mHelper = _helper;
	}

	public void Uninit()
	{
		if (mWorldMapMenu != null)
		{
			WorldMapMenu worldMapMenu = mWorldMapMenu;
			worldMapMenu.mCastleCallback = (WorldMapMenu.CastleCallback)Delegate.Remove(worldMapMenu.mCastleCallback, new WorldMapMenu.CastleCallback(CastleCallback));
		}
		if (mCastleMenu != null)
		{
			CastleMenu castleMenu = mCastleMenu;
			castleMenu.mOnCastleRequest = (CastleMenu.OnCastleRequest)Delegate.Remove(castleMenu.mOnCastleRequest, new CastleMenu.OnCastleRequest(OnJoinBattle));
			CastleMenu castleMenu2 = mCastleMenu;
			castleMenu2.mOnClanInfo = (CastleMenu.OnClanInfo)Delegate.Remove(castleMenu2.mOnClanInfo, new CastleMenu.OnClanInfo(OnClanInfo));
			CastleMenu castleMenu3 = mCastleMenu;
			castleMenu3.mOnCastleHistoryRequest = (CastleMenu.OnCastleRequest)Delegate.Remove(castleMenu3.mOnCastleHistoryRequest, new CastleMenu.OnCastleRequest(OnCastleHistoryRequest));
			CastleMenu castleMenu4 = mCastleMenu;
			castleMenu4.mOnObserveBattle = (CastleMenu.OnObserveBattle)Delegate.Remove(castleMenu4.mOnObserveBattle, new CastleMenu.OnObserveBattle(OnObserveBattle));
		}
		if (mCastleRequestMenu != null)
		{
			CastleRequestMenu castleRequestMenu = mCastleRequestMenu;
			castleRequestMenu.mCancelRequest = (CastleRequestMenu.CancelRequest)Delegate.Remove(castleRequestMenu.mCancelRequest, new CastleRequestMenu.CancelRequest(DesertCallback));
			CastleRequestMenu castleRequestMenu2 = mCastleRequestMenu;
			castleRequestMenu2.mAcceptFighters = (CastleRequestMenu.AcceptFighters)Delegate.Remove(castleRequestMenu2.mAcceptFighters, new CastleRequestMenu.AcceptFighters(SetFighters));
			CastleRequestMenu castleRequestMenu3 = mCastleRequestMenu;
			castleRequestMenu3.mOnClanInfo = (CastleRequestMenu.OnClanInfo)Delegate.Remove(castleRequestMenu3.mOnClanInfo, new CastleRequestMenu.OnClanInfo(OnClanInfo));
		}
		if (mCastleHistoryMenu != null)
		{
			CastleHistoryMenu castleHistoryMenu = mCastleHistoryMenu;
			castleHistoryMenu.mOnClanInfo = (CastleHistoryMenu.OnInfoById)Delegate.Remove(castleHistoryMenu.mOnClanInfo, new CastleHistoryMenu.OnInfoById(OnClanInfo));
			CastleHistoryMenu castleHistoryMenu2 = mCastleHistoryMenu;
			castleHistoryMenu2.mOnBattleHistoryInfo = (CastleHistoryMenu.OnBattleInfoById)Delegate.Remove(castleHistoryMenu2.mOnBattleHistoryInfo, new CastleHistoryMenu.OnBattleInfoById(OnBattleHistoryrequest));
		}
		if (mSelectAvatar != null)
		{
			SelectAvatarWindow selectAvatarWindow = mSelectAvatar;
			selectAvatarWindow.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Remove(selectAvatarWindow.mDesertCallback, new SelectAvatarWindow.DesertCallback(DesertBattle));
			SelectAvatarWindow selectAvatarWindow2 = mSelectAvatar;
			selectAvatarWindow2.mReadyCallback = (SelectAvatarWindow.ReadyCallback)Delegate.Remove(selectAvatarWindow2.mReadyCallback, new SelectAvatarWindow.ReadyCallback(Ready));
			SelectAvatarWindow selectAvatarWindow3 = mSelectAvatar;
			selectAvatarWindow3.mSelectAvatarCallback = (SelectAvatarWindow.SelectAvatarCallback)Delegate.Remove(selectAvatarWindow3.mSelectAvatarCallback, new SelectAvatarWindow.SelectAvatarCallback(OnSelectAvatarCallback));
		}
		mCastleMenu = null;
		mWorldMapMenu = null;
		mOkDialog = null;
		mHelper = null;
		mSelectAvatar = null;
		mCastleRequestMenu = null;
	}

	public void SetData(CastleMenu _castleMenu, WorldMapMenu _worldMapMenu, Clan _clan, OkDialog _ok, CastleRequestMenu _requestMenu, MapJoinQueueMenu _joinMenu, ScreenManager _screenMgr, SelectAvatarWindow _selectAvatar, CastleHistoryMenu _castleHistoryMenu)
	{
		if (mWorldMapMenu != null)
		{
			WorldMapMenu worldMapMenu = mWorldMapMenu;
			worldMapMenu.mCastleCallback = (WorldMapMenu.CastleCallback)Delegate.Remove(worldMapMenu.mCastleCallback, new WorldMapMenu.CastleCallback(CastleCallback));
		}
		if (mCastleMenu != null)
		{
			CastleMenu castleMenu = mCastleMenu;
			castleMenu.mOnCastleRequest = (CastleMenu.OnCastleRequest)Delegate.Remove(castleMenu.mOnCastleRequest, new CastleMenu.OnCastleRequest(OnJoinBattle));
			CastleMenu castleMenu2 = mCastleMenu;
			castleMenu2.mOnClanInfo = (CastleMenu.OnClanInfo)Delegate.Remove(castleMenu2.mOnClanInfo, new CastleMenu.OnClanInfo(OnClanInfo));
			CastleMenu castleMenu3 = mCastleMenu;
			castleMenu3.mOnCastleHistoryRequest = (CastleMenu.OnCastleRequest)Delegate.Remove(castleMenu3.mOnCastleHistoryRequest, new CastleMenu.OnCastleRequest(OnCastleHistoryRequest));
			CastleMenu castleMenu4 = mCastleMenu;
			castleMenu4.mOnObserveBattle = (CastleMenu.OnObserveBattle)Delegate.Remove(castleMenu4.mOnObserveBattle, new CastleMenu.OnObserveBattle(OnObserveBattle));
		}
		if (mCastleRequestMenu != null)
		{
			CastleRequestMenu castleRequestMenu = mCastleRequestMenu;
			castleRequestMenu.mCancelRequest = (CastleRequestMenu.CancelRequest)Delegate.Remove(castleRequestMenu.mCancelRequest, new CastleRequestMenu.CancelRequest(DesertCallback));
			CastleRequestMenu castleRequestMenu2 = mCastleRequestMenu;
			castleRequestMenu2.mAcceptFighters = (CastleRequestMenu.AcceptFighters)Delegate.Remove(castleRequestMenu2.mAcceptFighters, new CastleRequestMenu.AcceptFighters(SetFighters));
		}
		if (mCastleHistoryMenu != null)
		{
			CastleHistoryMenu castleHistoryMenu = mCastleHistoryMenu;
			castleHistoryMenu.mOnClanInfo = (CastleHistoryMenu.OnInfoById)Delegate.Remove(castleHistoryMenu.mOnClanInfo, new CastleHistoryMenu.OnInfoById(OnClanInfo));
			CastleHistoryMenu castleHistoryMenu2 = mCastleHistoryMenu;
			castleHistoryMenu2.mOnBattleHistoryInfo = (CastleHistoryMenu.OnBattleInfoById)Delegate.Remove(castleHistoryMenu2.mOnBattleHistoryInfo, new CastleHistoryMenu.OnBattleInfoById(OnBattleHistoryrequest));
		}
		if (mSelectAvatar != null)
		{
			SelectAvatarWindow selectAvatarWindow = mSelectAvatar;
			selectAvatarWindow.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Remove(selectAvatarWindow.mDesertCallback, new SelectAvatarWindow.DesertCallback(DesertBattle));
			SelectAvatarWindow selectAvatarWindow2 = mSelectAvatar;
			selectAvatarWindow2.mReadyCallback = (SelectAvatarWindow.ReadyCallback)Delegate.Remove(selectAvatarWindow2.mReadyCallback, new SelectAvatarWindow.ReadyCallback(Ready));
			SelectAvatarWindow selectAvatarWindow3 = mSelectAvatar;
			selectAvatarWindow3.mSelectAvatarCallback = (SelectAvatarWindow.SelectAvatarCallback)Delegate.Remove(selectAvatarWindow3.mSelectAvatarCallback, new SelectAvatarWindow.SelectAvatarCallback(OnSelectAvatarCallback));
			mSelectAvatar.Uninit();
		}
		if (_clan == null)
		{
			throw new ArgumentNullException("_clan");
		}
		if (_ok == null)
		{
			throw new ArgumentNullException("_ok");
		}
		if (_joinMenu == null)
		{
			throw new ArgumentNullException("_joinMenu");
		}
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		mCastleMenu = _castleMenu;
		mWorldMapMenu = _worldMapMenu;
		mOkDialog = _ok;
		mCastleRequestMenu = _requestMenu;
		mJoinMenu = _joinMenu;
		mScreenMgr = _screenMgr;
		mSelectAvatar = _selectAvatar;
		mCastleHistoryMenu = _castleHistoryMenu;
		if (mWorldMapMenu != null)
		{
			WorldMapMenu worldMapMenu2 = mWorldMapMenu;
			worldMapMenu2.mCastleCallback = (WorldMapMenu.CastleCallback)Delegate.Combine(worldMapMenu2.mCastleCallback, new WorldMapMenu.CastleCallback(CastleCallback));
		}
		if (mCastleMenu != null)
		{
			CastleMenu castleMenu5 = mCastleMenu;
			castleMenu5.mOnCastleRequest = (CastleMenu.OnCastleRequest)Delegate.Combine(castleMenu5.mOnCastleRequest, new CastleMenu.OnCastleRequest(OnJoinBattle));
			CastleMenu castleMenu6 = mCastleMenu;
			castleMenu6.mOnClanInfo = (CastleMenu.OnClanInfo)Delegate.Combine(castleMenu6.mOnClanInfo, new CastleMenu.OnClanInfo(OnClanInfo));
			CastleMenu castleMenu7 = mCastleMenu;
			castleMenu7.mOnCastleHistoryRequest = (CastleMenu.OnCastleRequest)Delegate.Combine(castleMenu7.mOnCastleHistoryRequest, new CastleMenu.OnCastleRequest(OnCastleHistoryRequest));
			CastleMenu castleMenu8 = mCastleMenu;
			castleMenu8.mOnObserveBattle = (CastleMenu.OnObserveBattle)Delegate.Combine(castleMenu8.mOnObserveBattle, new CastleMenu.OnObserveBattle(OnObserveBattle));
		}
		if (mCastleRequestMenu != null)
		{
			CastleRequestMenu castleRequestMenu3 = mCastleRequestMenu;
			castleRequestMenu3.mCancelRequest = (CastleRequestMenu.CancelRequest)Delegate.Combine(castleRequestMenu3.mCancelRequest, new CastleRequestMenu.CancelRequest(DesertCallback));
			CastleRequestMenu castleRequestMenu4 = mCastleRequestMenu;
			castleRequestMenu4.mAcceptFighters = (CastleRequestMenu.AcceptFighters)Delegate.Combine(castleRequestMenu4.mAcceptFighters, new CastleRequestMenu.AcceptFighters(SetFighters));
			CastleRequestMenu castleRequestMenu5 = mCastleRequestMenu;
			castleRequestMenu5.mOnClanInfo = (CastleRequestMenu.OnClanInfo)Delegate.Combine(castleRequestMenu5.mOnClanInfo, new CastleRequestMenu.OnClanInfo(OnClanInfo));
		}
		if (mCastleHistoryMenu != null)
		{
			CastleHistoryMenu castleHistoryMenu3 = mCastleHistoryMenu;
			castleHistoryMenu3.mOnClanInfo = (CastleHistoryMenu.OnInfoById)Delegate.Combine(castleHistoryMenu3.mOnClanInfo, new CastleHistoryMenu.OnInfoById(OnClanInfo));
			CastleHistoryMenu castleHistoryMenu4 = mCastleHistoryMenu;
			castleHistoryMenu4.mOnBattleHistoryInfo = (CastleHistoryMenu.OnBattleInfoById)Delegate.Combine(castleHistoryMenu4.mOnBattleHistoryInfo, new CastleHistoryMenu.OnBattleInfoById(OnBattleHistoryrequest));
		}
		if (mSelectAvatar != null)
		{
			SelectAvatarWindow selectAvatarWindow4 = mSelectAvatar;
			selectAvatarWindow4.mSelectAvatarCallback = (SelectAvatarWindow.SelectAvatarCallback)Delegate.Combine(selectAvatarWindow4.mSelectAvatarCallback, new SelectAvatarWindow.SelectAvatarCallback(OnSelectAvatarCallback));
			SelectAvatarWindow selectAvatarWindow5 = mSelectAvatar;
			selectAvatarWindow5.mReadyCallback = (SelectAvatarWindow.ReadyCallback)Delegate.Combine(selectAvatarWindow5.mReadyCallback, new SelectAvatarWindow.ReadyCallback(Ready));
			SelectAvatarWindow selectAvatarWindow6 = mSelectAvatar;
			selectAvatarWindow6.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Combine(selectAvatarWindow6.mDesertCallback, new SelectAvatarWindow.DesertCallback(DesertBattle));
		}
	}

	public void Init()
	{
		if (mWorldMapMenu != null)
		{
			mHelper.CastleListRequest();
		}
		if (mHelper.StartTime != null && mHelper.StartTime.mStartTime > DateTime.Now)
		{
			SetTimerMessage(mHelper.StartTime);
		}
	}

	public void SetTimer(int _timeSec)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.StartTimer(_timeSec);
		}
	}

	public void ShowMessage(int _error)
	{
		ShowMessage((Text)_error);
	}

	private void ShowMessage(Text _error)
	{
		string id = "GUI_CASTLE_" + _error;
		ShowMessage(GuiSystem.GetLocaleText(id));
	}

	private void ShowMessage(string text)
	{
		mOkDialog.SetData(text, null);
	}

	public void Notify(string text)
	{
		mOkDialog.SetData(GuiSystem.GetLocaleText(text), null);
	}

	public void ShowDeferredMessage(DeferredMessageArg _arg)
	{
		string newValue = GuiSystem.GetLocaleText(_arg.mStrParameter);
		string id = "GUI_CASTLE_MESSAGE_" + _arg.mType;
		id = GuiSystem.GetLocaleText(id);
		if (_arg.mType == 3)
		{
			if (_arg.mStrParameter == "1")
			{
				newValue = GuiSystem.GetLocaleText("GUI_CASTLE_MESSAGE_3_1");
			}
			else
			{
				newValue = GuiSystem.GetLocaleText("GUI_CASTLE_MESSAGE_3_2");
				newValue = newValue.Replace("{COUNT}", _arg.mStrParameter);
			}
		}
		if (_arg.mType == 2)
		{
			SetTimerMessage(null);
		}
		if (_arg.mType == 1 || _arg.mType == 2 || _arg.mType == 3)
		{
			id = id.Replace("{PARAM1}", newValue);
			ShowMessage(id);
		}
	}

	public void SelfAvatarError()
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.SelectAvatar(-1);
		}
	}

	public void SetTimerMessage(CastleStartBattleInfoArg _arg)
	{
		mJoinMenu.SetCastleJoinRequest(_arg);
	}

	public void Won()
	{
		mScreenMgr.Gui.SetCurGuiSet("central_square");
	}

	public void SetFighters(Dictionary<int, List<Clan.ClanMember>> _arg, string _tag, bool _canLeave)
	{
		if (mCastleRequestMenu != null)
		{
			mCastleRequestMenu.SetFighters(_arg, _tag, _canLeave);
			mCastleRequestMenu.Open();
		}
		if (mCastleMenu != null)
		{
			mCastleMenu.Close();
		}
		mCurrentRequest = null;
	}

	public void SetCastleHistory(List<CastleHistory> _history)
	{
		if (mCastleHistoryMenu != null)
		{
			mCastleHistoryMenu.SetHistoryData(_history);
			mCastleHistoryMenu.Open();
		}
	}

	public void Desert()
	{
		ShowMessage(Text.DESERTED);
		if (mCastleRequestMenu != null)
		{
			mCastleRequestMenu.Close();
		}
	}

	private void CastleCallback(CastleInfo _castleData)
	{
		if (mCurrentCastleInfo == null && _castleData != null && mCastleMenu != null)
		{
			mCurrentCastleInfo = _castleData;
			mCastleMenu.Close();
			mCastleMenu.SetCastleData(_castleData);
			mHelper.GetCastleInfo(_castleData.mId);
		}
	}

	private int GetCounterMessage(int _number)
	{
		if (_number > 9 && _number < 22)
		{
			return 1;
		}
		_number %= 10;
		if (_number > 1 && _number < 5)
		{
			return 2;
		}
		return 1;
	}

	private void OnJoinBattle(CastleInfo _castleData)
	{
		if (mCastleRequestMenu == null)
		{
			return;
		}
		if (mBanCount > 0 && !_castleData.mSelfOwner)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_CASTLE_BAN" + GetCounterMessage(mBanCount));
			localeText = localeText.Replace("{COUNT}", mBanCount.ToString());
			ShowMessage(localeText);
		}
		else if (mIsRightLevel)
		{
			if (mCurrentRequest == null)
			{
				mCastleRequestMenu.Close();
				mCastleRequestMenu.SetCastleData(_castleData);
				mCurrentRequest = _castleData;
				mHelper.RefreshSelfClanInfo();
			}
		}
		else
		{
			ShowMessage(Text.WRONG_LEVEL);
		}
	}

	private void OnClanInfo(int _clanId)
	{
		mHelper.GetClanInfo(_clanId);
	}

	private void OnBattleHistoryrequest(int _castleId, int _battleId)
	{
		mHelper.GetBattleInfo(_castleId, _battleId);
	}

	private void OnCastleHistoryRequest(CastleInfo _castleInfo)
	{
		if (mCastleHistoryMenu != null)
		{
			mCastleHistoryMenu.SetCastleData(_castleInfo);
			mHelper.GetCastleHistory(_castleInfo.mId);
		}
	}

	private void SetFighters(int _castleId, Dictionary<int, int> _fighters)
	{
		mHelper.SetFighters(_castleId, _fighters);
		mLastRequest = _castleId;
	}

	public void SetCastleList(CastleListArg _arg)
	{
		if (mWorldMapMenu == null)
		{
			return;
		}
		foreach (CastleInfo mCastle in _arg.mCastles)
		{
			mWorldMapMenu.SetCastleInfo(mCastle);
		}
	}

	public void FightersErrorOccured(int _error)
	{
		mCurrentRequest = null;
		ShowMessage(_error);
	}

	public void SetCastleMembers(CastleMembersArg _arg)
	{
		mBanCount = _arg.mBanCount;
		mIsRightLevel = _arg.mRightLevel;
		if (mCastleMenu == null)
		{
			mCurrentCastleInfo = null;
			return;
		}
		mCastleMenu.SetMembersData(_arg);
		if (!_arg.mInProgress)
		{
			mCurrentCastleInfo = null;
			mCastleMenu.Open();
		}
		else
		{
			mHelper.GetCurrentBattleInfo(mCurrentCastleInfo.mId);
		}
	}

	public void SetCastleBattleInfo(CastleBattleInfoArg _arg)
	{
		if (mCurrentCastleInfo != null)
		{
			mCurrentCastleInfo = null;
			if (mCastleMenu != null)
			{
				mCastleMenu.SetBattleData(_arg);
				mCastleMenu.Open();
			}
		}
		else
		{
			mCastleHistoryMenu.SetBattleHistoryData(_arg.mBattles);
		}
	}

	public void ClanInfoUpdated()
	{
		if (mCurrentRequest != null)
		{
			mHelper.GetFighters(mCurrentRequest.mId);
		}
	}

	private void DesertCallback(int _castleId)
	{
		mHelper.DesertCastleRequest(_castleId);
	}

	private void OnSelectAvatarCallback(int _id)
	{
		if (mSelectAvatar.IsClanWar())
		{
			mHelper.SelectAvatar(_id);
		}
	}

	private void Ready()
	{
		if (mSelectAvatar.IsClanWar())
		{
			mHelper.SetReady();
		}
	}

	private void DesertBattle()
	{
		if (mSelectAvatar.IsClanWar())
		{
			mHelper.DesertBattle();
		}
	}

	public void StartCastleBattle(CastleStartRequestArg _arg)
	{
		if (mSelectAvatar == null)
		{
			BattleScreen battleScreen = mScreenMgr.Holder.GetScreen(ScreenType.BATTLE) as BattleScreen;
			battleScreen.Exit();
			return;
		}
		mSelectAvatar.Uninit();
		mScreenMgr.Gui.SetCurGuiSet("select_avatar");
		int num = ((_arg.mTeam1.Count <= _arg.mTeam2.Count) ? _arg.mTeam1.Count : _arg.mTeam2.Count);
		int num2 = ((_arg.mTeam1.Count >= _arg.mTeam2.Count) ? _arg.mTeam1.Count : _arg.mTeam2.Count);
		mSelectAvatar.SetData(_arg.mAvatars, _arg.mAddStats, num * 2, num2 * 2, 7);
		foreach (Fighter item in _arg.mTeam1)
		{
			mSelectAvatar.AddPlayer(item.mId, item.mNick, item.mLevel, item.mTeam);
		}
		foreach (Fighter item2 in _arg.mTeam2)
		{
			mSelectAvatar.AddPlayer(item2.mId, item2.mNick, item2.mLevel, item2.mTeam);
		}
		if (mOnStarted != null)
		{
			mOnStarted();
		}
	}

	public void SetAvatarTimer(int _time, List<int> _users)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.StartTimer(_time);
			mSelectAvatar.SetAvailable(_users);
		}
	}

	public void SetAvatar(int _userId, int _avatarId)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.SelectAvatar(_userId, _avatarId);
			if (_userId == mHelper.SelfUserId)
			{
				mSelectAvatar.SetReadyStatus(_status: true);
			}
		}
	}

	public void SetReadyStatus(int _id)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.PlayerReady(_id, _ready: true);
			if (_id == mHelper.SelfUserId)
			{
				mSelectAvatar.Lock();
			}
		}
	}

	public void PlayerDeserted(int _id)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.RemovePlayer(_id);
			if (_id == mHelper.SelfUserId)
			{
				mScreenMgr.Gui.SetCurGuiSet("central_square");
			}
		}
	}

	private void OnObserveBattle(ServerData _serverData)
	{
		BattleServerData battleServerData = new BattleServerData();
		battleServerData.mHost = _serverData.mHost;
		battleServerData.mPorts = _serverData.mPorts;
		BattleServerData.DebugJoinData debugJoinData = (battleServerData.mDebugJoin = new BattleServerData.DebugJoinData());
		debugJoinData.mAvatarPrefab = string.Empty;
		debugJoinData.mBattleId = _serverData.mBattleId;
		debugJoinData.mMapId = 0;
		debugJoinData.mTeam = 0;
		debugJoinData.mAvatarParams = new MixedArray();
		Log.Debug("Host:" + _serverData.mHost);
		BattleMapData battleMapData = new BattleMapData();
		battleMapData.mMapName = _serverData.mMap;
		mObserverCallback(battleServerData, battleMapData);
	}
}
