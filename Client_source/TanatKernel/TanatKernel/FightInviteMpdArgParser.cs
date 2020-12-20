using AMF;

namespace TanatKernel
{
	internal class FightInviteMpdArgParser : CtrlPacketArgParser<FightInviteMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref FightInviteMpdArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mMapId = SafeGet<int>(mixedArray, "map_id");
			_arg.mNick = SafeGet<string>(mixedArray, "nick");
			if (mixedArray.Associative.ContainsKey("leave"))
			{
				_arg.mIsLeave = SafeGet<bool>(mixedArray, "leave");
			}
		}
	}
}
