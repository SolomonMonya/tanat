using AMF;

namespace TanatKernel
{
	internal class EffectStartArgParser : BattlePacketArgParser<EffectStartArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref EffectStartArg _arg)
		{
			_arg.mEffectId = SafeGet(_packet, "effect", -1);
			_arg.mOwnerId = SafeGet(_packet, "owner", -1);
			_arg.mFx = SafeGet(_packet, "fx", "");
			_arg.mArgs = _packet.Arguments.TryGet("args", new MixedArray());
		}
	}
}
