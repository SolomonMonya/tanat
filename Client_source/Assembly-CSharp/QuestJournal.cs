using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class QuestJournal : GuiElement, EscapeListener
{
	private enum QuestProcessType
	{
		NONE,
		CURRENT,
		EXECUTED
	}

	private class QuestGroup : GuiElement
	{
		public QuestPvEType mQuestGroup;

		public GuiButton mGroupButton;

		public List<GuiButton> mQuestButtons;

		public Rect mRenderZone;

		private bool mOpened;

		private Texture2D mQuestSelectionImg;

		private Vector2 mOffset = Vector2.zero;

		public QuestGroup(QuestPvEType _type)
		{
			mQuestGroup = _type;
		}

		public override void Init()
		{
			mOpened = false;
			mGroupButton = new GuiButton();
			mGroupButton.mId = (int)mQuestGroup;
			mGroupButton.mElementId = "GROUP_BUTTON";
			mGroupButton.mLabel = GetQuestGroupText(mQuestGroup);
			mGroupButton.mLockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			mGroupButton.Init();
			mQuestSelectionImg = GuiSystem.GetImage("Gui/QuestJournal/selection");
			mQuestButtons = new List<GuiButton>();
			SetGroupButtonImage();
		}

		public override void SetSize()
		{
			SetQuestsSize();
		}

		public override void RenderElement()
		{
			if (ButtonInRenderZone(mGroupButton))
			{
				mGroupButton.RenderElement();
			}
			if (!mOpened)
			{
				return;
			}
			foreach (GuiButton mQuestButton in mQuestButtons)
			{
				if (ButtonInRenderZone(mQuestButton))
				{
					mQuestButton.RenderElement();
				}
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			mGroupButton.CheckEvent(_curEvent);
			if (!mOpened)
			{
				return;
			}
			foreach (GuiButton mQuestButton in mQuestButtons)
			{
				mQuestButton.CheckEvent(_curEvent);
			}
		}

		public void ClearQuests()
		{
			mQuestButtons.Clear();
			SetGroupState(_state: false);
			SetGroupButtonState();
		}

		public void AddQuest(ISelfQuest _quest)
		{
			if (_quest != null)
			{
				GuiButton guiButton = new GuiButton();
				guiButton.mId = _quest.Id;
				guiButton.mElementId = "QUEST_BUTTON";
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, mGroupButton.mOnMouseUp);
				guiButton.mOverImg = mQuestSelectionImg;
				guiButton.mLabel = GuiSystem.GetLocaleText(_quest.Quest.Name);
				guiButton.mLabelStyle = "middle_left";
				mQuestButtons.Add(guiButton);
				SetGroupButtonState();
			}
		}

		public int GetFirstQuest()
		{
			if (mQuestButtons.Count > 0)
			{
				return mQuestButtons[0].mId;
			}
			return -1;
		}

		public void SetQuestsSize()
		{
			int num = 0;
			foreach (GuiButton mQuestButton in mQuestButtons)
			{
				mQuestButton.mZoneRect = new Rect(mOffset.x, mOffset.y + 23f + (float)(num * mQuestSelectionImg.height), mQuestSelectionImg.width, mQuestSelectionImg.height);
				GuiSystem.SetChildRect(mZoneRect, ref mQuestButton.mZoneRect);
				num++;
			}
		}

		public void SetGroupState(bool _state)
		{
			mOpened = _state;
			SetGroupButtonImage();
		}

		public bool GetGroupState()
		{
			return mOpened;
		}

		public float GetGroupLength()
		{
			if (mOpened)
			{
				return 23 + mQuestButtons.Count * mQuestSelectionImg.height;
			}
			return 23f;
		}

		public void SetOffset(Vector2 _offset)
		{
			mOffset = _offset;
			mGroupButton.mZoneRect = new Rect(mOffset.x, mOffset.y, 150f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref mGroupButton.mZoneRect);
		}

		public void SetSelectedQuest(int _id)
		{
			foreach (GuiButton mQuestButton in mQuestButtons)
			{
				mQuestButton.Pressed = _id == mQuestButton.mId;
				if (mQuestButton.Pressed)
				{
					SetGroupState(_state: true);
				}
			}
		}

		public bool HasQuest(int _id)
		{
			foreach (GuiButton mQuestButton in mQuestButtons)
			{
				if (mQuestButton.mId == _id)
				{
					return true;
				}
			}
			return false;
		}

		private bool ButtonInRenderZone(GuiButton _btn)
		{
			return _btn.mZoneRect.y >= mRenderZone.y && _btn.mZoneRect.y <= mRenderZone.y + mRenderZone.height - _btn.mZoneRect.height;
		}

		private void SetGroupButtonState()
		{
			if (mGroupButton != null)
			{
				mGroupButton.mLocked = mQuestButtons.Count == 0;
			}
		}

		private void SetGroupButtonImage()
		{
			string text = "Gui/QuestJournal/button_" + ((!mOpened) ? "1" : "2");
			mGroupButton.mNormImg = GuiSystem.GetImage(text + "_norm");
			mGroupButton.mOverImg = GuiSystem.GetImage(text + "_over");
			mGroupButton.mPressImg = GuiSystem.GetImage(text + "_press");
			mGroupButton.SetCurBtnImage();
		}

		private string GetQuestGroupText(QuestPvEType _type)
		{
			return _type switch
			{
				QuestPvEType.SINGLE => GuiSystem.GetLocaleText("Quest_Type_Single_Text"), 
				QuestPvEType.GROUP => GuiSystem.GetLocaleText("Quest_Type_Group_Text"), 
				QuestPvEType.REPLAY => GuiSystem.GetLocaleText("Quest_Type_Replay_Text"), 
				_ => string.Empty, 
			};
		}
	}

	private class QuestInfo : GuiElement
	{
		public GuiButton mCancelQuestButton;

		public GuiButton mShowQuestInfoButton;

		private ISelfQuest mQuestData;

		private Texture2D mCurFrame;

		private Texture2D mFrame1;

		private Texture2D mFrame2;

		private Texture2D mFrame3;

		private string mName;

		private Rect mNameRect;

		private string mProgressDesc;

		private Rect mProgressDescRect;

		private string mTaskDesc;

		private Rect mTaskDescRect;

		private string mCooldownText;

		private string mCooldown;

		private Rect mCooldownRect;

		private RewardQuestInfo mRewardQuestInfo;

		private SelfHero mSelfHero;

		private FormatedTipMgr mFormatedTipMgr;

		private string mDayStr;

		private string mHourStr;

		private string mMinStr;

		private string mSecStr;

		public override void Init()
		{
			mFrame1 = GuiSystem.GetImage("Gui/QuestJournal/frame2");
			mFrame2 = GuiSystem.GetImage("Gui/QuestJournal/frame3");
			mFrame3 = GuiSystem.GetImage("Gui/QuestJournal/frame4");
			mDayStr = GuiSystem.GetLocaleText("Day_Text");
			mHourStr = GuiSystem.GetLocaleText("Hour_Text");
			mMinStr = GuiSystem.GetLocaleText("Min_Text");
			mSecStr = GuiSystem.GetLocaleText("Sec_Text");
			mCancelQuestButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			mCancelQuestButton.mElementId = "CANCEL_QUEST_BUTTON";
			mCancelQuestButton.mLabel = GuiSystem.GetLocaleText("Quest_Cancel_Text");
			mCancelQuestButton.Init();
			mShowQuestInfoButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			mShowQuestInfoButton.mElementId = "SHOW_QUEST_INFO_BUTTON";
			mShowQuestInfoButton.Init();
		}

		public override void SetSize()
		{
			mNameRect = new Rect(8f, 8f, 323f, 16f);
			mProgressDescRect = new Rect(15f, 155f, 310f, 39f);
			mCancelQuestButton.mZoneRect = new Rect(79f, 372f, 175f, 28f);
			mShowQuestInfoButton.mZoneRect = new Rect(69f, 205f, 199f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mProgressDescRect);
			GuiSystem.SetChildRect(mZoneRect, ref mCancelQuestButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mShowQuestInfoButton.mZoneRect);
			if (mQuestData != null)
			{
				SetRewardInfoSize(mQuestData.Status);
				SetInfoSize(mQuestData.Status);
			}
		}

		public override void RenderElement()
		{
			if (mQuestData != null)
			{
				GuiSystem.DrawImage(mCurFrame, mZoneRect);
				GuiSystem.DrawString(mName, mNameRect, "middle_center");
				if (mTaskDesc != string.Empty)
				{
					GuiSystem.DrawString(mTaskDesc, mTaskDescRect, "middle_center");
				}
				if (mProgressDesc != string.Empty)
				{
					GuiSystem.DrawString(mProgressDesc, mProgressDescRect, "middle_center");
				}
				if (mQuestData.Status == QuestStatus.WAIT_COOLDOWN)
				{
					GuiSystem.DrawString(mCooldown, mCooldownRect, "middle_center");
				}
				if (mRewardQuestInfo != null)
				{
					mRewardQuestInfo.RenderElement();
				}
				if (mQuestData.Status == QuestStatus.IN_PROGRESS || mQuestData.Status == QuestStatus.DONE)
				{
					mCancelQuestButton.RenderElement();
					mShowQuestInfoButton.RenderElement();
				}
			}
		}

		public override void Update()
		{
			if (mQuestData == null || mQuestData.Status != 0)
			{
				return;
			}
			mDayStr = GuiSystem.GetLocaleText("Day_Text");
			mHourStr = GuiSystem.GetLocaleText("Hour_Text");
			mMinStr = GuiSystem.GetLocaleText("Min_Text");
			mSecStr = GuiSystem.GetLocaleText("Sec_Text");
			if (mQuestData.HasCooldownTime)
			{
				mCooldown = mCooldownText;
				TimeSpan timeSpan = mQuestData.CooldownExpireTime - DateTime.Now;
				if (timeSpan.Days > 0)
				{
					mCooldown = mCooldown + timeSpan.Days + mDayStr + " ";
				}
				if (timeSpan.Hours > 0 || (timeSpan.Hours == 0 && timeSpan.Days > 0))
				{
					mCooldown = mCooldown + timeSpan.Hours + mHourStr + " ";
				}
				if (timeSpan.Minutes > 0 || (timeSpan.Minutes == 0 && (timeSpan.Days > 0 || timeSpan.Hours > 0)))
				{
					mCooldown = mCooldown + timeSpan.Minutes + mMinStr + " ";
				}
				if (timeSpan.Seconds > 0 || (timeSpan.Seconds == 0 && (timeSpan.Days > 0 || timeSpan.Hours > 0 || timeSpan.Minutes > 0)))
				{
					mCooldown = mCooldown + timeSpan.Seconds + mSecStr;
				}
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mQuestData != null)
			{
				mRewardQuestInfo.CheckEvent(_curEvent);
				if (mQuestData.Status == QuestStatus.IN_PROGRESS || mQuestData.Status == QuestStatus.DONE)
				{
					mCancelQuestButton.CheckEvent(_curEvent);
					mShowQuestInfoButton.CheckEvent(_curEvent);
				}
			}
		}

		public void SetQuest(ISelfQuest _quest)
		{
			mQuestData = _quest;
			if (mQuestData == null)
			{
				return;
			}
			mCancelQuestButton.mId = mQuestData.Id;
			mShowQuestInfoButton.mId = mQuestData.Id;
			mCurFrame = GetQuestFrame(mQuestData.Status);
			mName = GuiSystem.GetLocaleText(mQuestData.Quest.Name);
			mRewardQuestInfo = new RewardQuestInfo();
			mRewardQuestInfo.Init();
			mRewardQuestInfo.SetData(mQuestData.Quest, mSelfHero, mFormatedTipMgr, _pvp: false);
			mTaskDesc = GuiSystem.GetLocaleText(mQuestData.Quest.TaskDesc);
			mProgressDesc = string.Empty;
			if (mQuestData.Status == QuestStatus.DONE || mQuestData.Status == QuestStatus.IN_PROGRESS)
			{
				int num = 0;
				foreach (IQuestProgress item in mQuestData.Progress)
				{
					if (num > 0)
					{
						mProgressDesc += "\n";
					}
					mProgressDesc += GuiSystem.GetLocaleText(item.Desc);
					if (item.MaxVal > 0)
					{
						string text = mProgressDesc;
						mProgressDesc = text + " " + item.CurVal + "/" + item.MaxVal;
					}
					num++;
				}
			}
			if (mQuestData.Status == QuestStatus.WAIT_COOLDOWN)
			{
				if (_quest.HasCooldownTime)
				{
					mCooldownText = GuiSystem.GetLocaleText("Quests_Cooldown_Text") + "\n";
				}
				else
				{
					mCooldown = (mCooldownText = GuiSystem.GetLocaleText("Quests_Cooldown_Finished_Text"));
				}
			}
			SetInfoSize(mQuestData.Status);
			SetRewardInfoSize(mQuestData.Status);
			SetQuestInfoButtonState();
		}

		public void SetData(SelfHero _selfHero, FormatedTipMgr _tipMgr)
		{
			mSelfHero = _selfHero;
			mFormatedTipMgr = _tipMgr;
		}

		public void SetQuestInfoButtonState()
		{
			if (OptionsMgr.IsQuestActive(mShowQuestInfoButton.mId))
			{
				mShowQuestInfoButton.mLabel = GuiSystem.GetLocaleText("Quests_Info_Hide_Text");
			}
			else
			{
				mShowQuestInfoButton.mLabel = GuiSystem.GetLocaleText("Quests_Info_Show_Text");
			}
		}

		private void SetRewardInfoSize(QuestStatus _status)
		{
			if (_status == QuestStatus.CLOSED || _status == QuestStatus.WAIT_COOLDOWN)
			{
				mRewardQuestInfo.mZoneRect = new Rect(11f, 153f, 317f, 127f);
			}
			else
			{
				mRewardQuestInfo.mZoneRect = new Rect(11f, 238f, 317f, 127f);
			}
			GuiSystem.SetChildRect(mZoneRect, ref mRewardQuestInfo.mZoneRect);
			mRewardQuestInfo.mExpStringRect = new Rect(92f, 58f, 214f, 16f);
			mRewardQuestInfo.mRewardDescRect = new Rect(92f, 76f, 214f, 16f);
			mRewardQuestInfo.mRewardStringRect = new Rect(9f, 6f, 300f, 16f);
			mRewardQuestInfo.mRewardItem.mZoneRect = new Rect(29f, 48f, 36f, 38f);
			if (mRewardQuestInfo.mRewardMoney != null)
			{
				mRewardQuestInfo.mRewardMoney.SetSize(mRewardQuestInfo.mZoneRect);
				mRewardQuestInfo.mRewardMoney.SetOffset(new Vector2(92f, 40f) * GuiSystem.mYRate);
			}
			mRewardQuestInfo.SetSize();
		}

		private void SetInfoSize(QuestStatus _status)
		{
			mTaskDescRect = new Rect(15f, 42f, 309f, 104f);
			mCooldownRect = new Rect(15f, 288f, 309f, 69f);
			GuiSystem.SetChildRect(mZoneRect, ref mTaskDescRect);
			GuiSystem.SetChildRect(mZoneRect, ref mCooldownRect);
		}

		private Texture2D GetQuestFrame(QuestStatus _status)
		{
			return _status switch
			{
				QuestStatus.CLOSED => mFrame2, 
				QuestStatus.WAIT_COOLDOWN => mFrame3, 
				_ => mFrame1, 
			};
		}
	}

	public delegate void OnQuestCallback(int _id);

	public delegate void VoidCallback();

	private IStoreContentProvider<ISelfQuest> mSelfQuestData;

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	public OnQuestCallback mOnQuestCancel;

	public VoidCallback mOnActiveQuestChanged;

	private GuiButton mQuestCurrentButton;

	private GuiButton mQuestExecutedButton;

	private QuestProcessType mCurQuestProcessType;

	private ISelfQuest mSelectedQuest;

	private SortedDictionary<QuestPvEType, QuestGroup> mQuestGroups;

	private QuestInfo mQuestInfo;

	private SelfHero mSelfHero;

	private FormatedTipMgr mFormatedTipMgr;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private Rect mGroupsZoneRect;

	private YesNoDialog mYesNoDialog;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(300, this);
		mQuestGroups = new SortedDictionary<QuestPvEType, QuestGroup>();
		mCurQuestProcessType = QuestProcessType.NONE;
		mQuestInfo = new QuestInfo();
		mQuestInfo.SetData(mSelfHero, mFormatedTipMgr);
		mQuestInfo.Init();
		GuiButton mCancelQuestButton = mQuestInfo.mCancelQuestButton;
		mCancelQuestButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mCancelQuestButton.mOnMouseUp, new OnMouseUp(OnButton));
		GuiButton mShowQuestInfoButton = mQuestInfo.mShowQuestInfoButton;
		mShowQuestInfoButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mShowQuestInfoButton.mOnMouseUp, new OnMouseUp(OnButton));
		mFrame = GuiSystem.GetImage("Gui/QuestJournal/frame1");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mQuestCurrentButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mQuestCurrentButton.mId = 1;
		mQuestCurrentButton.mElementId = "QUEST_PROCESS_TYPE_BUTTON";
		GuiButton guiButton2 = mQuestCurrentButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mQuestCurrentButton.Init();
		mQuestExecutedButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mQuestExecutedButton.mId = 2;
		mQuestExecutedButton.mElementId = "QUEST_PROCESS_TYPE_BUTTON";
		GuiButton guiButton3 = mQuestExecutedButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mQuestExecutedButton.Init();
		mQuestGroups.Add(QuestPvEType.SINGLE, new QuestGroup(QuestPvEType.SINGLE));
		mQuestGroups.Add(QuestPvEType.GROUP, new QuestGroup(QuestPvEType.GROUP));
		mQuestGroups.Add(QuestPvEType.REPLAY, new QuestGroup(QuestPvEType.REPLAY));
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.Init();
			GuiButton mGroupButton = mQuestGroup.Value.mGroupButton;
			mGroupButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mGroupButton.mOnMouseUp, new OnMouseUp(OnButton));
		}
		mYesNoDialog = new YesNoDialog();
		mYesNoDialog.Init();
		mYesNoDialog.SetActive(_active: false);
	}

	public override void SetActive(bool _active)
	{
		if (_active)
		{
			InitData();
		}
		base.SetActive(_active);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(654f, 6f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mQuestCurrentButton.mZoneRect = new Rect(18f, 40f, 116f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestCurrentButton.mZoneRect);
		mQuestExecutedButton.mZoneRect = new Rect(134f, 40f, 116f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestExecutedButton.mZoneRect);
		mScrollbar.mZoneRect = new Rect(285f, 75f, 22f, 405f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mGroupsZoneRect = new Rect(35f, 75f, 270f, 405f);
		GuiSystem.SetChildRect(mZoneRect, ref mGroupsZoneRect);
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.mZoneRect = mZoneRect;
			mQuestGroup.Value.mRenderZone = mGroupsZoneRect;
			mQuestGroup.Value.SetSize();
		}
		SetGroupOffsets();
		mQuestInfo.mZoneRect = new Rect(318f, 70f, 339f, 414f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestInfo.mZoneRect);
		mQuestInfo.SetSize();
		mYesNoDialog.SetSize();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		mCloseButton.RenderElement();
		mQuestCurrentButton.RenderElement();
		mQuestExecutedButton.RenderElement();
		mScrollbar.RenderElement();
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.RenderElement();
		}
		mQuestInfo.RenderElement();
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mQuestCurrentButton.CheckEvent(_curEvent);
		mQuestExecutedButton.CheckEvent(_curEvent);
		mScrollbar.CheckEvent(_curEvent);
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.CheckEvent(_curEvent);
		}
		mQuestInfo.CheckEvent(_curEvent);
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.CheckEvent(_curEvent);
		}
	}

	public override void Update()
	{
		mQuestInfo.Update();
	}

	public override void OnInput()
	{
		mScrollbar.OnInput();
	}

	public void SetData(IStoreContentProvider<ISelfQuest> _selfQuests, SelfHero _selfHero, FormatedTipMgr _tipMgr)
	{
		mSelfQuestData = _selfQuests;
		mSelfHero = _selfHero;
		mFormatedTipMgr = _tipMgr;
	}

	public void Clean()
	{
		mSelfQuestData = null;
		mSelfHero = null;
		mFormatedTipMgr = null;
	}

	public void SetSelectedQuest(int _id)
	{
		if (mSelectedQuest != null && _id == mSelectedQuest.Id)
		{
			return;
		}
		ISelfQuest selfQuest = mSelfQuestData.TryGet(_id);
		if (selfQuest == null)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			if (mQuestGroup.Value.HasQuest(_id))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SetQuestProcessTab((mCurQuestProcessType != QuestProcessType.CURRENT) ? QuestProcessType.CURRENT : QuestProcessType.EXECUTED);
		}
		SetSelectedQuest(selfQuest);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			SetActive(_active: false);
		}
		else if (_sender.mElementId == "QUEST_PROCESS_TYPE_BUTTON" && _buttonId == 0)
		{
			SetQuestProcessTab((QuestProcessType)_sender.mId);
		}
		else if (_sender.mElementId == "GROUP_BUTTON" && _buttonId == 0)
		{
			if (mQuestGroups.TryGetValue((QuestPvEType)_sender.mId, out var value))
			{
				value.SetGroupState(!value.GetGroupState());
				SetGroupOffsets();
			}
		}
		else if (_sender.mElementId == "QUEST_BUTTON" && _buttonId == 0)
		{
			SetSelectedQuest(mSelfQuestData.TryGet(_sender.mId));
		}
		else if (_sender.mElementId == "CANCEL_QUEST_BUTTON" && _buttonId == 0)
		{
			mYesNoDialog.SetData(GuiSystem.GetLocaleText("GUI_QUEST_CANCEL"), "YES_TEXT", "NO_TEXT", OnQuestCancel);
		}
		else if (_sender.mElementId == "SHOW_QUEST_INFO_BUTTON" && _buttonId == 0)
		{
			UserLog.AddAction(UserActionType.SHOW_HIDE_QUEST_INFO, _sender.mId, $"{GuiSystem.GetLocaleText(mSelectedQuest.Quest.Name)}:({mQuestInfo.mShowQuestInfoButton.mLabel})");
			OptionsMgr.SetActiveQuest(_sender.mId, !OptionsMgr.IsQuestActive(_sender.mId));
			mQuestInfo.SetQuestInfoButtonState();
			if (mOnActiveQuestChanged != null)
			{
				mOnActiveQuestChanged();
			}
		}
	}

	private void OnQuestCancel(bool _yes)
	{
		if (_yes && mOnQuestCancel != null)
		{
			UserLog.AddAction(UserActionType.QUEST_DECLINE, mSelectedQuest.Quest.Id, GuiSystem.GetLocaleText(mSelectedQuest.Quest.Name));
			mOnQuestCancel(mSelectedQuest.Id);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetGroupOffsets();
		}
	}

	private void SetGroupOffsets()
	{
		float num = 0f;
		float x = 43f;
		float num2 = 75f;
		float maxHeight = 405f;
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.SetOffset(new Vector2(x, num2 + num - mScrollOffset));
			mQuestGroup.Value.SetQuestsSize();
			num += mQuestGroup.Value.GetGroupLength();
		}
		mScrollbar.SetData(maxHeight, num);
	}

	private void InitData()
	{
		mQuestCurrentButton.mLabel = GuiSystem.GetLocaleText("Quests_Current_Text") + "(" + GetQuestCount(QuestProcessType.CURRENT) + ")";
		mQuestExecutedButton.mLabel = GuiSystem.GetLocaleText("Quests_Executed_Text") + "(" + GetQuestCount(QuestProcessType.EXECUTED) + ")";
		if (mCurQuestProcessType == QuestProcessType.NONE)
		{
			SetQuestProcessTab(QuestProcessType.CURRENT);
		}
		else
		{
			SetQuestProcessTab(mCurQuestProcessType);
		}
	}

	private void SetQuestProcessTab(QuestProcessType _type)
	{
		mScrollOffset = 0f;
		mScrollbar.Refresh();
		mCurQuestProcessType = _type;
		mQuestCurrentButton.Pressed = mCurQuestProcessType == QuestProcessType.CURRENT;
		mQuestExecutedButton.Pressed = mCurQuestProcessType == QuestProcessType.EXECUTED;
		SetSelectedQuest(null);
		InitQuestGroups(mCurQuestProcessType);
	}

	private void InitQuestGroups(QuestProcessType _type)
	{
		List<ISelfQuest> questsByProcessType = GetQuestsByProcessType(_type);
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.ClearQuests();
		}
		foreach (ISelfQuest item in questsByProcessType)
		{
			if (mQuestGroups.TryGetValue(item.Quest.PvEType, out var value))
			{
				value.AddQuest(item);
			}
		}
		int num = -1;
		if (mSelectedQuest == null)
		{
			foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup2 in mQuestGroups)
			{
				num = mQuestGroup2.Value.GetFirstQuest();
				if (num != -1)
				{
					SetSelectedQuest(mSelfQuestData.Get(num));
					break;
				}
			}
		}
		SetGroupOffsets();
	}

	private void SetSelectedQuest(ISelfQuest _selfQuestData)
	{
		if (_selfQuestData == mSelectedQuest)
		{
			return;
		}
		mSelectedQuest = _selfQuestData;
		foreach (KeyValuePair<QuestPvEType, QuestGroup> mQuestGroup in mQuestGroups)
		{
			mQuestGroup.Value.SetSelectedQuest((mSelectedQuest != null) ? mSelectedQuest.Id : (-1));
		}
		InitSelectedQuestInfo();
		SetGroupOffsets();
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.Clean();
		}
	}

	private void InitSelectedQuestInfo()
	{
		mQuestInfo.SetQuest(mSelectedQuest);
	}

	private int GetQuestCount(QuestProcessType _type)
	{
		return GetQuestsByProcessType(_type).Count;
	}

	private List<ISelfQuest> GetQuestsByProcessType(QuestProcessType _type)
	{
		List<ISelfQuest> list = new List<ISelfQuest>();
		if (mSelfQuestData == null)
		{
			return list;
		}
		int num = 0;
		foreach (ISelfQuest item in mSelfQuestData.Content)
		{
			if (item.Status == QuestStatus.CLOSED && _type == QuestProcessType.EXECUTED)
			{
				list.Add(item);
			}
			else if (item.Status != QuestStatus.CLOSED && _type == QuestProcessType.CURRENT)
			{
				list.Add(item);
			}
			num++;
		}
		return list;
	}
}
