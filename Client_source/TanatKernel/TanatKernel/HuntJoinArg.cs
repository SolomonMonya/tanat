using System.Collections.Generic;

namespace TanatKernel
{
	public class HuntJoinArg
	{
		public List<MapAvatarData> mAvatars = new List<MapAvatarData>();

		public Dictionary<string, AddStat> mAddStats = new Dictionary<string, AddStat>();

		public int mMapId;

		public Fighter mFighter;
	}
}
