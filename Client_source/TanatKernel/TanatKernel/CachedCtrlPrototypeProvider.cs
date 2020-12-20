using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class CachedCtrlPrototypeProvider : IStoreContentProvider<CtrlPrototype>
	{
		private PropertyHolder mProperties;

		private Store<CtrlPrototype> mPrototypes;

		private StoreContentProvider<CtrlPrototype> mContentProv;

		public int Count => mContentProv.Count;

		public IEnumerable<CtrlPrototype> Content => mContentProv.Content;

		public CachedCtrlPrototypeProvider(PropertyHolder _ctrlProperties)
		{
			if (_ctrlProperties == null)
			{
				throw new ArgumentNullException("_ctrlProperties");
			}
			mProperties = _ctrlProperties;
			mPrototypes = new Store<CtrlPrototype>("CtrlPrototypes");
			mContentProv = new StoreContentProvider<CtrlPrototype>(mPrototypes);
		}

		public CtrlPrototype Get(int _id)
		{
			CtrlPrototype ctrlPrototype = mContentProv.TryGet(_id);
			if (ctrlPrototype == null)
			{
				ctrlPrototype = new CtrlPrototype(_id, mProperties);
				lock (mPrototypes)
				{
					mPrototypes.Add(ctrlPrototype);
					return ctrlPrototype;
				}
			}
			return ctrlPrototype;
		}

		public CtrlPrototype TryGet(int _id)
		{
			return mContentProv.TryGet(_id);
		}

		public bool Exists(int _id)
		{
			return mContentProv.Exists(_id);
		}
	}
}
