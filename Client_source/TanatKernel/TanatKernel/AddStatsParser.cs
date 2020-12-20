using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class AddStatsParser
	{
		public static void Parse(Dictionary<string, AddStat> _addStats, MixedArray _args)
		{
			MixedArray mixedArray = _args.TryGet("add_stats", new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				MixedArray mixedArray2 = item.Value;
				AddStat addStat = new AddStat();
				addStat.mAdd = mixedArray2.TryGet("ADD", 0f);
				addStat.mMul = mixedArray2.TryGet("MUL", 0f);
				_addStats[item.Key] = addStat;
			}
		}
	}
}
