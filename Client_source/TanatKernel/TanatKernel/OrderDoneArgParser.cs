namespace TanatKernel
{
	internal class OrderDoneArgParser : BattlePacketArgParser<OrderDoneArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref OrderDoneArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mActionId = SafeGet(_packet, "action", -1);
		}
	}
}
