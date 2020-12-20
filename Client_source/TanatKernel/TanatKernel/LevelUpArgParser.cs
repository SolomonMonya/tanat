namespace TanatKernel
{
	internal class LevelUpArgParser : BattlePacketArgParser<LevelUpArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref LevelUpArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mLevel = SafeGet(_packet, "level", 0);
			_arg.mSkillPoints = SafeGet(_packet, "points", 0);
		}
	}
}
