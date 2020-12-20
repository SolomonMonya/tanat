using AMF;

namespace TanatKernel
{
	internal class HeroDataListMpdArgParser : CtrlPacketArgParser<HeroDataListMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packetArray, ref HeroDataListMpdArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packetArray, "arguments");
			MixedArray args = mixedArray.TryGet("data", new MixedArray());
			HeroDataListArgParser heroDataListArgParser = new HeroDataListArgParser();
			_arg.mData = new HeroDataListArg();
			heroDataListArgParser.ParseData(args, _arg.mData);
		}
	}
}
