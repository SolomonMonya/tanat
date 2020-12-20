using AMF;

namespace TanatKernel
{
	internal class FullHeroInfoArgParser : CtrlPacketArgParser<FullHeroInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packetArray, ref FullHeroInfoArg _arg)
		{
			MixedArray args = SafeGet<MixedArray>(_packetArray, "visual_data");
			MixedArray args2 = SafeGet<MixedArray>(_packetArray, "hero_data");
			_arg.mNick = SafeGet<string>(_packetArray, "nick");
			HeroDataListArgParser heroDataListArgParser = new HeroDataListArgParser();
			heroDataListArgParser.ParseData(args, _arg.mData);
			HeroGameInfoArgParser.ParseGameInfo(args2, _arg.mInfo, out _arg.mHeroId);
		}
	}
}
