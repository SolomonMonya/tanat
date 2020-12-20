namespace TanatKernel
{
	internal class JoinToGroupArgParser : CtrlPacketArgParser<JoinToGroupArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinToGroupArg _arg)
		{
			_arg.mNotInGroup = _packet.Arguments.TryGet("not_in_group", _default: false);
		}
	}
}
