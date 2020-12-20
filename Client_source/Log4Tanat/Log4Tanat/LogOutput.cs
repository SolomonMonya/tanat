using System.Collections.Generic;

namespace Log4Tanat
{
	public abstract class LogOutput
	{
		private int mMinLevel;

		private bool mAutoFlush;

		private bool mWriteThreadInfo;

		protected IThreadInfoProvider mThreadInfoProv;

		public bool AutoFlush
		{
			get
			{
				return mAutoFlush;
			}
			set
			{
				mAutoFlush = value;
			}
		}

		public bool WriteThreadInfo
		{
			get
			{
				return mWriteThreadInfo;
			}
			set
			{
				mWriteThreadInfo = value;
			}
		}

		public void SetMinLevel(int _minLevel)
		{
			mMinLevel = _minLevel;
		}

		public bool CanWrite(Log.Category _category)
		{
			return (int)_category >= mMinLevel;
		}

		public void SetThreadInfoProvider(IThreadInfoProvider _threadInfoProv)
		{
			mThreadInfoProv = _threadInfoProv;
		}

		public virtual void Init(IDictionary<string, string> _params)
		{
		}

		public virtual void Enable()
		{
		}

		public virtual void Disable()
		{
		}

		public virtual void Flush()
		{
		}

		public abstract void Write(Log.Category _category, string _caller, string _time, string _data);
	}
}
