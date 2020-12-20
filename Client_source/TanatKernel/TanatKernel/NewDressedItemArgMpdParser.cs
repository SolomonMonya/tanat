using AMF;

namespace TanatKernel
{
	internal class NewDressedItemArgMpdParser : CtrlPacketArgParser<NewDressedItemArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref NewDressedItemArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mHeroId = SafeGet<int>(args, "user_id");
			_arg.mArticleId = SafeGet<int>(args, "item");
			_arg.mSlot = SafeGet<int>(args, "slot");
		}
	}
}
