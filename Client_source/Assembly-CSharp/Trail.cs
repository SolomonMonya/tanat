using System;
using UnityEngine;

public class Trail : MonoBehaviour
{
	private class Point
	{
		public float timeCreated;

		public float fadeAlpha;

		public Vector3 position = Vector3.zero;

		public Quaternion rotation = Quaternion.identity;

		public float timeAlive => Time.time - timeCreated;

		public Point(Transform trans)
		{
			position = trans.position;
			rotation = trans.rotation;
			timeCreated = Time.time;
		}

		public void update(Transform trans)
		{
			position = trans.position;
			rotation = trans.rotation;
			timeCreated = Time.time;
		}
	}

	public Material material;

	private Material instanceMaterial;

	public bool emit = true;

	private bool emittingDone;

	public float lifeTime = 1f;

	private float lifeTimeRatio = 1f;

	private float fadeOutRatio;

	public Color[] colors;

	public float[] widths;

	public float maxAngle = 2f;

	public float minVertexDistance = 0.1f;

	public float maxVertexDistance = 1f;

	public float optimizeAngleInterval = 0.1f;

	public float optimizeDistanceInterval = 0.05f;

	public int optimizeCount = 30;

	private GameObject trailObj;

	private Mesh mesh;

	private Point[] points = new Point[100];

	private int pointCnt;

	private void Start()
	{
		trailObj = new GameObject("Trail");
		trailObj.transform.parent = null;
		trailObj.transform.position = Vector3.zero;
		trailObj.transform.rotation = Quaternion.identity;
		trailObj.transform.localScale = Vector3.one;
		MeshFilter meshFilter = (MeshFilter)trailObj.AddComponent(typeof(MeshFilter));
		mesh = meshFilter.mesh;
		trailObj.AddComponent(typeof(MeshRenderer));
		instanceMaterial = new Material(material);
		fadeOutRatio = 1f / instanceMaterial.GetColor("_TintColor").a;
		trailObj.renderer.material = instanceMaterial;
	}

	private void Update()
	{
		if (!emit)
		{
			emittingDone = true;
		}
		if (emittingDone)
		{
			emit = false;
		}
		int num = pointCnt - 1;
		while (num >= 0)
		{
			Point point = points[num];
			if (point == null || point.timeAlive > lifeTime)
			{
				points[num] = null;
				pointCnt--;
				num--;
				continue;
			}
			break;
		}
		if (pointCnt > optimizeCount)
		{
			maxAngle += optimizeAngleInterval;
			maxVertexDistance += optimizeDistanceInterval;
			optimizeCount++;
		}
		if (emit)
		{
			if (pointCnt == 0)
			{
				points[pointCnt++] = new Point(base.transform);
				points[pointCnt++] = new Point(base.transform);
			}
			if (pointCnt == 1)
			{
				insertPoint();
			}
			bool flag = false;
			float sqrMagnitude = (points[1].position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude > minVertexDistance * minVertexDistance)
			{
				if (sqrMagnitude > maxVertexDistance * maxVertexDistance)
				{
					flag = true;
				}
				else if (Quaternion.Angle(base.transform.rotation, points[1].rotation) > maxAngle)
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (pointCnt == points.Length)
				{
					Array.Resize(ref points, points.Length + 50);
				}
				insertPoint();
			}
			if (!flag)
			{
				points[0].update(base.transform);
			}
		}
		if (pointCnt < 2)
		{
			trailObj.renderer.enabled = false;
			return;
		}
		trailObj.renderer.enabled = true;
		lifeTimeRatio = 1f / lifeTime;
		if (!emit)
		{
			if (pointCnt != 0)
			{
				Color color = instanceMaterial.GetColor("_TintColor");
				color.a -= fadeOutRatio * lifeTimeRatio * Time.deltaTime;
				if (color.a > 0f)
				{
					instanceMaterial.SetColor("_TintColor", color);
					return;
				}
				UnityEngine.Object.Destroy(trailObj);
				UnityEngine.Object.Destroy(this);
			}
			return;
		}
		Vector3[] array = new Vector3[pointCnt * 2];
		Vector2[] array2 = new Vector2[pointCnt * 2];
		int[] array3 = new int[(pointCnt - 1) * 6];
		Color[] array4 = new Color[pointCnt * 2];
		float num2 = 1f / (points[pointCnt - 1].timeAlive - points[0].timeAlive);
		for (int i = 0; i < pointCnt; i++)
		{
			Point point2 = points[i];
			float num3 = point2.timeAlive * lifeTimeRatio;
			Color color2;
			if (colors.Length == 0)
			{
				color2 = Color.Lerp(Color.white, Color.clear, num3);
			}
			else if (colors.Length == 1)
			{
				color2 = Color.Lerp(colors[0], Color.clear, num3);
			}
			else if (colors.Length == 2)
			{
				color2 = Color.Lerp(colors[0], colors[1], num3);
			}
			else
			{
				float num4 = num3 * (float)(colors.Length - 1);
				int num5 = (int)Mathf.Floor(num4);
				float t = Mathf.InverseLerp(num5, num5 + 1, num4);
				color2 = Color.Lerp(colors[num5], colors[num5 + 1], t);
			}
			array4[i * 2] = color2;
			array4[i * 2 + 1] = color2;
			float num6;
			if (widths.Length == 0)
			{
				num6 = 1f;
			}
			else if (widths.Length == 1)
			{
				num6 = widths[0];
			}
			else if (widths.Length == 2)
			{
				num6 = Mathf.Lerp(widths[0], widths[1], num3);
			}
			else
			{
				float num7 = num3 * (float)(widths.Length - 1);
				int num8 = (int)Mathf.Floor(num7);
				float t2 = Mathf.InverseLerp(num8, num8 + 1, num7);
				num6 = Mathf.Lerp(widths[num8], widths[num8 + 1], t2);
			}
			trailObj.transform.position = point2.position;
			trailObj.transform.rotation = point2.rotation;
			ref Vector3 reference = ref array[i * 2];
			reference = trailObj.transform.TransformPoint(0f, num6 * 0.5f, 0f);
			ref Vector3 reference2 = ref array[i * 2 + 1];
			reference2 = trailObj.transform.TransformPoint(0f, (0f - num6) * 0.5f, 0f);
			float x = (point2.timeAlive - points[0].timeAlive) * num2;
			ref Vector2 reference3 = ref array2[i * 2];
			reference3 = new Vector2(x, 0f);
			ref Vector2 reference4 = ref array2[i * 2 + 1];
			reference4 = new Vector2(x, 1f);
			if (i > 0)
			{
				int num9 = (i - 1) * 6;
				int num10 = i * 2;
				array3[num9] = num10 - 2;
				array3[num9 + 1] = num10 - 1;
				array3[num9 + 2] = num10;
				array3[num9 + 3] = num10 + 1;
				array3[num9 + 4] = num10;
				array3[num9 + 5] = num10 - 1;
			}
		}
		trailObj.transform.position = Vector3.zero;
		trailObj.transform.rotation = Quaternion.identity;
		mesh.Clear();
		mesh.vertices = array;
		mesh.colors = array4;
		mesh.uv = array2;
		mesh.triangles = array3;
	}

	private void insertPoint()
	{
		for (int num = pointCnt; num > 0; num--)
		{
			points[num] = points[num - 1];
		}
		points[0] = new Point(base.transform);
		pointCnt++;
	}
}
