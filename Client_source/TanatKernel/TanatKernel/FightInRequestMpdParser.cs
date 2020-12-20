using AMF;

namespace TanatKernel
{
	internal class FightInRequestMpdParser : CtrlPacketArgParser<FightInRequestMpd>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightInRequestMpd _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
		}
	}
}
