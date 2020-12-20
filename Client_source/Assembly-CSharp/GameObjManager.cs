using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class GameObjManager : IHeroViewManager, IGameObjectManager
{
	private class CreateGameObjData
	{
		public int mObjectId;

		public BattlePrototype mPrototype;

		public Notifier<IGameObject, object> mNotifier;
	}

	private AssetLoader mLoader;

	private HeroMgr mHeroMgr;

	private VisualEffectsMgr mEffectsMgr;

	private TaskQueue mTaskQueue;

	private bool mExported;

	public GameObjManager(AssetLoader _loader, HeroMgr _heroMgr, VisualEffectsMgr _effectsMgr, TaskQueue _taskQueue)
	{
		if (_loader == null)
		{
			throw new ArgumentNullException("_loader");
		}
		if (_heroMgr == null)
		{
			throw new ArgumentNullException("_heroMgr");
		}
		if (_effectsMgr == null)
		{
			throw new ArgumentNullException("_effectsMgr");
		}
		if (_taskQueue == null)
		{
			throw new ArgumentNullException("_taskQueue");
		}
		mLoader = _loader;
		mHeroMgr = _heroMgr;
		mEffectsMgr = _effectsMgr;
		mTaskQueue = _taskQueue;
	}

	private void Export()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(ExportObjectData));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			ExportObjectData exportObjectData = @object as ExportObjectData;
			exportObjectData.gameObject.SetActiveRecursively(state: false);
			UnityEngine.Object.Destroy(exportObjectData.gameObject);
		}
	}

	public IGameObject CreateGameObject(int _objectId, BattlePrototype _proto)
	{
		return null;
	}

	public void CreateGameObjectAsync(int _objectId, BattlePrototype _proto, Notifier<IGameObject, object> _notifier)
	{
		if (!mExported)
		{
			Export();
			mExported = true;
		}
		CreateGameObjData createGameObjData = new CreateGameObjData();
		createGameObjData.mObjectId = _objectId;
		createGameObjData.mPrototype = _proto;
		createGameObjData.mNotifier = _notifier;
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>(OnGameObjectLoaded, createGameObjData);
		mLoader.LoadAsset(_proto.Prefab.mValue, typeof(GameObject), notifier);
	}

	public void OnGameObjectLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		CreateGameObjData createGameObjData = _data as CreateGameObjData;
		if (createGameObjData == null)
		{
			return;
		}
		if (!_success)
		{
			if (createGameObjData.mNotifier != null)
			{
				createGameObjData.mNotifier.Call(_success: false, null);
			}
			return;
		}
		InstanceData data = new InstanceData(createGameObjData.mObjectId);
		GameObject gameObject = UnityEngine.Object.Instantiate(_asset.Asset) as GameObject;
		gameObject.SetActiveRecursively(state: false);
		gameObject.name = createGameObjData.mObjectId.ToString();
		GameObjUtil.TrySetParent(gameObject, "/objects");
		GameData gameData = gameObject.AddComponent<GameData>();
		gameData.Init(createGameObjData.mPrototype, data);
		if (createGameObjData.mNotifier != null)
		{
			createGameObjData.mNotifier.Call(_success: true, gameData);
		}
	}

	public void EnableGameObject(IGameObject _obj)
	{
		if (_obj == null)
		{
			throw new ArgumentNullException("_obj");
		}
		GameData gameData = _obj as GameData;
		VisibilityMgr component = gameData.gameObject.GetComponent<VisibilityMgr>();
		if (component != null)
		{
			component.Activate();
		}
		else
		{
			gameData.gameObject.SetActiveRecursively(state: true);
		}
		DeathBehaviour.Reborn(gameData.gameObject);
		if (gameData.NetTransform != null)
		{
			gameData.NetTransform.enabled = true;
		}
		if (gameData.Animation != null)
		{
			gameData.Animation.Reset();
		}
		ParticleEmitter[] componentsInChildren = gameData.gameObject.GetComponentsInChildren<ParticleEmitter>();
		ParticleEmitter[] array = componentsInChildren;
		foreach (ParticleEmitter particleEmitter in array)
		{
			particleEmitter.ClearParticles();
		}
		TrailRenderer[] componentsInChildren2 = gameData.gameObject.GetComponentsInChildren<TrailRenderer>();
		TrailRenderer[] array2 = componentsInChildren2;
		foreach (TrailRenderer trailRenderer in array2)
		{
			trailRenderer.enabled = true;
		}
		CursorDetector component2 = gameData.gameObject.GetComponent<CursorDetector>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		if (gameData.Data.IsPlayerBinded && gameData.Data.Player.IsSelf)
		{
			VisualEffectOptions component3 = gameData.gameObject.GetComponent<VisualEffectOptions>();
			if (component3 != null)
			{
				component3.ShowSelection(_self: true, Friendliness.FRIEND);
			}
		}
	}

	public void DisableGameObject(IGameObject _obj)
	{
		if (_obj == null)
		{
			throw new ArgumentNullException("_obj");
		}
		GameData gameData = _obj as GameData;
		CursorDetector component = gameData.gameObject.GetComponent<CursorDetector>();
		if (component != null)
		{
			component.enabled = false;
		}
		ShortObjectInfo component2 = gameData.gameObject.GetComponent<ShortObjectInfo>();
		if (component2 != null)
		{
			component2.DisableTip();
		}
		TrailRenderer[] componentsInChildren = gameData.gameObject.GetComponentsInChildren<TrailRenderer>();
		TrailRenderer[] array = componentsInChildren;
		foreach (TrailRenderer trailRenderer in array)
		{
			trailRenderer.enabled = false;
		}
		DeathBehaviour.SetMode(gameData.gameObject, _disableMode: true);
		DeathBehaviour.DieSelectively(gameData.gameObject);
		TryDisable(gameData.gameObject);
	}

	public void DeleteGameObject(IGameObject _obj)
	{
		if (_obj == null)
		{
			throw new ArgumentNullException("_obj");
		}
		GameData gameData = _obj as GameData;
		DeathBehaviour.SetMode(gameData.gameObject, _disableMode: false);
		DeathBehaviour.DieSelectively(gameData.gameObject);
		TryDelete(gameData.gameObject);
	}

	public void DeleteAllGameObjects()
	{
		mExported = false;
		GameObjUtil.DeleteObjsTask task = new GameObjUtil.DeleteObjsTask(new string[2]
		{
			"/objects",
			"/effects"
		}, null);
		mTaskQueue.AddTask(task);
	}

	public void TryDisable(GameObject _go)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		if (DeathBehaviour.IsDone(_go))
		{
			VisibilityMgr component = _go.GetComponent<VisibilityMgr>();
			if (component != null)
			{
				component.Deactivate();
			}
			else
			{
				_go.SetActiveRecursively(state: false);
			}
		}
	}

	public void TryDelete(GameObject _go)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		GameData component = _go.GetComponent<GameData>();
		if (component.Data.GameObjProv.CheckRemoving(component.Id) && DeathBehaviour.IsDone(_go))
		{
			ShortObjectInfo component2 = _go.GetComponent<ShortObjectInfo>();
			if (component2 != null)
			{
				component2.OnDestroy();
			}
			mEffectsMgr.RemoveObjectEffects(_go);
			UnityEngine.Object.Destroy(_go);
			component.Data.GameObjProv.CompleteRemove(component.Id);
		}
	}

	private HeroWear GetHeroWear(IGameObject _obj)
	{
		GameObject gameObject = (_obj as GameData).gameObject;
		HeroWear componentInChildren = gameObject.GetComponentInChildren<HeroWear>();
		if (componentInChildren == null)
		{
			Log.Error("there is no HeroWear on " + gameObject.name);
			return null;
		}
		return componentInChildren;
	}

	public void SetHeroView(IGameObject _go, HeroView _view, Notifier<IHeroViewManager, object> _notifier)
	{
		GameObject gameObject = (_go as GameData).gameObject;
		HeroMgr.CreateHeroData createHeroData = new HeroMgr.CreateHeroData();
		createHeroData.mParentObj = gameObject;
		createHeroData.mParams = _view;
		createHeroData.mOnHeroLoaded = delegate(GameObject _hero, object _data)
		{
			HeroMgr.CreateHeroData createHeroData2 = _data as HeroMgr.CreateHeroData;
			if (createHeroData2 != null && !(createHeroData2.mParentObj == null))
			{
				_hero.transform.parent = createHeroData2.mParentObj.transform;
				_hero.name = "hero";
				HeroWear component = _hero.GetComponent<HeroWear>();
				component.SetDefault((HeroRace)createHeroData2.mParams.mRace, createHeroData2.mParams.mGender);
				component.SetHero(createHeroData2.mParams);
				VisibilityMgr component2 = createHeroData2.mParentObj.GetComponent<VisibilityMgr>();
				if (null != component2)
				{
					component2.MakeVisible();
				}
				GameData component3 = createHeroData2.mParentObj.GetComponent<GameData>();
				component3.InitAnimation();
				if (_notifier != null)
				{
					_notifier.Call(_success: true, this);
				}
			}
		};
		mHeroMgr.CreateHero((HeroRace)_view.mRace, _view.mGender, createHeroData);
	}

	public void SetHeroViewItems(IGameObject _go, IEnumerable<CtrlThing> _items)
	{
		HeroWear heroWear = GetHeroWear(_go);
		if (heroWear == null)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (CtrlThing _item in _items)
		{
			list.Add(_item.CtrlProto.Prefab.mValue);
		}
		heroWear.SetItems(list);
	}

	public void SetHeroViewItem(IGameObject _go, CtrlThing _item)
	{
		HeroWear heroWear = GetHeroWear(_go);
		if (!(heroWear == null))
		{
			heroWear.SetItem(_item.CtrlProto.Prefab.mValue);
		}
	}

	public void RemoveHeroViewItem(IGameObject _go, CtrlThing _item)
	{
		if (_item != null && _item.CtrlProto != null && _item.CtrlProto.Prefab != null)
		{
			HeroWear heroWear = GetHeroWear(_go);
			if (!(heroWear == null))
			{
				heroWear.RemoveItem(_item.CtrlProto.Prefab.mValue);
			}
		}
	}
}
