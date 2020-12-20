using System;

namespace TanatKernel
{
	public class CastleInfo
	{
		public class Reward
		{
			public int mMoney;

			public int mDiamonds;

			public int mExp;

			public int mItem;

			public int mItemCount;
		}

		public int mId;

		public string mName;

		public int mLevelMin;

		public int mLevelMax;

		public string mOwnerName;

		public int mOwnerId;

		public bool mSelfOwner;

		public int mFightersMin;

		public Reward mReward = new Reward();

		public DateTime mStartTime;
	}
}
