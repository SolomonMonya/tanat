namespace TanatKernel
{
	internal class SetAvatarArgParser : BattlePacketArgParser<SetAvatarArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref SetAvatarArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "playerID", -1);
			_arg.mObjId = SafeGet(_packet, "avatarID", -1);
			_arg.mLevel = SafeGet(_packet, "level", 0);
			_arg.mSkillPoints = SafeGet(_packet, "points", 0);
		}
	}
}
