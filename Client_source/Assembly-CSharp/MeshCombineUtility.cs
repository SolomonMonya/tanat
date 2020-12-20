using UnityEngine;

public class MeshCombineUtility
{
	public struct MeshInstance
	{
		public Mesh mesh;

		public int subMeshIndex;

		public Matrix4x4 transform;
	}

	public static Mesh Combine(MeshInstance[] combines, bool generateStrips)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < combines.Length; i++)
		{
			MeshInstance meshInstance = combines[i];
			if (!meshInstance.mesh)
			{
				continue;
			}
			num += meshInstance.mesh.vertexCount;
			if (!generateStrips)
			{
				continue;
			}
			int num4 = meshInstance.mesh.GetTriangleStrip(meshInstance.subMeshIndex).Length;
			if (num4 != 0)
			{
				if (num3 != 0)
				{
					num3 = (((num3 & 1) != 1) ? (num3 + 2) : (num3 + 3));
				}
				num3 += num4;
			}
			else
			{
				generateStrips = false;
			}
		}
		if (!generateStrips)
		{
			for (int j = 0; j < combines.Length; j++)
			{
				MeshInstance meshInstance2 = combines[j];
				if ((bool)meshInstance2.mesh)
				{
					num2 += meshInstance2.mesh.GetTriangles(meshInstance2.subMeshIndex).Length;
				}
			}
		}
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = new Vector3[num];
		Vector4[] array3 = new Vector4[num];
		Vector2[] array4 = new Vector2[num];
		Vector2[] array5 = new Vector2[num];
		int[] array6 = new int[num2];
		int[] array7 = new int[num3];
		int offset = 0;
		for (int k = 0; k < combines.Length; k++)
		{
			MeshInstance meshInstance3 = combines[k];
			if ((bool)meshInstance3.mesh)
			{
				Copy(meshInstance3.mesh.vertexCount, meshInstance3.mesh.vertices, array, ref offset, meshInstance3.transform);
			}
		}
		offset = 0;
		for (int l = 0; l < combines.Length; l++)
		{
			MeshInstance meshInstance4 = combines[l];
			if ((bool)meshInstance4.mesh)
			{
				Matrix4x4 transform = meshInstance4.transform;
				transform = transform.inverse.transpose;
				CopyNormal(meshInstance4.mesh.vertexCount, meshInstance4.mesh.normals, array2, ref offset, transform);
			}
		}
		offset = 0;
		for (int m = 0; m < combines.Length; m++)
		{
			MeshInstance meshInstance5 = combines[m];
			if ((bool)meshInstance5.mesh)
			{
				Matrix4x4 transform2 = meshInstance5.transform;
				transform2 = transform2.inverse.transpose;
				CopyTangents(meshInstance5.mesh.vertexCount, meshInstance5.mesh.tangents, array3, ref offset, transform2);
			}
		}
		offset = 0;
		for (int n = 0; n < combines.Length; n++)
		{
			MeshInstance meshInstance6 = combines[n];
			if ((bool)meshInstance6.mesh)
			{
				Copy(meshInstance6.mesh.vertexCount, meshInstance6.mesh.uv, array4, ref offset);
			}
		}
		offset = 0;
		for (int num5 = 0; num5 < combines.Length; num5++)
		{
			MeshInstance meshInstance7 = combines[num5];
			if ((bool)meshInstance7.mesh)
			{
				Copy(meshInstance7.mesh.vertexCount, meshInstance7.mesh.uv1, array5, ref offset);
			}
		}
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		for (int num9 = 0; num9 < combines.Length; num9++)
		{
			MeshInstance meshInstance8 = combines[num9];
			if (!meshInstance8.mesh)
			{
				continue;
			}
			if (generateStrips)
			{
				int[] triangleStrip = meshInstance8.mesh.GetTriangleStrip(meshInstance8.subMeshIndex);
				if (num7 != 0)
				{
					if ((num7 & 1) == 1)
					{
						array7[num7] = array7[num7 - 1];
						array7[num7 + 1] = triangleStrip[0] + num8;
						array7[num7 + 2] = triangleStrip[0] + num8;
						num7 += 3;
					}
					else
					{
						array7[num7] = array7[num7 - 1];
						array7[num7 + 1] = triangleStrip[0] + num8;
						num7 += 2;
					}
				}
				for (int num10 = 0; num10 < triangleStrip.Length; num10++)
				{
					array7[num10 + num7] = triangleStrip[num10] + num8;
				}
				num7 += triangleStrip.Length;
			}
			else
			{
				int[] triangles = meshInstance8.mesh.GetTriangles(meshInstance8.subMeshIndex);
				for (int num11 = 0; num11 < triangles.Length; num11++)
				{
					array6[num11 + num6] = triangles[num11] + num8;
				}
				num6 += triangles.Length;
			}
			num8 += meshInstance8.mesh.vertexCount;
		}
		Mesh mesh = new Mesh();
		mesh.name = "Combined Mesh";
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.tangents = array3;
		mesh.uv = array4;
		mesh.uv1 = array5;
		if (generateStrips)
		{
			mesh.SetTriangleStrip(array7, 0);
		}
		else
		{
			mesh.triangles = array6;
		}
		return mesh;
	}

	private static void Copy(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector3 reference = ref dst[i + offset];
			reference = transform.MultiplyPoint(src[i]);
		}
		offset += vertexcount;
	}

	private static void CopyNormal(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector3 reference = ref dst[i + offset];
			reference = transform.MultiplyVector(src[i]).normalized;
		}
		offset += vertexcount;
	}

	private static void Copy(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector2 reference = ref dst[i + offset];
			reference = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyTangents(int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			Vector4 vector = src[i];
			Vector3 v = new Vector3(vector.x, vector.y, vector.z);
			v = transform.MultiplyVector(v);
			ref Vector4 reference = ref dst[i + offset];
			reference = new Vector4(v.x, v.y, v.z, vector.w);
		}
		offset += vertexcount;
	}
}
