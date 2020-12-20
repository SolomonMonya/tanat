using AMF;

namespace TanatKernel
{
	internal class CtrlOfflineArgMpdParser : CtrlPacketArgParser<CtrlOfflineArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CtrlOfflineArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
		}
	}
}
