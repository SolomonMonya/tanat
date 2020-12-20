namespace TanatKernel
{
	internal class RefreshDropInfoArgParser : BattlePacketArgParser<RefreshDropInfoArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref RefreshDropInfoArg _arg)
		{
			_arg.mContainerId = SafeGet(_packet, "id", -1);
		}
	}
}
