namespace TanatKernel
{
	internal class SetStateArgParser : BattlePacketArgParser<SetStateArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref SetStateArg _arg)
		{
			_arg.mStateId = _packet.Request.Arguments["state"];
			_arg.mIsPet = _packet.Request.Arguments["pet"];
		}
	}
}
