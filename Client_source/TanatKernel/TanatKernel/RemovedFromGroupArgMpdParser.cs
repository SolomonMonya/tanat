using AMF;

namespace TanatKernel
{
	internal class RemovedFromGroupArgMpdParser : CtrlPacketArgParser<RemovedFromGroupArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemovedFromGroupArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user");
		}
	}
}
