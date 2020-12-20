using AMF;

namespace TanatKernel
{
	internal class PlayerDesertedMpdArgParser : CtrlPacketArgParser<PlayerDesertedArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref PlayerDesertedArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = SafeGet<int>(mixedArray, "id");
			_arg.mWait = mixedArray.TryGet("wait", -1);
			_arg.mInQueue = mixedArray.TryGet("in_queue", _default: false);
		}
	}
}
