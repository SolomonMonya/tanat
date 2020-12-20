namespace TanatKernel
{
	internal class PickedUpArgParser : BattlePacketArgParser<PickedUpArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref PickedUpArg _arg)
		{
			_arg.mItemId = _packet.Request.Arguments["id"];
		}
	}
}
