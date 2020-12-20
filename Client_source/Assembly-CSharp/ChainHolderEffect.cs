using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/ChainHolderEffect")]
public class ChainHolderEffect : MultiTargetsEffect
{
	public string mBossAnimName;

	public string mHolderAnimName;

	public WrapMode mBossAnimWrapMode;

	private GameObject mBossObject;

	private GameObject mHolderObject;

	private AnimationState mBossAnim;

	private AnimationState mHolderAnim;

	private float mDeltaAnim = 0.03f;

	public override void Init()
	{
		AnimationExt componentInChildren = mBossObject.GetComponentInChildren<AnimationExt>();
		Animation componentInChildren2 = mBossObject.GetComponentInChildren<Animation>();
		Animation componentInChildren3 = mHolderObject.GetComponentInChildren<Animation>();
		mBossObject.transform.forward = mHolderObject.transform.forward;
		if (componentInChildren2 != null)
		{
			componentInChildren.SetWrapMode(mBossAnimName, mBossAnimWrapMode);
			componentInChildren.PlayAnimation(mBossAnimName);
			mBossAnim = componentInChildren2[mBossAnimName];
		}
		if (componentInChildren3 != null)
		{
			componentInChildren3.Play(mHolderAnimName);
			mHolderAnim = componentInChildren3[mHolderAnimName];
		}
	}

	public void Update()
	{
		if (mHolderAnim != null && mBossAnim != null && Mathf.Abs(mHolderAnim.time - mBossAnim.time) > mDeltaAnim)
		{
			mHolderAnim.time = mBossAnim.time;
		}
		if (mBossObject != null && mHolderObject != null && mBossObject.transform.forward != mHolderObject.transform.forward)
		{
			mBossObject.transform.forward = mHolderObject.transform.forward;
		}
	}

	public override bool SetTargets(List<GameObject> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count SmoothMove.SetTargets()");
			return false;
		}
		mBossObject = _targets[0];
		mHolderObject = _targets[1];
		return true;
	}
}
