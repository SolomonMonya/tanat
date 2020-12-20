namespace TanatKernel
{
	internal class UpdateSkillArgParser : BattlePacketArgParser<UpdateSkillArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref UpdateSkillArg _arg)
		{
			_arg.mSkillId = _packet.Request.Arguments["id"];
		}
	}
}
