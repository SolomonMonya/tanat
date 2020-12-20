using AMF;

namespace TanatKernel
{
	internal class NotifyBeaconArgParser : BattlePacketArgParser<NotifyBeaconArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref NotifyBeaconArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "pos", new MixedArray());
			_arg.mX = mixedArray.TryGet("x", 0f);
			_arg.mY = mixedArray.TryGet("y", 0f);
		}
	}
}
