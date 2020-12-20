using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Death Behaviour/ObjDeath")]
public class ObjDeath : DeathBehaviour
{
	public enum DeathEffectPlayType
	{
		ALWAYS,
		WITH_ANIM,
		WITH_EXPLOSION
	}

	public string mDeathEffectId = string.Empty;

	public bool mShakeCam;

	public float mToAlphaTime = 1f;

	public float mAnimRand = 1f;

	public DeathEffectPlayType mEffectPlayType;

	public float mMaxDieTime = 1.8f;

	private Dictionary<GameObject, Material[]> mCatchedMaterials;

	private Explosion mExplosion;

	private WarFogObject mWarFogObject;

	private VisualEffectOptions mVisualEffectOptions;

	private AnimationExt mAnimationExt;

	private bool mDead;

	private bool mStartDie;

	private Vector3 mDeltaPos = new Vector3(0f, 0.0025f, 0f);

	private Vector3 mStartFallPos = Vector3.zero;

	private float mTargetYPos;

	private float mStartDieTime;

	private float mBeginDieTime;

	private float mAnimLength;

	private bool mPlayAnim = true;

	private bool mStopEffects;

	public void Awake()
	{
		mExplosion = GetComponent<Explosion>();
		mWarFogObject = GetComponent<WarFogObject>();
		mVisualEffectOptions = GetComponent<VisualEffectOptions>();
	}

	public void Update()
	{
		if (!mStartDie)
		{
			return;
		}
		if (!VisibilityMgr.IsVisible(base.gameObject) || !mPlayAnim)
		{
			Done();
			return;
		}
		mAnimLength = mAnimationExt.GetCurAnimationLength(AnimationExt.State.death);
		float num = Time.time - mBeginDieTime;
		if (mAnimationExt.IsPlaying(AnimationExt.State.death))
		{
			Vector3 position = mStartFallPos;
			float num2 = 1f - num / mAnimLength;
			num2 = ((!(num2 < 0f)) ? num2 : 0f);
			position.y = mTargetYPos + (mStartFallPos.y - mTargetYPos) * num2;
			base.gameObject.transform.position = position;
		}
		if (mStartDieTime == 0f && num >= mAnimLength * mToAlphaTime)
		{
			mStartDieTime = Time.time;
			foreach (KeyValuePair<GameObject, Material[]> mCatchedMaterial in mCatchedMaterials)
			{
				Material[] materials = mCatchedMaterial.Key.renderer.materials;
				Material[] array = materials;
				foreach (Material material in array)
				{
					material.shader = Shader.Find("Tanat/Fx/DeathShader");
				}
			}
			if (mWarFogObject != null)
			{
				mWarFogObject.ToFog();
			}
			ParticleEmitter[] componentsInChildren = GetComponentsInChildren<ParticleEmitter>();
			ParticleEmitter[] array2 = componentsInChildren;
			foreach (ParticleEmitter particleEmitter in array2)
			{
				particleEmitter.emit = false;
			}
		}
		if (!mDead && mStartDieTime > 0f)
		{
			float num3 = Time.time - mStartDieTime;
			Color color = Color.clear;
			float num4 = 1f - num3 / mMaxDieTime;
			if (num4 < 0f)
			{
				num4 = 0f;
				mDead = true;
			}
			foreach (GameObject key in mCatchedMaterials.Keys)
			{
				Material[] materials2 = key.renderer.materials;
				Material[] array3 = materials2;
				foreach (Material material2 in array3)
				{
					color = material2.color;
					color.a = num4;
					material2.color = color;
				}
			}
			if (num4 < 0.6f)
			{
				base.gameObject.transform.position -= mDeltaPos;
			}
		}
		if (mDead)
		{
			bool flag = EffectsDone();
			if (!flag && mStopEffects)
			{
				mStopEffects = false;
				VisualEffectsMgr.Instance.StopObjectEffects(base.gameObject);
			}
			if (flag)
			{
				Done();
			}
		}
	}

	protected override void StartDie()
	{
		if (mStartDie)
		{
			return;
		}
		mAnimationExt = GetComponent<AnimationExt>();
		if (mAnimationExt != null)
		{
			mAnimLength = mAnimationExt.GetCurAnimationLength(AnimationExt.State.death);
		}
		mCatchedMaterials = mVisualEffectOptions.GetDefaultMaterials();
		if (mAnimLength == 0f || mAnimationExt == null)
		{
			Done();
			return;
		}
		mStartFallPos = base.gameObject.transform.position;
		mBeginDieTime = Time.time;
		mTargetYPos = HeightMap.GetY(mStartFallPos.x, mStartFallPos.z);
		float value = Random.value;
		mPlayAnim = value <= mAnimRand;
		mStartDie = true;
		mStopEffects = true;
		if (mDeathEffectId != string.Empty && (mEffectPlayType == DeathEffectPlayType.ALWAYS || (mEffectPlayType == DeathEffectPlayType.WITH_ANIM && mPlayAnim) || (mEffectPlayType == DeathEffectPlayType.WITH_EXPLOSION && !mPlayAnim && (bool)mExplosion)))
		{
			VisualEffectsMgr.Instance.PlayEffect(mDeathEffectId, base.gameObject.transform.position);
		}
		if (!mExplosion || mPlayAnim)
		{
			return;
		}
		if (mShakeCam)
		{
			GameCamera component = Camera.main.GetComponent<GameCamera>();
			if (null != component)
			{
				component.MakeShakeEffect(base.gameObject.transform.position);
			}
		}
		mExplosion.Explode();
	}

	public override void Reborn()
	{
		if (mDead || mStartDie)
		{
			mDead = false;
			mStartDie = false;
			mStartDieTime = 0f;
			mBeginDieTime = 0f;
			mStartFallPos = Vector3.zero;
			mPlayAnim = true;
			if (mWarFogObject != null)
			{
				mWarFogObject.FromFog();
			}
			ParticleEmitter[] componentsInChildren = GetComponentsInChildren<ParticleEmitter>();
			ParticleEmitter[] array = componentsInChildren;
			foreach (ParticleEmitter particleEmitter in array)
			{
				particleEmitter.emit = true;
			}
			mVisualEffectOptions.SetDefaultMaterials();
			base.Reborn();
		}
	}

	public bool GetDead()
	{
		return mDead;
	}
}
