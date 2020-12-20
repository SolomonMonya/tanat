using AMF;

namespace TanatKernel
{
	internal class DressedItemsArgParser : CtrlPacketArgParser<DressedItemsArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref DressedItemsArg _arg)
		{
			_arg.mHeroId = SafeGet<int>(_packet, "user_id");
			MixedArray mixedArray = SafeGet(_packet, "bag", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args = item;
				DressedItemsArg.DressedItem dressedItem = new DressedItemsArg.DressedItem();
				dressedItem.mId = SafeGet<int>(args, "id");
				dressedItem.mArticleId = SafeGet<int>(args, "artikul_id");
				dressedItem.mCount = SafeGet(args, "cnt", 0);
				dressedItem.mSlot = SafeGet(args, "slot", 0);
				_arg.mItems.Add(dressedItem);
			}
		}
	}
}
