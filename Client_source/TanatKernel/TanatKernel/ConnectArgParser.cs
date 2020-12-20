namespace TanatKernel
{
	internal class ConnectArgParser : BattlePacketArgParser<ConnectArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ConnectArg _arg)
		{
			_arg.mSelfPlayerId = SafeGet(_packet, "clientId", -1);
			_arg.mBattleId = SafeGet(_packet, "battleId", -1);
		}
	}
}
