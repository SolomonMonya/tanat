using UnityEngine;

[AddComponentMenu("FXs/Explosion")]
public class Explosion : MonoBehaviour
{
	public int mPartsCountMin;

	public int mPartsCountMax;

	public float mMinTimeLife;

	public float mMaxTimeLife;

	public float mGravity;

	public float mSpeedMin;

	public float mSpeedMax;

	public float mAcceleration;

	public float mXRange = 1f;

	public float mYRange = 1f;

	public float mZRange = 1f;

	public Vector3 mOffset = Vector3.zero;

	public GameObject[] mExplosionParts;

	public void Explode()
	{
		if (mExplosionParts.Length == 0)
		{
			return;
		}
		int num = Random.Range(mPartsCountMin, mPartsCountMax);
		for (int i = 0; i < num; i++)
		{
			Object @object = mExplosionParts[Random.Range(0, mExplosionParts.Length)];
			if (!(null == @object))
			{
				GameObject gameObject = Object.Instantiate(@object) as GameObject;
				gameObject.transform.position = base.transform.position + mOffset;
				ExplosionPart component = gameObject.GetComponent<ExplosionPart>();
				component.mSpeedDirection = GeneratePartSpeedDirection();
				component.mSpeed = Random.Range(mSpeedMin, mSpeedMax);
				component.mTimeLife = Random.Range(mMinTimeLife, mMaxTimeLife);
				component.mGravity = mGravity;
				GameObjUtil.TrySetParent(gameObject, "/effects");
			}
		}
	}

	private Vector3 GeneratePartSpeedDirection()
	{
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(0f - mXRange, mXRange);
		zero.y = Random.Range(0f, mYRange);
		zero.z = Random.Range(0f - mZRange, mZRange);
		zero.Normalize();
		return zero;
	}
}
