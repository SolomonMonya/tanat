using AMF;

namespace TanatKernel
{
	internal class JoinedToGroupArgMpdParser : CtrlPacketArgParser<JoinedToGroupArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinedToGroupArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user");
			_arg.mName = SafeGet<string>(args, "nick");
		}
	}
}
