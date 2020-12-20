using AMF;

namespace TanatKernel
{
	internal class FriendRequestMpdArgParser : CtrlPacketArgParser<FriendRequestMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FriendRequestMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
			_arg.mNick = SafeGet<string>(args, "nick");
		}
	}
}
