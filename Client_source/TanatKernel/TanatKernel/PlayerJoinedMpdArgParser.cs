using AMF;

namespace TanatKernel
{
	internal class PlayerJoinedMpdArgParser : CtrlPacketArgParser<PlayerJoinedArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref PlayerJoinedArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = SafeGet<int>(mixedArray, "id");
			_arg.mNick = SafeGet<string>(mixedArray, "nick");
			_arg.mLevel = SafeGet<int>(mixedArray, "level");
			_arg.mTeam = SafeGet<int>(mixedArray, "team");
			_arg.mInRequest = mixedArray.TryGet("in_request", _default: true);
			_arg.mAvatar = mixedArray.TryGet("avatar", -1);
			_arg.mIsReady = mixedArray.TryGet("ready", _default: false);
			_arg.mWait = mixedArray.TryGet("wait", -1);
			_arg.mServTime = mixedArray.TryGet("current_time", 0);
		}
	}
}
