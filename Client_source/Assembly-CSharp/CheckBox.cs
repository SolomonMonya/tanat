using System;
using TanatKernel;
using UnityEngine;

public class CheckBox : GuiElement
{
	public delegate void OnChecked(GuiElement _sender, bool _value);

	private bool mValue;

	private GuiButton mButton;

	private string mNormImgStr;

	private string mOverImgStr;

	private string mPressImgStr;

	private string mCheckImgStr;

	private Texture2D mCheckImg;

	public OnChecked mOnChecked;

	public override void Init()
	{
		mButton = GuiSystem.CreateButton(mNormImgStr, mOverImgStr, mPressImgStr, string.Empty, string.Empty);
		mButton.mElementId = "CHECKBOX_BUTTON";
		GuiButton guiButton = mButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mButton.Init();
		mCheckImg = GuiSystem.GetImage(mCheckImgStr);
	}

	public override void SetSize()
	{
		mButton.mZoneRect = mZoneRect;
	}

	public override void RenderElement()
	{
		mButton.RenderElement();
		if (mValue)
		{
			GuiSystem.DrawImage(mCheckImg, mZoneRect);
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void SetData(bool _value, string _normImg, string _overImg, string _pressImg, string _checkImg)
	{
		mValue = _value;
		mNormImgStr = _normImg;
		mOverImgStr = _overImg;
		mPressImgStr = _pressImg;
		mCheckImgStr = _checkImg;
	}

	public void SetValue(bool _value)
	{
		mValue = _value;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CHECKBOX_BUTTON" && _buttonId == 0)
		{
			mValue = !mValue;
			string comment = string.Format("{0}({1})", GuiSystem.GetLocaleText(mElementId), GuiSystem.GetLocaleText("LOG_CB_VALUE_" + mValue));
			UserLog.AddAction(UserActionType.OPTIONS_CHECK_BOX_CHANGED, comment);
			if (mOnChecked != null)
			{
				mOnChecked(this, mValue);
			}
		}
	}
}
