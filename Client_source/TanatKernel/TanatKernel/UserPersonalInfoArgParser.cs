using AMF;

namespace TanatKernel
{
	internal class UserPersonalInfoArgParser : CtrlPacketArgParser<UserPersonalInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref UserPersonalInfoArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet.Arguments, "info");
			_arg.mInfo.mName = SafeGet(args, "name", "");
			_arg.mInfo.mRegion = SafeGet(args, "region", 0);
			_arg.mInfo.mSlogan = SafeGet(args, "slogan", "");
			_arg.mInfo.mAbout = SafeGet(args, "about", "");
			MixedArray args2 = SafeGet(args, "birthdate", new MixedArray());
			_arg.mInfo.mDay = SafeGet(args2, "day", 0);
			_arg.mInfo.mMonth = SafeGet(args2, "month", 0);
			_arg.mInfo.mYear = SafeGet(args2, "year", 0);
		}
	}
}
