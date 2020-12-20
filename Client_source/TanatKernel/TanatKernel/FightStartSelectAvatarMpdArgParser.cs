using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class FightStartSelectAvatarMpdArgParser : CtrlPacketArgParser<FightStartSelectAvatarMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightStartSelectAvatarMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			MixedArray mixedArray = SafeGet(args, "avatars", new MixedArray());
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				MapAvatarData mapAvatarData = new MapAvatarData();
				if (!int.TryParse(item2.Key, out mapAvatarData.mId))
				{
					Log.Warning("invalid avatar id: " + item2.Key);
					continue;
				}
				MixedArray args2 = item2.Value;
				mapAvatarData.mAvailable = SafeGet(args2, "available", _default: false);
				mapAvatarData.mDenyForMap = SafeGet(args2, "deny_for_map", _default: false);
				MixedArray mixedArray2 = SafeGet(args2, "restrictions", new MixedArray());
				foreach (Variable item3 in mixedArray2.Dense)
				{
					MixedArray args3 = item3;
					AvatarRestriction item = default(AvatarRestriction);
					item.mType = SafeGet(args3, "type", -1);
					item.mValue = SafeGet(args3, "value", "");
					item.mAllow = SafeGet(args3, "allow", _default: false);
					mapAvatarData.mRestrictions.Add(item);
				}
				_arg.mAvatars.Add(mapAvatarData);
			}
			AddStatsParser.Parse(_arg.mAddStats, args);
			MixedArray mixedArray3 = SafeGet(args, "fighters", new MixedArray());
			_arg.mMapId = SafeGet(args, "map_id", 0);
			_arg.mWaitTime = SafeGet<int>(args, "time");
			foreach (KeyValuePair<string, Variable> item4 in mixedArray3.Associative)
			{
				Fighter fighter = new Fighter();
				if (!int.TryParse(item4.Key, out fighter.mId))
				{
					Log.Error("cant parse battle id - " + item4.Key);
					continue;
				}
				MixedArray args4 = item4.Value;
				fighter.mNick = SafeGet<string>(args4, "nick");
				fighter.mTag = SafeGet<string>(args4, "tag");
				fighter.mTeam = SafeGet<int>(args4, "team");
				if (fighter.mTeam == 1)
				{
					_arg.mTeam1.Add(fighter);
				}
				else
				{
					_arg.mTeam2.Add(fighter);
				}
			}
		}
	}
}
