using AMF;

namespace TanatKernel
{
	internal class DropInfoArgParser : BattlePacketArgParser<DropInfoArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref DropInfoArg _arg)
		{
			_arg.mContainerId = _packet.Request.Arguments["id"];
			MixedArray mixedArray = SafeGet(_packet, "info", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				DroppedItem droppedItem = new DroppedItem();
				MixedArray mixedArray2 = item;
				droppedItem.mId = mixedArray2.TryGet("id", -1);
				droppedItem.mProtoId = mixedArray2.TryGet("proto", -1);
				droppedItem.mCount = mixedArray2.TryGet("count", -1);
				MixedArray mixedArray3 = mixedArray2.TryGet("allowed", new MixedArray());
				foreach (Variable item2 in mixedArray3.Dense)
				{
					droppedItem.mAllowed.Add(item2);
				}
				_arg.mItems.Add(droppedItem);
			}
		}
	}
}
