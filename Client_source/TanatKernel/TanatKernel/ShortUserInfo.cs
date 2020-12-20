namespace TanatKernel
{
	public class ShortUserInfo
	{
		public enum Status
		{
			OFFLINE,
			CS,
			BATTLE
		}

		public int mId;

		public string mName;

		public string mTag;

		public int mClanId;

		public int mLevel;

		public int mRating;

		public Status mOnline;
	}
}
