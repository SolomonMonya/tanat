namespace TanatKernel
{
	internal class CanReconnectArgParser : CtrlPacketArgParser<CanReconnectArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CanReconnectArg _arg)
		{
			_arg.mAnswer = SafeGet(_packet, "answer", _default: false);
			_arg.mTimer = SafeGet(_packet, "timer", 0f);
		}
	}
}
