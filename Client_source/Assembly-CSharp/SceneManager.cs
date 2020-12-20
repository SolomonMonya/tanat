using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("Tanat/Main/SceneManager")]
public class SceneManager : MonoBehaviour, IMapManager
{
	[Serializable]
	public class SceneConfig
	{
		public string mSceneName;

		public bool mPacked;

		public string[] mCameras;

		public string mScreen;

		public SoundSystem.Music[] mMusic;

		public MapType mMapType;
	}

	[Serializable]
	public class MiniMapConfig
	{
		public string mMapName;

		public Vector2 mMapOffset;

		public float mScaleFactor = 1f;

		public float mRotFactor;

		public string mMiniMapImage;

		public float mMapSize;

		public bool mBattleStartClock;
	}

	private class LoadSceneFromPackTask : TaskQueue.Task
	{
		private string mSceneName;

		private AsyncOperation mAsyncOp;

		public LoadSceneFromPackTask(string _sceneName, Notifier<TaskQueue.ITask, object> _notifier)
			: base(_notifier)
		{
			mSceneName = _sceneName;
		}

		public override void Begin()
		{
			if (!mBegined)
			{
				base.Begin();
				mAsyncOp = Application.LoadLevelAdditiveAsync(mSceneName);
			}
		}

		public override bool IsDone()
		{
			return base.IsDone() && mAsyncOp.isDone;
		}
	}

	private class UpdateCamerasTask : TaskQueue.Task
	{
		private List<string> mCamerasToCreate;

		public UpdateCamerasTask(IEnumerable<string> _names)
		{
			mCamerasToCreate = new List<string>(_names);
		}

		public override void Begin()
		{
			if (mBegined)
			{
				return;
			}
			base.Begin();
			GameObject gameObject = GameObjUtil.GetGameObject("/cameras");
			if (null == gameObject)
			{
				gameObject = new GameObject("/cameras");
			}
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in gameObject.transform)
			{
				if (item.gameObject.GetComponent<Camera>() == null || !mCamerasToCreate.Contains(item.gameObject.name))
				{
					list.Add(item.gameObject);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				GameObject gameObject2 = list[i];
				AudioListener component = gameObject2.GetComponent<AudioListener>();
				if (null != component)
				{
					component.enabled = false;
				}
				GameObjUtil.DestroyGameObject(gameObject2);
			}
			foreach (string item2 in mCamerasToCreate)
			{
				GameObject gameObject3 = GameObjUtil.GetGameObject("/cameras/" + item2);
				if (null == gameObject3)
				{
					gameObject3 = CameraMgr.CreateCamera(item2);
				}
			}
		}
	}

	private class SetScreenTask : TaskQueue.Task
	{
		private ScreenHolder mScrHolder;

		private string mScreenName;

		private object mScreenType;

		public SetScreenTask(ScreenHolder _scrHolder, string _screenName, Notifier<TaskQueue.ITask, object> _notifier)
			: base(_notifier)
		{
			mScrHolder = _scrHolder;
			mScreenName = _screenName;
		}

		public override void Begin()
		{
			if (mBegined)
			{
				return;
			}
			base.Begin();
			if (string.IsNullOrEmpty(mScreenName))
			{
				return;
			}
			try
			{
				mScreenType = Enum.Parse(typeof(ScreenType), mScreenName, ignoreCase: true);
				if (mScreenType != null)
				{
					mScrHolder.ShowScreen((ScreenType)(int)mScreenType);
				}
			}
			catch (FormatException)
			{
			}
		}

		public override bool IsDone()
		{
			if (mScreenType == null)
			{
				return base.IsDone();
			}
			return base.IsDone() && mScrHolder.IsCurScreen((ScreenType)(int)mScreenType);
		}
	}

	private class UnloadResourcesTask : TaskQueue.Task
	{
		private AssetLoader mLoader;

		public UnloadResourcesTask(AssetLoader _loader)
		{
			mLoader = _loader;
		}

		public override void Begin()
		{
			if (!mBegined)
			{
				base.Begin();
				mLoader.UnloadAll();
				Resources.UnloadUnusedAssets();
			}
		}
	}

	public SceneConfig[] mSceneConfigs;

	public MiniMapConfig[] mMiniMapConfigs;

	private string mScenesDir;

	private TaskQueue mTaskQueue;

	private AssetLoader mLoader;

	private ScreenHolder mScreenHolder;

	private BattleMapData mLastLoadedMap;

	private bool mLoadingInProgress;

	public bool LoadingInProgress => mLoadingInProgress;

	public void Init(Config _config, TaskQueue _taskQueue, AssetLoader _loader, ScreenHolder _screenHolder)
	{
		if (_config == null)
		{
			throw new ArgumentNullException("_config");
		}
		if (null == _taskQueue)
		{
			throw new ArgumentNullException("_taskQueue");
		}
		if (_loader == null)
		{
			throw new ArgumentNullException("_loader");
		}
		if (_screenHolder == null)
		{
			throw new ArgumentNullException("_screenHolder");
		}
		mScenesDir = _config.DataDir + "scenes/";
		mTaskQueue = _taskQueue;
		mLoader = _loader;
		mScreenHolder = _screenHolder;
	}

	public void LoadScene(string _sceneName, Notifier<IMapManager, object> _notifier)
	{
		BattleMapData battleMapData = new BattleMapData();
		battleMapData.mMapName = _sceneName;
		LoadBattleMap(battleMapData, _isObserver: false, _notifier);
	}

	public void LoadBattleMap(BattleMapData _data, bool _isObserver, Notifier<IMapManager, object> _notifier)
	{
		SceneConfig sceneConfig = Array.Find(mSceneConfigs, (SceneConfig c) => c.mSceneName == _data.mMapName);
		Notifier<IMapManager, object> notifier = null;
		if (sceneConfig == null)
		{
			Log.Error("unknown scene " + _data.mMapName);
			notifier = new Notifier<IMapManager, object>(RecallFailedNotifier, _notifier);
		}
		mLoadingInProgress = true;
		if (mLastLoadedMap == null || mLastLoadedMap.mMapName != _data.mMapName)
		{
			UnloadCurrentBattleMap(notifier, _unloadRes: true);
		}
		else
		{
			UnloadCurrentBattleMap(notifier, _unloadRes: false);
		}
		GameObjUtil.CreateObjsTask task = new GameObjUtil.CreateObjsTask(new string[4]
		{
			"objects",
			"effects",
			"cameras",
			"heroes"
		}, null);
		mTaskQueue.AddTask(task);
		UpdateCamerasTask task2 = new UpdateCamerasTask(sceneConfig.mCameras);
		mTaskQueue.AddTask(task2);
		if (sceneConfig != null)
		{
			Notifier<TaskQueue.ITask, object> notifier2 = null;
			if (_notifier != null)
			{
				notifier2 = new Notifier<TaskQueue.ITask, object>(RecallSuccessNotifier, _notifier);
			}
			Notifier<TaskQueue.ITask, object> notifier3 = null;
			if (sceneConfig.mMusic != null && sceneConfig.mMusic.Length > 0)
			{
				SoundSystem.PlayMusicTask data = new SoundSystem.PlayMusicTask(sceneConfig.mMusic, notifier2);
				notifier3 = new Notifier<TaskQueue.ITask, object>(OnResourceLoaded, data);
			}
			else
			{
				notifier3 = new Notifier<TaskQueue.ITask, object>(OnResourceLoaded, notifier2);
			}
			SetScreenTask setScreenTask = null;
			if (_isObserver)
			{
				setScreenTask = new SetScreenTask(mScreenHolder, "OBSERVER", notifier3);
			}
			else if (!string.IsNullOrEmpty(sceneConfig.mScreen))
			{
				setScreenTask = new SetScreenTask(mScreenHolder, sceneConfig.mScreen, notifier3);
			}
			Notifier<TaskQueue.ITask, object> notifier4 = null;
			notifier4 = ((setScreenTask == null) ? new Notifier<TaskQueue.ITask, object>(OnResourceLoaded, notifier3) : new Notifier<TaskQueue.ITask, object>(OnResourceLoaded, setScreenTask));
			if (!sceneConfig.mPacked)
			{
				throw new NotImplementedException("TODO");
			}
			LoadSceneFromPackTask data2 = new LoadSceneFromPackTask(_data.mMapName, notifier4);
			Notifier<ILoadedAsset, object> notifier5 = new Notifier<ILoadedAsset, object>(OnLevelResourcesLoaded, data2);
			string assetName = mScenesDir + _data.mMapName + ".unity3d";
			mLoader.LoadAsset(assetName, typeof(AssetBundle), notifier5);
		}
		mLastLoadedMap = _data;
	}

	private void OnResourceLoaded(bool _success, TaskQueue.ITask _task, object _data)
	{
		if (_success)
		{
			TaskQueue.ITask task = _data as TaskQueue.ITask;
			if (task != null)
			{
				mTaskQueue.AddTask(task);
			}
			else
			{
				(_data as Notifier<TaskQueue.ITask, object>)?.Call(_success: true, _task);
			}
		}
	}

	private void OnLevelResourcesLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			TaskQueue.ITask task = _data as TaskQueue.ITask;
			mTaskQueue.AddTask(task);
		}
	}

	public void UnloadCurrentBattleMap(Notifier<IMapManager, object> _notifier, bool _unloadRes)
	{
		Notifier<TaskQueue.ITask, object> notifier = null;
		if (_notifier != null)
		{
			notifier = new Notifier<TaskQueue.ITask, object>(RecallNotifier, _notifier);
		}
		GameObjUtil.DeleteObjsTask task = new GameObjUtil.DeleteObjsTask(new string[5]
		{
			"/level",
			"/objects",
			"/effects",
			"/cameras",
			"/heroes"
		}, notifier);
		mTaskQueue.AddTask(task);
		if (_unloadRes)
		{
			mTaskQueue.AddTask(new UnloadResourcesTask(mLoader));
		}
	}

	private void RecallNotifier(bool _success, TaskQueue.ITask _task, object _data)
	{
		Notifier<IMapManager, object> notifier = _data as Notifier<IMapManager, object>;
		notifier.Call(_success, this);
	}

	private void RecallSuccessNotifier(bool _success, TaskQueue.ITask _task, object _data)
	{
		mLoadingInProgress = false;
		Notifier<IMapManager, object> notifier = _data as Notifier<IMapManager, object>;
		notifier.Call(_success, this);
	}

	private void RecallFailedNotifier(bool _success, IMapManager _mapMgr, object _data)
	{
		Notifier<IMapManager, object> notifier = _data as Notifier<IMapManager, object>;
		notifier.Call(_success: false, this);
	}

	public void InitMiniMap(MiniMap _miniMap)
	{
		if (_miniMap != null && mLastLoadedMap != null && mMiniMapConfigs != null)
		{
			MiniMapConfig miniMapConfig = Array.Find(mMiniMapConfigs, (MiniMapConfig c) => c.mMapName == mLastLoadedMap.mMapName);
			if (miniMapConfig != null)
			{
				_miniMap.SetMap(miniMapConfig.mMiniMapImage, miniMapConfig.mMapSize, miniMapConfig.mMapOffset, miniMapConfig.mScaleFactor, miniMapConfig.mRotFactor);
			}
		}
	}

	public void InitClock(ShortGameInfo _guiElem, BattleTimer _timer)
	{
		if (_guiElem == null || mLastLoadedMap == null || mMiniMapConfigs == null)
		{
			return;
		}
		MiniMapConfig miniMapConfig = Array.Find(mMiniMapConfigs, (MiniMapConfig c) => c.mMapName == mLastLoadedMap.mMapName);
		if (miniMapConfig != null)
		{
			if (miniMapConfig.mBattleStartClock)
			{
				_guiElem.SetTimeOffset(_timer.Time);
			}
			else
			{
				_guiElem.SetTimeOffset(0f);
			}
		}
	}

	public int GetLastLoadedMapType()
	{
		if (mLastLoadedMap == null)
		{
			return -1;
		}
		return (int)(Array.Find(mSceneConfigs, (SceneConfig c) => c.mSceneName == mLastLoadedMap.mMapName)?.mMapType ?? ((MapType)(-1)));
	}
}
