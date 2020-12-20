using UnityEngine;

[AddComponentMenu("FXs/RefractionEffect")]
public class RefractionEffect : MonoBehaviour
{
	public float mDuration;

	public float mTimeForMaxDistortion;

	public float mMaxDistortion;

	public Vector3 mScaleSpeed = Vector3.zero;

	public float mTimeStart;

	private float mCurDistortion;

	private float mCurTime;

	private Vector3 mTargetScale = Vector3.zero;

	public bool mDone;

	private void Start()
	{
		mTargetScale = base.transform.localScale + mScaleSpeed * mDuration * 100f;
	}

	private void Update()
	{
		if (null == base.renderer || null == base.renderer.material || mDone)
		{
			return;
		}
		if (mTimeStart > 0f)
		{
			mTimeStart -= Time.deltaTime;
			if (mTimeStart < 0f)
			{
				mTimeStart = 0f;
			}
		}
		else if (mTimeStart == 0f && mCurTime < mDuration)
		{
			mCurTime += Time.deltaTime;
			if (mCurTime > mDuration)
			{
				mCurTime = mDuration;
			}
			if (mScaleSpeed != Vector3.zero)
			{
				base.transform.localScale = mTargetScale * (mCurTime / mDuration);
			}
			if (mCurTime < mTimeForMaxDistortion)
			{
				mCurDistortion = mCurTime / mTimeForMaxDistortion * mMaxDistortion;
				base.renderer.material.SetFloat("_BumpAmt", mCurDistortion);
			}
			else if (mCurTime >= mDuration - mTimeForMaxDistortion)
			{
				mCurDistortion = (mDuration - mCurTime) / mTimeForMaxDistortion * mMaxDistortion;
				base.renderer.material.SetFloat("_BumpAmt", mCurDistortion);
			}
		}
		else if (mTimeStart == 0f && mCurTime >= mDuration && !mDone)
		{
			mDone = true;
		}
	}
}
