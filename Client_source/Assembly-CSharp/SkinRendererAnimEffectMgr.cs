using UnityEngine;

[AddComponentMenu("FXs/SkinRendererAnimEffectMgr")]
public class SkinRendererAnimEffectMgr : MonoBehaviour
{
	public string mCurAnimName;

	public WrapMode mWrapMode = WrapMode.Once;

	public bool mDisableRenderer;

	public int mLayer;

	public string mAnimationObjName;

	public string mRendererObjName;

	private Animation mCurAnimation;

	private SkinnedMeshRenderer mCurRenderer;

	public void Start()
	{
		GameObject objectChildSafe = VisualEffectsMgr.GetObjectChildSafe(mAnimationObjName, base.transform.parent.gameObject);
		GameObject objectChildSafe2 = VisualEffectsMgr.GetObjectChildSafe(mRendererObjName, base.transform.parent.gameObject);
		if (objectChildSafe == null || objectChildSafe2 == null)
		{
			Done();
			return;
		}
		mCurAnimation = objectChildSafe.GetComponent<Animation>();
		mCurRenderer = objectChildSafe2.GetComponent<SkinnedMeshRenderer>();
		if (mCurAnimation == null || mCurRenderer == null)
		{
			Done();
		}
		else
		{
			PlayAnimation();
		}
	}

	public void Update()
	{
		AnimationState animationState = mCurAnimation[mCurAnimName];
		if (animationState.time >= animationState.length && mWrapMode != WrapMode.Loop)
		{
			Done();
		}
	}

	public void OnDisable()
	{
		if (mDisableRenderer && mCurRenderer != null)
		{
			mCurRenderer.enabled = false;
		}
	}

	private void PlayAnimation()
	{
		if (mCurRenderer != null)
		{
			mCurRenderer.enabled = true;
		}
		AnimationState animationState = mCurAnimation[mCurAnimName];
		if (animationState == null)
		{
			Done();
			return;
		}
		animationState.layer = mLayer;
		animationState.time = 0f;
		animationState.wrapMode = mWrapMode;
		mCurAnimation.CrossFade(mCurAnimName);
	}

	private void Done()
	{
		Object.Destroy(base.gameObject);
	}
}
