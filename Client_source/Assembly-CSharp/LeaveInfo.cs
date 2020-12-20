using System;
using TanatKernel;
using UnityEngine;

public class LeaveInfo : GuiElement, EscapeListener
{
	public enum Type
	{
		FROM_GAME,
		FROM_BATTLE
	}

	public delegate void OnAccept(Type _type);

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private GuiButton mAcceptButton;

	private GuiButton mCancelButton;

	private UserLeaveInfoArg mData;

	private Type mExitType;

	private string mLabel1;

	private string mLabel2;

	private string mInfo1;

	private string mInfo2;

	private string mCurKarma;

	private string mNewKarma;

	private string mLabels;

	private string mPenaltyLabelsText;

	private string mPenaltyLabelsValue;

	private string mPenaltyTimeText;

	private string mPenaltyTimeValue;

	private Rect mLabel1Rect;

	private Rect mLabel2Rect;

	private Rect mInfo1Rect;

	private Rect mInfo2Rect;

	private Rect mCurKarmaRect;

	private Rect mNewKarmaRect;

	private Rect mLabelsRect;

	private Rect mPenaltyLabelsRect;

	private Rect mPenaltyTimeRect;

	public OnAccept mOnAccept;

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
		mFrame = GuiSystem.GetImage("Gui/LeaveInfo/frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(550, this);
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mAcceptButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mAcceptButton.mElementId = "ACCEPT_BUTTON";
		GuiButton guiButton2 = mAcceptButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mAcceptButton.mLabel = GuiSystem.GetLocaleText("Accep_Button_Name");
		mAcceptButton.RegisterAction(UserActionType.PLAYER_LEAVE);
		mAcceptButton.Init();
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "CANCEL_BUTTON";
		GuiButton guiButton3 = mCancelButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.mLabel = GuiSystem.GetLocaleText("Cancel_Button_Name");
		mCancelButton.Init();
		mLabel1 = GuiSystem.GetLocaleText("LeaveInfo_Label1_Text");
		mLabel2 = GuiSystem.GetLocaleText("LeaveInfo_Label2_Text");
		mInfo1 = GuiSystem.GetLocaleText("LeaveInfo_Info1_Text");
		mInfo2 = GuiSystem.GetLocaleText("LeaveInfo_Info2_Text");
		mPenaltyLabelsText = GuiSystem.GetLocaleText("LeaveInfo_PenaltyLabels_Text");
		mPenaltyTimeText = GuiSystem.GetLocaleText("LeaveInfo_PenaltyTime_Text");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 100f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(408f, 6f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mAcceptButton.mZoneRect = new Rect(117f, 534f, 96f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mAcceptButton.mZoneRect);
		mCancelButton.mZoneRect = new Rect(230f, 534f, 96f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		mLabel1Rect = new Rect(34f, 4f, 375f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel1Rect);
		mLabel2Rect = new Rect(34f, 48f, 375f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel2Rect);
		mInfo1Rect = new Rect(79f, 239f, 285f, 45f);
		GuiSystem.SetChildRect(mZoneRect, ref mInfo1Rect);
		mInfo2Rect = new Rect(36f, 336f, 369f, 84f);
		GuiSystem.SetChildRect(mZoneRect, ref mInfo2Rect);
		mCurKarmaRect = new Rect(40f, 292f, 360f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mCurKarmaRect);
		mNewKarmaRect = new Rect(40f, 314f, 360f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mNewKarmaRect);
		mLabelsRect = new Rect(37f, 424f, 367f, 22f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelsRect);
		mPenaltyLabelsRect = new Rect(55f, 457f, 332f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mPenaltyLabelsRect);
		mPenaltyTimeRect = new Rect(55f, 480f, 332f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mPenaltyTimeRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		mCloseButton.RenderElement();
		mAcceptButton.RenderElement();
		mCancelButton.RenderElement();
		GuiSystem.DrawString(mLabel1, mLabel1Rect, "label");
		GuiSystem.DrawString(mLabel2, mLabel2Rect, "label");
		GuiSystem.DrawString(mCurKarma, mCurKarmaRect, "label");
		GuiSystem.DrawString(mPenaltyLabelsText, mPenaltyLabelsRect, "middle_left");
		GuiSystem.DrawString(mPenaltyLabelsValue, mPenaltyLabelsRect, "middle_right");
		GuiSystem.DrawString(mPenaltyTimeText, mPenaltyTimeRect, "middle_left");
		GuiSystem.DrawString(mPenaltyTimeValue, mPenaltyTimeRect, "middle_right");
		GUI.contentColor = Color.red;
		GuiSystem.DrawString(mInfo1, mInfo1Rect, "label");
		GuiSystem.DrawString(mInfo2, mInfo2Rect, "label");
		GuiSystem.DrawString(mNewKarma, mNewKarmaRect, "label");
		GuiSystem.DrawString(mLabels, mLabelsRect, "middle_center");
		GUI.contentColor = Color.white;
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mAcceptButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void SetData(UserLeaveInfoArg _data, Type _type)
	{
		mData = _data;
		mExitType = _type;
		if (mData != null)
		{
			mCurKarma = GuiSystem.GetLocaleText("LeaveInfo_CurKarma_Text");
			mCurKarma = mCurKarma.Replace("{VALUE}", mData.mCurrentKarma.ToString());
			mNewKarma = GuiSystem.GetLocaleText("LeaveInfo_NewKarma_Text");
			mNewKarma = mNewKarma.Replace("{VALUE}", mData.mNewKarma.ToString());
			mLabels = GuiSystem.GetLocaleText("LeaveInfo_Labels_Text");
			mLabels = mLabels.Replace("{VALUE}", mData.mLabels.ToString());
			mPenaltyTimeValue = mData.mTime + " " + GuiSystem.GetLocaleText("Hour_Text");
			mPenaltyLabelsValue = mData.mLabelsLimit + " " + GuiSystem.GetLocaleText("LeaveInfo_Penalty_Text");
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ((_sender.mElementId == "CLOSE_BUTTON" || _sender.mElementId == "CANCEL_BUTTON") && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "ACCEPT_BUTTON" && _buttonId == 0 && mOnAccept != null)
		{
			mOnAccept(mExitType);
		}
	}
}
