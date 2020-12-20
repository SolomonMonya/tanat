using AMF;

namespace TanatKernel
{
	internal class PlayerReadyMpdArgParser : CtrlPacketArgParser<PlayerReadyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref PlayerReadyArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = SafeGet<int>(args, "id");
		}
	}
}
