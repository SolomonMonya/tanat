using System;
using UnityEngine;

public class Mod : MonoBehaviour
{
	[Serializable]
	public class Part
	{
		public UnityEngine.Object mSkinnedMesh;

		public Mesh mMesh;

		public int mLayer;

		public Texture mDiffuse;

		public Texture mTint;

		public Color mTintAdditive = Color.black;

		public Color mTintMultiply = Color.white;
	}

	public string mId;

	public string mSlot;

	public Part[] mParts;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
