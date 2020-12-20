using System.Collections.Generic;

namespace TanatKernel
{
	public class HeroDataListArg
	{
		public class HeroDataItem
		{
			public bool mPersExists;

			public int mHeroId;

			public HeroView mView = new HeroView();

			public List<DressedItemsArg.DressedItem> mItems = new List<DressedItemsArg.DressedItem>();

			public int mClanId = -1;

			public string mClanTag;

			public int mLevel = -1;

			public int mExp;

			public int mNextExp;

			public int mRating = -1;

			public void AddItem(DressedItemsArg.DressedItem _item)
			{
				if (_item == null)
				{
					return;
				}
				foreach (DressedItemsArg.DressedItem mItem in mItems)
				{
					if (mItem.mId == _item.mId)
					{
						return;
					}
				}
				mItems.Add(_item);
			}

			public void RemoveItem(int _itemId)
			{
				foreach (DressedItemsArg.DressedItem mItem in mItems)
				{
					if (mItem.mId == _itemId)
					{
						mItems.Remove(mItem);
						break;
					}
				}
			}

			public void RemoveItemBySlot(int _slot)
			{
				foreach (DressedItemsArg.DressedItem mItem in mItems)
				{
					if (mItem.mSlot == _slot)
					{
						mItems.Remove(mItem);
						break;
					}
				}
			}
		}

		public List<HeroDataItem> mItems = new List<HeroDataItem>();
	}
}
