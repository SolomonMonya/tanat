namespace TanatKernel
{
	internal class HeroMoneyArgParser : CtrlPacketArgParser<HeroMoneyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroMoneyArg _arg)
		{
			_arg.mMoney = SafeGet(_packet, "money", 0);
			_arg.mDiamondMoney = SafeGet(_packet, "money_d", 0);
		}
	}
}
