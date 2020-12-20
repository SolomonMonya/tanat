using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class QuestListArgParser : CtrlPacketArgParser<QuestListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref QuestListArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "quests");
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				QuestStateData item = QuestUpdateArgMpdParser.ParseQuestData(item2);
				_arg.mCurStateData.Add(item);
			}
		}
	}
}
