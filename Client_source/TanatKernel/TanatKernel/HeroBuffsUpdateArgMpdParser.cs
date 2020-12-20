using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class HeroBuffsUpdateArgMpdParser : CtrlPacketArgParser<HeroBuffsUpdateMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref HeroBuffsUpdateMpdArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			MixedArray mixedArray2 = mixedArray.TryGet("buffs", new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray2.Associative)
			{
				int key = int.Parse(item.Key);
				double value = item.Value;
				_arg.mBuffs[key] = DateTime.Now.AddSeconds(value);
			}
		}
	}
}
