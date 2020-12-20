using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class CastleHistoryArgParser : CtrlPacketArgParser<CastleHistoryArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleHistoryArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "battles");
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				MixedArray args = item.Value;
				if (!int.TryParse(item.Key, out var result))
				{
					Log.Error("cant parse battle id - " + item.Key);
					continue;
				}
				CastleHistory castleHistory = new CastleHistory();
				castleHistory.mBattleId = result;
				int num = SafeGet<int>(args, "date");
				castleHistory.mDate = DateTime.Now.AddSeconds(-num);
				castleHistory.mWinnerId = SafeGet<int>(args, "winner_id");
				castleHistory.mWinnerName = SafeGet<string>(args, "winner_name");
				_arg.mHistory.Add(castleHistory);
			}
		}
	}
}
