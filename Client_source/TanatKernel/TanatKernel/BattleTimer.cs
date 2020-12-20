using System;

namespace TanatKernel
{
	public class BattleTimer
	{
		public static float deltaTime;

		private object mLock;

		private bool mInited;

		private float mTimeOnServer;

		private DateTime mSyncTime;

		private float mAimTimeOnServer;

		private DateTime mAimSyncTime;

		private float mRealTime;

		public float Time => mRealTime;

		public void Sync(float _time)
		{
			if (mLock == null)
			{
				mLock = new object();
			}
			lock (mLock)
			{
				mAimTimeOnServer = _time;
				mAimSyncTime = DateTime.Now;
				if (!mInited)
				{
					mTimeOnServer = mAimTimeOnServer;
					mSyncTime = mAimSyncTime;
					mInited = true;
				}
				if (!BattleServerConnection.mIsReady)
				{
					mInited = false;
				}
			}
		}

		public void Update()
		{
			if (mLock == null)
			{
				return;
			}
			lock (mLock)
			{
				float num = (float)DateTime.Now.Subtract(mAimSyncTime).TotalSeconds;
				float num2 = mAimTimeOnServer + num;
				float num3 = (float)DateTime.Now.Subtract(mSyncTime).TotalSeconds;
				float num4 = mTimeOnServer + num3;
				float num5 = num2 - num4;
				float num6 = ((!(deltaTime > 0f)) ? 0.1f : deltaTime);
				if (Math.Abs(num5) > num6)
				{
					num5 = ((!(num5 < 0f)) ? num6 : (0f - num6));
				}
				mTimeOnServer += num5;
				num4 += num5;
				if (num4 > mRealTime)
				{
					mRealTime = num4;
				}
			}
		}
	}
}
