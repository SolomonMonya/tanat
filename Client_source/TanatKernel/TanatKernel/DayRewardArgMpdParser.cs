using AMF;

namespace TanatKernel
{
	internal class DayRewardArgMpdParser : CtrlPacketArgParser<DayRewardMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref DayRewardMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mCurrent = SafeGet<int>(args, "current");
			MixedArray mixedArray = SafeGet<MixedArray>(args, "items");
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args2 = item;
				OneDayReward oneDayReward = new OneDayReward();
				oneDayReward.mCount = SafeGet<int>(args2, "count");
				oneDayReward.mItemId = SafeGet<int>(args2, "item");
				_arg.mReward.Add(oneDayReward);
			}
		}
	}
}
