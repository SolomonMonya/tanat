using System;

namespace AMF
{
	internal class TypeMarkerRecognizer
	{
		public static TypeMarker Recognize(Variable _var)
		{
			if (_var == null)
			{
				throw new ArgumentNullException();
			}
			Type valueType = _var.ValueType;
			if (valueType == null)
			{
				return TypeMarker.Null;
			}
			if (typeof(int) == valueType)
			{
				return TypeMarker.Integer;
			}
			if (typeof(double) == valueType || typeof(float) == valueType)
			{
				return TypeMarker.Double;
			}
			if (typeof(string) == valueType)
			{
				return TypeMarker.String;
			}
			if (typeof(MixedArray) == valueType)
			{
				return TypeMarker.Array;
			}
			if (typeof(byte[]) == valueType)
			{
				return TypeMarker.ByteArray;
			}
			if (typeof(bool) == valueType)
			{
				if (!_var)
				{
					return TypeMarker.False;
				}
				return TypeMarker.True;
			}
			return TypeMarker.Undefined;
		}
	}
}
