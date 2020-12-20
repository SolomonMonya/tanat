namespace TanatKernel
{
	internal class KillArgParser : BattlePacketArgParser<KillArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref KillArg _arg)
		{
			_arg.mKillerId = SafeGet(_packet, "killer", -1);
			_arg.mVictimId = SafeGet(_packet, "id", -1);
		}
	}
}
