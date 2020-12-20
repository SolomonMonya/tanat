namespace TanatKernel
{
	internal class JoinFromGroupArgParser : CtrlPacketArgParser<JoinFromGroupArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinFromGroupArg _arg)
		{
			_arg.mInGroup = _packet.Arguments.TryGet("user_in_group", _default: false);
		}
	}
}
