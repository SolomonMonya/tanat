using System;
using UnityEngine;

internal class SelectRaceWindow : GuiElement
{
	public delegate void RaceSelected(HeroRace _race);

	public RaceSelected mRaceSelected;

	private GuiButton mSelectHumanButton;

	private GuiButton mSelectElfButton;

	private Texture2D mSelectFrame;

	private Texture2D mInfoFrame;

	private string mInfoHumanText;

	private string mInfoElfText;

	private Rect mSelectFrameRect;

	private Rect mInfoHumanFrameRect;

	private Rect mInfoElfFrameRect;

	private Rect mInfoHumanTextRect;

	private Rect mInfoElfTextRect;

	private string mElfText;

	private string mHumanText;

	private Rect mElfTextRect;

	private Rect mHumanTextRect;

	public override void Init()
	{
		mSelectFrame = GuiSystem.GetImage("Gui/SelectRaceWindow/frame1");
		mInfoFrame = GuiSystem.GetImage("Gui/SelectRaceWindow/frame2");
		mSelectHumanButton = GuiSystem.CreateButton("Gui/SelectRaceWindow/button_1_norm", "Gui/SelectRaceWindow/button_1_over", "Gui/SelectRaceWindow/button_1_press", string.Empty, string.Empty);
		mSelectHumanButton.mElementId = "SELECT_RACE_BUTTON";
		mSelectHumanButton.mId = 1;
		GuiButton guiButton = mSelectHumanButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mSelectHumanButton.Init();
		mSelectElfButton = GuiSystem.CreateButton("Gui/SelectRaceWindow/button_1_norm", "Gui/SelectRaceWindow/button_1_over", "Gui/SelectRaceWindow/button_1_press", string.Empty, string.Empty);
		mSelectElfButton.mElementId = "SELECT_RACE_BUTTON";
		mSelectElfButton.mId = 2;
		GuiButton guiButton2 = mSelectElfButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mSelectElfButton.Init();
		mInfoHumanText = GuiSystem.GetLocaleText("Race_Desc_" + 1);
		mInfoElfText = GuiSystem.GetLocaleText("Race_Desc_" + 2);
		mHumanText = GuiSystem.GetLocaleText("Race_Name_" + 1);
		mElfText = GuiSystem.GetLocaleText("Race_Name_" + 2);
	}

	public override void SetSize()
	{
		mSelectFrameRect = new Rect(0f, 24f, mSelectFrame.width, mSelectFrame.height);
		GuiSystem.GetRectScaled(ref mSelectFrameRect);
		mSelectFrameRect.x = ((float)OptionsMgr.mScreenWidth - mSelectFrameRect.width) / 2f;
		mInfoHumanFrameRect = new Rect(0f, 604f, mInfoFrame.width, mInfoFrame.height);
		GuiSystem.GetRectScaled(ref mInfoHumanFrameRect);
		mInfoHumanFrameRect.x = (float)OptionsMgr.mScreenWidth / 2f - mInfoHumanFrameRect.width - 145f * GuiSystem.mYRate;
		mInfoElfFrameRect = new Rect(0f, 604f, mInfoFrame.width, mInfoFrame.height);
		GuiSystem.GetRectScaled(ref mInfoElfFrameRect);
		mInfoElfFrameRect.x = (float)OptionsMgr.mScreenWidth / 2f + 145f * GuiSystem.mYRate;
		mSelectHumanButton.mZoneRect = new Rect(110f, 287f, 154f, 46f);
		GuiSystem.SetChildRect(mInfoHumanFrameRect, ref mSelectHumanButton.mZoneRect);
		mSelectElfButton.mZoneRect = new Rect(110f, 287f, 154f, 46f);
		GuiSystem.SetChildRect(mInfoElfFrameRect, ref mSelectElfButton.mZoneRect);
		mInfoHumanTextRect = new Rect(22f, 50f, 307f, 231f);
		GuiSystem.SetChildRect(mInfoHumanFrameRect, ref mInfoHumanTextRect);
		mInfoElfTextRect = new Rect(22f, 50f, 307f, 231f);
		GuiSystem.SetChildRect(mInfoElfFrameRect, ref mInfoElfTextRect);
		mHumanTextRect = new Rect(25f, 13f, 300f, 16f);
		GuiSystem.SetChildRect(mInfoHumanFrameRect, ref mHumanTextRect);
		mElfTextRect = new Rect(25f, 13f, 300f, 16f);
		GuiSystem.SetChildRect(mInfoElfFrameRect, ref mElfTextRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mSelectFrame, mSelectFrameRect);
		GuiSystem.DrawImage(mInfoFrame, mInfoHumanFrameRect);
		GuiSystem.DrawImage(mInfoFrame, mInfoElfFrameRect);
		GuiSystem.DrawString(mInfoHumanText, mInfoHumanTextRect, "middle_center");
		GuiSystem.DrawString(mInfoElfText, mInfoElfTextRect, "middle_center");
		GuiSystem.DrawString(mHumanText, mHumanTextRect, "label");
		GuiSystem.DrawString(mElfText, mElfTextRect, "label");
		mSelectHumanButton.RenderElement();
		mSelectElfButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mSelectHumanButton.CheckEvent(_curEvent);
		mSelectElfButton.CheckEvent(_curEvent);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "SELECT_RACE_BUTTON" && _buttonId == 0 && mRaceSelected != null)
		{
			mRaceSelected((HeroRace)_sender.mId);
		}
	}
}
