using AMF;

namespace TanatKernel
{
	internal class ChatGagArgParser : CtrlPacketArgParser<ChatGagArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ChatGagArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mNick = SafeGet<string>(args, "user_nick");
			_arg.mDuration = SafeGet<int>(args, "duration");
		}
	}
}
