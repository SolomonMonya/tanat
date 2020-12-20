using UnityEngine;

public class ObjectTriggerEffect : MonoBehaviour
{
	private enum TriggerState
	{
		IDLE,
		TRIGGER_BEGINED,
		TRIGGER_DONE
	}

	public bool mOnce;

	public string mIdle;

	public string mActionStart;

	public string mActionEnd;

	private GameObject mTriggeredGameObject;

	private TriggerState mTriggerState;

	private bool mAnimationSeted;

	public void Start()
	{
		base.animation.wrapMode = WrapMode.Default;
		AnimationState animationState = null;
		animationState = base.animation[mIdle];
		animationState.layer = 0;
		animationState.wrapMode = WrapMode.Loop;
		animationState = base.animation[mActionStart];
		animationState.layer = 1;
		animationState.wrapMode = WrapMode.ClampForever;
		animationState = base.animation[mActionEnd];
		animationState.layer = 1;
		animationState.wrapMode = WrapMode.Once;
		base.animation.Play(mIdle);
	}

	public void Update()
	{
		if (mTriggerState == TriggerState.TRIGGER_BEGINED && !mAnimationSeted)
		{
			base.animation.CrossFade(mActionStart);
			mAnimationSeted = true;
		}
		else if (mTriggerState == TriggerState.TRIGGER_DONE && !mAnimationSeted && !mOnce)
		{
			base.animation.CrossFade(mActionEnd);
			mAnimationSeted = true;
		}
	}

	public void OnTriggerEnter(Collider _collider)
	{
		if (mTriggeredGameObject == null && _collider.gameObject.layer == LayerMask.NameToLayer("Character"))
		{
			mTriggeredGameObject = _collider.gameObject;
			mTriggerState = TriggerState.TRIGGER_BEGINED;
			mAnimationSeted = false;
		}
	}

	public void OnTriggerExit(Collider _collider)
	{
		if (mTriggeredGameObject == _collider.gameObject && !mOnce)
		{
			mTriggeredGameObject = null;
			mTriggerState = TriggerState.TRIGGER_DONE;
			mAnimationSeted = false;
		}
	}
}
