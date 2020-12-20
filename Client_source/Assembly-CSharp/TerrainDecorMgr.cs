using System;
using UnityEngine;

[Serializable]
public class TerrainDecorMgr : MonoBehaviour
{
	public float mXZRot;

	public float mRadius = 1f;

	public int mDensity = 1;

	public float mScale = 1f;

	public float mRot;

	public int mPrefNum = -1;

	public bool mUseNormal;

	public GameObject[] mPrefabs;

	public GameObject mParent;

	public GameObject[] GetDecor()
	{
		if (mPrefNum >= mPrefabs.Length)
		{
			Debug.LogError("mPrefNum is too hight!");
			return null;
		}
		if (null == mParent)
		{
			Debug.LogError("Parent is null!");
			return null;
		}
		GameObject[] array = new GameObject[mDensity];
		for (int i = 0; i < mDensity; i++)
		{
			array[i] = mPrefabs[(mPrefNum == -1) ? UnityEngine.Random.Range(0, mPrefabs.Length) : mPrefNum];
		}
		return array;
	}
}
