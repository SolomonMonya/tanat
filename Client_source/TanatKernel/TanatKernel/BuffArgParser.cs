using System;

namespace TanatKernel
{
	internal class BuffArgParser : BattlePacketArgParser<BuffArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref BuffArg _arg)
		{
			_arg.mId = SafeGet(_packet, "id", -1);
			_arg.mBuff = SafeGet(_packet, "buff", -1);
			_arg.mEndTime = DateTime.Now.AddSeconds(SafeGet(_packet, "time", 0.0));
		}
	}
}
