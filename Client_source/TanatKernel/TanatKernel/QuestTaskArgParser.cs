namespace TanatKernel
{
	internal class QuestTaskArgParser : BattlePacketArgParser<QuestTaskArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref QuestTaskArg _arg)
		{
			_arg.mQuestId = SafeGet(_packet, "task", -1);
			_arg.mState = SafeGet(_packet, "state", -1);
			_arg.mLimit = SafeGet(_packet, "limit", -1);
			_arg.mEndTime = SafeGet(_packet, "time", 0f);
		}
	}
}
