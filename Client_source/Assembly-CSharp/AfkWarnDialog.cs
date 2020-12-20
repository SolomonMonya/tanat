using System;
using UnityEngine;

public class AfkWarnDialog : GuiElement
{
	private Texture2D mFrameImg;

	private string mWarningText;

	private Rect mWarningRect;

	private Rect mTimeRect;

	private DateTime mEndTime;

	public override void Init()
	{
		mFrameImg = GuiSystem.GetImage("Gui/misc/frame1");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mWarningRect = new Rect(27f, 20f, 195f, 70f);
		GuiSystem.SetChildRect(mZoneRect, ref mWarningRect);
		mTimeRect = new Rect(100f, 80f, 50f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		GuiSystem.DrawString(mWarningText, mWarningRect, "middle_center");
		string @string = ((int)mEndTime.Subtract(DateTime.Now).TotalSeconds).ToString();
		GuiSystem.DrawString(@string, mTimeRect, "middle_center");
	}

	public override void CheckEvent(Event _curEvent)
	{
		base.CheckEvent(_curEvent);
	}

	public void SetData(float _timer, string _textId)
	{
		mEndTime = DateTime.Now.AddSeconds(_timer);
		mWarningText = GuiSystem.GetLocaleText(_textId);
		SetActive(_active: true);
	}
}
