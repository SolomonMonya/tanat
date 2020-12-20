using AMF;

namespace TanatKernel
{
	internal class UpdateBagArgMpdParser : CtrlPacketArgParser<UpdateBagArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref UpdateBagArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			foreach (Variable item in mixedArray.Dense)
			{
				MixedArray args = item;
				UpdateBagArg.UpdData updData = new UpdateBagArg.UpdData();
				updData.mItemId = SafeGet<int>(args, "id");
				updData.mArticleId = SafeGet(args, "artikul_id", -1);
				updData.mCount = SafeGet(args, "cnt", 0);
				updData.mUsed = SafeGet(args, "used", 0);
				_arg.mUpdItems.Add(updData);
			}
		}
	}
}
