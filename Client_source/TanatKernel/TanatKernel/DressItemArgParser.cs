namespace TanatKernel
{
	internal class DressItemArgParser : CtrlPacketArgParser<DressItemArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref DressItemArg _arg)
		{
			_arg.mSlot = SafeGet<int>(_packet, "slot");
			_arg.mItemId = SafeGet<int>(_packet, "item");
			_arg.mArticleId = SafeGet<int>(_packet, "artikul");
		}
	}
}
