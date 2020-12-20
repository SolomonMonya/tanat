using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class MapListArgParser : CtrlPacketArgParser<MapListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref MapListArg _arg)
		{
			_arg.mMapType = SafeGet<int>(_packet, "type");
			MixedArray args = SafeGet(_packet, "leave_info", new MixedArray());
			_arg.mIsBanned = SafeGet<bool>(args, "banned");
			if (_arg.mIsBanned)
			{
				int num = SafeGet(args, "time", 0);
				_arg.mBanTimeEnd = DateTime.Now.AddSeconds(num);
				_arg.mNextBanTime = 0;
			}
			else
			{
				_arg.mBanTimeEnd = DateTime.Now;
				_arg.mNextBanTime = SafeGet(args, "time", 0);
			}
			MixedArray mixedArray = SafeGet(_packet, "maps", new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				MapData mapData = new MapData();
				mapData.mType = _arg.mMapType;
				if (!int.TryParse(item.Key, out mapData.mId))
				{
					Log.Warning("invalid map data id " + item.Key);
					continue;
				}
				MixedArray args2 = item.Value;
				mapData.mName = SafeGet(args2, "name", "");
				mapData.mScene = SafeGet(args2, "scene", "");
				mapData.mAvailable = SafeGet(args2, "available", _default: false);
				mapData.mUsed = SafeGet(args2, "used", _default: false);
				mapData.mMinLevel = SafeGet(args2, "level_min", 0);
				mapData.mMaxLevel = SafeGet(args2, "level_max", 0);
				mapData.mDesc = SafeGet(args2, "desc", "UNDEFINED");
				mapData.mWinDesc = SafeGet(args2, "win_desc", "UNDEFINED");
				mapData.mMinPlayers = SafeGet(args2, "max_players", 0);
				mapData.mMaxPlayers = SafeGet(args2, "map_max_players", 0);
				_arg.mMaps.Add(mapData);
			}
		}
	}
}
