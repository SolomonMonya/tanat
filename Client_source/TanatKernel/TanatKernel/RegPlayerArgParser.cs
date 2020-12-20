namespace TanatKernel
{
	internal class RegPlayerArgParser : BattlePacketArgParser<RegPlayerArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref RegPlayerArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "id", -1);
			_arg.mName = SafeGet(_packet, "name", "noname");
			_arg.mTeam = SafeGet(_packet, "team", -1024);
			_arg.mAvatar = SafeGet(_packet, "avatar", -1);
		}
	}
}
