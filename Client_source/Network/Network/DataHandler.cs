using System;
using System.Runtime.CompilerServices;

namespace Network
{
	public class DataHandler<PacketT, ArgT> : ValidationHandler<PacketT>
	{
		public delegate void RecvDataCallback(ArgT _arg);

		private RecvDataCallback mRecvDataCallback = delegate
		{
		};

		private ArgumentParser<PacketT, ArgT> mParser;

		[CompilerGenerated]
		private static RecvDataCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public void SetParser(ArgumentParser<PacketT, ArgT> _parser)
		{
			mParser = _parser;
		}

		public void SubscribeRecvData(RecvDataCallback _recvData)
		{
			mRecvDataCallback = (RecvDataCallback)Delegate.Combine(mRecvDataCallback, _recvData);
		}

		public void UnsubscribeRecvData(RecvDataCallback _recvData)
		{
			mRecvDataCallback = (RecvDataCallback)Delegate.Remove(mRecvDataCallback, _recvData);
		}

		public override void Unsubscribe(object _receiver)
		{
			base.Unsubscribe(_receiver);
			Delegate[] invocationList = mRecvDataCallback.GetInvocationList();
			Delegate[] array = invocationList;
			foreach (Delegate @delegate in array)
			{
				if (@delegate.Target == _receiver)
				{
					mRecvDataCallback = (RecvDataCallback)Delegate.Remove(mRecvDataCallback, @delegate as RecvDataCallback);
				}
			}
		}

		public override bool Perform(PacketT _packet)
		{
			if (!base.Perform(_packet))
			{
				return false;
			}
			if (mParser != null && mParser.Parse(_packet, out var _arg))
			{
				mRecvDataCallback(_arg);
				return true;
			}
			mFailCallback(0);
			return false;
		}
	}
}
