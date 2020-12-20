using System.Collections.Generic;

namespace TanatKernel
{
	internal class QuestStateData
	{
		public int mQuestId;

		public int mStatus = -1;

		public int mCooldownTime = -1;

		public Dictionary<int, int> mCurProgress = new Dictionary<int, int>();
	}
}
