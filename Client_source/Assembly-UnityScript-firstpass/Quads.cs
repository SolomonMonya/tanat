using System;
using UnityEngine;

[Serializable]
public class Quads : MonoBehaviour
{
	public static Mesh[] meshes;

	public static int currentQuads;

	public static bool HasMeshes()
	{
		int result;
		if (meshes == null)
		{
			result = 0;
		}
		else
		{
			int num = 0;
			Mesh[] array = meshes;
			int length = array.Length;
			while (true)
			{
				if (num < length)
				{
					if (null == array[num])
					{
						result = 0;
						break;
					}
					num = checked(num + 1);
					continue;
				}
				result = 1;
				break;
			}
		}
		return (byte)result != 0;
	}

	public static void Cleanup()
	{
		if (meshes == null)
		{
			return;
		}
		int i = 0;
		Mesh[] array = meshes;
		for (int length = array.Length; i < length; i = checked(i + 1))
		{
			if (null != array[i])
			{
				UnityEngine.Object.DestroyImmediate(array[i]);
				array[i] = null;
			}
		}
		meshes = null;
	}

	public static Mesh[] GetMeshes(int totalWidth, int totalHeight)
	{
		checked
		{
			Mesh[] result;
			if (HasMeshes() && currentQuads == totalWidth * totalHeight)
			{
				result = meshes;
			}
			else
			{
				int num = 10833;
				int num2 = (currentQuads = totalWidth * totalHeight);
				int num3 = Mathf.CeilToInt(1f * (float)num2 / (1f * (float)num));
				meshes = new Mesh[num3];
				int num4 = 0;
				int num5 = 0;
				for (num4 = 0; num4 < num2; num4 += num)
				{
					int triCount = Mathf.FloorToInt(Mathf.Clamp(num2 - num4, 0, num));
					meshes[num5] = GetMesh(triCount, num4, totalWidth, totalHeight);
					num5++;
				}
				result = meshes;
			}
			return result;
		}
	}

	public static Mesh GetMesh(int triCount, int triOffset, int totalWidth, int totalHeight)
	{
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		checked
		{
			Vector3[] array = new Vector3[triCount * 4];
			Vector2[] array2 = new Vector2[triCount * 4];
			Vector2[] array3 = new Vector2[triCount * 4];
			int[] array4 = new int[triCount * 6];
			float num = 0.0075f;
			for (int i = 0; i < triCount; i++)
			{
				int num2 = i * 4;
				int num3 = i * 6;
				int num4 = triOffset + i;
				float num5;
				float num6;
				Vector3 vector;
				unchecked
				{
					num5 = Mathf.Floor(num4 % totalWidth) / (float)totalWidth;
					num6 = Mathf.Floor(num4 / totalWidth) / (float)totalHeight;
					vector = new Vector3(num5 * 2f - 1f, num6 * 2f - 1f, 1f);
				}
				array[num2 + 0] = vector;
				array[num2 + 1] = vector;
				array[num2 + 2] = vector;
				array[num2 + 3] = vector;
				ref Vector2 reference = ref array2[num2 + 0];
				reference = new Vector2(0f, 0f);
				ref Vector2 reference2 = ref array2[num2 + 1];
				reference2 = new Vector2(1f, 0f);
				ref Vector2 reference3 = ref array2[num2 + 2];
				reference3 = new Vector2(0f, 1f);
				ref Vector2 reference4 = ref array2[num2 + 3];
				reference4 = new Vector2(1f, 1f);
				ref Vector2 reference5 = ref array3[num2 + 0];
				reference5 = new Vector2(num5, num6);
				ref Vector2 reference6 = ref array3[num2 + 1];
				reference6 = new Vector2(num5, num6);
				ref Vector2 reference7 = ref array3[num2 + 2];
				reference7 = new Vector2(num5, num6);
				ref Vector2 reference8 = ref array3[num2 + 3];
				reference8 = new Vector2(num5, num6);
				array4[num3 + 0] = num2 + 0;
				array4[num3 + 1] = num2 + 1;
				array4[num3 + 2] = num2 + 2;
				array4[num3 + 3] = num2 + 1;
				array4[num3 + 4] = num2 + 2;
				array4[num3 + 5] = num2 + 3;
			}
			mesh.vertices = array;
			mesh.triangles = array4;
			mesh.uv = array2;
			mesh.uv2 = array3;
			return mesh;
		}
	}

	public override void Main()
	{
	}
}
