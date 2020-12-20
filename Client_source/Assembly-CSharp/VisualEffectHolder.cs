using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffectHolder")]
public class VisualEffectHolder : MonoBehaviour
{
	public delegate void OnDestroy(GameObject _helderObject);

	private bool mLockXRot;

	private bool mLockYRot;

	private bool mLockZRot;

	private Transform mParentTransform;

	private float mEffectSpeed = 1f;

	public bool mShakeCam;

	public float mStartShakeTime;

	public string mDefaultBone;

	public bool mWaitDeath = true;

	private List<GameObject> mEffectObjects = new List<GameObject>();

	private List<GameObject> mEffectsToRemove = new List<GameObject>();

	private MorphEffect mMorphEffect;

	public OnDestroy mOnDestroy;

	public void Init()
	{
		InitEffects();
	}

	public void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Fx");
		if (null != mMorphEffect)
		{
			mMorphEffect.StartMorph(base.transform.parent.gameObject);
		}
	}

	public void StopEffect()
	{
		foreach (GameObject mEffectObject in mEffectObjects)
		{
			if (mEffectObject != null)
			{
				VisualEffect component = mEffectObject.GetComponent<VisualEffect>();
				if (component != null)
				{
					component.StopEffect();
				}
			}
		}
		if (null != mMorphEffect)
		{
			mMorphEffect.EndMorph();
		}
	}

	public void RemoveEffect()
	{
		if (mEffectObjects.Count == 0 && null == mMorphEffect)
		{
			SelfDestroy();
		}
		foreach (GameObject mEffectObject in mEffectObjects)
		{
			if (mEffectObject != null)
			{
				VisualEffect component = mEffectObject.GetComponent<VisualEffect>();
				if (component != null)
				{
					component.mOnSelfDestroy = (VisualEffect.OnSelfDestroy)Delegate.Remove(component.mOnSelfDestroy, new VisualEffect.OnSelfDestroy(OnEffectDestory));
				}
				UnityEngine.Object.Destroy(mEffectObject);
			}
		}
		mEffectObjects.Clear();
		if (null != mMorphEffect)
		{
			mMorphEffect.EndMorphImmediate();
		}
		SelfDestroy();
	}

	private void InitEffects()
	{
		VisualEffect[] componentsInChildren = GetComponentsInChildren<VisualEffect>();
		VisualEffect[] array = componentsInChildren;
		foreach (VisualEffect visualEffect in array)
		{
			if (mEffectSpeed != 1f)
			{
				visualEffect.SetEffectSpeed(mEffectSpeed);
			}
			visualEffect.mOnSelfDestroy = (VisualEffect.OnSelfDestroy)Delegate.Combine(visualEffect.mOnSelfDestroy, new VisualEffect.OnSelfDestroy(OnEffectDestory));
			mEffectObjects.Add(visualEffect.gameObject);
		}
		mMorphEffect = GetComponent<MorphEffect>();
		if (null != mMorphEffect)
		{
			MorphEffect morphEffect = mMorphEffect;
			morphEffect.mOnSelfDestroy = (MorphEffect.OnSelfDestroy)Delegate.Combine(morphEffect.mOnSelfDestroy, new MorphEffect.OnSelfDestroy(OnEffectDestory));
		}
	}

	private void SelfDestroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnDisable()
	{
		foreach (GameObject mEffectObject in mEffectObjects)
		{
			if (mEffectObject != null)
			{
				VisualEffect component = mEffectObject.GetComponent<VisualEffect>();
				if (component != null)
				{
					component.mOnSelfDestroy = (VisualEffect.OnSelfDestroy)Delegate.Remove(component.mOnSelfDestroy, new VisualEffect.OnSelfDestroy(OnEffectDestory));
				}
			}
		}
		if (mOnDestroy != null)
		{
			mOnDestroy(base.gameObject);
		}
		mOnDestroy = null;
	}

	public void Update()
	{
		if (mEffectsToRemove.Count > 0)
		{
			foreach (GameObject item in mEffectsToRemove)
			{
				if (mEffectObjects.Contains(item))
				{
					mEffectObjects.Remove(item);
				}
			}
			mEffectsToRemove.Clear();
			if (mEffectObjects.Count == 0 && null == mMorphEffect)
			{
				SelfDestroy();
			}
		}
		if (null != mParentTransform)
		{
			Vector3 eulerAngles = mParentTransform.eulerAngles;
			if (mLockXRot)
			{
				eulerAngles.x = 0f;
			}
			if (mLockYRot)
			{
				eulerAngles.y = 0f;
			}
			if (mLockZRot)
			{
				eulerAngles.z = 0f;
			}
			mParentTransform.eulerAngles = eulerAngles;
		}
		if (!mShakeCam)
		{
			return;
		}
		mStartShakeTime -= Time.deltaTime;
		if (mStartShakeTime <= 0f)
		{
			mShakeCam = false;
			GameCamera gameCamera = Camera.main.GetComponent(typeof(GameCamera)) as GameCamera;
			if ((bool)gameCamera)
			{
				gameCamera.MakeShakeEffect(base.gameObject.transform.position);
			}
		}
	}

	public void LockRotation(Transform _parent, bool _x, bool _y, bool _z)
	{
		mParentTransform = _parent;
		mLockXRot = _x;
		mLockYRot = _y;
		mLockZRot = _z;
	}

	public void SetEffectSpeed(float _speed)
	{
		mEffectSpeed = _speed;
	}

	public float GetEffectSpeed()
	{
		return mEffectSpeed;
	}

	public void OnEffectDestory(GameObject _effectObject)
	{
		if (mEffectObjects.Contains(_effectObject))
		{
			mEffectsToRemove.Add(_effectObject);
		}
	}

	public void OnEffectDestory(MorphEffect _effect)
	{
		mMorphEffect = null;
		if (mEffectObjects.Count == 0)
		{
			SelfDestroy();
		}
	}
}
