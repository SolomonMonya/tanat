using System.Collections.Generic;

namespace AMF
{
	internal class StringRefTable
	{
		private Dictionary<string, int> mStr2Id = new Dictionary<string, int>();

		private Dictionary<int, string> mId2Str = new Dictionary<int, string>();

		private int mNextId;

		public int GetStrId(string _str)
		{
			if (mStr2Id.TryGetValue(_str, out var value))
			{
				return value;
			}
			return -1;
		}

		public string GetStr(int _id)
		{
			if (mId2Str.TryGetValue(_id, out var value))
			{
				return value;
			}
			return "";
		}

		public void AddStr(string _str)
		{
			if (!string.IsNullOrEmpty(_str))
			{
				mStr2Id[_str] = mNextId;
				mId2Str[mNextId] = _str;
				mNextId++;
			}
		}

		public void Clear()
		{
			mStr2Id.Clear();
			mId2Str.Clear();
			mNextId = 0;
		}
	}
}
