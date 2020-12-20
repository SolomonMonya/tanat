using AMF;

namespace TanatKernel
{
	internal class InfoMpdArgParser : CtrlPacketArgParser<InfoMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref InfoMpdArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mId = SafeGet<int>(args, "id");
			_arg.mUserId = SafeGet<int>(args, "user_id");
			_arg.mTag = SafeGet<string>(args, "tag");
		}
	}
}
