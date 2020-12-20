using UnityEngine;

[AddComponentMenu("FXs/RandomTransform")]
public class RandomTransform : MonoBehaviour
{
	public Vector3 mScale = Vector3.one;

	public Vector3 mRotate = Vector3.zero;

	private void Start()
	{
		base.gameObject.transform.Rotate(Random.Range(0f - mRotate.x, mRotate.x), Random.Range(0f - mRotate.y, mRotate.y), Random.Range(0f - mRotate.z, mRotate.z));
		base.gameObject.transform.localScale = new Vector3(Random.Range(1f, mScale.x), Random.Range(1f, mScale.y), Random.Range(1f, mScale.z));
		Object.Destroy(this);
	}
}
