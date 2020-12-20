namespace TanatKernel
{
	internal class BattleEndArgParser : BattlePacketArgParser<BattleEndArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref BattleEndArg _arg)
		{
			_arg.mWinnterTeam = SafeGet(_packet, "id", -1024);
		}
	}
}
