namespace TanatKernel
{
	internal class ServerTimeArgParser : BattlePacketArgParser<ServerTimeArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ServerTimeArg _arg)
		{
			_arg.mTime = SafeGet(_packet, "time", 0f);
			_arg.mReceivingTime = _packet.ReceivingTime;
			_arg.mPing = _packet.GetPing();
		}
	}
}
