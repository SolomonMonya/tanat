using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/FlyEffect")]
public class FlyEffect : MultiTargetsEffect
{
	private Vector3 mTargetPos = Vector3.zero;

	private Vector3 mStartPos = Vector3.zero;

	public float mLength;

	public float mFlighSpeed;

	public float mDeltaHeight;

	private Vector3 mDirection = Vector3.zero;

	private float mDuration;

	private float mCurTime;

	private Renderer mRenderer;

	private Projector mProjector;

	public override void Init()
	{
		mTargetPos.y += mDeltaHeight;
		mDirection = mTargetPos - mStartPos;
		mDirection.Normalize();
		base.transform.position = mStartPos;
		mDuration = mLength / mFlighSpeed;
		base.transform.forward = mDirection;
		mRenderer = base.gameObject.GetComponentInChildren<Renderer>();
		mProjector = base.gameObject.GetComponentInChildren<Projector>();
	}

	public void Update()
	{
		if (mDone)
		{
			return;
		}
		if (mCurTime < mDuration)
		{
			mCurTime += Time.deltaTime;
			base.transform.position = mStartPos + mDirection * mLength * (mCurTime / mDuration);
		}
		else if (!mDone)
		{
			if (mRenderer != null)
			{
				mRenderer.enabled = false;
			}
			if (mProjector != null)
			{
				mProjector.enabled = false;
			}
			mDone = true;
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
