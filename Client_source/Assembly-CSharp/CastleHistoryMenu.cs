using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CastleHistoryMenu : GuiElement, EscapeListener
{
	private class CastleHistoryComparer : IComparer<CastleHistory>
	{
		public int Compare(CastleHistory _obj1, CastleHistory _obj2)
		{
			return _obj2.mDate.CompareTo(_obj1.mDate);
		}
	}

	private class BattleHistory : GuiElement
	{
		private Texture2D mFrame;

		private GuiButton mCloseButton;

		private string mLabel;

		private Rect mLabelRect;

		private int mCurPageNum;

		private int mMaxPageNum;

		private string mRoundText;

		private string mCastleText;

		private string mFinalText;

		private string mCurRound;

		private Rect mCurRoundRect;

		private string mWinner;

		private string mDate;

		private string mWinnerText;

		private string mDateText;

		private Rect mWinnerRect;

		private Rect mDateRect;

		private Rect mWinnerTextRect;

		private Rect mDateTextRect;

		private GuiButton mPageLeftButton;

		private GuiButton mPageRightButton;

		private SortedDictionary<int, List<BattleInfo>> mHistoryData;

		private CastleHistory mCastleHistoryData;

		private List<CastleMenu.CastleMember> mCastleMembers;

		public OnInfoById mOnClanInfo;

		private VerticalScrollbar mScrollbar;

		private float mScrollOffset;

		private float mStartScrollOffset;

		private Rect mDrawRect;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/CastleHistoryMenu/frame4");
			mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
			mCloseButton.mElementId = "CLOSE_BUTTON";
			GuiButton guiButton = mCloseButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mCloseButton.Init();
			mLabel = GuiSystem.GetLocaleText("Battle_History_Battles");
			mRoundText = GuiSystem.GetLocaleText("Battle_History_Round");
			mFinalText = GuiSystem.GetLocaleText("Battle_History_Final");
			mCastleText = GuiSystem.GetLocaleText("Battle_History_Castle");
			mCastleMembers = new List<CastleMenu.CastleMember>();
			mPageLeftButton = GuiSystem.CreateButton("Gui/misc/button_6_norm", "Gui/misc/button_6_over", "Gui/misc/button_6_press", string.Empty, string.Empty);
			mPageLeftButton.mElementId = "PAGE_LEFT_BUTTON";
			GuiButton guiButton2 = mPageLeftButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			mPageLeftButton.Init();
			mPageRightButton = GuiSystem.CreateButton("Gui/misc/button_7_norm", "Gui/misc/button_7_over", "Gui/misc/button_7_press", string.Empty, string.Empty);
			mPageRightButton.mElementId = "PAGE_RIGHT_BUTTON";
			GuiButton guiButton3 = mPageRightButton;
			guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
			mPageRightButton.Init();
			mScrollbar = new VerticalScrollbar();
			mScrollbar.Init();
			VerticalScrollbar verticalScrollbar = mScrollbar;
			verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
			mDateText = GuiSystem.GetLocaleText("Battle_History_Date_Text");
			mWinnerText = GuiSystem.GetLocaleText("Battle_History_Winner_Text");
		}

		public override void Uninit()
		{
			mOnClanInfo = null;
			mHistoryData = null;
			mCastleHistoryData = null;
			if (mCastleMembers != null)
			{
				mCastleMembers.Clear();
			}
		}

		public override void SetSize()
		{
			mStartScrollOffset = 138f;
			mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
			mCloseButton.mZoneRect = new Rect(626f, 142f, 23f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
			mLabelRect = new Rect(60f, 143f, 565f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			mScrollbar.mZoneRect = new Rect(626f, 241f, 22f, 138f);
			GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
			mScrollbar.SetSize();
			mDrawRect = new Rect(47f, 241f, 577f, 138f);
			GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
			mCurRoundRect = new Rect(280f, 400f, 116f, 32f);
			GuiSystem.SetChildRect(mZoneRect, ref mCurRoundRect);
			mPageLeftButton.mZoneRect = new Rect(255f, 398f, 21f, 38f);
			GuiSystem.SetChildRect(mZoneRect, ref mPageLeftButton.mZoneRect);
			mPageRightButton.mZoneRect = new Rect(399f, 398f, 21f, 38f);
			GuiSystem.SetChildRect(mZoneRect, ref mPageRightButton.mZoneRect);
			mDateRect = new Rect(256f, 179f, 158f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mDateRect);
			mWinnerRect = new Rect(260f, 210f, 365f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mWinnerRect);
			mDateTextRect = new Rect(49f, 179f, 178f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mDateTextRect);
			mWinnerTextRect = new Rect(49f, 210f, 178f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mWinnerTextRect);
			SetCastleMembersSize();
		}

		public override void OnInput()
		{
			mScrollbar.OnInput();
		}

		public override void CheckEvent(Event _curEvent)
		{
			mCloseButton.CheckEvent(_curEvent);
			foreach (CastleMenu.CastleMember mCastleMember in mCastleMembers)
			{
				if (mCastleMember.Active)
				{
					mCastleMember.CheckEvent(_curEvent);
				}
			}
			mPageLeftButton.CheckEvent(_curEvent);
			mPageRightButton.CheckEvent(_curEvent);
			mScrollbar.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mLabel, mLabelRect, "label");
			GuiSystem.DrawString(mCurRound, mCurRoundRect, "middle_center");
			foreach (CastleMenu.CastleMember mCastleMember in mCastleMembers)
			{
				if (mCastleMember.Active)
				{
					mCastleMember.RenderElement();
				}
			}
			GuiSystem.DrawString(mDateText, mDateTextRect, "middle_right");
			GuiSystem.DrawString(mWinnerText, mWinnerTextRect, "middle_right");
			GuiSystem.DrawString(mDate, mDateRect, "middle_center");
			GuiSystem.DrawString(mWinner, mWinnerRect, "middle_left");
			mPageLeftButton.RenderElement();
			mPageRightButton.RenderElement();
			mScrollbar.RenderElement();
			mCloseButton.RenderElement();
		}

		public void Close()
		{
			SetActive(_active: false);
			if (mCastleMembers != null)
			{
				mCastleMembers.Clear();
			}
			mHistoryData = null;
		}

		public void Open()
		{
			SetActive(_active: true);
			InitData();
		}

		public void SetData(SortedDictionary<int, List<BattleInfo>> _historyData, CastleHistory _data)
		{
			mHistoryData = _historyData;
			mCastleHistoryData = _data;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
			{
				Close();
			}
			else if (_sender.mElementId == "PAGE_LEFT_BUTTON" && _buttonId == 0)
			{
				SetCurPage(mCurPageNum - 1);
			}
			else if (_sender.mElementId == "PAGE_RIGHT_BUTTON" && _buttonId == 0)
			{
				SetCurPage(mCurPageNum + 1);
			}
		}

		private void InitData()
		{
			if (mHistoryData != null && mCastleHistoryData != null && mHistoryData.Count != 0)
			{
				mCurPageNum = 0;
				mScrollOffset = 0f;
				mDate = mCastleHistoryData.mDate.ToString();
				mWinner = mCastleHistoryData.mWinnerName;
				mMaxPageNum = ((mHistoryData.Count != 1) ? (mHistoryData.Count - 1) : 0);
				mScrollbar.Refresh();
				SetCurPage(0);
			}
		}

		private void SetCurPage(int _pageNum)
		{
			mCurPageNum = _pageNum;
			if (mCurPageNum < 0)
			{
				mCurPageNum = 0;
			}
			if (mCurPageNum > mMaxPageNum)
			{
				mCurPageNum = mMaxPageNum;
			}
			mPageLeftButton.mLocked = mCurPageNum == 0;
			mPageRightButton.mLocked = mCurPageNum == mMaxPageNum;
			int num = 0;
			int num2 = 0;
			foreach (int key in mHistoryData.Keys)
			{
				if (num2 == mMaxPageNum - mCurPageNum)
				{
					num = key;
					break;
				}
				num2++;
			}
			switch (num)
			{
			case 1:
				mCurRound = mFinalText;
				break;
			case 0:
				mCurRound = mCastleText;
				break;
			default:
				mCurRound = mRoundText;
				mCurRound = mCurRound.Replace("{CUR_ROUND}", (mCurPageNum + 1).ToString());
				mCurRound = mCurRound.Replace("{CUR_ROUND_PART}", "1");
				mCurRound = mCurRound.Replace("{MAX_ROUND}", num.ToString());
				break;
			}
			if (mHistoryData.ContainsKey(num))
			{
				InitCastleMembers(mHistoryData[num]);
				SetCastleMembersSize();
			}
		}

		private void InitCastleMembers(List<BattleInfo> _battleInfo)
		{
			if (_battleInfo == null)
			{
				return;
			}
			mScrollOffset = 0f;
			mScrollbar.Refresh();
			mCastleMembers.Clear();
			CastleMenu.CastleMember castleMember = null;
			foreach (BattleInfo item in _battleInfo)
			{
				castleMember = new CastleMenu.CastleMember("Gui/CastleHistoryMenu/frame3", "Gui/CastleHistoryMenu/frame_win", "Gui/CastleHistoryMenu/frame_lose");
				castleMember.Init();
				castleMember.SetData(0, new KeyValuePair<int, string>(item.mFirstId, item.mFirstName));
				castleMember.SetData(1, new KeyValuePair<int, string>(item.mSecondId, item.mSecondName));
				castleMember.SetWinner(item.mWinner);
				CastleMenu.CastleMember castleMember2 = castleMember;
				castleMember2.mOnClanInfo = (CastleMenu.OnClanInfo)Delegate.Combine(castleMember2.mOnClanInfo, new CastleMenu.OnClanInfo(OnGetClanInfo));
				mCastleMembers.Add(castleMember);
			}
		}

		private void OnScrollbar(GuiElement _sender, float _offset)
		{
			if (_offset != mScrollOffset)
			{
				mScrollOffset = _offset;
				SetCastleMembersSize();
			}
		}

		private void SetCastleMembersSize()
		{
			int num = 47;
			int num2 = 241;
			int num3 = 23;
			int num4 = 0;
			int num5 = 0;
			foreach (CastleMenu.CastleMember mCastleMember in mCastleMembers)
			{
				mCastleMember.mZoneRect = new Rect(num, (float)(num2 + num4 * num3) - mScrollOffset, 579f, 24f);
				GuiSystem.SetChildRect(mZoneRect, ref mCastleMember.mZoneRect);
				mCastleMember.SetSize();
				mCastleMember.SetActive(!(mCastleMember.mZoneRect.y + 1f < mDrawRect.y) && !(mCastleMember.mZoneRect.y + mCastleMember.mZoneRect.height > mDrawRect.y + mDrawRect.height + 1f));
				num5 += num3;
				num4++;
			}
			mScrollbar.SetData(mStartScrollOffset, num5);
		}

		private void OnGetClanInfo(int _clanId)
		{
			if (mOnClanInfo != null)
			{
				mOnClanInfo(_clanId);
			}
		}
	}

	public class HistoryMember : GuiElement
	{
		private CastleHistory mData;

		private Texture2D mFrame;

		private GuiButton mHistoryButton;

		private GuiButton mClanButton;

		private string mTime;

		private Rect mTimeRect;

		public OnInfoById mOnClanInfo;

		public OnInfoById mOnBattleHistoryInfo;

		public override void Init()
		{
			if (mData != null)
			{
				mFrame = GuiSystem.GetImage("Gui/CastleHistoryMenu/frame2");
				mClanButton = new GuiButton();
				mClanButton.mElementId = "CLAN_BUTTON";
				mClanButton.mId = mData.mWinnerId;
				mClanButton.mLabel = mData.mWinnerName;
				mClanButton.mOverColor = Color.yellow;
				GuiButton guiButton = mClanButton;
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				mClanButton.Init();
				mHistoryButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
				mHistoryButton.mElementId = "HISTORY_BUTTON";
				mHistoryButton.mId = mData.mBattleId;
				GuiButton guiButton2 = mHistoryButton;
				guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
				mHistoryButton.mLabel = GuiSystem.GetLocaleText("Battle_History_Button");
				mHistoryButton.Init();
				mTime = mData.mDate.ToString();
			}
		}

		public override void SetSize()
		{
			mClanButton.mZoneRect = new Rect(159f, 1f, 376f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mClanButton.mZoneRect);
			mHistoryButton.mZoneRect = new Rect(537f, 1f, 68f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mHistoryButton.mZoneRect);
			mTimeRect = new Rect(1f, 1f, 156f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mTime, mTimeRect, "middle_center");
			mClanButton.RenderElement();
			mHistoryButton.RenderElement();
		}

		public override void CheckEvent(Event _curEvent)
		{
			mClanButton.CheckEvent(_curEvent);
			mHistoryButton.CheckEvent(_curEvent);
		}

		public void SetData(CastleHistory _data)
		{
			mData = _data;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "CLAN_BUTTON" && _buttonId == 0)
			{
				if (mOnClanInfo != null)
				{
					mOnClanInfo(_sender.mId);
				}
			}
			else if (_sender.mElementId == "HISTORY_BUTTON" && _buttonId == 0 && mOnBattleHistoryInfo != null && mData != null)
			{
				mOnBattleHistoryInfo(_sender.mId);
			}
		}
	}

	public delegate void OnInfoById(int _id);

	public delegate void OnBattleInfoById(int _id, int _additionalId);

	private Texture2D mFrame;

	private Texture2D mFrameTime;

	private GuiButton mCloseButton;

	private GuiButton mBackButton;

	private string mLabel;

	private Rect mLabelRect;

	private Rect mFrameTimeRect;

	private string mBattleHistoryLabel;

	private string mBattleHistoryDate;

	private string mBattleHistoryWinner;

	private Rect mBattleHistoryLabelRect;

	private Rect mBattleHistoryDateRect;

	private Rect mBattleHistoryWinnerRect;

	private GuiButton mCastleClanControlButton;

	private string mCastleClanLevel;

	private string mCastleBattleStartTimeText;

	private string mCastleClanControlText;

	private string mCastleBattleStartTime;

	private string mCastleClanControl;

	private Rect mCastleClanLevelRect;

	private Rect mCastleBattleStartTimeTextRect;

	private Rect mCastleClanControlTextRect;

	private Rect mCastleBattleStartTimeRect;

	private Rect mCastleClanControlRect;

	private Rect mCastleBattleStartedTextRect;

	private CastleInfo mCastleInfo;

	private bool mBattleStarted;

	private List<CastleHistory> mHistoryData;

	private List<HistoryMember> mHistoryMembers;

	private BattleHistory mBattleHistory;

	private int mCastleHistoryBattleId;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private Rect mDrawRect;

	public OnBattleInfoById mOnBattleHistoryInfo;

	public OnInfoById mOnClanInfo;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			Close();
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(390, this);
		mFrame = GuiSystem.GetImage("Gui/CastleHistoryMenu/frame1");
		mFrameTime = GuiSystem.GetImage("Gui/CastleMenu/frame_time");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mBackButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBackButton.mElementId = "BACK_BUTTON";
		GuiButton guiButton2 = mBackButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mBackButton.mLabel = GuiSystem.GetLocaleText("Back_Button_Name");
		mBackButton.Init();
		mCastleClanControlButton = new GuiButton();
		mCastleClanControlButton.mElementId = "CASTLE_CLAN_CONTROL_BUTTON";
		GuiButton guiButton3 = mCastleClanControlButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mCastleClanControlButton.mOverColor = Color.yellow;
		mCastleClanControlButton.mLabelStyle = "middle_left";
		mCastleClanControlButton.Init();
		mCastleBattleStartTimeText = GuiSystem.GetLocaleText("Castle_Battle_Start_Time");
		mCastleClanControlText = GuiSystem.GetLocaleText("Castle_Clan_Control");
		mBattleHistoryLabel = GuiSystem.GetLocaleText("Battle_History_Label");
		mBattleHistoryDate = GuiSystem.GetLocaleText("Battle_History_Date");
		mBattleHistoryWinner = GuiSystem.GetLocaleText("Battle_History_Winner");
		mHistoryMembers = new List<HistoryMember>();
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		mBattleHistory = new BattleHistory();
		mBattleHistory.Init();
		mBattleHistory.Close();
		BattleHistory battleHistory = mBattleHistory;
		battleHistory.mOnClanInfo = (OnInfoById)Delegate.Combine(battleHistory.mOnClanInfo, new OnInfoById(OnGetClanInfo));
	}

	public override void Uninit()
	{
		mCastleInfo = null;
		if (mBattleHistory != null)
		{
			BattleHistory battleHistory = mBattleHistory;
			battleHistory.mOnClanInfo = (OnInfoById)Delegate.Remove(battleHistory.mOnClanInfo, new OnInfoById(OnGetClanInfo));
			mBattleHistory.Uninit();
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mFrameTimeRect = new Rect(294f, 109f, 166f, 24f);
		GuiSystem.SetChildRect(mZoneRect, ref mFrameTimeRect);
		mCloseButton.mZoneRect = new Rect(661f, 8f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mBackButton.mZoneRect = new Rect(298f, 488f, 102f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mBackButton.mZoneRect);
		mLabelRect = new Rect(28f, 8f, 644f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		mCastleClanLevelRect = new Rect(254f, 58f, 188f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanLevelRect);
		mCastleBattleStartTimeRect = new Rect(297f, 111f, 158f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleBattleStartTimeRect);
		mCastleClanControlRect = new Rect(303f, 142f, 360f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanControlRect);
		mCastleClanControlButton.mZoneRect = mCastleClanControlRect;
		mCastleBattleStartTimeTextRect = new Rect(8f, 111f, 265f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleBattleStartTimeTextRect);
		mCastleClanControlTextRect = new Rect(8f, 142f, 265f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanControlTextRect);
		mCastleBattleStartedTextRect = new Rect(260f, 111f, 175f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleBattleStartedTextRect);
		mStartScrollOffset = 231f;
		mScrollbar.mZoneRect = new Rect(640f, 232f, 22f, 228f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mDrawRect = new Rect(34f, 232f, 606f, 231f);
		GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
		mBattleHistoryLabelRect = new Rect(37f, 182f, 620f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mBattleHistoryLabelRect);
		mBattleHistoryDateRect = new Rect(34f, 207f, 158f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mBattleHistoryDateRect);
		mBattleHistoryWinnerRect = new Rect(192f, 207f, 378f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mBattleHistoryWinnerRect);
		mBattleHistory.SetSize();
		SetHistoryMembersSize();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		if (!mBattleStarted)
		{
			GuiSystem.DrawImage(mFrameTime, mFrameTimeRect);
			GuiSystem.DrawString(mCastleBattleStartTimeText, mCastleBattleStartTimeTextRect, "middle_right");
			GuiSystem.DrawString(mCastleBattleStartTime, mCastleBattleStartTimeRect, "middle_center");
		}
		else
		{
			GuiSystem.DrawString(mCastleBattleStartTime, mCastleBattleStartedTextRect, "middle_center");
		}
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.RenderElement();
		}
		else
		{
			GuiSystem.DrawString(mCastleClanControl, mCastleClanControlRect, "middle_left");
		}
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		GuiSystem.DrawString(mCastleClanLevel, mCastleClanLevelRect, "middle_center");
		GuiSystem.DrawString(mCastleClanControlText, mCastleClanControlTextRect, "middle_right");
		GuiSystem.DrawString(mCastleClanControl, mCastleClanControlRect, "middle_left");
		GuiSystem.DrawString(mBattleHistoryLabel, mBattleHistoryLabelRect, "label");
		GuiSystem.DrawString(mBattleHistoryDate, mBattleHistoryDateRect, "label");
		GuiSystem.DrawString(mBattleHistoryWinner, mBattleHistoryWinnerRect, "label");
		foreach (HistoryMember mHistoryMember in mHistoryMembers)
		{
			if (mHistoryMember.Active)
			{
				mHistoryMember.RenderElement();
			}
		}
		mCloseButton.RenderElement();
		mBackButton.RenderElement();
		mScrollbar.RenderElement();
		if (mBattleHistory.Active)
		{
			mBattleHistory.RenderElement();
		}
	}

	public override void OnInput()
	{
		if (mBattleHistory.Active)
		{
			mBattleHistory.OnInput();
		}
		else
		{
			mScrollbar.OnInput();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mBattleHistory.Active)
		{
			mBattleHistory.CheckEvent(_curEvent);
		}
		if (_curEvent.type == EventType.Used)
		{
			return;
		}
		mCloseButton.CheckEvent(_curEvent);
		mBackButton.CheckEvent(_curEvent);
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.CheckEvent(_curEvent);
		}
		foreach (HistoryMember mHistoryMember in mHistoryMembers)
		{
			if (mHistoryMember.Active)
			{
				mHistoryMember.CheckEvent(_curEvent);
			}
		}
		mScrollbar.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void Open()
	{
		SetActive(_active: true);
		InitData();
	}

	public void Close()
	{
		SetActive(_active: false);
		mCastleInfo = null;
		mBattleStarted = false;
		mCastleHistoryBattleId = 0;
		mCastleClanControl = string.Empty;
	}

	public void SetCastleData(CastleInfo _castleData)
	{
		if (_castleData != null)
		{
			mCastleInfo = _castleData;
		}
	}

	public void SetHistoryData(List<CastleHistory> _history)
	{
		mHistoryData = _history;
		CastleHistoryComparer comparer = new CastleHistoryComparer();
		mHistoryData.Sort(comparer);
	}

	public void SetBattleHistoryData(SortedDictionary<int, List<BattleInfo>> _data)
	{
		CastleHistory data = null;
		foreach (CastleHistory mHistoryDatum in mHistoryData)
		{
			if (mHistoryDatum.mBattleId == mCastleHistoryBattleId)
			{
				data = mHistoryDatum;
				break;
			}
		}
		mBattleHistory.SetData(_data, data);
		mBattleHistory.Open();
		mCastleHistoryBattleId = 0;
	}

	private void InitData()
	{
		if (mCastleInfo != null)
		{
			mScrollOffset = 0f;
			mScrollbar.Refresh();
			mBattleStarted = DateTime.Now.CompareTo(mCastleInfo.mStartTime) >= 1;
			mLabel = GuiSystem.GetLocaleText(mCastleInfo.mName);
			mCastleClanLevel = GuiSystem.GetLocaleText("Castle_Clan_Level");
			string text = mCastleClanLevel;
			mCastleClanLevel = text + " " + mCastleInfo.mLevelMin + "-" + mCastleInfo.mLevelMax;
			if (mBattleStarted)
			{
				mCastleBattleStartTime = GuiSystem.GetLocaleText("Castle_Battle_Started");
			}
			else
			{
				mCastleBattleStartTime = mCastleInfo.mStartTime.ToString();
			}
			if (string.IsNullOrEmpty(mCastleInfo.mOwnerName))
			{
				mCastleClanControl = GuiSystem.GetLocaleText("Castle_Clan_No_Control_Text");
				mCastleClanControlButton.mLabel = string.Empty;
				mCastleClanControlButton.mId = 0;
			}
			else
			{
				mCastleClanControl = string.Empty;
				mCastleClanControlButton.mLabel = mCastleInfo.mOwnerName;
				mCastleClanControlButton.mId = mCastleInfo.mOwnerId;
			}
			InitHistoryMembers();
			SetHistoryMembersSize();
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ((_sender.mElementId == "CLOSE_BUTTON" || _sender.mElementId == "BACK_BUTTON") && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "CASTLE_CLAN_CONTROL_BUTTON" && _buttonId == 0)
		{
			OnGetClanInfo(_sender.mId);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetHistoryMembersSize();
		}
	}

	private void InitHistoryMembers()
	{
		if (mHistoryData == null)
		{
			return;
		}
		mHistoryMembers.Clear();
		HistoryMember historyMember = null;
		foreach (CastleHistory mHistoryDatum in mHistoryData)
		{
			historyMember = new HistoryMember();
			historyMember.SetData(mHistoryDatum);
			historyMember.Init();
			HistoryMember historyMember2 = historyMember;
			historyMember2.mOnClanInfo = (OnInfoById)Delegate.Combine(historyMember2.mOnClanInfo, new OnInfoById(OnGetClanInfo));
			HistoryMember historyMember3 = historyMember;
			historyMember3.mOnBattleHistoryInfo = (OnInfoById)Delegate.Combine(historyMember3.mOnBattleHistoryInfo, new OnInfoById(OnBattleHistoryInfo));
			mHistoryMembers.Add(historyMember);
		}
	}

	private void SetHistoryMembersSize()
	{
		int num = 34;
		int num2 = 232;
		int num3 = 27;
		int num4 = 0;
		int num5 = 0;
		foreach (HistoryMember mHistoryMember in mHistoryMembers)
		{
			mHistoryMember.mZoneRect = new Rect(num, (float)(num2 + num4 * num3) - mScrollOffset, 606f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mHistoryMember.mZoneRect);
			mHistoryMember.SetSize();
			mHistoryMember.SetActive(!(mHistoryMember.mZoneRect.y + 1f < mDrawRect.y) && !(mHistoryMember.mZoneRect.y + mHistoryMember.mZoneRect.height > mDrawRect.y + mDrawRect.height + 1f));
			num5 += num3;
			num4++;
		}
		mScrollbar.SetData(mStartScrollOffset, num5);
	}

	private void OnGetClanInfo(int _clanId)
	{
		if (mOnClanInfo != null)
		{
			mOnClanInfo(_clanId);
		}
	}

	private void OnBattleHistoryInfo(int _battleId)
	{
		if (mOnBattleHistoryInfo != null && mCastleInfo != null)
		{
			mCastleHistoryBattleId = _battleId;
			mOnBattleHistoryInfo(mCastleInfo.mId, mCastleHistoryBattleId);
		}
	}
}
