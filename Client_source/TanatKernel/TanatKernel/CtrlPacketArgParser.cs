using System;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	internal abstract class CtrlPacketArgParser<ArgT> : ArgumentParser<CtrlPacket, ArgT> where ArgT : new()
	{
		public virtual bool Parse(CtrlPacket _packet, out ArgT _arg)
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
			catch (NetSystemException)
			{
			}
			_arg = default(ArgT);
			return false;
		}

		protected T SafeGet<T>(CtrlPacket _packet, string _key, T _default)
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

		protected T SafeGet<T>(CtrlPacket _packet, string _key)
		{
			if (_packet == null)
			{
				throw new ArgumentNullException("_packet");
			}
			if (!_packet.Arguments.TryGet(_key, default(T), out var _val))
			{
				throw new NetSystemException("invalid packet content - key not found: " + _key + " at " + _packet.Id);
			}
			return _val;
		}

		protected T SafeGet<T>(MixedArray _args, string _key, T _default)
		{
			if (_args == null)
			{
				throw new ArgumentNullException("_args");
			}
			if (!_args.TryGet(_key, _default, out var _val))
			{
				Log.Warning("key not found: " + _key);
			}
			return _val;
		}

		protected T SafeGet<T>(MixedArray _args, string _key)
		{
			if (_args == null)
			{
				throw new ArgumentNullException("_args");
			}
			if (!_args.TryGet(_key, default(T), out var _val))
			{
				throw new NetSystemException("invalid packet content - key not found: " + _key);
			}
			return _val;
		}

		protected abstract void ParseArg(CtrlPacket _root, ref ArgT _arg);
	}
}
