using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ChatConfArgParser : CtrlPacketArgParser<ChatConfArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ChatConfArg _arg)
		{
			_arg.mMpdHost = SafeGet<string>(_packet, "chat_server_host");
			List<Variable> dense = SafeGet<MixedArray>(_packet, "chat_server_port").Dense;
			_arg.mMpdPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mMpdPorts[i] = dense[i];
			}
			_arg.mUid = SafeGet<int>(_packet, "chat_server_uid");
			_arg.mSid = SafeGet<string>(_packet, "chat_server_sid");
		}
	}
}
