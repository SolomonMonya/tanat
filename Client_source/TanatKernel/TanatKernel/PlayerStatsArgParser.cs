namespace TanatKernel
{
	internal class PlayerStatsArgParser : BattlePacketArgParser<PlayerStatsArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref PlayerStatsArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "id", -1);
			_arg.mLevel = SafeGet(_packet, "level", 0);
			_arg.mKillsCnt = SafeGet(_packet, "kills", 0);
			_arg.mDeathsCnt = SafeGet(_packet, "deaths", 0);
			_arg.mLastKiller = SafeGet(_packet, "killer", -1);
			_arg.mAssistsCnt = SafeGet(_packet, "assists", 0);
		}
	}
}
