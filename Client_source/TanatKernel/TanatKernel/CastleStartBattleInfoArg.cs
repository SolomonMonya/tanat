using System;

namespace TanatKernel
{
	public class CastleStartBattleInfoArg
	{
		public enum WaitType
		{
			OWNER = 1,
			TWO_HOURS,
			FIVE_MINUTES
		}

		public WaitType mType;

		public string mCastleName;

		public DateTime mStartTime;

		public int mStage;

		public int mRound;
	}
}
