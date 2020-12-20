using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ObserverArgParser : CtrlPacketArgParser<ObserverArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ObserverArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "observer", new MixedArray());
			if (mixedArray != null && mixedArray.Associative.Count > 0)
			{
				_arg.mServerData = new ServerData();
				_arg.mServerData.mHost = SafeGet<string>(mixedArray, "ip");
				List<Variable> dense = SafeGet<MixedArray>(mixedArray, "port").Dense;
				_arg.mServerData.mPorts = new int[dense.Count];
				for (int i = 0; i < dense.Count; i++)
				{
					_arg.mServerData.mPorts[i] = dense[i];
				}
				_arg.mServerData.mMap = SafeGet<string>(mixedArray, "scene");
				_arg.mServerData.mBattleId = SafeGet<int>(mixedArray, "battle_id");
			}
		}
	}
}
