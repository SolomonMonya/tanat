using AMF;

namespace TanatKernel
{
	internal class JoinQueueReadyArgMpdParser : CtrlPacketArgParser<JoinQueueReadyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinQueueReadyArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mRequestId = SafeGet<int>(args, "request_id");
			_arg.mWaitTime = SafeGet<int>(args, "wait");
		}
	}
}
