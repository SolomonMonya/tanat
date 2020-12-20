using System.Collections.Generic;

namespace TanatKernel
{
	public class ShopContentArg
	{
		public class Category
		{
			public int mWeight;

			public List<int> mArticles = new List<int>();
		}

		public Dictionary<int, Category> mItems = new Dictionary<int, Category>();
	}
}
