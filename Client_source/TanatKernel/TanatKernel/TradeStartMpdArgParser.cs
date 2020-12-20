using AMF;

namespace TanatKernel
{
	internal class TradeStartMpdArgParser : CtrlPacketArgParser<TradeStartArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref TradeStartArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mUserId = SafeGet<int>(args, "user_id");
		}
	}
}
