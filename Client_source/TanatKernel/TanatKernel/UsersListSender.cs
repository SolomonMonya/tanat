using System;
using AMF;

namespace TanatKernel
{
	public class UsersListSender
	{
		private CtrlEntryPoint mEntryPoint;

		public UsersListSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void GetFullHeroInfo(int _userId)
		{
			mEntryPoint.Send(CtrlCmdId.user.full_hero_info, new NamedVar("user_id", _userId));
		}

		public void Find(string _tag, string _nick)
		{
			mEntryPoint.Send(CtrlCmdId.user.find, new NamedVar("tag", _tag), new NamedVar("nick", _nick));
		}

		public void UpdateList(int _type)
		{
			mEntryPoint.Send(CtrlCmdId.user.get_bw_list, new NamedVar("type", _type));
		}

		public void AddToList(int _userId, int _type)
		{
			mEntryPoint.Send(CtrlCmdId.user.add_to_list, new NamedVar("user_id", _userId), new NamedVar("type", _type));
		}

		public void RemoveFromList(int _userId, int _type)
		{
			mEntryPoint.Send(CtrlCmdId.user.remove_from_list, new NamedVar("user_id", _userId), new NamedVar("type", _type));
		}

		public void AnswerForFriendRequest(int _userId, bool _answer)
		{
			mEntryPoint.Send(CtrlCmdId.user.friend_answer, new NamedVar("user_id", _userId), new NamedVar("answer", _answer));
		}

		public void GetObserverInfo(int _userId)
		{
			mEntryPoint.Send(CtrlCmdId.user.get_observer_info, new NamedVar("user_id", _userId));
		}
	}
}
