using System;
using Boo.Lang;

namespace UnityScript.Lang
{
	[Serializable]
	public class Extensions
	{
		[Extension]
		public static int length => a.Length;

		[Extension]
		public static int length => s.Length;

		[Extension]
		public static bool operator ==(char lhs, string rhs)
		{
			bool num = 1 == rhs.Length;
			if (num)
			{
				num = lhs == rhs[0];
			}
			return num;
		}

		[Extension]
		public static bool operator ==(string lhs, char rhs)
		{
			return rhs == lhs;
		}

		[Extension]
		public static bool operator !=(char lhs, string rhs)
		{
			bool num = 1 != rhs.Length;
			if (!num)
			{
				num = lhs != rhs[0];
			}
			return num;
		}

		[Extension]
		public static bool operator !=(string lhs, char rhs)
		{
			return rhs != lhs;
		}

		[Extension]
		public static implicit operator bool(Enum e)
		{
			return ((IConvertible)e).ToInt32((IFormatProvider)null) != 0;
		}
	}
}
