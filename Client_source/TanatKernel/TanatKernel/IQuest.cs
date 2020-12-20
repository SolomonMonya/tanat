using System.Collections.Generic;

namespace TanatKernel
{
	public interface IQuest : IStorable
	{
		QuestType Type
		{
			get;
		}

		QuestPvEType PvEType
		{
			get;
		}

		string Name
		{
			get;
		}

		string TaskDesc
		{
			get;
		}

		string StartDesc
		{
			get;
		}

		string InProgressDesc
		{
			get;
		}

		string WinDesc
		{
			get;
		}

		string LoseDesc
		{
			get;
		}

		int Money
		{
			get;
		}

		Currency MoneyCurrency
		{
			get;
		}

		int Exp
		{
			get;
		}

		string RewardDesc
		{
			get;
		}

		ICollection<IQuestReward> Rewards
		{
			get;
		}

		ICollection<IQuestReward> RewardsHuman
		{
			get;
		}

		ICollection<IQuestReward> RewardsElf
		{
			get;
		}

		IDictionary<int, IQuestProgressData> ProgressData
		{
			get;
		}

		bool ShowCur
		{
			get;
		}

		bool ShowMax
		{
			get;
		}
	}
}
