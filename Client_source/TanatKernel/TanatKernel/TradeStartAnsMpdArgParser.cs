using AMF;

namespace TanatKernel
{
	internal class TradeStartAnsMpdArgParser : CtrlPacketArgParser<TradeStartAnsArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref TradeStartAnsArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mAnswer = SafeGet<bool>(args, "answer");
		}
	}
}
