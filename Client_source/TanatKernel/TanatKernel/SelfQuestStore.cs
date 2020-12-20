using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class SelfQuestStore : Store<ISelfQuest>
	{
		private class SelfQuest : ISelfQuest, IStorable
		{
			public QuestStatus mStatus;

			public bool mHasCooldownTime;

			public DateTime mCooldownExpireTime;

			public Dictionary<int, QuestProgressCur> mCurProgress = new Dictionary<int, QuestProgressCur>();

			private IQuest mQuest;

			private List<IQuestProgress> mProgress;

			public int Id => mQuest.Id;

			public IQuest Quest => mQuest;

			public QuestStatus Status => mStatus;

			public bool HasCooldownTime => mHasCooldownTime;

			public DateTime CooldownExpireTime => mCooldownExpireTime;

			public ICollection<IQuestProgress> Progress
			{
				get
				{
					if (mProgress == null)
					{
						mProgress = new List<IQuestProgress>();
						foreach (KeyValuePair<int, QuestProgressCur> item2 in mCurProgress)
						{
							if (!mQuest.ProgressData.TryGetValue(item2.Key, out var value))
							{
								Log.Error("cannot find progress data " + item2.Key + " in quest " + mQuest.Id);
							}
							else
							{
								QuestProgress item = new QuestProgress(value, item2.Value);
								mProgress.Add(item);
							}
						}
					}
					return mProgress;
				}
			}

			public SelfQuest(IQuest _quest)
			{
				mQuest = _quest;
			}
		}

		private class QuestProgressCur : IQuestProgressCur
		{
			public int mCurVal;

			public int CurVal => mCurVal;
		}

		private class QuestProgress : IQuestProgress, IQuestProgressData, IQuestProgressCur
		{
			private IQuestProgressData mData;

			private IQuestProgressCur mCur;

			public string Desc => mData.Desc;

			public int MaxVal => mData.MaxVal;

			public int CurVal => mCur.CurVal;

			public QuestProgress(IQuestProgressData _data, IQuestProgressCur _cur)
			{
				mData = _data;
				mCur = _cur;
			}
		}

		public delegate void OnUpdatedCallback(SelfQuestStore _quests, IList<int> _added, IList<int> _updated);

		private IStoreContentProvider<IQuest> mQuestProv;

		private QuestSender mSender;

		private NpcStore mNpcs;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		private OnUpdatedCallback mOnUpdatedCallback = delegate
		{
		};

		private bool mNeedUpdateBag;

		[CompilerGenerated]
		private static OnUpdatedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public SelfQuestStore(QuestSender _sender, IStoreContentProvider<IQuest> _questProv, NpcStore _npcs)
			: base("SelfQuests")
		{
			if (_sender == null)
			{
				throw new ArgumentNullException("_sender");
			}
			if (_questProv == null)
			{
				throw new ArgumentNullException("_questProv");
			}
			if (_npcs == null)
			{
				throw new ArgumentNullException("_npcs");
			}
			mSender = _sender;
			mQuestProv = _questProv;
			mNpcs = _npcs;
		}

		public void UpdateContent()
		{
			Clear();
			mSender.UpdateSelfQuests();
		}

		public void AcceptQuest(int _questId)
		{
			ISelfQuest selfQuest = TryGet(_questId);
			if (selfQuest != null && selfQuest.Status != 0)
			{
				Log.Warning("quest " + _questId + " is already accepted");
			}
			else
			{
				mSender.Accept(_questId);
			}
		}

		public void Cancel(int _questId)
		{
			if (!Exists(_questId))
			{
				Log.Warning("cannot cancel quest " + _questId + " which is not accepted");
			}
			else
			{
				mSender.Cancel(_questId);
			}
		}

		public void Done(int _questId)
		{
			ISelfQuest selfQuest = TryGet(_questId);
			if (selfQuest == null)
			{
				Log.Warning("cannot done quest " + _questId + " which is not accepted");
			}
			else if (selfQuest.Status != QuestStatus.DONE)
			{
				Log.Warning("cannot done quest " + _questId + " with current status " + selfQuest.Status);
			}
			else
			{
				mSender.Done(_questId);
			}
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<QuestListArg>(CtrlCmdId.quest.update, null, null, OnQuestList);
			mHandlerMgr.Subscribe<QuestUpdateArg>(CtrlCmdId.quest.update_mpd, null, null, OnUpdateQuests);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		public void SubscribeOnUpdated(OnUpdatedCallback _callback)
		{
			mOnUpdatedCallback = (OnUpdatedCallback)Delegate.Combine(mOnUpdatedCallback, _callback);
		}

		public void UnsubscribeOnUpdated(OnUpdatedCallback _callback)
		{
			mOnUpdatedCallback = (OnUpdatedCallback)Delegate.Remove(mOnUpdatedCallback, _callback);
		}

		private bool UpdateQuest(QuestStateData _stateData)
		{
			QuestStatus questStatus = QuestStatus.NONE;
			bool flag = false;
			SelfQuest selfQuest;
			lock (this)
			{
				selfQuest = TryGet(_stateData.mQuestId) as SelfQuest;
			}
			if (selfQuest == null)
			{
				IQuest quest = mQuestProv.Get(_stateData.mQuestId);
				if (quest == null)
				{
					return false;
				}
				selfQuest = new SelfQuest(quest);
				foreach (KeyValuePair<int, int> item in _stateData.mCurProgress)
				{
					QuestProgressCur questProgressCur = new QuestProgressCur();
					questProgressCur.mCurVal = item.Value;
					selfQuest.mCurProgress.Add(item.Key, questProgressCur);
				}
				lock (this)
				{
					Add(selfQuest);
				}
				flag = true;
			}
			else
			{
				foreach (KeyValuePair<int, int> item2 in _stateData.mCurProgress)
				{
					if (!selfQuest.mCurProgress.TryGetValue(item2.Key, out var value))
					{
						Log.Error("cannot find progress id " + item2.Key + " in quest " + selfQuest.Id);
					}
					else
					{
						value.mCurVal = item2.Value;
					}
				}
				questStatus = selfQuest.mStatus;
				flag = false;
			}
			selfQuest.mStatus = (QuestStatus)_stateData.mStatus;
			if (questStatus != QuestStatus.NONE && selfQuest.mStatus != questStatus && (selfQuest.mStatus == QuestStatus.CLOSED || selfQuest.mStatus == QuestStatus.WAIT_COOLDOWN))
			{
				mNeedUpdateBag = true;
			}
			if (_stateData.mCooldownTime >= 0)
			{
				selfQuest.mCooldownExpireTime = DateTime.Now.AddSeconds(_stateData.mCooldownTime);
				selfQuest.mHasCooldownTime = true;
			}
			else
			{
				selfQuest.mHasCooldownTime = false;
				if (selfQuest.mStatus == QuestStatus.WAIT_COOLDOWN)
				{
					lock (this)
					{
						Remove(_stateData.mQuestId);
						return flag;
					}
				}
			}
			return flag;
		}

		private void OnQuestList(QuestListArg _arg)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			foreach (QuestStateData mCurStateDatum in _arg.mCurStateData)
			{
				if (UpdateQuest(mCurStateDatum))
				{
					list.Add(mCurStateDatum.mQuestId);
				}
				else
				{
					list2.Add(mCurStateDatum.mQuestId);
				}
			}
			mOnUpdatedCallback(this, list, list2);
		}

		private void OnUpdateQuests(QuestUpdateArg _arg)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			foreach (QuestStateData mCurStateDatum in _arg.mCurStateData)
			{
				if (UpdateQuest(mCurStateDatum))
				{
					list.Add(mCurStateDatum.mQuestId);
				}
				else
				{
					list2.Add(mCurStateDatum.mQuestId);
				}
			}
			mNpcs.UpdateContent();
			if (mNeedUpdateBag)
			{
				mSender.GetBag();
				mNeedUpdateBag = false;
			}
			mOnUpdatedCallback(this, list, list2);
		}
	}
}
