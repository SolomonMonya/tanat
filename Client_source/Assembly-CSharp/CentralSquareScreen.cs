using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;
using Network;
using TanatKernel;
using UnityEngine;

public class CentralSquareScreen : BaseCentralSquareScreen, IGameObjectInitializer
{
	private ScreenManager mScreenMgr;

	private CtrlServerConnection mCtrlSrv;

	private TanatApp mApp;

	private PlayerControl mPlayerCtrl;

	private Notifier<CentralSquareScreen, object>.Group mOnShowNotifiers = new Notifier<CentralSquareScreen, object>.Group();

	private CSMainMenu mMainCsWnd;

	private SelectGameMenu mSelGameMenu;

	private SelectAvatarWindow mSelAvaWnd;

	private MiniMap mMiniMapWnd;

	private WorldMapMenu mWorldMapMenu;

	private ChatWindow mChatWnd;

	private CSStats mStatsWnd;

	private HeroInfo mHeroInfoWnd;

	private ShopGUI mShopWnd;

	private ShopGUI mShopRealWnd;

	private InventoryMenu mInventoryWnd;

	private FormatedTipMgr mFormatedTipMgr;

	private PlayerTradeMenu mTradeWnd;

	private YesNoDialog mYesNoDialogWnd;

	private OkDialog mOkDialogWnd;

	private PopUpMenu mPopupMenuWnd;

	private PopupInfo mPopupInfoWnd;

	private Bank mBankWnd;

	private AvatarBuyMenu mAvatarShopWnd;

	private CompensationMenu mCompensationWnd;

	private GroupWindow mGroupWnd;

	private TutorialWindow mTutorialWnd;

	private ItemCountMenu mItemCountWnd;

	private MapJoinQueueMenu mMapJoinQueueWnd;

	private NPCMenu mNpcWnd;

	private ClanMenu mClanMenu;

	private ClanMenuCreate mClanMenuCreate;

	private QuestMenu mQuestWnd;

	private QuestJournal mQuestJournalWnd;

	private QuestProgressMenu mQuestProgressWnd;

	private CastleMenu mCastleMenu;

	private CastleRequestMenu mCastleRequestMenu;

	private CastleHistoryMenu mCastleHistoryMenu;

	private EscapeDialog mEscDialogWnd;

	private TradeView mTradeView;

	private TradeManager mTradeMgr;

	private GroupView mGroupView;

	private ClanView mClanView;

	private FightView mFightView;

	private TutorialMgr mTutorial;

	private CastleJoinView mCastleView;

	private PlayersListController mListController;

	private CurseInfo mCurseWnd;

	private RewardInfo mRewardWnd;

	private Dictionary<Building, BuildingSelector> mFunctionalBuildings = new Dictionary<Building, BuildingSelector>();

	private bool mFirstTimeQuests = true;

	private bool mShopTypeRequested;

	private ShortPlayerInfo mSelfPlayerHistory;

	private int mFightLogBattleId = -1;

	private Dictionary<int, BattleEndData> mFightLogData;

	private int mBattleType;

	public Notifier<CentralSquareScreen, object>.Group OnShowNotifiers => mOnShowNotifiers;

	public CentralSquareScreen(Core _core, ScreenManager _screenMgr, TanatApp _app)
		: base(_core)
	{
		mScreenMgr = _screenMgr;
		mCtrlSrv = mCore.CtrlServer;
		mApp = _app;
		mTutorial = mApp.TutorialMgr();
		GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("central_square");
		mMainCsWnd = guiSet.GetElementById<CSMainMenu>("CENTRAL_SQUARE_MENU");
		mMiniMapWnd = guiSet.GetElementById<MiniMap>("MINI_MAP_GAME_MENU");
		mWorldMapMenu = guiSet.GetElementById<WorldMapMenu>("WORLD_MAP_MENU");
		mChatWnd = guiSet.GetElementById<ChatWindow>("CHAT_WINDOW");
		mStatsWnd = guiSet.GetElementById<CSStats>("CENTRAL_SQUARE_STATS");
		mHeroInfoWnd = guiSet.GetElementById<HeroInfo>("HERO_MENU");
		mShopWnd = guiSet.GetElementById<ShopGUI>("SHOP_WINDOW");
		mShopRealWnd = guiSet.GetElementById<ShopGUI>("SHOP_REAL_WINDOW");
		mInventoryWnd = guiSet.GetElementById<InventoryMenu>("INVENTORY_MENU");
		mTradeWnd = guiSet.GetElementById<PlayerTradeMenu>("TRADE_MENU");
		mYesNoDialogWnd = guiSet.GetElementById<YesNoDialog>("YES_NO_DAILOG");
		mOkDialogWnd = guiSet.GetElementById<OkDialog>("OK_DAILOG");
		mPopupMenuWnd = guiSet.GetElementById<PopUpMenu>("POPUP_MENU");
		mPopupInfoWnd = guiSet.GetElementById<PopupInfo>("POPUP_INFO");
		mBankWnd = guiSet.GetElementById<Bank>("BANK");
		mCompensationWnd = guiSet.GetElementById<CompensationMenu>("COMPENSATION_MENU");
		mGroupWnd = guiSet.GetElementById<GroupWindow>("GROUP_WINDOW");
		mTutorialWnd = guiSet.GetElementById<TutorialWindow>("TUTORIAL_WINDOW");
		mItemCountWnd = guiSet.GetElementById<ItemCountMenu>("ITEM_COUNT_MENU");
		mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		mMapJoinQueueWnd = guiSet.GetElementById<MapJoinQueueMenu>("MAP_JOIN_QUEUE_MENU");
		mSelGameMenu = guiSet.GetElementById<SelectGameMenu>("SELECT_GAME_MENU");
		mNpcWnd = guiSet.GetElementById<NPCMenu>("NPC_MENU");
		mQuestWnd = guiSet.GetElementById<QuestMenu>("QUEST_MENU");
		mQuestJournalWnd = guiSet.GetElementById<QuestJournal>("QUEST_JOURNAL_MENU");
		mQuestProgressWnd = guiSet.GetElementById<QuestProgressMenu>("QUEST_PROGRESS_MENU");
		mClanMenu = guiSet.GetElementById<ClanMenu>("CLAN_MENU");
		mClanMenuCreate = guiSet.GetElementById<ClanMenuCreate>("CLAN_MENU_CREATE");
		mCastleMenu = guiSet.GetElementById<CastleMenu>("CASTLE_MENU");
		mCastleRequestMenu = guiSet.GetElementById<CastleRequestMenu>("CASTLE_REQUEST_MENU");
		mCastleHistoryMenu = guiSet.GetElementById<CastleHistoryMenu>("CASTLE_HISTORY_MENU");
		mCurseWnd = guiSet.GetElementById<CurseInfo>("CURSE_INFO");
		mRewardWnd = guiSet.GetElementById<RewardInfo>("REWARD_INFO");
		GuiSystem.GuiSet guiSet2 = mScreenMgr.Gui.GetGuiSet("select_avatar");
		mSelAvaWnd = guiSet2.GetElementById<SelectAvatarWindow>("SELECT_AVATAR_WINDOW");
		mAvatarShopWnd = guiSet2.GetElementById<AvatarBuyMenu>("AVATAR_BUY_MENU");
		mEscDialogWnd = new EscapeDialog(guiSet.GetElementById<EscapeMenu>("ESCAPE_MENU"), mYesNoDialogWnd, guiSet.GetElementById<OptionsMenu>("OPTIONS_MENU"), null);
		mEscDialogWnd.SetBattleType(MapType.CS);
		mTradeView = new TradeView(mTradeWnd, mYesNoDialogWnd, mOkDialogWnd);
		mGroupView = new GroupView(mYesNoDialogWnd, mOkDialogWnd);
		mClanView = new ClanView(mYesNoDialogWnd, mOkDialogWnd);
		mFightView = new FightView(this, mScreenMgr, mMapJoinQueueWnd, mSelGameMenu, mSelAvaWnd, mTutorialWnd, mTutorial, mYesNoDialogWnd, mOkDialogWnd, mWorldMapMenu);
	}

	public override void Show()
	{
		base.Show();
		mCtrlSrv.Npcs.UpdateContent();
		mCtrlSrv.SelfQuests.UpdateContent();
		mCore.Battle.AddGameObjInitializer(this);
		mCore.CtrlServer.KeepSessionAlive = false;
		mPlayerCtrl = new PlayerControl();
	}

	protected override void ShowGui()
	{
		Battle battle = mCore.Battle;
		if (mCore.BattleServer != null)
		{
			mCore.BattleServer.NeedMicroReconnect(_needReconnect: false);
		}
		if (mCore.CtrlServer != null && mCore.CtrlServer.MpdConnection != null)
		{
			mCore.CtrlServer.MpdConnection.NeedMicroReconnect(_needReconnect: true);
		}
		mPlayerCtrl.Init(battle.SelfPlayer, battle.GetGameObjProvider(), battle.GetPrototypeProvider(), VisualEffectsMgr.Instance);
		CommonInput commonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (commonInput != null)
		{
			commonInput.Init(mScreenMgr);
		}
		CentralSquareInput centralSquareInput = GameObjUtil.FindObjectOfType<CentralSquareInput>();
		if (centralSquareInput != null)
		{
			centralSquareInput.Init(mScreenMgr, mPlayerCtrl);
			centralSquareInput.SubscribePopUpMenu(ShowPopUpMenu);
			centralSquareInput.SubscribePlayerClick(mMainCsWnd.ShowUserInfo);
		}
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Init(mCore.BattleServer.PacketMgr);
		}
		GuiSystem.mGuiInputMgr.SetGui(mItemCountWnd, mYesNoDialogWnd);
		GuiInputMgr mGuiInputMgr = GuiSystem.mGuiInputMgr;
		mGuiInputMgr.mDropItemCallback = (GuiInputMgr.DropItemCallback)Delegate.Combine(mGuiInputMgr.mDropItemCallback, new GuiInputMgr.DropItemCallback(OnDropItem));
		Chat chat = mCtrlSrv.Chat;
		chat.mOnErrorCallback = (Chat.OnErrorCallback)Delegate.Combine(chat.mOnErrorCallback, new Chat.OnErrorCallback(OnChatError));
		mMainCsWnd.SetData(mCtrlSrv.Group, mPlayerCtrl.SelfPlayer, mCtrlSrv.GetPrototypeProvider(), mFormatedTipMgr, mCtrlSrv.Heroes);
		CSMainMenu cSMainMenu = mMainCsWnd;
		cSMainMenu.mShowBattleMenuCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu.mShowBattleMenuCallback, new CSMainMenu.VoidCallback(ShowStartBattleMenu));
		CSMainMenu cSMainMenu2 = mMainCsWnd;
		cSMainMenu2.mShowHeroInfoCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu2.mShowHeroInfoCallback, new CSMainMenu.VoidCallback(ShowSelfHeroInfo));
		CSMainMenu cSMainMenu3 = mMainCsWnd;
		cSMainMenu3.mShowShopCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu3.mShowShopCallback, new CSMainMenu.VoidCallback(ShopContentRequest));
		CSMainMenu cSMainMenu4 = mMainCsWnd;
		cSMainMenu4.mShowShopRealCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu4.mShowShopRealCallback, new CSMainMenu.VoidCallback(ShopRealContentRequest));
		CSMainMenu cSMainMenu5 = mMainCsWnd;
		cSMainMenu5.mShowBagCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu5.mShowBagCallback, new CSMainMenu.VoidCallback(BagRequest));
		CSMainMenu cSMainMenu6 = mMainCsWnd;
		cSMainMenu6.mShowBankCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu6.mShowBankCallback, new CSMainMenu.VoidCallback(BankRequest));
		CSMainMenu cSMainMenu7 = mMainCsWnd;
		cSMainMenu7.mShowNPCCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu7.mShowNPCCallback, new CSMainMenu.VoidCallback(ShowNpcsMenu));
		CSMainMenu cSMainMenu8 = mMainCsWnd;
		cSMainMenu8.mOnPlayer = (CSMainMenu.OnPlayerCallback)Delegate.Combine(cSMainMenu8.mOnPlayer, new CSMainMenu.OnPlayerCallback(ShowPopUpMenu));
		CSMainMenu cSMainMenu9 = mMainCsWnd;
		cSMainMenu9.mShowClanCallback = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu9.mShowClanCallback, new CSMainMenu.VoidCallback(ShowClanMenu));
		CSMainMenu cSMainMenu10 = mMainCsWnd;
		cSMainMenu10.mOnShowWorldMap = (CSMainMenu.VoidCallback)Delegate.Combine(cSMainMenu10.mOnShowWorldMap, new CSMainMenu.VoidCallback(OnShowWorldMap));
		mSelAvaWnd.SetData(battle.SelfPlayer.PlayerId, mCore.CtrlServer.Chat, mCtrlSrv.SelfHero.Hero, mCtrlSrv.GetCtrlAvatarStore());
		SelectAvatarWindow selectAvatarWindow = mSelAvaWnd;
		selectAvatarWindow.mOnSelectLockedAvatar = (SelectAvatarWindow.OnSelectLockedAvatar)Delegate.Combine(selectAvatarWindow.mOnSelectLockedAvatar, new SelectAvatarWindow.OnSelectLockedAvatar(ShowAvatarShop));
		SelectAvatarWindow selectAvatarWindow2 = mSelAvaWnd;
		selectAvatarWindow2.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Combine(selectAvatarWindow2.mDesertCallback, new SelectAvatarWindow.DesertCallback(mCtrlSrv.StopWaitReconnect));
		mSelGameMenu.SetData(mCtrlSrv.MapTypeDescs);
		mMiniMapWnd.SetData(battle.GetGameObjProvider());
		(mCore.MapManager as SceneManager).InitMiniMap(mMiniMapWnd);
		MiniMap miniMap = mMiniMapWnd;
		miniMap.mOnClick = (MiniMap.OnMiniMapClick)Delegate.Combine(miniMap.mOnClick, new MiniMap.OnMiniMapClick(OnMiniMapClick));
		MiniMap miniMap2 = mMiniMapWnd;
		miniMap2.mOnShowWorldMap = (MiniMap.OnShowWorldMap)Delegate.Combine(miniMap2.mOnShowWorldMap, new MiniMap.OnShowWorldMap(OnShowWorldMap));
		mChatWnd.SetData(mCore.CtrlServer.Chat);
		mChatWnd.SetActive(_active: true);
		ChatWindow chatWindow = mChatWnd;
		chatWindow.mShowPopUpMenuCallback = (ChatWindow.ShowPopUpMenuCallback)Delegate.Combine(chatWindow.mShowPopUpMenuCallback, new ChatWindow.ShowPopUpMenuCallback(ShowPopUpMenu));
		CSStats cSStats = mStatsWnd;
		cSStats.mMenuCallback = (CSStats.MenuCallback)Delegate.Combine(cSStats.mMenuCallback, new CSStats.MenuCallback(ShowEscDialog));
		CSStats cSStats2 = mStatsWnd;
		cSStats2.mShowPopUpMenuCallback = (CSStats.ShowPopUpMenuCallback)Delegate.Combine(cSStats2.mShowPopUpMenuCallback, new CSStats.ShowPopUpMenuCallback(ShowPopUpMenu));
		CSStats cSStats3 = mStatsWnd;
		cSStats3.mShowClanInfoCallback = (Action<string>)Delegate.Combine(cSStats3.mShowClanInfoCallback, new Action<string>(OnClanInfoRequest));
		mMapJoinQueueWnd.SetData(mYesNoDialogWnd);
		mListController = new PlayersListController();
		mListController.mObserverEnabled = (mCtrlSrv.EntryPoint.UserData.UserFlags & 1) == 1;
		if (mListController.mObserverEnabled)
		{
			Log.Debug("Observer enabled");
		}
		mListController.Init(mStatsWnd, mOkDialogWnd, mYesNoDialogWnd, mCore.Battle.GetPlayerProvider(), mCtrlSrv.EntryPoint.HandlerMgr, mCore.CtrlServer.UsersListSender, mCore.Battle.SelfPlayer.PlayerId);
		HeroMgr heroMgr = GameObjUtil.FindObjectOfType<HeroMgr>();
		mHeroInfoWnd.SetData(mFormatedTipMgr, mCtrlSrv.GetPrototypeProvider(), mCtrlSrv.Heroes, heroMgr);
		HeroInfo heroInfo = mHeroInfoWnd;
		heroInfo.mDressCallback = (HeroInfo.DressCallback)Delegate.Combine(heroInfo.mDressCallback, new HeroInfo.DressCallback(Dress));
		HeroInfo heroInfo2 = mHeroInfoWnd;
		heroInfo2.mUndressCallback = (HeroInfo.UndressCallback)Delegate.Combine(heroInfo2.mUndressCallback, new HeroInfo.UndressCallback(Undress));
		HeroStore heroes = mCtrlSrv.Heroes;
		heroes.mNewItemDressedCallback = (HeroStore.NewItemDressedCallback)Delegate.Combine(heroes.mNewItemDressedCallback, new HeroStore.NewItemDressedCallback(OnNewItemDressed));
		HeroStore heroes2 = mCtrlSrv.Heroes;
		heroes2.mItemUndressedCallback = (HeroStore.ItemUndressedCallback)Delegate.Combine(heroes2.mItemUndressedCallback, new HeroStore.ItemUndressedCallback(OnItemUndressed));
		HeroInfo heroInfo3 = mHeroInfoWnd;
		heroInfo3.mClanInfoCallback = (Action<string>)Delegate.Combine(heroInfo3.mClanInfoCallback, new Action<string>(OnClanInfoRequest));
		ShopGUI shopGUI = mShopWnd;
		shopGUI.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Combine(shopGUI.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		ShopGUI shopGUI2 = mShopWnd;
		shopGUI2.mSellRequestCallback = (ShopGUI.SellRequestCallback)Delegate.Combine(shopGUI2.mSellRequestCallback, new ShopGUI.SellRequestCallback(Sell));
		ShopGUI shopGUI3 = mShopWnd;
		shopGUI3.mOnUpdateInventory = (ShopGUI.OnVoidAction)Delegate.Combine(shopGUI3.mOnUpdateInventory, new ShopGUI.OnVoidAction(UpdateInventoryRequest));
		ShopGUI shopGUI4 = mShopWnd;
		shopGUI4.mOnShowBank = (ShopGUI.OnVoidAction)Delegate.Combine(shopGUI4.mOnShowBank, new ShopGUI.OnVoidAction(BankRequest));
		ShopGUI shopGUI5 = mShopRealWnd;
		shopGUI5.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Combine(shopGUI5.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		ShopGUI shopGUI6 = mShopRealWnd;
		shopGUI6.mOnShowBank = (ShopGUI.OnVoidAction)Delegate.Combine(shopGUI6.mOnShowBank, new ShopGUI.OnVoidAction(BankRequest));
		mInventoryWnd.SetData(mCtrlSrv.SelfHero, mFormatedTipMgr, mPlayerCtrl);
		InventoryMenu inventoryMenu = mInventoryWnd;
		inventoryMenu.mUseCtrlItemCallback = (InventoryMenu.UseCtrlItemCallback)Delegate.Combine(inventoryMenu.mUseCtrlItemCallback, new InventoryMenu.UseCtrlItemCallback(UseCtrlItem));
		SelfHero selfHero = mCtrlSrv.SelfHero;
		selfHero.mInventoryChangedCallback = (SelfHero.InventoryChangedCallback)Delegate.Combine(selfHero.mInventoryChangedCallback, new SelfHero.InventoryChangedCallback(OnInventoryChanged));
		mTradeWnd.SetData(mCtrlSrv.GetPrototypeProvider(), mFormatedTipMgr, mOkDialogWnd);
		PopUpMenu popUpMenu = mPopupMenuWnd;
		popUpMenu.mOnMenuItem = (PopUpMenu.OnMenuItem)Delegate.Combine(popUpMenu.mOnMenuItem, new PopUpMenu.OnMenuItem(OnPopUpMenu));
		PopupEvent.Init(mPopupInfoWnd, battle.GetPrototypeProvider(), mCore.CtrlServer.GetPrototypeProvider(), null, mCore.CtrlServer.SelfHero.Inventory, null);
		EscapeDialog escapeDialog = mEscDialogWnd;
		escapeDialog.mExitCallback = (EscapeDialog.ExitCallback)Delegate.Combine(escapeDialog.mExitCallback, new EscapeDialog.ExitCallback(OnExit));
		mBankWnd.SetData(mCtrlSrv.SelfHero);
		Bank bank = mBankWnd;
		bank.mChangeMoneyCallback = (Bank.ChangeMoneyCallback)Delegate.Combine(bank.mChangeMoneyCallback, new Bank.ChangeMoneyCallback(ChangeMoneyRequest));
		AvatarBuyMenu avatarBuyMenu = mAvatarShopWnd;
		avatarBuyMenu.mBuy = (AvatarBuyMenu.Buy)Delegate.Combine(avatarBuyMenu.mBuy, new AvatarBuyMenu.Buy(Buy));
		mGroupWnd.SetData(mCtrlSrv.Group);
		mTutorialWnd.SetData(mOkDialogWnd, mYesNoDialogWnd, mQuestWnd, mCtrlSrv.Npcs, mCtrlSrv.SelfHero, mCtrlSrv.FightHelper);
		TutorialWindow tutorialWindow = mTutorialWnd;
		tutorialWindow.mOnStarted = (TutorialWindow.BattleStarted)Delegate.Combine(tutorialWindow.mOnStarted, new TutorialWindow.BattleStarted(mCore.BattleServer.WaitReconnect));
		TutorialWindow tutorialWindow2 = mTutorialWnd;
		tutorialWindow2.mTeleportRequest = (TutorialWindow.TeleportAction)Delegate.Combine(tutorialWindow2.mTeleportRequest, new TutorialWindow.TeleportAction(Teleport));
		TutorialWindow tutorialWindow3 = mTutorialWnd;
		tutorialWindow3.mChangeLocationRequest = (TutorialWindow.ChangeLocationAction)Delegate.Combine(tutorialWindow3.mChangeLocationRequest, new TutorialWindow.ChangeLocationAction(ChangeLocation));
		TutorialWindow tutorialWindow4 = mTutorialWnd;
		tutorialWindow4.mOnNpcUpdate = (TutorialWindow.NPCUpdateAction)Delegate.Combine(tutorialWindow4.mOnNpcUpdate, new TutorialWindow.NPCUpdateAction(UpdateQuestMarksNpc));
		TutorialWindow tutorialWindow5 = mTutorialWnd;
		tutorialWindow5.mOnShowPlayerList = (TutorialWindow.ShowListAction)Delegate.Combine(tutorialWindow5.mOnShowPlayerList, new TutorialWindow.ShowListAction(mStatsWnd.ShowPlayerList));
		mNpcWnd.SetData(mCtrlSrv.GetNpcProvider(), mCtrlSrv.GetQuestProvider(), mCtrlSrv.GetSelfQuestProvider());
		NPCMenu nPCMenu = mNpcWnd;
		nPCMenu.mShowQuestCallback = (NPCMenu.ShowQuestCallback)Delegate.Combine(nPCMenu.mShowQuestCallback, new NPCMenu.ShowQuestCallback(OnShowQuest));
		NPCMenu nPCMenu2 = mNpcWnd;
		nPCMenu2.mShowQuestJournalCallback = (NPCMenu.ShowQuestJournalCallback)Delegate.Combine(nPCMenu2.mShowQuestJournalCallback, new NPCMenu.ShowQuestJournalCallback(ShowQuestJournal));
		QuestMenu questMenu = mQuestWnd;
		questMenu.mOnQuestAccept = (QuestMenu.OnQuestCallback)Delegate.Combine(questMenu.mOnQuestAccept, new QuestMenu.OnQuestCallback(OnQuestAccept));
		QuestMenu questMenu2 = mQuestWnd;
		questMenu2.mOnQuestCancel = (QuestMenu.OnQuestCallback)Delegate.Combine(questMenu2.mOnQuestCancel, new QuestMenu.OnQuestCallback(OnQuestCancel));
		QuestMenu questMenu3 = mQuestWnd;
		questMenu3.mOnQuestDone = (QuestMenu.OnQuestCallback)Delegate.Combine(questMenu3.mOnQuestDone, new QuestMenu.OnQuestCallback(OnQuestDone));
		QuestMenu questMenu4 = mQuestWnd;
		questMenu4.mOnClose = (QuestMenu.OnCloseCallback)Delegate.Combine(questMenu4.mOnClose, new QuestMenu.OnCloseCallback(OnReopenNpcsMenu));
		mQuestJournalWnd.SetData(mCtrlSrv.GetSelfQuestProvider(), mCtrlSrv.SelfHero, mFormatedTipMgr);
		QuestJournal questJournal = mQuestJournalWnd;
		questJournal.mOnQuestCancel = (QuestJournal.OnQuestCallback)Delegate.Combine(questJournal.mOnQuestCancel, new QuestJournal.OnQuestCallback(OnQuestCancel));
		QuestJournal questJournal2 = mQuestJournalWnd;
		questJournal2.mOnActiveQuestChanged = (QuestJournal.VoidCallback)Delegate.Combine(questJournal2.mOnActiveQuestChanged, new QuestJournal.VoidCallback(OnActiveQuestChanged));
		mQuestProgressWnd.SetData(mCore.CtrlServer.GetSelfQuestProvider(), mCore.Battle.SelfPlayer.GetSelfPvpQuestProvider(), mCore.Battle.GetTimer());
		QuestProgressMenu questProgressMenu = mQuestProgressWnd;
		questProgressMenu.mOnShowQuestInfo = (QuestProgressMenu.ShowQuestInfoCallback)Delegate.Combine(questProgressMenu.mOnShowQuestInfo, new QuestProgressMenu.ShowQuestInfoCallback(ShowQuestInfo));
		mCtrlSrv.Npcs.SubscribeOnUpdated(OnNpcsUpdated);
		mCtrlSrv.SelfQuests.SubscribeOnUpdated(OnSelfQuestsUpdated);
		HandlerManager<CtrlPacket, Enum> handlerMgr = mCore.CtrlServer.EntryPoint.HandlerMgr;
		handlerMgr.Subscribe<ShopContentArg>(CtrlCmdId.store.list, null, null, OnShopContent);
		handlerMgr.Subscribe(CtrlCmdId.bank.change, OnMoneyChanged, OnMoneyChangeFailed);
		handlerMgr.Subscribe<FightLogArg>(CtrlCmdId.fight.log, null, null, OnFightLog);
		handlerMgr.Subscribe<ActionUseArg>(CtrlCmdId.common.action, null, null, OnActionDone);
		handlerMgr.Subscribe<ObserverArg>(CtrlCmdId.user.get_observer_info, null, null, OnObservRequest);
		handlerMgr.Subscribe<FullHeroInfoArg>(CtrlCmdId.user.full_hero_info, null, null, OnFullHeroInfo);
		handlerMgr.Subscribe<DeferredMessageArg>(CtrlCmdId.common.message_mpd, null, null, OnMessage);
		handlerMgr.Subscribe<HeroesInfoUpdateArg>(CtrlCmdId.user.update_info_mpd, null, null, OnUpdateHeroesInfo);
		handlerMgr.Subscribe<HeroDataListArg>(CtrlCmdId.hero.get_data_list, null, null, OnHeroDataList);
		handlerMgr.Subscribe<HeroDataListMpdArg>(CtrlCmdId.hero.get_data_list_mpd, null, null, OnHeroDataListBroadCast);
		handlerMgr.Subscribe<DayRewardMpdArg>(CtrlCmdId.common.day_reward_mpd, null, null, OnDayRewardMessage);
		mCtrlSrv.Group.Init(mGroupView, mCore.Battle.GetPlayerProvider());
		mCtrlSrv.Group.UpdateList();
		mTradeMgr = new TradeManager(mCtrlSrv.TradeSender, mCore.Battle.GetPlayerProvider());
		mTradeMgr.Subscribe(mCtrlSrv.EntryPoint.HandlerMgr, mTradeView);
		mTradeView.Init(mTradeMgr);
		ShowCentralSquareMenu();
		mShopWnd.SetData(mCtrlSrv.SelfHero, mFormatedTipMgr, mCtrlSrv.SelfHero);
		mShopRealWnd.SetData(mCtrlSrv.SelfHero, mFormatedTipMgr, mCtrlSrv.SelfHero);
		mFightView.Init(mCtrlSrv.FightHelper, mCore.Battle.SelfPlayer.PlayerId);
		mCtrlSrv.FightHelper.SubscribeLaunch(OnLaunchBattle);
		mCtrlSrv.FightHelper.BindGui(mFightView);
		mChatWnd.SetChatModes(new ChatChannel[5]
		{
			ChatChannel.area,
			ChatChannel.trade,
			ChatChannel.private_msg,
			ChatChannel.group,
			ChatChannel.clan
		});
		mCtrlSrv.HeroSender.GetBag();
		mCtrlSrv.SendGetMoney();
		mCtrlSrv.GetSelfBuffs();
		mCastleView = new CastleJoinView(mCtrlSrv.CastleHelper);
		mCastleView.SetData(mCastleMenu, mWorldMapMenu, mCtrlSrv.Clan, mOkDialogWnd, mCastleRequestMenu, mMapJoinQueueWnd, mScreenMgr, mSelAvaWnd, mCastleHistoryMenu);
		CastleJoinView castleJoinView = mCastleView;
		castleJoinView.mOnStarted = (CastleJoinView.Started)Delegate.Combine(castleJoinView.mOnStarted, new CastleJoinView.Started(mCore.BattleServer.WaitReconnect));
		CastleJoinView castleJoinView2 = mCastleView;
		castleJoinView2.mObserverCallback = (CastleHelper.LaunchCallback)Delegate.Combine(castleJoinView2.mObserverCallback, new CastleHelper.LaunchCallback(OnStartObserver));
		mCtrlSrv.CastleHelper.BindGui(mCastleView);
		mCtrlSrv.CastleHelper.SubscribeLaunch(OnLaunchBattle);
		if (mFightLogBattleId != -1)
		{
			if (mFightLogData != null)
			{
				ShowFightLog(mFightLogData);
				mFightLogData = null;
			}
			else if (mBattleType != 4)
			{
				mCtrlSrv.SendGetFightLog(mFightLogBattleId, mBattleType);
			}
			mFightLogBattleId = -1;
		}
		mOnShowNotifiers.Call(_success: true, this);
		mFirstTimeQuests = true;
		mQuestProgressWnd.UpdateInfo(new List<int>());
		mClanMenuCreate.SetData(mCtrlSrv.SelfHero, mOkDialogWnd, mCore.Battle.SelfPlayer.Player.Name);
		ClanMenuCreate clanMenuCreate = mClanMenuCreate;
		clanMenuCreate.mOnClanCreate = (ClanMenuCreate.OnClanCreate)Delegate.Combine(clanMenuCreate.mOnClanCreate, new ClanMenuCreate.OnClanCreate(OnClanCreate));
		mCtrlSrv.Clan.Init(mClanView, mCore.Battle.GetPlayerProvider());
		Clan clan = mCtrlSrv.Clan;
		clan.mOnClanCreated = (Clan.Action)Delegate.Combine(clan.mOnClanCreated, new Clan.Action(OnClanCreated));
		Clan clan2 = mCtrlSrv.Clan;
		clan2.mOnSelfRemoved = (Clan.Action)Delegate.Combine(clan2.mOnSelfRemoved, new Clan.Action(OnSelfRemoved));
		Clan clan3 = mCtrlSrv.Clan;
		clan3.mChangedCallback = (Action<Clan>)Delegate.Combine(clan3.mChangedCallback, new Action<Clan>(OnClanInfoChanged));
		Clan clan4 = mCtrlSrv.Clan;
		clan4.mClanInfoCallback = (Action<Clan.ClanInfo>)Delegate.Combine(clan4.mClanInfoCallback, new Action<Clan.ClanInfo>(OnClanInfo));
		Clan clan5 = mCtrlSrv.Clan;
		clan5.mOnClanRemoved = (Clan.Action)Delegate.Combine(clan5.mOnClanRemoved, new Clan.Action(OnClanremoved));
		mClanMenu.SetData(mCtrlSrv.Clan, mYesNoDialogWnd);
		ClanMenu clanMenu = mClanMenu;
		clanMenu.mOnPlayerInfo = (Action<int>)Delegate.Combine(clanMenu.mOnPlayerInfo, new Action<int>(ShowHeroInfo));
		WorldMapMenu worldMapMenu = mWorldMapMenu;
		worldMapMenu.mChangeLocation = (WorldMapMenu.ChangeLocation)Delegate.Combine(worldMapMenu.mChangeLocation, new WorldMapMenu.ChangeLocation(ChangeLocation));
		mWorldMapMenu.SetData(new WorldMapParser().GetMapElements(AssetLoader.ReadText("configs/world_map")), mYesNoDialogWnd, mOkDialogWnd, (Location)mCtrlSrv.mCurrentLocation);
		NPCSelector[] array = UnityEngine.Object.FindObjectsOfType(typeof(NPCSelector)) as NPCSelector[];
		if (array != null)
		{
			NPCSelector[] array2 = array;
			foreach (NPCSelector nPCSelector in array2)
			{
				nPCSelector.mSelectedCallback = (Action<int>)Delegate.Combine(nPCSelector.mSelectedCallback, new Action<int>(OnNpcClicked));
			}
		}
		PortalSelector[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(PortalSelector)) as PortalSelector[];
		if (mWorldMapMenu != null)
		{
			PortalSelector[] array4 = array3;
			foreach (PortalSelector portalSelector in array4)
			{
				portalSelector.mSelectedCallback = (Action<int>)Delegate.Combine(portalSelector.mSelectedCallback, new Action<int>(OnPortalClicked));
			}
		}
		BuildingSelector[] array5 = UnityEngine.Object.FindObjectsOfType(typeof(BuildingSelector)) as BuildingSelector[];
		BuildingSelector[] array6 = array5;
		foreach (BuildingSelector buildingSelector in array6)
		{
			mFunctionalBuildings[buildingSelector.mBuilding] = buildingSelector;
			buildingSelector.mSelectedCallback = (Action<int>)Delegate.Combine(buildingSelector.mSelectedCallback, new Action<int>(OnBuildingClicked));
		}
		mCtrlSrv.HeroSender.GameInfoRequest(mPlayerCtrl.SelfPlayer.PlayerId);
		mCastleMenu.SetData(mFormatedTipMgr, mCtrlSrv.GetPrototypeProvider(), mOkDialogWnd);
		mCastleRequestMenu.SetData(mYesNoDialogWnd, mOkDialogWnd);
		mRewardWnd.SetData(mCtrlSrv.GetPrototypeProvider(), mFormatedTipMgr);
		UpdateDynamicNpc();
		UpdateQuestMarksNpc();
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
		mCore.BattleServer.StopWaitReconnect();
		if (mCore.IsBattleCreated())
		{
			mCore.Battle.RemoveGameObjInitializer(this);
		}
		if (mCtrlSrv.Group != null)
		{
			mCtrlSrv.Group.Uninit();
		}
		mTradeView.Uninit();
		if (mTradeMgr != null)
		{
			mTradeMgr.Unsubscribe();
			mTradeMgr = null;
		}
		mYesNoDialogWnd.Clean();
		mFightView.Uninit();
		mCtrlSrv.FightHelper.UnbindGui();
		mCtrlSrv.FightHelper.UnsubscribeLaunch(OnLaunchBattle);
		mCtrlSrv.Npcs.Clear();
		mCtrlSrv.Npcs.UnsubscribeOnUpdated(OnNpcsUpdated);
		mCtrlSrv.SelfQuests.UnsubscribeOnUpdated(OnSelfQuestsUpdated);
		mCore.CtrlServer.EntryPoint.HandlerMgr.Unsubscribe(this);
		if (mPlayerCtrl != null)
		{
			mPlayerCtrl.Uninit();
			mPlayerCtrl = null;
		}
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Uninit();
		}
		mFightLogBattleId = -1;
		mSelfPlayerHistory = null;
		mFightLogData = null;
		mOnShowNotifiers.Clear();
		GuiSystem.mGuiInputMgr.Clean();
		VisualEffectOptions.Uninit();
		if (VisualEffectsMgr.Instance != null)
		{
			VisualEffectsMgr.Instance.Uninit();
		}
		Chat chat = mCore.CtrlServer.Chat;
		chat.mOnErrorCallback = (Chat.OnErrorCallback)Delegate.Remove(chat.mOnErrorCallback, new Chat.OnErrorCallback(OnChatError));
		CSMainMenu cSMainMenu = mMainCsWnd;
		cSMainMenu.mShowBattleMenuCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu.mShowBattleMenuCallback, new CSMainMenu.VoidCallback(ShowStartBattleMenu));
		CSMainMenu cSMainMenu2 = mMainCsWnd;
		cSMainMenu2.mShowHeroInfoCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu2.mShowHeroInfoCallback, new CSMainMenu.VoidCallback(ShowSelfHeroInfo));
		CSMainMenu cSMainMenu3 = mMainCsWnd;
		cSMainMenu3.mShowShopCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu3.mShowShopCallback, new CSMainMenu.VoidCallback(ShopContentRequest));
		CSMainMenu cSMainMenu4 = mMainCsWnd;
		cSMainMenu4.mShowShopRealCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu4.mShowShopRealCallback, new CSMainMenu.VoidCallback(ShopRealContentRequest));
		CSMainMenu cSMainMenu5 = mMainCsWnd;
		cSMainMenu5.mShowBagCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu5.mShowBagCallback, new CSMainMenu.VoidCallback(BagRequest));
		CSMainMenu cSMainMenu6 = mMainCsWnd;
		cSMainMenu6.mShowBankCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu6.mShowBankCallback, new CSMainMenu.VoidCallback(BankRequest));
		CSMainMenu cSMainMenu7 = mMainCsWnd;
		cSMainMenu7.mShowNPCCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu7.mShowNPCCallback, new CSMainMenu.VoidCallback(ShowNpcsMenu));
		CSMainMenu cSMainMenu8 = mMainCsWnd;
		cSMainMenu8.mOnPlayer = (CSMainMenu.OnPlayerCallback)Delegate.Remove(cSMainMenu8.mOnPlayer, new CSMainMenu.OnPlayerCallback(ShowPopUpMenu));
		CSMainMenu cSMainMenu9 = mMainCsWnd;
		cSMainMenu9.mShowClanCallback = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu9.mShowClanCallback, new CSMainMenu.VoidCallback(ShowClanMenu));
		CSMainMenu cSMainMenu10 = mMainCsWnd;
		cSMainMenu10.mOnShowWorldMap = (CSMainMenu.VoidCallback)Delegate.Remove(cSMainMenu10.mOnShowWorldMap, new CSMainMenu.VoidCallback(OnShowWorldMap));
		mMainCsWnd.Clean();
		mSelGameMenu.Clean();
		mMiniMapWnd.Clear();
		MiniMap miniMap = mMiniMapWnd;
		miniMap.mOnClick = (MiniMap.OnMiniMapClick)Delegate.Remove(miniMap.mOnClick, new MiniMap.OnMiniMapClick(OnMiniMapClick));
		MiniMap miniMap2 = mMiniMapWnd;
		miniMap2.mOnShowWorldMap = (MiniMap.OnShowWorldMap)Delegate.Remove(miniMap2.mOnShowWorldMap, new MiniMap.OnShowWorldMap(OnShowWorldMap));
		ChatWindow chatWindow = mChatWnd;
		chatWindow.mShowPopUpMenuCallback = (ChatWindow.ShowPopUpMenuCallback)Delegate.Remove(chatWindow.mShowPopUpMenuCallback, new ChatWindow.ShowPopUpMenuCallback(ShowPopUpMenu));
		mChatWnd.Uninit();
		mChatWnd.SetActive(_active: false);
		mListController.Unint();
		mStatsWnd.Uninit();
		CSStats cSStats = mStatsWnd;
		cSStats.mMenuCallback = (CSStats.MenuCallback)Delegate.Remove(cSStats.mMenuCallback, new CSStats.MenuCallback(ShowEscDialog));
		CSStats cSStats2 = mStatsWnd;
		cSStats2.mShowPopUpMenuCallback = (CSStats.ShowPopUpMenuCallback)Delegate.Remove(cSStats2.mShowPopUpMenuCallback, new CSStats.ShowPopUpMenuCallback(ShowPopUpMenu));
		CSStats cSStats3 = mStatsWnd;
		cSStats3.mShowClanInfoCallback = (Action<string>)Delegate.Remove(cSStats3.mShowClanInfoCallback, new Action<string>(OnClanInfoRequest));
		HeroInfo heroInfo = mHeroInfoWnd;
		heroInfo.mDressCallback = (HeroInfo.DressCallback)Delegate.Remove(heroInfo.mDressCallback, new HeroInfo.DressCallback(Dress));
		HeroInfo heroInfo2 = mHeroInfoWnd;
		heroInfo2.mUndressCallback = (HeroInfo.UndressCallback)Delegate.Remove(heroInfo2.mUndressCallback, new HeroInfo.UndressCallback(Undress));
		HeroStore heroes = mCtrlSrv.Heroes;
		heroes.mNewItemDressedCallback = (HeroStore.NewItemDressedCallback)Delegate.Remove(heroes.mNewItemDressedCallback, new HeroStore.NewItemDressedCallback(OnNewItemDressed));
		HeroStore heroes2 = mCtrlSrv.Heroes;
		heroes2.mItemUndressedCallback = (HeroStore.ItemUndressedCallback)Delegate.Remove(heroes2.mItemUndressedCallback, new HeroStore.ItemUndressedCallback(OnItemUndressed));
		HeroInfo heroInfo3 = mHeroInfoWnd;
		heroInfo3.mClanInfoCallback = (Action<string>)Delegate.Remove(heroInfo3.mClanInfoCallback, new Action<string>(OnClanInfoRequest));
		mShopWnd.Uninit();
		mShopWnd.Clear();
		ShopGUI shopGUI = mShopWnd;
		shopGUI.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Remove(shopGUI.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		ShopGUI shopGUI2 = mShopWnd;
		shopGUI2.mSellRequestCallback = (ShopGUI.SellRequestCallback)Delegate.Remove(shopGUI2.mSellRequestCallback, new ShopGUI.SellRequestCallback(Sell));
		ShopGUI shopGUI3 = mShopWnd;
		shopGUI3.mOnUpdateInventory = (ShopGUI.OnVoidAction)Delegate.Remove(shopGUI3.mOnUpdateInventory, new ShopGUI.OnVoidAction(UpdateInventoryRequest));
		ShopGUI shopGUI4 = mShopWnd;
		shopGUI4.mOnShowBank = (ShopGUI.OnVoidAction)Delegate.Remove(shopGUI4.mOnShowBank, new ShopGUI.OnVoidAction(BankRequest));
		mShopRealWnd.Uninit();
		mShopRealWnd.Clear();
		ShopGUI shopGUI5 = mShopRealWnd;
		shopGUI5.mBuyRequestCallback = (ShopGUI.BuyRequestCallback)Delegate.Remove(shopGUI5.mBuyRequestCallback, new ShopGUI.BuyRequestCallback(Buy));
		ShopGUI shopGUI6 = mShopRealWnd;
		shopGUI6.mOnShowBank = (ShopGUI.OnVoidAction)Delegate.Remove(shopGUI6.mOnShowBank, new ShopGUI.OnVoidAction(BankRequest));
		mInventoryWnd.Clean();
		InventoryMenu inventoryMenu = mInventoryWnd;
		inventoryMenu.mUseCtrlItemCallback = (InventoryMenu.UseCtrlItemCallback)Delegate.Remove(inventoryMenu.mUseCtrlItemCallback, new InventoryMenu.UseCtrlItemCallback(UseCtrlItem));
		SelfHero selfHero = mCtrlSrv.SelfHero;
		selfHero.mInventoryChangedCallback = (SelfHero.InventoryChangedCallback)Delegate.Remove(selfHero.mInventoryChangedCallback, new SelfHero.InventoryChangedCallback(OnInventoryChanged));
		mTradeWnd.Clean();
		mYesNoDialogWnd.Clean();
		mOkDialogWnd.Clean();
		PopUpMenu popUpMenu = mPopupMenuWnd;
		popUpMenu.mOnMenuItem = (PopUpMenu.OnMenuItem)Delegate.Remove(popUpMenu.mOnMenuItem, new PopUpMenu.OnMenuItem(OnPopUpMenu));
		PopupEvent.Destroy();
		EscapeDialog escapeDialog = mEscDialogWnd;
		escapeDialog.mExitCallback = (EscapeDialog.ExitCallback)Delegate.Remove(escapeDialog.mExitCallback, new EscapeDialog.ExitCallback(OnExit));
		mEscDialogWnd.Hide();
		mBankWnd.Clean();
		Bank bank = mBankWnd;
		bank.mChangeMoneyCallback = (Bank.ChangeMoneyCallback)Delegate.Remove(bank.mChangeMoneyCallback, new Bank.ChangeMoneyCallback(ChangeMoneyRequest));
		mAvatarShopWnd.Clean();
		AvatarBuyMenu avatarBuyMenu = mAvatarShopWnd;
		avatarBuyMenu.mBuy = (AvatarBuyMenu.Buy)Delegate.Remove(avatarBuyMenu.mBuy, new AvatarBuyMenu.Buy(Buy));
		mCompensationWnd.Close();
		mGroupWnd.Clean();
		mSelGameMenu.SetActive(_active: false);
		mMapJoinQueueWnd.Clean();
		mNpcWnd.Clean();
		NPCMenu nPCMenu = mNpcWnd;
		nPCMenu.mShowQuestCallback = (NPCMenu.ShowQuestCallback)Delegate.Remove(nPCMenu.mShowQuestCallback, new NPCMenu.ShowQuestCallback(OnShowQuest));
		NPCMenu nPCMenu2 = mNpcWnd;
		nPCMenu2.mShowQuestJournalCallback = (NPCMenu.ShowQuestJournalCallback)Delegate.Remove(nPCMenu2.mShowQuestJournalCallback, new NPCMenu.ShowQuestJournalCallback(ShowQuestJournal));
		QuestMenu questMenu = mQuestWnd;
		questMenu.mOnQuestAccept = (QuestMenu.OnQuestCallback)Delegate.Remove(questMenu.mOnQuestAccept, new QuestMenu.OnQuestCallback(OnQuestAccept));
		QuestMenu questMenu2 = mQuestWnd;
		questMenu2.mOnQuestCancel = (QuestMenu.OnQuestCallback)Delegate.Remove(questMenu2.mOnQuestCancel, new QuestMenu.OnQuestCallback(OnQuestCancel));
		QuestMenu questMenu3 = mQuestWnd;
		questMenu3.mOnQuestDone = (QuestMenu.OnQuestCallback)Delegate.Remove(questMenu3.mOnQuestDone, new QuestMenu.OnQuestCallback(OnQuestDone));
		QuestMenu questMenu4 = mQuestWnd;
		questMenu4.mOnClose = (QuestMenu.OnCloseCallback)Delegate.Remove(questMenu4.mOnClose, new QuestMenu.OnCloseCallback(OnReopenNpcsMenu));
		mQuestJournalWnd.Clean();
		QuestJournal questJournal = mQuestJournalWnd;
		questJournal.mOnQuestCancel = (QuestJournal.OnQuestCallback)Delegate.Remove(questJournal.mOnQuestCancel, new QuestJournal.OnQuestCallback(OnQuestCancel));
		QuestJournal questJournal2 = mQuestJournalWnd;
		questJournal2.mOnActiveQuestChanged = (QuestJournal.VoidCallback)Delegate.Remove(questJournal2.mOnActiveQuestChanged, new QuestJournal.VoidCallback(OnActiveQuestChanged));
		QuestProgressMenu questProgressMenu = mQuestProgressWnd;
		questProgressMenu.mOnShowQuestInfo = (QuestProgressMenu.ShowQuestInfoCallback)Delegate.Remove(questProgressMenu.mOnShowQuestInfo, new QuestProgressMenu.ShowQuestInfoCallback(ShowQuestInfo));
		mTutorial.UnInit();
		mClanMenu.Uninit();
		ClanMenu clanMenu = mClanMenu;
		clanMenu.mOnPlayerInfo = (Action<int>)Delegate.Remove(clanMenu.mOnPlayerInfo, new Action<int>(ShowHeroInfo));
		ClanMenuCreate clanMenuCreate = mClanMenuCreate;
		clanMenuCreate.mOnClanCreate = (ClanMenuCreate.OnClanCreate)Delegate.Remove(clanMenuCreate.mOnClanCreate, new ClanMenuCreate.OnClanCreate(OnClanCreate));
		mClanMenuCreate.Uninit();
		mCtrlSrv.Clan.Uninit();
		Clan clan = mCtrlSrv.Clan;
		clan.mOnClanCreated = (Clan.Action)Delegate.Remove(clan.mOnClanCreated, new Clan.Action(OnClanCreated));
		Clan clan2 = mCtrlSrv.Clan;
		clan2.mOnSelfRemoved = (Clan.Action)Delegate.Remove(clan2.mOnSelfRemoved, new Clan.Action(OnSelfRemoved));
		Clan clan3 = mCtrlSrv.Clan;
		clan3.mChangedCallback = (Action<Clan>)Delegate.Remove(clan3.mChangedCallback, new Action<Clan>(OnClanInfoChanged));
		Clan clan4 = mCtrlSrv.Clan;
		clan4.mClanInfoCallback = (Action<Clan.ClanInfo>)Delegate.Remove(clan4.mClanInfoCallback, new Action<Clan.ClanInfo>(OnClanInfo));
		Clan clan5 = mCtrlSrv.Clan;
		clan5.mOnClanRemoved = (Clan.Action)Delegate.Remove(clan5.mOnClanRemoved, new Clan.Action(OnClanremoved));
		mWorldMapMenu.Uninit();
		WorldMapMenu worldMapMenu = mWorldMapMenu;
		worldMapMenu.mChangeLocation = (WorldMapMenu.ChangeLocation)Delegate.Remove(worldMapMenu.mChangeLocation, new WorldMapMenu.ChangeLocation(ChangeLocation));
		SelectAvatarWindow selectAvatarWindow = mSelAvaWnd;
		selectAvatarWindow.mOnSelectLockedAvatar = (SelectAvatarWindow.OnSelectLockedAvatar)Delegate.Remove(selectAvatarWindow.mOnSelectLockedAvatar, new SelectAvatarWindow.OnSelectLockedAvatar(ShowAvatarShop));
		SelectAvatarWindow selectAvatarWindow2 = mSelAvaWnd;
		selectAvatarWindow2.mDesertCallback = (SelectAvatarWindow.DesertCallback)Delegate.Remove(selectAvatarWindow2.mDesertCallback, new SelectAvatarWindow.DesertCallback(mCtrlSrv.StopWaitReconnect));
		mSelAvaWnd.Clear();
		NPCSelector[] array = UnityEngine.Object.FindObjectsOfType(typeof(NPCSelector)) as NPCSelector[];
		if (array != null)
		{
			NPCSelector[] array2 = array;
			foreach (NPCSelector nPCSelector in array2)
			{
				nPCSelector.mSelectedCallback = (Action<int>)Delegate.Remove(nPCSelector.mSelectedCallback, new Action<int>(OnNpcClicked));
			}
		}
		PortalSelector[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(PortalSelector)) as PortalSelector[];
		if (array != null)
		{
			PortalSelector[] array4 = array3;
			foreach (PortalSelector portalSelector in array4)
			{
				portalSelector.mSelectedCallback = (Action<int>)Delegate.Remove(portalSelector.mSelectedCallback, new Action<int>(OnPortalClicked));
			}
		}
		BuildingSelector[] array5 = UnityEngine.Object.FindObjectsOfType(typeof(BuildingSelector)) as BuildingSelector[];
		BuildingSelector[] array6 = array5;
		foreach (BuildingSelector buildingSelector in array6)
		{
			buildingSelector.mSelectedCallback = (Action<int>)Delegate.Remove(buildingSelector.mSelectedCallback, new Action<int>(OnBuildingClicked));
		}
		mCtrlSrv.CastleHelper.UnsubscribeLaunch(OnLaunchBattle);
		TutorialWindow tutorialWindow = mTutorialWnd;
		tutorialWindow.mOnStarted = (TutorialWindow.BattleStarted)Delegate.Remove(tutorialWindow.mOnStarted, new TutorialWindow.BattleStarted(mCore.BattleServer.WaitReconnect));
		TutorialWindow tutorialWindow2 = mTutorialWnd;
		tutorialWindow2.mTeleportRequest = (TutorialWindow.TeleportAction)Delegate.Remove(tutorialWindow2.mTeleportRequest, new TutorialWindow.TeleportAction(Teleport));
		TutorialWindow tutorialWindow3 = mTutorialWnd;
		tutorialWindow3.mChangeLocationRequest = (TutorialWindow.ChangeLocationAction)Delegate.Remove(tutorialWindow3.mChangeLocationRequest, new TutorialWindow.ChangeLocationAction(ChangeLocation));
		TutorialWindow tutorialWindow4 = mTutorialWnd;
		tutorialWindow4.mOnNpcUpdate = (TutorialWindow.NPCUpdateAction)Delegate.Remove(tutorialWindow4.mOnNpcUpdate, new TutorialWindow.NPCUpdateAction(UpdateQuestMarksNpc));
		TutorialWindow tutorialWindow5 = mTutorialWnd;
		tutorialWindow5.mOnShowPlayerList = (TutorialWindow.ShowListAction)Delegate.Remove(tutorialWindow5.mOnShowPlayerList, new TutorialWindow.ShowListAction(mStatsWnd.ShowPlayerList));
		mCtrlSrv.CastleHelper.UnbindGui();
		CastleJoinView castleJoinView = mCastleView;
		castleJoinView.mOnStarted = (CastleJoinView.Started)Delegate.Remove(castleJoinView.mOnStarted, new CastleJoinView.Started(mCore.BattleServer.WaitReconnect));
		CastleJoinView castleJoinView2 = mCastleView;
		castleJoinView2.mObserverCallback = (CastleHelper.LaunchCallback)Delegate.Remove(castleJoinView2.mObserverCallback, new CastleHelper.LaunchCallback(OnStartObserver));
		mCastleMenu.Uninit();
		mCastleRequestMenu.Uninit();
		mCastleView.Uninit();
		mCastleHistoryMenu.Uninit();
		mCtrlSrv.Heroes.Clear();
	}

	public void InitGameObject(IGameObject _obj)
	{
		GameData gameData = _obj as GameData;
		gameData.InitMapSymbol();
		if (gameData.Proto.Avatar != null)
		{
			gameData.gameObject.AddComponent<EffectEmiter>();
		}
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

	private void OnExit()
	{
		mApp.Exit();
	}

	public void ShowCentralSquareMenu()
	{
		mScreenMgr.Gui.SetCurGuiSet("central_square");
		mCore.BattleServer.StopWaitReconnect();
	}

	private void OnMiniMapClick(int _btnNum, Vector2 _pos)
	{
		if (_btnNum == 1)
		{
			mPlayerCtrl.SelfPlayer.Move(_pos.x, _pos.y, _rel: false);
		}
	}

	private void OnShowWorldMap()
	{
		if (!mWorldMapMenu.Active)
		{
			mCtrlSrv.CastleHelper.CastleListRequest();
			mWorldMapMenu.Open();
		}
		else
		{
			mWorldMapMenu.Close();
		}
	}

	private void OnChatError(Chat.Error _errorType)
	{
		if (_errorType == Chat.Error.GAG)
		{
			mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_CHAT_ERROR_GAG"));
		}
	}

	private void OnPortalClicked(int _location)
	{
		if (mTutorial.IsPortalEnabled(_location))
		{
			mWorldMapMenu.TryChangeLocation(_location);
		}
	}

	private void OnBuildingClicked(int _buildingId)
	{
		if (mTutorial.IsBuildingEnabled(_buildingId))
		{
			UserLog.AddAction(UserActionType.BUILDING_CLICK, _buildingId, GuiSystem.GetLocaleText("LOG_BUILDING_" + (Building)_buildingId));
			mTutorial.OnBuildingClicked(_buildingId);
			if (mFunctionalBuildings.ContainsKey((Building)_buildingId))
			{
				BuildingSelector buildingSelector = mFunctionalBuildings[(Building)_buildingId];
				Vector2 targetPoint = buildingSelector.GetTargetPoint();
				mCore.BattleServer.SendMovePlayer(targetPoint.x, targetPoint.y, _rel: false);
			}
			switch (_buildingId)
			{
			case 3001:
				BankRequest();
				break;
			case 3003:
				ShowStartBattleMenu();
				break;
			case 3005:
				ShowClanMenu();
				break;
			case 3004:
				ShopRealContentRequest();
				break;
			case 3002:
				ShopContentRequest();
				break;
			}
		}
	}

	private void Teleport(int _location, int _x, int _y)
	{
		if (mCtrlSrv.mCurrentLocation == _location)
		{
			mCore.Battle.SelfPlayer.Teleport(_x, _y);
		}
	}

	private void ChangeLocation(int _location)
	{
		if (mCtrlSrv.mCurrentLocation != _location)
		{
			mScreenMgr.Gui.SetCurGuiSet("loading_screen");
			mCtrlSrv.SendReconnectRequest(_location);
		}
	}

	private void OnNpcClicked(int _id)
	{
		bool flag = false;
		foreach (INpc item in mCtrlSrv.GetNpcProvider().Content)
		{
			if (_id == item.Id)
			{
				UserLog.AddAction(UserActionType.NPC_CLICK, _id, GuiSystem.GetLocaleText(item.Name));
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (!mTutorial.OnNpcClicked(_id))
			{
				mNpcWnd.mSelectedNPC = _id;
				mNpcWnd.SetActive(_active: true);
			}
		}
		else
		{
			UserLog.AddAction(UserActionType.NPC_CLICK, _id, GuiSystem.GetLocaleText("LOG_WRONG_NPC"));
			mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_ANOTHER_NPC"));
		}
	}

	private void ShowNpcsMenu()
	{
		mNpcWnd.SetActive(!mNpcWnd.Active);
	}

	private void ShowClanMenu()
	{
		if (mCtrlSrv.Clan.IsCreated)
		{
			if (!mClanMenu.Active)
			{
				mCtrlSrv.Clan.RefreshInfo();
			}
			else
			{
				mClanMenu.Close();
			}
		}
		else if (!mClanMenuCreate.Active)
		{
			mClanMenuCreate.Open();
		}
		else
		{
			mClanMenuCreate.Close();
		}
	}

	private void OnShowQuest(int _npcId, int _questId)
	{
		INpc npc = mCtrlSrv.Npcs.Get(_npcId);
		if (npc == null)
		{
			Log.Warning("cannot find npc");
			return;
		}
		IQuest quest = mCtrlSrv.Quests.Get(_questId);
		if (quest == null)
		{
			Log.Warning("cannot find quest");
			return;
		}
		ISelfQuest selfQuest = mCtrlSrv.SelfQuests.TryGet(_questId);
		mQuestWnd.SetData(npc, quest, selfQuest, mCtrlSrv.SelfHero, mFormatedTipMgr);
		mQuestWnd.SetActive(_active: true);
	}

	private void OnQuestAccept(int _questId)
	{
		mCtrlSrv.SelfQuests.AcceptQuest(_questId);
	}

	private void OnQuestCancel(int _questId)
	{
		mCtrlSrv.SelfQuests.Cancel(_questId);
	}

	private void OnQuestDone(int _questId)
	{
		mCtrlSrv.SelfQuests.Done(_questId);
	}

	private void OnReopenNpcsMenu()
	{
		mNpcWnd.SetActive(_active: true);
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
		ShowQuestJournal(_questId);
	}

	private void OnNpcsUpdated(NpcStore _npcs)
	{
		if (mNpcWnd.Active)
		{
			mNpcWnd.SetActive(_active: true);
		}
		UpdateDynamicNpc();
		UpdateQuestMarksNpc();
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
		}
		else
		{
			foreach (int item in _added)
			{
				OptionsMgr.SetActiveQuest(item, _show: true);
			}
			List<int> list = new List<int>();
			list.AddRange(_added);
			list.AddRange(_updated);
			mQuestProgressWnd.UpdateInfo(list);
		}
		UpdateQuestMarksNpc();
	}

	private void UpdateQuestMarksNpc()
	{
		Dictionary<int, QuestStatus> dictionary = new Dictionary<int, QuestStatus>();
		IStoreContentProvider<INpc> npcProvider = mCtrlSrv.GetNpcProvider();
		IStoreContentProvider<ISelfQuest> selfQuestProvider = mCtrlSrv.GetSelfQuestProvider();
		foreach (INpc item in npcProvider.Content)
		{
			dictionary[item.Id] = NPCMenu.GetNPCQuestStatus(item, selfQuestProvider);
		}
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(QuestMark));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			QuestMark questMark = @object as QuestMark;
			if (questMark != null)
			{
				questMark.SetData(dictionary);
			}
		}
	}

	private void UpdateDynamicNpc()
	{
		IStoreContentProvider<INpc> npcProvider = mCtrlSrv.GetNpcProvider();
		NPCSelector[] array = UnityEngine.Object.FindObjectsOfType(typeof(NPCSelector)) as NPCSelector[];
		if (array == null)
		{
			return;
		}
		NPCSelector[] array2 = array;
		foreach (NPCSelector nPCSelector in array2)
		{
			INpc npc = npcProvider.TryGet(nPCSelector.mNPC);
			if (npc != null)
			{
				nPCSelector.mRaceObject.SetActiveRecursively(npc.NeedShow);
			}
		}
	}

	private void OnActiveQuestChanged()
	{
		mQuestProgressWnd.UpdateInfo(new List<int>());
	}

	public void ShowPopUpMenu(object _player, Vector2 _pos)
	{
		Player player = null;
		int num = -1;
		ShortUserInfo shortUserInfo = null;
		IStoreContentProvider<Player> playerProvider = mCore.Battle.GetPlayerProvider();
		if (_player is Player)
		{
			player = (Player)_player;
		}
		else if (_player is string)
		{
			string text = (string)_player;
			foreach (Player item in playerProvider.Content)
			{
				if (item.Name == text)
				{
					player = item;
					break;
				}
			}
		}
		else if (_player is int)
		{
			num = (int)_player;
			player = playerProvider.TryGet(num);
		}
		else if (_player is ShortUserInfo)
		{
			shortUserInfo = (ShortUserInfo)_player;
			num = shortUserInfo.mId;
			player = playerProvider.TryGet(num);
		}
		List<PopUpMenu.PopUpMenuItem> list = new List<PopUpMenu.PopUpMenuItem>();
		list.Add(PopUpMenu.PopUpMenuItem.HERO_INFO_ITEM);
		bool flag = false;
		if (player != null)
		{
			flag = player.IsSelf;
			num = player.Id;
		}
		if (num == -1)
		{
			return;
		}
		if (flag)
		{
			if (!mCtrlSrv.Group.IsEmpty)
			{
				list.Add(PopUpMenu.PopUpMenuItem.GROUP_LEAVE);
			}
		}
		else
		{
			if (player != null)
			{
				list.Add(PopUpMenu.PopUpMenuItem.PRIVATE_MSG_ITEM);
				list.Add(PopUpMenu.PopUpMenuItem.TRADE_HERO_ITEM);
				if (mCtrlSrv.Group.IsEmpty)
				{
					list.Add(PopUpMenu.PopUpMenuItem.GROUP_JOIN_REQUEST);
				}
			}
			if (player == null && shortUserInfo != null && shortUserInfo.mOnline != 0)
			{
				list.Add(PopUpMenu.PopUpMenuItem.PRIVATE_MSG_ITEM);
				if (mCtrlSrv.Group.IsEmpty)
				{
					list.Add(PopUpMenu.PopUpMenuItem.GROUP_JOIN_REQUEST);
				}
			}
			if (mCtrlSrv.Group.IsLeader)
			{
				if (mCtrlSrv.Group.Contains(num))
				{
					list.Add(PopUpMenu.PopUpMenuItem.GROUP_REMOVE);
					if (player != null)
					{
						list.Add(PopUpMenu.PopUpMenuItem.GROUP_CHANGE_LEADER);
					}
					else if (shortUserInfo != null && shortUserInfo.mOnline == ShortUserInfo.Status.CS)
					{
						list.Add(PopUpMenu.PopUpMenuItem.GROUP_CHANGE_LEADER);
					}
				}
				else if (player != null)
				{
					list.Add(PopUpMenu.PopUpMenuItem.GROUP_INVITE);
				}
				else if (shortUserInfo != null && shortUserInfo.mOnline == ShortUserInfo.Status.CS)
				{
					list.Add(PopUpMenu.PopUpMenuItem.GROUP_INVITE);
				}
			}
			if (!flag)
			{
				if (PlayersListController.IsFriend(num))
				{
					list.Add(PopUpMenu.PopUpMenuItem.REMOVE_FRIEND);
				}
				else
				{
					list.Add(PopUpMenu.PopUpMenuItem.ADD_FRIEND);
				}
				if (PlayersListController.IsIgnore(num))
				{
					list.Add(PopUpMenu.PopUpMenuItem.REMOVE_IGNORE);
				}
				else
				{
					list.Add(PopUpMenu.PopUpMenuItem.ADD_IGNORE);
				}
			}
		}
		object data = ((player != null) ? player : ((shortUserInfo == null) ? ((object)num) : shortUserInfo));
		mPopupMenuWnd.SetMenuItems(list.ToArray(), data);
		mPopupMenuWnd.ShowMenu(_pos);
	}

	private void OnPopUpMenu(PopUpMenu.PopUpMenuItem _itemId, object _data)
	{
		Player player = null;
		int num = -1;
		string text = string.Empty;
		ShortUserInfo shortUserInfo = null;
		if (_data is Player)
		{
			player = (Player)_data;
			if (player == null)
			{
				return;
			}
			num = player.Id;
			text = player.Name;
		}
		else if (_data is int)
		{
			num = (int)_data;
			IStoreContentProvider<Player> playerProvider = mCore.Battle.GetPlayerProvider();
			player = playerProvider.TryGet(num);
			if (player != null)
			{
				text = player.Name;
			}
		}
		else if (_data is ShortUserInfo)
		{
			shortUserInfo = (ShortUserInfo)_data;
			num = shortUserInfo.mId;
			text = shortUserInfo.mName;
		}
		switch (_itemId)
		{
		case PopUpMenu.PopUpMenuItem.HERO_INFO_ITEM:
			UserLog.AddAction(UserActionType.HERO_INFO, num, text);
			ShowHeroInfo(num);
			break;
		case PopUpMenu.PopUpMenuItem.TRADE_HERO_ITEM:
			mTradeView.OpenSession(num);
			break;
		case PopUpMenu.PopUpMenuItem.PRIVATE_MSG_ITEM:
			if (!string.IsNullOrEmpty(text))
			{
				mChatWnd.AddRecipientToMsg(text);
			}
			break;
		case PopUpMenu.PopUpMenuItem.GROUP_INVITE:
			mCtrlSrv.Group.Invite(num);
			break;
		case PopUpMenu.PopUpMenuItem.GROUP_JOIN_REQUEST:
			mCtrlSrv.Group.JoinRequest(num);
			break;
		case PopUpMenu.PopUpMenuItem.GROUP_LEAVE:
			mCtrlSrv.Group.Leave();
			break;
		case PopUpMenu.PopUpMenuItem.GROUP_REMOVE:
			mCtrlSrv.Group.Remove(num);
			break;
		case PopUpMenu.PopUpMenuItem.GROUP_CHANGE_LEADER:
			mCtrlSrv.Group.ChangeLeader(num);
			break;
		case PopUpMenu.PopUpMenuItem.ADD_FRIEND:
			if (shortUserInfo != null)
			{
				mListController.AddToFriend(shortUserInfo);
			}
			else
			{
				mListController.AddOnlineToFriend(num);
			}
			break;
		case PopUpMenu.PopUpMenuItem.ADD_IGNORE:
			mListController.AddToIgnore(num, text);
			break;
		case PopUpMenu.PopUpMenuItem.REMOVE_FRIEND:
			mListController.RemoveFromFriends(num);
			break;
		case PopUpMenu.PopUpMenuItem.REMOVE_IGNORE:
			mListController.RemoveFromIgnore(num);
			break;
		}
	}

	private void OnObservRequest(ObserverArg _arg)
	{
		if (_arg.mServerData != null)
		{
			BattleServerData battleServerData = new BattleServerData();
			battleServerData.mHost = _arg.mServerData.mHost;
			battleServerData.mPorts = _arg.mServerData.mPorts;
			BattleServerData.DebugJoinData debugJoinData = (battleServerData.mDebugJoin = new BattleServerData.DebugJoinData());
			debugJoinData.mAvatarPrefab = string.Empty;
			debugJoinData.mBattleId = _arg.mServerData.mBattleId;
			debugJoinData.mMapId = 0;
			debugJoinData.mTeam = 0;
			debugJoinData.mAvatarParams = new MixedArray();
			Log.Debug("Observer Host:" + _arg.mServerData.mHost);
			BattleMapData battleMapData = new BattleMapData();
			battleMapData.mMapName = _arg.mServerData.mMap;
			OnStartObserver(battleServerData, battleMapData);
		}
	}

	private void ShowStartBattleMenu()
	{
		if (mSelGameMenu.IsInited())
		{
			mSelGameMenu.SetMaps();
			mSelGameMenu.SetActive(_active: true);
		}
	}

	private void OnLaunchBattle(BattleServerData _battleSrvData, BattleMapData _mapData)
	{
		Log.Debug("Launch battle");
		mCore.BattleServer.Connect(_battleSrvData, _mapData);
		Log.Debug("Hide CS screen");
		mScreenMgr.Holder.HideCurScreen();
	}

	private void OnStartObserver(BattleServerData _battleSrvData, BattleMapData _mapData)
	{
		Log.Debug("Launch observer");
		mCore.BattleServer.mIsObserver = true;
		mCore.BattleServer.Connect(_battleSrvData, _mapData);
		Log.Debug("Hide CS screen");
		mScreenMgr.Holder.HideCurScreen();
	}

	private void OnDropItem(int _itemOrigin, int _itemId, int _count)
	{
		if (_itemOrigin == 2)
		{
			mCore.CtrlServer.HeroSender.DropItem(_itemId, _count);
			mCore.CtrlServer.HeroSender.GetBag();
		}
	}

	private void UseCtrlItem(int _itemId)
	{
		CtrlThing ctrlThing = mCtrlSrv.SelfHero.Inventory.Get(_itemId);
		if (ctrlThing != null)
		{
			if (ctrlThing.CtrlProto.Article.mMinHeroLvl > mCtrlSrv.SelfHero.Hero.GameInfo.mLevel)
			{
				mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_INVENTORY_WRONG_LEVEL"));
				return;
			}
			UserLog.AddAction(UserActionType.USE_THING, _itemId, GuiSystem.GetLocaleText(ctrlThing.CtrlProto.Desc.mName));
			mCtrlSrv.HeroSender.DoAction(ctrlThing, _check: true);
		}
	}

	private void OnInventoryChanged(IStoreContentProvider<CtrlThing> _inventory)
	{
		List<Thing> list = new List<Thing>();
		foreach (CtrlThing item in _inventory.Content)
		{
			list.Add(item);
		}
		mInventoryWnd.SetItems(list);
	}

	private void BagRequest()
	{
		if (mInventoryWnd.Active)
		{
			mInventoryWnd.Close();
		}
		else
		{
			mInventoryWnd.Open();
		}
	}

	private void ShowAvatarShop(AvatarData _data, MapAvatarData _mapData)
	{
		mAvatarShopWnd.SetData(_data, _mapData, mCtrlSrv.SelfHero, mCtrlSrv.GetPrototypeProvider());
		mAvatarShopWnd.SetActive(_active: true);
		mShopWnd.Close();
		mShopRealWnd.Close();
	}

	private void ShopContentRequest()
	{
		mShopTypeRequested = false;
		if (!mShopWnd.Active)
		{
			mCtrlSrv.ShopSender.ContentRequest(ShopType.SIMPLE);
		}
		else
		{
			mShopWnd.Close();
		}
	}

	private void ShopRealContentRequest()
	{
		mShopTypeRequested = true;
		if (!mShopRealWnd.Active)
		{
			mCtrlSrv.ShopSender.ContentRequest(ShopType.REAL);
		}
		else
		{
			mShopRealWnd.Close();
		}
	}

	private void ShopAvatarContentRequest()
	{
		mShopTypeRequested = true;
		if (!mShopRealWnd.Active)
		{
			mCtrlSrv.ShopSender.ContentRequest(ShopType.AVATAR);
		}
		else
		{
			mShopRealWnd.Close();
		}
	}

	private void OnShopContent(ShopContentArg _arg)
	{
		IStoreContentProvider<CtrlPrototype> prototypeProvider = mCtrlSrv.GetPrototypeProvider();
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
		if (mShopTypeRequested)
		{
			if (mShopWnd.Active)
			{
				mShopWnd.Close();
			}
			mShopRealWnd.SetItems(list);
			mShopRealWnd.Open();
		}
		else
		{
			if (mShopRealWnd.Active)
			{
				mShopRealWnd.Close();
			}
			mShopWnd.SetItems(list);
			mShopWnd.Open();
		}
		BagRequest();
	}

	private void Buy(Dictionary<int, int> _basket)
	{
		Notifier<SelfHero, object> notifier = new Notifier<SelfHero, object>();
		notifier.mCallback = OnBought;
		mCtrlSrv.SelfHero.Buy(_basket, notifier);
	}

	private void OnBought(bool _success, SelfHero _selfHero, object _data)
	{
		if (mSelAvaWnd.Active)
		{
			mSelAvaWnd.BuyComplete(_success);
		}
		mCtrlSrv.HeroSender.GetBag();
		mCtrlSrv.SendGetMoney();
	}

	private void Sell(Dictionary<int, int> _basket)
	{
		Notifier<SelfHero, object> notifier = new Notifier<SelfHero, object>();
		notifier.mCallback = OnSold;
		mCtrlSrv.SelfHero.Sell(_basket, notifier);
	}

	private void OnSold(bool _success, SelfHero _selfHero, object _data)
	{
		mCtrlSrv.HeroSender.GetBag();
	}

	private void UpdateInventoryRequest()
	{
		mCtrlSrv.HeroSender.GetBag();
	}

	private void ShowSelfHeroInfo()
	{
		if (mHeroInfoWnd.Active)
		{
			mHeroInfoWnd.Close();
		}
		else
		{
			ShowHeroInfo(mCore.Battle.SelfPlayer.PlayerId);
		}
	}

	private void ShowHeroInfo(int _playerId)
	{
		mCtrlSrv.UsersListSender.GetFullHeroInfo(_playerId);
	}

	private void OnFullHeroInfo(FullHeroInfoArg _arg)
	{
		mHeroInfoWnd.SetHeroData(_arg.mNick, _arg.mInfo, _arg.mData.mItems[0], mCore.Battle.SelfPlayer.PlayerId == _arg.mHeroId);
		mHeroInfoWnd.Open();
	}

	private void OnHeroDataListBroadCast(HeroDataListMpdArg _arg)
	{
		OnHeroDataList(_arg.mData);
	}

	private void OnHeroDataList(HeroDataListArg _arg)
	{
		int userId = mCore.UserNetData.UserId;
		foreach (HeroDataListArg.HeroDataItem mItem in _arg.mItems)
		{
			if (mItem.mHeroId == userId)
			{
				UserLog.AddAction(UserActionType.CS_START, userId, GuiSystem.GetLocaleText("LOG_LEVEL") + mItem.mLevel);
				mTutorial.SetWindow(mTutorialWnd);
				mTutorial.SetScreenId("central_square");
			}
		}
	}

	private void OnUpdateHeroesInfo(HeroesInfoUpdateArg _arg)
	{
		int playerId = mCore.Battle.SelfPlayer.PlayerId;
		foreach (KeyValuePair<int, HeroGameInfo> item in _arg.mInfo)
		{
			if (item.Key == playerId)
			{
				UserLog.AddAction(UserActionType.EXP_UPDATE, playerId, GuiSystem.GetLocaleText("LOG_LEVEL") + item.Value.mLevel);
			}
		}
	}

	private void OnDayRewardMessage(DayRewardMpdArg _arg)
	{
		mRewardWnd.SetData(_arg.mReward, _arg.mCurrent);
		mRewardWnd.Open();
	}

	private void OnMessage(DeferredMessageArg _arg)
	{
		if (_arg.mType == 4)
		{
			CtrlPrototype ctrlPrototype = mCtrlSrv.GetPrototypeProvider().Get(_arg.mIntParameter);
			if (ctrlPrototype != null)
			{
				mCurseWnd.SetData(ctrlPrototype.Desc.mDesc, "Gui/Icons/Items/" + ctrlPrototype.Desc.mIcon);
				mCurseWnd.Open();
			}
			UserLog.AddAction(UserActionType.PLAYER_LEAVE);
		}
		else if (_arg.mType == 5)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_MESSAGE_5");
			localeText = localeText.Replace("{COUNT}", (_arg.mIntParameter / 60 / 60).ToString());
			mOkDialogWnd.SetData(localeText);
			UserLog.AddAction(UserActionType.PLAYER_LEAVE);
		}
	}

	private void Dress(int _itemId, int _slot)
	{
		Notifier<SelfHero, object> notifier = new Notifier<SelfHero, object>();
		notifier.mCallback = delegate(bool _success, SelfHero _selfHero, object _data)
		{
			if (_success)
			{
				ShowHeroInfo(_selfHero.Hero.Id);
			}
		};
		Notifier<int, object> notifier2 = new Notifier<int, object>();
		notifier2.mCallback = delegate(bool _success, int _error, object _data)
		{
			if (!_success)
			{
				string id = "GUI_INVENTORY_CANNOT_DRESS";
				if (_error == 2027)
				{
					id = "GUI_INVENTORY_WRONGE_RACE";
				}
				mOkDialogWnd.SetData(GuiSystem.GetLocaleText(id), null);
			}
			else
			{
				CtrlThing ctrlThing = mCtrlSrv.SelfHero.Inventory.Get(_itemId);
				string comment = "unknown thing name";
				if (ctrlThing != null)
				{
					comment = GuiSystem.GetLocaleText(ctrlThing.CtrlProto.Desc.mName);
				}
				UserLog.AddAction(UserActionType.DRESS, _itemId, comment);
			}
		};
		mCtrlSrv.SelfHero.Dress(_itemId, _slot, notifier, notifier2);
	}

	private void Undress(int _slot)
	{
		Notifier<SelfHero, object> notifier = new Notifier<SelfHero, object>();
		notifier.mCallback = delegate(bool _success, SelfHero _selfHero, object _data)
		{
			if (_success)
			{
				ShowHeroInfo(_selfHero.Hero.Id);
			}
		};
		CtrlThing itemAtSlot = mCtrlSrv.SelfHero.Hero.GetItemAtSlot(_slot);
		string comment = "unknown thing name";
		if (itemAtSlot != null)
		{
			comment = GuiSystem.GetLocaleText(itemAtSlot.CtrlProto.Desc.mName);
		}
		UserLog.AddAction(UserActionType.UNDRESS, itemAtSlot.Id, comment);
		mCtrlSrv.SelfHero.Undress(_slot, notifier);
	}

	private void OnNewItemDressed(Hero _hero, int _slot)
	{
		if (mHeroInfoWnd.Active && _hero.Id != mCtrlSrv.SelfHero.Hero.Id && _hero.Id == mHeroInfoWnd.GetCurHeroId())
		{
			ShowHeroInfo(_hero.Id);
		}
	}

	private void OnItemUndressed(Hero _hero, int _slot, CtrlThing _item)
	{
		if (mHeroInfoWnd.Active && _hero.Id != mCtrlSrv.SelfHero.Hero.Id && _hero.Id == mHeroInfoWnd.GetCurHeroId())
		{
			ShowHeroInfo(_hero.Id);
		}
	}

	private void OnActionDone(ActionUseArg _arg)
	{
		if (_arg.mConflictItemId <= 0)
		{
			return;
		}
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			if (_yes)
			{
				CtrlThing ctrlThing = mCtrlSrv.SelfHero.Inventory.Get(_arg.mCurrentItemId);
				if (ctrlThing != null)
				{
					UserLog.AddAction(UserActionType.REPLACE_THING, _arg.mCurrentItemId, GuiSystem.GetLocaleText(ctrlThing.CtrlProto.Desc.mName));
					mCtrlSrv.HeroSender.DoAction(ctrlThing, _check: false);
				}
			}
		};
		CtrlPrototype ctrlPrototype = mCtrlSrv.GetPrototypeProvider().Get(_arg.mConflictItemId);
		string text = GuiSystem.GetLocaleText("Same_Thing");
		if (ctrlPrototype != null)
		{
			text = text.Replace("{NAME}", GuiSystem.GetLocaleText(ctrlPrototype.Desc.mName));
		}
		else
		{
			Log.Warning("No item in PrototypeProvider itemId - " + _arg.mConflictItemId);
		}
		mYesNoDialogWnd.SetData(text, "Apply_Button_Name", "Cancel_Button_Name", callback);
	}

	private void BankRequest()
	{
		mCtrlSrv.SendGetMoney();
		mBankWnd.SetActive(!mBankWnd.Active);
	}

	private void ChangeMoneyRequest(int _diamondMoney, int _goldMoney)
	{
		mCtrlSrv.SendBankChange(_diamondMoney, _goldMoney);
	}

	private void OnMoneyChanged()
	{
	}

	private void OnMoneyChangeFailed(int _errorCode)
	{
	}

	public void PrepareFightLog(int _battleId, int _battleType, ShortPlayerInfo _selfPlayerHistory, Dictionary<int, BattleEndData> _fightLogData)
	{
		mFightLogBattleId = _battleId;
		mBattleType = _battleType;
		mSelfPlayerHistory = _selfPlayerHistory;
		mFightLogData = _fightLogData;
	}

	private void OnFightLog(FightLogArg _arg)
	{
		ShowFightLog(_arg.mData);
	}

	private void ShowFightLog(Dictionary<int, BattleEndData> _fightLogData)
	{
		if (_fightLogData == null)
		{
			throw new ArgumentNullException("_fightLogData");
		}
		if (mSelfPlayerHistory == null)
		{
			Log.Warning("null self player history");
			return;
		}
		if (!_fightLogData.TryGetValue(mSelfPlayerHistory.mId, out var value))
		{
			Log.Warning("cannot find self battle end data");
			return;
		}
		mCompensationWnd.SetData(value, mCore.CtrlServer.GetPrototypeProvider(), mFormatedTipMgr, mCtrlSrv.SelfHero.Hero.View.mRace, (MapType)mBattleType);
		mCompensationWnd.Open();
		mSelfPlayerHistory = null;
	}

	private void OnClanCreate(string _name, string _tag)
	{
		mCtrlSrv.Clan.CreateClan(_name, _tag);
	}

	private void OnClanCreated()
	{
		if (mClanMenuCreate.Active)
		{
			mClanMenuCreate.Close();
		}
		mCtrlSrv.Clan.RefreshInfo();
	}

	private void OnClanInfoChanged(Clan _clan)
	{
		if (mClanMenu.Active)
		{
			mClanMenu.Close();
		}
		mClanMenu.SetData(_clan, mYesNoDialogWnd);
		mClanMenu.Open();
	}

	private void OnClanremoved()
	{
		if (mClanMenu.Active)
		{
			mClanMenu.Close();
		}
	}

	private void OnClanInfoRequest(string _clanTag)
	{
		mCtrlSrv.Clan.GetClanInfo(string.Empty, string.Empty, _clanTag);
	}

	private void OnClanInfo(Clan.ClanInfo _info)
	{
		if (mClanMenu.Active)
		{
			mClanMenu.Close();
		}
		mClanMenu.SetData(_info, mYesNoDialogWnd);
		mClanMenu.Open();
	}

	private void OnSelfRemoved()
	{
		if (mClanMenu.Active)
		{
			mClanMenu.Close();
		}
	}
}
