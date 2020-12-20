using AMF;

namespace TanatKernel
{
	internal class FightDesertMpdArgParser : CtrlPacketArgParser<FightDesertMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightDesertMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mMapId = SafeGet<int>(args, "map_id");
		}
	}
}
