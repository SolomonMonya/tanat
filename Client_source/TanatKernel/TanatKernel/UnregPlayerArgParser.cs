namespace TanatKernel
{
	internal class UnregPlayerArgParser : BattlePacketArgParser<UnregPlayerArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref UnregPlayerArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "id", -1);
		}
	}
}
