using System.Collections.Generic;

namespace TanatKernel
{
	public class DressedItemsArg
	{
		public class DressedItem
		{
			public int mId;

			public int mArticleId;

			public int mCount;

			public int mSlot;
		}

		public List<DressedItem> mItems = new List<DressedItem>();

		public int mHeroId;
	}
}
