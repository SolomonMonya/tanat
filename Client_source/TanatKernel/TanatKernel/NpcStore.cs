using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Network;

namespace TanatKernel
{
	public class NpcStore : Store<INpc>
	{
		public class Npc : INpc, IStorable
		{
			private int mId;

			public string mName;

			public string mDesc;

			public string mIcon;

			public int[] mQuests;

			public bool mNeedShow;

			public int Id => mId;

			public string Name => mName;

			public string Desc => mDesc;

			public string Icon => mIcon;

			public int[] Quests => mQuests;

			public bool NeedShow => mNeedShow;

			public Npc(int _id)
			{
				mId = _id;
			}
		}

		public delegate void OnUpdatedCallback(NpcStore _npcs);

		private QuestSender mSender;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		private OnUpdatedCallback mOnUpdatedCallback = delegate
		{
		};

		[CompilerGenerated]
		private static OnUpdatedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public NpcStore(QuestSender _sender)
			: base("Npcs")
		{
			mSender = _sender;
		}

		public void UpdateContent()
		{
			mSender.UpdateNpcs();
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<NpcListArg>(CtrlCmdId.npc.list, null, null, OnNpcList);
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

		private void OnNpcList(NpcListArg _arg)
		{
			Clear();
			foreach (KeyValuePair<int, NpcData> mNpc in _arg.mNpcs)
			{
				Npc npc = new Npc(mNpc.Key);
				npc.mName = mNpc.Value.mName;
				npc.mDesc = mNpc.Value.mDesc;
				npc.mIcon = mNpc.Value.mIcon;
				npc.mNeedShow = mNpc.Value.mNeedShow;
				npc.mQuests = mNpc.Value.mQuests.ToArray();
				lock (this)
				{
					Add(npc);
				}
			}
			mOnUpdatedCallback(this);
		}
	}
}
