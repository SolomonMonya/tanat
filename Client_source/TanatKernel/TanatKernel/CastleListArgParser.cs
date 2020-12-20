using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class CastleListArgParser : CtrlPacketArgParser<CastleListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleListArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "castles");
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				MixedArray args = item.Value;
				if (!int.TryParse(item.Key, out var result))
				{
					Log.Error("cant parse castle id - " + item.Key);
					continue;
				}
				CastleInfo castleInfo = new CastleInfo();
				castleInfo.mId = result;
				castleInfo.mLevelMax = SafeGet<int>(args, "level_max");
				castleInfo.mLevelMin = SafeGet<int>(args, "level_min");
				castleInfo.mName = SafeGet<string>(args, "name");
				castleInfo.mOwnerName = SafeGet<string>(args, "owner_name");
				castleInfo.mOwnerId = SafeGet<int>(args, "owner_id");
				castleInfo.mFightersMin = SafeGet<int>(args, "fighters_min");
				castleInfo.mStartTime = DateTime.Now.AddSeconds(SafeGet<int>(args, "start_time"));
				MixedArray args2 = SafeGet<MixedArray>(args, "rewards");
				castleInfo.mReward.mDiamonds = SafeGet<int>(args2, "money_d");
				castleInfo.mReward.mMoney = SafeGet<int>(args2, "money");
				castleInfo.mReward.mExp = SafeGet<int>(args2, "exp");
				castleInfo.mReward.mItem = SafeGet<int>(args2, "item");
				castleInfo.mReward.mItemCount = SafeGet<int>(args2, "item_count");
				_arg.mCastles.Add(castleInfo);
			}
		}
	}
}
