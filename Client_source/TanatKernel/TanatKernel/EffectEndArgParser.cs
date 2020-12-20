namespace TanatKernel
{
	internal class EffectEndArgParser : BattlePacketArgParser<EffectEndArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref EffectEndArg _arg)
		{
			_arg.mEffectId = SafeGet(_packet, "id", -1);
		}
	}
}
