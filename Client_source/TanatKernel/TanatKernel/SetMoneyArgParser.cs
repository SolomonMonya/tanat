using AMF;

namespace TanatKernel
{
	internal class SetMoneyArgParser : BattlePacketArgParser<SetMoneyArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref SetMoneyArg _arg)
		{
			_arg.mFromObjId = SafeGet(_packet, "from", -1);
			MixedArray mixedArray = SafeGet(_packet, "money", new MixedArray());
			_arg.mVirtualMoney = mixedArray.TryGet("v", 0);
			_arg.mRealMoney = mixedArray.TryGet("r", 0);
			MixedArray mixedArray2 = SafeGet(_packet, "delta", new MixedArray());
			_arg.mChangedVirtual = mixedArray2.TryGet("v", 0);
			_arg.mChangedReal = mixedArray2.TryGet("r", 0);
		}
	}
}
