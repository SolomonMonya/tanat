using UnityEngine;

[AddComponentMenu("FXs/ProjectorEffect")]
public class ProjectorEffect : MonoBehaviour
{
	public float mDuration;

	public float mFromAlphaTime;

	public float mToAlphaTime;

	public Color mFromAlphaColor = Color.clear;

	public Color mToAlphaColor = Color.clear;

	private float mCurTime;

	private Projector mProjector;

	private Color mStartColor;

	public float mTimeStart;

	private void Start()
	{
		mProjector = GetComponent<Projector>();
		Material material = new Material(mProjector.material);
		mProjector.material = material;
		mCurTime = 0f;
		GameObjUtil.TrySetParent(base.gameObject, "/effects");
		mStartColor = mProjector.material.color;
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
				mProjector.material.color = Color.Lerp(mStartColor, mFromAlphaColor, t);
			}
			else if (mCurTime >= mDuration - mToAlphaTime)
			{
				float t2 = 1f - (mDuration - mCurTime) / mToAlphaTime;
				mProjector.material.color = Color.Lerp(mFromAlphaColor, mToAlphaColor, t2);
			}
		}
		else if (mTimeStart == 0f && mCurTime >= mDuration)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
