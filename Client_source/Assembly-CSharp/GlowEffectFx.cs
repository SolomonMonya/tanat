using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("FXs/GlowEffectFx")]
public class GlowEffectFx : MonoBehaviour
{
	public float mDuration;

	public float mStartDuration;

	public float mEndDuration;

	public Color mTargetColor;

	public float mTargetIntensity;

	public float mMaxDistance = 20f;

	private GlowEffect mGlowEffect;

	private Color mStartGlowColor;

	private float mStartIntensity;

	private float mBeginTime;

	private float mPowerK;

	private void Start()
	{
		mGlowEffect = Camera.main.GetComponent<GlowEffect>();
		GameCamera component = Camera.main.GetComponent<GameCamera>();
		if (mGlowEffect == null || component == null)
		{
			Object.Destroy(base.gameObject);
		}
		float magnitude = (base.transform.position - component.GetCamLookPoint()).magnitude;
		mPowerK = ((!(magnitude <= mMaxDistance)) ? 0f : (magnitude / mMaxDistance));
		if (mPowerK == 0f)
		{
			Object.Destroy(base.gameObject);
		}
		mTargetIntensity *= mPowerK;
		mStartGlowColor = mGlowEffect.glowTint;
		mStartIntensity = mGlowEffect.glowIntensity;
		mBeginTime = 0f;
	}

	private void OnDestroy()
	{
		mGlowEffect.glowTint = mStartGlowColor;
		mGlowEffect.glowIntensity = mStartIntensity;
	}

	private void Update()
	{
		if (mBeginTime == 0f)
		{
			mBeginTime = Time.time;
			return;
		}
		float num = Time.time - mBeginTime;
		if (num < mDuration)
		{
			if (num <= mStartDuration)
			{
				float t = num / mStartDuration;
				mGlowEffect.glowTint = Color.Lerp(mStartGlowColor, mTargetColor, t);
				mGlowEffect.glowIntensity = Mathf.Lerp(mStartIntensity, mTargetIntensity, t);
			}
			else if (num < mDuration - mEndDuration && num > mStartDuration)
			{
				mGlowEffect.glowTint = mTargetColor;
				mGlowEffect.glowIntensity = mTargetIntensity;
			}
			else if (num >= mDuration - mEndDuration)
			{
				float t2 = 1f - (mDuration - num) / mEndDuration;
				mGlowEffect.glowTint = Color.Lerp(mTargetColor, mStartGlowColor, t2);
				mGlowEffect.glowIntensity = Mathf.Lerp(mTargetIntensity, mStartIntensity, t2);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
