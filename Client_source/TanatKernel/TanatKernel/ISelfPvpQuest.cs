namespace TanatKernel
{
	public interface ISelfPvpQuest : IStorable
	{
		IQuest Quest
		{
			get;
		}

		int State
		{
			get;
		}

		int Limit
		{
			get;
		}

		float EndTime
		{
			get;
		}
	}
}
