using AMF;

namespace TanatKernel
{
	internal class JoinFromGroupAnswerArgMpdParser : CtrlPacketArgParser<JoinFromGroupAnswerArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinFromGroupAnswerArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mAnswer = SafeGet<int>(args, "answer");
		}
	}
}
