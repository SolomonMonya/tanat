using System;
using AMF;

namespace TanatKernel
{
	public class GroupSender
	{
		private CtrlEntryPoint mEntryPoint;

		public GroupSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void GroupList()
		{
			mEntryPoint.Send(CtrlCmdId.user.group_list);
		}

		public void GroupList(int _userId)
		{
			mEntryPoint.Send(CtrlCmdId.user.group_list, new NamedVar("user_id", _userId));
		}

		public void JoinFromGroupRequest(int _userId, bool _isReferred)
		{
			mEntryPoint.Send(CtrlCmdId.user.join_from_group_request, new NamedVar("user_id", _userId), new NamedVar("referred", _isReferred));
		}

		public void JoinFromGroupAnswer(int _leaderId, bool _isAgreed)
		{
			mEntryPoint.Send(CtrlCmdId.user.join_from_group_answer, new NamedVar("user_id", _leaderId), new NamedVar("answer", _isAgreed));
		}

		public void JoinToGroupRequest(int _groupMemberId)
		{
			mEntryPoint.Send(CtrlCmdId.user.join_to_group_request, new NamedVar("user_id", _groupMemberId));
		}

		public void JoinToGroupAnswer(int _userId, bool _isAgreed)
		{
			mEntryPoint.Send(CtrlCmdId.user.join_to_group_answer, new NamedVar("user_id", _userId), new NamedVar("answer", _isAgreed));
		}

		public void Leave()
		{
			mEntryPoint.Send(CtrlCmdId.user.leave_group);
		}

		public void LeaveAsLeader()
		{
			mEntryPoint.Send(CtrlCmdId.user.leave_group, new NamedVar("user_id", -1));
		}

		public void ChangeLeader(int _newLeaderId)
		{
			mEntryPoint.Send(CtrlCmdId.user.change_leader, new NamedVar("user_id", _newLeaderId));
		}

		public void Remove(int _memberId)
		{
			mEntryPoint.Send(CtrlCmdId.user.remove_from_group, new NamedVar("user_id", _memberId));
		}
	}
}
