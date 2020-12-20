namespace TanatKernel
{
	internal class ReceiveHitArgParser : BattlePacketArgParser<ReceiveHitArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ReceiveHitArg _arg)
		{
			_arg.mVictimObjId = SafeGet(_packet, "object", -1);
			_arg.mDamagerObjId = SafeGet(_packet, "damager", -1);
			_arg.mHitParamsMask = SafeGet(_packet, "flags", 0);
			_arg.mDamage = SafeGet(_packet, "damage", 0f);
		}
	}
}
