using System;
using UnityEngine;

public class Utils
{
	public interface PlaceDescriptor
	{
		string GetName();

		string GetPath();
	}

	public class Named
	{
		public string mName = string.Empty;
	}

	[Serializable]
	public class WeightHolder : Named
	{
		public float mWeight;
	}

	[Serializable]
	public class VariantsList : Named
	{
		public WeightHolder[] mVariants;
	}

	public static T GetElement<T>(T[] _elems, string _name) where T : Named
	{
		if (_elems == null || _name == null || _name.Length == 0)
		{
			return (T)null;
		}
		foreach (T val in _elems)
		{
			if (val.mName.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				return val;
			}
		}
		return (T)null;
	}

	public static int Select(WeightHolder[] _weights)
	{
		if (_weights == null || _weights.Length == 0)
		{
			return -1;
		}
		if (_weights.Length == 1)
		{
			return 0;
		}
		float num = 0f;
		foreach (WeightHolder weightHolder in _weights)
		{
			num += weightHolder.mWeight;
		}
		float num2 = UnityEngine.Random.value * num;
		num = 0f;
		for (int j = 0; j < _weights.Length; j++)
		{
			num += _weights[j].mWeight;
			if (num2 <= num)
			{
				return j;
			}
		}
		return -1;
	}

	public static float BytesToMB(long _bytes)
	{
		return (float)_bytes / 1048576f;
	}
}
