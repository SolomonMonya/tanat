using UnityEngine;

[AddComponentMenu("FXs/ExplosionPart")]
public class ExplosionPart : MonoBehaviour
{
	public Vector3 mSpeedDirection = Vector3.zero;

	public float mSpeed;

	public float mGravity;

	public float mTimeLife;

	public float mAcceleration;

	public float mRotSpeedMin;

	public float mRotSpeedMax;

	private Vector3 mRotSpeed = Vector3.zero;

	private Vector3 mStartSpeed = Vector3.zero;

	private float mCurTimeLife;

	private Vector3 mStartPos = Vector3.zero;

	private ParticleEmitter mParticleEmitter;

	private void Start()
	{
		mParticleEmitter = GetComponentInChildren<ParticleEmitter>();
		mStartSpeed = mSpeedDirection * mSpeed;
		mStartPos = base.transform.position;
		mRotSpeed = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * Random.Range(mRotSpeedMin, mRotSpeedMax);
	}

	private void Update()
	{
		if (mCurTimeLife < mTimeLife)
		{
			mCurTimeLife += Time.deltaTime;
			base.transform.position = ParabolicMove.GetParabolicPosByTime(mStartPos, mStartSpeed, mGravity, mCurTimeLife);
			base.transform.Rotate(mRotSpeed * Time.deltaTime, Space.World);
		}
		else if (null != mParticleEmitter)
		{
			if (mParticleEmitter.emit)
			{
				mParticleEmitter.emit = false;
				Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = false;
				}
			}
			else if (mParticleEmitter.particleCount == 0)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
