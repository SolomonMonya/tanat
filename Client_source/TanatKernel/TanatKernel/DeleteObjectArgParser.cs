namespace TanatKernel
{
	internal class DeleteObjectArgParser : BattlePacketArgParser<DeleteObjectArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref DeleteObjectArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
		}
	}
}
