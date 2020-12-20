using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public static class CastUtil
	{
		public static object SafeChangeType(object _obj, Type _type)
		{
			if (_obj == null || _type == null)
			{
				return null;
			}
			try
			{
				return Convert.ChangeType(_obj, _type);
			}
			catch (InvalidCastException)
			{
				return null;
			}
			catch (FormatException)
			{
				return null;
			}
		}

		public static T GetParam<T>(IDictionary<string, string> _params, string _paramName)
		{
			if (string.IsNullOrEmpty(_paramName) || _params == null)
			{
				return default(T);
			}
			if (_params.TryGetValue(_paramName, out var value))
			{
				try
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}
				catch (InvalidCastException)
				{
				}
			}
			return default(T);
		}
	}
}
