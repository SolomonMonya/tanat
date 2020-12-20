using UnityEngine;

[AddComponentMenu("FXs/FogZone")]
internal class FogZone : MonoBehaviour
{
	public Color mTargetColor = Color.white;

	public float mLerpTime;

	public float mTargetDensity;

	private Color mStartColor = Color.clear;

	private Color mCurColor = Color.clear;

	private float mCurLerpTime;

	private float mStartDensity;

	private float mCurDensity;

	public void Start()
	{
		mCurLerpTime = mLerpTime;
		base.gameObject.layer = 2;
	}

	public void Update()
	{
		if (mCurLerpTime < mLerpTime)
		{
			mCurLerpTime += Time.deltaTime;
			if (mCurLerpTime > mLerpTime)
			{
				mCurLerpTime = mLerpTime;
			}
			float t = mCurLerpTime / mLerpTime;
			mCurColor = Color.Lerp(mStartColor, mTargetColor, t);
			mCurDensity = Mathf.Lerp(mStartDensity, mTargetDensity, t);
			RenderSettings.fogColor = mCurColor;
			RenderSettings.fogDensity = mCurDensity;
		}
	}

	public void OnTriggerEnter(Collider _collider)
	{
		mStartColor = RenderSettings.fogColor;
		mStartDensity = RenderSettings.fogDensity;
		mCurLerpTime = 0f;
	}

	public void OnTriggerExit(Collider _collider)
	{
		mCurLerpTime = mLerpTime;
		mStartColor = Color.clear;
		mCurColor = Color.clear;
	}
}
