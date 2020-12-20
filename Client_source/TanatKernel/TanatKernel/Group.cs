using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class Group
	{
		public class Member
		{
			private int mId;

			private string mName;

			private bool mIsOnline;

			private Group mGroup;

			public int Id => mId;

			public string Name => mName;

			public bool IsLeader => mId == mGroup.mLeaderId;

			public bool IsOnline => mIsOnline;

			public Player Player
			{
				get
				{
					if (mGroup.mPlayerProv == null)
					{
						return null;
					}
					return mGroup.mPlayerProv.TryGet(mId);
				}
			}

			public Hero Hero => mGroup.mCtrlSrv.Heroes.TryGet(mId);

			public Member(Group _group, int _id, string _name)
			{
				if (_group == null)
				{
					throw new ArgumentNullException("_group");
				}
				mGroup = _group;
				mId = _id;
				mName = _name;
			}

			public void SetOnline()
			{
				mIsOnline = true;
			}

			public void SetOffline()
			{
				mIsOnline = false;
			}
		}

		public delegate void ChangedCallback(Group _group);

		public enum Text
		{
			JOIN_REQUEST,
			ADD_REQUEST,
			DECLINED,
			NOT_IN_GROUP,
			ALREADY_IN_GROUP,
			ALREADY_HAS_REQUEST,
			PLAYER_DECLINED,
			PLAYER_DECLINED_5_TIME,
			PLAYER_IGNORED,
			UNEXPECTED_GROUP_ERROR
		}

		private enum JoinAnswer
		{
			NO_PLACE = 1,
			ADDED,
			DECLINED
		}

		private Dictionary<int, Member> mMembers = new Dictionary<int, Member>();

		private int mLeaderId;

		private ChangedCallback mChangedCallback = delegate
		{
		};

		private IStoreContentProvider<Player> mPlayerProv;

		private IGuiView mGuiView;

		private UserNetData mSelfUserData;

		private GroupSender mSender;

		private CtrlServerConnection mCtrlSrv;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		[CompilerGenerated]
		private static ChangedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		private int SelfHeroId => mSelfUserData.UserId;

		public bool IsEmpty => mMembers.Count == 0;

		public int MembersCount => mMembers.Count;

		public bool IsLeader => mLeaderId == SelfHeroId;

		public int LeaderId => mLeaderId;

		public IEnumerable<Member> Members => mMembers.Values;

		public Group(CtrlServerConnection _ctrlSrv, UserNetData _selfUserData)
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
			mSender = mCtrlSrv.GroupSender;
		}

		public bool Contains(int _userId)
		{
			return mMembers.ContainsKey(_userId);
		}

		public Member GetMemberById(int _id)
		{
			mMembers.TryGetValue(_id, out var value);
			return value;
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
			mHandlerMgr.Subscribe<GroupListArg>(CtrlCmdId.user.group_list, null, OnGroupError, OnGroupList);
			mHandlerMgr.Subscribe(CtrlCmdId.user.group_deleted_mpd, OnGroupDeleted, null);
			mHandlerMgr.Subscribe<RemoveFromGroupArg>(CtrlCmdId.user.remove_from_group_mpd, null, OnGroupError, OnRemoveFromGroup);
			mHandlerMgr.Subscribe(CtrlCmdId.user.leave_group, OnLeaved, null);
			mHandlerMgr.Subscribe<JoinedToGroupArg>(CtrlCmdId.user.joined_to_group_mpd, null, OnGroupError, OnJoinedToGroup);
			mHandlerMgr.Subscribe<RemovedFromGroupArg>(CtrlCmdId.user.removed_from_group_mpd, null, OnGroupError, OnRemovedFromGroup);
			mHandlerMgr.Subscribe<JoinFromGroupRequestArg>(CtrlCmdId.user.join_from_group_request_mpd, null, OnGroupError, OnJoinFromGroupRequest);
			mHandlerMgr.Subscribe<JoinFromGroupAnswerArg>(CtrlCmdId.user.join_from_group_answer_mpd, null, OnGroupError, OnJoinFromGroupAnswer);
			mHandlerMgr.Subscribe(CtrlCmdId.user.join_from_group_answer, OnJoinAnswer, null);
			mHandlerMgr.Subscribe<JoinFromGroupArg>(CtrlCmdId.user.join_from_group_request, null, OnGroupError, OnJoinFromGroup);
			mHandlerMgr.Subscribe<JoinToGroupRequestArg>(CtrlCmdId.user.join_to_group_request_mpd, null, OnGroupError, OnJoinToGroupRequest);
			mHandlerMgr.Subscribe<JoinToGroupAnswerArg>(CtrlCmdId.user.join_to_group_answer_mpd, null, OnGroupError, OnJoinToGroupAnswer);
			mHandlerMgr.Subscribe(CtrlCmdId.user.join_to_group_answer, OnJoinAnswer, null);
			mHandlerMgr.Subscribe<JoinToGroupArg>(CtrlCmdId.user.join_to_group_request, null, OnGroupError, OnJoinToGroup);
			mHandlerMgr.Subscribe<LeaderChangedArg>(CtrlCmdId.user.group_leader_changed_mpd, null, OnGroupError, OnLeaderChanged);
			mHandlerMgr.Subscribe<CtrlOnlineArg>(CtrlCmdId.user.online_mpd, null, OnGroupError, OnOnline);
			mHandlerMgr.Subscribe<CtrlOfflineArg>(CtrlCmdId.user.offline_mpd, null, OnGroupError, OnOffline);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		public void SubscribeChanged(ChangedCallback _callback)
		{
			mChangedCallback = (ChangedCallback)Delegate.Combine(mChangedCallback, _callback);
		}

		public void UnsubscribeChanged(ChangedCallback _callback)
		{
			mChangedCallback = (ChangedCallback)Delegate.Remove(mChangedCallback, _callback);
		}

		private void OnGroupList(GroupListArg _arg)
		{
			mMembers.Clear();
			foreach (GroupListArg.Member mMember in _arg.mMembers)
			{
				Member member = new Member(this, mMember.mId, mMember.mName);
				if (mMember.mIsOnline)
				{
					member.SetOnline();
				}
				else
				{
					member.SetOffline();
				}
				mCtrlSrv.Heroes.AddByPlayerId(mMember.mId);
				Hero hero = mCtrlSrv.Heroes.Get(mMember.mId);
				hero.GameInfo.mLevel = mMember.mLevel;
				hero.SetClanInfo(mMember.mClanId, mMember.mClanTag);
				hero.View.mGender = mMember.mGender;
				hero.View.mRace = mMember.mRace;
				if (!mMembers.ContainsKey(mMember.mId))
				{
					mMembers.Add(mMember.mId, member);
					continue;
				}
				Log.Error("This member in group already. " + mMember.mId + " " + mMember.mName);
			}
			if (_arg.mLeaderId != 0)
			{
				mLeaderId = _arg.mLeaderId;
			}
			else
			{
				mLeaderId = SelfHeroId;
			}
			mChangedCallback(this);
		}

		private void OnGroupDeleted()
		{
			mMembers.Clear();
			mLeaderId = SelfHeroId;
			mChangedCallback(this);
		}

		private void OnRemoveFromGroup(RemoveFromGroupArg _arg)
		{
			OnGroupDeleted();
		}

		private void OnLeaved()
		{
			OnGroupDeleted();
		}

		private void OnJoinedToGroup(JoinedToGroupArg _arg)
		{
			UpdateList();
			if (mPlayerProv != null)
			{
				if (IsEmpty)
				{
					Player player = mPlayerProv.Get(SelfHeroId);
					Member value = new Member(this, SelfHeroId, player.Name);
					mMembers[SelfHeroId] = value;
				}
				Member member = new Member(this, _arg.mUserId, _arg.mName);
				member.SetOnline();
				mMembers[_arg.mUserId] = member;
				mChangedCallback(this);
			}
		}

		private void OnRemovedFromGroup(RemovedFromGroupArg _arg)
		{
			mMembers.Remove(_arg.mUserId);
			if (_arg.mUserId == SelfHeroId || mMembers.Count == 1)
			{
				OnGroupDeleted();
			}
			else
			{
				mChangedCallback(this);
			}
		}

		private void OnJoinFromGroupRequest(JoinFromGroupRequestArg _arg)
		{
			Notifier<IGuiView, object> notifier = new Notifier<IGuiView, object>();
			notifier.mCallback = delegate(bool _success, IGuiView _asker, object _data)
			{
				mSender.JoinFromGroupAnswer(_arg.mLeaderIdd, _success);
			};
			mGuiView.Ask(GetType(), 0, _arg.mLeaderNick, notifier);
		}

		private void OnJoinFromGroupAnswer(JoinFromGroupAnswerArg _arg)
		{
			Log.Debug("OnJoinFromGroupAnswer : " + _arg.mAnswer);
			if (_arg.mAnswer == 3)
			{
				mGuiView.Inform(GetType(), 6, null);
			}
		}

		private void OnJoinToGroupRequest(JoinToGroupRequestArg _arg)
		{
			Notifier<IGuiView, object> notifier = new Notifier<IGuiView, object>();
			notifier.mCallback = delegate(bool _success, IGuiView _asker, object _data)
			{
				mSender.JoinToGroupAnswer(_arg.mUserIdd, _success);
			};
			mGuiView.Ask(GetType(), 1, _arg.mUserNick, notifier);
		}

		private void OnJoinToGroupAnswer(JoinToGroupAnswerArg _arg)
		{
			if (_arg.mAnswer == 2)
			{
				mSender.GroupList();
			}
			else
			{
				mGuiView.Inform(GetType(), 2, null);
			}
		}

		private void OnLeaderChanged(LeaderChangedArg _arg)
		{
			if (!IsEmpty)
			{
				mLeaderId = _arg.mLeaderId;
				mChangedCallback(this);
			}
		}

		private void OnJoinAnswer()
		{
			mSender.GroupList();
		}

		private void OnJoinToGroup(JoinToGroupArg _arg)
		{
			if (_arg.mNotInGroup)
			{
				mGuiView.Inform(GetType(), 3, null);
			}
		}

		private void OnJoinFromGroup(JoinFromGroupArg _arg)
		{
			if (_arg.mInGroup)
			{
				mGuiView.Inform(GetType(), 4, null);
			}
		}

		private void OnGroupError(int _errorCode)
		{
			Log.Warning("OnGroupError : " + _errorCode);
			switch (_errorCode)
			{
			case 2016:
				mGuiView.Inform(GetType(), 5, null);
				break;
			case 2014:
				mGuiView.Inform(GetType(), 7, null);
				break;
			case 2028:
				mGuiView.Inform(GetType(), 8, null);
				break;
			default:
				mGuiView.Inform(GetType(), 9, null);
				break;
			}
		}

		private void OnOnline(CtrlOnlineArg _arg)
		{
			if (mMembers.TryGetValue(_arg.mUserId, out var value))
			{
				value.SetOnline();
				mChangedCallback(this);
			}
		}

		private void OnOffline(CtrlOfflineArg _arg)
		{
			if (mMembers.TryGetValue(_arg.mUserId, out var value))
			{
				value.SetOffline();
				mChangedCallback(this);
			}
		}

		public void Clean()
		{
			mMembers.Clear();
			mLeaderId = -1;
			Log.Debug("clean group");
		}

		public void UpdateList()
		{
			Clean();
			mLeaderId = mSelfUserData.UserId;
			mSender.GroupList();
		}

		public void Invite(int _userId)
		{
			if (!IsLeader)
			{
				Log.Warning("not a leader");
			}
			else
			{
				mSender.JoinFromGroupRequest(_userId, _isReferred: false);
			}
		}

		public void JoinRequest(int _userId)
		{
			mSender.JoinToGroupRequest(_userId);
		}

		public void Leave()
		{
			if (IsLeader)
			{
				mSender.LeaveAsLeader();
			}
			else
			{
				mSender.Leave();
			}
		}

		public void ChangeLeader(int _memberId)
		{
			if (!IsLeader)
			{
				Log.Warning("not a leader");
			}
			else if (!mMembers.ContainsKey(_memberId))
			{
				Log.Warning("player " + _memberId + " is not a group member");
			}
			else
			{
				mSender.ChangeLeader(_memberId);
			}
		}

		public void ChangeLeader()
		{
			ChangeLeader(-1);
		}

		public void Remove(int _memberId)
		{
			if (!IsLeader)
			{
				Log.Warning("not a leader");
			}
			else
			{
				mSender.Remove(_memberId);
			}
		}
	}
}
