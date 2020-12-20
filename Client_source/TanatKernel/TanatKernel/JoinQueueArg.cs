namespace TanatKernel
{
	public struct JoinQueueArg
	{
		public enum Status
		{
			OK,
			NO_PLACES,
			STARTED
		}

		public int mRequestId;

		public int mMapId;

		public int mStatus;
	}
}
