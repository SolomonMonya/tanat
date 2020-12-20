using System;
using System.Runtime.CompilerServices;
using Network;

namespace TanatKernel
{
	public class SelfPvpQuestStore : Store<ISelfPvpQuest>
	{
		private class SelfQuest : ISelfPvpQuest, IStorable
		{
			public int mState;

			public int mLimit;

			public float mEndTime;

			private IQuest mQuest;

			public int Id => mQuest.Id;

			public IQuest Quest => mQuest;

			public int State => mState;

			public int Limit => mLimit;

			public float EndTime => mEndTime;

			public SelfQuest(IQuest _quest)
			{
				mQuest = _quest;
			}
		}

		public delegate void OnUpdatedCallback(SelfPvpQuestStore _store, ISelfPvpQuest _quest, bool _added);

		private IStoreContentProvider<IQuest> mQuestProv;

		private HandlerManager<BattlePacket, BattleCmdId> mHandlerMgr;

		private OnUpdatedCallback mOnUpdatedCallback = delegate
		{
		};

		[CompilerGenerated]
		private static OnUpdatedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public SelfPvpQuestStore(IStoreContentProvider<IQuest> _questProv)
			: base("SelfPvpQuests")
		{
			if (_questProv == null)
			{
				throw new ArgumentNullException("_questProv");
			}
			mQuestProv = _questProv;
		}

		public void Subscribe(HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<QuestTaskArg>(BattleCmdId.QUEST_TASK, null, null, OnTask);
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

		private void OnTask(QuestTaskArg _arg)
		{
			SelfQuest selfQuest = TryGet(_arg.mQuestId) as SelfQuest;
			bool added = false;
			if (selfQuest == null)
			{
				IQuest quest = mQuestProv.Get(_arg.mQuestId);
				if (quest == null)
				{
					return;
				}
				selfQuest = new SelfQuest(quest);
				lock (this)
				{
					Add(selfQuest);
				}
				added = true;
			}
			selfQuest.mState = _arg.mState;
			selfQuest.mLimit = _arg.mLimit;
			selfQuest.mEndTime = _arg.mEndTime;
			if (selfQuest.mState == -3 || selfQuest.mState == -2)
			{
				lock (this)
				{
					Remove(selfQuest.Id);
				}
			}
			mOnUpdatedCallback(this, selfQuest, added);
		}
	}
}
