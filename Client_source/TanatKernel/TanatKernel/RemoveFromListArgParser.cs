namespace TanatKernel
{
	internal class RemoveFromListArgParser : CtrlPacketArgParser<RemoveFromListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemoveFromListArg _arg)
		{
			_arg.mType = (ListType)SafeGet<int>(_packet, "type");
		}
	}
}
