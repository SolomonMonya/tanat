using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class Hero : IStorable
	{
		private int mId;

		private UserPersonalInfo mPersonalInfo;

		private HeroView mView;

		private HeroGameInfo mGameInfo;

		public Action<HeroGameInfo> mGameInfoUpdated;

		public Action<HeroGameInfo> mBuffsUpdated;

		public Action<string> mTagUpdated;

		private Dictionary<int, CtrlThing> mDressedItems = new Dictionary<int, CtrlThing>();

		public int Id => mId;

		public UserPersonalInfo PersonalInfo
		{
			get
			{
				if (mPersonalInfo == null)
				{
					mPersonalInfo = new UserPersonalInfo();
				}
				return mPersonalInfo;
			}
		}

		public HeroView View
		{
			get
			{
				if (mView == null)
				{
					mView = new HeroView();
				}
				return mView;
			}
		}

		public HeroGameInfo GameInfo
		{
			get
			{
				if (mGameInfo == null)
				{
					mGameInfo = new HeroGameInfo();
				}
				return mGameInfo;
			}
		}

		public IEnumerable<CtrlThing> DressedItems => mDressedItems.Values;

		public IEnumerable<KeyValuePair<int, CtrlThing>> DressedItemsWithSlots => mDressedItems;

		public Hero(int _id)
		{
			mId = _id;
		}

		public void SetPersonalInfo(UserPersonalInfo _personalInfo)
		{
			if (_personalInfo == null)
			{
				throw new ArgumentNullException("_personalInfo");
			}
			mPersonalInfo = _personalInfo;
		}

		public void SetView(HeroView _view)
		{
			if (_view == null)
			{
				throw new ArgumentNullException("_view");
			}
			mView = _view;
		}

		public void SetGameInfo(HeroGameInfo _gameInfo)
		{
			if (_gameInfo == null)
			{
				throw new ArgumentNullException("_gameInfo");
			}
			if (GameInfo.mClanTag != _gameInfo.mClanTag && mTagUpdated != null)
			{
				mTagUpdated(_gameInfo.mClanTag);
			}
			mGameInfo = _gameInfo;
			if (mGameInfoUpdated != null)
			{
				mGameInfoUpdated(mGameInfo);
			}
			if (mBuffsUpdated != null)
			{
				mBuffsUpdated(mGameInfo);
			}
		}

		public void SetClanInfo(int _id, string _tag)
		{
			if (mGameInfo.mClanTag != _tag && mTagUpdated != null)
			{
				mTagUpdated(_tag);
			}
			mGameInfo.mClanId = _id;
			mGameInfo.mClanTag = _tag;
		}

		public void UpdateGameInfo(HeroGameInfo _gameInfo)
		{
			HeroGameInfo gameInfo = GameInfo;
			gameInfo.Update(_gameInfo);
		}

		public void SetBuffs(Dictionary<int, DateTime> _newBuffs)
		{
			if (_newBuffs == null)
			{
				throw new ArgumentNullException("_newBuffs");
			}
			GameInfo.mBuffs = _newBuffs;
			if (mBuffsUpdated != null)
			{
				mBuffsUpdated(GameInfo);
			}
		}

		public CtrlThing GetItemAtSlot(int _slot)
		{
			mDressedItems.TryGetValue(_slot, out var value);
			return value;
		}

		public void UndressAll()
		{
			mDressedItems.Clear();
		}

		public void Dress(int _slot, CtrlThing _item)
		{
			if (_item == null)
			{
				throw new ArgumentNullException("_item");
			}
			mDressedItems[_slot] = _item;
		}

		public CtrlThing Undress(int _slot)
		{
			if (!mDressedItems.TryGetValue(_slot, out var value))
			{
				return null;
			}
			mDressedItems.Remove(_slot);
			return value;
		}
	}
}
