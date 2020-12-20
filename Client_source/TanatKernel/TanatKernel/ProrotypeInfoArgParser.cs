namespace TanatKernel
{
	internal class ProrotypeInfoArgParser : BattlePacketArgParser<ProrotypeInfoArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ProrotypeInfoArg _arg)
		{
			_arg.mProrotypeId = SafeGet(_packet, "id", -1);
			_arg.mInfo = SafeGet(_packet, "desc", "");
		}
	}
}
