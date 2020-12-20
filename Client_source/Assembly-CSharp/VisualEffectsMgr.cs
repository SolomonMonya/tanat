using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffectsMgr")]
public class VisualEffectsMgr : MonoBehaviour
{
	private class CreateTask
	{
		public EffectCreatedCallback mCallback;

		public EffectToStart mEts;

		public int mEffectId;
	}

	[Serializable]
	public class DamageEffect
	{
		public string mDamageType;

		public string mDamageEffectId;

		public DamageEffect()
		{
			mDamageType = string.Empty;
			mDamageEffectId = string.Empty;
		}
	}

	private class EffectToStart
	{
		public string mId = string.Empty;

		public float mSpeed;

		public float mTimeStart;

		public GameObject[] mTargetObjects;

		public Vector3[] mTargetVectors;
	}

	private class Effect
	{
		public string mVisualEffectId;

		public GameObject mVisualEffectObj;

		public GameObject mTargetObject;

		public VisualEffectHolder mVisualEffectHolder;

		public float mTimeStart;

		public bool mDone;

		public Effect()
		{
			mVisualEffectId = string.Empty;
			mVisualEffectObj = null;
			mTargetObject = null;
			mVisualEffectHolder = null;
			mTimeStart = 0f;
			mDone = false;
		}
	}

	private delegate void EffectCreatedCallback(UnityEngine.Object _obj, EffectToStart _ets, int _effectId);

	private int mLastEffectId;

	private Dictionary<int, GameObject> mEffects;

	private Dictionary<int, EffectToStart> mEffectsToStart;

	private List<int> mEffectsToRemove;

	private List<int> mStartEffectsToRemove;

	public DamageEffect[] mDamageEffects;

	public BattleData.EffectHolder[] mEffectsOptions;

	private Dictionary<string, int> mEffectsOptionsCache = new Dictionary<string, int>();

	private List<int> mEffectsToStop;

	private static VisualEffectsMgr mInstance;

	public static VisualEffectsMgr Instance => mInstance;

	public void Start()
	{
		Init();
	}

	public void Update()
	{
		foreach (KeyValuePair<int, EffectToStart> item in mEffectsToStart)
		{
			item.Value.mTimeStart -= Time.deltaTime;
			if (item.Value.mTimeStart <= 0f)
			{
				StartEffect(item.Key);
				mStartEffectsToRemove.Add(item.Key);
			}
		}
		foreach (KeyValuePair<int, GameObject> mEffect in mEffects)
		{
			if (null == mEffect.Value)
			{
				mEffectsToRemove.Add(mEffect.Key);
			}
		}
		RemoveEffects();
	}

	private void RemoveEffects()
	{
		foreach (int item in mStartEffectsToRemove)
		{
			mEffectsToStart.Remove(item);
		}
		foreach (int item2 in mEffectsToRemove)
		{
			UnityEngine.Object.Destroy(mEffects[item2]);
			mEffects.Remove(item2);
		}
		mStartEffectsToRemove.Clear();
		mEffectsToRemove.Clear();
	}

	private void Init()
	{
		mEffects = new Dictionary<int, GameObject>();
		mEffectsToStart = new Dictionary<int, EffectToStart>();
		mEffectsToRemove = new List<int>();
		mStartEffectsToRemove = new List<int>();
		mEffectsToStop = new List<int>();
		mLastEffectId = 0;
		string empty = string.Empty;
		int i = 0;
		for (int num = mEffectsOptions.Length; i < num; i++)
		{
			empty = mEffectsOptions[i].mName;
			if (!mEffectsOptionsCache.ContainsKey(empty))
			{
				mEffectsOptionsCache[empty] = i;
			}
			else
			{
				Log.Error("Already existing effect in mEffectsOptions : " + empty);
			}
			mEffectsOptionsCache[empty] = i;
		}
	}

	public void Uninit()
	{
		mEffects.Clear();
		mEffectsToStart.Clear();
		mEffectsToRemove.Clear();
		mStartEffectsToRemove.Clear();
		mEffectsToStop.Clear();
	}

	public BattleData.EffectHolder GetEffectHolder(string _fx)
	{
		if (_fx == null || _fx.Length == 0)
		{
			return null;
		}
		if (mEffectsOptionsCache.TryGetValue(_fx, out var value))
		{
			return mEffectsOptions[value];
		}
		return null;
	}

	private bool CreateEffectPrefabAsync(EffectToStart _ets, int _effectId, EffectCreatedCallback _callback)
	{
		CreateTask createTask = new CreateTask();
		createTask.mCallback = _callback;
		createTask.mEts = _ets;
		createTask.mEffectId = _effectId;
		AssetLoader.Instance?.LoadAsset(_ets.mId, typeof(GameObject), new Notifier<ILoadedAsset, object>(OnLoaded, createTask));
		return true;
	}

	private void OnLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			CreateTask createTask = _data as CreateTask;
			createTask.mCallback(_asset.Asset as GameObject, createTask.mEts, createTask.mEffectId);
		}
	}

	public bool StopEffect(int _effectId)
	{
		if (mEffects.ContainsKey(_effectId))
		{
			GameObject gameObject = mEffects[_effectId];
			if (null != gameObject)
			{
				VisualEffectTargets component = mEffects[_effectId].GetComponent<VisualEffectTargets>();
				component.StopEffect();
				return true;
			}
			Log.Error("Try to stop null effect");
			return false;
		}
		Log.Debug("Try to stop not existing effect : " + _effectId + " adding to mEffectsToStop");
		mEffectsToStop.Add(_effectId);
		return false;
	}

	public void RemoveObjectEffects(GameObject _obj)
	{
		if (null == _obj)
		{
			Log.Error("null object");
			return;
		}
		VisualEffectHolder[] componentsInChildren = _obj.GetComponentsInChildren<VisualEffectHolder>();
		VisualEffectHolder[] array = componentsInChildren;
		foreach (VisualEffectHolder visualEffectHolder in array)
		{
			visualEffectHolder.RemoveEffect();
		}
	}

	public void StopObjectEffects(GameObject _obj)
	{
		if (null == _obj)
		{
			Log.Error("null object");
			return;
		}
		VisualEffectHolder[] componentsInChildren = _obj.GetComponentsInChildren<VisualEffectHolder>();
		VisualEffectHolder[] array = componentsInChildren;
		foreach (VisualEffectHolder visualEffectHolder in array)
		{
			visualEffectHolder.StopEffect();
		}
	}

	public static GameObject GetObjectChildSafe(string _bone, GameObject _object)
	{
		GameObject objectChild = GetObjectChild(_bone, _object);
		if (objectChild != null)
		{
			return objectChild;
		}
		return _object;
	}

	private static GameObject GetObjectChild(string _bone, GameObject _object)
	{
		if (_object.transform.childCount != 0)
		{
			GameObject gameObject = null;
			foreach (Transform item in _object.transform)
			{
				if (item.name == _bone)
				{
					return item.gameObject;
				}
				if (null != (gameObject = GetObjectChild(_bone, item.gameObject)))
				{
					return gameObject;
				}
			}
			return null;
		}
		return null;
	}

	public string GetEffectByDamageType(string _damageType)
	{
		DamageEffect[] array = mDamageEffects;
		foreach (DamageEffect damageEffect in array)
		{
			if (damageEffect.mDamageType == _damageType)
			{
				return damageEffect.mDamageEffectId;
			}
		}
		return null;
	}

	public int PlayEffect<T>(string _id, T _target)
	{
		T[] targets = new T[1]
		{
			_target
		};
		return PlayEffect(_id, 0f, 1f, targets);
	}

	public int PlayEffect<T>(string _id, T[] _targets)
	{
		return PlayEffect(_id, 0f, 1f, _targets);
	}

	public int PlayEffect<T>(string _id, float _time, float _speed, T[] _targets)
	{
		EffectToStart effectToStart = new EffectToStart();
		effectToStart.mId = _id;
		effectToStart.mTimeStart = _time;
		effectToStart.mSpeed = _speed;
		effectToStart.mTargetObjects = _targets as GameObject[];
		effectToStart.mTargetVectors = _targets as Vector3[];
		mEffectsToStart.Add(++mLastEffectId, effectToStart);
		return mLastEffectId;
	}

	private void StartEffect(int _effectId)
	{
		EffectToStart value;
		if (mEffectsToStop.Contains(_effectId))
		{
			mEffectsToStop.Remove(_effectId);
		}
		else if (mEffectsToStart.TryGetValue(_effectId, out value))
		{
			CreateEffectPrefabAsync(value, _effectId, OnEffectObjCreated);
		}
	}

	private void OnEffectObjCreated(UnityEngine.Object _effectPrefab, EffectToStart _ets, int _effectId)
	{
		if (!(null == _effectPrefab))
		{
			if (!mEffectsToStop.Contains(_effectId))
			{
				InitEffect(_effectPrefab, _effectId, _ets);
			}
			else
			{
				mEffectsToStop.Remove(_effectId);
			}
		}
	}

	private bool ValidateTargets(EffectToStart _ets)
	{
		if (_ets == null)
		{
			return false;
		}
		if (_ets.mTargetObjects != null)
		{
			GameObject[] mTargetObjects = _ets.mTargetObjects;
			foreach (GameObject gameObject in mTargetObjects)
			{
				if (gameObject == null)
				{
					return false;
				}
			}
			return true;
		}
		if (_ets.mTargetVectors != null)
		{
			return true;
		}
		return false;
	}

	private bool InitEffect(UnityEngine.Object _effectPrefab, int _effectId, EffectToStart _ets)
	{
		if (null == _effectPrefab)
		{
			return false;
		}
		if (!ValidateTargets(_ets))
		{
			return false;
		}
		if (mEffects.ContainsKey(_effectId))
		{
			Log.Error("Try to add effect with already existing id : " + _effectId + " " + _ets.mId);
			if (mEffectsToStart.ContainsKey(_effectId))
			{
				mStartEffectsToRemove.Add(_effectId);
			}
			return false;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_effectPrefab) as GameObject;
		GameObjUtil.TrySetParent(gameObject, "/effects");
		VisualEffectTargets component = gameObject.GetComponent<VisualEffectTargets>();
		if (component == null)
		{
			Log.Error("VisualEffectTargets is null : " + _ets.mId);
			return false;
		}
		List<VisualEffectTargets.Target> mTargets = component.mTargets;
		if (_ets.mTargetVectors != null)
		{
			int i = 0;
			for (int count = mTargets.Count; i < count; i++)
			{
				VisualEffectHolder component2 = mTargets[i].mGameObject.GetComponent<VisualEffectHolder>();
				if (null == component2)
				{
					Log.Error("Effect holder is null : " + mTargets[i].mGameObject.name + " in : " + _ets.mId);
					return false;
				}
				if (mTargets[i].mNums.Count == 1)
				{
					mTargets[i].mGameObject.transform.position += _ets.mTargetVectors[mTargets[i].mNums[0]];
				}
				else
				{
					List<Vector3> list = new List<Vector3>();
					foreach (int mNum in mTargets[i].mNums)
					{
						list.Add(_ets.mTargetVectors[mNum]);
					}
					if (!AddEffect(_ets, mTargets[i].mGameObject, list))
					{
						return false;
					}
				}
				component2.Init();
			}
		}
		else if (_ets.mTargetObjects != null)
		{
			int j = 0;
			for (int count2 = mTargets.Count; j < count2; j++)
			{
				if (mTargets[j].mNums.Count == 1)
				{
					GameObject target = _ets.mTargetObjects[mTargets[j].mNums[0]];
					if (!AddEffect(_ets, mTargets[j].mGameObject, target))
					{
						return false;
					}
					continue;
				}
				List<GameObject> list2 = new List<GameObject>();
				foreach (int mNum2 in mTargets[j].mNums)
				{
					list2.Add(_ets.mTargetObjects[mNum2]);
				}
				if (!AddEffect(_ets, mTargets[j].mGameObject, list2))
				{
					return false;
				}
			}
		}
		mEffects.Add(_effectId, gameObject);
		return true;
	}

	private bool AddEffect(EffectToStart _ets, GameObject _effect, GameObject _target)
	{
		if (null == _effect || null == _target)
		{
			Log.Error("null arguments in try to play effect : " + _ets.mId);
			return false;
		}
		VisualEffectHolder component = _effect.GetComponent<VisualEffectHolder>();
		if (null == component)
		{
			Log.Error("Effect holder is null : " + _effect.name + " in : " + _ets.mId);
			return false;
		}
		VisualEffectOptions component2 = _target.GetComponent<VisualEffectOptions>();
		if (null == component2)
		{
			Log.Error("VisualEffectOptions is null in : " + _target.name);
			return false;
		}
		VisualEffectOptions.EffectOptions effectOptions = component2.GetEffectOptions(_ets.mId);
		VisualEffectOptions.EffectOptions effectOptions2 = component2.GetEffectOptions(_effect.name);
		Transform transform = _target.transform;
		string text = string.Empty;
		if (component.mDefaultBone.Length > 0)
		{
			text = component.mDefaultBone;
		}
		else if (effectOptions.mBone.Length > 0)
		{
			text = effectOptions.mBone;
		}
		else if (effectOptions2.mBone.Length > 0)
		{
			text = effectOptions2.mBone;
		}
		if (text.Length > 0)
		{
			transform = GetObjectChildSafe(text, _target).transform;
		}
		_effect.transform.parent = transform;
		Vector3 mScale = effectOptions.mScale;
		mScale.x /= _target.transform.localScale.x;
		mScale.y /= _target.transform.localScale.y;
		mScale.z /= _target.transform.localScale.z;
		_effect.transform.localScale = mScale;
		_effect.transform.position += effectOptions.mOffset;
		_effect.transform.position += transform.position;
		_effect.transform.rotation = transform.rotation;
		component.LockRotation(_effect.transform, effectOptions.mLockXRot, effectOptions.mLockYRot, effectOptions.mLockZRot);
		component.SetEffectSpeed(_ets.mSpeed);
		component.Init();
		return true;
	}

	private bool AddEffect(EffectToStart _ets, GameObject _effect, List<GameObject> _targets)
	{
		VisualEffectHolder component = _effect.GetComponent<VisualEffectHolder>();
		if (null == component)
		{
			Log.Error("Effect holder is null : " + _effect.name + " in : " + _ets.mId);
			return false;
		}
		VisualEffectOptions component2 = _targets[0].GetComponent<VisualEffectOptions>();
		if (null == component2)
		{
			Log.Error("VisualEffectOptions is null in : " + _targets[0].name);
			return false;
		}
		VisualEffectOptions.EffectOptions effectOptions = component2.GetEffectOptions(_ets.mId);
		MultiTargetsEffect componentInChildren = _effect.GetComponentInChildren<MultiTargetsEffect>();
		if (effectOptions.mBone != string.Empty)
		{
			_targets[0] = GetObjectChildSafe(effectOptions.mBone, _targets[0]);
		}
		componentInChildren.SetTargets(_targets);
		componentInChildren.Init();
		component.Init();
		return true;
	}

	private bool AddEffect(EffectToStart _ets, GameObject _effect, List<Vector3> _targets)
	{
		VisualEffectHolder component = _effect.GetComponent<VisualEffectHolder>();
		if (null == component)
		{
			Log.Error("Effect holder is null : " + _effect.name + " in : " + _ets.mId);
			return false;
		}
		MultiTargetsEffect componentInChildren = _effect.GetComponentInChildren<MultiTargetsEffect>();
		componentInChildren.SetTargets(_targets);
		componentInChildren.Init();
		component.Init();
		return true;
	}

	public void OnEnable()
	{
		mInstance = this;
	}

	public void OnDisable()
	{
		mInstance = null;
	}
}
