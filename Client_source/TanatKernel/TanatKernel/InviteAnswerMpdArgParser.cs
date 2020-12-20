using AMF;

namespace TanatKernel
{
	internal class InviteAnswerMpdArgParser : CtrlPacketArgParser<InviteAnswerMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref InviteAnswerMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mName = SafeGet<string>(args, "nick");
			_arg.mAnswer = SafeGet<bool>(args, "answer");
		}
	}
}
