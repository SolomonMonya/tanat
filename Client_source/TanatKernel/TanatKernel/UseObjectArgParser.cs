namespace TanatKernel
{
	internal class UseObjectArgParser : BattlePacketArgParser<UseObjectArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref UseObjectArg _arg)
		{
			_arg.mObjId = _packet.Request.Arguments["id"];
		}
	}
}
