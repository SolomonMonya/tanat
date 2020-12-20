using AMF;

namespace TanatKernel
{
	internal class FindResultArgParser : CtrlPacketArgParser<FindResultArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FindResultArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "result", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray arg = item;
				_arg.mResult.Add(BwListArgParser.ParseUser(arg));
			}
		}
	}
}
