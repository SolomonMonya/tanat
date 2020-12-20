namespace TanatKernel
{
	public interface INpc : IStorable
	{
		string Name
		{
			get;
		}

		string Desc
		{
			get;
		}

		string Icon
		{
			get;
		}

		bool NeedShow
		{
			get;
		}

		int[] Quests
		{
			get;
		}
	}
}
