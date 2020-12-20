namespace TanatKernel
{
	internal class EquipItemArgParser : BattlePacketArgParser<EquipItemArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref EquipItemArg _arg)
		{
			_arg.mItemId = _packet.Request.Arguments["id"];
		}
	}
}
