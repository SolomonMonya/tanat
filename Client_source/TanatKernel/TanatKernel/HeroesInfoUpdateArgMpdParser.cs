using AMF;

namespace TanatKernel
{
	internal class HeroesInfoUpdateArgMpdParser : CtrlPacketArgParser<HeroesInfoUpdateArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroesInfoUpdateArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			foreach (Variable item in mixedArray.Dense)
			{
				HeroGameInfo heroGameInfo = new HeroGameInfo();
				HeroGameInfoArgParser.ParseGameInfo(item, heroGameInfo, out var _heroId);
				_arg.mInfo[_heroId] = heroGameInfo;
			}
		}
	}
}
