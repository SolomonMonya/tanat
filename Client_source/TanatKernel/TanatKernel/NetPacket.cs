using AMF;

namespace TanatKernel
{
	public abstract class NetPacket<IdT>
	{
		protected IdT mId;

		protected MixedArray mArguments;

		public IdT Id => mId;

		public MixedArray Arguments => mArguments;

		public abstract Variable Serialize();
	}
}
