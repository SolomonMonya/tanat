using UnityEngine;

[AddComponentMenu("FXs/SunBlow")]
public class SunBlow : MonoBehaviour
{
	public GameObject mParticle;

	public int mCount;

	public float mMaxDistance;

	public float mSpeed;

	private float mAngle;

	private void Start()
	{
		mAngle = 360f / (float)mCount;
		for (int i = 0; i < mCount; i++)
		{
			GameObject gameObject = Object.Instantiate(mParticle, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject.transform.position = base.gameObject.transform.position;
			gameObject.transform.Rotate(0f, (float)i * mAngle, 0f, Space.World);
			MoveObject moveObject = gameObject.GetComponent(typeof(MoveObject)) as MoveObject;
			moveObject.mDirection = gameObject.transform.right;
			moveObject.mMaxDistance = mMaxDistance;
			moveObject.mSpeed = mSpeed;
			GameObjUtil.TrySetParent(gameObject, "/effects");
		}
	}
}
