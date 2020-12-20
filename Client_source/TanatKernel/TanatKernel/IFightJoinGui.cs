using System.Collections.Generic;

namespace TanatKernel
{
	public interface IFightJoinGui
	{
		void SetBattleInfo(FightStartSelectAvatarMpdArg _arg, int _timer);

		void PlayersFound(int _mapId, int _timer, bool _needClear);

		void SetReady(int _userId);

		void SetAvatar(int _iserId, int _avatarId);

		void PlayerDeserted();

		void PlayerJoined(int _userId);

		void ShowMessage(string _messageId);

		void ShowError(int _error);

		void FriendInvite(string _nick, int _mapId, bool _isLeave);

		void Joined(IJoinedQueue _queue, bool isNew);

		void ShowMapsDesc(List<MapData> _info);

		void Binded();

		void ExitFromQueue(int _id);

		void SetTimer(int _time);

		void SelfAvatarError();

		void BanMessage(int _time);
	}
}
