using AMF;

namespace TanatKernel
{
	internal class HeroMoneyMpdArgParser : CtrlPacketArgParser<HeroMoneyMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroMoneyMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mMoney.mMoney = SafeGet(args, "money", 0);
			_arg.mMoney.mDiamondMoney = SafeGet(args, "money_d", 0);
		}
	}
}
