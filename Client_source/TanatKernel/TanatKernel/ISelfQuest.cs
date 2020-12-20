using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public interface ISelfQuest : IStorable
	{
		IQuest Quest
		{
			get;
		}

		QuestStatus Status
		{
			get;
		}

		ICollection<IQuestProgress> Progress
		{
			get;
		}

		bool HasCooldownTime
		{
			get;
		}

		DateTime CooldownExpireTime
		{
			get;
		}
	}
}
