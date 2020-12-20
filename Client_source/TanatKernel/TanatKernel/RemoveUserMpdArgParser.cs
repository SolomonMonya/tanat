using AMF;

namespace TanatKernel
{
	internal class RemoveUserMpdArgParser : CtrlPacketArgParser<RemoveUserMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref RemoveUserMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
			_arg.mReason = (Clan.RemoveReason)SafeGet<int>(args, "type");
		}
	}
}
