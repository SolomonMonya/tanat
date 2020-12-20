using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ServerDataArgParser : CtrlPacketArgParser<ServerDataArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ServerDataArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "area_conf");
			_arg.mHost = SafeGet<string>(args, "ip");
			List<Variable> dense = SafeGet<MixedArray>(args, "port").Dense;
			_arg.mPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mPorts[i] = dense[i];
			}
			_arg.mMap = SafeGet<string>(args, "scene");
			_arg.mPasswd = SafeGet<string>(args, "passwd");
			_arg.mAreaId = SafeGet<int>(args, "area_id");
			_arg.mNeedLog = SafeGet<int>(_packet, "log");
		}
	}
}
