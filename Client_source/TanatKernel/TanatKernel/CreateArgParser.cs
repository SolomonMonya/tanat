namespace TanatKernel
{
	internal class CreateArgParser : CtrlPacketArgParser<CreateArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CreateArg _arg)
		{
			_arg.mId = SafeGet<int>(_packet, "id");
		}
	}
}
