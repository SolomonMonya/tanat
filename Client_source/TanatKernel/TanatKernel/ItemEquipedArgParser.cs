namespace TanatKernel
{
	internal class ItemEquipedArgParser : BattlePacketArgParser<ItemEquipedArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ItemEquipedArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mItemObjId = SafeGet(_packet, "item", -1);
			_arg.mItemProtoId = SafeGet(_packet, "proto", -1);
			_arg.mIsEquiped = SafeGet(_packet, "equip", _default: true);
		}
	}
}
