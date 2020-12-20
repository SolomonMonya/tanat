using System;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class WaveMeshAdjustment : MonoBehaviour
{
	public Collider col;

	public override void Start()
	{
		MeshFilter meshFilter = (MeshFilter)GetComponent(typeof(MeshFilter));
		Mesh mesh = meshFilter.mesh;
		Transform transform = this.transform;
		Vector3[] vertices = mesh.vertices;
		int i = 1;
		RaycastHit hitInfo = default(RaycastHit);
		checked
		{
			for (; i < Extensions.get_length((System.Array)vertices) - 1; i += 2)
			{
				Vector3 vector = vertices[i - 1] - vertices[i];
				if (transform.TransformDirection(vector) != Vector3.zero && col.Raycast(new Ray(transform.TransformPoint(vertices[i]), transform.TransformDirection(vector)), out hitInfo, 30f))
				{
					Vector3 vector2 = transform.InverseTransformPoint(hitInfo.point);
					Vector3 position = vector2 + vector / 3f;
					position.y += 15f;
					if (col.Raycast(new Ray(transform.TransformPoint(position), -Vector3.up), out hitInfo, 30f))
					{
						vector2 = transform.InverseTransformPoint(hitInfo.point);
					}
					if (!(vector2.y <= 1.5f))
					{
						vector2.y = 0f;
					}
					vertices[i - 1] = vector2;
				}
			}
			mesh.vertices = vertices;
			meshFilter.mesh = mesh;
		}
	}

	public override void Main()
	{
	}
}
