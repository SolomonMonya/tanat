namespace TanatKernel
{
	internal class GameDataArgParser : BattlePacketArgParser<GameDataArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref GameDataArg _arg)
		{
			_arg.mGameDataXml = SafeGet(_packet, "data", "");
		}
	}
}
