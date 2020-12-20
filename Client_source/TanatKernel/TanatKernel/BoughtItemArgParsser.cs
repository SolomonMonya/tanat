namespace TanatKernel
{
	internal class BoughtItemArgParsser : BattlePacketArgParser<BoughtItemArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref BoughtItemArg _arg)
		{
			_arg.mItemProtoId = _packet.Request.Arguments["itemId"];
		}
	}
}
