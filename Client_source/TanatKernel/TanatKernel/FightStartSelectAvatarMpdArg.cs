using System.Collections.Generic;

namespace TanatKernel
{
	public class FightStartSelectAvatarMpdArg
	{
		public List<MapAvatarData> mAvatars = new List<MapAvatarData>();

		public Dictionary<string, AddStat> mAddStats = new Dictionary<string, AddStat>();

		public List<Fighter> mTeam1 = new List<Fighter>();

		public List<Fighter> mTeam2 = new List<Fighter>();

		public int mMapId;

		public int mWaitTime;
	}
}
