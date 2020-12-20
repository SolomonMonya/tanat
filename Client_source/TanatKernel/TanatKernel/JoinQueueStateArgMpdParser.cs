using AMF;

namespace TanatKernel
{
	internal class JoinQueueStateArgMpdParser : CtrlPacketArgParser<JoinQueueStateArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinQueueStateArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mRequestId = SafeGet<int>(args, "request_id");
			_arg.mCountTeam1 = SafeGet<int>(args, "team1");
			_arg.mCountTeam2 = SafeGet<int>(args, "team2");
			_arg.mMode = SafeGet<int>(args, "mode");
		}
	}
}
