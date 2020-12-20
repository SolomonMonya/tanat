using AMF;

namespace TanatKernel
{
	internal class RemoveFromGroupArgMpdParser : CtrlPacketArgParser<RemoveFromGroupArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemoveFromGroupArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mLeaderId = SafeGet<int>(args, "user_id");
		}
	}
}
