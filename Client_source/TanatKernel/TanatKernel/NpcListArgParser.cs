using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class NpcListArgParser : CtrlPacketArgParser<NpcListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref NpcListArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "npcs");
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				int key = int.Parse(item2.Key);
				MixedArray args = item2.Value;
				NpcData npcData = new NpcData();
				npcData.mName = SafeGet<string>(args, "name");
				npcData.mDesc = SafeGet<string>(args, "desc");
				npcData.mIcon = SafeGet<string>(args, "icon");
				npcData.mNeedShow = SafeGet<bool>(args, "need_show");
				MixedArray mixedArray2 = SafeGet(args, "quests", new MixedArray());
				foreach (Variable item3 in mixedArray2.Dense)
				{
					int item = item3;
					npcData.mQuests.Add(item);
				}
				_arg.mNpcs.Add(key, npcData);
			}
		}
	}
}
