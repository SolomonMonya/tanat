using UnityEngine;

[AddComponentMenu("FXs/MoveObject")]
public class MoveObject : MonoBehaviour
{
	public Vector3 mDirection = Vector3.zero;

	public float mSpeed;

	public float mMaxDistance;

	private float mCurDistance;

	private void Update()
	{
		Vector3 translation = mDirection * mSpeed * Time.deltaTime;
		base.transform.Translate(translation, Space.World);
		mCurDistance += translation.magnitude;
		if (mCurDistance >= mMaxDistance)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
