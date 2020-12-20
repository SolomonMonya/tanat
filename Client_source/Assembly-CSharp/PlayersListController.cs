using System;
using System.Collections.Generic;
using Network;
using TanatKernel;

public class PlayersListController
{
	private enum Text
	{
		YOU_ARE_IN_IGNORE = 2028,
		TOO_MUTCH_IGNORE,
		TOO_MUTCH_FRIENDS,
		NOTHER_TOO_MUTCH_FRIENDS
	}

	public enum ListType
	{
		BOTH,
		FRIENDS,
		IGNORE,
		LOCAL_AREA
	}

	private CSStats mStatsWnd;

	private UsersListSender mSender;

	private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

	private IStoreContentProvider<Player> mPlayerProv;

	private OkDialog mOkDialog;

	private YesNoDialog mYesNoDialog;

	private int mSelfId;

	private DateTime mLastFriendAdded;

	private DateTime mLastFind;

	private List<ShortUserInfo> mCurrentPlayers = new List<ShortUserInfo>();

	public static List<ShortUserInfo> mFriends = new List<ShortUserInfo>();

	public static List<ShortUserInfo> mIgnore = new List<ShortUserInfo>();

	public bool mObserverEnabled;

	private bool mNeedUpdate;

	public int PlayersCount => (mPlayerProv != null) ? mPlayerProv.Count : 0;

	public static bool IsFriend(int _userId)
	{
		return mFriends.Exists((ShortUserInfo _ui) => _ui.mId == _userId);
	}

	public static bool IsIgnore(int _userId)
	{
		return mIgnore.Exists((ShortUserInfo _ui) => _ui.mId == _userId);
	}

	public static bool IsFriend(string _userName)
	{
		return mFriends.Exists((ShortUserInfo _ui) => _ui.mName == _userName);
	}

	public static bool IsIgnore(string _userName)
	{
		return mIgnore.Exists((ShortUserInfo _ui) => _ui.mName == _userName);
	}

	public void Init(CSStats _gui, OkDialog _okDialog, YesNoDialog _yesNoDialog, IStoreContentProvider<Player> _playerProv, HandlerManager<CtrlPacket, Enum> _handlerMgr, UsersListSender _sender, int _selfId)
	{
		mStatsWnd = _gui;
		mHandlerMgr = _handlerMgr;
		mOkDialog = _okDialog;
		mYesNoDialog = _yesNoDialog;
		mPlayerProv = _playerProv;
		mSender = _sender;
		mSelfId = _selfId;
		mHandlerMgr.Subscribe<BwListArg>(CtrlCmdId.user.get_bw_list, null, CommonError, OnListsRecieved);
		mHandlerMgr.Subscribe<FindResultArg>(CtrlCmdId.user.find, null, CommonError, OnFindResultRecieved);
		mHandlerMgr.Subscribe<RemoveFromListArg>(CtrlCmdId.user.remove_from_list, null, CommonError, OnRemovedFromList);
		mHandlerMgr.Subscribe<AddToListMpdArg>(CtrlCmdId.user.add_to_list_mpd, null, CommonError, OnAddToListMpd);
		mHandlerMgr.Subscribe(CtrlCmdId.user.add_to_list, null, CommonError);
		mHandlerMgr.Subscribe<FriendRequestMpdArg>(CtrlCmdId.user.friend_request_mpd, null, CommonError, OnFriendRequestMpd);
		mHandlerMgr.Subscribe(CtrlCmdId.user.friend_answer, null, CommonError);
		CSStats cSStats = mStatsWnd;
		cSStats.mAddToFriendList = (CSStats.AddToList)Delegate.Combine(cSStats.mAddToFriendList, new CSStats.AddToList(AddToFriend));
		CSStats cSStats2 = mStatsWnd;
		cSStats2.mAddToIgnoreList = (CSStats.AddToListWithName)Delegate.Combine(cSStats2.mAddToIgnoreList, new CSStats.AddToListWithName(AddToIgnore));
		CSStats cSStats3 = mStatsWnd;
		cSStats3.mSearch = (CSStats.Search)Delegate.Combine(cSStats3.mSearch, new CSStats.Search(Find));
		CSStats cSStats4 = mStatsWnd;
		cSStats4.mGetList = (CSStats.GetList)Delegate.Combine(cSStats4.mGetList, new CSStats.GetList(UpdateList));
		CSStats cSStats5 = mStatsWnd;
		cSStats5.mOnPlayerClicked = (CSStats.OnPlayerClicked)Delegate.Combine(cSStats5.mOnPlayerClicked, new CSStats.OnPlayerClicked(GetObserverInfo));
		CSStats cSStats6 = mStatsWnd;
		cSStats6.mUpdateCS = (CSStats.UpdateCS)Delegate.Combine(cSStats6.mUpdateCS, new CSStats.UpdateCS(UpdateCurrentPlayers));
		UpdateList(ListType.BOTH);
	}

	public void Unint()
	{
		CSStats cSStats = mStatsWnd;
		cSStats.mAddToFriendList = (CSStats.AddToList)Delegate.Remove(cSStats.mAddToFriendList, new CSStats.AddToList(AddToFriend));
		CSStats cSStats2 = mStatsWnd;
		cSStats2.mAddToIgnoreList = (CSStats.AddToListWithName)Delegate.Remove(cSStats2.mAddToIgnoreList, new CSStats.AddToListWithName(AddToIgnore));
		CSStats cSStats3 = mStatsWnd;
		cSStats3.mSearch = (CSStats.Search)Delegate.Remove(cSStats3.mSearch, new CSStats.Search(Find));
		CSStats cSStats4 = mStatsWnd;
		cSStats4.mGetList = (CSStats.GetList)Delegate.Remove(cSStats4.mGetList, new CSStats.GetList(UpdateList));
		CSStats cSStats5 = mStatsWnd;
		cSStats5.mOnPlayerClicked = (CSStats.OnPlayerClicked)Delegate.Remove(cSStats5.mOnPlayerClicked, new CSStats.OnPlayerClicked(GetObserverInfo));
		CSStats cSStats6 = mStatsWnd;
		cSStats6.mUpdateCS = (CSStats.UpdateCS)Delegate.Remove(cSStats6.mUpdateCS, new CSStats.UpdateCS(UpdateCurrentPlayers));
		if (mHandlerMgr != null)
		{
			mHandlerMgr.Unsubscribe(this);
			mHandlerMgr = null;
		}
		mPlayerProv = null;
	}

	private void UpdateCurrentPlayers(int _count)
	{
		if (PlayersCount != _count || mNeedUpdate)
		{
			mNeedUpdate = false;
			ReinitPlayers();
		}
	}

	public void ReinitPlayers()
	{
		mCurrentPlayers = new List<ShortUserInfo>();
		foreach (Player item in mPlayerProv.Content)
		{
			ShortUserInfo shortUserInfo = new ShortUserInfo();
			Player player = mPlayerProv.Get(item.Id);
			shortUserInfo.mId = player.Id;
			shortUserInfo.mLevel = player.Hero.GameInfo.mLevel;
			if (shortUserInfo.mLevel < 0)
			{
				mNeedUpdate = true;
			}
			shortUserInfo.mName = player.Name;
			shortUserInfo.mOnline = ShortUserInfo.Status.CS;
			shortUserInfo.mRating = player.Hero.GameInfo.mRating;
			if (shortUserInfo.mRating < 0)
			{
				mNeedUpdate = true;
			}
			shortUserInfo.mTag = player.Hero.GameInfo.mClanTag;
			shortUserInfo.mClanId = player.Hero.GameInfo.mClanId;
			if (shortUserInfo.mLevel >= 0 && shortUserInfo.mRating >= 0)
			{
				mCurrentPlayers.Add(shortUserInfo);
			}
		}
		mStatsWnd.SetData(mCurrentPlayers);
	}

	private void OnListsRecieved(BwListArg _arg)
	{
		if (_arg.mIgnore != null)
		{
			mIgnore = _arg.mIgnore;
		}
		if (_arg.mFriends != null)
		{
			mFriends = _arg.mFriends;
		}
		mStatsWnd.SetData(_arg.mFriends, _arg.mIgnore);
	}

	private void OnFindResultRecieved(FindResultArg _arg)
	{
		if (_arg.mResult.Count == 0)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_LIST_NO_RESULT");
			mOkDialog.SetData(localeText);
		}
		mStatsWnd.SetFindResult(_arg.mResult);
	}

	private void OnRemovedFromList(RemoveFromListArg _arg)
	{
		mSender.UpdateList((int)_arg.mType);
	}

	private void OnAddToListMpd(AddToListMpdArg _arg)
	{
		mSender.UpdateList((int)_arg.mType);
		if (_arg.mType == TanatKernel.ListType.FRIEND)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_LIST_ANSWER_AGREE");
			if (!_arg.mAnswer)
			{
				localeText = GuiSystem.GetLocaleText("GUI_LIST_ANSWER_DECLINE");
			}
			localeText = localeText.Replace("{NAME}", _arg.mName);
			mOkDialog.SetData(localeText);
		}
	}

	private void OnFriendRequestMpd(FriendRequestMpdArg _arg)
	{
		string localeText = GuiSystem.GetLocaleText("GUI_LIST_FRIEND_QUESTION");
		localeText = localeText.Replace("{NAME}", _arg.mNick);
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			mSender.AnswerForFriendRequest(_arg.mUserId, _yes);
			if (_yes)
			{
				mSender.UpdateList(1);
			}
		};
		mYesNoDialog.SetData(localeText, "Ok_Button_Name", "Cancel_Button_Name", callback);
	}

	private void CommonError(int _errorCode)
	{
		string id = "GUI_LIST_" + (Text)_errorCode;
		mOkDialog.SetData(GuiSystem.GetLocaleText(id));
	}

	private void ShowText(Text _text)
	{
		string id = "GUI_LIST_" + _text;
		mOkDialog.SetData(GuiSystem.GetLocaleText(id));
	}

	private void Find(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return;
		}
		if ((DateTime.Now - mLastFind).TotalSeconds < 5.0)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_LIST_SEARCH_OFTEN");
			mOkDialog.SetData(localeText);
			return;
		}
		string empty = string.Empty;
		string text = string.Empty;
		int num = _name.IndexOf('[');
		int num2 = _name.IndexOf(']');
		if (num == 0 && num2 > 0)
		{
			empty = _name.Substring(1, num2 - num - 1).Trim();
			if (_name.Length - num2 > 1)
			{
				text = _name.Substring(num2 + 1).Trim();
			}
			if ((empty + text).Length < 2)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_LIST_SEARCH_SHORT"));
				return;
			}
			mSender.Find(empty, text);
			mLastFind = DateTime.Now;
		}
		else if ((empty + _name.Trim()).Length < 2)
		{
			mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_LIST_SEARCH_SHORT"));
		}
		else
		{
			mSender.Find(empty, _name.Trim());
			mLastFind = DateTime.Now;
		}
	}

	private void UpdateList(ListType _type)
	{
		if (_type == ListType.LOCAL_AREA)
		{
			ReinitPlayers();
		}
		else
		{
			mSender.UpdateList((int)_type);
		}
	}

	public void AddOnlineToFriend(int _userId)
	{
		ShortUserInfo shortUserInfo = new ShortUserInfo();
		shortUserInfo.mId = _userId;
		shortUserInfo.mOnline = ShortUserInfo.Status.CS;
		shortUserInfo.mName = string.Empty;
		AddToFriend(shortUserInfo);
	}

	public void AddToFriend(ShortUserInfo _user)
	{
		if (mSelfId == _user.mId)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_LIST_NO_SELF_FRIEND");
			mOkDialog.SetData(localeText);
			return;
		}
		if (IsFriend(_user.mId))
		{
			string localeText2 = GuiSystem.GetLocaleText("GUI_LIST_ALREADY_FRIEND");
			mOkDialog.SetData(localeText2);
			return;
		}
		if (mFriends.Count >= 36)
		{
			string localeText3 = GuiSystem.GetLocaleText("GUI_LIST_TOO_MUTCH_FRIENDS");
			mOkDialog.SetData(localeText3);
			return;
		}
		if ((DateTime.Now - mLastFriendAdded).TotalSeconds < 5.0)
		{
			string localeText4 = GuiSystem.GetLocaleText("GUI_LIST_FRIEND_OFTEN");
			mOkDialog.SetData(localeText4);
			return;
		}
		mLastFriendAdded = DateTime.Now;
		if (_user.mOnline != ShortUserInfo.Status.CS)
		{
			string localeText5 = GuiSystem.GetLocaleText("GUI_LIST_FRIEND_REQUEST_SEND");
			localeText5 = localeText5.Replace("{NAME}", _user.mName);
			mOkDialog.SetData(localeText5);
		}
		if (IsIgnore(_user.mId))
		{
			string localeText6 = GuiSystem.GetLocaleText("GUI_LIST_FRIEND_IN_IGNORE");
			localeText6 = localeText6.Replace("{NAME}", _user.mName);
			YesNoDialog.OnAnswer callback = delegate(bool _yes)
			{
				if (_yes)
				{
					mSender.RemoveFromList(_user.mId, 2);
					mSender.AddToList(_user.mId, 1);
				}
			};
			mYesNoDialog.SetData(localeText6, "Ok_Button_Name", "Cancel_Button_Name", callback);
		}
		else
		{
			mSender.AddToList(_user.mId, 1);
		}
	}

	public void AddToIgnore(int _userId, string _name)
	{
		if (mSelfId == _userId)
		{
			string localeText = GuiSystem.GetLocaleText("GUI_LIST_NO_SELF_IGNORE");
			mOkDialog.SetData(localeText);
			return;
		}
		if (IsIgnore(_userId))
		{
			string localeText2 = GuiSystem.GetLocaleText("GUI_LIST_ALREADY_IGNORE");
			mOkDialog.SetData(localeText2);
			return;
		}
		if (mIgnore.Count >= 36)
		{
			string localeText3 = GuiSystem.GetLocaleText("GUI_LIST_TOO_MUTCH_IGNORE");
			mOkDialog.SetData(localeText3);
			return;
		}
		string localeText4 = GuiSystem.GetLocaleText("GUI_LIST_ADD_IGNORE_QUESTION");
		bool isFriend = IsFriend(_userId);
		if (isFriend)
		{
			localeText4 = GuiSystem.GetLocaleText("GUI_LIST_ADD_IGNORE_IN_FRIEND");
		}
		localeText4 = localeText4.Replace("{NAME}", _name);
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			if (_yes)
			{
				if (isFriend)
				{
					mSender.RemoveFromList(_userId, 1);
				}
				mSender.AddToList(_userId, 2);
			}
		};
		mYesNoDialog.SetData(localeText4, "Ok_Button_Name", "Cancel_Button_Name", callback);
	}

	public void RemoveFromFriends(int _userId)
	{
		mSender.RemoveFromList(_userId, 1);
	}

	public void RemoveFromIgnore(int _userId)
	{
		mSender.RemoveFromList(_userId, 2);
	}

	public void GetObserverInfo(int _userId)
	{
		if (mObserverEnabled)
		{
			mSender.GetObserverInfo(_userId);
		}
	}
}
