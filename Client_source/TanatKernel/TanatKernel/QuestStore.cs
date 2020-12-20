using System;
using System.Collections.Generic;
using System.IO;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class QuestStore : Store<IQuest>
	{
		public class Quest : IQuest, IStorable
		{
			private int mId;

			public QuestType mType;

			public QuestPvEType mPveType;

			public string mName;

			public string mTaskDesc;

			public string mStartDesc;

			public string mInProgressDesc;

			public string mWinDesc;

			public string mLoseDesc;

			public int mMoney;

			public Currency mMoneyCurrency;

			public int mExp;

			public string mRewardDesc;

			public List<IQuestReward> mRewards = new List<IQuestReward>();

			public List<IQuestReward> mRewardsHuman = new List<IQuestReward>();

			public List<IQuestReward> mRewardsElf = new List<IQuestReward>();

			public Dictionary<int, IQuestProgressData> mProgressData = new Dictionary<int, IQuestProgressData>();

			public bool mShowCur;

			public bool mShowMax;

			public int Id => mId;

			public QuestType Type => mType;

			public QuestPvEType PvEType => mPveType;

			public string Name => mName;

			public string TaskDesc => mTaskDesc;

			public string StartDesc => mStartDesc;

			public string InProgressDesc => mInProgressDesc;

			public string WinDesc => mWinDesc;

			public string LoseDesc => mLoseDesc;

			public int Money => mMoney;

			public Currency MoneyCurrency => mMoneyCurrency;

			public int Exp => mExp;

			public string RewardDesc => mRewardDesc;

			public ICollection<IQuestReward> Rewards => mRewards;

			public ICollection<IQuestReward> RewardsHuman => mRewardsHuman;

			public ICollection<IQuestReward> RewardsElf => mRewardsElf;

			public IDictionary<int, IQuestProgressData> ProgressData => mProgressData;

			public bool ShowCur => mShowCur;

			public bool ShowMax => mShowMax;

			public Quest(int _id)
			{
				mId = _id;
			}
		}

		private class Reward : IQuestReward
		{
			public int mActicleId;

			public int mCount;

			private IStoreContentProvider<CtrlPrototype> mCtrlProtoProv;

			public CtrlPrototype ArticleProto => mCtrlProtoProv.Get(mActicleId);

			public int Count => mCount;

			public Reward(IStoreContentProvider<CtrlPrototype> _ctrlProtoProv)
			{
				mCtrlProtoProv = _ctrlProtoProv;
			}
		}

		private class QuestProgressData : IQuestProgressData
		{
			public string mDesc;

			public int mMaxVal;

			public string Desc => mDesc;

			public int MaxVal => mMaxVal;
		}

		private IStoreContentProvider<CtrlPrototype> mCtrlProtoProv;

		public QuestStore(IStoreContentProvider<CtrlPrototype> _ctrlProtoProv)
			: base("Quests")
		{
			if (_ctrlProtoProv == null)
			{
				throw new ArgumentNullException("_ctrlProtoProv");
			}
			mCtrlProtoProv = _ctrlProtoProv;
		}

		public void Retrieve(Stream _stream)
		{
			if (_stream == null)
			{
				throw new ArgumentNullException("_stream");
			}
			Formatter formatter = new Formatter();
			_stream.Position = 0L;
			Variable variable = formatter.Deserialize(_stream);
			if (variable == null || variable.ValueType != typeof(MixedArray))
			{
				Log.Warning("invalid root amf element");
				return;
			}
			MixedArray mixedArray = variable;
			foreach (Variable item in mixedArray.Dense)
			{
				if (item.ValueType != typeof(MixedArray))
				{
					continue;
				}
				MixedArray mixedArray2 = item;
				if (!mixedArray2.Associative.TryGetValue("id", out var value))
				{
					continue;
				}
				int id = value;
				Quest quest = new Quest(id);
				quest.mType = (QuestType)mixedArray2.TryGet("type", 0);
				quest.mPveType = (QuestPvEType)mixedArray2.TryGet("pve_type", 0);
				quest.mName = mixedArray2.TryGet("name", "");
				quest.mTaskDesc = mixedArray2.TryGet("task_desc", "");
				quest.mStartDesc = mixedArray2.TryGet("start_desc", "");
				quest.mInProgressDesc = mixedArray2.TryGet("in_progress_desc", "");
				quest.mWinDesc = mixedArray2.TryGet("win_desc", "");
				quest.mLoseDesc = mixedArray2.TryGet("lose_desc", "");
				quest.mMoney = mixedArray2.TryGet("money", 0);
				quest.mMoneyCurrency = (Currency)mixedArray2.TryGet("money_type", 1);
				quest.mExp = mixedArray2.TryGet("exp", 0);
				quest.mRewardDesc = mixedArray2.TryGet("reward_desc", "");
				quest.mShowCur = mixedArray2.TryGet("show_cur", _default: true);
				quest.mShowMax = mixedArray2.TryGet("show_max", _default: true);
				ParseRewards(mixedArray2, quest.mRewards, "rewards");
				ParseRewards(mixedArray2, quest.mRewardsHuman, "rewards_human");
				ParseRewards(mixedArray2, quest.mRewardsElf, "rewards_elf");
				MixedArray mixedArray3 = mixedArray2.TryGet("progress", new MixedArray());
				foreach (KeyValuePair<string, Variable> item2 in mixedArray3.Associative)
				{
					if (int.TryParse(item2.Key, out var result))
					{
						QuestProgressData questProgressData = new QuestProgressData();
						MixedArray mixedArray4 = item2.Value;
						questProgressData.mDesc = mixedArray4.TryGet("desc", "");
						questProgressData.mMaxVal = mixedArray4.TryGet("max", 0);
						quest.mProgressData.Add(result, questProgressData);
					}
				}
				Add(quest);
			}
		}

		private void ParseRewards(MixedArray _data, ICollection<IQuestReward> _rewards, string _argName)
		{
			MixedArray mixedArray = _data.TryGet(_argName, new MixedArray());
			foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
			{
				Reward reward = new Reward(mCtrlProtoProv);
				if (int.TryParse(item.Key, out reward.mActicleId))
				{
					reward.mCount = item.Value;
					_rewards.Add(reward);
				}
			}
		}
	}
}
