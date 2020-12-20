using System.Collections.Generic;

namespace TanatKernel
{
	public interface ICastleJoinGui
	{
		int LastRequest
		{
			get;
		}

		void Init();

		void ShowMessage(int _error);

		void ShowDeferredMessage(DeferredMessageArg _arg);

		void SetTimerMessage(CastleStartBattleInfoArg _arg);

		void Notify(string text);

		void SetFighters(Dictionary<int, List<Clan.ClanMember>> _arg, string _tag, bool _canLeave);

		void Desert();

		void SetCastleList(CastleListArg _arg);

		void FightersErrorOccured(int _error);

		void SetCastleMembers(CastleMembersArg _arg);

		void SetCastleBattleInfo(CastleBattleInfoArg _arg);

		void ClanInfoUpdated();

		void SetCastleHistory(List<CastleHistory> _history);

		void SetTimer(int _timeSec);

		void Won();

		void StartCastleBattle(CastleStartRequestArg _arg);

		void SetAvatarTimer(int _time, List<int> _users);

		void SetAvatar(int _userId, int _avatarId);

		void SetReadyStatus(int _id);

		void PlayerDeserted(int _id);

		void SelfAvatarError();
	}
}
