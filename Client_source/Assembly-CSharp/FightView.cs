using System;
using System.Collections.Generic;
using TanatKernel;

public class FightView : IFightJoinGui
{
	private enum Text
	{
		COMMON_ERROR = 0,
		ANOTHER_QUEUE_STARTED = 8011,
		FIGHT_BANNED = 8023,
		ALREADY_OUT = 8022,
		ALREADY_IN = 8024,
		NO_PLACE = 8025
	}

	private FightHelper mJoinHelper;

	private MapJoinQueueMenu mMapJoinQueueWnd;

	private SelectGameMenu mSelGameMenu;

	private SelectAvatarWindow mSelectAvatar;

	private YesNoDialog mYesNoDialogWnd;

	private OkDialog mOkDialogWnd;

	private TutorialWindow mTutorialWnd;

	private TutorialWindow mOldWindow;

	private YesNoDialog mYesNoTutorialWnd;

	private OkDialog mOkTutorialWnd;

	private TutorialMgr mTutorial;

	private WorldMapMenu mWorldMap;

	private int mSelfId;

	private ScreenManager mScreenMgr;

	public BaseBattleScreen mCurScreen;

	public FightView(BaseBattleScreen _curScreen, ScreenManager _screenMgr, MapJoinQueueMenu _mapJoinQueueWnd, SelectGameMenu _selGameMenu, SelectAvatarWindow _selAvaWnd, TutorialWindow _oldWindow, TutorialMgr _tutorial, YesNoDialog _yesNoWnd, OkDialog _okWnd, WorldMapMenu _worldMap)
	{
		if (_curScreen == null)
		{
			throw new ArgumentNullException("_curScreen");
		}
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		if (_mapJoinQueueWnd == null)
		{
			throw new ArgumentNullException("_mapJoinQueueWnd");
		}
		if (_oldWindow == null)
		{
			throw new ArgumentNullException("_oldWindow");
		}
		if (_tutorial == null)
		{
			throw new ArgumentNullException("_tutorial");
		}
		if (_yesNoWnd == null)
		{
			throw new ArgumentNullException("_yesNoWnd");
		}
		if (_okWnd == null)
		{
			throw new ArgumentNullException("_okWnd");
		}
		mCurScreen = _curScreen;
		mScreenMgr = _screenMgr;
		mMapJoinQueueWnd = _mapJoinQueueWnd;
		mSelGameMenu = _selGameMenu;
		mSelectAvatar = _selAvaWnd;
		mTutorial = _tutorial;
		mOldWindow = _oldWindow;
		mOkDialogWnd = _okWnd;
		mYesNoDialogWnd = _yesNoWnd;
		mWorldMap = _worldMap;
		GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("select_avatar");
		mYesNoTutorialWnd = guiSet.GetElementById<YesNoDialog>("YES_NO_DAILOG");
		mOkTutorialWnd = guiSet.GetElementById<OkDialog>("OK_DAILOG");
		mTutorialWnd = guiSet.GetElementById<TutorialWindow>("TUTORIAL_WINDOW");
	}

	public void Init(FightHelper _joinHelper, int _selfId)
	{
		if (_joinHelper == null)
		{
			throw new ArgumentNullException("_joinHelper");
		}
		mSelfId = _selfId;
		mJoinHelper = _joinHelper;
		MapJoinQueueMenu mapJoinQueueMenu = mMapJoinQueueWnd;
		mapJoinQueueMenu.mDesertCallback = (MapJoinQueueMenu.DesertCallback)Delegate.Combine(mapJoinQueueMenu.mDesertCallback, new MapJoinQueueMenu.DesertCallback(OnDesertQueue));
		MapJoinQueueMenu mapJoinQueueMenu2 = mMapJoinQueueWnd;
		mapJoinQueueMenu2.mAcceptJoinCallback = (MapJoinQueueMenu.VoidCallback)Delegate.Combine(mapJoinQueueMenu2.mAcceptJoinCallback, new MapJoinQueueMenu.VoidCallback(OnAcceptJoinFight));
		if (mSelGameMenu != null)
		{
			SelectGameMenu selectGameMenu = mSelGameMenu;
			selectGameMenu.mJoinCallback = (SelectGameMenu.JoinCallback)Delegate.Combine(selectGameMenu.mJoinCallback, new SelectGameMenu.JoinCallback(OnMapSelected));
		}
		if (mWorldMap != null)
		{
			WorldMapMenu worldMapMenu = mWorldMap;
			worldMapMenu.mJoinCallback = (WorldMapMenu.BattleMapCallback)Delegate.Combine(worldMapMenu.mJoinCallback, new WorldMapMenu.BattleMapCallback(OnGloablMapSelected));
			WorldMapMenu worldMapMenu2 = mWorldMap;
			worldMapMenu2.mGetMapInfo = (WorldMapMenu.BattleMapCallback)Delegate.Combine(worldMapMenu2.mGetMapInfo, new WorldMapMenu.BattleMapCallback(OnGetMapInfo));
		}
		if (mSelectAvatar != null)
		{
			SelectAvatarWindow selectAvatarWindow = mSelectAvatar;
			selectAvatarWindow.mSelectAvatarCallback = (SelectAvatarWindow.SelectAvatarCallback)Delegate.Combine(selectAvatarWindow.mSelectAvatarCallback, new SelectAvatarWindow.SelectAvatarCallback(OnAvatarSelected));
			SelectAvatarWindow selectAvatarWindow2 = mSelectAvatar;
			selectAvatarWindow2.mReadyCallback = (SelectAvatarWindow.ReadyCallback)Delegate.Combine(selectAvatarWindow2.mReadyCallback, new SelectAvatarWindow.ReadyCallback(OnReady));
			SelectAvatarWindow selectAvatarWindow3 = mSelectAvatar;
			selectAvatarWindow3.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Combine(selectAvatarWindow3.mDesertCallback, new SelectAvatarWindow.DesertCallback(OnDesert));
		}
		mTutorialWnd.SetData(mOkTutorialWnd, mYesNoTutorialWnd, null, null, null, null);
	}

	public void Uninit()
	{
		MapJoinQueueMenu mapJoinQueueMenu = mMapJoinQueueWnd;
		mapJoinQueueMenu.mDesertCallback = (MapJoinQueueMenu.DesertCallback)Delegate.Remove(mapJoinQueueMenu.mDesertCallback, new MapJoinQueueMenu.DesertCallback(OnDesertQueue));
		MapJoinQueueMenu mapJoinQueueMenu2 = mMapJoinQueueWnd;
		mapJoinQueueMenu2.mAcceptJoinCallback = (MapJoinQueueMenu.VoidCallback)Delegate.Remove(mapJoinQueueMenu2.mAcceptJoinCallback, new MapJoinQueueMenu.VoidCallback(OnAcceptJoinFight));
		mMapJoinQueueWnd.ClearRequests();
		if (mSelGameMenu != null)
		{
			SelectGameMenu selectGameMenu = mSelGameMenu;
			selectGameMenu.mJoinCallback = (SelectGameMenu.JoinCallback)Delegate.Remove(selectGameMenu.mJoinCallback, new SelectGameMenu.JoinCallback(OnMapSelected));
		}
		if (mSelectAvatar != null)
		{
			SelectAvatarWindow selectAvatarWindow = mSelectAvatar;
			selectAvatarWindow.mSelectAvatarCallback = (SelectAvatarWindow.SelectAvatarCallback)Delegate.Remove(selectAvatarWindow.mSelectAvatarCallback, new SelectAvatarWindow.SelectAvatarCallback(OnAvatarSelected));
			SelectAvatarWindow selectAvatarWindow2 = mSelectAvatar;
			selectAvatarWindow2.mReadyCallback = (SelectAvatarWindow.ReadyCallback)Delegate.Remove(selectAvatarWindow2.mReadyCallback, new SelectAvatarWindow.ReadyCallback(OnReady));
			SelectAvatarWindow selectAvatarWindow3 = mSelectAvatar;
			selectAvatarWindow3.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Remove(selectAvatarWindow3.mDesertCallback, new SelectAvatarWindow.DesertCallback(OnDesert));
		}
		if (mWorldMap != null)
		{
			WorldMapMenu worldMapMenu = mWorldMap;
			worldMapMenu.mJoinCallback = (WorldMapMenu.BattleMapCallback)Delegate.Remove(worldMapMenu.mJoinCallback, new WorldMapMenu.BattleMapCallback(OnGloablMapSelected));
			WorldMapMenu worldMapMenu2 = mWorldMap;
			worldMapMenu2.mGetMapInfo = (WorldMapMenu.BattleMapCallback)Delegate.Remove(worldMapMenu2.mGetMapInfo, new WorldMapMenu.BattleMapCallback(OnGetMapInfo));
		}
		mTutorial.UnInit();
	}

	private void OnGetMapInfo(MapType _type, int _mapId)
	{
		if (mWorldMap != null && mWorldMap.Active)
		{
			List<MapData> list = new List<MapData>();
			list.Add(mJoinHelper.FindMapById(_mapId));
			mWorldMap.SetMapData(_type, list);
		}
	}

	private void OnGloablMapSelected(MapType _type, int _mapId)
	{
		OnMapSelected(_mapId);
	}

	private void OnAcceptJoinFight()
	{
		mJoinHelper.JoinRequest();
	}

	private void OnDesertQueue(int _mapId)
	{
		MapData mapData = mJoinHelper.FindMapById(_mapId);
		string id = ((mapData != null) ? mapData.mName : "wrong id");
		UserLog.AddAction(UserActionType.BUTTON_REQUEST_DESERT, _mapId, GuiSystem.GetLocaleText(id));
		mJoinHelper.Desert(_mapId);
	}

	private void OnMapSelected(int _mapId)
	{
		MapData mapData = mJoinHelper.FindMapById(_mapId);
		string id = ((mapData != null) ? mapData.mName : "wrong id");
		if (mJoinHelper.IsMapJoined(_mapId))
		{
			UserLog.AddAction(UserActionType.BUTTON_REQUEST_DESERT, _mapId, GuiSystem.GetLocaleText(id));
			mJoinHelper.Desert(_mapId);
		}
		else
		{
			UserLog.AddAction(UserActionType.BUTTON_MAP, _mapId, GuiSystem.GetLocaleText(id));
			mJoinHelper.JoinBattle(_mapId);
		}
	}

	private void OnAvatarSelected(int _avatarId)
	{
		if (!mSelectAvatar.IsClanWar())
		{
			mJoinHelper.SelectAvatar(_avatarId);
		}
	}

	private void OnReady()
	{
		if (!mSelectAvatar.IsClanWar())
		{
			mJoinHelper.SetReady(mSelfId);
		}
	}

	private void OnDesert()
	{
		if (!mSelectAvatar.IsClanWar())
		{
			mTutorial.SetWindow(mOldWindow);
			mTutorial.SetScreenId("central_square");
			mJoinHelper.ExitFromLobby();
			mScreenMgr.Gui.SetCurGuiSet("central_square");
		}
	}

	public void SetBattleInfo(FightStartSelectAvatarMpdArg _arg, int _timer)
	{
		if (mSelectAvatar == null)
		{
			BattleScreen battleScreen = mScreenMgr.Holder.GetScreen(ScreenType.BATTLE) as BattleScreen;
			battleScreen.Exit();
			return;
		}
		mSelectAvatar.Uninit();
		mScreenMgr.Gui.SetCurGuiSet("select_avatar");
		MapData mapData = mJoinHelper.FindMapById(_arg.mMapId);
		mSelectAvatar.SetData(_arg.mAvatars, _arg.mAddStats, mapData.mMinPlayers, mapData.mMaxPlayers, mapData.mType);
		mSelectAvatar.ExitEnabled = mapData.mType == 4;
		if (mapData.mType != 4)
		{
			mMapJoinQueueWnd.ClearRequests();
		}
		foreach (Fighter item in _arg.mTeam1)
		{
			string text = item.mNick;
			if (!string.IsNullOrEmpty(item.mTag))
			{
				text = "[" + item.mTag + "]" + text;
			}
			mSelectAvatar.AddPlayer(item.mId, text, item.mLevel, item.mTeam);
			mSelectAvatar.PlayerInRequestState(item.mId, _inRequest: false);
		}
		foreach (Fighter item2 in _arg.mTeam2)
		{
			string text2 = item2.mNick;
			if (!string.IsNullOrEmpty(item2.mTag))
			{
				text2 = "[" + item2.mTag + "]" + text2;
			}
			mSelectAvatar.AddPlayer(item2.mId, text2, item2.mLevel, item2.mTeam);
			mSelectAvatar.PlayerInRequestState(item2.mId, _inRequest: false);
		}
		foreach (int item3 in mJoinHelper.PlayersInLobby())
		{
			mSelectAvatar.PlayerInRequestState(item3, _inRequest: true);
		}
		foreach (KeyValuePair<int, int> item4 in mJoinHelper.SelectedAvatars())
		{
			mSelectAvatar.SelectAvatar(item4.Key, item4.Value);
		}
		foreach (int item5 in mJoinHelper.PlayersReady())
		{
			mSelectAvatar.PlayerReady(item5, _ready: true);
		}
		mSelectAvatar.StartTimer(_timer);
		mTutorial.SetWindow(mTutorialWnd);
		mTutorial.SetScreenId("select_avatar");
	}

	public void BanMessage(int _time)
	{
		TimeSpan timeSpan = DateTime.Now.AddSeconds(_time) - DateTime.Now;
		string localeText = GuiSystem.GetLocaleText("GUI_FIGHT_BANNED_TIMER");
		int num = (int)timeSpan.TotalHours;
		localeText = localeText.Replace("{HOUR}", num.ToString());
		int num2 = timeSpan.Minutes;
		if (num == 0 && num2 == 0)
		{
			num2++;
		}
		localeText = localeText.Replace("{MIN}", num2.ToString());
		mOkDialogWnd.SetData(localeText);
	}

	public void PlayersFound(int _mapId, int _timer, bool _needClear)
	{
		if (_needClear)
		{
			mScreenMgr.Gui.SetCurGuiSet("central_square");
		}
		if (mSelGameMenu != null)
		{
			mSelGameMenu.SetActive(_active: false);
		}
		MapData mapData = mJoinHelper.FindMapById(_mapId);
		JoinedQueue joinedQueue = new JoinedQueue();
		joinedQueue.MapData = mapData;
		joinedQueue.StartTime = DateTime.Now.AddSeconds(_timer);
		mMapJoinQueueWnd.AskJoin(joinedQueue, _timer);
	}

	public void SetReady(int _userId)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.PlayerReady(_userId, _ready: true);
			if (_userId == mSelfId)
			{
				mSelectAvatar.Lock();
				mSelectAvatar.LockBackButton(_status: true);
			}
		}
	}

	public void SetAvatar(int _userId, int _avatarId)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.PlayerInRequestState(_userId, _inRequest: true);
			mSelectAvatar.SelectAvatar(_userId, _avatarId);
			if (_userId == mSelfId)
			{
				mSelectAvatar.SelectAvatar(_avatarId);
			}
		}
	}

	public void SelfAvatarError()
	{
		mSelectAvatar.SelectAvatar(-1);
	}

	public void PlayerDeserted()
	{
		if (mSelectAvatar != null && mScreenMgr.Gui.GetCurGuiSetId() == "select_avatar")
		{
			mSelectAvatar.ClearLobby();
		}
		mMapJoinQueueWnd.ClearRequests();
	}

	public void ShowMessage(string _messageId)
	{
		mOkDialogWnd.SetData(GuiSystem.GetLocaleText(_messageId), null);
	}

	public void ShowError(int _error)
	{
		string messageId = "GUI_" + (Text)_error;
		ShowMessage(messageId);
	}

	public void FriendInvite(string _nick, int _mapId, bool _isLeave)
	{
		if (mScreenMgr.Gui.GetCurGuiSetId() == "select_avatar" && _isLeave)
		{
			mScreenMgr.Gui.SetCurGuiSet("central_square");
			mJoinHelper.ExitFromLobby();
		}
		else if (mScreenMgr.Gui.GetCurGuiSetId() == "select_avatar")
		{
			mJoinHelper.InviteAnswer(_mapId, _answer: false);
			return;
		}
		string localeText = GuiSystem.GetLocaleText("GUI_BATTLE_INVITE");
		if (_isLeave)
		{
			localeText = GuiSystem.GetLocaleText("GUI_BATTLE_REINVITE");
		}
		MapData mapData = mJoinHelper.FindMapById(_mapId);
		localeText = localeText.Replace("{NAME}", _nick);
		if (mapData != null)
		{
			localeText = localeText.Replace("{MAP}", GuiSystem.GetLocaleText(((MapType)mapData.mType).ToString() + "_Text"));
			localeText = localeText.Replace("{BATTLE}", GuiSystem.GetLocaleText(mapData.mName));
		}
		YesNoDialog.OnAnswer callback = delegate(bool _answer)
		{
			mJoinHelper.InviteAnswer(_mapId, _answer);
			if (!_answer)
			{
				mMapJoinQueueWnd.RemoveJoinRequest(_mapId);
			}
		};
		mYesNoDialogWnd.SetData(localeText, "YES_TEXT", "NO_TEXT", callback);
	}

	public void Joined(IJoinedQueue _jq, bool isNew)
	{
		if (isNew)
		{
			ShowMessage("GUI_YOU_IN_REQUEST");
		}
		mMapJoinQueueWnd.AddMapJoinRequest(_jq);
	}

	public void PlayerJoined(int _userId)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.PlayerInRequestState(_userId, _inRequest: true);
		}
	}

	public void ShowMapsDesc(List<MapData> _info)
	{
		if (mWorldMap != null)
		{
			mWorldMap.SetMapDescription(_info);
		}
		if (mCurScreen is CentralSquareScreen)
		{
			mSelGameMenu.InitMaps(_info);
		}
	}

	public void Binded()
	{
		mJoinHelper.MapsInfo();
	}

	public void ExitFromQueue(int _id)
	{
		mMapJoinQueueWnd.RemoveJoinRequest(_id);
	}

	public void SetTimer(int _time)
	{
		if (mSelectAvatar != null)
		{
			mSelectAvatar.StartTimer(_time);
		}
	}
}
