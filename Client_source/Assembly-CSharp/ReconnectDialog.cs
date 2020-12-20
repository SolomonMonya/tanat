using System;
using TanatKernel;
using UnityEngine;

public class ReconnectDialog : GuiElement
{
	public delegate void OnAnswer(bool _true);

	public OnAnswer mOnAnswer;

	private Texture2D mFrameImg;

	private GuiButton mYesButton;

	private GuiButton mNoButton;

	private string mQuestion;

	private Rect mQuestionRect;

	private Rect mTimeRect;

	private DateTime mEndTime;

	private string mTimeString;

	public override void Init()
	{
		mFrameImg = GuiSystem.GetImage("Gui/misc/frame1");
		mYesButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mYesButton.mElementId = "YES_BUTTON";
		GuiButton guiButton = mYesButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mYesButton.RegisterAction(UserActionType.RECONNECT);
		mYesButton.Init();
		mNoButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mNoButton.mElementId = "NO_BUTTON";
		GuiButton guiButton2 = mNoButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mNoButton.Init();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mYesButton.mZoneRect = new Rect(46f, 95f, 70f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mYesButton.mZoneRect);
		mNoButton.mZoneRect = new Rect(131f, 95f, 70f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mNoButton.mZoneRect);
		mQuestionRect = new Rect(27f, 8f, 195f, 70f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestionRect);
		mTimeRect = new Rect(100f, 60f, 50f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		GuiSystem.DrawString(mQuestion, mQuestionRect, "middle_center");
		mYesButton.RenderElement();
		mNoButton.RenderElement();
		TimeSpan timeSpan = mEndTime.Subtract(DateTime.Now);
		if (timeSpan.TotalSeconds > 0.0)
		{
			string text = timeSpan.Minutes.ToString();
			string text2 = $"{timeSpan.Seconds:00.#}";
			mTimeString = text + ":" + text2;
		}
		GuiSystem.DrawString(mTimeString, mTimeRect, "middle_center");
	}

	public override void CheckEvent(Event _curEvent)
	{
		mYesButton.CheckEvent(_curEvent);
		mNoButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void SetData(float _timer)
	{
		if (_timer < 0f)
		{
			mTimeString = GuiSystem.GetLocaleText("GUI_RECONNECT_TIME_UNCNOWN");
		}
		mEndTime = DateTime.Now.AddSeconds(_timer);
		mQuestion = GuiSystem.GetLocaleText("GUI_ASK_RECONNECT");
		mYesButton.mLabel = GuiSystem.GetLocaleText("YES_TEXT");
		mNoButton.mLabel = GuiSystem.GetLocaleText("NO_TEXT");
		SetActive(_active: true);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			SetActive(_active: false);
			if (mOnAnswer != null)
			{
				mOnAnswer(_sender.mElementId == "YES_BUTTON");
			}
		}
	}
}
