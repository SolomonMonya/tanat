namespace TanatKernel
{
	internal class RemoveFromInventoryArgParser : BattlePacketArgParser<RemoveFromInventoryArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref RemoveFromInventoryArg _arg)
		{
			_arg.mItemObjId = SafeGet(_packet, "id", -1);
			_arg.mCount = SafeGet(_packet, "count", 0);
		}
	}
}
