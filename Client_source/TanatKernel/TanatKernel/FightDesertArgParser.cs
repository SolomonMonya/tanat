namespace TanatKernel
{
	internal class FightDesertArgParser : CtrlPacketArgParser<FightDesertArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightDesertArg _arg)
		{
			_arg.mMapId = SafeGet<int>(_packet, "map_id");
		}
	}
}
