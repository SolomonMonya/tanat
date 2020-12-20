using System.Collections.Generic;

namespace TanatKernel
{
	public class CastleMembersArg
	{
		public Dictionary<int, string> mMembers = new Dictionary<int, string>();

		public Dictionary<int, string> mQueue = new Dictionary<int, string>();

		public bool mJoined;

		public bool mCanJoin;

		public bool mRightLevel;

		public bool mInProgress;

		public int mBanCount;
	}
}
