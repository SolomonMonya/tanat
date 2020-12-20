using System.Collections.Generic;

namespace TanatKernel
{
	public class GroupListArg
	{
		public struct Member
		{
			public int mId;

			public string mName;

			public bool mIsOnline;

			public int mClanId;

			public string mClanTag;

			public int mLevel;

			public int mRace;

			public bool mGender;
		}

		public List<Member> mMembers = new List<Member>();

		public int mLeaderId;
	}
}
