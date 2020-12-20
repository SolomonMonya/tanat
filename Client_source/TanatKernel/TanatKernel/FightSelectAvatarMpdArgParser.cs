using AMF;

namespace TanatKernel
{
	internal class FightSelectAvatarMpdArgParser : CtrlPacketArgParser<FightSelectAvatarMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightSelectAvatarMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
			_arg.mAvatarId = SafeGet<int>(args, "avatar_id");
		}
	}
}
