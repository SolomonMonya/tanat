using AMF;

namespace TanatKernel
{
	internal class AddAffectorArgParser : BattlePacketArgParser<AddEffectorArg>
	{
		protected override void ParseArg(BattlePacket _packet, ref AddEffectorArg _arg)
		{
			_arg.mEffectorId = SafeGet(_packet, "id", -1);
			_arg.mEffectorProtoId = SafeGet(_packet, "proto", -1);
			_arg.mOwnerId = SafeGet(_packet, "owner", -1);
			_arg.mParentId = SafeGet(_packet, "parent", -1);
			_arg.mArgs = _packet.Arguments.TryGet("args", new MixedArray());
		}
	}
}
