using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class CastleFighterListArgParser : CtrlPacketArgParser<CastleFighterListArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref CastleFighterListArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "fighters");
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				if (!int.TryParse(item.Key, out var result))
				{
					Log.Error("cant parse battle id - " + item.Key);
					continue;
				}
				int value = item.Value.Cast<int>();
				_arg.mFighters[result] = value;
			}
			_arg.mCanLeave = SafeGet<bool>(_packet, "can_leave");
		}
	}
}
