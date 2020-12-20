using System;
using System.Collections.Generic;
using UnityEngine;

public class OkDialog : GuiElement, EscapeListener
{
	private class Question
	{
		public OnAnswer mAnsCallback;

		public string mText;
	}

	public delegate void OnAnswer();

	private OnAnswer mOnAnswer;

	private Texture2D mFrameImg;

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private GuiButton mOkButton;

	private string mQuestion = string.Empty;

	private Rect mQuestionRect;

	private Queue<Question> mQuestions = new Queue<Question>();

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			if (mOnAnswer != null)
			{
				mOnAnswer();
			}
			TryShowNext();
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(25, this);
		mFrame1 = GuiSystem.GetImage("Gui/misc/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/misc/frame2");
		mOkButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mOkButton.mElementId = "OK_BUTTON";
		GuiButton guiButton = mOkButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mOkButton.mLabel = GuiSystem.GetLocaleText("Ok_Button_Name");
		mOkButton.Init();
		AddTutorialElement(mOkButton.mElementId, mOkButton);
	}

	public override void SetSize()
	{
		SetDataSize(mQuestion);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		GuiSystem.DrawString(mQuestion, mQuestionRect, "middle_center");
		mOkButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mOkButton.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyUp && GuiSystem.InputIsFree() && _curEvent.keyCode == KeyCode.Return)
		{
			_curEvent.Use();
			if (mOnAnswer != null)
			{
				mOnAnswer();
			}
			TryShowNext();
			SetActive(_active: false);
		}
		base.CheckEvent(_curEvent);
	}

	private void SetDataSize(string _text)
	{
		if (!string.IsNullOrEmpty(_text))
		{
			if (_text.Length < 100)
			{
				mFrameImg = mFrame1;
				mOkButton.mZoneRect = new Rect(0f, 95f, 70f, 28f);
				mQuestionRect = new Rect(27f, 19f, 195f, 70f);
			}
			else
			{
				mFrameImg = mFrame2;
				mOkButton.mZoneRect = new Rect(0f, 150f, 73f, 28f);
				mQuestionRect = new Rect(27f, 31f, 234f, 111f);
			}
			mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
			mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
			GuiSystem.SetChildRect(mZoneRect, ref mOkButton.mZoneRect);
			mOkButton.mZoneRect.x = mZoneRect.x + (mZoneRect.width - mOkButton.mZoneRect.width) / 2f;
			GuiSystem.SetChildRect(mZoneRect, ref mQuestionRect);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && _sender.mElementId == "OK_BUTTON")
		{
			SetActive(_active: false);
			if (mOnAnswer != null)
			{
				mOnAnswer();
				mOnAnswer = null;
			}
			TryShowNext();
		}
	}

	private void TryShowNext()
	{
		Question question = null;
		lock (mQuestions)
		{
			if (mQuestions.Count > 0)
			{
				question = mQuestions.Dequeue();
			}
		}
		if (question != null)
		{
			mQuestion = question.mText;
			mOnAnswer = question.mAnsCallback;
			SetActive(_active: true);
		}
		SetDataSize(mQuestion);
	}

	public void SetData(string _text, OnAnswer _callback)
	{
		if (base.Active)
		{
			Question question = new Question();
			question.mText = _text;
			question.mAnsCallback = _callback;
			lock (mQuestions)
			{
				mQuestions.Enqueue(question);
			}
		}
		else
		{
			mQuestion = _text;
			mOnAnswer = _callback;
			SetActive(_active: true);
		}
		SetDataSize(mQuestion);
	}

	public void SetData(string _text)
	{
		SetData(_text, null);
	}

	public void Clean()
	{
		mOnAnswer = null;
		lock (mQuestions)
		{
			mQuestions.Clear();
		}
		SetActive(_active: false);
	}
}
