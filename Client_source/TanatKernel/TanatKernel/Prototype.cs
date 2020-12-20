using System;

namespace TanatKernel
{
	public abstract class Prototype : IStorable
	{
		public class PDesc
		{
			public string mName;

			public string mDesc;

			public string mLongDesc;

			public string mIcon;
		}

		private int mId;

		private PropertyHolder mPropHolder;

		public int Id => mId;

		public abstract PDesc Desc
		{
			get;
		}

		public Prototype(int _id, PropertyHolder _propHolder)
		{
			if (_propHolder == null)
			{
				throw new ArgumentNullException("_propHolder");
			}
			mId = _id;
			mPropHolder = _propHolder;
		}

		protected T GetProperty<T>(ref T _cache) where T : class
		{
			if (_cache == null)
			{
				return _cache = mPropHolder.GetProperty<T>(mId);
			}
			return _cache;
		}
	}
}
