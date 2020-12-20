namespace TanatKernel
{
	internal class RemoveAffectorArgParser : BattlePacketArgParser<RemoveAffectorArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref RemoveAffectorArg _arg)
		{
			_arg.mEffectorId = SafeGet(_packet, "id", -1);
		}
	}
}
