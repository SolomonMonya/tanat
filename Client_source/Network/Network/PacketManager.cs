namespace Network
{
	public abstract class PacketManager<PacketT, IdT>
	{
		private int mPause;

		private HandlerManager<PacketT, IdT> mHandlerMgr = new HandlerManager<PacketT, IdT>();

		public HandlerManager<PacketT, IdT> HandlerMgr => mHandlerMgr;

		public void Suspend()
		{
			mPause++;
		}

		public void Resume()
		{
			mPause--;
			if (mPause < 0)
			{
				mPause = 0;
			}
		}

		public virtual void Update()
		{
			if (mPause != 0)
			{
				return;
			}
			PacketT _packet;
			IdT _id;
			while (GetNextIncomingPacket(out _packet, out _id))
			{
				mHandlerMgr.Perform(_packet, _id);
				if (mPause != 0)
				{
					break;
				}
			}
		}

		protected abstract bool GetNextIncomingPacket(out PacketT _packet, out IdT _id);
	}
}
