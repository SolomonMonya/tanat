namespace TanatKernel
{
	internal class OnlineArgParser : BattlePacketArgParser<OnlineArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref OnlineArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "id", -1);
			_arg.mIsOnline = SafeGet(_packet, "online", _default: false);
		}
	}
}
