using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class QuestUpdateArgMpdParser : CtrlPacketArgParser<QuestUpdateArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref QuestUpdateArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			MixedArray mixedArray = SafeGet<MixedArray>(args, "quests");
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				QuestStateData item = ParseQuestData(item2);
				_arg.mCurStateData.Add(item);
			}
		}

		public static QuestStateData ParseQuestData(KeyValuePair<string, Variable> _packetData)
		{
			MixedArray mixedArray = _packetData.Value;
			QuestStateData questStateData = new QuestStateData();
			questStateData.mQuestId = int.Parse(_packetData.Key);
			questStateData.mStatus = mixedArray.TryGet("status", -1);
			questStateData.mCooldownTime = mixedArray.TryGet("time", -1);
			if (mixedArray.Associative.TryGetValue("progress", out var value))
			{
				MixedArray mixedArray2 = value;
				{
					foreach (KeyValuePair<string, Variable> item in mixedArray2.Associative)
					{
						int key = int.Parse(item.Key);
						int value2 = item.Value;
						questStateData.mCurProgress.Add(key, value2);
					}
					return questStateData;
				}
			}
			return questStateData;
		}
	}
}
