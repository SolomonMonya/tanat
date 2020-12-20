using System;
using AMF;

namespace TanatKernel
{
	public class QuestSender
	{
		private CtrlEntryPoint mEntryPoint;

		public QuestSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void UpdateNpcs()
		{
			mEntryPoint.Send(CtrlCmdId.npc.list);
		}

		public void UpdateSelfQuests()
		{
			mEntryPoint.Send(CtrlCmdId.quest.update);
		}

		public void Accept(int _questId)
		{
			mEntryPoint.Send(CtrlCmdId.quest.accept, new NamedVar("quest_id", _questId));
		}

		public void Cancel(int _questId)
		{
			mEntryPoint.Send(CtrlCmdId.quest.cancel, new NamedVar("quest_id", _questId));
		}

		public void Done(int _questId)
		{
			mEntryPoint.Send(CtrlCmdId.quest.done, new NamedVar("quest_id", _questId));
		}

		public void GetBag()
		{
			mEntryPoint.Send(CtrlCmdId.user.bag);
		}
	}
}
