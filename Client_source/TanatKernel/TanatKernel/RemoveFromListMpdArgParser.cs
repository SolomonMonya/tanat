using AMF;

namespace TanatKernel
{
	internal class RemoveFromListMpdArgParser : CtrlPacketArgParser<RemoveFromListMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemoveFromListMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mNick = SafeGet<string>(args, "nick");
		}
	}
}
