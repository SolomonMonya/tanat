using AMF;

namespace TanatKernel
{
	internal class JoinInviteArgMpdParser : CtrlPacketArgParser<JoinInviteArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinInviteArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mInviterId = SafeGet<int>(args, "inviter");
			_arg.mMapId = SafeGet<int>(args, "map_id");
			_arg.mAllow = SafeGet(args, "allow", _default: true);
			if (_arg.mAllow)
			{
				_arg.mRequestId = SafeGet(args, "request_id", 0);
				_arg.mMapType = SafeGet<int>(args, "type");
				_arg.mTeam = SafeGet<int>(args, "team");
				_arg.mGroup = SafeGet<int>(args, "group");
			}
		}
	}
}
