using AMF;

namespace TanatKernel
{
	internal class JoinToGroupAnswerArgMpdParser : CtrlPacketArgParser<JoinToGroupAnswerArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinToGroupAnswerArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mAnswer = SafeGet<int>(args, "answer");
		}
	}
}
