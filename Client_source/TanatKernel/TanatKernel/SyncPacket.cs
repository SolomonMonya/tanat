using System;
using System.Collections;
using System.Collections.Generic;
using Network;

namespace TanatKernel
{
	internal class SyncPacket
	{
		private List<SyncData> mData = new List<SyncData>();

		private List<int> mVisibleIds = new List<int>();

		private List<int> mInvisibleIds = new List<int>();

		private List<int> mRelevantIds = new List<int>();

		private List<int> mUnrelevantIds = new List<int>();

		private float mTime;

		public ICollection<SyncData> Data => mData;

		public ICollection<int> NewVisibleIds => mVisibleIds;

		public ICollection<int> NewInvisibleIds => mInvisibleIds;

		public ICollection<int> NewRelevantIds => mRelevantIds;

		public ICollection<int> NewUnrelevantIds => mUnrelevantIds;

		public float Time => mTime;

		public void Parse(byte[] _buffer, TrackingIdManager _trackingIdMgr)
		{
			int num = 0;
			if (num + 4 > _buffer.Length)
			{
				throw new NetSystemException("wrong buffer size 1");
			}
			mTime = BitConverter.ToSingle(_buffer, num);
			num += 4;
			if (num + 2 > _buffer.Length)
			{
				throw new NetSystemException("wrong buffer size 2");
			}
			short num2 = BitConverter.ToInt16(_buffer, num);
			num += 2;
			if (num2 > 0)
			{
				if (num + 4 * num2 > _buffer.Length)
				{
					throw new NetSystemException("wrong buffer size 3");
				}
				int[] array = new int[num2];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = BitConverter.ToInt32(_buffer, num);
					num += 4;
				}
				int[] array2 = array;
				foreach (int num3 in array2)
				{
					int _id = num3;
					if (_trackingIdMgr.RegTrackingId(ref _id, mVisibleIds, mInvisibleIds))
					{
						mRelevantIds.Add(_id);
					}
					else
					{
						mUnrelevantIds.Add(_id);
					}
				}
			}
			if (num + 2 > _buffer.Length)
			{
				throw new NetSystemException("wrong buffer size 4");
			}
			ulong value = BitConverter.ToUInt64(_buffer, num);
			num += 8;
			BitArray bitArray = new BitArray(BitConverter.GetBytes(value));
			Queue<int> queue = new Queue<int>();
			for (int k = 0; k < bitArray.Length; k++)
			{
				if (bitArray.Get(k))
				{
					queue.Enqueue(k);
				}
			}
			int num4 = _trackingIdMgr.IdsCount / 8;
			if (_trackingIdMgr.IdsCount % 8 > 0)
			{
				num4++;
			}
			byte[] array3 = new byte[num4];
			foreach (int item in queue)
			{
				SyncType type = (SyncType)(1L << item);
				int valuesCount = GetValuesCount(type);
				if (valuesCount < 0)
				{
					throw new NetSystemException("unknown sync value type");
				}
				if (num + num4 > _buffer.Length)
				{
					throw new NetSystemException("wrong buffer size 5");
				}
				Array.Copy(_buffer, num, array3, 0, num4);
				num += num4;
				BitArray bitArray2 = new BitArray(array3);
				for (int l = 0; l < bitArray2.Length; l++)
				{
					if (bitArray2.Get(l))
					{
						if (num + 4 * valuesCount > _buffer.Length)
						{
							throw new NetSystemException("wrong buffer size 6");
						}
						if (l < 0 || l >= _trackingIdMgr.IdsCount)
						{
							throw new NetSystemException("wrong tracking id index " + l + ", count " + _trackingIdMgr.IdsCount);
						}
						int id = _trackingIdMgr.GetId(l);
						SyncData syncData = new SyncData(id, type);
						syncData.Parse(valuesCount, _buffer, ref num);
						mData.Add(syncData);
					}
				}
			}
			if (num != _buffer.Length)
			{
				throw new NetSystemException("wrong buffer size. offset " + num + ". buffer length " + _buffer.Length);
			}
		}

		private int GetValuesCount(SyncType _type)
		{
			return _type switch
			{
				SyncType.POSITION => 5, 
				SyncType.POS_ANGLE => 3, 
				_ => 1, 
			};
		}
	}
}
