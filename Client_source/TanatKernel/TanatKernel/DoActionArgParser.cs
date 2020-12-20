namespace TanatKernel
{
	internal class DoActionArgParser : BattlePacketArgParser<DoActionArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref DoActionArg _arg)
		{
			_arg.mObjId = _packet.Request.Arguments["id"];
			_arg.mTargetId = _packet.Request.Arguments["target"];
			_arg.mActionId = _packet.Request.Arguments["action"];
		}
	}
}
