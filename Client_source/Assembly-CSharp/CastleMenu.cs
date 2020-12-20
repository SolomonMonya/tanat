using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CastleMenu : GuiElement, EscapeListener
{
	private enum CastleMenuTab
	{
		NONE,
		MEMBERS,
		QUEUE,
		CURRENT
	}

	public class CastleMember : GuiElement
	{
		private Texture2D mFrame;

		private GuiButton mClanButton1;

		private GuiButton mClanButton2;

		private GuiButton mVSButton;

		private string mFramePath;

		private string mFrameWinPath;

		private string mFrameLosePath;

		private int mWinnerId;

		private ServerData mServerData;

		public OnClanInfo mOnClanInfo;

		public OnObserveBattle mOnObserveBattle;

		public CastleMember(string _framePath, string _winPath, string _losePath)
		{
			mFramePath = _framePath;
			mFrameWinPath = _winPath;
			mFrameLosePath = _losePath;
			mWinnerId = -1;
		}

		public override void Init()
		{
			mFrame = GuiSystem.GetImage(mFramePath);
			mVSButton = GuiSystem.CreateButton("Gui/CastleMenu/button_5_norm", "Gui/CastleMenu/button_5_over", "Gui/CastleMenu/button_5_press", string.Empty, string.Empty);
			mVSButton.mElementId = "VS_BUTTON";
			GuiButton guiButton = mVSButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mVSButton.mLockedColor = Color.gray;
			mVSButton.mLocked = true;
			mVSButton.Init();
		}

		public override void SetSize()
		{
			mVSButton.mZoneRect = new Rect((float)(mFrame.width - 49) / 2f, (float)(mFrame.height - 26) / 2f, 49f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mVSButton.mZoneRect);
			SetClanButtonSize();
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			if (mClanButton1 != null)
			{
				mClanButton1.RenderElement();
			}
			if (mClanButton2 != null)
			{
				mClanButton2.RenderElement();
			}
			if (mWinnerId != -1)
			{
				mVSButton.RenderElement();
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mWinnerId != -1)
			{
				mVSButton.CheckEvent(_curEvent);
			}
			if (mClanButton1 != null)
			{
				mClanButton1.CheckEvent(_curEvent);
			}
			if (mClanButton2 != null)
			{
				mClanButton2.CheckEvent(_curEvent);
			}
		}

		public void SetData(int _clan, KeyValuePair<int, string> _data)
		{
			if (_clan % 2 == 0)
			{
				mClanButton1 = new GuiButton();
				mClanButton1.mElementId = "CLAN_BUTTON";
				mClanButton1.mId = _data.Key;
				mClanButton1.mLabel = _data.Value;
				mClanButton1.mOverColor = Color.yellow;
				GuiButton guiButton = mClanButton1;
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				mClanButton1.Init();
			}
			else
			{
				mClanButton2 = new GuiButton();
				mClanButton2.mElementId = "CLAN_BUTTON";
				mClanButton2.mId = _data.Key;
				mClanButton2.mLabel = _data.Value;
				mClanButton2.mOverColor = Color.yellow;
				GuiButton guiButton2 = mClanButton2;
				guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
				mClanButton2.Init();
			}
			SetClanButtonSize();
		}

		public void SetWinner(int _winnerId)
		{
			mWinnerId = _winnerId;
			if (mWinnerId > 0)
			{
				mClanButton1.mIconImg = ((mWinnerId != mClanButton1.mId) ? GuiSystem.GetImage(mFrameLosePath) : GuiSystem.GetImage(mFrameWinPath));
				mClanButton2.mIconImg = ((mWinnerId != mClanButton2.mId) ? GuiSystem.GetImage(mFrameLosePath) : GuiSystem.GetImage(mFrameWinPath));
			}
		}

		public void SetServerData(ServerData _serverData)
		{
			mServerData = _serverData;
			if (mServerData != null)
			{
				mVSButton.mLocked = false;
			}
		}

		private void SetClanButtonSize()
		{
			if (mClanButton1 != null)
			{
				mClanButton1.mZoneRect = new Rect(4f, 4f, (float)mFrame.width / 2f - 8f, mFrame.height - 8);
				GuiSystem.SetChildRect(mZoneRect, ref mClanButton1.mZoneRect);
			}
			if (mClanButton2 != null)
			{
				mClanButton2.mZoneRect = new Rect((float)mFrame.width / 2f + 4f, 4f, (float)mFrame.width / 2f - 8f, mFrame.height - 8);
				GuiSystem.SetChildRect(mZoneRect, ref mClanButton2.mZoneRect);
			}
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
			else if (_sender.mElementId == "VS_BUTTON" && _buttonId == 0 && mOnObserveBattle != null && mServerData != null)
			{
				mOnObserveBattle(mServerData);
			}
		}
	}

	public class CastleRewardRenderer : GuiElement
	{
		private string mCastleRewardText;

		private Rect mCastleRewardTextRect;

		private FormatedTipMgr mFormatedTipMgr;

		private GuiButton mRewardItem;

		private CtrlPrototype mRewardItemArticul;

		private string mRewardItemCount;

		private MoneyRenderer mMoneyReward;

		private Texture2D mExpImage;

		private string mExp;

		private Rect mExpImageRect;

		private Rect mExpRect;

		public override void Init()
		{
			mCastleRewardText = GuiSystem.GetLocaleText("Castle_Reward");
			mRewardItem = new GuiButton();
			GuiButton guiButton = mRewardItem;
			guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton guiButton2 = mRewardItem;
			guiButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			mRewardItem.Init();
			mExpImage = GuiSystem.GetImage("Gui/misc/star");
		}

		public override void SetSize()
		{
			mCastleRewardTextRect = new Rect(9f, 14f, 620f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mCastleRewardTextRect);
			mRewardItem.mZoneRect = new Rect(241f, 51f, 38f, 38f);
			GuiSystem.SetChildRect(mZoneRect, ref mRewardItem.mZoneRect);
			mExpImageRect = new Rect(333f, 50f, 18f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mExpImageRect);
			mExpRect = new Rect(353f, 50f, 150f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mExpRect);
			SetMoneyRewardSize();
		}

		public override void CheckEvent(Event _curEvent)
		{
			mRewardItem.CheckEvent(_curEvent);
		}

		public override void RenderElement()
		{
			mRewardItem.RenderElement();
			GuiSystem.DrawImage(mExpImage, mExpImageRect);
			GuiSystem.DrawString(mCastleRewardText, mCastleRewardTextRect, "label");
			GuiSystem.DrawString(mRewardItemCount, mRewardItem.mZoneRect, "lower_right");
			GuiSystem.DrawString(mExp, mExpRect, "middle_left");
			if (mMoneyReward != null)
			{
				mMoneyReward.Render();
			}
		}

		public void SetData(FormatedTipMgr _tipMgr, CastleInfo.Reward _rewardData, CtrlPrototype _itemData)
		{
			mFormatedTipMgr = _tipMgr;
			mRewardItemArticul = null;
			mRewardItem.mIconImg = null;
			mRewardItem.mId = -1;
			mRewardItemCount = string.Empty;
			mExp = string.Empty;
			bool flag = _rewardData.mDiamonds != 0;
			mMoneyReward = new MoneyRenderer(_renderMoneyImage: true, flag);
			mMoneyReward.SetMoney((!flag) ? _rewardData.mMoney : _rewardData.mDiamonds);
			SetMoneyRewardSize();
			if (_rewardData != null && _itemData != null)
			{
				mRewardItemArticul = _itemData;
				mRewardItem.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mRewardItemArticul.Desc.mIcon);
				mRewardItem.mId = mRewardItemArticul.Id;
				mRewardItemCount = _rewardData.mItemCount.ToString();
				mExp = _rewardData.mExp.ToString();
			}
		}

		private void SetMoneyRewardSize()
		{
			if (mMoneyReward != null)
			{
				mMoneyReward.SetSize(mZoneRect);
				mMoneyReward.SetOffset(new Vector2(333f, 78f) * GuiSystem.mYRate);
			}
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Show(null, mRewardItemArticul, 999, 999, _sender.UId, true);
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(_sender.UId);
			}
		}
	}

	public delegate void OnCastleRequest(CastleInfo _castleInfo);

	public delegate void OnClanInfo(int _clanId);

	public delegate void OnObserveBattle(ServerData _serverData);

	private Texture2D mFrame;

	private Texture2D mFrameTime;

	private Texture2D mFramePage;

	private GuiButton mCloseButton;

	private GuiButton mHistoryButton;

	private GuiButton mJoinButton;

	private GuiButton mTabButton1;

	private GuiButton mTabButton2;

	private string mLabel;

	private Rect mLabelRect;

	private Rect mFrameTimeRect;

	private Rect mFramePageRect;

	private CastleInfo mCastleInfo;

	private CastleMembersArg mMembersData;

	private CastleBattleInfoArg mBattleData;

	private CastleMenuTab mCastleMenuTab;

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

	private GuiButton mPageLeftButton;

	private GuiButton mPageRightButton;

	private int mCurPageNum;

	private int mMaxPageNum;

	private string mRoundText;

	private string mCastleText;

	private string mFinalText;

	private string mCurRound;

	private Rect mCurRoundRect;

	private List<CastleMember> mCastleMembers;

	private bool mBattleStarted;

	private CastleRewardRenderer mCastleRewardRenderer;

	private FormatedTipMgr mFormatedTipMgr;

	private IStoreContentProvider<CtrlPrototype> mPrototypes;

	private OkDialog mOkDialog;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private Rect mDrawRect;

	public OnCastleRequest mOnCastleRequest;

	public OnCastleRequest mOnCastleHistoryRequest;

	public OnClanInfo mOnClanInfo;

	public OnObserveBattle mOnObserveBattle;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(380, this);
		mStartScrollOffset = 136f;
		mFrame = GuiSystem.GetImage("Gui/CastleMenu/frame1");
		mFramePage = GuiSystem.GetImage("Gui/CastleMenu/frame_page");
		mFrameTime = GuiSystem.GetImage("Gui/CastleMenu/frame_time");
		mCastleMembers = new List<CastleMember>();
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mHistoryButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mHistoryButton.mElementId = "HISTORY_BUTTON";
		GuiButton guiButton2 = mHistoryButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mHistoryButton.mLabel = GuiSystem.GetLocaleText("Castle_History");
		mHistoryButton.Init();
		mJoinButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mJoinButton.mElementId = "JOIN_BUTTON";
		GuiButton guiButton3 = mJoinButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mJoinButton.Init();
		mTabButton1 = new GuiButton();
		mTabButton1.mElementId = "TAB_BUTTON";
		GuiButton guiButton4 = mTabButton1;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mTabButton1.Init();
		mTabButton2 = new GuiButton();
		mTabButton2.mElementId = "TAB_BUTTON";
		GuiButton guiButton5 = mTabButton2;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mTabButton2.Init();
		mPageLeftButton = GuiSystem.CreateButton("Gui/misc/button_6_norm", "Gui/misc/button_6_over", "Gui/misc/button_6_press", string.Empty, string.Empty);
		mPageLeftButton.mElementId = "PAGE_LEFT_BUTTON";
		GuiButton guiButton6 = mPageLeftButton;
		guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
		mPageLeftButton.Init();
		mPageRightButton = GuiSystem.CreateButton("Gui/misc/button_7_norm", "Gui/misc/button_7_over", "Gui/misc/button_7_press", string.Empty, string.Empty);
		mPageRightButton.mElementId = "PAGE_RIGHT_BUTTON";
		GuiButton guiButton7 = mPageRightButton;
		guiButton7.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton7.mOnMouseUp, new OnMouseUp(OnButton));
		mPageRightButton.Init();
		mCastleClanControlButton = new GuiButton();
		mCastleClanControlButton.mElementId = "CASTLE_CLAN_CONTROL_BUTTON";
		GuiButton guiButton8 = mCastleClanControlButton;
		guiButton8.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton8.mOnMouseUp, new OnMouseUp(OnButton));
		mCastleClanControlButton.mOverColor = Color.yellow;
		mCastleClanControlButton.mLabelStyle = "middle_left";
		mCastleClanControlButton.Init();
		mCastleBattleStartTimeText = GuiSystem.GetLocaleText("Castle_Battle_Start_Time");
		mCastleClanControlText = GuiSystem.GetLocaleText("Castle_Clan_Control");
		mRoundText = GuiSystem.GetLocaleText("Battle_History_Round");
		mFinalText = GuiSystem.GetLocaleText("Battle_History_Final");
		mCastleText = GuiSystem.GetLocaleText("Battle_History_Castle");
		mCastleRewardRenderer = new CastleRewardRenderer();
		mCastleRewardRenderer.Init();
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mFrameTimeRect = new Rect(294f, 109f, 166f, 24f);
		GuiSystem.SetChildRect(mZoneRect, ref mFrameTimeRect);
		mFramePageRect = new Rect(286f, 482f, 122f, 40f);
		GuiSystem.SetChildRect(mZoneRect, ref mFramePageRect);
		mCloseButton.mZoneRect = new Rect(661f, 8f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mHistoryButton.mZoneRect = new Rect(32f, 488f, 154f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mHistoryButton.mZoneRect);
		mJoinButton.mZoneRect = new Rect(509f, 488f, 154f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mJoinButton.mZoneRect);
		mTabButton1.mZoneRect = new Rect(27f, 291f, 163f, 35f);
		GuiSystem.SetChildRect(mZoneRect, ref mTabButton1.mZoneRect);
		mTabButton2.mZoneRect = new Rect(163f, 291f, 163f, 35f);
		GuiSystem.SetChildRect(mZoneRect, ref mTabButton2.mZoneRect);
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
		mCastleRewardRenderer.mZoneRect = new Rect(29f, 169f, 637f, 113f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleRewardRenderer.mZoneRect);
		mCastleRewardRenderer.SetSize();
		mScrollbar.mZoneRect = new Rect(640f, 325f, 22f, 136f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mDrawRect = new Rect(33f, 325f, 607f, 138f);
		GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
		mCurRoundRect = new Rect(290f, 485f, 116f, 32f);
		GuiSystem.SetChildRect(mZoneRect, ref mCurRoundRect);
		mPageLeftButton.mZoneRect = new Rect(265f, 482f, 21f, 38f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageLeftButton.mZoneRect);
		mPageRightButton.mZoneRect = new Rect(408f, 482f, 21f, 38f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageRightButton.mZoneRect);
		SetCastleMembersSize();
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
			if (mCastleMenuTab == CastleMenuTab.CURRENT)
			{
				GuiSystem.DrawImage(mFramePage, mFramePageRect);
				GuiSystem.DrawString(mCurRound, mCurRoundRect, "middle_center");
				mPageLeftButton.RenderElement();
				mPageRightButton.RenderElement();
			}
		}
		mCastleRewardRenderer.RenderElement();
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		GuiSystem.DrawString(mCastleClanLevel, mCastleClanLevelRect, "middle_center");
		GuiSystem.DrawString(mCastleClanControlText, mCastleClanControlTextRect, "middle_right");
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.RenderElement();
		}
		else
		{
			GuiSystem.DrawString(mCastleClanControl, mCastleClanControlRect, "middle_left");
		}
		mCloseButton.RenderElement();
		mHistoryButton.RenderElement();
		mJoinButton.RenderElement();
		if (mTabButton1.mId == (int)mCastleMenuTab)
		{
			mTabButton2.RenderElement();
			mTabButton1.RenderElement();
		}
		else if (mTabButton2.mId == (int)mCastleMenuTab)
		{
			mTabButton1.RenderElement();
			mTabButton2.RenderElement();
		}
		foreach (CastleMember mCastleMember in mCastleMembers)
		{
			if (mCastleMember.Active)
			{
				mCastleMember.RenderElement();
			}
		}
		mScrollbar.RenderElement();
	}

	public override void OnInput()
	{
		mScrollbar.OnInput();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mHistoryButton.CheckEvent(_curEvent);
		mJoinButton.CheckEvent(_curEvent);
		mCastleRewardRenderer.CheckEvent(_curEvent);
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.CheckEvent(_curEvent);
		}
		if (mTabButton1.mId == (int)mCastleMenuTab)
		{
			mTabButton1.CheckEvent(_curEvent);
			mTabButton2.CheckEvent(_curEvent);
		}
		else if (mTabButton2.mId == (int)mCastleMenuTab)
		{
			mTabButton2.CheckEvent(_curEvent);
			mTabButton1.CheckEvent(_curEvent);
		}
		if (mBattleStarted && mCastleMenuTab == CastleMenuTab.CURRENT)
		{
			mPageLeftButton.CheckEvent(_curEvent);
			mPageRightButton.CheckEvent(_curEvent);
		}
		foreach (CastleMember mCastleMember in mCastleMembers)
		{
			if (mCastleMember.Active)
			{
				mCastleMember.CheckEvent(_curEvent);
			}
		}
		mScrollbar.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void Uninit()
	{
		mFormatedTipMgr = null;
		mPrototypes = null;
		mOkDialog = null;
	}

	public void SetData(FormatedTipMgr _tipMg, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes, OkDialog _okDialog)
	{
		mFormatedTipMgr = _tipMg;
		mPrototypes = _ctrlPrototypes;
		mOkDialog = _okDialog;
	}

	public void SetCastleData(CastleInfo _castleData)
	{
		if (_castleData != null)
		{
			mCastleInfo = _castleData;
		}
	}

	public void SetMembersData(CastleMembersArg _membersData)
	{
		if (_membersData != null)
		{
			mMembersData = _membersData;
		}
	}

	public void SetBattleData(CastleBattleInfoArg _battleData)
	{
		if (_battleData != null)
		{
			mBattleData = _battleData;
			mMaxPageNum = ((mBattleData.mBattles.Count != 1) ? (mBattleData.mBattles.Count - 1) : 0);
		}
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
		mMembersData = null;
		mBattleData = null;
		mBattleStarted = false;
		mCastleMenuTab = CastleMenuTab.NONE;
		mCastleMembers.Clear();
		mCastleClanControl = string.Empty;
	}

	private void InitData()
	{
		if (mCastleInfo != null && mMembersData != null)
		{
			mCurPageNum = 0;
			mScrollOffset = 0f;
			mScrollbar.Refresh();
			mBattleStarted = mMembersData.mInProgress;
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
			mJoinButton.mLocked = !mMembersData.mCanJoin;
			mJoinButton.mLabel = (mMembersData.mJoined ? GuiSystem.GetLocaleText("Castle_Join_Edit") : ((!mCastleInfo.mSelfOwner) ? GuiSystem.GetLocaleText("Castle_Join") : GuiSystem.GetLocaleText("Castle_Join_Defence")));
			mCastleRewardRenderer.SetData(mFormatedTipMgr, mCastleInfo.mReward, mPrototypes.Get(mCastleInfo.mReward.mItem));
			mTabButton1.mId = ((mBattleData == null) ? 1 : 3);
			mTabButton2.mId = 2;
			SetTabButtonLables();
			SetCurTab((CastleMenuTab)mTabButton1.mId);
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

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "HISTORY_BUTTON" && _buttonId == 0)
		{
			if (mOnCastleHistoryRequest != null && mCastleInfo != null)
			{
				mOnCastleHistoryRequest(mCastleInfo);
			}
		}
		else if (_sender.mElementId == "JOIN_BUTTON" && _buttonId == 0)
		{
			if (mBattleStarted)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("Castle_Request_Edit_Error"));
			}
			else if (mOnCastleRequest != null && mCastleInfo != null)
			{
				mOnCastleRequest(mCastleInfo);
			}
		}
		else if (_sender.mElementId == "TAB_BUTTON" && _buttonId == 0)
		{
			SetCurTab((CastleMenuTab)_sender.mId);
		}
		else if (_sender.mElementId == "PAGE_LEFT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPageNum - 1);
		}
		else if (_sender.mElementId == "PAGE_RIGHT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPageNum + 1);
		}
		else if (_sender.mElementId == "CASTLE_CLAN_CONTROL_BUTTON" && _buttonId == 0)
		{
			OnGetClanInfo(_sender.mId);
		}
	}

	private void SetCurTab(CastleMenuTab _tab)
	{
		if (mCastleMenuTab != _tab)
		{
			mCastleMenuTab = _tab;
			SetTabButtonImages();
			SetCastleMembers();
		}
	}

	private void SetTabButtonLables()
	{
		mTabButton1.mLabel = GuiSystem.GetLocaleText("Castle_Tab_Text_" + mTabButton1.mId);
		mTabButton2.mLabel = GuiSystem.GetLocaleText("Castle_Tab_Text_" + mTabButton2.mId);
	}

	private void SetTabButtonImages()
	{
		if (mTabButton1.mId == (int)mCastleMenuTab)
		{
			mTabButton1.mNormImg = GuiSystem.GetImage("Gui/CastleMenu/button_1_norm");
			mTabButton1.mOverImg = GuiSystem.GetImage("Gui/CastleMenu/button_1_over");
			mTabButton1.mPressImg = GuiSystem.GetImage("Gui/CastleMenu/button_1_over");
			mTabButton2.mNormImg = GuiSystem.GetImage("Gui/CastleMenu/button_4_norm");
			mTabButton2.mOverImg = GuiSystem.GetImage("Gui/CastleMenu/button_4_over");
			mTabButton2.mPressImg = GuiSystem.GetImage("Gui/CastleMenu/button_4_over");
		}
		else if (mTabButton2.mId == (int)mCastleMenuTab)
		{
			mTabButton1.mNormImg = GuiSystem.GetImage("Gui/CastleMenu/button_2_norm");
			mTabButton1.mOverImg = GuiSystem.GetImage("Gui/CastleMenu/button_2_over");
			mTabButton1.mPressImg = GuiSystem.GetImage("Gui/CastleMenu/button_2_over");
			mTabButton2.mNormImg = GuiSystem.GetImage("Gui/CastleMenu/button_3_norm");
			mTabButton2.mOverImg = GuiSystem.GetImage("Gui/CastleMenu/button_3_over");
			mTabButton2.mPressImg = GuiSystem.GetImage("Gui/CastleMenu/button_3_over");
		}
		mTabButton1.SetCurBtnImage();
		mTabButton2.SetCurBtnImage();
	}

	private void SetCastleMembers()
	{
		if (mMembersData != null)
		{
			Dictionary<int, string> dictionary = null;
			if (mCastleMenuTab == CastleMenuTab.QUEUE)
			{
				dictionary = mMembersData.mQueue;
			}
			else if (mCastleMenuTab == CastleMenuTab.MEMBERS)
			{
				dictionary = mMembersData.mMembers;
			}
			if (dictionary != null)
			{
				InitCastleMembers(dictionary);
			}
			else if (dictionary == null && mBattleData != null)
			{
				SetCurPage(mCurPageNum);
			}
			SetCastleMembersSize();
		}
	}

	private void InitCastleMembers(Dictionary<int, string> _members)
	{
		if (_members == null)
		{
			return;
		}
		mCastleMembers.Clear();
		CastleMember castleMember = null;
		int num = 0;
		foreach (KeyValuePair<int, string> _member in _members)
		{
			if (num % 2 == 0)
			{
				castleMember = new CastleMember("Gui/CastleMenu/frame2", "Gui/CastleMenu/frame_win", "Gui/CastleMenu/frame_lose");
				castleMember.Init();
				CastleMember castleMember2 = castleMember;
				castleMember2.mOnClanInfo = (OnClanInfo)Delegate.Combine(castleMember2.mOnClanInfo, new OnClanInfo(OnGetClanInfo));
			}
			castleMember.SetData(num, _member);
			num++;
			if (num % 2 == 0 || num == _members.Count)
			{
				mCastleMembers.Add(castleMember);
			}
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
		CastleMember castleMember = null;
		foreach (BattleInfo item in _battleInfo)
		{
			castleMember = new CastleMember("Gui/CastleMenu/frame2", "Gui/CastleMenu/frame_win", "Gui/CastleMenu/frame_lose");
			castleMember.Init();
			castleMember.SetData(0, new KeyValuePair<int, string>(item.mFirstId, item.mFirstName));
			castleMember.SetData(1, new KeyValuePair<int, string>(item.mSecondId, item.mSecondName));
			castleMember.SetWinner(item.mWinner);
			castleMember.SetServerData(item.mServerData);
			CastleMember castleMember2 = castleMember;
			castleMember2.mOnClanInfo = (OnClanInfo)Delegate.Combine(castleMember2.mOnClanInfo, new OnClanInfo(OnGetClanInfo));
			CastleMember castleMember3 = castleMember;
			castleMember3.mOnObserveBattle = (OnObserveBattle)Delegate.Combine(castleMember3.mOnObserveBattle, new OnObserveBattle(OnJoinObserveBattle));
			mCastleMembers.Add(castleMember);
		}
	}

	private void SetCastleMembersSize()
	{
		int num = 33;
		int num2 = 325;
		int num3 = 23;
		int num4 = 0;
		int num5 = 0;
		foreach (CastleMember mCastleMember in mCastleMembers)
		{
			mCastleMember.mZoneRect = new Rect(num, (float)(num2 + num4 * num3) - mScrollOffset, 607f, 24f);
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

	private void OnJoinObserveBattle(ServerData _serverData)
	{
		if (mOnObserveBattle != null)
		{
			mOnObserveBattle(_serverData);
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
		foreach (int key in mBattleData.mBattles.Keys)
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
		if (mBattleData.mBattles.ContainsKey(num))
		{
			InitCastleMembers(mBattleData.mBattles[num]);
			SetCastleMembersSize();
		}
	}
}
