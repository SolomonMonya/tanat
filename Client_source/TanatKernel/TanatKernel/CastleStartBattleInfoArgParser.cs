using System;
using AMF;

namespace TanatKernel
{
	internal class CastleStartBattleInfoArgParser : CtrlPacketArgParser<CastleStartBattleInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleStartBattleInfoArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mType = (CastleStartBattleInfoArg.WaitType)SafeGet<int>(mixedArray, "type");
			_arg.mCastleName = SafeGet<string>(mixedArray, "castle_name");
			_arg.mRound = mixedArray.TryGet("round", 0);
			_arg.mStage = mixedArray.TryGet("stage", 0);
			_arg.mStartTime = DateTime.Now.AddSeconds(mixedArray.TryGet("time", 0));
		}
	}
}
