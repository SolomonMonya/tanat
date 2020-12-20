using System.Collections.Generic;

namespace TanatKernel
{
	public class JoinArg
	{
		public List<MapAvatarData> mAvatars = new List<MapAvatarData>();

		public Dictionary<string, AddStat> mAddStats = new Dictionary<string, AddStat>();

		public int mMapId;
	}
}
