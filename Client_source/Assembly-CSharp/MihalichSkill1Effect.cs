using UnityEngine;

[AddComponentMenu("FXs/MihalichSkill1Effect")]
public class MihalichSkill1Effect : MonoBehaviour
{
	public GameObject mStartEffect;

	public GameObject mEndEffect;

	public GameObject mChainEffect;

	private SmoothMove mSmoothMove;

	private bool mToBack;

	private Lightning mLightning;

	public void Awake()
	{
		mStartEffect.SetActiveRecursively(state: false);
		mEndEffect.SetActiveRecursively(state: false);
	}

	public void Start()
	{
		mSmoothMove = GetComponent<SmoothMove>();
		mStartEffect.SetActiveRecursively(state: true);
		mStartEffect.animation.Play();
		if (mChainEffect != null)
		{
			mLightning = mChainEffect.GetComponent<Lightning>();
			mLightning.SetEndTransform(base.transform);
			mLightning.Init();
		}
	}

	public void Update()
	{
		if (mSmoothMove.mDone && !mToBack)
		{
			if (mStartEffect != null)
			{
				mStartEffect.SetActiveRecursively(state: false);
			}
			if (mEndEffect != null)
			{
				mEndEffect.SetActiveRecursively(state: true);
				mEndEffect.animation.Play();
			}
			mToBack = true;
		}
	}
}
