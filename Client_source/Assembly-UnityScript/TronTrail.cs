using System;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
[RequireComponent(typeof(MeshFilter))]
public class TronTrail : MonoBehaviour
{
	public float height;

	public float time;

	public bool alwaysUp;

	public float minDistance;

	public Color startColor;

	public Color endColor;

	private UnityScript.Lang.Array sections;

	public TronTrail()
	{
		height = 2f;
		time = 2f;
		minDistance = 0.1f;
		startColor = Color.white;
		endColor = new Color(1f, 1f, 1f, 0f);
		sections = new UnityScript.Lang.Array();
	}

	public override void LateUpdate()
	{
		Vector3 position = transform.position;
		float num = Time.time;
		checked
		{
			while (sections.length > 0 && RuntimeServices.ToBool(RuntimeServices.InvokeBinaryOperator("op_GreaterThan", num, RuntimeServices.InvokeBinaryOperator("op_Addition", UnityRuntimeServices.GetProperty(sections[sections.length - 1], "time"), time))))
			{
				sections.Pop();
			}
			if (sections.length == 0 || RuntimeServices.ToBool(RuntimeServices.InvokeBinaryOperator("op_GreaterThan", UnityRuntimeServices.GetProperty(RuntimeServices.InvokeBinaryOperator("op_Subtraction", UnityRuntimeServices.GetProperty(sections[0], "point"), position), "sqrMagnitude"), minDistance * minDistance)))
			{
				TronTrailSection tronTrailSection = new TronTrailSection();
				tronTrailSection.point = position;
				if (alwaysUp)
				{
					tronTrailSection.upDir = Vector3.up;
				}
				else
				{
					tronTrailSection.upDir = transform.TransformDirection(Vector3.up);
				}
				tronTrailSection.time = num;
				sections.Unshift(tronTrailSection);
			}
			Mesh mesh = ((MeshFilter)GetComponent(typeof(MeshFilter))).mesh;
			mesh.Clear();
			if (sections.length < 2)
			{
				return;
			}
			Vector3[] array = new Vector3[sections.length * 2];
			Color[] array2 = new Color[sections.length * 2];
			Vector2[] array3 = new Vector2[sections.length * 2];
			object obj = sections[0];
			if (!(obj is TronTrailSection))
			{
				obj = RuntimeServices.Coerce(obj, typeof(TronTrailSection));
			}
			TronTrailSection tronTrailSection2 = (TronTrailSection)obj;
			object obj2 = sections[0];
			if (!(obj2 is TronTrailSection))
			{
				obj2 = RuntimeServices.Coerce(obj2, typeof(TronTrailSection));
			}
			TronTrailSection tronTrailSection3 = (TronTrailSection)obj2;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			for (int i = 0; i < sections.length; i++)
			{
				tronTrailSection2 = tronTrailSection3;
				object obj3 = sections[i];
				if (!(obj3 is TronTrailSection))
				{
					obj3 = RuntimeServices.Coerce(obj3, typeof(TronTrailSection));
				}
				tronTrailSection3 = (TronTrailSection)obj3;
				float num2 = 0f;
				if (i != 0)
				{
					num2 = Mathf.Clamp01((Time.time - tronTrailSection3.time) / time);
				}
				Vector3 upDir = tronTrailSection3.upDir;
				ref Vector3 reference = ref array[i * 2 + 0];
				reference = worldToLocalMatrix.MultiplyPoint(tronTrailSection3.point);
				ref Vector3 reference2 = ref array[i * 2 + 1];
				reference2 = worldToLocalMatrix.MultiplyPoint(tronTrailSection3.point + upDir * height);
				ref Vector2 reference3 = ref array3[i * 2 + 0];
				reference3 = new Vector2(num2, 0f);
				ref Vector2 reference4 = ref array3[i * 2 + 1];
				reference4 = new Vector2(num2, 1f);
				Color color = Color.Lerp(startColor, endColor, num2);
				array2[i * 2 + 0] = color;
				array2[i * 2 + 1] = color;
			}
			int[] array4 = new int[(sections.length - 1) * 2 * 3];
			for (int i = 0; i < unchecked(Extensions.get_length((System.Array)array4) / 6); i++)
			{
				array4[i * 6 + 0] = i * 2;
				array4[i * 6 + 1] = i * 2 + 1;
				array4[i * 6 + 2] = i * 2 + 2;
				array4[i * 6 + 3] = i * 2 + 2;
				array4[i * 6 + 4] = i * 2 + 1;
				array4[i * 6 + 5] = i * 2 + 3;
			}
			mesh.vertices = array;
			mesh.colors = array2;
			mesh.uv = array3;
			mesh.triangles = array4;
		}
	}

	public override void Main()
	{
	}
}
