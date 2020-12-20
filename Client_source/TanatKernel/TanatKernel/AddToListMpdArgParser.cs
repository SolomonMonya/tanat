using AMF;

namespace TanatKernel
{
	internal class AddToListMpdArgParser : CtrlPacketArgParser<AddToListMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref AddToListMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mName = SafeGet<string>(args, "nick");
			_arg.mAnswer = SafeGet<bool>(args, "answer");
			_arg.mType = (ListType)SafeGet<int>(args, "type");
		}
	}
}
