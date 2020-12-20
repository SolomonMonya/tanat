using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class HeroGameInfoArgParser : CtrlPacketArgParser<HeroGameInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroGameInfoArg _arg)
		{
			ParseGameInfo(_packet.Arguments, _arg.mInfo, out _arg.mHeroId);
		}

		public static void ParseGameInfo(MixedArray _args, HeroGameInfo _gameInfo, out int _heroId)
		{
			_heroId = _args.TryGet("user_id", -1);
			_gameInfo.mLevel = _args.TryGet("level", -1);
			_gameInfo.mExp = _args.TryGet("exp", -1);
			_gameInfo.mNextExp = _args.TryGet("next_exp", -1);
			MixedArray mixedArray = _args.TryGet("stats", new MixedArray());
			_gameInfo.mRating = mixedArray.TryGet("HERO_RATING", -1);
			_gameInfo.mAssists = mixedArray.TryGet("ASSISTS", -1);
			_gameInfo.mAvatarKills = mixedArray.TryGet("AVATARKILLS", -1);
			_gameInfo.mCreepKills = mixedArray.TryGet("CREEPKILLS", -1);
			_gameInfo.mDeaths = mixedArray.TryGet("DEATHS", -1);
			_gameInfo.mFights = mixedArray.TryGet("FIGHTS", -1);
			_gameInfo.mWinFights = mixedArray.TryGet("WINS", -1);
			_gameInfo.mLoseFights = mixedArray.TryGet("LOSE", -1);
			_gameInfo.mLeaves = mixedArray.TryGet("FIGHTS_LEAVING", 0);
			_gameInfo.mHonor = mixedArray.TryGet("HONOR", 0);
			MixedArray mixedArray2 = _args.TryGet("clan_info", new MixedArray());
			_gameInfo.mClanId = mixedArray2.TryGet("id", -1);
			_gameInfo.mClanTag = mixedArray2.TryGet("tag", "");
			MixedArray mixedArray3 = _args.TryGet("buffs", new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray3.Associative)
			{
				int key = int.Parse(item.Key);
				double value = item.Value;
				_gameInfo.mBuffs[key] = DateTime.Now.AddSeconds(value);
			}
			AddStatsParser.Parse(_gameInfo.mAddStats, _args);
		}
	}
}
