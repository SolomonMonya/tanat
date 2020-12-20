using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class JoinArgParser : CtrlPacketArgParser<JoinArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "avatars", new MixedArray());
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				MapAvatarData mapAvatarData = new MapAvatarData();
				if (!int.TryParse(item2.Key, out mapAvatarData.mId))
				{
					Log.Warning("invalid avatar id: " + item2.Key);
					continue;
				}
				MixedArray args = item2.Value;
				mapAvatarData.mAvailable = SafeGet(args, "available", _default: false);
				mapAvatarData.mDenyForMap = SafeGet(args, "deny_for_map", _default: false);
				MixedArray mixedArray2 = SafeGet(args, "restrictions", new MixedArray());
				foreach (Variable item3 in mixedArray2.Dense)
				{
					MixedArray args2 = item3;
					AvatarRestriction item = default(AvatarRestriction);
					item.mType = SafeGet(args2, "type", -1);
					item.mValue = SafeGet(args2, "value", "");
					item.mAllow = SafeGet(args2, "allow", _default: false);
					mapAvatarData.mRestrictions.Add(item);
				}
				_arg.mAvatars.Add(mapAvatarData);
			}
			_arg.mMapId = SafeGet(_packet, "map_id", 0);
			AddStatsParser.Parse(_arg.mAddStats, _packet.Arguments);
		}
	}
}
