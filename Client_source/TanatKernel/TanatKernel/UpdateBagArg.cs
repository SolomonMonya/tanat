using System.Collections.Generic;

namespace TanatKernel
{
	public class UpdateBagArg
	{
		public class UpdData
		{
			public int mItemId;

			public int mArticleId;

			public int mCount;

			public int mUsed;
		}

		public List<UpdData> mUpdItems = new List<UpdData>();
	}
}
