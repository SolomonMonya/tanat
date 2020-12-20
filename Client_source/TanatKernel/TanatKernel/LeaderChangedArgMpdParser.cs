using AMF;

namespace TanatKernel
{
	internal class LeaderChangedArgMpdParser : CtrlPacketArgParser<LeaderChangedArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref LeaderChangedArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mLeaderId = SafeGet<int>(args, "leader");
		}
	}
}
