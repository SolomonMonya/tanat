using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/ProjectileBurst")]
public class ProjectileBurst : MultiTargetsEffect
{
	public GameObject mProjectilePrefab;

	public float mInterval;

	public float mSpeed;

	public Vector3 mOffset;

	public float mDelay;

	private Vector3 mDirection;

	private float mDistance;

	private Vector3 mStartPos;

	private float mCurDeltaTime;

	private float mCurDelay;

	public void Start()
	{
		mCurDelay = mDelay;
	}

	public void Update()
	{
		if (mProjectilePrefab == null)
		{
			return;
		}
		if (mCurDelay > 0f)
		{
			mCurDelay -= Time.deltaTime;
			if (mCurDelay <= 0f)
			{
				Shot();
			}
		}
		if (mCurDelay <= 0f)
		{
			mCurDeltaTime += Time.deltaTime;
			if (mCurDeltaTime >= mInterval)
			{
				mCurDeltaTime = 0f;
				Shot();
			}
		}
	}

	public override bool SetTargets(List<Vector3> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count ProjectileBurst.SetTargets()");
			return false;
		}
		mStartPos = _targets[0];
		mDirection = (_targets[1] - _targets[0]).normalized;
		mDistance = Vector3.Distance(_targets[0], _targets[1]);
		return true;
	}

	private void Shot()
	{
		GameObject gameObject = Object.Instantiate(mProjectilePrefab) as GameObject;
		gameObject.transform.position = mStartPos + mOffset;
		MoveObject component = gameObject.GetComponent<MoveObject>();
		component.mDirection = mDirection;
		component.mMaxDistance = mDistance;
		component.mSpeed = mSpeed;
		GameObjUtil.TrySetParent(gameObject, "/effects");
	}
}
