using System;
using System.Collections.Generic;
using UnityEngine;

public class YesNoDialog : GuiElement, EscapeListener
{
	private class Question
	{
		public OnAnswer mAnsCallback;

		public string mText;

		public string mYesText;

		public string mNoText;
	}

	public delegate void OnAnswer(bool _true);

	private OnAnswer mOnAnswer;

	private Texture2D mFrameImg;

	private GuiButton mYesButton;

	private GuiButton mNoButton;

	private string mQuestion;

	private Rect mQuestionRect;

	private string mCostText;

	private Rect mCostTextRect;

	private MoneyRenderer mMoneyRenderer;

	private Queue<Question> mQuestions = new Queue<Question>();

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			if (mOnAnswer != null)
			{
				mOnAnswer(_true: false);
			}
			SetActive(_active: false);
			TryShowNext();
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mFrameImg = GuiSystem.GetImage("Gui/misc/frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(5, this);
		mYesButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mYesButton.mElementId = "YES_BUTTON";
		GuiButton guiButton = mYesButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mYesButton.Init();
		mNoButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mNoButton.mElementId = "NO_BUTTON";
		GuiButton guiButton2 = mNoButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mNoButton.Init();
		AddTutorialElement(mNoButton.mElementId, mNoButton);
		AddTutorialElement(mYesButton.mElementId, mYesButton);
		mCostText = GuiSystem.GetLocaleText("Money_Upgrade_Text");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mYesButton.mZoneRect = new Rect(38f, 95f, 86f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mYesButton.mZoneRect);
		mNoButton.mZoneRect = new Rect(131f, 95f, 86f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mNoButton.mZoneRect);
		mQuestionRect = new Rect(27f, 19f, 195f, 70f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestionRect);
		mCostTextRect = new Rect(38f, 75f, 180f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mCostTextRect);
		if (mMoneyRenderer != null)
		{
			mMoneyRenderer.SetSize(mZoneRect);
			mMoneyRenderer.SetOffset(new Vector2(190f, 75f) * GuiSystem.mYRate);
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		GuiSystem.DrawString(mQuestion, mQuestionRect, "middle_center");
		mYesButton.RenderElement();
		mNoButton.RenderElement();
		if (mMoneyRenderer != null)
		{
			GuiSystem.DrawString(mCostText, mCostTextRect, "middle_left");
			mMoneyRenderer.Render();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mYesButton.CheckEvent(_curEvent);
		mNoButton.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyUp && GuiSystem.InputIsFree() && _curEvent.keyCode == KeyCode.Return)
		{
			_curEvent.Use();
			if (mOnAnswer != null)
			{
				mOnAnswer(_true: true);
				mOnAnswer = null;
			}
			SetActive(_active: false);
			TryShowNext();
		}
		base.CheckEvent(_curEvent);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			if (mOnAnswer != null)
			{
				mOnAnswer(_sender.mElementId == "YES_BUTTON");
				mOnAnswer = null;
			}
			SetActive(_active: false);
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
			mYesButton.mLabel = question.mYesText;
			mNoButton.mLabel = question.mNoText;
			mOnAnswer = question.mAnsCallback;
			SetActive(_active: true);
		}
	}

	public void SetData(string _text, string _yesId, string _noId, OnAnswer _callback)
	{
		string localeText = GuiSystem.GetLocaleText(_yesId);
		string localeText2 = GuiSystem.GetLocaleText(_noId);
		mMoneyRenderer = null;
		if (base.Active)
		{
			Question question = new Question();
			question.mAnsCallback = _callback;
			question.mText = _text;
			question.mYesText = localeText;
			question.mNoText = localeText2;
			lock (mQuestions)
			{
				mQuestions.Enqueue(question);
			}
		}
		else
		{
			mQuestion = _text;
			mYesButton.mLabel = localeText;
			mNoButton.mLabel = localeText2;
			mOnAnswer = _callback;
			SetActive(_active: true);
		}
	}

	public void SetMoneyData(int _money, bool _diamonds)
	{
		mMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, _diamonds);
		mMoneyRenderer.SetMoney(_money);
		mMoneyRenderer.SetSize(mZoneRect);
		mMoneyRenderer.SetOffset(new Vector2(190f, 75f) * GuiSystem.mYRate);
	}

	public void Clean()
	{
		mMoneyRenderer = null;
		mOnAnswer = null;
		lock (mQuestions)
		{
			mQuestions.Clear();
		}
		SetActive(_active: false);
	}
}
