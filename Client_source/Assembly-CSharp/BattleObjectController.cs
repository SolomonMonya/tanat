using UnityEngine;

[AddComponentMenu("MainMenu/BattleObjectController")]
public class BattleObjectController : MonoBehaviour
{
	private Animation mAnimation;

	private float mBlendTime;

	public void Awake()
	{
		mAnimation = GetComponentInChildren<Animation>();
		ResetAnimations();
	}

	public void PlayAnim(string _anim)
	{
		if (!(mAnimation == null))
		{
			mAnimation.CrossFade(_anim, mBlendTime, PlayMode.StopSameLayer);
		}
	}

	public float GetAnimTime(string _anim)
	{
		if (mAnimation == null)
		{
			return 0f;
		}
		AnimationState animationState = mAnimation[_anim];
		if (animationState == null)
		{
			return 0f;
		}
		return animationState.length;
	}

	public void SetBlendTime(float _blendTime)
	{
		mBlendTime = _blendTime;
	}

	private void ResetAnimations()
	{
		if (mAnimation == null)
		{
			return;
		}
		foreach (AnimationState item in mAnimation)
		{
			item.wrapMode = WrapMode.Once;
			item.time = 0f;
			item.layer = 0;
		}
	}
}
