using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	public class CastleSender
	{
		private CtrlEntryPoint mEntryPoint;

		public CastleSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void CastleListRequest()
		{
			mEntryPoint.Send(CtrlCmdId.castle.list);
		}

		public void GetCastleInfo(int _castleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.info, new NamedVar("castle_id", _castleId));
		}

		public void GetBattleInfo(int _castleId, int _battleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.battle_info, new NamedVar("castle_id", _castleId), new NamedVar("battle_id", _battleId.ToString()));
		}

		public void GetCurrentBattleInfo(int _castleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.battle_info, new NamedVar("castle_id", _castleId));
		}

		public void GetFighters(int _castleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.fighters, new NamedVar("castle_id", _castleId));
		}

		public void SetFighters(int _castleId, Dictionary<int, int> _fighters)
		{
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> _fighter in _fighters)
			{
				mixedArray[_fighter.Key.ToString()] = _fighter.Value;
			}
			mEntryPoint.Send(CtrlCmdId.castle.set_fighters, new NamedVar("castle_id", _castleId), new NamedVar("fighters", mixedArray));
		}

		public void DesertCastlerequest(int _castleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.desert, new NamedVar("castle_id", _castleId));
		}

		public void SelectAvatar(int _avatarId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.select_avatar, new NamedVar("avatar", _avatarId));
		}

		public void SetReady()
		{
			mEntryPoint.Send(CtrlCmdId.castle.ready);
		}

		public void DesertBattle()
		{
			mEntryPoint.Send(CtrlCmdId.castle.desert_battle);
		}

		public void GetCastleHistory(int _castleId)
		{
			mEntryPoint.Send(CtrlCmdId.castle.history, new NamedVar("castle_id", _castleId));
		}
	}
}
