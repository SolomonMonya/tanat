using System;
using UnityEngine;

public class PassibilityData : MonoBehaviour
{
	[Serializable]
	public class Polygon
	{
		public Vector2[] mPoints;

		public bool mClockwise;
	}

	public string mFileName = "map.xml";

	public Polygon[] mPolygons;

	public bool ValidatePolygon(int _polyNum)
	{
		return _polyNum >= 0 && _polyNum < mPolygons.Length;
	}

	public bool ValidatePoint(int _pointNum, int _polyNum)
	{
		if (!ValidatePolygon(_polyNum))
		{
			return false;
		}
		return _pointNum >= 0 && _pointNum < mPolygons[_polyNum].mPoints.Length;
	}

	public int AddPolygon()
	{
		int num = 0;
		Polygon[] array = mPolygons;
		if (array != null)
		{
			num = array.Length;
		}
		mPolygons = new Polygon[num + 1];
		if (num > 0)
		{
			Array.Copy(array, mPolygons, num);
		}
		int num2 = mPolygons.Length - 1;
		mPolygons[num2] = new Polygon();
		mPolygons[num2].mPoints = new Vector2[0];
		return num2;
	}

	public void AddPoint(int _polyNum, Vector2 _p)
	{
		if (ValidatePolygon(_polyNum))
		{
			Vector2[] mPoints = mPolygons[_polyNum].mPoints;
			Vector2[] array = new Vector2[mPoints.Length + 1];
			Array.Copy(mPoints, array, mPoints.Length);
			array[array.Length - 1] = _p;
			mPolygons[_polyNum].mPoints = array;
		}
	}

	public void RemovePoint(int _polyNum, int _pointNum)
	{
		if (ValidatePoint(_pointNum, _polyNum))
		{
			Vector2[] mPoints = mPolygons[_polyNum].mPoints;
			if (mPoints.Length != 1)
			{
				Vector2[] array = new Vector2[mPoints.Length - 1];
				Array.Copy(mPoints, array, _pointNum);
				Array.Copy(mPoints, _pointNum + 1, array, _pointNum, mPoints.Length - _pointNum - 1);
				mPolygons[_polyNum].mPoints = array;
			}
		}
	}

	public void InsertPointNext(int _polyNum, int _pointNum)
	{
		if (ValidatePoint(_pointNum, _polyNum))
		{
			Vector2[] mPoints = mPolygons[_polyNum].mPoints;
			if (mPoints.Length >= 2)
			{
				Vector2[] array = new Vector2[mPoints.Length + 1];
				Array.Copy(mPoints, array, _pointNum + 1);
				Array.Copy(mPoints, _pointNum + 1, array, _pointNum + 2, mPoints.Length - _pointNum - 1);
				Vector2 vector = ((_pointNum != mPoints.Length - 1) ? mPoints[_pointNum + 1] : mPoints[0]);
				Vector2 vector2 = (vector - mPoints[_pointNum]) / 2f;
				ref Vector2 reference = ref array[_pointNum + 1];
				reference = mPoints[_pointNum] + vector2;
				mPolygons[_polyNum].mPoints = array;
			}
		}
	}

	public int GetNearest(Vector2 _hitPos, int _polyNum)
	{
		if (!ValidatePolygon(_polyNum))
		{
			return -1;
		}
		int result = -1;
		float num = float.MaxValue;
		for (int num2 = mPolygons[_polyNum].mPoints.Length - 1; num2 >= 0; num2--)
		{
			Vector2 vector = mPolygons[_polyNum].mPoints[num2];
			float magnitude = (_hitPos - vector).magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				result = num2;
			}
		}
		return result;
	}

	public bool MakeFirst(int _polyNum)
	{
		if (mPolygons.Length <= 1 || _polyNum == 0)
		{
			return false;
		}
		if (!ValidatePolygon(_polyNum))
		{
			return false;
		}
		Polygon polygon = mPolygons[_polyNum];
		Array.Copy(mPolygons, 0, mPolygons, 1, _polyNum);
		mPolygons[0] = polygon;
		return true;
	}
}
