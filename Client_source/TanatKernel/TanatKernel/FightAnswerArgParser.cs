namespace TanatKernel
{
	internal class FightAnswerArgParser : CtrlPacketArgParser<FightAnswerArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightAnswerArg _arg)
		{
			if (_packet.Arguments.Associative.ContainsKey("map_id"))
			{
				_arg.mMapId = SafeGet<int>(_packet, "map_id");
			}
			if (_packet.Arguments.Associative.ContainsKey("answer"))
			{
				_arg.mAnswer = SafeGet<bool>(_packet, "answer");
			}
		}
	}
}
