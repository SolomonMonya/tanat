using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Log4Tanat;
using Network;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("Tanat/Main/TanatApp")]
public class TanatApp : MonoBehaviour, ILoginInitiator, IDebugJoinInitiator
{
	private class FullDisconnectData
	{
		public enum ConnectionType
		{
			BATTLE,
			MPD,
			CTRL
		}

		public bool mState;

		public ConnectionType mConType;

		public int mError = -1;

		public string mHost;

		public string mPort;
	}

	public double mLogFlushInterval = 5.0;

	private DateTime mLastLogFlushTime;

	private Config mConfig;

	private NetSystem mNetSys;

	private Core mCore;

	private TutorialMgr mTutorial;

	private TaskQueue mTaskQueue;

	private ScreenManager mScreenMgr;

	private SceneManager mSceneMgr;

	private AssetLoader mAssetLoader;

	private GameObjManager mGameObjMgr;

	private HeroMgr mHeroMgr;

	private SoundSystem mSoundSys;

	public static string mVersion;

	private FullDisconnectData mFullDisconnectData;

	private static Mutex mGlobAppMut;

	public string mProcessName = "Tanat";

	public void Start()
	{
		mConfig = new Config();
		mConfig.ApplyThreadSettings();
		string commandLineArg = Core.GetCommandLineArg("-c");
		if (string.IsNullOrEmpty(commandLineArg))
		{
			mConfig.LoadFromFile("configs/config.xml");
		}
		else
		{
			mConfig.LoadFromFile(commandLineArg);
		}
		mConfig.InitLog();
		mConfig.CreateLocaleState();
		mConfig.LocaleState.LoadFromFile("configs/locale.xml");
		// launcher = GameObjUtil.FindObjectOfType<Launcher>();
		//if (launcher != null)
		//{
		//	mVersion = launcher.mVersion;
		//}
		Log.Debug(string.Concat("platform: ", Application.platform, " version: ", mVersion));
		//if (launcher != null && Application.platform == RuntimePlatform.WindowsPlayer)
		//{
		//	launcher.Launch(this, mConfig);
		//	return;
		//}
		//if (launcher != null)
		//{
		//	UnityEngine.Object.Destroy(launcher.gameObject);
		//}
		Init();
	}

	public void Init()
	{
		if (!IsSingleAppInstance())
		{
			Application.Quit();
			return;
		}
		Log.Debug("BEGIN");
		string currentDirectory = Directory.GetCurrentDirectory();
		Log.Debug("working directory: " + currentDirectory);
		if (RetrieveComponents())
		{
			mNetSys = new NetSystem();
			mAssetLoader = new AssetLoader(mTaskQueue);
			mAssetLoader.LoadAssetsDataFromFile(mConfig.DataDir + "resources.xml");
			mGameObjMgr = new GameObjManager(mAssetLoader, mHeroMgr, VisualEffectsMgr.Instance, mTaskQueue);
			mScreenMgr.Init(mConfig, mTaskQueue);
			mSceneMgr.Init(mConfig, mTaskQueue, mAssetLoader, mScreenMgr.Holder);
			mHeroMgr.Init(mConfig);
			mSoundSys.Init();
			ShowStartScreen();
			mTutorial = new TutorialMgr(AssetLoader.ReadText("configs/tutorial"));
		}
	}

	private bool RetrieveComponents()
	{
		mTaskQueue = GameObjUtil.FindObjectOfType<TaskQueue>();
		if (mTaskQueue == null)
		{
			Log.Error("TaskQueue doesn't exists");
			return false;
		}
		mScreenMgr = GameObjUtil.FindObjectOfType<ScreenManager>();
		if (mScreenMgr == null)
		{
			Log.Error("ScreenManager doesn't exist");
			return false;
		}
		mSceneMgr = GameObjUtil.FindObjectOfType<SceneManager>();
		if (mSceneMgr == null)
		{
			Log.Error("SceneManager doesn't exist");
			return false;
		}
		mHeroMgr = GameObjUtil.FindObjectOfType<HeroMgr>();
		if (mHeroMgr == null)
		{
			Log.Error("HeroMgr doesn't exist");
			return false;
		}
		mSoundSys = GameObjUtil.FindObjectOfType<SoundSystem>();
		if (mSoundSys == null)
		{
			Log.Error("SoundSystem doesn't exist");
			return false;
		}
		Log.Info("all components retrieved");
		return true;
	}

	public void Update()
	{
		//Discarded unreachable code: IL_002e
		BattleTimer.deltaTime = Time.deltaTime;
		try
		{
			if (mCore != null)
			{
				mCore.Update();
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
		if ((DateTime.Now - mLastLogFlushTime).TotalSeconds > mLogFlushInterval)
		{
			Log.Flush();
			mLastLogFlushTime = DateTime.Now;
		}
		if (mFullDisconnectData == null || mSceneMgr.LoadingInProgress)
		{
			return;
		}
		LoginScreen loginScreen = mScreenMgr.Holder.GetScreen(ScreenType.LOGIN) as LoginScreen;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["{HOST}"] = mFullDisconnectData.mHost;
		dictionary["{PORT}"] = mFullDisconnectData.mPort;
		switch (mFullDisconnectData.mConType)
		{
		case FullDisconnectData.ConnectionType.BATTLE:
			if (mFullDisconnectData.mState)
			{
				loginScreen.ShowPopup("GUI_SRV_DISCONNECT", dictionary);
			}
			else
			{
				loginScreen.ShowPopup("GUI_CANNOT_CONNECT", dictionary);
			}
			break;
		case FullDisconnectData.ConnectionType.MPD:
			switch (mFullDisconnectData.mError)
			{
			case 1:
				loginScreen.ShowPopup("GUI_MPD_CANNOT_CONNECT", dictionary);
				break;
			case 3:
				loginScreen.ShowPopup("GUI_MPD_CANNOT_AUTHORIZE", dictionary);
				break;
			case 2:
				loginScreen.ShowPopup("GUI_MPD_DISCONNECT", dictionary);
				break;
			}
			break;
		case FullDisconnectData.ConnectionType.CTRL:
			loginScreen.ShowPopup("GUI_SERVER_NOT_AVAILABLE", dictionary);
			break;
		}
		mFullDisconnectData = null;
		Exit();
		if (mCore != null)
		{
			mCore.CtrlServer.KeepSessionAlive = true;
		}
	}

	public void OnApplicationQuit()
	{
		UserLog.AddAction(UserActionType.GAME_QUIT);
		UserLog.SaveLog();
		mCore.CtrlServer.EntryPoint.SendOutcomings();
		if (mNetSys != null)
		{
			mNetSys.DisconnectAll();
		}
		Log.Debug("END");
		Log.DisableAll();
		if (mGlobAppMut != null)
		{
			mGlobAppMut.ReleaseMutex();
		}
	}

	private Core CreateCore(UserNetData _userData)
	{
		_userData.ClientVersion = mVersion;
		Core core = new Core(mNetSys, mConfig, _userData);
		core.SetGameObjectManager(mGameObjMgr);
		core.SetMapManager(mSceneMgr);
		core.SetHeroViewManager(mGameObjMgr);
		core.CtrlServer.EnableAutoConnect(core.BattleServer);
		return core;
	}

	public void StartLogin(string _email, string _passwd, Notifier<LoginPerformer, object> _notifier)
	{
		UserNetData userData = new UserNetData(_email, _passwd);
		Core core = CreateCore(userData);
		LoginTask loginTask = base.gameObject.AddComponent<LoginTask>();
		loginTask.Init(core, this, _notifier, mScreenMgr.Holder.GetScreen(ScreenType.LOGIN) as LoginScreen);
	}

	public void StartDebugJoin(BattleServerData _srvData, BattleMapData _mapData)
	{
		if (_srvData == null)
		{
			throw new ArgumentNullException("_srvData");
		}
		if (_mapData == null)
		{
			throw new ArgumentNullException("_mapData");
		}
		UserNetData userNetData = new UserNetData("player@mail.ru", string.Empty);
		Core core = CreateCore(userNetData);
		userNetData.UserId = -128;
		core.BattleServer.Connect(_srvData, _mapData);
		SetCore(core);
	}

	public bool IsCoreExists()
	{
		return mCore != null;
	}

	public void SetCore(Core _core)
	{
		if (_core == null)
		{
			throw new ArgumentNullException();
		}
		if (mCore != null)
		{
			throw new InvalidOperationException("Core already exists");
		}
		mCore = _core;
		mCore.BattleServer.SubscribeFullDisconnect(OnFullDisconnect);
		mCore.CtrlServer.EntryPoint.SubscribeConnectionError(OnCtrlConnectionError);
		if (mCore.CtrlServer.MpdConnection != null)
		{
			mCore.CtrlServer.MpdConnection.SubscribeConnectionError(OnMpdConnectionError);
		}
		ScreenHolder holder = mScreenMgr.Holder;
		if (!holder.IsScreenRegistered(ScreenType.BATTLE))
		{
			BattleScreen screen = new BattleScreen(mCore, mScreenMgr, this);
			holder.RegisterScreen(ScreenType.BATTLE, screen);
		}
		if (!holder.IsScreenRegistered(ScreenType.OBSERVER))
		{
			ObserverScreen screen2 = new ObserverScreen(mCore, mScreenMgr, this);
			holder.RegisterScreen(ScreenType.OBSERVER, screen2);
		}
		if (!holder.IsScreenRegistered(ScreenType.CS))
		{
			CentralSquareScreen screen3 = new CentralSquareScreen(mCore, mScreenMgr, this);
			holder.RegisterScreen(ScreenType.CS, screen3);
		}
		if (!holder.IsScreenRegistered(ScreenType.CUSTOMIZE_HERO))
		{
			CustomizeHeroScreen screen4 = new CustomizeHeroScreen(mScreenMgr, mCore.CtrlServer, mHeroMgr, this);
			holder.RegisterScreen(ScreenType.CUSTOMIZE_HERO, screen4);
		}
		Log.Debug("screens inited");
	}

	public void Exit()
	{
		if (mCore != null)
		{
			mCore.BattleServer.WaitReconnect();
			mCore.BattleServer.Disconnect();
			mCore.CtrlServer.Logout();
			mCore = null;
			mScreenMgr.Holder.UnregisterAllScreens();
			ShowStartScreen();
		}
	}

	public void ShowStartScreen()
	{
		ScreenHolder holder = mScreenMgr.Holder;
		if (!holder.IsScreenRegistered(ScreenType.LOGIN))
		{
			LoginScreen loginScreen = new LoginScreen(this, mScreenMgr, mSceneMgr);
			loginScreen.EnableDebugJoin(this);
			holder.RegisterScreen(ScreenType.LOGIN, loginScreen);
		}
		holder.ShowScreen(ScreenType.LOGIN);
	}

	private void OnFullDisconnect(bool _state, string _host, string _port)
	{
		FullDisconnectData fullDisconnectData = new FullDisconnectData();
		fullDisconnectData.mConType = FullDisconnectData.ConnectionType.BATTLE;
		fullDisconnectData.mState = _state;
		fullDisconnectData.mHost = _host;
		fullDisconnectData.mPort = _port;
		mFullDisconnectData = fullDisconnectData;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("battle");
		stringBuilder.Append(" | ");
		stringBuilder.Append((!_state) ? "cannot connect" : "disconnect");
		stringBuilder.Append(" | ");
		stringBuilder.Append(_host);
		stringBuilder.Append(":");
		stringBuilder.Append(_port);
		mCore.CtrlServer.SendLog(CtrlServerConnection.LogLevel.error, stringBuilder.ToString());
	}

	private void OnCtrlConnectionError()
	{
		Log.Error("OnCtrlConnectionError");
		FullDisconnectData fullDisconnectData = new FullDisconnectData();
		fullDisconnectData.mConType = FullDisconnectData.ConnectionType.CTRL;
		CtrlEntryPoint entryPoint = mCore.CtrlServer.EntryPoint;
		fullDisconnectData.mHost = entryPoint.Host;
		fullDisconnectData.mPort = entryPoint.Port;
		mFullDisconnectData = fullDisconnectData;
	}

	private void OnMpdConnectionError(MpdConnection.ConnectionError _error, string _host, string _port)
	{
		FullDisconnectData fullDisconnectData = new FullDisconnectData();
		fullDisconnectData.mConType = FullDisconnectData.ConnectionType.MPD;
		fullDisconnectData.mState = _error == MpdConnection.ConnectionError.DISCONNECT;
		fullDisconnectData.mError = (int)_error;
		fullDisconnectData.mHost = _host;
		fullDisconnectData.mPort = _port;
		mFullDisconnectData = fullDisconnectData;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("jmpd");
		stringBuilder.Append(" | ");
		switch (_error)
		{
		case MpdConnection.ConnectionError.CANNOT_CONNECT:
			stringBuilder.Append("cannot connect");
			break;
		case MpdConnection.ConnectionError.AUTHORIZATION_FAILED:
			stringBuilder.Append("authorization failed");
			break;
		case MpdConnection.ConnectionError.DISCONNECT:
			stringBuilder.Append("disconnect");
			break;
		}
		stringBuilder.Append(" | ");
		stringBuilder.Append(_host);
		stringBuilder.Append(":");
		stringBuilder.Append(_port);
		mCore.CtrlServer.SendLog(CtrlServerConnection.LogLevel.error, stringBuilder.ToString());
	}

	public TutorialMgr TutorialMgr()
	{
		return mTutorial;
	}

	private bool IsSingleAppInstance()
	{
		if (!mConfig.SingleAppInstance)
		{
			return true;
		}
		if (Application.platform != RuntimePlatform.WindowsPlayer)
		{
			return true;
		}
		return CheckAppByMutex();
	}

	private int CheckAppByProcessName()
	{
		//Discarded unreachable code: IL_003e, IL_004b
		try
		{
			Process currentProcess = Process.GetCurrentProcess();
			Log.Debug("self process name: " + currentProcess.ProcessName);
			Process[] processesByName = Process.GetProcessesByName(mProcessName);
			if (processesByName.Length == 1)
			{
				return 0;
			}
			return 1;
		}
		catch (SystemException)
		{
			return -1;
		}
	}

	private bool CheckAppByMutex()
	{
		string name = "Global\\TanatOnline";
		Mutex mutex = null;
		try
		{
			mutex = Mutex.OpenExisting(name);
		}
		catch (Exception ex)
		{
			Log.Debug("CheckAppByMutex Exception : " + ex.ToString());
		}
		if (mutex == null)
		{
			mutex = (mGlobAppMut = new Mutex(initiallyOwned: true, name));
			return true;
		}
		return false;
	}
}
