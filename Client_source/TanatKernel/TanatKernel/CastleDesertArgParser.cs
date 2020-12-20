using AMF;

namespace TanatKernel
{
	internal class CastleDesertArgParser : CtrlPacketArgParser<CastleDesertArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleDesertArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = mixedArray.TryGet("id", 0);
		}
	}
}
