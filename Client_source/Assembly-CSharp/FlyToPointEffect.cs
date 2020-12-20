using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/FlyToPointEffect")]
public class FlyToPointEffect : MultiTargetsEffect
{
	private Vector3 mTargetPos = Vector3.zero;

	private Vector3 mStartPos = Vector3.zero;

	public float mFlighSpeed;

	private Vector3 mDirection = Vector3.zero;

	private float mDuration;

	private float mCurTime;

	private float mLength;

	public override void Init()
	{
		mDirection = mTargetPos - mStartPos;
		mLength = mDirection.magnitude;
		mDirection.Normalize();
		base.transform.position = mStartPos;
		mDuration = mLength / mFlighSpeed;
		base.transform.forward = mDirection;
	}

	public void Update()
	{
		if (!mDone && mCurTime < mDuration)
		{
			mCurTime += Time.deltaTime;
			base.transform.position = mStartPos + mDirection * mLength * (mCurTime / mDuration);
		}
	}

	public override bool SetTargets(List<Vector3> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count FlyEffect.SetTargets()");
			return false;
		}
		mStartPos = _targets[0];
		mTargetPos = _targets[1];
		return true;
	}
}
