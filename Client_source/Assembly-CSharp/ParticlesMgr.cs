using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/ParticlesMgr")]
public class ParticlesMgr : MonoBehaviour
{
	private ParticleEmitter mEmitter;

	private ParticleAnimator mAnimator;

	public float mSpeedK = 1f;

	public void Init(float _speed)
	{
		mEmitter = GetComponent<ParticleEmitter>();
		mAnimator = GetComponent<ParticleAnimator>();
		if (null == mEmitter)
		{
			Log.Error("no ParticleEmitter");
		}
		if (null == mAnimator)
		{
			Log.Error("no ParticleAnimator");
		}
		mEmitter.minEmission *= mSpeedK * _speed;
		mEmitter.maxEmission *= mSpeedK * _speed;
		mEmitter.minEnergy *= mSpeedK / _speed;
		mEmitter.maxEnergy *= mSpeedK / _speed;
		mEmitter.worldVelocity *= mSpeedK * _speed;
		mEmitter.localVelocity *= mSpeedK * _speed;
		mEmitter.rndVelocity *= mSpeedK * _speed;
		mAnimator.force *= mSpeedK * _speed;
		mAnimator.rndForce *= mSpeedK * _speed;
		mAnimator.sizeGrow *= mSpeedK * _speed;
	}
}
