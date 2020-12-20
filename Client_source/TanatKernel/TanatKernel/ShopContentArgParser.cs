using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ShopContentArgParser : CtrlPacketArgParser<ShopContentArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ShopContentArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "store", new MixedArray());
			foreach (KeyValuePair<string, Variable> item2 in mixedArray.Associative)
			{
				if (!int.TryParse(item2.Key, out var result))
				{
					continue;
				}
				MixedArray args = item2.Value;
				ShopContentArg.Category category = new ShopContentArg.Category();
				category.mWeight = SafeGet<int>(args, "weight");
				MixedArray mixedArray2 = SafeGet<MixedArray>(args, "items");
				foreach (Variable item3 in mixedArray2.Dense)
				{
					int item = item3;
					category.mArticles.Add(item);
				}
				_arg.mItems[result] = category;
			}
		}
	}
}
