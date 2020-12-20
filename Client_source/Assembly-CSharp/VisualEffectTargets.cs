using System;
using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffectTargets")]
public class VisualEffectTargets : MonoBehaviour
{
	[Serializable]
	public class Target
	{
		public List<int> mNums = new List<int>();

		public GameObject mGameObject;
	}

	public List<Target> mTargets = new List<Target>();

	public void Awake()
	{
		if (mTargets.Count == base.transform.childCount)
		{
			foreach (Target mTarget in mTargets)
			{
				VisualEffectHolder component = mTarget.mGameObject.GetComponent<VisualEffectHolder>();
				component.mOnDestroy = (VisualEffectHolder.OnDestroy)Delegate.Combine(component.mOnDestroy, new VisualEffectHolder.OnDestroy(OnDestroyHolder));
			}
			return;
		}
		VisualEffectHolder[] componentsInChildren = GetComponentsInChildren<VisualEffectHolder>();
		if (componentsInChildren.Length == 0)
		{
			Log.Error("VisualEffectHolder count is 0 in : " + base.name);
			return;
		}
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			Target target = new Target();
			target.mNums.Add(i);
			target.mGameObject = componentsInChildren[i].gameObject;
			VisualEffectHolder obj = componentsInChildren[i];
			obj.mOnDestroy = (VisualEffectHolder.OnDestroy)Delegate.Combine(obj.mOnDestroy, new VisualEffectHolder.OnDestroy(OnDestroyHolder));
			mTargets.Add(target);
		}
	}

	public void OnDisable()
	{
		if (mTargets.Count == 0)
		{
			return;
		}
		foreach (Target mTarget in mTargets)
		{
			if (!(mTarget.mGameObject == null))
			{
				VisualEffectHolder component = mTarget.mGameObject.GetComponent<VisualEffectHolder>();
				component.mOnDestroy = (VisualEffectHolder.OnDestroy)Delegate.Remove(component.mOnDestroy, new VisualEffectHolder.OnDestroy(OnDestroyHolder));
			}
		}
	}

	public void StopEffect()
	{
		foreach (Target mTarget in mTargets)
		{
			if (!(mTarget.mGameObject == null))
			{
				VisualEffectHolder component = mTarget.mGameObject.GetComponent<VisualEffectHolder>();
				component.StopEffect();
			}
		}
	}

	private void OnDestroyHolder(GameObject _holderObject)
	{
		VisualEffectHolder component = _holderObject.GetComponent<VisualEffectHolder>();
		if (component == null)
		{
			return;
		}
		foreach (Target mTarget in mTargets)
		{
			if (mTarget.mGameObject == _holderObject)
			{
				mTargets.Remove(mTarget);
				break;
			}
		}
		if (mTargets.Count == 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
