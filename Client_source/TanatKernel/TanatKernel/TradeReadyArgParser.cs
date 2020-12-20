using AMF;

namespace TanatKernel
{
	internal class TradeReadyArgParser : CtrlPacketArgParser<TradeReadyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref TradeReadyArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mMoney = SafeGet(args, "money", 0);
			MixedArray mixedArray = SafeGet(args, "items", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args2 = item;
				int key = SafeGet<int>(args2, "id");
				int value = SafeGet(args2, "cnt", 0);
				_arg.mItems[key] = value;
			}
		}
	}
}
