using AMF;

namespace TanatKernel
{
	internal class GroupListArgParser : CtrlPacketArgParser<GroupListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref GroupListArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "users", new MixedArray());
			foreach (Variable item2 in mixedArray.Dense)
			{
				GroupListArg.Member item = default(GroupListArg.Member);
				MixedArray args = item2;
				item.mId = SafeGet<int>(args, "id");
				item.mName = SafeGet<string>(args, "nick");
				item.mIsOnline = SafeGet<bool>(args, "online");
				item.mLevel = SafeGet(args, "level", -1);
				item.mRace = SafeGet(args, "race", 0);
				item.mGender = SafeGet(args, "gender", _default: true);
				MixedArray mixedArray2 = SafeGet(args, "clan_info", new MixedArray());
				item.mClanId = mixedArray2.TryGet("id", -1);
				item.mClanTag = mixedArray2.TryGet("tag", "");
				_arg.mMembers.Add(item);
			}
			_arg.mLeaderId = SafeGet(_packet, "leader", -1);
		}
	}
}
