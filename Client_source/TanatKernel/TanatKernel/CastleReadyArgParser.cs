using AMF;

namespace TanatKernel
{
	internal class CastleReadyArgParser : CtrlPacketArgParser<CastleReadyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleReadyArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = mixedArray.TryGet("id", 0);
		}
	}
}
