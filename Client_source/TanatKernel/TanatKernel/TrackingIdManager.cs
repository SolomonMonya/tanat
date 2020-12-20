using System.Collections.Generic;
using Log4Tanat;

namespace TanatKernel
{
	internal class TrackingIdManager
	{
		private static readonly int addMask = int.MinValue;

		private static readonly int remMask = 1073741824;

		private static readonly int invMask = 536870912;

		private static readonly int idMask = ~(addMask | remMask | invMask);

		private List<int> mTrackingIds = new List<int>();

		public int IdsCount => mTrackingIds.Count;

		public bool RegTrackingId(ref int _id, ICollection<int> _vis, ICollection<int> _invis)
		{
			bool flag = (_id & addMask) != 0;
			bool flag2 = (_id & remMask) != 0;
			bool flag3 = (_id & invMask) != 0;
			_id &= idMask;
			if (flag)
			{
				mTrackingIds.Add(_id);
				flag2 = false;
			}
			else
			{
				int num = _id;
				if (num >= 0 && num < mTrackingIds.Count)
				{
					_id = mTrackingIds[num];
					if (flag2)
					{
						int index = mTrackingIds.Count - 1;
						mTrackingIds[num] = mTrackingIds[index];
						mTrackingIds.RemoveAt(index);
						flag3 = true;
					}
				}
				else
				{
					Log.Error("index " + num + " out of bounds " + mTrackingIds.Count);
				}
			}
			if (flag3)
			{
				_invis.Add(_id);
			}
			else
			{
				_vis.Add(_id);
			}
			return !flag2;
		}

		public int GetId(int _i)
		{
			return mTrackingIds[_i];
		}

		public void Clear()
		{
			mTrackingIds.Clear();
		}
	}
}
