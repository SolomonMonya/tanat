using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class Notifier<OwnerT, DataT>
	{
		public class Group
		{
			private Queue<Notifier<OwnerT, DataT>> mNotifiers = new Queue<Notifier<OwnerT, DataT>>();

			public void Add(Notifier<OwnerT, DataT> _notifier)
			{
				mNotifiers.Enqueue(_notifier);
			}

			public void Call(bool _success, OwnerT _owner)
			{
				foreach (Notifier<OwnerT, DataT> mNotifier in mNotifiers)
				{
					mNotifier?.Call(_success, _owner);
				}
				Clear();
			}

			public void Clear()
			{
				mNotifiers.Clear();
			}

			public int GetCount()
			{
				return mNotifiers.Count;
			}
		}

		public delegate void Callback(bool _success, OwnerT _owner, DataT _data);

		public Callback mCallback;

		public DataT mData;

		public Notifier()
		{
		}

		public Notifier(Callback _callback, DataT _data)
		{
			mCallback = (Callback)Delegate.Combine(mCallback, _callback);
			mData = _data;
		}

		public void Call(bool _success, OwnerT _owner)
		{
			if (mCallback != null)
			{
				mCallback(_success, _owner, mData);
			}
		}
	}
}
