using System;
using UnityEngine;

public class HeightMap
{
	public bool mEnableCache = true;

	private float mCellSize = 0.1f;

	private float mTerrHalfSize;

	private int mCellsHalfCnt;

	private float[,] mSurface;

	private Vector3[,] mNormals;

	public static int mLayersMask;

	public HeightMap(float _cellSize)
	{
		mCellSize = _cellSize;
		Init();
	}

	static HeightMap()
	{
		mLayersMask = 0;
		GameObjUtil.InitLayerMask("lightmapped", ref mLayersMask);
		GameObjUtil.InitLayerMask("terrain", ref mLayersMask);
	}

	public void Init()
	{
		float terrainSize = GetTerrainSize();
		int num = (int)(terrainSize / mCellSize);
		if (num == 0)
		{
			return;
		}
		mCellsHalfCnt = num >> 1;
		num = (mCellsHalfCnt << 1) + 1;
		mTerrHalfSize = (float)mCellsHalfCnt * mCellSize + mCellSize / 2f;
		mSurface = new float[num, num];
		mNormals = new Vector3[num, num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				mSurface[i, j] = -1024f;
			}
		}
	}

	private void InitCell(int _i, int _j, float _x, float _z)
	{
		try
		{
			Vector3 _normal;
			float y = GetY(_x, _z, out _normal);
			mSurface[_i, _j] = y;
			mNormals[_i, _j] = _normal;
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	private float TryGetY(int _i, int _j, float _x, float _z)
	{
		if (mSurface == null)
		{
			return 0f;
		}
		float num = 0f;
		try
		{
			num = mSurface[_i, _j];
			if (num < -100f)
			{
				InitCell(_i, _j, _x, _z);
				num = mSurface[_i, _j];
				return num;
			}
			return num;
		}
		catch (IndexOutOfRangeException)
		{
			return num;
		}
	}

	public float GetCachedY(float _x, float _z)
	{
		if (!mEnableCache)
		{
			return GetY(_x, _z);
		}
		GetCellCoords(_x, _z, out var _i, out var _j);
		float num = (float)(_i - mCellsHalfCnt) * mCellSize;
		float num2 = (float)(_j - mCellsHalfCnt) * mCellSize;
		float num3 = TryGetY(_i, _j, num, num2);
		float num4 = num;
		float num5 = num2;
		int num6 = _i;
		int num7 = _j;
		if (_x > num)
		{
			num4 += mCellSize;
			num6++;
		}
		else
		{
			num4 -= mCellSize;
			num6--;
		}
		if (_z > num2)
		{
			num5 += mCellSize;
			num7++;
		}
		else
		{
			num5 -= mCellSize;
			num7--;
		}
		float num8 = TryGetY(num6, _j, num4, num2);
		float num9 = TryGetY(_i, num7, num, num5);
		float num10 = _x - num;
		float num11 = _z - num2;
		float num12 = num9 - num3;
		float num13 = num5 - num2;
		float num14 = num4 - num;
		float num15 = num8 - num3;
		float num16 = (num11 * num12 * num14 + num10 * num13 * num15) / (num13 * num14);
		return num16 + num3;
	}

	public Vector3 GetCachedNormal(float _x, float _z)
	{
		//Discarded unreachable code: IL_0036
		if (!mEnableCache)
		{
			GetY(_x, _z, out var _normal);
			return _normal;
		}
		GetCellCoords(_x, _z, out var _i, out var _j);
		try
		{
			return mNormals[_i, _j];
		}
		catch (IndexOutOfRangeException)
		{
		}
		return Vector3.up;
	}

	private void GetCellCoords(float _x, float _z, out int _i, out int _j)
	{
		_i = mCellsHalfCnt + (int)(_x / mCellSize);
		_j = mCellsHalfCnt + (int)(_z / mCellSize);
	}

	public float GetCachedTerrainSize()
	{
		return (!mEnableCache) ? GetTerrainSize() : (mTerrHalfSize * 2f);
	}

	public static float GetTerrainSize()
	{
		float result = 0f;
		CommonInput commonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (null != commonInput)
		{
			result = commonInput.mMapSize;
		}
		UnityEngine.Object[] array = null;
		if (array != null && array.Length > 0)
		{
			GameObject gameObject = (array[0] as Component).gameObject;
			if (null != gameObject.collider)
			{
				float z = gameObject.collider.bounds.size.z;
				float x = gameObject.collider.bounds.size.x;
				result = ((!(x > z)) ? z : x);
			}
		}
		return result;
	}

	private static RaycastHit GetHighestHit(float _x, float _z)
	{
		Physics.Raycast(new Vector3(_x, 512f, _z), new Vector3(0f, -1f, 0f), out var hitInfo, 1024f, mLayersMask);
		return hitInfo;
	}

	public static float GetY(float _x, float _z)
	{
		return GetHighestHit(_x, _z).point.y;
	}

	public static float GetY(HeightMap _hm, float _x, float _z)
	{
		return _hm?.GetCachedY(_x, _z) ?? GetY(_x, _z);
	}

	public static float GetY(float _x, float _z, out Vector3 _normal)
	{
		RaycastHit highestHit = GetHighestHit(_x, _z);
		_normal = highestHit.normal;
		if (_normal.sqrMagnitude == 0f)
		{
			_normal = Vector3.up;
		}
		return highestHit.point.y;
	}
}
