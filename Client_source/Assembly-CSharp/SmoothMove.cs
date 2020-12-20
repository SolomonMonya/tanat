using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/SmoothMove")]
public class SmoothMove : MultiTargetsEffect
{
	public float mSpeed = 0.1f;

	public float mTargetTime;

	public Vector3 mOffset = Vector3.zero;

	public float mMaxDistance = 10f;

	public float mMaxHeight = 2f;

	public float mFromAlphaK = 1f;

	public bool mDestroyOnDone = true;

	public bool mBySpeed = true;

	private Vector3 mDirection = Vector3.zero;

	private Vector3 mCurPos = Vector3.zero;

	private Transform mEnd;

	private bool mToAlpha;

	private float mMinDeltaPos;

	private float mCurTime;

	private float mCurMaxDistance;

	private float mCurDistance;

	private Vector3 mABC;

	private Vector3 mCurStartPos;

	private Vector3 mCurEndPos;

	private float mWaitTrailsStart = -1f;

	public void Awake()
	{
		mMinDeltaPos = 0.05f;
		mCurTime = 0f;
		mCurMaxDistance = 0f;
		mCurDistance = 0f;
	}

	public void Update()
	{
		if (mDone && mDestroyOnDone)
		{
			return;
		}
		if (!mToAlpha)
		{
			Move();
		}
		else
		{
			mDone = true;
			ParticleEmitter[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleEmitter>();
			ParticleEmitter[] array = componentsInChildren;
			foreach (ParticleEmitter particleEmitter in array)
			{
				if (particleEmitter.particleCount > 0)
				{
					mDone = false;
					break;
				}
			}
			if (mDone)
			{
				TrailRenderer[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<TrailRenderer>();
				if (componentsInChildren2.Length == 0)
				{
					mDone = true;
				}
				else
				{
					if (mWaitTrailsStart < 0f)
					{
						mWaitTrailsStart = Time.realtimeSinceStartup;
					}
					float num = 0f;
					TrailRenderer[] array2 = componentsInChildren2;
					foreach (TrailRenderer trailRenderer in array2)
					{
						if (trailRenderer.time > num)
						{
							num = trailRenderer.time;
						}
					}
					float num2 = Time.realtimeSinceStartup - mWaitTrailsStart;
					mDone = num2 > num;
				}
			}
		}
		if (mDone && mDestroyOnDone)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override bool SetTargets(List<GameObject> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count SmoothMove.SetTargets()");
			return false;
		}
		mEnd = _targets[1].transform;
		mCurStartPos = _targets[0].transform.position;
		mCurEndPos = mEnd.position;
		if (mBySpeed)
		{
			mTargetTime = Vector3.Distance(mCurStartPos, mCurEndPos) / mSpeed;
		}
		else
		{
			mSpeed = Vector3.Distance(mCurStartPos, mCurEndPos) / mTargetTime;
		}
		mDirection = (mCurEndPos - mCurStartPos).normalized;
		mABC = ParabolicMove.GetParabolicABC(mCurStartPos, mCurEndPos, mMaxHeight, mMaxDistance);
		base.transform.forward = mDirection;
		base.transform.position = mCurStartPos;
		return true;
	}

	private void UpdateParams()
	{
		if (Vector3.Distance(mCurEndPos, mEnd.position) > mMinDeltaPos)
		{
			mCurEndPos = mEnd.position;
			mABC = ParabolicMove.GetParabolicABC(mCurStartPos, mCurEndPos, mMaxHeight, mMaxDistance);
		}
		mDirection = (mCurEndPos - mCurStartPos).normalized;
		mCurMaxDistance = Vector3.Distance(mCurStartPos, mCurEndPos);
		mCurTime += Time.deltaTime;
		if (mCurTime > mTargetTime)
		{
			mCurTime = mTargetTime;
		}
		if (mTargetTime == 0f)
		{
			mCurDistance = mCurMaxDistance;
		}
		else
		{
			mCurDistance = mCurMaxDistance * (mCurTime / mTargetTime);
		}
		base.transform.forward = mDirection;
		mCurDistance = mCurMaxDistance * (mCurTime / mTargetTime);
		mCurPos = mCurStartPos + mDirection * mCurDistance;
	}

	private void Move()
	{
		if (mEnd == null)
		{
			ToAlpha();
			return;
		}
		UpdateParams();
		base.transform.position = ParabolicMove.GetParabolicPosByLength(mCurPos, mABC) + mOffset;
		if (Mathf.Abs(mCurDistance - mCurMaxDistance) <= mMinDeltaPos)
		{
			ToAlpha();
		}
	}

	private void ToAlpha()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.enabled = false;
		}
		TrailRenderer[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<TrailRenderer>();
		TrailRenderer[] array2 = componentsInChildren2;
		foreach (TrailRenderer trailRenderer in array2)
		{
			trailRenderer.autodestruct = true;
		}
		ParticleEmitter[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<ParticleEmitter>();
		ParticleEmitter[] array3 = componentsInChildren3;
		foreach (ParticleEmitter particleEmitter in array3)
		{
			particleEmitter.emit = false;
		}
		mToAlpha = true;
	}
}
