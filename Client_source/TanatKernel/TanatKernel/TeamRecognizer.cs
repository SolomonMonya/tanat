using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class TeamRecognizer
	{
		private struct Group
		{
			public bool mIsNeutral;

			public List<int> mGroupTeams;
		}

		private class SelfData
		{
			public Friendliness mFriendliness;

			public bool mIsNotInGroup = true;
		}

		private int mSelfTeam;

		private bool mInited;

		private Dictionary<int, Group> mTeams = new Dictionary<int, Group>();

		private bool mIsAllNeutrals;

		private SelfData mSelfData;

		public void SetSelfTeam(int _team)
		{
			mSelfTeam = _team;
			mInited = true;
			mSelfData = null;
		}

		public void SetAllNeutrals(bool _isAllNeutrals)
		{
			mIsAllNeutrals = _isAllNeutrals;
			mSelfData = null;
		}

		public void AddTeams(IEnumerable<int> _teams, bool _isNeutral)
		{
			if (_teams == null)
			{
				throw new ArgumentNullException("_teams");
			}
			List<int> mGroupTeams = new List<int>(_teams);
			foreach (int _team in _teams)
			{
				Group value = default(Group);
				value.mIsNeutral = _isNeutral;
				value.mGroupTeams = mGroupTeams;
				mTeams[_team] = value;
			}
			mSelfData = null;
		}

		public Friendliness GetFriendliness(SyncedParams _params)
		{
			if (_params == null)
			{
				throw new ArgumentNullException("_params");
			}
			if (!mInited || !_params.IsTeamInited)
			{
				return Friendliness.UNKNOWN;
			}
			if (mSelfData == null)
			{
				mSelfData = new SelfData();
				if (mSelfTeam > 0)
				{
					if (mTeams.TryGetValue(mSelfTeam, out var value))
					{
						mSelfData.mIsNotInGroup = false;
						if (value.mIsNeutral)
						{
							mSelfData.mFriendliness = Friendliness.NEUTRAL;
						}
						else
						{
							mSelfData.mFriendliness = Friendliness.ENEMY;
						}
					}
				}
				else if (mSelfTeam < 0)
				{
					mSelfData.mFriendliness = Friendliness.ENEMY;
				}
				else if (mSelfTeam == 0)
				{
					mSelfData.mFriendliness = Friendliness.NEUTRAL;
				}
			}
			if (mSelfData.mFriendliness == Friendliness.NEUTRAL)
			{
				return Friendliness.NEUTRAL;
			}
			int team = _params.Team;
			if (team == mSelfTeam)
			{
				return Friendliness.FRIEND;
			}
			if (team == 0)
			{
				return Friendliness.NEUTRAL;
			}
			if (team < 0)
			{
				return Friendliness.ENEMY;
			}
			if (mTeams.TryGetValue(team, out var value2))
			{
				if (value2.mIsNeutral)
				{
					return Friendliness.NEUTRAL;
				}
				if (!mSelfData.mIsNotInGroup && value2.mGroupTeams.Contains(mSelfTeam))
				{
					return Friendliness.NEUTRAL;
				}
				return Friendliness.ENEMY;
			}
			if (mSelfData.mFriendliness == Friendliness.ENEMY)
			{
				return Friendliness.ENEMY;
			}
			if (mSelfData.mIsNotInGroup)
			{
				if (!mIsAllNeutrals)
				{
					return Friendliness.ENEMY;
				}
				return Friendliness.NEUTRAL;
			}
			if (!mIsAllNeutrals)
			{
				return Friendliness.NEUTRAL;
			}
			return Friendliness.ENEMY;
		}
	}
}
