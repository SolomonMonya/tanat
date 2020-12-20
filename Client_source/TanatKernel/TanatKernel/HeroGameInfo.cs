using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class HeroGameInfo
	{
		public int mLevel = -1;

		public int mExp;

		public int mNextExp;

		public int mRating = -1;

		public int mAssists;

		public int mAvatarKills;

		public int mCreepKills;

		public int mDeaths;

		public int mFights;

		public int mLoseFights;

		public int mWinFights;

		public int mLeaves;

		public int mHonor;

		public int mClanId = -1;

		public string mClanTag;

		public Dictionary<string, AddStat> mAddStats = new Dictionary<string, AddStat>();

		public Dictionary<int, DateTime> mBuffs = new Dictionary<int, DateTime>();

		public void Update(HeroGameInfo _other)
		{
			TryAssign(ref mLevel, _other.mLevel);
			TryAssign(ref mExp, _other.mExp);
			TryAssign(ref mNextExp, _other.mNextExp);
			TryAssign(ref mRating, _other.mRating);
			TryAssign(ref mAssists, _other.mAssists);
			TryAssign(ref mAvatarKills, _other.mAvatarKills);
			TryAssign(ref mCreepKills, _other.mCreepKills);
			TryAssign(ref mDeaths, _other.mDeaths);
			TryAssign(ref mFights, _other.mFights);
			TryAssign(ref mLoseFights, _other.mLoseFights);
			TryAssign(ref mWinFights, _other.mWinFights);
			TryAssign(ref mLeaves, _other.mLeaves);
			TryAssign(ref mHonor, _other.mHonor);
			TryAssign(ref mClanId, _other.mClanId);
			if (_other.mClanId >= 0)
			{
				mClanTag = _other.mClanTag;
			}
			mAddStats = new Dictionary<string, AddStat>(_other.mAddStats);
			mBuffs = new Dictionary<int, DateTime>(_other.mBuffs);
		}

		private void TryAssign(ref int _dest, int _val)
		{
			if (_val >= 0)
			{
				_dest = _val;
			}
		}
	}
}
