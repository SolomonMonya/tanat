using System;

namespace TanatKernel
{
	public class SyncData
	{
		private int mTrackingId;

		private SyncType mSyncType;

		private object[] mValues;

		public int TrackingId => mTrackingId;

		public SyncType SyncType => mSyncType;

		public int ValuesCount => mValues.Length;

		public SyncData(int _trackingId, SyncType _type)
		{
			mTrackingId = _trackingId;
			mSyncType = _type;
		}

		public T GetValue<T>(int _i)
		{
			return (T)Convert.ChangeType(mValues[_i], typeof(T));
		}

		public void Parse(int _valuesCount, byte[] _buffer, ref int _offset)
		{
			mValues = new object[_valuesCount];
			if (SyncType.TEAM == mSyncType || SyncType.MAG_IMM == mSyncType || SyncType.PHYS_IMM == mSyncType || SyncType.SILENCE == mSyncType)
			{
				ParseInts(_buffer, ref _offset);
			}
			else
			{
				ParseFloats(_buffer, ref _offset);
			}
		}

		private void ParseFloats(byte[] _buffer, ref int _offset)
		{
			for (int i = 0; i < mValues.Length; i++)
			{
				float num = BitConverter.ToSingle(_buffer, _offset);
				_offset += 4;
				mValues[i] = num;
			}
		}

		private void ParseInts(byte[] _buffer, ref int _offset)
		{
			for (int i = 0; i < mValues.Length; i++)
			{
				int num = BitConverter.ToInt32(_buffer, _offset);
				_offset += 4;
				mValues[i] = num;
			}
		}
	}
}
