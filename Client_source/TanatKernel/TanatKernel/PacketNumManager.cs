using System.Collections.Generic;

namespace TanatKernel
{
	internal class PacketNumManager : IPacketNumManager
	{
		private object mSync = new object();

		private Dictionary<int, BattlePacket> mWaitings = new Dictionary<int, BattlePacket>();

		private int mNextNum = 1;

		private Queue<int> mFreeNums = new Queue<int>();

		public void Register(BattlePacket _packet, out int _num)
		{
			lock (mSync)
			{
				if (mFreeNums.Count > 0)
				{
					_num = mFreeNums.Dequeue();
				}
				else
				{
					_num = mNextNum++;
				}
				mWaitings[_num] = _packet;
			}
		}

		public bool Unregister(int _num, out BattlePacket _request)
		{
			lock (mSync)
			{
				if (!mWaitings.TryGetValue(_num, out _request))
				{
					return false;
				}
				mWaitings.Remove(_num);
				mFreeNums.Enqueue(_num);
			}
			return true;
		}

		public void Clear()
		{
			lock (mSync)
			{
				mWaitings.Clear();
				mNextNum = 1;
				mFreeNums.Clear();
			}
		}
	}
}
