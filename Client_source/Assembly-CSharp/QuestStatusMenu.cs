using System;
using TanatKernel;
using UnityEngine;

public class QuestStatusMenu : GuiElement, EscapeListener
{
	private Texture2D mCurFrame;

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private GuiButton mOkButton;

	private GuiButton mCloseButton;

	private string mLabel;

	private string mDesc;

	private string mTask;

	private Rect mDescRect;

	private Rect mTaskRect;

	private Rect mLabelRect;

	private SelfHero mSelfHero;

	private FormatedTipMgr mFormatedTipMgr;

	private ISelfPvpQuest mQuestData;

	private RewardQuestInfo mRewardQuestInfo;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(475, this);
		mFrame1 = GuiSystem.GetImage("Gui/QuestStatusMenu/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/QuestStatusMenu/frame2");
		mOkButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mOkButton.mElementId = "OK_BUTTON";
		GuiButton guiButton = mOkButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mOkButton.mLabel = GuiSystem.GetLocaleText("Ok_Button_Name");
		mOkButton.Init();
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton2 = mCloseButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
	}

	public override void SetSize()
	{
		if (mQuestData != null)
		{
			mZoneRect = new Rect(0f, 100f, mCurFrame.width, mCurFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
			mCloseButton.mZoneRect = new Rect(502f, 6f, 26f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
			if (mQuestData.State >= 0 || mQuestData.State == -3)
			{
				mOkButton.mZoneRect = new Rect(203f, 474f, 129f, 28f);
			}
			else
			{
				mOkButton.mZoneRect = new Rect(203f, 356f, 129f, 28f);
			}
			GuiSystem.SetChildRect(mZoneRect, ref mOkButton.mZoneRect);
			mDescRect = new Rect(28f, 52f, 480f, 215f);
			GuiSystem.SetChildRect(mZoneRect, ref mDescRect);
			mLabelRect = new Rect(25f, 8f, 486f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			mTaskRect = new Rect(28f, 278f, 480f, 61f);
			GuiSystem.SetChildRect(mZoneRect, ref mTaskRect);
			SetRewardInfoSize();
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mCurFrame, mZoneRect);
		GuiSystem.DrawString(mDesc, mDescRect, "middle_center");
		GuiSystem.DrawString(mTask, mTaskRect, "middle_center");
		GuiSystem.DrawString(mLabel, mLabelRect, "middle_center");
		if (mRewardQuestInfo != null)
		{
			mRewardQuestInfo.RenderElement();
		}
		mOkButton.RenderElement();
		mCloseButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mOkButton.CheckEvent(_curEvent);
		mCloseButton.CheckEvent(_curEvent);
		if (mRewardQuestInfo != null)
		{
			mRewardQuestInfo.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public void SetData(SelfHero _selfHero, FormatedTipMgr _tipMgr)
	{
		mSelfHero = _selfHero;
		mFormatedTipMgr = _tipMgr;
	}

	public void SetQuest(ISelfPvpQuest _questData)
	{
		mQuestData = _questData;
		if (mQuestData != null)
		{
			mTask = GuiSystem.GetLocaleText(mQuestData.Quest.TaskDesc);
			mCurFrame = ((mQuestData.State < 0 && mQuestData.State != -3) ? mFrame2 : mFrame1);
			if (mQuestData.State >= 0)
			{
				mLabel = GuiSystem.GetLocaleText(mQuestData.Quest.Name);
				mDesc = GuiSystem.GetLocaleText(mQuestData.Quest.StartDesc);
				mTask = GuiSystem.GetLocaleText("Quest_Target_Text") + mTask;
				mRewardQuestInfo = new RewardQuestInfo();
				mRewardQuestInfo.Init();
				mRewardQuestInfo.SetData(mQuestData.Quest, mSelfHero, mFormatedTipMgr, _pvp: true);
			}
			else if (mQuestData.State == -3)
			{
				mLabel = GuiSystem.GetLocaleText("Quest_End_Win_Text");
				mDesc = GuiSystem.GetLocaleText(mQuestData.Quest.WinDesc);
				mTask = GuiSystem.GetLocaleText("Quest_Win_Text") + mTask;
				mRewardQuestInfo = new RewardQuestInfo();
				mRewardQuestInfo.Init();
				mRewardQuestInfo.SetData(mQuestData.Quest, mSelfHero, mFormatedTipMgr, _pvp: true);
			}
			else if (mQuestData.State == -2)
			{
				mLabel = GuiSystem.GetLocaleText("Quest_End_Failed_Text");
				mDesc = GuiSystem.GetLocaleText(mQuestData.Quest.LoseDesc);
				mTask = GuiSystem.GetLocaleText("Quest_Failed_Text") + mTask;
				mRewardQuestInfo = null;
			}
			SetSize();
			SetRewardInfoSize();
		}
	}

	private void SetRewardInfoSize()
	{
		if (mRewardQuestInfo != null)
		{
			mRewardQuestInfo.mZoneRect = new Rect(24f, 347f, 487f, 117f);
			GuiSystem.SetChildRect(mZoneRect, ref mRewardQuestInfo.mZoneRect);
			mRewardQuestInfo.mRewardStringRect = new Rect(9f, 6f, 470f, 16f);
			mRewardQuestInfo.mRewardItem.mZoneRect = new Rect(23f, 43f, 36f, 38f);
			mRewardQuestInfo.mExpStringRect = new Rect(105f, 36f, 370f, 16f);
			mRewardQuestInfo.mRewardDescRect = new Rect(105f, 72f, 370f, 16f);
			if (mRewardQuestInfo.mRewardMoney != null)
			{
				mRewardQuestInfo.mRewardMoney.SetSize(mRewardQuestInfo.mZoneRect);
				mRewardQuestInfo.mRewardMoney.SetOffset(new Vector2(105f, 54f) * GuiSystem.mYRate);
			}
			mRewardQuestInfo.SetSize();
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" || _sender.mElementId == "OK_BUTTON")
		{
			SetActive(_active: false);
		}
	}
}
