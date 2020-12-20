using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class StoreContentProvider<T> : IStoreContentProvider<T> where T : class, IStorable
	{
		protected Store<T> mStore;

		public int Count
		{
			get
			{
				lock (mStore)
				{
					return mStore.Count;
				}
			}
		}

		public IEnumerable<T> Content => mStore.Objects;

		public StoreContentProvider(Store<T> _store)
		{
			if (_store == null)
			{
				throw new ArgumentNullException("_store");
			}
			mStore = _store;
		}

		public T Get(int _id)
		{
			lock (mStore)
			{
				return mStore.Get(_id);
			}
		}

		public T TryGet(int _id)
		{
			lock (mStore)
			{
				return mStore.TryGet(_id);
			}
		}

		public bool Exists(int _id)
		{
			lock (mStore)
			{
				return mStore.Exists(_id);
			}
		}

		public void Subscribe(Store<T>.AddCallback _addCallback, Store<T>.RemoveCallback _removeCallback)
		{
			Store<T> store = mStore;
			store.mAddCallback = (Store<T>.AddCallback)Delegate.Combine(store.mAddCallback, _addCallback);
			Store<T> store2 = mStore;
			store2.mRemoveCallback = (Store<T>.RemoveCallback)Delegate.Combine(store2.mRemoveCallback, _removeCallback);
		}

		public void Unsubscribe(Store<T>.AddCallback _addCallback, Store<T>.RemoveCallback _removeCallback)
		{
			Store<T> store = mStore;
			store.mAddCallback = (Store<T>.AddCallback)Delegate.Remove(store.mAddCallback, _addCallback);
			Store<T> store2 = mStore;
			store2.mRemoveCallback = (Store<T>.RemoveCallback)Delegate.Remove(store2.mRemoveCallback, _removeCallback);
		}
	}
}
