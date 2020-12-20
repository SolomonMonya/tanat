using UnityEngine;

[AddComponentMenu("FXs/ProjectorPointer")]
public class ProjectorPointer : MonoBehaviour
{
	private void Update()
	{
		base.transform.RotateAround(Vector3.zero, Vector3.up, 360f * Time.deltaTime);
	}
}
