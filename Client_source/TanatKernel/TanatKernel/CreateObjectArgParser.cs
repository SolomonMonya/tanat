namespace TanatKernel
{
	internal class CreateObjectArgParser : BattlePacketArgParser<CreateObjectArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref CreateObjectArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mPrototypeId = SafeGet(_packet, "proto", -1);
		}
	}
}
