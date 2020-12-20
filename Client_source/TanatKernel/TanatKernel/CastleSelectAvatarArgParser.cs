using AMF;

namespace TanatKernel
{
	internal class CastleSelectAvatarArgParser : CtrlPacketArgParser<CastleSelectAvatarArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleSelectAvatarArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = mixedArray.TryGet("id", 0);
			_arg.mAvatar = mixedArray.TryGet("avatar", 0);
		}
	}
}
