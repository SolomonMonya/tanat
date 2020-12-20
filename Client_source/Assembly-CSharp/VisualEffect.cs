using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffect")]
public class VisualEffect : MonoBehaviour
{
	private class KeyOptions
	{
		public Color mColor = Color.clear;

		public Vector2 mTextureOffsets = Vector2.zero;

		public bool mEmitParticles;

		public KeyOptions()
		{
			mColor = Color.clear;
			mTextureOffsets = Vector2.zero;
			mEmitParticles = false;
		}
	}

	public delegate void OnSelfDestroy(GameObject _effectObject);

	public float mMaxDuration;

	public bool mLoopEffect;

	public OnSelfDestroy mOnSelfDestroy;

	private VisualEffectKeys mKeys;

	private MeshRenderer mMeshRenderer;

	private SkinnedMeshRenderer mSkinnedMeshRenderer;

	private RefractionEffect mRefractionEffect;

	private MultiTargetsEffect mMultiTargetsEffect;

	private ParticleEmitter mCurParticleEmmiter;

	private float mEffectSpeed = 1f;

	private float mCurDuration;

	public void Start()
	{
		mKeys = GetComponent<VisualEffectKeys>();
		mMeshRenderer = GetComponent<MeshRenderer>();
		mSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		mCurParticleEmmiter = GetComponent<ParticleEmitter>();
		mRefractionEffect = GetComponent<RefractionEffect>();
		mMultiTargetsEffect = GetComponent<MultiTargetsEffect>();
		if (null != base.transform.parent.gameObject.animation && mMaxDuration == 0f)
		{
			if (base.transform.parent.gameObject.animation.clip == null)
			{
				Log.Error("null clip in animation : " + base.transform.parent.gameObject.name);
			}
			else
			{
				mMaxDuration = base.transform.parent.gameObject.animation.clip.length;
			}
		}
	}

	public void Update()
	{
		if (mCurDuration < mMaxDuration || mLoopEffect)
		{
			if (mCurDuration == mMaxDuration)
			{
				mCurDuration = 0f;
			}
			mCurDuration += Time.deltaTime * mEffectSpeed;
			if (mCurDuration > mMaxDuration)
			{
				mCurDuration = mMaxDuration;
			}
			KeyOptions keyOptionsByTime = GetKeyOptionsByTime(mCurDuration);
			if (keyOptionsByTime != null)
			{
				if (null != mMeshRenderer)
				{
					mMeshRenderer.material.SetColor("_TintColor", keyOptionsByTime.mColor);
					mMeshRenderer.material.SetTextureOffset("_MainTex", keyOptionsByTime.mTextureOffsets);
				}
				if (null != mSkinnedMeshRenderer)
				{
					mSkinnedMeshRenderer.material.SetColor("_TintColor", keyOptionsByTime.mColor);
					mSkinnedMeshRenderer.material.SetTextureOffset("_MainTex", keyOptionsByTime.mTextureOffsets);
				}
				if (null != mCurParticleEmmiter)
				{
					mCurParticleEmmiter.emit = keyOptionsByTime.mEmitParticles;
				}
			}
		}
		else if (null != mCurParticleEmmiter)
		{
			if (mCurParticleEmmiter.emit)
			{
				mCurParticleEmmiter.emit = false;
			}
			if (mCurParticleEmmiter.particleCount == 0)
			{
				SelfDestroy();
			}
		}
		else if (null != mRefractionEffect)
		{
			if (mRefractionEffect.mDone)
			{
				SelfDestroy();
			}
		}
		else if (null != mMultiTargetsEffect)
		{
			if (mMultiTargetsEffect.mDone)
			{
				SelfDestroy();
			}
		}
		else
		{
			SelfDestroy();
		}
	}

	private void SelfDestroy()
	{
		Object.Destroy(base.gameObject);
	}

	public void OnDisable()
	{
		if (mOnSelfDestroy != null)
		{
			mOnSelfDestroy(base.gameObject);
		}
		mOnSelfDestroy = null;
	}

	private KeyOptions GetKeyOptionsByTime(float _curAnimTime)
	{
		if (null == mKeys)
		{
			return null;
		}
		KeyOptions keyOptions = new KeyOptions();
		float num = 0f;
		float num2 = 0f;
		if ((bool)mCurParticleEmmiter)
		{
			keyOptions.mEmitParticles = mCurParticleEmmiter.emit;
		}
		int i = 0;
		for (int keysCount = mKeys.GetKeysCount(); i < keysCount; i++)
		{
			if (mKeys.mVisualEffectKeys[i].mTime >= _curAnimTime)
			{
				if (i == 0)
				{
					num2 = mKeys.mVisualEffectKeys[i].mTime;
					num = _curAnimTime;
					keyOptions.mColor = mKeys.mVisualEffectKeys[i].mColor / num2;
					keyOptions.mTextureOffsets.x = mKeys.mVisualEffectKeys[i].mTextureOffsetX / num2;
					keyOptions.mTextureOffsets.y = mKeys.mVisualEffectKeys[i].mTextureOffsetY / num2;
					keyOptions.mColor *= num;
					keyOptions.mTextureOffsets *= num;
				}
				else
				{
					num2 = mKeys.mVisualEffectKeys[i].mTime - mKeys.mVisualEffectKeys[i - 1].mTime;
					num = _curAnimTime - mKeys.mVisualEffectKeys[i - 1].mTime;
					keyOptions.mColor = (mKeys.mVisualEffectKeys[i].mColor - mKeys.mVisualEffectKeys[i - 1].mColor) / num2;
					keyOptions.mTextureOffsets.x = (mKeys.mVisualEffectKeys[i].mTextureOffsetX - mKeys.mVisualEffectKeys[i - 1].mTextureOffsetX) / num2;
					keyOptions.mTextureOffsets.y = (mKeys.mVisualEffectKeys[i].mTextureOffsetY - mKeys.mVisualEffectKeys[i - 1].mTextureOffsetY) / num2;
					keyOptions.mColor = mKeys.mVisualEffectKeys[i - 1].mColor + keyOptions.mColor * num;
					keyOptions.mTextureOffsets.x = mKeys.mVisualEffectKeys[i - 1].mTextureOffsetX + keyOptions.mTextureOffsets.x * num;
					keyOptions.mTextureOffsets.y = mKeys.mVisualEffectKeys[i - 1].mTextureOffsetY + keyOptions.mTextureOffsets.y * num;
				}
				return keyOptions;
			}
			keyOptions.mEmitParticles = mKeys.mVisualEffectKeys[i].mEmitParticles;
		}
		return keyOptions;
	}

	public void StopEffect()
	{
		mCurDuration = mMaxDuration;
		mLoopEffect = false;
		if ((bool)mCurParticleEmmiter)
		{
			mCurParticleEmmiter.emit = false;
		}
		if ((bool)mMultiTargetsEffect)
		{
			mMultiTargetsEffect.mDone = true;
		}
		if ((bool)mRefractionEffect)
		{
			mRefractionEffect.mDone = true;
		}
	}

	public void SetEffectSpeed(float _speed)
	{
		mEffectSpeed = _speed;
		ParticlesMgr component = GetComponent<ParticlesMgr>();
		if (null != component)
		{
			component.Init(mEffectSpeed);
		}
	}
}
