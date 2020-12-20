namespace TanatKernel
{
	internal class HeroStatsArgParser : CtrlPacketArgParser<HeroStatsArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroStatsArg _arg)
		{
			_arg.mLevel = SafeGet(_packet, "level", 0);
			_arg.mMoney = SafeGet(_packet, "money", 0);
		}
	}
}
