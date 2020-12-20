using UnityEngine;

[AddComponentMenu("FXs/MorphEffect")]
public class MorphEffect : MonoBehaviour
{
	private enum MorphType
	{
		MORPH_NONE,
		MORPH_TO,
		MORPH_FROM
	}

	public delegate void OnSelfDestroy(MorphEffect _effect);

	public Vector3 mTargetScale = Vector3.zero;

	public Vector3 mStartScale = Vector3.zero;

	public float mTimeForScale;

	public float mRunSpeed = 6f;

	private float mStartRunSpeed = 6f;

	private AnimationExt mAnimationExt;

	private GameObject mObject;

	private Vector3 mDeltaScale = Vector3.zero;

	private float mStartScaleTime;

	private MorphType mMorphType;

	public OnSelfDestroy mOnSelfDestroy;

	public void Start()
	{
		mAnimationExt = base.transform.parent.transform.gameObject.GetComponent<AnimationExt>();
		mStartRunSpeed = mAnimationExt.mRunSpeed;
	}

	public void Update()
	{
		if (null == mObject || mMorphType == MorphType.MORPH_NONE)
		{
			return;
		}
		if (Time.time - mStartScaleTime < mTimeForScale)
		{
			float num = (Time.time - mStartScaleTime) / mTimeForScale;
			num = ((!(num <= 1f)) ? 1f : num);
			mObject.transform.localScale = ((mMorphType != MorphType.MORPH_TO) ? (mTargetScale + mDeltaScale * num) : mStartScale);
		}
		else if (mMorphType == MorphType.MORPH_TO)
		{
			mObject.transform.localScale = mTargetScale;
			mAnimationExt.mRunSpeed = mRunSpeed;
		}
		else if (mMorphType == MorphType.MORPH_FROM)
		{
			mAnimationExt.mRunSpeed = mStartRunSpeed;
			mObject.transform.localScale = mStartScale;
			if (mOnSelfDestroy != null)
			{
				mOnSelfDestroy(this);
			}
		}
	}

	public void StartMorph(GameObject _object)
	{
		mMorphType = MorphType.MORPH_TO;
		mObject = _object;
		mDeltaScale = mTargetScale - mStartScale;
		mStartScaleTime = Time.time;
	}

	public void EndMorph()
	{
		mMorphType = MorphType.MORPH_FROM;
		mDeltaScale = mStartScale - mTargetScale;
		mStartScaleTime = Time.time;
	}

	public void EndMorphImmediate()
	{
		mMorphType = MorphType.MORPH_NONE;
		mObject.transform.localScale = mStartScale;
	}
}
