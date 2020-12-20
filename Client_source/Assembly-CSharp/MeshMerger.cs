using UnityEngine;

public class MeshMerger : MonoBehaviour
{
	public MeshFilter[] meshFilters;

	public Material material;

	private void Start()
	{
		if (meshFilters.Length == 0)
		{
			Component[] componentsInChildren = GetComponentsInChildren(typeof(MeshFilter));
			meshFilters = new MeshFilter[componentsInChildren.Length];
			int num = 0;
			Component[] array = componentsInChildren;
			foreach (Component component in array)
			{
				meshFilters[num++] = (MeshFilter)component;
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		MeshFilter[] array2 = meshFilters;
		foreach (MeshFilter meshFilter in array2)
		{
			num2 += meshFilter.mesh.vertices.Length;
			num3 += meshFilter.mesh.normals.Length;
			num4 += meshFilter.mesh.triangles.Length;
			num5 += meshFilter.mesh.uv.Length;
			if (material == null)
			{
				material = meshFilter.gameObject.renderer.material;
			}
		}
		Vector3[] array3 = new Vector3[num2];
		Vector3[] array4 = new Vector3[num3];
		Transform[] array5 = new Transform[meshFilters.Length];
		Matrix4x4[] array6 = new Matrix4x4[meshFilters.Length];
		BoneWeight[] array7 = new BoneWeight[num2];
		int[] array8 = new int[num4];
		Vector2[] array9 = new Vector2[num5];
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		MeshFilter[] array10 = meshFilters;
		foreach (MeshFilter meshFilter2 in array10)
		{
			int[] triangles = meshFilter2.mesh.triangles;
			foreach (int num11 in triangles)
			{
				array8[num8++] = num11 + num6;
			}
			array5[num10] = meshFilter2.transform;
			ref Matrix4x4 reference = ref array6[num10];
			reference = Matrix4x4.identity;
			Vector3[] vertices = meshFilter2.mesh.vertices;
			foreach (Vector3 vector in vertices)
			{
				array7[num6].weight0 = 1f;
				array7[num6].boneIndex0 = num10;
				array3[num6++] = vector;
			}
			Vector3[] normals = meshFilter2.mesh.normals;
			foreach (Vector3 vector2 in normals)
			{
				array4[num7++] = vector2;
			}
			Vector2[] uv = meshFilter2.mesh.uv;
			foreach (Vector2 vector3 in uv)
			{
				array9[num9++] = vector3;
			}
			num10++;
			MeshRenderer meshRenderer = meshFilter2.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
			if ((bool)meshRenderer)
			{
				meshRenderer.enabled = false;
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = base.gameObject.name;
		mesh.vertices = array3;
		mesh.normals = array4;
		mesh.boneWeights = array7;
		mesh.uv = array9;
		mesh.triangles = array8;
		mesh.bindposes = array6;
		SkinnedMeshRenderer skinnedMeshRenderer = base.gameObject.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		skinnedMeshRenderer.sharedMesh = mesh;
		skinnedMeshRenderer.bones = array5;
		base.renderer.material = material;
	}
}
