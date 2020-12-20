using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class QuestMenu : GuiElement
{
	public delegate void OnQuestCallback(int _id);

	public delegate void OnCloseCallback();

	public OnQuestCallback mOnQuestAccept;

	public OnQuestCallback mOnQuestCancel;

	public OnQuestCallback mOnQuestDone;

	public OnCloseCallback mOnClose;

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private INpc mNPCData;

	private IQuest mQuestData;

	private ISelfQuest mSelfQuestData;

	private SelfHero mSelfHero;

	private FormatedTipMgr mFormatedTipMgr;

	private CtrlPrototype mRewardItemArticul;

	private Texture2D mNPCIcon;

	private Texture2D mQuestStateIcon;

	private string mQuestName;

	private string mNPCName;

	private string mQuestDesc;

	private string mQuestTask;

	private string mQuestReward;

	private Rect mNPCIconRect;

	private Rect mQuestStateIconRect;

	private Rect mQuestNameRect;

	private Rect mNPCNameRect;

	private Rect mQuestDescRect;

	private Rect mQuestTaskRect;

	private Rect mQuestRewardRect;

	private QuestStatus mQuestStatus;

	private GuiButton mOkButton;

	private GuiButton mCancelButton;

	private MoneyRenderer mRewardMoney;

	private GuiButton mRewardItem;

	private string mRewardItemCount;

	private string mExpString;

	private string mRewardDesc;

	private Rect mExpStringRect;

	private Rect mRewardDescRect;

	private YesNoDialog mYesNoDialog;

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/QuestMenu/frame");
		mQuestReward = GuiSystem.GetLocaleText("Quest_Reward_Text");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mOkButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mOkButton.mElementId = "OK_BUTTON";
		GuiButton guiButton2 = mOkButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mOkButton.Init();
		AddTutorialElement(mOkButton);
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "CANCEL_BUTTON";
		GuiButton guiButton3 = mCancelButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.Init();
		mRewardItem = new GuiButton();
		GuiButton guiButton4 = mRewardItem;
		guiButton4.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton4.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
		GuiButton guiButton5 = mRewardItem;
		guiButton5.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton5.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
		mRewardItem.Init();
		mYesNoDialog = new YesNoDialog();
		mYesNoDialog.Init();
		mYesNoDialog.SetActive(_active: false);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(654f, 6f, 26f, 26f);
		mOkButton.mZoneRect = new Rect(194f, 471f, 158f, 28f);
		mCancelButton.mZoneRect = new Rect(353f, 471f, 158f, 28f);
		mRewardItem.mZoneRect = new Rect(60f, 407f, 36f, 38f);
		mNPCIconRect = new Rect(24f, 44f, 52f, 52f);
		mQuestStateIconRect = new Rect(14f, 9f, 11f, 18f);
		mQuestNameRect = new Rect(34f, 9f, 620f, 16f);
		mNPCNameRect = new Rect(80f, 55f, 574f, 16f);
		mQuestDescRect = new Rect(45f, 125f, 598f, 176f);
		mQuestTaskRect = new Rect(45f, 320f, 594f, 38f);
		mQuestRewardRect = new Rect(33f, 373f, 620f, 16f);
		mExpStringRect = new Rect(144f, 417f, 507f, 16f);
		mRewardDescRect = new Rect(144f, 438f, 507f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mOkButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRewardItem.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNPCIconRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestStateIconRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestNameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNPCNameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestDescRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestTaskRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestRewardRect);
		GuiSystem.SetChildRect(mZoneRect, ref mExpStringRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRewardDescRect);
		mYesNoDialog.SetSize();
		SetMoneySize();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		if (mQuestStateIcon != null)
		{
			GuiSystem.DrawImage(mQuestStateIcon, mQuestStateIconRect);
		}
		GuiSystem.DrawImage(mNPCIcon, mNPCIconRect);
		GuiSystem.DrawString(mQuestReward, mQuestRewardRect, "middle_center");
		GuiSystem.DrawString(mQuestName, mQuestNameRect, "middle_center");
		GuiSystem.DrawString(mNPCName, mNPCNameRect, "middle_center");
		GuiSystem.DrawString(mQuestDesc, mQuestDescRect, "upper_left");
		GuiSystem.DrawString(mQuestTask, mQuestTaskRect, "upper_left");
		if (mRewardDesc != string.Empty)
		{
			GuiSystem.DrawString(mRewardDesc, mRewardDescRect, "middle_left");
		}
		if (mExpString != string.Empty)
		{
			GuiSystem.DrawString(mExpString, mExpStringRect, "middle_left");
		}
		if (mRewardMoney != null)
		{
			mRewardMoney.Render();
		}
		mCloseButton.RenderElement();
		mOkButton.RenderElement();
		mCancelButton.RenderElement();
		mRewardItem.RenderElement();
		GuiSystem.DrawString(mRewardItemCount, mRewardItem.mZoneRect, "lower_right");
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mOkButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		mRewardItem.CheckEvent(_curEvent);
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public override void SetActive(bool _active)
	{
		base.SetActive(_active);
		if (!_active && mOnClose != null)
		{
			mOnClose();
		}
	}

	public void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			SetActive(_active: false);
		}
		else if (_sender.mElementId == "OK_BUTTON" && _buttonId == 0)
		{
			if (mQuestStatus == QuestStatus.EXIST || mQuestStatus == QuestStatus.WAIT_COOLDOWN)
			{
				UserLog.AddAction(UserActionType.QUEST_ACCEPT, mQuestData.Id, GuiSystem.GetLocaleText(mQuestData.Name));
				if (mOnQuestAccept != null)
				{
					mOnQuestAccept(mQuestData.Id);
				}
				SetActive(_active: false);
			}
			else if (mQuestStatus == QuestStatus.IN_PROGRESS)
			{
				SetActive(_active: false);
			}
			else if (mQuestStatus == QuestStatus.DONE)
			{
				UserLog.AddAction(UserActionType.QUEST_DONE, mQuestData.Id, GuiSystem.GetLocaleText(mQuestData.Name));
				if (mOnQuestDone != null)
				{
					mOnQuestDone(mQuestData.Id);
				}
				SetActive(_active: false);
			}
			else if (mQuestStatus == QuestStatus.FAKE)
			{
				base.SetActive(_active: false);
			}
		}
		else if (_sender.mElementId == "CANCEL_BUTTON" && _buttonId == 0)
		{
			if (mQuestStatus == QuestStatus.IN_PROGRESS)
			{
				mYesNoDialog.SetData(GuiSystem.GetLocaleText("GUI_QUEST_CANCEL"), "YES_TEXT", "NO_TEXT", OnQuestCancel);
			}
			else
			{
				SetActive(_active: false);
			}
		}
	}

	public void SetData(INpc _npc, IQuest _quest, ISelfQuest _selfQuest, SelfHero _selfHero, FormatedTipMgr _tipMgr)
	{
		mNPCData = _npc;
		mQuestData = _quest;
		mSelfQuestData = _selfQuest;
		mSelfHero = _selfHero;
		mFormatedTipMgr = _tipMgr;
		InitNPCData();
		InitQuestData();
		InitButtonData();
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.Clean();
		}
	}

	public void SetFakeStatus()
	{
		mQuestStatus = QuestStatus.FAKE;
	}

	private void OnQuestCancel(bool _yes)
	{
		if (_yes && mOnQuestCancel != null)
		{
			UserLog.AddAction(UserActionType.QUEST_DECLINE, mQuestData.Id, GuiSystem.GetLocaleText(mQuestData.Name));
			mOnQuestCancel(mQuestData.Id);
		}
		SetActive(_active: false);
	}

	private void InitNPCData()
	{
		mNPCIcon = GuiSystem.GetImage("Gui/NPCMenu/Icons/" + mNPCData.Icon);
		mQuestName = GuiSystem.GetLocaleText(mQuestData.Name);
		mNPCName = GuiSystem.GetLocaleText(mNPCData.Name);
	}

	private void InitQuestData()
	{
		if (mSelfQuestData != null)
		{
			mQuestStatus = mSelfQuestData.Status;
		}
		else
		{
			mQuestStatus = QuestStatus.EXIST;
		}
		mQuestStateIcon = NPCMenu.GetStatusImage(mQuestStatus);
		mQuestTask = GuiSystem.GetLocaleText(mQuestData.TaskDesc);
		if (mQuestStatus == QuestStatus.EXIST || mQuestStatus == QuestStatus.WAIT_COOLDOWN)
		{
			mQuestDesc = GuiSystem.GetLocaleText(mQuestData.StartDesc);
		}
		else if (mQuestStatus == QuestStatus.IN_PROGRESS)
		{
			mQuestDesc = GuiSystem.GetLocaleText(mQuestData.InProgressDesc);
		}
		else if (mQuestStatus == QuestStatus.DONE)
		{
			mQuestDesc = GuiSystem.GetLocaleText(mQuestData.WinDesc);
		}
		if (mQuestData.RewardDesc != null && mQuestData.RewardDesc.Length > 0)
		{
			mRewardDesc = GuiSystem.GetLocaleText(mQuestData.RewardDesc);
		}
		if (mQuestData.Money > 0)
		{
			mRewardMoney = new MoneyRenderer(_renderMoneyImage: true, mQuestData.MoneyCurrency == Currency.REAL);
			mRewardMoney.SetMoney(mQuestData.Money);
			SetMoneySize();
		}
		if (mQuestData.Exp != 0)
		{
			mExpString = mQuestData.Exp + " " + GuiSystem.GetLocaleText("Quest_Reward_Exp_Text");
		}
		mRewardItemCount = string.Empty;
		mRewardItemArticul = null;
		mRewardItem.mIconImg = null;
		ICollection<IQuestReward> collection;
		if (mSelfHero.Hero.View.mRace == 1)
		{
			ICollection<IQuestReward> rewardsHuman = mQuestData.RewardsHuman;
			collection = rewardsHuman;
		}
		else
		{
			collection = mQuestData.RewardsElf;
		}
		ICollection<IQuestReward> collection2 = collection;
		using IEnumerator<IQuestReward> enumerator = collection2.GetEnumerator();
		if (enumerator.MoveNext())
		{
			IQuestReward current = enumerator.Current;
			mRewardItemArticul = current.ArticleProto;
			mRewardItem.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + current.ArticleProto.Desc.mIcon);
			mRewardItem.mId = current.ArticleProto.Id;
			mRewardItemCount = current.Count.ToString();
		}
	}

	private void SetMoneySize()
	{
		if (mRewardMoney != null)
		{
			mRewardMoney.SetSize(mZoneRect);
			mRewardMoney.SetOffset(new Vector2(146f, 399f) * GuiSystem.mYRate);
		}
	}

	private void InitButtonData()
	{
		if (mQuestStatus == QuestStatus.EXIST || mQuestStatus == QuestStatus.WAIT_COOLDOWN)
		{
			mOkButton.mLabel = GuiSystem.GetLocaleText("Quest_Accept_Text");
			mCancelButton.mLabel = GuiSystem.GetLocaleText("Quest_Delay_Text");
		}
		else if (mQuestStatus == QuestStatus.IN_PROGRESS)
		{
			mOkButton.mLabel = GuiSystem.GetLocaleText("Quest_Progress_Text");
			mCancelButton.mLabel = GuiSystem.GetLocaleText("Quest_Cancel_Text");
		}
		else if (mQuestStatus == QuestStatus.DONE)
		{
			mOkButton.mLabel = GuiSystem.GetLocaleText("Quest_Done_Text");
			mCancelButton.mLabel = GuiSystem.GetLocaleText("Quest_Done_Delay_Text");
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && mRewardItemArticul != null)
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
