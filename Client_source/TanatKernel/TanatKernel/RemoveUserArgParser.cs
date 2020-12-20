namespace TanatKernel
{
	internal class RemoveUserArgParser : CtrlPacketArgParser<RemoveUserArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemoveUserArg _arg)
		{
			_arg.mId = SafeGet<int>(_packet, "user_id");
		}
	}
}
