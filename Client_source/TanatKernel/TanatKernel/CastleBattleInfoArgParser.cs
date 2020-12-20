using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class CastleBattleInfoArgParser : CtrlPacketArgParser<CastleBattleInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleBattleInfoArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "stages");
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args = item;
				int key = SafeGet<int>(args, "stage");
				MixedArray mixedArray2 = SafeGet<MixedArray>(args, "data");
				_arg.mBattles[key] = new List<BattleInfo>();
				foreach (Variable item2 in mixedArray2.Dense)
				{
					BattleInfo battleInfo = new BattleInfo();
					battleInfo.mFirstId = SafeGet<int>(item2, "id1");
					battleInfo.mFirstName = SafeGet<string>(item2, "name1");
					battleInfo.mSecondId = SafeGet<int>(item2, "id2");
					battleInfo.mSecondName = SafeGet<string>(item2, "name2");
					battleInfo.mWinner = SafeGet<int>(item2, "winner");
					MixedArray mixedArray3 = item2;
					MixedArray mixedArray4 = mixedArray3.TryGet<MixedArray>("observer", null);
					if (mixedArray4 != null && mixedArray4.Associative.Count > 0)
					{
						battleInfo.mServerData = new ServerData();
						battleInfo.mServerData.mHost = SafeGet<string>(mixedArray4, "ip");
						List<Variable> dense = SafeGet<MixedArray>(mixedArray4, "port").Dense;
						battleInfo.mServerData.mPorts = new int[dense.Count];
						for (int i = 0; i < dense.Count; i++)
						{
							battleInfo.mServerData.mPorts[i] = dense[i];
						}
						battleInfo.mServerData.mMap = SafeGet<string>(mixedArray4, "scene");
						battleInfo.mServerData.mBattleId = SafeGet<int>(mixedArray4, "battle_id");
					}
					_arg.mBattles[key].Add(battleInfo);
				}
			}
		}
	}
}
