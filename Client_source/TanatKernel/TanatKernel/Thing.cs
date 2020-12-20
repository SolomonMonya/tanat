using System;
using Log4Tanat;

namespace TanatKernel
{
	public abstract class Thing : IStorable
	{
		public enum PlaceType
		{
			HERO,
			AVATAR,
			QUEST
		}

		private int mId;

		private int mCount = 1;

		private int mUsed;

		public int Id => mId;

		public abstract CtrlPrototype CtrlProto
		{
			get;
		}

		public abstract PlaceType Place
		{
			get;
		}

		public int Count => mCount;

		public bool IsEmpty => mCount == 0;

		public int Used => mUsed;

		public int AvailCount => mCount - mUsed;

		public Thing(int _id)
		{
			mId = _id;
		}

		public void Add(int _count)
		{
			if (_count < 0)
			{
				throw new ArgumentException("_count < 0");
			}
			mCount += _count;
		}

		public void Remove(int _count)
		{
			if (_count < 0)
			{
				throw new ArgumentException("_count < 0");
			}
			int num = mCount - _count;
			if (num >= 0)
			{
				mCount = num;
			}
			else
			{
				Log.Error("negative count " + num);
			}
		}

		public void Use(int _count)
		{
			if (_count < 0)
			{
				throw new ArgumentException("_count < 0");
			}
			int num = mUsed + _count;
			if (num <= mCount)
			{
				mUsed = num;
				return;
			}
			Log.Error("try to use " + num + " items with count " + mCount);
		}

		public void Unuse(int _count)
		{
			if (_count < 0)
			{
				throw new ArgumentException("_count < 0");
			}
			int num = mUsed - _count;
			if (num >= 0)
			{
				mUsed = num;
				return;
			}
			Log.Error("unuse " + _count + " items, used " + mUsed);
		}
	}
}
