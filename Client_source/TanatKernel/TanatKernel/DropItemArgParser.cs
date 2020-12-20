namespace TanatKernel
{
	internal class DropItemArgParser : BattlePacketArgParser<DropItemArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref DropItemArg _arg)
		{
			_arg.mObjId = _packet.Request.Arguments["id"];
		}
	}
}
