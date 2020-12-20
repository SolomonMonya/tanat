using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class LaunchMpdArgParser : CtrlPacketArgParser<LaunchArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref LaunchArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mHost = SafeGet<string>(mixedArray, "ip");
			List<Variable> dense = SafeGet<MixedArray>(mixedArray, "port").Dense;
			_arg.mPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mPorts[i] = dense[i];
			}
			_arg.mPasswd = SafeGet<string>(mixedArray, "passwd");
			_arg.mMap = SafeGet<string>(mixedArray, "scene");
			_arg.mWait = SafeGet(mixedArray, "wait", -1);
			_arg.mServTime = mixedArray.TryGet("current_time", 0);
		}
	}
}
