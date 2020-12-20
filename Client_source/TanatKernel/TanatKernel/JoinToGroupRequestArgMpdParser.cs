using AMF;

namespace TanatKernel
{
	internal class JoinToGroupRequestArgMpdParser : CtrlPacketArgParser<JoinToGroupRequestArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinToGroupRequestArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserIdd = SafeGet<int>(args, "user_id");
			_arg.mUserNick = SafeGet<string>(args, "nick");
		}
	}
}
