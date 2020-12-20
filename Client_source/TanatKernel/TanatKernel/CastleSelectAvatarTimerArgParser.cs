using AMF;

namespace TanatKernel
{
	internal class CastleSelectAvatarTimerArgParser : CtrlPacketArgParser<CastleSelectAvatarTimerArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleSelectAvatarTimerArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mTimer = mixedArray.TryGet("time", 0);
			MixedArray mixedArray2 = SafeGet<MixedArray>(mixedArray, "fighters");
			foreach (Variable item in mixedArray2.Dense)
			{
				_arg.mFightersId.Add(item.Cast<int>());
			}
		}
	}
}
