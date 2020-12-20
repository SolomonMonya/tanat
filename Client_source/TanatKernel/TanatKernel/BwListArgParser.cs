using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class BwListArgParser : CtrlPacketArgParser<BwListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref BwListArg _arg)
		{
			if (_packet.Arguments.Associative.ContainsKey("black"))
			{
				MixedArray mixedArray = SafeGet<MixedArray>(_packet, "black");
				_arg.mIgnore = new List<ShortUserInfo>();
				foreach (Variable item in mixedArray.Dense)
				{
					MixedArray arg = item;
					_arg.mIgnore.Add(ParseUser(arg));
				}
			}
			if (!_packet.Arguments.Associative.ContainsKey("white"))
			{
				return;
			}
			MixedArray mixedArray2 = SafeGet<MixedArray>(_packet, "white");
			_arg.mFriends = new List<ShortUserInfo>();
			foreach (Variable item2 in mixedArray2.Dense)
			{
				MixedArray arg2 = item2;
				_arg.mFriends.Add(ParseUser(arg2));
			}
		}

		public static ShortUserInfo ParseUser(MixedArray _arg)
		{
			ShortUserInfo shortUserInfo = new ShortUserInfo();
			shortUserInfo.mId = _arg.TryGet("id", 0);
			shortUserInfo.mLevel = _arg.TryGet("level", 0);
			shortUserInfo.mName = _arg.TryGet("nick", "");
			shortUserInfo.mRating = _arg.TryGet("rating", 0);
			shortUserInfo.mTag = _arg.TryGet("tag", "");
			shortUserInfo.mClanId = _arg.TryGet("clan_id", 0);
			if (_arg.Associative.Count > 5)
			{
				shortUserInfo.mOnline = (ShortUserInfo.Status)_arg.TryGet("online", 0);
			}
			return shortUserInfo;
		}
	}
}
