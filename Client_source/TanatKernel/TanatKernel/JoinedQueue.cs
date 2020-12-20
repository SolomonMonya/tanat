using System;

namespace TanatKernel
{
	public class JoinedQueue : IJoinedQueue
	{
		private MapData mMapData;

		private DateTime mStartTime;

		public MapData MapData
		{
			get
			{
				return mMapData;
			}
			set
			{
				mMapData = value;
			}
		}

		public DateTime StartTime
		{
			get
			{
				return mStartTime;
			}
			set
			{
				mStartTime = value;
			}
		}
	}
}
