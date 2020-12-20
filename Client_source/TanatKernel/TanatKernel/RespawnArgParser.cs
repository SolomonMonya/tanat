using AMF;

namespace TanatKernel
{
	internal class RespawnArgParser : BattlePacketArgParser<RespawnArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref RespawnArg _arg)
		{
			_arg.mPlayerId = SafeGet(_packet, "id", -1);
			if (_packet.Arguments.Associative.TryGetValue("time", out var value) && value.Value != null)
			{
				_arg.mTime = value;
				_arg.mContainsTime = true;
			}
			else
			{
				_arg.mContainsTime = false;
			}
			if (_packet.Arguments.Associative.TryGetValue("cost", out var value2) && value2.Value != null)
			{
				MixedArray mixedArray = value2;
				_arg.mVirtualCost = mixedArray.TryGet("v", 0);
				_arg.mRealCost = mixedArray.TryGet("r", 0);
				_arg.mContainsCost = true;
			}
			else
			{
				_arg.mContainsCost = false;
			}
		}
	}
}
