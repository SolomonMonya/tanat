using System;
using AMF;

namespace TanatKernel
{
	public class FightSender
	{
		private CtrlEntryPoint mEntryPoint;

		public FightSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void Join(int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.fight.join, new NamedVar("map_id", _mapId));
		}

		public void JoinHunt(int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.hunt.join, new NamedVar("map_id", _mapId));
		}

		public void Desert(int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.fight.desert, new NamedVar("map_id", _mapId));
		}

		public void GroupAnswer(int _mapId, bool _result)
		{
			mEntryPoint.Send(CtrlCmdId.fight.answer, new NamedVar("map_id", _mapId), new NamedVar("result", _result));
		}

		public void SelectAvatar(int _avatarId)
		{
			mEntryPoint.Send(CtrlCmdId.fight.select_avatar, new NamedVar("avatar_id", _avatarId));
		}

		public void JoinRequest()
		{
			mEntryPoint.Send(CtrlCmdId.fight.in_request);
		}

		public void Ready()
		{
			mEntryPoint.Send(CtrlCmdId.fight.ready);
		}

		public void MapsInfo()
		{
			mEntryPoint.Send(CtrlCmdId.arena.get_maps_info);
		}

		public void HuntReady(int _mapId, int _avatarId)
		{
			mEntryPoint.Send(CtrlCmdId.hunt.ready, new NamedVar("map_id", _mapId), new NamedVar("avatar_id", _avatarId));
		}

		public void LeaveInfoRequest()
		{
			mEntryPoint.Send(CtrlCmdId.user.leave_info);
		}
	}
}
