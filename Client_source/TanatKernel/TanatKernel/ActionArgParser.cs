using AMF;

namespace TanatKernel
{
	internal class ActionArgParser : BattlePacketArgParser<ActionArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref ActionArg _arg)
		{
			_arg.mObjId = SafeGet(_packet, "id", -1);
			_arg.mActionId = SafeGet(_packet, "action", -1);
			_arg.mTargetId = SafeGet(_packet, "targetObj", -1);
			_arg.mStartTime = SafeGet(_packet, "start", 0f);
			_arg.mItem = SafeGet(_packet, "item", _default: false);
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "targetPos", null);
			if (mixedArray != null)
			{
				_arg.mTargetX = mixedArray.TryGet("x", 0);
				_arg.mTargetY = mixedArray.TryGet("y", 0);
			}
		}
	}
}
