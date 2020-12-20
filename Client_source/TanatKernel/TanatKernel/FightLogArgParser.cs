using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class FightLogArgParser : CtrlPacketArgParser<FightLogArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightLogArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "log", new MixedArray());
			MixedArray mixedArray2 = mixedArray.TryGet("heroes", new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray2.Associative)
			{
				MixedArray mixedArray3 = item.Value;
				BattleEndData battleEndData = new BattleEndData();
				int key = int.Parse(item.Key);
				battleEndData.mAvatarId = mixedArray3.TryGet("avatar", -1);
				battleEndData.mPlayerName = mixedArray3.TryGet("nick", "");
				battleEndData.mFinishType = mixedArray3.TryGet("finish_type", -1);
				battleEndData.mDeath = mixedArray3.TryGet("Deaths", 0);
				battleEndData.mTeam = mixedArray3.TryGet("team", -1024);
				battleEndData.mKillAssist = mixedArray3.TryGet("Assists", 0);
				battleEndData.mKill = mixedArray3.TryGet("AvatarKills", 0);
				battleEndData.mNewRating = mixedArray3.TryGet("rating", 0);
				battleEndData.mOldRating = mixedArray3.TryGet("old_rating", 0);
				battleEndData.mAvaLvl = mixedArray3.TryGet("level", 0);
				battleEndData.mMoney = mixedArray3.TryGet("money", 0);
				battleEndData.mHeroExp = mixedArray3.TryGet("hero_xp", 0);
				battleEndData.mItemsCount = mixedArray3.TryGet("items_count", 0);
				battleEndData.mKillRank = mixedArray3.TryGet("Rank", 0);
				battleEndData.mEfficiency = mixedArray3.TryGet("Efficiency", 0);
				battleEndData.mAssistsExp = mixedArray3.TryGet("AssistsExp", 0);
				battleEndData.mAvatarKillsExp = mixedArray3.TryGet("AvatarKillsExp", 0);
				battleEndData.mBattleExp = mixedArray3.TryGet("BattleExp", 0);
				battleEndData.mAvatarLevelExp = mixedArray3.TryGet("AvatarLevelExp", 0);
				battleEndData.mHonor = mixedArray3.TryGet("honor", 0);
				battleEndData.mOldXp = mixedArray3.TryGet("old_xp", 0);
				battleEndData.mNextXpLevel = mixedArray3.TryGet("next_xp_level", 0);
				battleEndData.mTime = mixedArray3.TryGet("Time", 0);
				battleEndData.mBattleMoney = mixedArray3.TryGet("BattleMoney", 0);
				_arg.mData.Add(key, battleEndData);
			}
		}
	}
}
