using System.Collections.Generic;

namespace TanatKernel
{
	public class UserBagArg
	{
		public class BagItem
		{
			public int mId;

			public int mArticleId;

			public int mCount;

			public int mUsed;
		}

		public List<BagItem> mItems = new List<BagItem>();

		public int mUserMoney;
	}
}
