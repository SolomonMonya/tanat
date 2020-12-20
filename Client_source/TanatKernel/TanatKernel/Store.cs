using System.Collections.Generic;
using Log4Tanat;

namespace TanatKernel
{
	public class Store<T> where T : class, IStorable
	{
		public delegate void AddCallback(Store<T> _store, T _obj);

		public delegate void RemoveCallback(Store<T> _store, T _obj);

		public delegate void ClearCallback(Store<T> _store);

		public AddCallback mAddCallback;

		public RemoveCallback mRemoveCallback;

		public ClearCallback mClearCallback;

		protected string mName;

		private Dictionary<int, T> mObjects = new Dictionary<int, T>();

		public IEnumerable<T> Objects => mObjects.Values;

		public int Count => mObjects.Count;

		public Store(string _name)
		{
			mName = _name;
		}

		public T Get(int _id)
		{
			if (mObjects.TryGetValue(_id, out var value))
			{
				return value;
			}
			Log.Warning("object " + _id + " not exists in " + mName + ". " + Log.StackTrace());
			return null;
		}

		public T TryGet(int _id)
		{
			mObjects.TryGetValue(_id, out var value);
			return value;
		}

		public bool Exists(int _id)
		{
			return mObjects.ContainsKey(_id);
		}

		public virtual void Add(T _obj)
		{
			if (Exists(_obj.Id))
			{
				Log.Warning("object " + _obj.Id + " already exists in " + mName + ".");
				return;
			}
			mObjects.Add(_obj.Id, _obj);
			Log.Info(_obj.Id + " added into " + mName + ".");
			if (mAddCallback != null)
			{
				mAddCallback(this, _obj);
			}
		}

		public virtual void Remove(int _id)
		{
			if (mObjects.TryGetValue(_id, out var value))
			{
				mObjects.Remove(_id);
				Log.Info("object " + _id + " removed from " + mName + ".");
				if (mRemoveCallback != null)
				{
					mRemoveCallback(this, value);
				}
			}
		}

		public virtual void Clear()
		{
			Log.Debug("removing " + mObjects.Count + " objects from " + mName + ".");
			mObjects.Clear();
			if (mClearCallback != null)
			{
				mClearCallback(this);
			}
		}
	}
}
