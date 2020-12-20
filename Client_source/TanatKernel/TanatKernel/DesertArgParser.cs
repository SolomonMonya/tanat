namespace TanatKernel
{
	internal class DesertArgParser : CtrlPacketArgParser<DesertArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref DesertArg _arg)
		{
			_arg.mRequestId = SafeGet(_packet, "request_id", -1);
			_arg.mInQueue = _packet.Arguments.TryGet("in_queue", _default: false);
		}
	}
}
