namespace TanatKernel
{
	internal class FightJoinArgParser : CtrlPacketArgParser<FightJoinArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightJoinArg _arg)
		{
			_arg.mMapId = SafeGet<int>(_packet, "map_id");
			if (_packet.Arguments.Associative.ContainsKey("time"))
			{
				_arg.mTime = SafeGet<int>(_packet, "time");
			}
		}
	}
}
