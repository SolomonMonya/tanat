using AMF;

namespace TanatKernel
{
	internal class InfoArgParser : CtrlPacketArgParser<InfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref InfoArg _arg)
		{
			_arg.mId = SafeGet<int>(_packet, "id");
			_arg.mTag = SafeGet<string>(_packet, "tag");
			_arg.mLevel = SafeGet<int>(_packet, "level");
			_arg.mRating = SafeGet<int>(_packet, "rating");
			_arg.mName = SafeGet<string>(_packet, "name");
			MixedArray mixedArray = SafeGet(_packet, "users", new MixedArray());
			foreach (Variable item2 in mixedArray.Dense)
			{
				InfoArg.User item = default(InfoArg.User);
				MixedArray args = item2;
				item.mId = SafeGet<int>(args, "id");
				item.mName = SafeGet<string>(args, "nick");
				item.mLocation = SafeGet<string>(args, "location");
				item.mRole = (Clan.Role)SafeGet<int>(args, "role");
				_arg.mUsers.Add(item);
			}
		}
	}
}
