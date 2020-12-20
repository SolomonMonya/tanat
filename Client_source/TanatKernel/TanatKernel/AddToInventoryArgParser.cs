namespace TanatKernel
{
	internal class AddToInventoryArgParser : BattlePacketArgParser<AddToInventoryArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref AddToInventoryArg _arg)
		{
			_arg.mItemObjId = SafeGet(_packet, "id", -1);
			_arg.mItemPrototypeId = SafeGet(_packet, "proto", -1);
			_arg.mCount = SafeGet(_packet, "count", 0);
		}
	}
}
