using UnityEngine;

[AddComponentMenu("Object Scripts/Transform Scripts/ObjectRotateXYZ")]
[ExecuteInEditMode]
public class ObjectRotate : MonoBehaviour
{
	public bool mWorld;

	public Vector3 mSpeed = Vector3.zero;

	private void Update()
	{
		if (Vector3.zero != mSpeed)
		{
			if (mWorld)
			{
				base.transform.Rotate(mSpeed * Time.deltaTime, Space.World);
			}
			else
			{
				base.transform.Rotate(mSpeed * Time.deltaTime);
			}
		}
	}
}
