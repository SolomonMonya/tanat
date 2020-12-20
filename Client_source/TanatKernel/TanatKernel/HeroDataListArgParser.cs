using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class HeroDataListArgParser : CtrlPacketArgParser<HeroDataListArg>
	{
		protected override void ParseArg(CtrlPacket _packetArray, ref HeroDataListArg _arg)
		{
			MixedArray args = SafeGet(_packetArray, "data", new MixedArray());
			ParseData(args, _arg);
		}

		public void ParseData(MixedArray _args, HeroDataListArg _gameData)
		{
			foreach (KeyValuePair<string, Variable> item in _args.Associative)
			{
				HeroDataListArg.HeroDataItem heroDataItem = new HeroDataListArg.HeroDataItem();
				heroDataItem.mPersExists = true;
				MixedArray args = SafeGet(item.Value, "load", new MixedArray());
				heroDataItem.mHeroId = int.Parse(item.Key);
				heroDataItem.mView.mRace = SafeGet(args, "race", 0);
				heroDataItem.mView.mGender = SafeGet(args, "gender", _default: true);
				heroDataItem.mView.mFace = SafeGet(args, "face", 0);
				heroDataItem.mView.mHair = SafeGet(args, "hair", 0);
				heroDataItem.mView.mDistMark = SafeGet(args, "dist_mark", 0);
				heroDataItem.mView.mSkinColor = SafeGet(args, "skin_color", 0);
				heroDataItem.mView.mHairColor = SafeGet(args, "hair_color", 0);
				MixedArray mixedArray = SafeGet(item.Value, "dressed_items", new MixedArray());
				foreach (Variable item2 in mixedArray.Dense)
				{
					MixedArray args2 = item2;
					DressedItemsArg.DressedItem dressedItem = new DressedItemsArg.DressedItem();
					dressedItem.mId = SafeGet<int>(args2, "id");
					dressedItem.mArticleId = SafeGet<int>(args2, "artikul_id");
					dressedItem.mCount = SafeGet(args2, "cnt", 0);
					dressedItem.mSlot = SafeGet(args2, "slot", 0);
					heroDataItem.mItems.Add(dressedItem);
				}
				MixedArray mixedArray2 = SafeGet(item.Value, "clan_info", new MixedArray());
				heroDataItem.mClanId = mixedArray2.TryGet("id", -1);
				heroDataItem.mClanTag = mixedArray2.TryGet("tag", "");
				MixedArray mixedArray3 = SafeGet(item.Value, "user_info", new MixedArray());
				heroDataItem.mLevel = mixedArray3.TryGet("level", -1);
				heroDataItem.mExp = mixedArray3.TryGet("exp", -1);
				heroDataItem.mNextExp = mixedArray3.TryGet("next_exp", -1);
				heroDataItem.mRating = mixedArray3.TryGet("rating", -1);
				_gameData.mItems.Add(heroDataItem);
			}
		}
	}
}
