namespace TanatKernel
{
	internal class SetProjectileArgParser : BattlePacketArgParser<SetProjectileArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref SetProjectileArg _arg)
		{
			_arg.mSourceObjId = SafeGet(_packet, "source", -1);
			_arg.mTargetId = SafeGet(_packet, "target", -1);
			_arg.mHitTime = SafeGet(_packet, "hit_at", 0f);
		}
	}
}
