namespace TanatKernel
{
	internal class ActionDoneArgParser : BattlePacketArgParser<ActionDoneArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ActionDoneArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mActionId = SafeGet(_packet, "action", -1);
			_arg.mItem = SafeGet(_packet, "item", _default: false);
			_arg.mCooldown = SafeGet(_packet, "cooldown", 0f);
		}
	}
}
