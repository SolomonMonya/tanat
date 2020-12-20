using AMF;

namespace TanatKernel
{
	internal class HuntReadyArgParser : CtrlPacketArgParser<HuntReadyArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HuntReadyArg _arg)
		{
			MixedArray data = SafeGet<MixedArray>(_packet, "params");
			_arg.mLaunchArg = new FightLaunchArg();
			FightLaunchArgParser.Parse(data, _arg.mLaunchArg);
		}
	}
}
