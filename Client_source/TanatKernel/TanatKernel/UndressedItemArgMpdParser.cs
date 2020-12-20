using AMF;

namespace TanatKernel
{
	internal class UndressedItemArgMpdParser : CtrlPacketArgParser<UndressedItemArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref UndressedItemArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mHeroId = SafeGet<int>(args, "user_id");
			_arg.mSlot = SafeGet<int>(args, "slot");
			_arg.mArticleId = SafeGet<int>(args, "item");
		}
	}
}
