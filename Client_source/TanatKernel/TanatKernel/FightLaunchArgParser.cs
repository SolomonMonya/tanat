using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class FightLaunchArgParser : CtrlPacketArgParser<FightLaunchArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightLaunchArg _arg)
		{
			MixedArray data = SafeGet<MixedArray>(_packet, "arguments");
			Parse(data, _arg);
		}

		public static void Parse(MixedArray _data, FightLaunchArg _arg)
		{
			_arg.mHost = _data.TryGet("ip", "");
			List<Variable> dense = _data.TryGet("port", new MixedArray()).Dense;
			_arg.mPorts = new int[dense.Count];
			for (int i = 0; i < dense.Count; i++)
			{
				_arg.mPorts[i] = dense[i];
			}
			_arg.mPasswd = _data.TryGet("passwd", "");
			_arg.mMap = _data.TryGet("scene", "");
			if (_data.Associative.ContainsKey("map_id"))
			{
				_arg.mMapId = _data.TryGet("map_id", 0);
			}
		}
	}
}
