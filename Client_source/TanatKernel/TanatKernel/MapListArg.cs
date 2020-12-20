using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class MapListArg
	{
		public int mMapType;

		public int mNextBanTime;

		public DateTime mBanTimeEnd;

		public bool mIsBanned;

		public List<MapData> mMaps = new List<MapData>();
	}
}
