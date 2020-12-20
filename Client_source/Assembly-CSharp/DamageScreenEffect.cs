using System;
using System.Collections;
using TanatKernel;
using UnityEngine;

public class DamageScreenEffect : MonoBehaviour
{
	private GameObject mDamageEffect;

	private GameObject mDeathEffect;

	private Animation mDamageAnimation;

	private AnimationState mDamageAnimationName;

	private Animation mDeathAnimation;

	private AnimationState mDeathAnimationName;

	private AnimationState mRebornAnimationName;

	private SyncedParams mParams;

	private float mLastHealth = 1f;

	private bool mRebornPlaying;

	private bool mInit;

	private bool mIsDead;

	public SyncedParams SyncedParams
	{
		set
		{
			mParams = value;
			if (mParams != null)
			{
				mLastHealth = mParams.HealthProgress;
			}
		}
	}

	private void Start()
	{
		mInit = true;
		mLastHealth = 1f;
		mDamageEffect = GameObject.Find("DamageEffect");
		mDeathEffect = GameObject.Find("DeathEffect");
		if (mDamageEffect == null)
		{
			return;
		}
		mDamageEffect.transform.parent = null;
		mDamageEffect.transform.position = new Vector3(0.5f, 0.5f, 0f);
		GameObjUtil.TrySetParent(mDamageEffect, "effects");
		mDamageAnimation = mDamageEffect.GetComponentInChildren<Animation>();
		IEnumerator enumerator = mDamageAnimation.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				AnimationState animationState = (AnimationState)enumerator.Current;
				animationState.wrapMode = WrapMode.Once;
				animationState.time = 0f;
				animationState.layer = 0;
				mDamageAnimationName = animationState;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		mDamageEffect.active = false;
		if (mDeathEffect == null)
		{
			return;
		}
		mDeathEffect.transform.parent = null;
		mDeathEffect.transform.position = new Vector3(0.5f, 0.5f, 0f);
		GameObjUtil.TrySetParent(mDeathEffect, "effects");
		mDeathAnimation = mDeathEffect.GetComponentInChildren<Animation>();
		bool flag = true;
		foreach (AnimationState item in mDeathAnimation)
		{
			item.wrapMode = WrapMode.Once;
			item.time = 0f;
			item.layer = 0;
			if (flag)
			{
				mDeathAnimationName = item;
			}
			else
			{
				mRebornAnimationName = item;
			}
			flag = !flag;
		}
		mDeathEffect.active = false;
	}

	public void StartDamage()
	{
		if (mDamageEffect != null)
		{
			mDamageEffect.active = true;
			int num = ((UnityEngine.Random.Range(0, 100) <= 50) ? 1 : (-1));
			int num2 = ((UnityEngine.Random.Range(0, 100) <= 50) ? 1 : (-1));
			mDamageEffect.transform.localScale = new Vector3((1f + 0.3f * mLastHealth) * (float)num, (1f + 0.3f * mLastHealth) * (float)num2, 1f);
			mDamageAnimationName.time = 0f;
			mDamageAnimation.CrossFadeQueued(mDamageAnimationName.name, 0f, QueueMode.PlayNow);
		}
	}

	private void Update()
	{
		if (!OptionsMgr.mShowDamageEffect)
		{
			return;
		}
		if (mInit && mParams != null)
		{
			mLastHealth = mParams.HealthProgress;
			if (mParams.HealthProgress > 0f)
			{
				mInit = false;
			}
			return;
		}
		if (mDamageEffect != null && mParams != null && mDeathEffect != null)
		{
			if (mDamageAnimation != null && !mDamageAnimation.IsPlaying(mDamageAnimationName.name) && mParams.HealthProgress > 0f && mParams.HealthProgress < mLastHealth)
			{
				StartDamage();
			}
			if (!mIsDead && mParams.HealthProgress == 0f)
			{
				mIsDead = true;
				mDeathEffect.active = true;
				mDeathAnimationName.time = 0f;
				mDeathAnimation.CrossFade(mDeathAnimationName.name, 0f, PlayMode.StopAll);
			}
			if (mIsDead && mParams.HealthProgress > 0f)
			{
				mIsDead = false;
				mDeathEffect.active = true;
				mRebornPlaying = true;
				mRebornAnimationName.time = 0f;
				mDeathAnimation.CrossFade(mRebornAnimationName.name, 0f, PlayMode.StopAll);
			}
			mLastHealth = mParams.HealthProgress;
		}
		if (mRebornPlaying && !mDeathAnimation.IsPlaying(mRebornAnimationName.name))
		{
			mDeathEffect.active = false;
			mRebornPlaying = false;
		}
	}
}
