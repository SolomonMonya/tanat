using AMF;

namespace TanatKernel
{
	internal class CtrlOnlineArgMpdParser : CtrlPacketArgParser<CtrlOnlineArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CtrlOnlineArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
		}
	}
}
