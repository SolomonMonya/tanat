using AMF;

namespace TanatKernel
{
	internal class SelectAvatarMpdArgParser : CtrlPacketArgParser<SelectAvatarArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref SelectAvatarArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mAvatarId = SafeGet<int>(args, "avatar");
			_arg.mUserId = SafeGet<int>(args, "id");
		}
	}
}
