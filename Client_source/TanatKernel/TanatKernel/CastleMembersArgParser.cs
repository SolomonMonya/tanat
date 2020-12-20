using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class CastleMembersArgParser : CtrlPacketArgParser<CastleMembersArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleMembersArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "members");
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				if (!int.TryParse(item.Key, out var result))
				{
					Log.Error("cant parse clan id - " + item.Key);
					continue;
				}
				string value = item.Value.Cast<string>();
				_arg.mMembers[result] = value;
			}
			mixedArray = SafeGet<MixedArray>(_packet, "queue");
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				if (!int.TryParse(item2.Key, out var result2))
				{
					Log.Error("cant parse clan id - " + item2.Key);
					continue;
				}
				string value2 = item2.Value.Cast<string>();
				_arg.mQueue[result2] = value2;
			}
			_arg.mJoined = SafeGet<bool>(_packet, "joined");
			_arg.mCanJoin = SafeGet<bool>(_packet, "editable");
			_arg.mInProgress = SafeGet<bool>(_packet, "in_progress");
			_arg.mRightLevel = SafeGet<bool>(_packet, "right_level");
			_arg.mBanCount = SafeGet(_packet, "ban_count", 0);
		}
	}
}
