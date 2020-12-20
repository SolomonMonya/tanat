using AMF;

namespace TanatKernel
{
	internal class FightReadyMpdArgParser : CtrlPacketArgParser<FightReadyMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightReadyMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
		}
	}
}
