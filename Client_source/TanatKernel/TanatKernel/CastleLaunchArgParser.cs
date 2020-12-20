using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class CastleLaunchArgParser : CtrlPacketArgParser<CastleLaunchArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleLaunchArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mHost = SafeGet<string>(args, "ip");
			List<Variable> dense = SafeGet<MixedArray>(args, "port").Dense;
			_arg.mPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mPorts[i] = dense[i];
			}
			_arg.mPasswd = SafeGet<string>(args, "passwd");
			_arg.mMap = SafeGet<string>(args, "scene");
		}
	}
}
