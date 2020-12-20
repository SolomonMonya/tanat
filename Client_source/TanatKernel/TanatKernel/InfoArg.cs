using System.Collections.Generic;

namespace TanatKernel
{
	public class InfoArg
	{
		public struct User
		{
			public int mId;

			public string mName;

			public Clan.Role mRole;

			public string mLocation;
		}

		public int mId;

		public string mTag;

		public int mLevel;

		public int mRating;

		public string mName;

		public List<User> mUsers = new List<User>();
	}
}
