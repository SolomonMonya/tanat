using System;
using UnityEngine;

public class CurseInfo : GuiElement, EscapeListener
{
	private Texture2D mFrame;

	private Texture2D mIcon;

	private string mText;

	private GuiButton mCloseButton;

	private Rect mIconRect;

	private Rect mTextRect;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(600, this);
		mFrame = GuiSystem.GetImage("Gui/CurseInfo/frame1");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		mCloseButton.mLabel = GuiSystem.GetLocaleText("Close_Button_Name");
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mCloseButton.mZoneRect = new Rect(64f, 140f, 102f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mIconRect = new Rect(95f, 73f, 38f, 38f);
		GuiSystem.SetChildRect(mZoneRect, ref mIconRect);
		mTextRect = new Rect(21f, 24f, 182f, 42f);
		GuiSystem.SetChildRect(mZoneRect, ref mTextRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GuiSystem.DrawImage(mIcon, mIconRect);
		GuiSystem.DrawString(mText, mTextRect, "middle_center");
		mCloseButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void SetData(string _textId, string _iconId)
	{
		mText = GuiSystem.GetLocaleText(_textId);
		mIcon = GuiSystem.GetImage(_iconId);
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
	}
}
