namespace TanatKernel
{
	internal class LoginArgParser : CtrlPacketArgParser<LoginArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref LoginArg _arg)
		{
			_arg.mUserId = SafeGet<int>(_packet, "id");
			_arg.mUserName = SafeGet<string>(_packet, "username");
			_arg.mSessKey = SafeGet<string>(_packet, "sess_key");
			_arg.mUserFlags = SafeGet<int>(_packet, "flags");
		}
	}
}
