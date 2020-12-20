using System;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffectKeys")]
public class VisualEffectKeys : MonoBehaviour
{
	[Serializable]
	public class VisualEffectKey
	{
		public Color mColor;

		public bool mEmitParticles;

		public float mTime;

		public float mTextureOffsetX;

		public float mTextureOffsetY;

		public VisualEffectKey()
		{
			mColor = Color.clear;
			mEmitParticles = false;
			mTime = 0f;
			mTextureOffsetX = 0f;
			mTextureOffsetY = 0f;
		}
	}

	public VisualEffectKey[] mVisualEffectKeys;

	public VisualEffectKeys()
	{
		mVisualEffectKeys = new VisualEffectKey[0];
	}

	public void AddEffectKey(int _keyNum)
	{
		if (_keyNum <= mVisualEffectKeys.Length && _keyNum >= 0 && mVisualEffectKeys.Length != 10)
		{
			VisualEffectKey[] array = new VisualEffectKey[mVisualEffectKeys.Length + 1];
			array[_keyNum] = new VisualEffectKey();
			Array.Copy(mVisualEffectKeys, 0, array, 0, _keyNum);
			if (mVisualEffectKeys.Length > 0)
			{
				Array.Copy(mVisualEffectKeys, _keyNum, array, _keyNum + 1, mVisualEffectKeys.Length - _keyNum);
			}
			mVisualEffectKeys = array;
		}
	}

	public void RemoveEffectKey(int _keyNum)
	{
		if (mVisualEffectKeys.Length == 0 || _keyNum < 0 || _keyNum > mVisualEffectKeys.Length)
		{
			return;
		}
		if (mVisualEffectKeys.Length - 1 == 0)
		{
			mVisualEffectKeys = new VisualEffectKey[0];
			return;
		}
		VisualEffectKey[] array = new VisualEffectKey[mVisualEffectKeys.Length - 1];
		Array.Copy(mVisualEffectKeys, 0, array, 0, _keyNum);
		if (array.Length - _keyNum > 0)
		{
			Array.Copy(mVisualEffectKeys, _keyNum + 1, array, _keyNum, array.Length - _keyNum);
		}
		mVisualEffectKeys = array;
	}

	public int GetKeysCount()
	{
		return mVisualEffectKeys.Length;
	}
}
