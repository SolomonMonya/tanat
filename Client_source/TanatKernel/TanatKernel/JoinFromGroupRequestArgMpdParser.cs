using AMF;

namespace TanatKernel
{
	internal class JoinFromGroupRequestArgMpdParser : CtrlPacketArgParser<JoinFromGroupRequestArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinFromGroupRequestArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mLeaderNick = SafeGet<string>(args, "nick");
			_arg.mLeaderIdd = SafeGet<int>(args, "user_id");
		}
	}
}
