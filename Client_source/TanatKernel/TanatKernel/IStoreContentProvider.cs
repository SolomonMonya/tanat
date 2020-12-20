using System.Collections.Generic;

namespace TanatKernel
{
	public interface IStoreContentProvider<T>
	{
		int Count
		{
			get;
		}

		IEnumerable<T> Content
		{
			get;
		}

		T Get(int _id);

		T TryGet(int _id);

		bool Exists(int _id);
	}
}
