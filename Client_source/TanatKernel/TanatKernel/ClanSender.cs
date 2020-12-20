using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class ClanSender
	{
		private CtrlEntryPoint mEntryPoint;

		public ClanSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void CreateClanRequest(string _name, string _tag)
		{
			mEntryPoint.Send(CtrlCmdId.clan.create, new NamedVar("name", _name), new NamedVar("tag", _tag));
		}

		public void ClanInfo(string _clanId, string _userId, string _tag)
		{
			List<NamedVar> list = new List<NamedVar>();
			if (!string.IsNullOrEmpty(_clanId))
			{
				list.Add(new NamedVar("clan_id", _clanId));
			}
			if (!string.IsNullOrEmpty(_userId))
			{
				list.Add(new NamedVar("user_id", _userId));
			}
			if (!string.IsNullOrEmpty(_tag))
			{
				list.Add(new NamedVar("tag", _tag));
			}
			if (list.Count == 0)
			{
				Log.Warning("All parameters are null for ClanInfo request");
			}
			else
			{
				mEntryPoint.Send(CtrlCmdId.clan.info, list.ToArray());
			}
		}

		public void RemoveUser(int _userId)
		{
			mEntryPoint.Send(CtrlCmdId.clan.remove_user, new NamedVar("user_id", _userId));
		}

		public void RemoveClan()
		{
			mEntryPoint.Send(CtrlCmdId.clan.remove);
		}

		public void ChangeRole(Dictionary<int, Clan.Role> _newRoles)
		{
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, Clan.Role> _newRole in _newRoles)
			{
				mixedArray[_newRole.Key.ToString()] = (int)_newRole.Value;
			}
			mEntryPoint.Send(CtrlCmdId.clan.change_role, new NamedVar("roles", mixedArray));
		}

		public void InviteRequest(string _name)
		{
			mEntryPoint.Send(CtrlCmdId.clan.invite, new NamedVar("nick", _name));
		}

		public void RequestAnswer(bool _answer)
		{
			mEntryPoint.Send(CtrlCmdId.clan.invite_answer, new NamedVar("answer", _answer));
		}
	}
}
