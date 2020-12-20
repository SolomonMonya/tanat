using AMF;

namespace TanatKernel
{
	internal class UserBagArgParser : CtrlPacketArgParser<UserBagArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref UserBagArg _arg)
		{
			_arg.mUserMoney = SafeGet(_packet, "user_money", 0);
			MixedArray mixedArray = SafeGet(_packet, "bag", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args = item;
				UserBagArg.BagItem bagItem = new UserBagArg.BagItem();
				bagItem.mId = SafeGet<int>(args, "id");
				bagItem.mArticleId = SafeGet<int>(args, "artikul_id");
				bagItem.mCount = SafeGet(args, "cnt", 0);
				bagItem.mUsed = SafeGet(args, "used", 0);
				_arg.mItems.Add(bagItem);
			}
		}
	}
}
