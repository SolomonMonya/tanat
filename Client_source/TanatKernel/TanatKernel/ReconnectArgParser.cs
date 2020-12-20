using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ReconnectArgParser : CtrlPacketArgParser<ReconnectArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ReconnectArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "conf");
			_arg.mServerDataArg.mHost = SafeGet<string>(args, "ip");
			List<Variable> dense = SafeGet<MixedArray>(args, "port").Dense;
			_arg.mServerDataArg.mPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mServerDataArg.mPorts[i] = dense[i];
			}
			_arg.mServerDataArg.mMap = SafeGet<string>(args, "scene");
			_arg.mServerDataArg.mPasswd = SafeGet<string>(args, "passwd");
			MixedArray args2 = SafeGet<MixedArray>(_packet, "user");
			_arg.mLoginArg.mUserId = SafeGet<int>(args2, "id");
			_arg.mLoginArg.mUserName = SafeGet<string>(args2, "nick");
		}
	}
}
