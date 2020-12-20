using System;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	internal abstract class BattlePacketArgParser<ArgT> : ArgumentParser<BattlePacket, ArgT> where ArgT : new()
	{
		public virtual bool Parse(BattlePacket _packet, out ArgT _arg)
		{
			try
			{
				if (_packet.Arguments != null)
				{
					_arg = new ArgT();
					ParseArg(_packet, ref _arg);
					return true;
				}
			}
			catch (NetSystemException ex)
			{
				Log.Error("error while parsing packet: " + ex.Message);
			}
			_arg = default(ArgT);
			return false;
		}

		protected T SafeGet<T>(BattlePacket _packet, string _key, T _default)
		{
			if (_packet == null)
			{
				throw new ArgumentNullException("_packet");
			}
			if (!_packet.Arguments.TryGet(_key, _default, out var _val))
			{
				Log.Warning("key not found: " + _key + " at " + _packet.Id);
			}
			return _val;
		}

		protected abstract void ParseArg(BattlePacket _packet, ref ArgT _arg);
	}
}
