using System;
using System.Collections.Generic;
using System.Timers;
using Log4Tanat;
using Network;
using TanatKernel;
using UnityEngine;

public class BattleScreen : BaseBattleScreen, IItemUsageMgr, IGameObjectInitializer
{
	private static int mWaitStatTime = 60;

	private ScreenManager mScreenMgr;

	private MainInfoWindow mMainInfoWnd;

	private ChatWindow mChatWnd;

	private MiniMap mMiniMapWnd;

	private ObjectInfo mObjectInfoPlayerWnd;

	private ObjectInfo mObjectInfoEnemyWnd;

	private EnemyInfoWindow mEnemyInfoWnd;

	private InventoryMenu mInventoryWnd;

	private FormatedTipMgr mFormatedTipMgr;

	private ShopMenu mBattleShopWnd;

	private BattleStats mBattleStats;

	private PopupInfo mPopupInfoWnd;

	private ShortGameInfo mGameInfoWnd;

	private DropMenu mDropWnd;

	private GameEndMenu mGameEndWnd;

	private OkDialog mOkDialogWnd;

	private EscapeDialog mEscDialogWnd;

	private AfkWarnDialog mAfkWarnDialog;

	private TimerWarningDialog mTimerWarning;

	private ItemCountMenu mItemCountWnd;

	private YesNoDialog mYesNoDialogWnd;

	private MapJoinQueueMenu mMapJoinQueueWnd;

	private QuestJournal mQuestJournalWnd;

	private QuestProgressMenu mQuestProgressWnd;

	private QuestStatusMenu mQuestStatusWnd;

	private ShopGUI mShopWnd;

	private TutorialWindow mTutorialWnd;

	private LeaveInfo mLeaveWnd;

	private int mBattleType;

	private VisualBattle mVisualBattle;

	private PlayerControl mPlayerCtrl;

	private HeightMap mHeightMap;

	private KillEventManager mKillEventMgr;

	private TutorialMgr mTutorial;

	private CastleJoinView mCastleView;

	private FightView mFightView;

	private bool mFirstTimeQuests = true;

	private Timer mExitTimer;

	public BattleScreen(Core _core, ScreenManager _screenMgr, TanatApp _app)
		: base(_core)
	{
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		mScreenMgr = _screenMgr;
		mTutorial = _app.TutorialMgr();
		GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("battle");
		mMainInfoWnd = guiSet.GetElementById<MainInfoWindow>("MAIN_INFO_WINDOW");
		mChatWnd = guiSet.GetElementById<ChatWindow>("CHAT_WINDOW");
		mMiniMapWnd = guiSet.GetElementById<MiniMap>("MINI_MAP_GAME_MENU");
		mObjectInfoPlayerWnd = guiSet.GetElementById<ObjectInfo>("PLAYER_AVATAR_INFO");
		mObjectInfoEnemyWnd = guiSet.GetElementById<ObjectInfo>("ENEMY_INFO");
		mEnemyInfoWnd = guiSet.GetElementById<EnemyInfoWindow>("ENEMY_INFO_WINDOW");
		mInventoryWnd = guiSet.GetElementById<InventoryMenu>("INVENTORY_MENU");
		mBattleShopWnd = guiSet.GetElementById<ShopMenu>("SHOP_MENU");
		mBattleStats = guiSet.GetElementById<BattleStats>("STATS_PLATE_GAME_MENU");
		mPopupInfoWnd = guiSet.GetElementById<PopupInfo>("POPUP_INFO");
		mGameInfoWnd = guiSet.GetElementById<ShortGameInfo>("SHORT_GAME_INFO");
		mDropWnd = guiSet.GetElementById<DropMenu>("DROP_MENU");
		mGameEndWnd = guiSet.GetElementById<GameEndMenu>("GAME_END_MENU");
		mOkDialogWnd = guiSet.GetElementById<OkDialog>("OK_DAILOG");
		mTimerWarning = guiSet.GetElementById<TimerWarningDialog>("TIMER_DAILOG");
		mAfkWarnDialog = guiSet.GetElementById<AfkWarnDialog>("AFK_WARN_DAILOG");
		mItemCountWnd = guiSet.GetElementById<ItemCountMenu>("ITEM_COUNT_MENU");
		mYesNoDialogWnd = guiSet.GetElementById<YesNoDialog>("YES_NO_DAILOG");
		mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		mMapJoinQueueWnd = guiSet.GetElementById<MapJoinQueueMenu>("MAP_JOIN_QUEUE_MENU");
		mQuestJournalWnd = guiSet.GetElementById<QuestJournal>("QUEST_JOURNAL_MENU");
		mQuestProgressWnd = guiSet.GetElementById<QuestProgressMenu>("QUEST_PROGRESS_MENU");
		mQuestStatusWnd = guiSet.GetElementById<QuestStatusMenu>("QUEST_STATUS_MENU");
		mShopWnd = guiSet.GetElementById<ShopGUI>("SHOP_WINDOW");
		mTutorialWnd = guiSet.GetElementById<TutorialWindow>("TUTORIAL_WINDOW");
		mLeaveWnd = guiSet.GetElementById<LeaveInfo>("LEAVE_INFO");
		mEscDialogWnd = new EscapeDialog(guiSet.GetElementById<EscapeMenu>("ESCAPE_MENU"), mYesNoDialogWnd, guiSet.GetElementById<OptionsMenu>("OPTIONS_MENU"), guiSet.GetElementById<LeaveInfo>("LEAVE_INFO"));
		mFightView = new FightView(this, mScreenMgr, mMapJoinQueueWnd, null, null, mTutorialWnd, mTutorial, mYesNoDialogWnd, mOkDialogWnd, null);
	}

	public override void Show()
	{
		base.Show();
		mPlayerCtrl = new PlayerControl();
		if (mCore.Battle != null)
		{
			mCore.Battle.AddGameObjInitializer(this);
			mCore.CtrlServer.KeepSessionAlive = true;
			mCore.Battle.mLoaded = true;
			if (mCore.BattleServer != null)
			{
				mCore.BattleServer.NeedMicroReconnect(_needReconnect: true);
				mCore.BattleServer.mReconnectStarted += OnMicroReconnectStarted;
				mCore.BattleServer.mReconnectStoped += OnMicroReconnectStoped;
			}
			if (mCore.CtrlServer != null && mCore.CtrlServer.MpdConnection != null)
			{
				mCore.CtrlServer.MpdConnection.NeedMicroReconnect(_needReconnect: true);
			}
			mHeightMap = new HeightMap(2f);
			mKillEventMgr = new KillEventManager(mCore.CtrlServer.Chat, mCore.Battle.SelfPlayer);
			mVisualBattle = new VisualBattle(mCore.Battle, mKillEventMgr, mPlayerCtrl);
			mVisualBattle.Subscribe(mCore.BattleServer.PacketMgr, mCore.Battle.PlayerStore);
			if (mCore.UserNetData.Inited)
			{
				mCore.CtrlServer.SelfQuests.UpdateContent();
			}
		}
	}

	protected override void ShowGui()
	{
		Battle battle = mCore.Battle;
		mBattleType = (mCore.MapManager as SceneManager).GetLastLoadedMapType();
		UserLog.AddAction(UserActionType.BATTLE_LOADED, mBattleType, GuiSystem.GetLocaleText(((MapType)mBattleType).ToString() + "_Text"));
		mPlayerCtrl.Init(battle.SelfPlayer, battle.GetGameObjProvider(), battle.GetPrototypeProvider(), VisualEffectsMgr.Instance);
		PlayerControl playerControl = mPlayerCtrl;
		playerControl.mSelectionChangedCallback = (PlayerControl.SelectionChangedCallback)Delegate.Combine(playerControl.mSelectionChangedCallback, new PlayerControl.SelectionChangedCallback(OnSelectionChanged));
		bool avatarLight = false;
		CommonInput commonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (null != commonInput)
		{
			commonInput.Init(mScreenMgr);
			avatarLight = commonInput.mAvatarLight;
			mHeightMap.mEnableCache = commonInput.mCacheTerrainHeight;
		}
		BattleInput battleInput = GameObjUtil.FindObjectOfType<BattleInput>();
		if (null != battleInput)
		{
			battleInput.Init(mScreenMgr, mPlayerCtrl, mMiniMapWnd);
			battleInput.InitAvatarAi(mCore.Config.AiUpdateRate, mCore.Config.AiViewRadius, mCore.BattleServer.PacketMgr.HandlerMgr);
			battleInput.InitAfk(mCore.Config.AfkKickTime, mCore.Config.AfkWarnTime);
			MapType mapType = (MapType)mBattleType;
			if (mapType == MapType.HUNT)
			{
				battleInput.mAfkCallback = (BattleInput.AfkCallback)Delegate.Combine(battleInput.mAfkCallback, new BattleInput.AfkCallback(OnAfkStateChanged));
			}
		}
		WarFog warFog = GameObjUtil.TryFindObjectOfType<WarFog>();
		if (warFog != null)
		{
			warFog.Init("battle");
		}
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Init(mCore.BattleServer.PacketMgr);
		}
		mVisualBattle.AvatarLight = avatarLight;
		mVisualBattle.TryAddLight();
		mEscDialogWnd.SetBattleType((MapType)mBattleType);
		GuiSystem.mGuiInputMgr.SetGui(mItemCountWnd, mYesNoDialogWnd);
		GuiInputMgr mGuiInputMgr = GuiSystem.mGuiInputMgr;
		mGuiInputMgr.mDropItemCallback = (GuiInputMgr.DropItemCallback)Delegate.Combine(mGuiInputMgr.mDropItemCallback, new GuiInputMgr.DropItemCallback(OnDropItem));
		Chat chat = mCore.CtrlServer.Chat;
		chat.mOnErrorCallback = (Chat.OnErrorCallback)Delegate.Combine(chat.mOnErrorCallback, new Chat.OnErrorCallback(OnChatError));
		mCore.CtrlServer.SelfQuests.SubscribeOnUpdated(OnSelfQuestsUpdated);
		battle.SelfPlayer.SelfPvpQuests.SubscribeOnUpdated(OnSelfPveQuestsUpdated);
		BattleInventory battleInventory = battle.SelfPlayer.BattleInventory;
		battleInventory.mAddCallback = (Store<BattleThing>.AddCallback)Delegate.Combine(battleInventory.mAddCallback, new Store<BattleThing>.AddCallback(OnBattleInventoryAdd));
		BattleInventory battleInventory2 = battle.SelfPlayer.BattleInventory;
		battleInventory2.mRemoveCallback = (Store<BattleThing>.RemoveCallback)Delegate.Combine(battleInventory2.mRemoveCallback, new Store<BattleThing>.RemoveCallback(OnBattleInventoryRemove));
		SelfPlayer selfPlayer = mPlayerCtrl.SelfPlayer;
		selfPlayer.mInventoryChangedCallback = (SelfPlayer.InventoryChangedCallback)Delegate.Combine(selfPlayer.mInventoryChangedCallback, new SelfPlayer.InventoryChangedCallback(OnBattleInventoryChanged));
		MainInfoWindow mainInfoWindow = mMainInfoWnd;
		mainInfoWindow.mOnInitInventory = (MainInfoWindow.OnInitInventory)Delegate.Combine(mainInfoWindow.mOnInitInventory, new MainInfoWindow.OnInitInventory(ToggleInventory));
		MainInfoWindow mainInfoWindow2 = mMainInfoWnd;
		mainInfoWindow2.mOnForceRespawn = (MainInfoWindow.VoidCallback)Delegate.Combine(mainInfoWindow2.mOnForceRespawn, new MainInfoWindow.VoidCallback(OnForceRespawnRequest));
		MainInfoWindow mainInfoWindow3 = mMainInfoWnd;
		mainInfoWindow3.mOnPortal = (MainInfoWindow.VoidCallback)Delegate.Combine(mainInfoWindow3.mOnPortal, new MainInfoWindow.VoidCallback(ExitFromBattle));
		MainInfoWindow mainInfoWindow4 = mMainInfoWnd;
		mainInfoWindow4.mOnQuestJournal = (MainInfoWindow.ShowQuestJournalCallback)Delegate.Combine(mainInfoWindow4.mOnQuestJournal, new MainInfoWindow.ShowQuestJournalCallback(ShowQuestJournal));
		MainInfoWindow mainInfoWindow5 = mMainInfoWnd;
		mainInfoWindow5.mOnShop = (MainInfoWindow.VoidCallback)Delegate.Combine(mainInfoWindow5.mOnShop, new MainInfoWindow.VoidCallback(ShopContentRequest));
		MainInfoWindow mainInfoWindow6 = mMainInfoWnd;
		mainInfoWindow6.mOnBuyItem = (MainInfoWindow.BattleItemCallback)Delegate.Combine(mainInfoWindow6.mOnBuyItem, new MainInfoWindow.BattleItemCallback(OnBuyBattleItem));
		MainInfoWindow mainInfoWindow7 = mMainInfoWnd;
		mainInfoWindow7.mOnRemoveItem = (MainInfoWindow.BattleItemCallback)Delegate.Combine(mainInfoWindow7.mOnRemoveItem, new MainInfoWindow.BattleItemCallback(OnRemoveBattleItem));
		mChatWnd.SetData(mCore.CtrlServer.Chat);
		mChatWnd.SetActive(_active: true);
		mMiniMapWnd.SetData(battle.GetGameObjProvider());
		(mCore.MapManager as SceneManager).InitMiniMap(mMiniMapWnd);
		mObjectInfoPlayerWnd.SetActive(_active: true);
		mObjectInfoPlayerWnd.SetData(battle.GetGameObjProvider());
		mObjectInfoPlayerWnd.SetObj(mPlayerCtrl.SelfPlayer.Player.AvatarObjId);
		mObjectInfoEnemyWnd.SetData(battle.GetGameObjProvider());
		mEnemyInfoWnd.SetData(battle.GetGameObjProvider(), mCore.CtrlServer.GetPrototypeProvider(), battle.GetPrototypeProvider(), mPopupInfoWnd, mCore.CtrlServer.Heroes, mFormatedTipMgr);
		mInventoryWnd.SetData(mPlayerCtrl.SelfPlayer, mFormatedTipMgr, mPlayerCtrl);
		mTutorialWnd.SetData(mOkDialogWnd, mYesNoDialogWnd, null, null, null, null);
		mMapJoinQueueWnd.SetData(mYesNoDialogWnd);
		BattleStats battleStats = mBattleStats;
		battleStats.mMenuCallback = (BattleStats.MenuCallback)Delegate.Combine(battleStats.mMenuCallback, new BattleStats.MenuCallback(ShowEscDialog));
		StoreContentProvider<BattleThing> inventory = battle.SelfPlayer.Inventory;
		StoreContentProvider<CtrlThing> ctrlInventory = mCore.CtrlServer.SelfHero.Inventory as StoreContentProvider<CtrlThing>;
		PopupEvent.Init(mPopupInfoWnd, battle.GetPrototypeProvider(), mCore.CtrlServer.GetPrototypeProvider(), inventory, ctrlInventory, mPlayerCtrl);
		mGameInfoWnd.SetData(mPlayerCtrl.SelfPlayer.Player, battle.GetTimer(), mKillEventMgr);
		(mCore.MapManager as SceneManager).InitClock(mGameInfoWnd, battle.GetTimer());
		mDropWnd.SetData(battle.GetPrototypeProvider(), mCore.CtrlServer.GetPrototypeProvider(), mPlayerCtrl, mFormatedTipMgr);
		GameEndMenu gameEndMenu = mGameEndWnd;
		gameEndMenu.mExitCallback = (GameEndMenu.ExitCallback)Delegate.Combine(gameEndMenu.mExitCallback, new GameEndMenu.ExitCallback(ExitFromBattle));
		GameEndMenu gameEndMenu2 = mGameEndWnd;
		gameEndMenu2.mStartExitTimerCallback = (GameEndMenu.StartExitTimer)Delegate.Combine(gameEndMenu2.mStartExitTimerCallback, new GameEndMenu.StartExitTimer(OnStartExitTimer));
		EscapeDialog escapeDialog = mEscDialogWnd;
		escapeDialog.mExitCallback = (EscapeDialog.ExitCallback)Delegate.Combine(escapeDialog.mExitCallback, new EscapeDialog.ExitCallback(ExitFromBattle));
		mLeaveWnd.SetActive(_active: false);
		mQuestJournalWnd.SetData(mCore.CtrlServer.GetSelfQuestProvider(), mCore.CtrlServer.SelfHero, mFormatedTipMgr);
		QuestJournal questJournal = mQuestJournalWnd;
		questJournal.mOnQuestCancel = (QuestJournal.OnQuestCallback)Delegate.Combine(questJournal.mOnQuestCancel, new QuestJournal.OnQuestCallback(OnQuestCancel));
		QuestJournal questJournal2 = mQuestJournalWnd;
		questJournal2.mOnActiveQuestChanged = (QuestJournal.VoidCallback)Delegate.Combine(questJournal2.mOnActiveQuestChanged, new QuestJournal.VoidCallback(OnActiveQuestChanged));
		mQuestProgressWnd.SetData(mCore.CtrlServer.GetSelfQuestProvider(), mCore.Battle.SelfPlayer.GetSelfPvpQuestProvider(), mCore.Battle.GetTimer());
		QuestProgressMenu questProgressMenu = mQuestProgressWnd;
		questProgressMenu.mOnShowQuestInfo = (QuestProgressMenu.ShowQuestInfoCallback)Delegate.Combine(questProgressMenu.mOnShowQuestInfo, new QuestProgressMenu.ShowQuestInfoCallback(ShowQuestInfo));
		mQuestStatusWnd.SetData(mCore.CtrlServer.SelfHero, mFormatedTipMgr);
		ShopGUI shopGUI = mShopWnd;
		shopGUI.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Combine(shopGUI.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		HandlerManager<BattlePacket, BattleCmdId> handlerMgr = mCore.BattleServer.PacketMgr.HandlerMgr;
		handlerMgr.Subscribe<BattleEndArg>(BattleCmdId.BATTLE_END, null, null, OnBattleEnd);
		handlerMgr.Subscribe(BattleCmdId.EQUIP_ITEM, null, OnEquipFailed);
		handlerMgr.Subscribe<DropInfoArg>(BattleCmdId.GET_DROP_INFO, null, null, OnDropInfo);
		handlerMgr.Subscribe<RefreshDropInfoArg>(BattleCmdId.REFRESH_DROP_CONTENT, null, null, OnRefreshDropInfo);
		handlerMgr.Subscribe<NotifyBeaconArg>(BattleCmdId.NOTIFY_BEACON, null, null, OnBeacon);
		handlerMgr.Subscribe(BattleCmdId.MOVE_PLAYER, mTutorial.OnMoveSendDone, null);
		handlerMgr.Subscribe(BattleCmdId.UPGRADE_SKILL, mTutorial.OnSkillUp, null);
		handlerMgr.Subscribe<DoActionArg>(BattleCmdId.DO_ACTION, null, null, mTutorial.OnSkillUse);
		handlerMgr.Subscribe<KillArg>(BattleCmdId.ON_KILL, null, null, mTutorial.OnKill);
		handlerMgr.Subscribe<LevelUpArg>(BattleCmdId.LEVEL_UP, null, null, mTutorial.OnLevelUp);
		mTutorial.SetEffectorData(battle.GetEffectorProvider(), battle.SelfPlayer.Player.AvatarObjId);
		HandlerManager<CtrlPacket, Enum> handlerMgr2 = mCore.CtrlServer.EntryPoint.HandlerMgr;
		handlerMgr2.Subscribe<FightLogArg>(CtrlCmdId.fight.log, null, null, OnFightLog);
		handlerMgr2.Subscribe<ShopContentArg>(CtrlCmdId.store.list, null, null, OnShopContent);
		handlerMgr2.Subscribe<UserLeaveInfoArg>(CtrlCmdId.user.leave_info, null, null, OnUserLeave);
		mCore.CtrlServer.FightSender.LeaveInfoRequest();
		mScreenMgr.Gui.SetCurGuiSet("battle");
		mShopWnd.SetData(mCore.CtrlServer.SelfHero, mFormatedTipMgr, mPlayerCtrl.SelfPlayer);
		mMainInfoWnd.SetData(this, mPlayerCtrl, (MapType)mBattleType, mCore.CtrlServer.GetPrototypeProvider(), mCore.CtrlServer.GetGroupedItems(), battle.GetPrototypeProvider(), battle.ArticleToProto, mCore.CtrlServer.Heroes);
		foreach (BattleThing item in mPlayerCtrl.SelfPlayer.Inventory.Content)
		{
			if (item.CtrlProto.Article.IsConsumable())
			{
				mMainInfoWnd.AddBattleItem(item.CtrlProto.Id);
			}
		}
		mFightView.Init(mCore.CtrlServer.FightHelper, mCore.Battle.SelfPlayer.PlayerId);
		mCore.CtrlServer.FightHelper.BindGui(mFightView);
		mBattleStats.SetData(battle.GetPlayerProvider(), (MapType)mBattleType);
		UpdateInventory();
		mCore.CtrlServer.Heroes.UpdateGameInfo(mPlayerCtrl.SelfPlayer.PlayerId, null);
		mCore.CtrlServer.SendGetMoney();
		mFirstTimeQuests = true;
		mQuestProgressWnd.UpdateInfo(new List<int>());
		mChatWnd.SetChatModes(new ChatChannel[5]
		{
			ChatChannel.fight,
			ChatChannel.private_msg,
			ChatChannel.group,
			ChatChannel.team,
			ChatChannel.clan
		});
		SelfPvpQuestStore selfPvpQuests = mCore.Battle.SelfPlayer.SelfPvpQuests;
		foreach (ISelfPvpQuest @object in selfPvpQuests.Objects)
		{
			OnSelfPveQuestsUpdated(selfPvpQuests, @object, _added: true);
		}
		mTutorial.SetWindow(mTutorialWnd);
		mTutorial.SetScreenId("battle");
		bool firstBlood = true;
		foreach (Player item2 in battle.GetPlayerProvider().Content)
		{
			if (item2.KillsCount > 0)
			{
				firstBlood = false;
				break;
			}
		}
		mKillEventMgr.SetFirstBlood(firstBlood);
		mCastleView = new CastleJoinView(mCore.CtrlServer.CastleHelper);
		mCastleView.SetData(null, null, mCore.CtrlServer.Clan, mOkDialogWnd, null, mMapJoinQueueWnd, mScreenMgr, null, null);
		mCore.CtrlServer.CastleHelper.BindGui(mCastleView);
		DamageScreenEffect componentInChildren = Camera.mainCamera.GetComponentInChildren<DamageScreenEffect>();
		if (componentInChildren != null)
		{
			componentInChildren.SyncedParams = mCore.Battle.GetGameObjProvider().Get(mPlayerCtrl.SelfPlayer.Player.AvatarObjId).Data.Params;
		}
		else
		{
			Log.Warning("Camera not ready");
		}
		BattleStats battleStats2 = mBattleStats;
		battleStats2.mOnNickClick = (BattleStats.OnNickClick)Delegate.Combine(battleStats2.mOnNickClick, new BattleStats.OnNickClick(mChatWnd.AddRecipientToMsg));
	}

	protected override void ReinitSelfPlayerGui()
	{
		base.ReinitSelfPlayerGui();
		int lastLoadedMapType = (mCore.MapManager as SceneManager).GetLastLoadedMapType();
		mMainInfoWnd.SetData(this, mPlayerCtrl, (MapType)lastLoadedMapType, mCore.CtrlServer.GetPrototypeProvider(), mCore.CtrlServer.GetGroupedItems(), mCore.Battle.GetPrototypeProvider(), mCore.Battle.ArticleToProto, mCore.CtrlServer.Heroes);
		mObjectInfoPlayerWnd.Clear();
		mObjectInfoPlayerWnd.SetActive(_active: true);
		mObjectInfoPlayerWnd.SetData(mCore.Battle.GetGameObjProvider());
		mObjectInfoPlayerWnd.SetObj(mPlayerCtrl.SelfPlayer.Player.AvatarObjId);
		mInventoryWnd.UpdatePotions();
		DamageScreenEffect componentInChildren = Camera.mainCamera.GetComponentInChildren<DamageScreenEffect>();
		if (componentInChildren != null)
		{
			componentInChildren.SyncedParams = mCore.Battle.GetGameObjProvider().Get(mPlayerCtrl.SelfPlayer.Player.AvatarObjId).Data.Params;
		}
		else
		{
			Log.Warning("Camera not ready");
		}
	}

	public override void Hide()
	{
		base.Hide();
		Uninit();
	}

	private void Uninit()
	{
		mScreenMgr.Gui.SetCurGuiSet("loading_screen");
		SoundSystem.Instance.StopAllMusic();
		if (mCore.BattleServer != null)
		{
			mCore.BattleServer.mReconnectStarted -= OnMicroReconnectStarted;
			mCore.BattleServer.mReconnectStoped -= OnMicroReconnectStoped;
		}
		mCore.CtrlServer.EntryPoint.HandlerMgr.Unsubscribe(this);
		mCore.BattleServer.PacketMgr.HandlerMgr.Unsubscribe(this);
		HandlerManager<BattlePacket, BattleCmdId> handlerMgr = mCore.BattleServer.PacketMgr.HandlerMgr;
		handlerMgr.Unsubscribe(BattleCmdId.MOVE_PLAYER, mTutorial.OnMoveSendDone, null);
		handlerMgr.Unsubscribe(BattleCmdId.UPGRADE_SKILL, mTutorial.OnSkillUp, null);
		handlerMgr.Unsubscribe<DoActionArg>(BattleCmdId.DO_ACTION, null, null, mTutorial.OnSkillUse);
		handlerMgr.Unsubscribe<KillArg>(BattleCmdId.ON_KILL, null, null, mTutorial.OnKill);
		handlerMgr.Unsubscribe<LevelUpArg>(BattleCmdId.LEVEL_UP, null, null, mTutorial.OnLevelUp);
		mTutorial.UnInit();
		if (mCore.IsBattleCreated())
		{
			Battle battle = mCore.Battle;
			battle.RemoveGameObjInitializer(this);
			SelfPlayer selfPlayer = battle.SelfPlayer;
			selfPlayer.mInventoryChangedCallback = (SelfPlayer.InventoryChangedCallback)Delegate.Remove(selfPlayer.mInventoryChangedCallback, new SelfPlayer.InventoryChangedCallback(OnBattleInventoryChanged));
			BattleInventory battleInventory = battle.SelfPlayer.BattleInventory;
			battleInventory.mAddCallback = (Store<BattleThing>.AddCallback)Delegate.Remove(battleInventory.mAddCallback, new Store<BattleThing>.AddCallback(OnBattleInventoryAdd));
			BattleInventory battleInventory2 = battle.SelfPlayer.BattleInventory;
			battleInventory2.mRemoveCallback = (Store<BattleThing>.RemoveCallback)Delegate.Remove(battleInventory2.mRemoveCallback, new Store<BattleThing>.RemoveCallback(OnBattleInventoryRemove));
			battle.SelfPlayer.SelfPvpQuests.UnsubscribeOnUpdated(OnSelfPveQuestsUpdated);
		}
		if (mVisualBattle != null)
		{
			mVisualBattle.Unsubscribe();
			mVisualBattle = null;
		}
		if (mPlayerCtrl != null)
		{
			mPlayerCtrl.Uninit();
			mPlayerCtrl = null;
		}
		if (mExitTimer != null)
		{
			mExitTimer.Enabled = false;
			mExitTimer.Close();
			mExitTimer = null;
		}
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Uninit();
		}
		GuiSystem.mGuiInputMgr.Clean();
		VisualEffectOptions.Uninit();
		if (VisualEffectsMgr.Instance != null)
		{
			VisualEffectsMgr.Instance.Uninit();
		}
		BattleStats battleStats = mBattleStats;
		battleStats.mOnNickClick = (BattleStats.OnNickClick)Delegate.Remove(battleStats.mOnNickClick, new BattleStats.OnNickClick(mChatWnd.AddRecipientToMsg));
		mFightView.Uninit();
		mCore.CtrlServer.FightHelper.UnbindGui();
		mCore.CtrlServer.SelfQuests.UnsubscribeOnUpdated(OnSelfQuestsUpdated);
		Chat chat = mCore.CtrlServer.Chat;
		chat.mOnErrorCallback = (Chat.OnErrorCallback)Delegate.Remove(chat.mOnErrorCallback, new Chat.OnErrorCallback(OnChatError));
		mMainInfoWnd.Clear();
		MainInfoWindow mainInfoWindow = mMainInfoWnd;
		mainInfoWindow.mOnInitInventory = (MainInfoWindow.OnInitInventory)Delegate.Remove(mainInfoWindow.mOnInitInventory, new MainInfoWindow.OnInitInventory(ToggleInventory));
		MainInfoWindow mainInfoWindow2 = mMainInfoWnd;
		mainInfoWindow2.mOnForceRespawn = (MainInfoWindow.VoidCallback)Delegate.Remove(mainInfoWindow2.mOnForceRespawn, new MainInfoWindow.VoidCallback(OnForceRespawnRequest));
		MainInfoWindow mainInfoWindow3 = mMainInfoWnd;
		mainInfoWindow3.mOnPortal = (MainInfoWindow.VoidCallback)Delegate.Remove(mainInfoWindow3.mOnPortal, new MainInfoWindow.VoidCallback(ExitFromBattle));
		MainInfoWindow mainInfoWindow4 = mMainInfoWnd;
		mainInfoWindow4.mOnQuestJournal = (MainInfoWindow.ShowQuestJournalCallback)Delegate.Remove(mainInfoWindow4.mOnQuestJournal, new MainInfoWindow.ShowQuestJournalCallback(ShowQuestJournal));
		MainInfoWindow mainInfoWindow5 = mMainInfoWnd;
		mainInfoWindow5.mOnShop = (MainInfoWindow.VoidCallback)Delegate.Remove(mainInfoWindow5.mOnShop, new MainInfoWindow.VoidCallback(ShopContentRequest));
		MainInfoWindow mainInfoWindow6 = mMainInfoWnd;
		mainInfoWindow6.mOnBuyItem = (MainInfoWindow.BattleItemCallback)Delegate.Remove(mainInfoWindow6.mOnBuyItem, new MainInfoWindow.BattleItemCallback(OnBuyBattleItem));
		MainInfoWindow mainInfoWindow7 = mMainInfoWnd;
		mainInfoWindow7.mOnRemoveItem = (MainInfoWindow.BattleItemCallback)Delegate.Remove(mainInfoWindow7.mOnRemoveItem, new MainInfoWindow.BattleItemCallback(OnRemoveBattleItem));
		mBattleStats.Uninit();
		BattleStats battleStats2 = mBattleStats;
		battleStats2.mMenuCallback = (BattleStats.MenuCallback)Delegate.Remove(battleStats2.mMenuCallback, new BattleStats.MenuCallback(ShowEscDialog));
		mChatWnd.Uninit();
		mChatWnd.SetActive(_active: false);
		mMiniMapWnd.Clear();
		mObjectInfoPlayerWnd.Clear();
		mObjectInfoEnemyWnd.Clear();
		mEnemyInfoWnd.Clear();
		mInventoryWnd.Clean();
		mBattleStats.Uninit();
		mPopupInfoWnd.mHints.Clear();
		PopupEvent.Destroy();
		mGameInfoWnd.Clear();
		mDropWnd.Clear();
		mGameEndWnd.SetActive(_active: false);
		GameEndMenu gameEndMenu = mGameEndWnd;
		gameEndMenu.mExitCallback = (GameEndMenu.ExitCallback)Delegate.Remove(gameEndMenu.mExitCallback, new GameEndMenu.ExitCallback(ExitFromBattle));
		GameEndMenu gameEndMenu2 = mGameEndWnd;
		gameEndMenu2.mStartExitTimerCallback = (GameEndMenu.StartExitTimer)Delegate.Remove(gameEndMenu2.mStartExitTimerCallback, new GameEndMenu.StartExitTimer(OnStartExitTimer));
		mOkDialogWnd.Clean();
		mYesNoDialogWnd.Clean();
		EscapeDialog escapeDialog = mEscDialogWnd;
		escapeDialog.mExitCallback = (EscapeDialog.ExitCallback)Delegate.Remove(escapeDialog.mExitCallback, new EscapeDialog.ExitCallback(ExitFromBattle));
		mEscDialogWnd.Hide();
		mAfkWarnDialog.SetActive(_active: false);
		mMapJoinQueueWnd.Clean();
		mQuestJournalWnd.Clean();
		QuestJournal questJournal = mQuestJournalWnd;
		questJournal.mOnQuestCancel = (QuestJournal.OnQuestCallback)Delegate.Remove(questJournal.mOnQuestCancel, new QuestJournal.OnQuestCallback(OnQuestCancel));
		QuestJournal questJournal2 = mQuestJournalWnd;
		questJournal2.mOnActiveQuestChanged = (QuestJournal.VoidCallback)Delegate.Remove(questJournal2.mOnActiveQuestChanged, new QuestJournal.VoidCallback(OnActiveQuestChanged));
		mQuestJournalWnd.SetActive(_active: false);
		QuestProgressMenu questProgressMenu = mQuestProgressWnd;
		questProgressMenu.mOnShowQuestInfo = (QuestProgressMenu.ShowQuestInfoCallback)Delegate.Remove(questProgressMenu.mOnShowQuestInfo, new QuestProgressMenu.ShowQuestInfoCallback(ShowQuestInfo));
		mQuestStatusWnd.SetActive(_active: false);
		mShopWnd.Clear();
		ShopGUI shopGUI = mShopWnd;
		shopGUI.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Remove(shopGUI.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		mCore.CtrlServer.CastleHelper.UnbindGui();
		if (mCastleView != null)
		{
			mCastleView.Uninit();
		}
	}

	public void InitGameObject(IGameObject _obj)
	{
		GameData gameData = _obj as GameData;
		gameData.InitMapSymbol();
		if (gameData.NetTransform != null)
		{
			gameData.NetTransform.SetHeightMap(mHeightMap);
		}
		if (gameData.Proto.Destructible != null)
		{
			CursorDetector cursorDetector = gameData.gameObject.AddComponent<CursorDetector>();
			cursorDetector.Init(mPlayerCtrl);
		}
		if (gameData.Proto.Touchable != null)
		{
			TouchableDetector touchableDetector = gameData.gameObject.AddComponent<TouchableDetector>();
			touchableDetector.Init(mCore.BattleServer);
			CursorDetector cursorDetector2 = gameData.gameObject.AddComponent<CursorDetector>();
			cursorDetector2.Init(mPlayerCtrl);
		}
		if (gameData.Proto.ItemContainer != null && gameData.Proto.Avatar == null)
		{
			DropContent dropContent = gameData.gameObject.AddComponent<DropContent>();
			dropContent.Init(mCore.BattleServer, mPlayerCtrl, VisualEffectsMgr.Instance);
			CursorDetector cursorDetector3 = gameData.gameObject.AddComponent<CursorDetector>();
			cursorDetector3.Init(mPlayerCtrl);
		}
		if (gameData.Proto.Shop != null)
		{
			Shop shop = gameData.gameObject.GetComponent<Shop>();
			if (shop == null)
			{
				shop = gameData.gameObject.AddComponent<Shop>();
			}
			shop.Init(mBattleShopWnd, mCore.Battle.GetPrototypeProvider(), mCore.CtrlServer.GetPrototypeProvider(), mPlayerCtrl);
		}
		if (gameData.Proto.Avatar != null)
		{
			gameData.gameObject.AddComponent<EffectEmiter>();
		}
		DeathBehaviour[] components = gameData.gameObject.GetComponents<DeathBehaviour>();
		DeathBehaviour[] array = components;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			deathBehaviour.Init(mCore.GameObjectManager as GameObjManager);
		}
	}

	private void OnSelectionChanged(int _objId)
	{
		if (mPlayerCtrl.SelfPlayer.Player.AvatarObjId != _objId)
		{
			mObjectInfoEnemyWnd.SetObj(_objId);
			mEnemyInfoWnd.SetEnemy(_objId);
		}
	}

	private void OnForceRespawnRequest()
	{
		mCore.BattleServer.SendForceRespawn();
	}

	private void ShowEscDialog()
	{
		if (mEscDialogWnd.Active)
		{
			mEscDialogWnd.Hide();
		}
		else
		{
			mEscDialogWnd.Show();
		}
	}

	public void Exit()
	{
		if (mCore.BattleServer != null)
		{
			mCore.BattleServer.NeedMicroReconnect(_needReconnect: false);
		}
		if (mCore.CtrlServer != null && mCore.CtrlServer.MpdConnection != null)
		{
			mCore.CtrlServer.MpdConnection.NeedMicroReconnect(_needReconnect: false);
		}
		mCore.CtrlServer.SendReconnectRequest();
		mScreenMgr.Holder.HideCurScreen();
	}

	private void ExitFromBattle()
	{
		Exit();
		PrepareFightLog();
		mCore.CtrlServer.FightSender.LeaveInfoRequest();
	}

	private void PrepareFightLog()
	{
		CentralSquareScreen centralSquareScreen = mScreenMgr.Holder.GetScreen(ScreenType.CS) as CentralSquareScreen;
		centralSquareScreen.PrepareFightLog(mCore.BattleServer.BattleId, mBattleType, mCore.Battle.GetSelfPlayerHistory(), null);
	}

	private void ShopContentRequest()
	{
		if (!mShopWnd.Active)
		{
			mCore.CtrlServer.ShopSender.ContentRequest((MapType)mBattleType);
		}
		else
		{
			mShopWnd.Close();
		}
	}

	private void OnShopContent(ShopContentArg _arg)
	{
		IStoreContentProvider<CtrlPrototype> prototypeProvider = mCore.CtrlServer.GetPrototypeProvider();
		List<KeyValuePair<int, CtrlItem>> list = new List<KeyValuePair<int, CtrlItem>>();
		foreach (KeyValuePair<int, ShopContentArg.Category> mItem in _arg.mItems)
		{
			CtrlItem ctrlItem = new CtrlItem();
			ctrlItem.mItemGroupWeight = mItem.Value.mWeight;
			ctrlItem.mItems = new List<CtrlPrototype>();
			foreach (int mArticle in mItem.Value.mArticles)
			{
				CtrlPrototype ctrlPrototype = prototypeProvider.Get(mArticle);
				if (ctrlPrototype != null)
				{
					ctrlItem.mItems.Add(ctrlPrototype);
				}
			}
			list.Add(new KeyValuePair<int, CtrlItem>(mItem.Key, ctrlItem));
		}
		mShopWnd.SetItems(list);
		mShopWnd.Open();
	}

	private void Buy(Dictionary<int, int> _basket)
	{
		mCore.CtrlServer.SelfHero.BuyFromBattle(_basket, mBattleType);
	}

	private void ShowQuestJournal(int _questId)
	{
		if (_questId == -1)
		{
			mQuestJournalWnd.SetActive(!mQuestJournalWnd.Active);
		}
		else
		{
			mQuestJournalWnd.SetActive(_active: true);
		}
		mQuestJournalWnd.SetSelectedQuest(_questId);
	}

	private void ShowQuestInfo(int _questId, bool _pvp)
	{
		if (_pvp)
		{
			ISelfPvpQuest selfPvpQuest = mCore.Battle.SelfPlayer.SelfPvpQuests.Get(_questId);
			if (selfPvpQuest != null)
			{
				mQuestStatusWnd.SetQuest(selfPvpQuest);
				mQuestStatusWnd.SetActive(_active: true);
			}
		}
		else
		{
			ShowQuestJournal(_questId);
		}
	}

	private void OnQuestCancel(int _questId)
	{
		mCore.CtrlServer.SelfQuests.Cancel(_questId);
	}

	private void OnSelfQuestsUpdated(SelfQuestStore _selfQuests, IList<int> _added, IList<int> _updated)
	{
		if (mQuestJournalWnd.Active)
		{
			mQuestJournalWnd.SetActive(_active: true);
		}
		if (mFirstTimeQuests)
		{
			mFirstTimeQuests = false;
			mQuestProgressWnd.UpdateInfo(new List<int>());
			return;
		}
		foreach (int item in _added)
		{
			OptionsMgr.SetActiveQuest(item, _show: true);
		}
		List<int> list = new List<int>();
		list.AddRange(_added);
		list.AddRange(_updated);
		mQuestProgressWnd.UpdateInfo(list);
	}

	private void OnSelfPveQuestsUpdated(SelfPvpQuestStore _store, ISelfPvpQuest _quest, bool _added)
	{
		List<int> list = new List<int>();
		list.Add(_quest.Id);
		mQuestProgressWnd.UpdateInfo(list);
		if (_quest.State < 0 || _added)
		{
			mQuestStatusWnd.SetQuest(_quest);
			mQuestStatusWnd.SetActive(_active: true);
		}
	}

	private void OnActiveQuestChanged()
	{
		mQuestProgressWnd.UpdateInfo(new List<int>());
	}

	private void OnDropItem(int _itemOrigin, int _itemId, int _count)
	{
		switch (_itemOrigin)
		{
		case 1:
			mPlayerCtrl.SelfPlayer.DropThing(_itemId, _count);
			break;
		case 2:
			mCore.CtrlServer.HeroSender.DropItem(_itemId, _count);
			mCore.CtrlServer.HeroSender.GetBag();
			break;
		}
	}

	private void UpdateInventory()
	{
		List<Thing> list = new List<Thing>();
		foreach (BattleThing item in mPlayerCtrl.SelfPlayer.Inventory.Content)
		{
			if (item != null && !item.CtrlProto.Article.IsConsumable())
			{
				list.Add(item);
			}
		}
		mInventoryWnd.SetItems(list);
	}

	private void ToggleInventory()
	{
		if (mInventoryWnd.Active)
		{
			mInventoryWnd.Close();
			UpdateInventory();
		}
		else
		{
			mInventoryWnd.Open();
		}
		if (mInventoryWnd.Active)
		{
			UpdateInventory();
		}
	}

	private void OnBattleInventoryChanged(SelfPlayer _player)
	{
		UpdateInventory();
	}

	public void UseItem(int _itemId)
	{
		SelfPlayer selfPlayer = mPlayerCtrl.SelfPlayer;
		if (selfPlayer != null && selfPlayer.Player.IsAvatarBinded)
		{
			BattleThing battleThing = selfPlayer.Inventory.Get(_itemId);
			string comment = ((battleThing != null) ? GuiSystem.GetLocaleText(battleThing.CtrlProto.Desc.mName) : string.Empty);
			UserLog.AddAction(UserActionType.USE_BATTLE_ITEM, _itemId, comment);
			mPlayerCtrl.SetActiveItem(battleThing);
		}
	}

	public double GetCooldownProgress(int _itemId)
	{
		BattleInventory battleInventory = mPlayerCtrl.SelfPlayer.BattleInventory;
		return battleInventory.GetCooldownProgress(_itemId);
	}

	public double GetCooldownProgress(CtrlPrototype _item)
	{
		BattleInventory battleInventory = mPlayerCtrl.SelfPlayer.BattleInventory;
		return battleInventory.GetCooldownProgress(_item);
	}

	private void OnEquipFailed(int _errorCode)
	{
		UpdateInventory();
		mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_INVENTORY_CANNOT_DRESS"), null);
	}

	private void OnBuyBattleItem(int _articleId)
	{
		if (mPlayerCtrl.SelfPlayer != null && mCore.Battle != null)
		{
			if (!mCore.Battle.ArticleToProto.TryGetValue(_articleId, out var value))
			{
				Log.Warning("cannot associate article " + _articleId + " with battle prototype");
			}
			else
			{
				mPlayerCtrl.SelfPlayer.Buy(0, 0, value, 1);
			}
		}
	}

	private void OnRemoveBattleItem(int _articleId)
	{
		if (mPlayerCtrl.SelfPlayer != null)
		{
			mPlayerCtrl.SelfPlayer.DropThingByArticle(_articleId);
		}
	}

	private void OnBattleInventoryAdd(Store<BattleThing> _inventory, BattleThing _item)
	{
		if (_item != null && _item.CtrlProto.Article.IsConsumable())
		{
			mMainInfoWnd.AddBattleItem(_item.CtrlProto.Id);
		}
	}

	private void OnBattleInventoryRemove(Store<BattleThing> _inventory, BattleThing _item)
	{
		if (_item.CtrlProto.Article.IsConsumable())
		{
			mMainInfoWnd.RemoveBattleItem(_item.CtrlProto.Id);
		}
	}

	private void OnStartExitTimer()
	{
		if (mExitTimer == null)
		{
			mExitTimer = new Timer();
			mExitTimer.AutoReset = false;
			mExitTimer.Elapsed += OnTimeoutExit;
			mExitTimer.Interval = mWaitStatTime * 1000;
			mExitTimer.Enabled = true;
		}
	}

	private void OnTimeoutExit(object _sender, ElapsedEventArgs _e)
	{
		ExitFromBattle();
		mExitTimer.Enabled = false;
	}

	private void OnBattleEnd(BattleEndArg _arg)
	{
		mCore.CtrlServer.KeepSessionAlive = false;
	}

	private void OnFightLog(FightLogArg _arg)
	{
		mGameEndWnd.SetData((MapType)mBattleType, _arg.mData, mCore.CtrlServer.GetCtrlAvatarStore());
		mGameEndWnd.Open();
		foreach (KeyValuePair<int, BattleEndData> mDatum in _arg.mData)
		{
			if (mDatum.Key == mCore.UserNetData.UserId)
			{
				UserLog.AddAction((mDatum.Value.mFinishType != 1) ? UserActionType.LOSE : UserActionType.WIN);
			}
		}
		CentralSquareScreen centralSquareScreen = mScreenMgr.Holder.GetScreen(ScreenType.CS) as CentralSquareScreen;
		centralSquareScreen.PrepareFightLog(mCore.BattleServer.BattleId, mBattleType, mCore.Battle.GetSelfPlayerHistory(), _arg.mData);
	}

	private void OnDropInfo(DropInfoArg _arg)
	{
		IGameObject gameObject = mCore.Battle.GetGameObjProvider().Get(_arg.mContainerId);
		if (gameObject != null)
		{
			GameData gameData = gameObject as GameData;
			DropContent component = gameData.gameObject.GetComponent<DropContent>();
			if (component != null)
			{
				component.SetContent(_arg.mItems);
				component.SetDropWnd(mDropWnd);
			}
			else
			{
				Log.Error("received drop info for not a drop container");
			}
		}
	}

	private void OnRefreshDropInfo(RefreshDropInfoArg _arg)
	{
		mCore.BattleServer.SendGetDropInfo(_arg.mContainerId);
	}

	private void OnAfkStateChanged(BattleInput.AfkState _state)
	{
		switch (_state)
		{
		case BattleInput.AfkState.ALLOWABLE:
			mAfkWarnDialog.SetActive(_active: false);
			break;
		case BattleInput.AfkState.WARN:
			mAfkWarnDialog.SetData(mCore.Config.AfkWarnTime, "GUI_AFK_WARNING");
			break;
		case BattleInput.AfkState.KICK:
		{
			GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("central_square");
			if (guiSet.Inited)
			{
				OkDialog elementById = guiSet.GetElementById<OkDialog>("OK_DAILOG");
				elementById.SetData(GuiSystem.GetLocaleText("GUI_AFK_MESSAGE"), null);
			}
			ExitFromBattle();
			break;
		}
		}
	}

	private void OnMicroReconnectStarted(int _reconnectTime)
	{
		if (mTimerWarning.Active)
		{
			mTimerWarning.SetTimer(_reconnectTime);
		}
		else
		{
			mTimerWarning.SetData(_reconnectTime, "GUI_MRC_MESSAGE");
		}
	}

	private void OnMicroReconnectStoped(bool _result)
	{
		if (_result)
		{
			mCore.CtrlServer.SendFastReconnect();
			mCore.CtrlServer.FightHelper.MicroReconnected();
		}
		mTimerWarning.SetActive(_active: false);
	}

	private void OnBeacon(NotifyBeaconArg _arg)
	{
		mMiniMapWnd.AddBeacon(_arg.mX, _arg.mY);
		SoundSystem.Instance.PlaySoundEvent("minimap_marker");
	}

	private void OnChatError(Chat.Error _errorType)
	{
		if (_errorType == Chat.Error.GAG)
		{
			mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_CHAT_ERROR_GAG"));
		}
	}

	private void OnUserLeave(UserLeaveInfoArg _arg)
	{
		mEscDialogWnd.SetLeaveInfo(_arg);
	}
}
