namespace TanatKernel
{
	public struct JoinQueueStateArg
	{
		public enum Mode
		{
			QUEUE,
			REQUEST
		}

		public int mRequestId;

		public int mCountTeam1;

		public int mCountTeam2;

		public int mMode;
	}
}
