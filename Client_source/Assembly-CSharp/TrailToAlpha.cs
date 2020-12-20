using UnityEngine;

[AddComponentMenu("FXs/TrailToAlpha")]
public class TrailToAlpha : MonoBehaviour
{
	public float mDuration;

	public float mFromAlphaTime;

	public float mToAlphaTime;

	public Color mFromAlphaColor = Color.clear;

	public Color mToAlphaColor = Color.clear;

	private float mCurTime;

	private TrailRenderer mProjector;

	private Color mStartColor;

	public float mTimeStart;

	public string mColorName = "_TintColor";

	private void Start()
	{
		mProjector = GetComponent(typeof(TrailRenderer)) as TrailRenderer;
		mCurTime = 0f;
		mStartColor = mProjector.material.GetColor(mColorName);
	}

	private void Update()
	{
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
			if (mCurTime < mFromAlphaTime)
			{
				float t = mCurTime / mFromAlphaTime;
				Color color = mProjector.material.GetColor(mColorName);
				color = Color.Lerp(mStartColor, mFromAlphaColor, t);
				mProjector.material.SetColor(mColorName, color);
			}
			else if (mCurTime >= mDuration - mToAlphaTime)
			{
				float t2 = 1f - (mDuration - mCurTime) / mToAlphaTime;
				Color color2 = mProjector.material.GetColor(mColorName);
				color2 = Color.Lerp(mFromAlphaColor, mToAlphaColor, t2);
				mProjector.material.SetColor(mColorName, color2);
			}
		}
		else if (mTimeStart == 0f && mCurTime >= mDuration)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
