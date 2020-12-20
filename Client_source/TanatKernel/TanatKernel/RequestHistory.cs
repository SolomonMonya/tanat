using System;
using System.Collections.Generic;

namespace TanatKernel
{
	internal class RequestHistory
	{
		private CastleStartRequestArg mStartBattle;

		private CastleSelectAvatarTimerArg mTimer;

		private DateTime mLastRequest;

		private Dictionary<int, int> mAvatars = new Dictionary<int, int>();

		private List<int> mReady = new List<int>();

		private List<int> mDeserted = new List<int>();

		public void Clear()
		{
			mStartBattle = null;
			mTimer = null;
			mLastRequest = DateTime.Now;
			mAvatars.Clear();
			mReady.Clear();
			mDeserted.Clear();
		}

		public void AvatarSelected(int _id, int _avatar)
		{
			if (mStartBattle != null)
			{
				mAvatars[_id] = _avatar;
			}
		}

		public void SetReady(int _id)
		{
			if (mStartBattle != null && !mReady.Contains(_id))
			{
				mReady.Add(_id);
			}
		}

		public void SetDesert(int _id)
		{
			if (mStartBattle != null && !mDeserted.Contains(_id))
			{
				mDeserted.Add(_id);
			}
		}

		public void SetRequest(CastleStartRequestArg _battle)
		{
			mStartBattle = _battle;
		}

		public void SetTimer(CastleSelectAvatarTimerArg _timer)
		{
			mTimer = _timer;
			mLastRequest = DateTime.Now.AddSeconds(_timer.mTimer);
		}

		public void BindGui(ICastleJoinGui _gui)
		{
			if (mStartBattle == null)
			{
				return;
			}
			_gui.StartCastleBattle(mStartBattle);
			int num = (int)(mLastRequest - DateTime.Now).TotalSeconds;
			if (num < 0)
			{
				num = 0;
			}
			_gui.SetAvatarTimer(num, mTimer.mFightersId);
			foreach (KeyValuePair<int, int> mAvatar in mAvatars)
			{
				_gui.SetAvatar(mAvatar.Key, mAvatar.Value);
			}
			foreach (int item in mReady)
			{
				_gui.SetReadyStatus(item);
			}
			foreach (int item2 in mDeserted)
			{
				_gui.PlayerDeserted(item2);
			}
		}
	}
}
