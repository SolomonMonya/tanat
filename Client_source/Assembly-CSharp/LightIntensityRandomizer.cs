using UnityEngine;

[AddComponentMenu("FXs/LightIntensityRandomizer")]
public class LightIntensityRandomizer : MonoBehaviour
{
	public float mSpeed = 0.5f;

	public float mRandSpeed = 0.15f;

	public float mIntensityRange = 0.2f;

	private bool mDir;

	private float mStartIntensity;

	private float mCurSpeed;

	private float mCurMinIntensity;

	private float mCurMaxIntensity;

	public void Start()
	{
		if (base.gameObject.light == null)
		{
			Object.Destroy(this);
		}
		mStartIntensity = base.gameObject.light.intensity;
		Reset();
	}

	public void Update()
	{
		float intensity = base.gameObject.light.intensity;
		float num = mCurSpeed * Time.deltaTime;
		if (mDir)
		{
			intensity += num;
			if (intensity > mCurMaxIntensity)
			{
				intensity = mCurMaxIntensity;
				Reset();
			}
		}
		else
		{
			intensity -= num;
			if (intensity < mCurMinIntensity)
			{
				intensity = mCurMinIntensity;
				Reset();
			}
		}
		base.gameObject.light.intensity = intensity;
	}

	private void Reset()
	{
		mDir = !mDir;
		mCurMaxIntensity = mStartIntensity + Random.Range(0f, mIntensityRange);
		mCurMinIntensity = mStartIntensity - Random.Range(0f, mIntensityRange);
		mCurSpeed = mSpeed - mRandSpeed + Random.Range(0f, mRandSpeed * 2f);
	}
}
