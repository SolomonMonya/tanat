using AMF;

namespace TanatKernel
{
	internal class MapInfoArgParser : CtrlPacketArgParser<MapInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref MapInfoArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "maps_info", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				MapDataDesc mapDataDesc = new MapDataDesc();
				mapDataDesc.mMapId = SafeGet(item, "id", 0);
				mapDataDesc.mType = (MapType)SafeGet(item, "type_id", -1);
				mapDataDesc.mName = SafeGet(item, "name", "");
				mapDataDesc.mMinLevel = SafeGet(item, "level_min", 0);
				mapDataDesc.mMaxLevel = SafeGet(item, "level_max", 0);
				mapDataDesc.mScene = SafeGet(item, "scene", "");
				mapDataDesc.mAvailable = SafeGet(item, "available", _default: false);
				mapDataDesc.mDesc = SafeGet(item, "desc", "UNDEFINED");
				mapDataDesc.mWinDesc = SafeGet(item, "win_desc", "UNDEFINED");
				mapDataDesc.mMinPlayers = SafeGet(item, "max_players", 0);
				mapDataDesc.mMaxPlayers = SafeGet(item, "map_max_players", 0);
				_arg.mMapsDesc.Add(mapDataDesc);
			}
		}
	}
}
