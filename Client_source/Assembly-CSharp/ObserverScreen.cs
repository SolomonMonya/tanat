using System;
using Network;
using TanatKernel;

public class ObserverScreen : BaseBattleScreen, IGameObjectInitializer
{
	private ScreenManager mScreenMgr;

	private BattleStats mBattleStats;

	private MiniMap mMiniMapWnd;

	private ObjectInfo mObjectInfoEnemyWnd;

	private EnemyInfoWindow mEnemyInfoWnd;

	private FormatedTipMgr mFormatedTipMgr;

	private PopupInfo mPopupInfoWnd;

	private VisualBattle mVisualBattle;

	private ObserverInput mPlayerCtrl;

	private KillEventManager mKillEventMgr;

	private HeightMap mHeightMap;

	public ObserverScreen(Core _core, ScreenManager _screenMgr, TanatApp _app)
		: base(_core)
	{
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		mScreenMgr = _screenMgr;
		GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("observer");
		mMiniMapWnd = guiSet.GetElementById<MiniMap>("MINI_MAP_GAME_MENU");
		mBattleStats = guiSet.GetElementById<BattleStats>("STATS_PLATE_GAME_MENU");
		mEnemyInfoWnd = guiSet.GetElementById<EnemyInfoWindow>("ENEMY_INFO_WINDOW");
		mObjectInfoEnemyWnd = guiSet.GetElementById<ObjectInfo>("ENEMY_INFO");
		mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		mPopupInfoWnd = guiSet.GetElementById<PopupInfo>("POPUP_INFO");
	}

	public override void Show()
	{
		base.Show();
		mCore.Battle.AddGameObjInitializer(this);
		mCore.CtrlServer.KeepSessionAlive = true;
		mCore.Battle.mLoaded = true;
		if (mCore.BattleServer != null)
		{
			mCore.BattleServer.NeedMicroReconnect(_needReconnect: false);
		}
		if (mCore.CtrlServer != null && mCore.CtrlServer.MpdConnection != null)
		{
			mCore.CtrlServer.MpdConnection.NeedMicroReconnect(_needReconnect: true);
		}
		mHeightMap = new HeightMap(2f);
		mPlayerCtrl = new ObserverInput();
		mKillEventMgr = new KillEventManager(mCore.CtrlServer.Chat, mCore.Battle.SelfPlayer);
		mVisualBattle = new VisualBattle(mCore.Battle, mKillEventMgr, mPlayerCtrl);
		mVisualBattle.Subscribe(mCore.BattleServer.PacketMgr, mCore.Battle.PlayerStore);
		if (mCore.UserNetData.Inited)
		{
			mCore.CtrlServer.SelfQuests.UpdateContent();
		}
		ShowGui();
	}

	protected override void ShowGui()
	{
		Battle battle = mCore.Battle;
		bool avatarLight = false;
		CommonInput commonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (null != commonInput)
		{
			commonInput.Init(mScreenMgr);
			avatarLight = commonInput.mAvatarLight;
			mHeightMap.mEnableCache = commonInput.mCacheTerrainHeight;
		}
		mPlayerCtrl.Init(battle.GetGameObjProvider());
		ObserverInput observerInput = mPlayerCtrl;
		observerInput.mSelectionChangedCallback = (ObserverInput.SelectionChangedCallback)Delegate.Combine(observerInput.mSelectionChangedCallback, new ObserverInput.SelectionChangedCallback(OnSelectionChanged));
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Init(mCore.BattleServer.PacketMgr);
		}
		mVisualBattle.AvatarLight = avatarLight;
		mVisualBattle.TryAddLight();
		mCore.CtrlServer.GetGroupedItems();
		WarFog warFog = GameObjUtil.TryFindObjectOfType<WarFog>();
		if (warFog != null)
		{
			warFog.Init("observer");
		}
		mMiniMapWnd.SetData(battle.GetGameObjProvider());
		(mCore.MapManager as SceneManager).InitMiniMap(mMiniMapWnd);
		HandlerManager<BattlePacket, BattleCmdId> handlerMgr = mCore.BattleServer.PacketMgr.HandlerMgr;
		handlerMgr.Subscribe<BattleEndArg>(BattleCmdId.BATTLE_END, null, null, OnBattleEnd);
		mScreenMgr.Gui.SetCurGuiSet("observer");
		int lastLoadedMapType = (mCore.MapManager as SceneManager).GetLastLoadedMapType();
		mBattleStats.SetData(battle.GetPlayerProvider(), (MapType)lastLoadedMapType);
		mObjectInfoEnemyWnd.SetData(battle.GetGameObjProvider());
		mEnemyInfoWnd.SetData(battle.GetGameObjProvider(), mCore.CtrlServer.GetPrototypeProvider(), battle.GetPrototypeProvider(), mPopupInfoWnd, mCore.CtrlServer.Heroes, mFormatedTipMgr);
	}

	private void OnBattleEnd(BattleEndArg _arg)
	{
		mCore.CtrlServer.KeepSessionAlive = false;
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
		mCore.CtrlServer.EntryPoint.HandlerMgr.Unsubscribe(this);
		mCore.BattleServer.PacketMgr.HandlerMgr.Unsubscribe(this);
		if (mCore.IsBattleCreated())
		{
			Battle battle = mCore.Battle;
			battle.RemoveGameObjInitializer(this);
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
		FpsCounter fpsCounter = GameObjUtil.FindObjectOfType<FpsCounter>();
		if (fpsCounter != null)
		{
			fpsCounter.Uninit();
		}
		GuiSystem.mGuiInputMgr.Clean();
		VisualEffectOptions.Uninit();
		mMiniMapWnd.Clear();
		mBattleStats.Uninit();
		mObjectInfoEnemyWnd.Clear();
		mEnemyInfoWnd.Clear();
		mPopupInfoWnd.mHints.Clear();
	}

	private void OnSelectionChanged(int _objId)
	{
		mObjectInfoEnemyWnd.SetObj(_objId);
		mEnemyInfoWnd.SetEnemy(_objId);
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
			CursorDetector cursorDetector3 = gameData.gameObject.AddComponent<CursorDetector>();
			cursorDetector3.Init(mPlayerCtrl);
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
}
