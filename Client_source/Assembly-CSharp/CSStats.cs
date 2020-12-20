using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CSStats : GuiElement
{
	private enum StatsTab
	{
		NONE,
		PLAYERS,
		SEARCH
	}

	private enum StatsPlayersTab
	{
		NONE,
		FRIENDS,
		CS,
		IGNORIES
	}

	private enum SortType
	{
		NONE,
		ONLINE,
		USERNAME,
		LEVEL,
		RAITING
	}

	private class StatsPlayerComparer : IComparer<StatsPlayer>
	{
		private SortType mSortType;

		public StatsPlayerComparer(SortType _type)
		{
			mSortType = _type;
		}

		public int Compare(StatsPlayer _obj1, StatsPlayer _obj2)
		{
			StatsPlayer statsPlayer = ((!mInverseSort) ? _obj1 : _obj2);
			StatsPlayer statsPlayer2 = ((!mInverseSort) ? _obj2 : _obj1);
			int num = 0;
			if (mSortType == SortType.ONLINE)
			{
				num = statsPlayer2.UserData.mOnline.CompareTo(statsPlayer.UserData.mOnline);
			}
			else if (mSortType == SortType.USERNAME)
			{
				num = statsPlayer.UserData.mName.CompareTo(statsPlayer2.UserData.mName);
			}
			else if (mSortType == SortType.LEVEL)
			{
				num = statsPlayer2.UserData.mLevel.CompareTo(statsPlayer.UserData.mLevel);
			}
			else if (mSortType == SortType.RAITING)
			{
				num = statsPlayer2.UserData.mRating.CompareTo(statsPlayer.UserData.mRating);
			}
			if (num == 0)
			{
				return statsPlayer.UserData.mName.CompareTo(statsPlayer2.UserData.mName);
			}
			return num;
		}
	}

	private class StatsPlayer : GuiElement
	{
		public delegate void ShowPopUpMenuCallback(object _palyerId, Vector2 _pos);

		public delegate void OnPlayerSelect(StatsPlayer _player);

		private ShortUserInfo mUserData;

		private GuiButton mButton;

		private GuiButton mNickButton;

		private Texture2D mStatusImg;

		private string mLevel;

		private string mRaiting;

		private Rect mStatusImgRect;

		private Rect mLevelRect;

		private Rect mRaitingRect;

		private float mStartPopupTime;

		private float mMaxPopupTime;

		private PopupInfo mPopupInfo;

		public OnPlayerSelect mOnPlayerSelect;

		public OnPlayerSelect mOnPlayerDoubleClicked;

		public ShowPopUpMenuCallback mOnPlayerPopUp;

		public ShortUserInfo UserData => mUserData;

		public override void Init()
		{
			if (mUserData != null)
			{
				mMaxPopupTime = 0.6f;
				string text = "Gui/CSStats/";
				if (mUserData.mOnline == ShortUserInfo.Status.OFFLINE)
				{
					text += "status_offline";
				}
				if (mUserData.mOnline == ShortUserInfo.Status.CS)
				{
					text += "status_cs";
				}
				if (mUserData.mOnline == ShortUserInfo.Status.BATTLE)
				{
					text += "status_battle";
				}
				if (mPopupInfo == null)
				{
					mPopupInfo = GuiSystem.mGuiSystem.GetGuiElement<PopupInfo>("central_square", "POPUP_INFO");
				}
				mStatusImg = GuiSystem.GetImage(text);
				mLevel = mUserData.mLevel.ToString();
				mRaiting = mUserData.mRating.ToString();
				mButton = new GuiButton();
				mButton.mOverImg = GuiSystem.GetImage("Gui/misc/selection");
				mButton.mPressImg = GuiSystem.GetImage("Gui/misc/selection");
				mButton.mId = mUserData.mId;
				mButton.mElementId = "STATS_PLAYER_BUTTON";
				GuiButton guiButton = mButton;
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				GuiButton guiButton2 = mButton;
				guiButton2.mOnDoubleClick = (OnDoubleClick)Delegate.Combine(guiButton2.mOnDoubleClick, new OnDoubleClick(OnPlayerDoubleClick));
				mButton.Init();
				mNickButton = new GuiButton();
				mNickButton.mElementId = "NICK_BUTTON";
				if (string.IsNullOrEmpty(mUserData.mTag))
				{
					mNickButton.mLabel = mUserData.mName;
				}
				else
				{
					mNickButton.mLabel = "[" + mUserData.mTag + "]" + mUserData.mName;
				}
				mNickButton.mLabelStyle = "nick";
				GuiButton guiButton3 = mNickButton;
				guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
				GuiButton guiButton4 = mNickButton;
				guiButton4.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton4.mOnMouseEnter, new OnMouseEnter(OnShowNick));
				GuiButton guiButton5 = mNickButton;
				guiButton5.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton5.mOnMouseLeave, new OnMouseLeave(OnHideNick));
				mNickButton.Init();
			}
		}

		public override void SetSize()
		{
			mButton.mZoneRect = mZoneRect;
			mStatusImgRect = new Rect(20f, 9f, 7f, 7f);
			mNickButton.mZoneRect = new Rect(36f, 2f, 85f, 18f);
			mLevelRect = new Rect(128f, 5f, 40f, 15f);
			mRaitingRect = new Rect(169f, 5f, 73f, 15f);
			GuiSystem.SetChildRect(mZoneRect, ref mStatusImgRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNickButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingRect);
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mUserData != null)
			{
				mNickButton.CheckEvent(_curEvent);
				mButton.CheckEvent(_curEvent);
				if (mStartPopupTime != 0f && Time.time - mStartPopupTime > mMaxPopupTime)
				{
					mStartPopupTime = 0f;
					mPopupInfo.ShowInfo(mNickButton.mLabel, new Vector2(mNickButton.mZoneRect.x, mNickButton.mZoneRect.y));
				}
			}
		}

		public override void RenderElement()
		{
			if (mUserData != null)
			{
				mButton.RenderElement();
				GuiSystem.DrawImage(mStatusImg, mStatusImgRect);
				mNickButton.RenderElement();
				GuiSystem.DrawString(mLevel, mLevelRect, "middle_left");
				GuiSystem.DrawString(mRaiting, mRaitingRect, "middle_left");
			}
		}

		public void SetData(ShortUserInfo _data)
		{
			mUserData = _data;
		}

		public void SetSelection(bool _selected)
		{
			if (mButton != null)
			{
				mButton.Pressed = _selected;
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if ((_sender.mElementId == "STATS_PLAYER_BUTTON" || _sender.mElementId == "NICK_BUTTON") && _buttonId == 0 && mOnPlayerSelect != null)
			{
				mOnPlayerSelect(this);
			}
			if ((_sender.mElementId == "STATS_PLAYER_BUTTON" || _sender.mElementId == "NICK_BUTTON") && _buttonId == 1 && mOnPlayerPopUp != null)
			{
				mOnPlayerPopUp(mUserData, new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y));
			}
		}

		private void OnPlayerDoubleClick(GuiElement _sender)
		{
			if (_sender.mElementId == "STATS_PLAYER_BUTTON" && mOnPlayerDoubleClicked != null)
			{
				mOnPlayerDoubleClicked(this);
			}
		}

		private void OnShowNick(GuiElement _sender)
		{
			if (mNickButton != null)
			{
				mStartPopupTime = Time.time;
			}
		}

		private void OnHideNick(GuiElement _sender)
		{
			mStartPopupTime = 0f;
			mPopupInfo.Hide();
		}
	}

	public delegate void ShowPopUpMenuCallback(object _palyerName, Vector2 _pos);

	public delegate void MenuCallback();

	public delegate void AddToList(ShortUserInfo _id);

	public delegate void AddToListWithName(int _id, string _name);

	public delegate void Search(string _id);

	public delegate void GetList(PlayersListController.ListType _type);

	public delegate void UpdateCS(int _playersCount);

	public delegate void OnPlayerClicked(int _id);

	public ShowPopUpMenuCallback mShowPopUpMenuCallback;

	public MenuCallback mMenuCallback;

	public AddToList mAddToFriendList;

	public AddToListWithName mAddToIgnoreList;

	public Search mSearch;

	public GetList mGetList;

	public UpdateCS mUpdateCS;

	public OnPlayerClicked mOnPlayerClicked;

	public Action<string> mShowClanInfoCallback;

	private int mMaxPlayersOnSearchPage = 6;

	private int mMaxPlayersOnPlayersPage = 9;

	private int mCurMaxPlayersOnPage;

	private int mCurPageNum;

	private int mMaxPageNum;

	private int mUpdatesCount;

	private GuiButton mScrollButtonL;

	private GuiButton mScrollButtonR;

	private Rect mPageRect;

	private StatsPlayer mSelectedPlayer;

	private SortType mSortType;

	private static bool mInverseSort;

	private GuiButton mPlayerListButton;

	private GuiButton mMenuButton;

	private GuiButton mTabPlayersButton;

	private GuiButton mTabSearchButton;

	private Texture2D mTabPlayersFrame;

	private Texture2D mTabSearchFrame;

	private Rect mTabPlayersFrameRect;

	private Rect mTabSearchFrameRect;

	private Texture2D mZoneFrame;

	private Texture2D mBgFrame;

	private Rect mBgRect;

	private bool mHided;

	private StatsTab mCurTab;

	private Texture2D mStatusImg;

	private Rect mStatusImgRect;

	private Texture2D mSearchFrame;

	private StaticTextField mSearchField;

	private GuiButton mSearchButton;

	private GuiButton mToFriendsButton;

	private GuiButton mToIgnoreButton;

	private Rect mSearchFrameRect;

	private List<GuiButton> mSortTypeButtons;

	private StatsPlayersTab mStatsPlayersTab;

	private Texture2D mPlayersFrame;

	private GuiButton mFrinedsButton;

	private GuiButton mIgnoresButton;

	private GuiButton mPlayersButton;

	private Rect mPlayersFrameRect;

	private Texture2D mTabPlayersFrame1;

	private Texture2D mTabPlayersFrame2;

	private Texture2D mTabPlayersFrame3;

	private Rect mTabPlayersFrameRect1;

	private Rect mTabPlayersFrameRect2;

	private Rect mTabPlayersFrameRect3;

	private List<ShortUserInfo> mFriendsData;

	private List<ShortUserInfo> mIgnoreData;

	private List<ShortUserInfo> mCSData;

	private List<ShortUserInfo> mSearchData;

	private List<StatsPlayer> mCurPlayers;

	public override void Update()
	{
		if (mUpdatesCount > 100 && mUpdateCS != null && mStatsPlayersTab == StatsPlayersTab.CS && mCurTab == StatsTab.PLAYERS)
		{
			if (mCSData != null)
			{
				mUpdateCS(mCSData.Count);
			}
			else
			{
				mUpdateCS(0);
			}
			mUpdatesCount = 0;
		}
		mUpdatesCount++;
	}

	public override void Init()
	{
		mZoneFrame = GuiSystem.GetImage("Gui/CSStats/frame1");
		mBgFrame = GuiSystem.GetImage("Gui/CSStats/frame2");
		mSearchFrame = GuiSystem.GetImage("Gui/CSStats/frame3");
		mPlayersFrame = GuiSystem.GetImage("Gui/CSStats/frame4");
		mTabPlayersFrame = GuiSystem.GetImage("Gui/CSStats/tab_left");
		mTabSearchFrame = GuiSystem.GetImage("Gui/CSStats/tab_right");
		mTabPlayersFrame1 = GuiSystem.GetImage("Gui/CSStats/tab_players1");
		mTabPlayersFrame2 = GuiSystem.GetImage("Gui/CSStats/tab_players2");
		mTabPlayersFrame3 = GuiSystem.GetImage("Gui/CSStats/tab_players3");
		mStatusImg = GuiSystem.GetImage("Gui/CSStats/status_cs");
		mCurPlayers = new List<StatsPlayer>();
		mSortTypeButtons = new List<GuiButton>();
		mScrollButtonL = GuiSystem.CreateButton("Gui/misc/button_15_norm", "Gui/misc/button_15_over", "Gui/misc/button_15_press", string.Empty, string.Empty);
		mScrollButtonL.mElementId = "SCROLL_LEFT";
		GuiButton guiButton = mScrollButtonL;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollButtonL.Init();
		AddTutorialElement(mScrollButtonL);
		mScrollButtonR = GuiSystem.CreateButton("Gui/misc/button_16_norm", "Gui/misc/button_16_over", "Gui/misc/button_16_press", string.Empty, string.Empty);
		mScrollButtonR.mElementId = "SCROLL_RIGHT";
		GuiButton guiButton2 = mScrollButtonR;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollButtonR.Init();
		AddTutorialElement(mScrollButtonR);
		mPlayerListButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mPlayerListButton.mElementId = "PLAYER_LIST_BUTTON";
		GuiButton guiButton3 = mPlayerListButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mPlayerListButton.mLabel = GuiSystem.GetLocaleText("Player_List_Button_Name");
		mPlayerListButton.Init();
		AddTutorialElement(mPlayerListButton);
		mMenuButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mMenuButton.mElementId = "MENU_BUTTON";
		GuiButton guiButton4 = mMenuButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mMenuButton.mLabel = GuiSystem.GetLocaleText("Menu_Button_Name");
		mMenuButton.Init();
		AddTutorialElement(mMenuButton);
		mTabPlayersButton = GuiSystem.CreateButton("Gui/CSStats/button_1_norm", "Gui/CSStats/button_1_over", "Gui/CSStats/button_1_press", string.Empty, string.Empty);
		mTabPlayersButton.mId = 1;
		mTabPlayersButton.mElementId = "TAB_BUTTON";
		GuiButton guiButton5 = mTabPlayersButton;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mTabPlayersButton.mLabel = GuiSystem.GetLocaleText("CSStats_Players_Text");
		mTabPlayersButton.Init();
		mTabSearchButton = GuiSystem.CreateButton("Gui/CSStats/button_1_norm", "Gui/CSStats/button_1_over", "Gui/CSStats/button_1_press", string.Empty, string.Empty);
		mTabSearchButton.mId = 2;
		mTabSearchButton.mElementId = "TAB_BUTTON";
		GuiButton guiButton6 = mTabSearchButton;
		guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
		mTabSearchButton.mLabel = GuiSystem.GetLocaleText("CSStats_Searchnig_Text");
		mTabSearchButton.Init();
		mFrinedsButton = GuiSystem.CreateButton("Gui/CSStats/button_3_norm", "Gui/CSStats/button_3_over", "Gui/CSStats/button_3_press", string.Empty, string.Empty);
		mFrinedsButton.mId = 1;
		mFrinedsButton.mElementId = "TAB_PLAYERS_BUTTON";
		GuiButton guiButton7 = mFrinedsButton;
		guiButton7.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton7.mOnMouseUp, new OnMouseUp(OnButton));
		mFrinedsButton.mLabel = GuiSystem.GetLocaleText("CSStats_Tab_Friends_Text");
		mFrinedsButton.Init();
		mPlayersButton = GuiSystem.CreateButton("Gui/CSStats/button_2_norm", "Gui/CSStats/button_2_over", "Gui/CSStats/button_2_press", string.Empty, string.Empty);
		mPlayersButton.mId = 2;
		mPlayersButton.mElementId = "TAB_PLAYERS_BUTTON";
		GuiButton guiButton8 = mPlayersButton;
		guiButton8.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton8.mOnMouseUp, new OnMouseUp(OnButton));
		mPlayersButton.mLabel = GuiSystem.GetLocaleText("CSStats_Tab_CS_Text");
		mPlayersButton.Init();
		mIgnoresButton = GuiSystem.CreateButton("Gui/CSStats/button_3_norm", "Gui/CSStats/button_3_over", "Gui/CSStats/button_3_press", string.Empty, string.Empty);
		mIgnoresButton.mId = 3;
		mIgnoresButton.mElementId = "TAB_PLAYERS_BUTTON";
		GuiButton guiButton9 = mIgnoresButton;
		guiButton9.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton9.mOnMouseUp, new OnMouseUp(OnButton));
		mIgnoresButton.mLabel = GuiSystem.GetLocaleText("CSStats_Tab_Ignores_Text");
		mIgnoresButton.Init();
		mSearchButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mSearchButton.mElementId = "SEARCH_BUTTON";
		GuiButton guiButton10 = mSearchButton;
		guiButton10.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton10.mOnMouseUp, new OnMouseUp(OnButton));
		mSearchButton.mLabel = GuiSystem.GetLocaleText("CSStats_Search_Text");
		mSearchButton.Init();
		mToFriendsButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mToFriendsButton.mElementId = "TO_FRIENDS_BUTTON";
		GuiButton guiButton11 = mToFriendsButton;
		guiButton11.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton11.mOnMouseUp, new OnMouseUp(OnButton));
		mToFriendsButton.mLabel = GuiSystem.GetLocaleText("CSStats_To_Friends_Text");
		mToFriendsButton.Init();
		mToIgnoreButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mToIgnoreButton.mElementId = "TO_IGNORE_BUTTON";
		GuiButton guiButton12 = mToIgnoreButton;
		guiButton12.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton12.mOnMouseUp, new OnMouseUp(OnButton));
		mToIgnoreButton.mLabel = GuiSystem.GetLocaleText("CSStats_To_Ignore_Text");
		mToIgnoreButton.Init();
		mSearchField = new StaticTextField();
		mSearchField.mElementId = "SEARCH_TEXT_FIELD";
		mSearchField.mLength = 32;
		mSearchField.mStyleId = "text_field_1";
		InitSortTypeButtons();
		SetSortType(SortType.USERNAME);
		SetCurStatsPlayerTab(StatsPlayersTab.CS);
		SetCurTab(StatsTab.PLAYERS);
	}

	public override void Uninit()
	{
		if (mSearchField != null)
		{
			mSearchField.Uninit();
			mSearchField.mData = string.Empty;
		}
		if (mCurPlayers != null)
		{
			foreach (StatsPlayer mCurPlayer in mCurPlayers)
			{
				mCurPlayer.mOnPlayerSelect = (StatsPlayer.OnPlayerSelect)Delegate.Remove(mCurPlayer.mOnPlayerSelect, new StatsPlayer.OnPlayerSelect(OnPlayerSelect));
				mCurPlayer.mOnPlayerDoubleClicked = (StatsPlayer.OnPlayerSelect)Delegate.Remove(mCurPlayer.mOnPlayerDoubleClicked, new StatsPlayer.OnPlayerSelect(OnPlayerDoubleClicked));
				mCurPlayer.mOnPlayerPopUp = (StatsPlayer.ShowPopUpMenuCallback)Delegate.Remove(mCurPlayer.mOnPlayerPopUp, new StatsPlayer.ShowPopUpMenuCallback(OnPlayerPopUp));
			}
			mCurPlayers.Clear();
		}
		mCSData = null;
		mIgnoreData = null;
		mSearchData = null;
		mFriendsData = null;
		mInverseSort = false;
		SetSortType(SortType.USERNAME);
		SetCurStatsPlayerTab(StatsPlayersTab.CS);
		SetCurTab(StatsTab.PLAYERS);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mZoneFrame.width, mZoneFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width;
		mBgRect = new Rect(0f, 50f, mBgFrame.width, mBgFrame.height);
		GuiSystem.SetChildRect(mZoneRect, ref mBgRect);
		mSearchFrameRect = new Rect(5f, 84f, mSearchFrame.width, mSearchFrame.height);
		GuiSystem.SetChildRect(mBgRect, ref mSearchFrameRect);
		mPlayersFrameRect = new Rect(5f, 4f, mPlayersFrame.width, mPlayersFrame.height);
		GuiSystem.SetChildRect(mBgRect, ref mPlayersFrameRect);
		mPlayerListButton.mZoneRect = new Rect(16f, 16f, 132f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayerListButton.mZoneRect);
		mMenuButton.mZoneRect = new Rect(165f, 16f, 84f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mMenuButton.mZoneRect);
		mTabPlayersButton.mZoneRect = new Rect(6f, 361f, 124f, 30f);
		GuiSystem.SetChildRect(mBgRect, ref mTabPlayersButton.mZoneRect);
		mTabSearchButton.mZoneRect = new Rect(134f, 361f, 124f, 30f);
		GuiSystem.SetChildRect(mBgRect, ref mTabSearchButton.mZoneRect);
		mSearchField.mZoneRect = new Rect(13f, 14f, 238f, 24f);
		GuiSystem.SetChildRect(mBgRect, ref mSearchField.mZoneRect);
		mSearchButton.mZoneRect = new Rect(94f, 48f, 79f, 28f);
		GuiSystem.SetChildRect(mBgRect, ref mSearchButton.mZoneRect);
		mToFriendsButton.mZoneRect = new Rect(12f, 317f, 79f, 28f);
		GuiSystem.SetChildRect(mBgRect, ref mToFriendsButton.mZoneRect);
		mToIgnoreButton.mZoneRect = new Rect(172f, 317f, 79f, 28f);
		GuiSystem.SetChildRect(mBgRect, ref mToIgnoreButton.mZoneRect);
		mFrinedsButton.mZoneRect = new Rect(4f, 328f, 74f, 20f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mFrinedsButton.mZoneRect);
		mPlayersButton.mZoneRect = new Rect(85f, 328f, 83f, 20f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mPlayersButton.mZoneRect);
		mIgnoresButton.mZoneRect = new Rect(175f, 328f, 74f, 20f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mIgnoresButton.mZoneRect);
		mPageRect = new Rect(111f, 291f, 41f, 16f);
		GuiSystem.SetChildRect(mBgRect, ref mPageRect);
		mScrollButtonL.mZoneRect = new Rect(97f, 292f, 15f, 15f);
		GuiSystem.SetChildRect(mBgRect, ref mScrollButtonL.mZoneRect);
		mScrollButtonR.mZoneRect = new Rect(152f, 292f, 15f, 15f);
		GuiSystem.SetChildRect(mBgRect, ref mScrollButtonR.mZoneRect);
		mTabPlayersFrameRect = new Rect(0f, 354f, 137f, 48f);
		GuiSystem.SetChildRect(mBgRect, ref mTabPlayersFrameRect);
		mTabSearchFrameRect = new Rect(126f, 354f, 137f, 48f);
		GuiSystem.SetChildRect(mBgRect, ref mTabSearchFrameRect);
		mTabPlayersFrameRect1 = new Rect(81f, 324f, 91f, 28f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mTabPlayersFrameRect1);
		mTabPlayersFrameRect2 = new Rect(0f, 324f, 82f, 28f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mTabPlayersFrameRect2);
		mTabPlayersFrameRect3 = new Rect(171f, 324f, 82f, 28f);
		GuiSystem.SetChildRect(mPlayersFrameRect, ref mTabPlayersFrameRect3);
		PopupInfo.AddTip(this, "TIP_TEXT92", mFrinedsButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT91", mPlayersButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT93", mIgnoresButton.mZoneRect);
		SetCurPlayersSize();
		SetSortTypeButtonsSize();
		PopupInfo.AddTip(this, "TIP_TEXT25", mMenuButton.mZoneRect);
	}

	public override void RenderElement()
	{
		if (!mHided)
		{
			mTabPlayersButton.RenderElement();
			mTabSearchButton.RenderElement();
			GuiSystem.DrawImage(mBgFrame, mBgRect);
			if (mCurTab == StatsTab.PLAYERS)
			{
				RenderPlayersTab();
			}
			else if (mCurTab == StatsTab.SEARCH)
			{
				RenderSearchTab();
			}
			foreach (GuiButton mSortTypeButton in mSortTypeButtons)
			{
				mSortTypeButton.RenderElement();
			}
			GuiSystem.DrawImage(mStatusImg, mStatusImgRect);
			int num = mCurPageNum * mCurMaxPlayersOnPage + mCurMaxPlayersOnPage;
			num = ((mCurPlayers.Count < num) ? mCurPlayers.Count : num);
			for (int i = mCurPageNum * mCurMaxPlayersOnPage; i < num; i++)
			{
				mCurPlayers[i].RenderElement();
			}
			GuiSystem.DrawString(mCurPageNum + 1 + "/" + (mMaxPageNum + 1), mPageRect, "middle_center");
			mScrollButtonL.RenderElement();
			mScrollButtonR.RenderElement();
		}
		GuiSystem.DrawImage(mZoneFrame, mZoneRect);
		mMenuButton.RenderElement();
		mPlayerListButton.RenderElement();
	}

	public override void OnInput()
	{
		if (!mHided && mCurTab == StatsTab.SEARCH)
		{
			mSearchField.OnInput();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mMenuButton.CheckEvent(_curEvent);
		mPlayerListButton.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.BackQuote && GuiSystem.InputIsFree())
		{
			SwitchPlayerList();
		}
		else if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Escape && GuiSystem.InputIsFree() && mMenuCallback != null)
		{
			mMenuCallback();
		}
		if (!mHided)
		{
			mTabPlayersButton.CheckEvent(_curEvent);
			mTabSearchButton.CheckEvent(_curEvent);
			mScrollButtonL.CheckEvent(_curEvent);
			mScrollButtonR.CheckEvent(_curEvent);
			if (mCurTab == StatsTab.PLAYERS)
			{
				CheckEventPlayersTab(_curEvent);
			}
			else if (mCurTab == StatsTab.SEARCH)
			{
				CheckEventSearchTab(_curEvent);
			}
			foreach (GuiButton mSortTypeButton in mSortTypeButtons)
			{
				mSortTypeButton.CheckEvent(_curEvent);
			}
			int num = mCurPageNum * mCurMaxPlayersOnPage + mCurMaxPlayersOnPage;
			num = ((mCurPlayers.Count < num) ? mCurPlayers.Count : num);
			for (int i = mCurPageNum * mCurMaxPlayersOnPage; i < num; i++)
			{
				mCurPlayers[i].CheckEvent(_curEvent);
			}
		}
		base.CheckEvent(_curEvent);
	}

	public void SetFindResult(List<ShortUserInfo> _data)
	{
		if (_data != null)
		{
			mSearchData = _data;
		}
		InitCurPlayers(mCurTab, mStatsPlayersTab);
	}

	public void SetData(List<ShortUserInfo> _friends, List<ShortUserInfo> _ignore)
	{
		if (_friends != null)
		{
			mFriendsData = _friends;
		}
		if (_ignore != null)
		{
			mIgnoreData = _ignore;
		}
		if (mCurTab == StatsTab.PLAYERS && (mStatsPlayersTab == StatsPlayersTab.FRIENDS || mStatsPlayersTab == StatsPlayersTab.IGNORIES))
		{
			SetCurTab(mCurTab);
		}
	}

	public void SetData(List<ShortUserInfo> _curUsers)
	{
		if (_curUsers != null)
		{
			mCSData = _curUsers;
			SetCurTab(mCurTab);
		}
	}

	private void InitSortTypeButtons()
	{
		GuiButton guiButton = null;
		foreach (int value in Enum.GetValues(typeof(SortType)))
		{
			if (value != 0)
			{
				guiButton = GuiSystem.CreateButton("Gui/CSStats/button_3_norm", "Gui/CSStats/button_3_over", "Gui/CSStats/button_3_press", string.Empty, string.Empty);
				guiButton.mElementId = "SORT_TYPE_BUTTON";
				guiButton.mId = value;
				GuiButton guiButton2 = guiButton;
				guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
				if (value != 1)
				{
					guiButton.mLabel = GuiSystem.GetLocaleText("CSStats_Sort_" + ((SortType)value).ToString() + "_Text");
				}
				guiButton.Init();
				mSortTypeButtons.Add(guiButton);
			}
		}
	}

	private void SetSortButtonIcon()
	{
		foreach (GuiButton mSortTypeButton in mSortTypeButtons)
		{
			if (mSortTypeButton.mId == (int)mSortType)
			{
				mSortTypeButton.mIconImg = ((!mInverseSort) ? GuiSystem.GetImage("Gui/CSStats/arrow_down") : GuiSystem.GetImage("Gui/CSStats/arrow_up"));
			}
			else
			{
				mSortTypeButton.mIconImg = null;
			}
		}
	}

	private void RenderPlayersTab()
	{
		GuiSystem.DrawImage(mTabPlayersFrame, mTabPlayersFrameRect);
		GuiSystem.DrawImage(mPlayersFrame, mPlayersFrameRect);
		mFrinedsButton.RenderElement();
		mPlayersButton.RenderElement();
		mIgnoresButton.RenderElement();
		if (mStatsPlayersTab == StatsPlayersTab.CS)
		{
			GuiSystem.DrawImage(mTabPlayersFrame1, mTabPlayersFrameRect1);
		}
		else if (mStatsPlayersTab == StatsPlayersTab.FRIENDS)
		{
			GuiSystem.DrawImage(mTabPlayersFrame2, mTabPlayersFrameRect2);
		}
		else if (mStatsPlayersTab == StatsPlayersTab.IGNORIES)
		{
			GuiSystem.DrawImage(mTabPlayersFrame3, mTabPlayersFrameRect3);
		}
	}

	private void RenderSearchTab()
	{
		GuiSystem.DrawImage(mTabSearchFrame, mTabSearchFrameRect);
		GuiSystem.DrawImage(mSearchFrame, mSearchFrameRect);
		mSearchButton.RenderElement();
		mToFriendsButton.RenderElement();
		mToIgnoreButton.RenderElement();
	}

	private void CheckEventPlayersTab(Event _curEvent)
	{
		mFrinedsButton.CheckEvent(_curEvent);
		mPlayersButton.CheckEvent(_curEvent);
		mIgnoresButton.CheckEvent(_curEvent);
	}

	private void CheckEventSearchTab(Event _curEvent)
	{
		mSearchField.CheckEvent(_curEvent);
		mSearchButton.CheckEvent(_curEvent);
		mToFriendsButton.CheckEvent(_curEvent);
		mToIgnoreButton.CheckEvent(_curEvent);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "PLAYER_LIST_BUTTON" && _buttonId == 0)
		{
			SwitchPlayerList();
		}
		else if (_sender.mElementId == "MENU_BUTTON" && _buttonId == 0)
		{
			if (mMenuCallback != null)
			{
				mMenuCallback();
			}
		}
		else if (_sender.mElementId == "TAB_BUTTON" && _buttonId == 0)
		{
			mCurPageNum = 0;
			SetCurTab((StatsTab)_sender.mId);
		}
		else if (_sender.mElementId == "SEARCH_BUTTON" && _buttonId == 0)
		{
			if (mSearch != null)
			{
				if (!string.IsNullOrEmpty(mSearchField.mData))
				{
					mSearch(mSearchField.mData);
				}
				else
				{
					InitCurPlayers(StatsTab.NONE, StatsPlayersTab.NONE);
				}
			}
		}
		else if (_sender.mElementId == "TO_FRIENDS_BUTTON" && _buttonId == 0)
		{
			AddToFriendList(mSelectedPlayer);
		}
		else if (_sender.mElementId == "TO_IGNORE_BUTTON" && _buttonId == 0)
		{
			AddToIgnoreList(mSelectedPlayer);
		}
		else if ((_sender.mElementId == "SCROLL_LEFT" || _sender.mElementId == "SCROLL_RIGHT") && _buttonId == 0)
		{
			IncPageNum("SCROLL_LEFT" == _sender.mElementId);
		}
		else if (_sender.mElementId == "SORT_TYPE_BUTTON" && _buttonId == 0)
		{
			SetSortType((SortType)_sender.mId);
		}
		else if (_sender.mElementId == "TAB_PLAYERS_BUTTON" && _buttonId == 0)
		{
			SetCurStatsPlayerTab((StatsPlayersTab)_sender.mId);
		}
	}

	private void SetPageButtonState()
	{
		mScrollButtonL.mLocked = mCurPageNum == 0;
		mScrollButtonR.mLocked = mCurPageNum == mMaxPageNum;
	}

	private void AddToFriendList(StatsPlayer _player)
	{
		if (_player != null && _player.UserData != null && mAddToFriendList != null)
		{
			mAddToFriendList(_player.UserData);
		}
	}

	private void AddToIgnoreList(StatsPlayer _player)
	{
		if (_player != null && _player.UserData != null && mAddToIgnoreList != null)
		{
			mAddToIgnoreList(_player.UserData.mId, _player.UserData.mName);
		}
	}

	private void OnPlayerSelect(StatsPlayer _player)
	{
		if (mSelectedPlayer != null && _player != mSelectedPlayer)
		{
			mSelectedPlayer.SetSelection(_selected: false);
		}
		mSelectedPlayer = _player;
		if (mSelectedPlayer != null)
		{
			mSelectedPlayer.SetSelection(_selected: true);
		}
		mToFriendsButton.mLocked = mSelectedPlayer == null;
		mToIgnoreButton.mLocked = mSelectedPlayer == null;
	}

	private void OnPlayerDoubleClicked(StatsPlayer _player)
	{
		if (mOnPlayerClicked != null)
		{
			mOnPlayerClicked(_player.UserData.mId);
		}
	}

	private void OnPlayerPopUp(object _playerId, Vector2 _pos)
	{
		if (mShowPopUpMenuCallback != null)
		{
			mShowPopUpMenuCallback(_playerId, _pos);
		}
	}

	private void SetCurPlayersSize()
	{
		SortPlayerList(mSortType);
		int num = 11;
		int num2 = 241;
		int num3 = 26;
		int num4 = 0;
		if (mCurTab == StatsTab.SEARCH)
		{
			num4 = 114;
		}
		else if (mCurTab == StatsTab.PLAYERS)
		{
			num4 = 33;
		}
		int num5 = 0;
		foreach (StatsPlayer mCurPlayer in mCurPlayers)
		{
			mCurPlayer.mZoneRect = new Rect(num, num4 + num5 * 26, num2, num3);
			GuiSystem.SetChildRect(mBgRect, ref mCurPlayer.mZoneRect);
			mCurPlayer.SetSize();
			num5++;
			if (num5 == mCurMaxPlayersOnPage)
			{
				num5 = 0;
			}
		}
	}

	private void SetSortTypeButtonsSize()
	{
		int num = 0;
		if (mCurTab == StatsTab.SEARCH)
		{
			num = 94;
		}
		else if (mCurTab == StatsTab.PLAYERS)
		{
			num = 12;
		}
		mStatusImgRect = new Rect(30f, num + 3, 7f, 7f);
		SortType sortType = SortType.NONE;
		foreach (GuiButton mSortTypeButton in mSortTypeButtons)
		{
			switch (mSortTypeButton.mId)
			{
			case 1:
				mSortTypeButton.mZoneRect = new Rect(14f, num, 28f, 14f);
				break;
			case 2:
				mSortTypeButton.mZoneRect = new Rect(43f, num, 91f, 14f);
				break;
			case 3:
				mSortTypeButton.mZoneRect = new Rect(135f, num, 40f, 14f);
				break;
			case 4:
				mSortTypeButton.mZoneRect = new Rect(176f, num, 74f, 14f);
				break;
			}
			mSortTypeButton.mIconRect = new Rect(3f, 2f, 7f, 9f);
			GuiSystem.SetChildRect(mBgRect, ref mSortTypeButton.mZoneRect);
			GuiSystem.SetChildRect(mSortTypeButton.mZoneRect, ref mSortTypeButton.mIconRect);
		}
		GuiSystem.SetChildRect(mBgRect, ref mStatusImgRect);
	}

	private void SortPlayerList(SortType _sortType)
	{
		if (mCurPlayers != null)
		{
			StatsPlayerComparer comparer = new StatsPlayerComparer(_sortType);
			mCurPlayers.Sort(comparer);
		}
	}

	public void ShowPlayerList()
	{
		mHided = true;
		SwitchPlayerList();
	}

	private void SwitchPlayerList()
	{
		mHided = !mHided;
		mPlayerListButton.Pressed = !mHided;
		string id = (mHided ? "CLOSE_LOG_TXT" : "OPEN_LOG_TXT");
		UserLog.AddAction(UserActionType.SWITCH_PLAYER_LIST, (!mHided) ? 1 : 0, GuiSystem.GetLocaleText(id));
	}

	private void SetCurTab(StatsTab _tab)
	{
		if (_tab != mCurTab)
		{
			mCurTab = _tab;
			if (mCurTab == StatsTab.PLAYERS)
			{
				mCurMaxPlayersOnPage = mMaxPlayersOnPlayersPage;
			}
			else if (mCurTab == StatsTab.SEARCH)
			{
				mCurMaxPlayersOnPage = mMaxPlayersOnSearchPage;
			}
			mTabPlayersButton.Pressed = mCurTab == StatsTab.PLAYERS;
			mTabSearchButton.Pressed = mCurTab == StatsTab.SEARCH;
			SetSortTypeButtonsSize();
			OnPlayerSelect(null);
		}
		InitCurPlayers(mCurTab, mStatsPlayersTab);
	}

	private void SetCurStatsPlayerTab(StatsPlayersTab _tab)
	{
		if (_tab == mStatsPlayersTab)
		{
			return;
		}
		mStatsPlayersTab = _tab;
		mFrinedsButton.Pressed = mStatsPlayersTab == (StatsPlayersTab)mFrinedsButton.mId;
		mIgnoresButton.Pressed = mStatsPlayersTab == (StatsPlayersTab)mIgnoresButton.mId;
		mPlayersButton.Pressed = mStatsPlayersTab == (StatsPlayersTab)mPlayersButton.mId;
		mCurPageNum = 0;
		OnPlayerSelect(null);
		SetCurTab(mCurTab);
		if (mGetList != null)
		{
			switch (_tab)
			{
			case StatsPlayersTab.FRIENDS:
				mGetList(PlayersListController.ListType.FRIENDS);
				break;
			case StatsPlayersTab.IGNORIES:
				mGetList(PlayersListController.ListType.IGNORE);
				break;
			case StatsPlayersTab.CS:
				mGetList(PlayersListController.ListType.LOCAL_AREA);
				break;
			}
		}
	}

	private void SetPageData()
	{
		mMaxPageNum = Mathf.CeilToInt((float)mCurPlayers.Count / (float)mCurMaxPlayersOnPage);
		mMaxPageNum = ((mMaxPageNum != 0) ? (mMaxPageNum - 1) : 0);
		mCurPageNum = ((mCurPageNum >= 0) ? mCurPageNum : 0);
		mCurPageNum = ((mCurPageNum <= mMaxPageNum) ? mCurPageNum : mMaxPageNum);
	}

	private void IncPageNum(bool _left)
	{
		if (_left)
		{
			mCurPageNum--;
		}
		else
		{
			mCurPageNum++;
		}
		mCurPageNum = ((mCurPageNum >= 0) ? mCurPageNum : 0);
		mCurPageNum = ((mCurPageNum <= mMaxPageNum) ? mCurPageNum : mMaxPageNum);
		SetPageButtonState();
	}

	private void SetSortType(SortType _type)
	{
		if (mSortType == _type)
		{
			mInverseSort = !mInverseSort;
		}
		mSortType = _type;
		foreach (GuiButton mSortTypeButton in mSortTypeButtons)
		{
			mSortTypeButton.Pressed = mSortType == (SortType)mSortTypeButton.mId;
		}
		SetSortButtonIcon();
		SetCurPlayersSize();
	}

	private void InitCurPlayers(StatsTab _tab, StatsPlayersTab _playersTab)
	{
		foreach (StatsPlayer mCurPlayer in mCurPlayers)
		{
			mCurPlayer.mOnPlayerSelect = (StatsPlayer.OnPlayerSelect)Delegate.Remove(mCurPlayer.mOnPlayerSelect, new StatsPlayer.OnPlayerSelect(OnPlayerSelect));
			mCurPlayer.mOnPlayerDoubleClicked = (StatsPlayer.OnPlayerSelect)Delegate.Remove(mCurPlayer.mOnPlayerDoubleClicked, new StatsPlayer.OnPlayerSelect(OnPlayerDoubleClicked));
			mCurPlayer.mOnPlayerPopUp = (StatsPlayer.ShowPopUpMenuCallback)Delegate.Remove(mCurPlayer.mOnPlayerPopUp, new StatsPlayer.ShowPopUpMenuCallback(OnPlayerPopUp));
		}
		mCurPlayers.Clear();
		List<ShortUserInfo> list = null;
		switch (_tab)
		{
		case StatsTab.SEARCH:
			list = mSearchData;
			break;
		case StatsTab.PLAYERS:
			switch (_playersTab)
			{
			case StatsPlayersTab.CS:
				list = mCSData;
				break;
			case StatsPlayersTab.FRIENDS:
				list = mFriendsData;
				break;
			case StatsPlayersTab.IGNORIES:
				list = mIgnoreData;
				break;
			}
			break;
		}
		if (list != null)
		{
			StatsPlayer statsPlayer = null;
			foreach (ShortUserInfo item in list)
			{
				statsPlayer = new StatsPlayer();
				statsPlayer.SetData(item);
				statsPlayer.Init();
				StatsPlayer statsPlayer2 = statsPlayer;
				statsPlayer2.mOnPlayerSelect = (StatsPlayer.OnPlayerSelect)Delegate.Combine(statsPlayer2.mOnPlayerSelect, new StatsPlayer.OnPlayerSelect(OnPlayerSelect));
				StatsPlayer statsPlayer3 = statsPlayer;
				statsPlayer3.mOnPlayerDoubleClicked = (StatsPlayer.OnPlayerSelect)Delegate.Combine(statsPlayer3.mOnPlayerDoubleClicked, new StatsPlayer.OnPlayerSelect(OnPlayerDoubleClicked));
				StatsPlayer statsPlayer4 = statsPlayer;
				statsPlayer4.mOnPlayerPopUp = (StatsPlayer.ShowPopUpMenuCallback)Delegate.Combine(statsPlayer4.mOnPlayerPopUp, new StatsPlayer.ShowPopUpMenuCallback(OnPlayerPopUp));
				mCurPlayers.Add(statsPlayer);
				if (mSelectedPlayer != null && mSelectedPlayer.UserData.mId == item.mId)
				{
					OnPlayerSelect(statsPlayer);
				}
			}
		}
		SetPageData();
		SetPageButtonState();
		SetCurPlayersSize();
	}
}
