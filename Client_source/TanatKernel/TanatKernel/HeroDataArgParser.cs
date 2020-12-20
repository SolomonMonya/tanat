using AMF;
using Network;

namespace TanatKernel
{
	internal class HeroDataArgParser : CtrlPacketArgParser<HeroDataArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroDataArg _arg)
		{
			if (_packet.Arguments.Associative.TryGetValue("load", out var value))
			{
				_arg.mPersExists = true;
			}
			else
			{
				if (!_packet.Arguments.Associative.TryGetValue("create", out value))
				{
					throw new NetSystemException("invalid content");
				}
				_arg.mPersExists = false;
			}
			MixedArray mixedArray = value;
			_arg.mHeroId = SafeGet<int>(mixedArray, "id");
			if (mixedArray.Associative.Count > 1)
			{
				_arg.mView.mRace = SafeGet(mixedArray, "race", 0);
				_arg.mView.mGender = SafeGet(mixedArray, "gender", _default: true);
				_arg.mView.mFace = SafeGet(mixedArray, "face", 0);
				_arg.mView.mHair = SafeGet(mixedArray, "hair", 0);
				_arg.mView.mDistMark = SafeGet(mixedArray, "dist_mark", 0);
				_arg.mView.mSkinColor = SafeGet(mixedArray, "skin_color", 0);
				_arg.mView.mHairColor = SafeGet(mixedArray, "hair_color", 0);
			}
		}
	}
}
