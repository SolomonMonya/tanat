using System.Text;
using AMF;

namespace TanatKernel
{
	public class BattleServerData
	{
		public class DebugJoinData
		{
			public int mBattleId;

			public int mMapId;

			public int mTeam;

			public string mAvatarPrefab;

			public MixedArray mAvatarParams;

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("battle: " + mBattleId);
				stringBuilder.AppendLine("map: " + mMapId);
				stringBuilder.AppendLine("team: " + mTeam);
				stringBuilder.AppendLine("avatar: " + mAvatarPrefab);
				return stringBuilder.ToString();
			}
		}

		public DebugJoinData mDebugJoin;

		public string mHost;

		public int[] mPorts;

		public string mPasswd;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("host: " + mHost);
			int[] array = mPorts;
			foreach (int num in array)
			{
				stringBuilder.AppendLine("port: " + num);
			}
			if (mDebugJoin != null)
			{
				stringBuilder.Append(mDebugJoin.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
