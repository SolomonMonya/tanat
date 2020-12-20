using UnityEngine;

public class TimerWarningDialog : GuiElement
{
	private Texture2D mFrameImg;

	private string mWarningText;

	private Rect mWarningRect;

	private Rect mTimeRect;

	private int mTime;

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
		string @string = mTime.ToString();
		GuiSystem.DrawString(@string, mTimeRect, "middle_center");
	}

	public override void CheckEvent(Event _curEvent)
	{
		base.CheckEvent(_curEvent);
	}

	public void SetData(int _timer, string _textId)
	{
		mTime = _timer;
		mWarningText = GuiSystem.GetLocaleText(_textId);
		SetActive(_active: true);
	}

	public void SetTimer(int _timer)
	{
		mTime = _timer;
	}
}
