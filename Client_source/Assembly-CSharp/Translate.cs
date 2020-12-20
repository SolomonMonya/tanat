using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/Translate")]
internal class Translate : MultiTargetsEffect
{
	public float mTime;

	private GameObject mCurObject;

	private GameObject mTargetObject;

	private Vector3 mDirection = Vector3.zero;

	private Vector3 mStartPos = Vector3.zero;

	private float mCurTime;

	private float mDistance;

	private NetSyncTransform mNetSyncTransform;

	public override void Init()
	{
		if (!(null == mCurObject) && !(null == mTargetObject))
		{
			Vector3 vector = mTargetObject.transform.position - mCurObject.transform.position;
			mDirection = vector.normalized;
			mDistance = vector.magnitude;
			mStartPos = mCurObject.transform.position;
		}
	}

	public void Start()
	{
		mNetSyncTransform = mCurObject.GetComponent<NetSyncTransform>();
		if (null != mNetSyncTransform)
		{
			mNetSyncTransform.enabled = false;
		}
	}

	public void Update()
	{
		if (mDone)
		{
			return;
		}
		if (mCurTime < mTime)
		{
			mCurTime += Time.deltaTime;
			if (mCurTime > mTime)
			{
				mCurTime = mTime;
			}
			mCurObject.transform.position = mStartPos + mCurTime / mTime * mDirection * mDistance;
		}
		else
		{
			mDone = true;
			if (null != mNetSyncTransform)
			{
				mNetSyncTransform.enabled = true;
			}
		}
	}

	public override bool SetTargets(List<GameObject> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count in Translate.SetTargets()");
			return false;
		}
		mCurObject = _targets[0];
		mTargetObject = _targets[1];
		return true;
	}
}
