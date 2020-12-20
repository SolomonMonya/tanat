using System;

namespace TanatKernel
{
	public interface IJoinedQueue
	{
		MapData MapData
		{
			get;
		}

		DateTime StartTime
		{
			get;
		}
	}
}
