using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class Clan : IClanInfo
	{
		public enum Role
		{
			WARRIOR = 1,
			RECRUITER,
			COMMANDER,
			DEPUTY,
			HEAD
		}

		public enum RemoveReason
		{
			REMOVE_USER = 1,
			REMOVE_CLAN
		}

		public enum ErrorCode
		{
			NO_ERROR = 0,
			SHORT_NAME = 1,
			LONG_NAME = 2,
			DIFFERENT_LANG = 3,
			SYSTEM_ERROR = 7000,
			NOT_ENOUGH_MONEY = 7010,
			BAD_TAG = 7011,
			BAD_CLAN_NAME = 7012,
			TAG_EXIST = 7013,
			CLAN_NAME_EXIST = 7014,
			WRONG_PARAMETERS = 7015,
			NO_MEMBERS = 7016,
			YOU_NOT_MEMBER = 7017,
			USER_NOT_MEMBER = 7018,
			FORBIDDEN = 7019,
			WRONG_ROLE = 7020,
			USER_ALREADY_MEMBER = 7021,
			ALREADY_INVITED = 7022,
			NOT_ENOUGH_SPACE = 7023,
			NOT_INVITED = 7024,
			USER_OFFLINE = 7025,
			NO_HEAD_IN_CLAN = 7026,
			TWO_HEAD_IN_CLAN = 7027,
			HEAD_NEEDED = 7028,
			USER_IN_BATTLE = 7029,
			CANT_DELETE_CLAN_IN_CASTLE = 7030,
			CANT_DELETE_USER_IN_CASTLE = 7031,
			CANT_INVITE_TIMEOUT = 7032,
			PLAYER_IGNORE = 7033
		}

		public enum Text
		{
			INVITE_REQUEST = 0,
			INVITED_AGREE = 1,
			INVITED_DECLINE = 2,
			ERROR = 3,
			HEAD_CHANGE_HEAD = 4,
			CLAN_CREATED = 5,
			CLAN_REMOVED = 6,
			USER_REMOVED = 7,
			WRONG_USER_NAME = 8,
			WRONG_SEARCH_PARAMETERS = 9,
			ROLES_CHANGED = 10,
			ANOTHER_USER_REMOVED = 11,
			INVITE_TIMER = 12,
			SYSTEM_ERROR = 7000,
			NOT_ENOUGH_MONEY = 7010,
			BAD_TAG = 7011,
			BAD_CLAN_NAME = 7012,
			TAG_EXIST = 7013,
			CLAN_NAME_EXIST = 7014,
			WRONG_PARAMETERS = 7015,
			NO_MEMBERS = 7016,
			YOU_NOT_MEMBER = 7017,
			USER_NOT_MEMBER = 7018,
			FORBIDDEN = 7019,
			WRONG_ROLE = 7020,
			USER_ALREADY_MEMBER = 7021,
			ALREADY_INVITED = 7022,
			NOT_ENOUGH_SPACE = 7023,
			NOT_INVITED = 7024,
			USER_OFFLINE = 7025,
			NO_HEAD_IN_CLAN = 7026,
			TWO_HEAD_IN_CLAN = 7027,
			HEAD_NEEDED = 7028,
			USER_IN_BATTLE = 7029,
			CANT_DELETE_CLAN_IN_CASTLE = 7030,
			CANT_DELETE_USER_IN_CASTLE = 7031,
			CANT_INVITE_TIMEOUT = 7032,
			PLAYER_IN_IGNORE = 7033
		}

		public class ClanMember
		{
			private int mId;

			private string mName;

			private Role mRole;

			private string mLocation;

			public int Id => mId;

			public string Name => mName;

			public string Location
			{
				get
				{
					return mLocation;
				}
				set
				{
					mLocation = value;
				}
			}

			public Role Role
			{
				get
				{
					return mRole;
				}
				set
				{
					mRole = value;
				}
			}

			public ClanMember(int _id, string _name)
			{
				mId = _id;
				mName = _name;
			}

			public ClanMember Clone()
			{
				ClanMember clanMember = new ClanMember(mId, mName);
				clanMember.Role = mRole;
				clanMember.Location = mLocation;
				return clanMember;
			}
		}

		public class ClanInfo : IClanInfo
		{
			public int mId = -1;

			public string mClanName;

			public string mTag;

			public int mLevel;

			public int mRating;

			public Dictionary<int, ClanMember> mMembers = new Dictionary<int, ClanMember>();

			public int Id => mId;

			public string ClanName => mClanName;

			public string Tag => mTag;

			public int Level => mLevel;

			public int Rating => mRating;

			public IEnumerable<ClanMember> Members => mMembers.Values;

			public bool Contains(int _userId)
			{
				return mMembers.ContainsKey(_userId);
			}
		}

		public delegate void Action();

		public Action<Clan> mChangedCallback;

		public Action<ClanInfo> mClanInfoCallback;

		public Action mOnClanCreated;

		public Action mOnClanRemoved;

		public Action mOnSelfRemoved;

		public Action mOnSelfUpdated;

		private IGuiView mGuiView;

		private ClanSender mSender;

		private CtrlServerConnection mCtrlSrv;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		private ClanInfo mClanInfo = new ClanInfo();

		private Dictionary<int, Role> mTemporalRoles;

		private IStoreContentProvider<Player> mPlayerProv;

		private UserNetData mSelfUserData;

		private bool mShowInfo;

		public int Id
		{
			get
			{
				if (mClanInfo.mId < 0 && mCtrlSrv.SelfHero.Hero.GameInfo.mClanId > 0)
				{
					mClanInfo.mId = mCtrlSrv.SelfHero.Hero.GameInfo.mClanId;
					mClanInfo.mTag = mCtrlSrv.SelfHero.Hero.GameInfo.mClanTag;
				}
				return mClanInfo.mId;
			}
		}

		public string ClanName => mClanInfo.mClanName;

		public string Tag => mClanInfo.mTag;

		public int Level => mClanInfo.mLevel;

		public int Rating => mClanInfo.mRating;

		public IEnumerable<ClanMember> Members => mClanInfo.mMembers.Values;

		public int MembersCount => mClanInfo.mMembers.Values.Count;

		public ClanMember SelfMember
		{
			get
			{
				if (!Contains(mSelfUserData.UserId))
				{
					return null;
				}
				return mClanInfo.mMembers[mSelfUserData.UserId];
			}
		}

		public int SelfMemberId => mCtrlSrv.SelfHero.Hero.Id;

		public bool IsCreated
		{
			get
			{
				if (mClanInfo.mId == -1)
				{
					mClanInfo.mId = mCtrlSrv.SelfHero.Hero.GameInfo.mClanId;
				}
				return Id != -1;
			}
		}

		public Clan(CtrlServerConnection _ctrlSrv, UserNetData _selfUserData)
		{
			if (_ctrlSrv == null)
			{
				throw new ArgumentNullException("_ctrlSrv");
			}
			if (_selfUserData == null)
			{
				throw new ArgumentNullException("_selfUserData");
			}
			mCtrlSrv = _ctrlSrv;
			mSelfUserData = _selfUserData;
			mSender = mCtrlSrv.ClanSender;
		}

		public bool Contains(int _userId)
		{
			return mClanInfo.mMembers.ContainsKey(_userId);
		}

		public void Init(IGuiView _guiView, IStoreContentProvider<Player> _playerProv)
		{
			if (_guiView == null)
			{
				throw new ArgumentNullException("_guiView");
			}
			if (_playerProv == null)
			{
				throw new ArgumentNullException("_playerProv");
			}
			mGuiView = _guiView;
			mPlayerProv = _playerProv;
			mClanInfo = new ClanInfo();
		}

		public void Uninit()
		{
			mGuiView = null;
			mPlayerProv = null;
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			mHandlerMgr.Subscribe<CreateArg>(CtrlCmdId.clan.create, null, OnClanCreateError, OnClanCreated);
			mHandlerMgr.Subscribe<InfoMpdArg>(CtrlCmdId.clan.info_mpd, null, CommonError, OnClanBroadcastInfo);
			mHandlerMgr.Subscribe<InfoArg>(CtrlCmdId.clan.info, null, OnSearchFailed, OnClanInfo);
			mHandlerMgr.Subscribe<RemoveUserArg>(CtrlCmdId.clan.remove_user, null, CommonError, OnUserRemoved);
			mHandlerMgr.Subscribe<RemoveUserMpdArg>(CtrlCmdId.clan.remove_user_mpd, null, CommonError, OnUserRemovedBroadcast);
			mHandlerMgr.Subscribe(CtrlCmdId.clan.remove, OnClanRemoved, CommonError);
			mHandlerMgr.Subscribe(CtrlCmdId.clan.change_role, OnRolesChanged, OnRolesChangingFailed);
			mHandlerMgr.Subscribe(CtrlCmdId.clan.invite, null, OnInviteError);
			mHandlerMgr.Subscribe<InviteMpdArg>(CtrlCmdId.clan.invite_mpd, null, CommonError, OnUserInvited);
			mHandlerMgr.Subscribe<InviteAnswerMpdArg>(CtrlCmdId.clan.invite_answer_mpd, null, CommonError, OnInviteAnswer);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		private void InvokeCallback()
		{
			if (mChangedCallback != null)
			{
				mChangedCallback(this);
			}
		}

		private void InvokeCallback(ClanInfo _clanInfo)
		{
			if (mClanInfoCallback != null)
			{
				mClanInfoCallback(_clanInfo);
			}
		}

		private bool CheckPermissions(Role _minimumPermissions)
		{
			if (!Contains(mSelfUserData.UserId))
			{
				Log.Warning("You are not a clan member.");
				return false;
			}
			ClanMember clanMember = mClanInfo.mMembers[mSelfUserData.UserId];
			if (clanMember.Role < _minimumPermissions)
			{
				Log.Warning("You don`t have permissions.");
				mGuiView.Inform(GetType(), 7019, null);
				return false;
			}
			return true;
		}

		private void OnClanCreated(CreateArg _arg)
		{
			Player player = mPlayerProv.Get(mSelfUserData.UserId);
			ClanMember clanMember = new ClanMember(mSelfUserData.UserId, player.Name);
			clanMember.Role = Role.HEAD;
			mClanInfo.mMembers.Clear();
			mClanInfo.mMembers.Add(mSelfUserData.UserId, clanMember);
			mClanInfo.mId = _arg.mId;
			if (mOnClanCreated != null)
			{
				mOnClanCreated();
			}
		}

		private void OnClanCreateError(int _errorCode)
		{
			mClanInfo.mClanName = "";
			mClanInfo.mTag = "";
			switch (_errorCode)
			{
			case 7010:
			case 7011:
			case 7012:
			case 7013:
			case 7014:
				mGuiView.Inform(GetType(), _errorCode, null);
				break;
			default:
				CommonError(_errorCode);
				break;
			}
		}

		private void CommonError(int _errorCode)
		{
			Log.Warning("Unexpected error " + (ErrorCode)_errorCode);
			mGuiView.Inform(GetType(), _errorCode, null);
		}

		private void OnClanBroadcastInfo(InfoMpdArg _arg)
		{
			Player player = mPlayerProv.Get(_arg.mUserId);
			if (player != null && player.Hero != null)
			{
				HeroGameInfo gameInfo = player.Hero.GameInfo;
				if (_arg.mId != gameInfo.mClanId || _arg.mTag != gameInfo.mClanTag)
				{
					player.Hero.SetClanInfo(_arg.mId, _arg.mTag);
				}
			}
		}

		private void OnClanInfo(InfoArg _arg)
		{
			bool flag = false;
			List<ClanMember> list = new List<ClanMember>();
			foreach (InfoArg.User mUser in _arg.mUsers)
			{
				ClanMember clanMember = new ClanMember(mUser.mId, mUser.mName);
				clanMember.Location = mUser.mLocation;
				clanMember.Role = mUser.mRole;
				list.Add(clanMember);
				if (mUser.mId == mSelfUserData.UserId)
				{
					flag = true;
				}
			}
			if (Id == _arg.mId || flag)
			{
				mClanInfo.mId = _arg.mId;
				mClanInfo.mLevel = _arg.mLevel;
				mClanInfo.mRating = _arg.mRating;
				mClanInfo.mClanName = _arg.mName;
				mClanInfo.mTag = _arg.mTag;
				mClanInfo.mMembers.Clear();
				Player player = mPlayerProv.Get(mSelfUserData.UserId);
				HeroGameInfo gameInfo = player.Hero.GameInfo;
				if (_arg.mId != gameInfo.mClanId || _arg.mTag != gameInfo.mClanTag)
				{
					player.Hero.SetClanInfo(_arg.mId, _arg.mTag);
				}
				foreach (ClanMember item in list)
				{
					mClanInfo.mMembers[item.Id] = item;
				}
				if (mShowInfo)
				{
					InvokeCallback();
				}
				mShowInfo = true;
				if (mOnSelfUpdated != null)
				{
					mOnSelfUpdated();
				}
				return;
			}
			ClanInfo clanInfo = new ClanInfo();
			clanInfo.mId = _arg.mId;
			clanInfo.mClanName = _arg.mName;
			clanInfo.mLevel = _arg.mLevel;
			clanInfo.mRating = _arg.mRating;
			clanInfo.mTag = _arg.mTag;
			foreach (ClanMember item2 in list)
			{
				clanInfo.mMembers[item2.Id] = item2.Clone();
			}
			InvokeCallback(clanInfo);
		}

		private void OnUserRemoved(RemoveUserArg _arg)
		{
			if (Contains(_arg.mId))
			{
				mClanInfo.mMembers.Remove(_arg.mId);
				if (_arg.mId != mSelfUserData.UserId)
				{
					mGuiView.Inform(GetType(), 11, null);
				}
				else
				{
					mCtrlSrv.SelfHero.Hero.SetClanInfo(-1, string.Empty);
					mClanInfo = new ClanInfo();
				}
				InvokeCallback();
			}
		}

		private void OnUserRemovedBroadcast(RemoveUserMpdArg _arg)
		{
			if (_arg.mUserId == mSelfUserData.UserId)
			{
				if (_arg.mReason == RemoveReason.REMOVE_USER)
				{
					mGuiView.Inform(GetType(), 7, null);
				}
				if (_arg.mReason == RemoveReason.REMOVE_CLAN)
				{
					mGuiView.Inform(GetType(), 6, null);
				}
				mClanInfo = new ClanInfo();
				if (mOnSelfRemoved != null)
				{
					mOnSelfRemoved();
				}
			}
			mPlayerProv.Get(_arg.mUserId)?.Hero.SetClanInfo(-1, string.Empty);
		}

		private void OnClanRemoved()
		{
			mCtrlSrv.SelfHero.Hero.SetClanInfo(-1, string.Empty);
			mClanInfo = new ClanInfo();
			if (mOnClanRemoved != null)
			{
				mOnClanRemoved();
			}
		}

		private void OnRolesChanged()
		{
			if (mTemporalRoles == null)
			{
				Log.Warning("Roles was change...");
				RefreshInfo();
				return;
			}
			foreach (KeyValuePair<int, Role> mTemporalRole in mTemporalRoles)
			{
				mClanInfo.mMembers[mTemporalRole.Key].Role = mTemporalRole.Value;
			}
			mTemporalRoles = null;
			mGuiView.Inform(GetType(), 10, null);
			InvokeCallback();
		}

		private void OnRolesChangingFailed(int _errorCode)
		{
			mTemporalRoles = null;
			CommonError(_errorCode);
			RefreshInfo();
		}

		private void OnInviteError(int _errorCode)
		{
			if (_errorCode == 7015)
			{
				mGuiView.Inform(GetType(), 8, null);
			}
			else
			{
				CommonError(_errorCode);
			}
		}

		private void OnSearchFailed(int _errorCode)
		{
			if (_errorCode == 7015)
			{
				mGuiView.Inform(GetType(), 9, null);
			}
			else
			{
				CommonError(_errorCode);
			}
		}

		private void OnUserInvited(InviteMpdArg _arg)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (_arg.mTime > DateTime.Now)
			{
				dictionary["{TIME}"] = _arg.mTime.ToString("dd/MM/yy HH:mm");
				mGuiView.Inform(GetType(), 12, dictionary);
				return;
			}
			Notifier<IGuiView, object> notifier = new Notifier<IGuiView, object>();
			notifier.mCallback = delegate(bool _success, IGuiView _asker, object _data)
			{
				SendRequestAnswer(_success);
			};
			dictionary["{CLAN}"] = _arg.mClanName;
			dictionary["{HEAD}"] = _arg.mName;
			mGuiView.Ask(GetType(), 0, dictionary, notifier);
		}

		private void OnInviteAnswer(InviteAnswerMpdArg _arg)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["{USER}"] = _arg.mName;
			if (_arg.mAnswer)
			{
				mGuiView.Inform(GetType(), 1, dictionary);
				RefreshInfo();
			}
			else
			{
				mGuiView.Inform(GetType(), 2, dictionary);
			}
		}

		private void OnInviteRequest()
		{
			if (mOnClanCreated != null)
			{
				mOnClanCreated();
			}
		}

		public void CreateClan(string _name, string _tag)
		{
			if (mPlayerProv == null)
			{
				Log.Error("mPlayerProv not initialized. Cant find clan creator.");
				return;
			}
			mClanInfo.mClanName = _name;
			mClanInfo.mTag = _tag;
			mSender.CreateClanRequest(_name, _tag);
		}

		public void RefreshInfo()
		{
			RefreshInfo(_showInfo: true);
		}

		public void RefreshInfo(bool _showInfo)
		{
			mShowInfo = _showInfo;
			if (Id != -1)
			{
				GetClanInfo(Id.ToString(), "", "");
			}
			else
			{
				GetClanInfo("", mSelfUserData.UserId.ToString(), "");
			}
		}

		public void GetClanInfo(string _clanId, string _userId, string _tag)
		{
			mSender.ClanInfo(_clanId, _userId, _tag);
		}

		public void RemoveUser(int _userId)
		{
			if (mSelfUserData.UserId == _userId)
			{
				RemoveSelf();
			}
			else if (CheckPermissions(Role.RECRUITER))
			{
				if (!Contains(_userId))
				{
					Log.Warning("Try to remove wrong user from clan.");
				}
				else
				{
					mSender.RemoveUser(_userId);
				}
			}
		}

		public void RemoveSelf()
		{
			if (!Contains(mSelfUserData.UserId))
			{
				Log.Warning("You are not a clan member.");
				mGuiView.Inform(GetType(), 7017, null);
			}
			else if (SelfMember.Role == Role.HEAD)
			{
				mGuiView.Inform(GetType(), 7028, null);
			}
			else
			{
				mSender.RemoveUser(mSelfUserData.UserId);
			}
		}

		public void RemoveClan()
		{
			if (CheckPermissions(Role.HEAD))
			{
				mSender.RemoveClan();
			}
		}

		public void ChangeRole(Dictionary<int, Role> _newRoles)
		{
			if (!CheckPermissions(Role.DEPUTY))
			{
				return;
			}
			bool flag = false;
			foreach (KeyValuePair<int, Role> _newRole in _newRoles)
			{
				flag = flag || _newRole.Value == Role.HEAD;
			}
			if (flag && !CheckPermissions(Role.HEAD))
			{
				mGuiView.Inform(GetType(), 4, null);
				return;
			}
			mTemporalRoles = new Dictionary<int, Role>();
			foreach (KeyValuePair<int, Role> _newRole2 in _newRoles)
			{
				mTemporalRoles[_newRole2.Key] = _newRole2.Value;
			}
			mSender.ChangeRole(_newRoles);
		}

		public void InviteRequest(string _name)
		{
			if (CheckPermissions(Role.RECRUITER))
			{
				mSender.InviteRequest(_name);
			}
		}

		public void SendRequestAnswer(bool _answer)
		{
			if (_answer)
			{
				mHandlerMgr.Subscribe(CtrlCmdId.clan.invite_answer, OnInviteRequest, CommonError);
			}
			else
			{
				mHandlerMgr.Unsubscribe(CtrlCmdId.clan.invite_answer, OnInviteRequest, CommonError);
			}
			mSender.RequestAnswer(_answer);
		}

		public void Clear()
		{
			mClanInfo.mMembers.Clear();
		}

		public static bool IsValidName(string _name, out ErrorCode _error)
		{
			_error = CheckName(_name, 30, 4);
			if (_error != 0)
			{
				return false;
			}
			return true;
		}

		public static bool IsValidTag(string _name, out ErrorCode _error)
		{
			_error = CheckName(_name, 4, 2);
			if (_error != 0)
			{
				return false;
			}
			return true;
		}

		public static bool IsValidName(string _name)
		{
			if (CheckName(_name, 30, 4) != 0)
			{
				return false;
			}
			return true;
		}

		public static bool IsValidTag(string _name)
		{
			if (CheckName(_name, 4, 2) != 0)
			{
				return false;
			}
			return true;
		}

		private static ErrorCode CheckName(string _name, int _maxLimit, int _minLimit)
		{
			Regex regex = new Regex("^[a-z, ,_,A-Z,0-9]*$|^[а-я,А-Я,0-9, ,_]*$", RegexOptions.IgnoreCase);
			if (_name.Trim().Length > _maxLimit)
			{
				return ErrorCode.LONG_NAME;
			}
			if (_name.Trim().Length < _minLimit)
			{
				return ErrorCode.SHORT_NAME;
			}
			if (!regex.IsMatch(_name))
			{
				return ErrorCode.DIFFERENT_LANG;
			}
			return ErrorCode.NO_ERROR;
		}
	}
}
