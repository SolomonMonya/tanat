using AMF;

namespace TanatKernel
{
	internal class MapTypeDescsArgParser : CtrlPacketArgParser<MapTypeDescsArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref MapTypeDescsArg _arg)
		{
			MixedArray mixedArray = SafeGet(_packet, "descs", new MixedArray());
			foreach (Variable item in mixedArray.Dense)
			{
				if (item.ValueType == typeof(MixedArray))
				{
					MixedArray mixedArray2 = item;
					int key = mixedArray2.TryGet("type_id", -1024);
					string value = mixedArray2.TryGet("desc", "");
					_arg.mDescs[key] = value;
				}
			}
		}
	}
}
