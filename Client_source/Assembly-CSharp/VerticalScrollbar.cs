using System;
using UnityEngine;

public class VerticalScrollbar : GuiElement
{
	public delegate void OnChangeVal(GuiElement _sender, float _offset);

	private Texture2D mLineImage;

	private GuiButton mUpButton;

	private GuiButton mDownButton;

	private GuiButton mThumbButton;

	private bool mNoScroll = true;

	private bool mDragStart;

	private float mMaxVal;

	private float mCurVal;

	private float mDeltaVal;

	private Color mLockedColor;

	public OnChangeVal mOnChangeVal;

	public override void Init()
	{
		mDeltaVal = 0.1f;
		mLockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		mLineImage = GuiSystem.GetImage("Gui/VerticalScrollbar/frame1");
		mUpButton = GuiSystem.CreateButton("Gui/VerticalScrollbar/button_1_norm", "Gui/VerticalScrollbar/button_1_over", "Gui/VerticalScrollbar/button_1_press", string.Empty, string.Empty);
		mUpButton.mElementId = "VERTICAL_SCROLLBAR_UP_BUTTON";
		GuiButton guiButton = mUpButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mUpButton.mLockedColor = mLockedColor;
		mUpButton.Init();
		mDownButton = GuiSystem.CreateButton("Gui/VerticalScrollbar/button_2_norm", "Gui/VerticalScrollbar/button_2_over", "Gui/VerticalScrollbar/button_2_press", string.Empty, string.Empty);
		mDownButton.mElementId = "VERTICAL_SCROLLBAR_DOWN_BUTTON";
		GuiButton guiButton2 = mDownButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mDownButton.mLockedColor = mLockedColor;
		mDownButton.Init();
		mThumbButton = GuiSystem.CreateButton("Gui/VerticalScrollbar/button_3_norm", "Gui/VerticalScrollbar/button_3_over", "Gui/VerticalScrollbar/button_3_press", string.Empty, string.Empty);
		mThumbButton.mElementId = "VERTICAL_SCROLLBAR_THUMB_BUTTON";
		GuiButton guiButton3 = mThumbButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mThumbButton.Init();
	}

	public override void SetSize()
	{
		mUpButton.mZoneRect = new Rect(0f, 0f, mUpButton.mNormImg.width, mUpButton.mNormImg.height);
		mDownButton.mZoneRect = new Rect(0f, 0f, mDownButton.mNormImg.width, mDownButton.mNormImg.height);
		mThumbButton.mZoneRect = new Rect(0f, 0f, mThumbButton.mNormImg.width, mThumbButton.mNormImg.height);
		GuiSystem.SetChildRect(mZoneRect, ref mUpButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDownButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mThumbButton.mZoneRect);
		mDownButton.mZoneRect.y = mZoneRect.y + mZoneRect.height - mDownButton.mZoneRect.height;
		mZoneRect.y += mUpButton.mZoneRect.height;
		mZoneRect.height -= mUpButton.mZoneRect.height + mDownButton.mZoneRect.height;
		SetThumbPos();
	}

	public override void RenderElement()
	{
		if (mNoScroll)
		{
			GuiSystem.DrawImage(mLineImage, mZoneRect, 4, 4, 4, 4, mLockedColor);
		}
		else
		{
			GuiSystem.DrawImage(mLineImage, mZoneRect, 4, 4, 4, 4);
		}
		mUpButton.RenderElement();
		mDownButton.RenderElement();
		if (!mNoScroll)
		{
			mThumbButton.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (!mNoScroll)
		{
			mUpButton.CheckEvent(_curEvent);
			mDownButton.CheckEvent(_curEvent);
			if (_curEvent.type == EventType.MouseDown && mZoneRect.Contains(_curEvent.mousePosition))
			{
				mDragStart = true;
				SetScrollValue(_curEvent.mousePosition);
			}
			else if (_curEvent.type == EventType.MouseDrag && mDragStart)
			{
				SetScrollValue(_curEvent.mousePosition);
			}
			else if (_curEvent.type == EventType.MouseUp)
			{
				mDragStart = false;
			}
			mThumbButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}
	}

	public void SetData(float _maxHeight, float _curHeight)
	{
		mNoScroll = _maxHeight >= _curHeight;
		if (!mNoScroll)
		{
			mMaxVal = _curHeight - _maxHeight;
		}
		else
		{
			mMaxVal = 0f;
		}
		mUpButton.mLocked = mNoScroll;
		mDownButton.mLocked = mNoScroll;
		mThumbButton.mLocked = mNoScroll;
	}

	public float GetValue()
	{
		return mCurVal * mMaxVal;
	}

	public void Refresh()
	{
		Refresh(_inverse: false);
	}

	public void Refresh(bool _inverse)
	{
		mCurVal = ((!_inverse) ? 0f : 1f);
		NormilizeCurVal();
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "VERTICAL_SCROLLBAR_UP_BUTTON" && _buttonId == 0)
		{
			mCurVal -= mDeltaVal;
			NormilizeCurVal();
		}
		else if (_sender.mElementId == "VERTICAL_SCROLLBAR_DOWN_BUTTON" && _buttonId == 0)
		{
			mCurVal += mDeltaVal;
			NormilizeCurVal();
		}
	}

	private void SetScrollValue(Vector2 _mousePositon)
	{
		mCurVal = (_mousePositon.y - mZoneRect.y) / mZoneRect.height;
		NormilizeCurVal();
	}

	private void NormilizeCurVal()
	{
		mCurVal = ((!(mCurVal < 0f)) ? mCurVal : 0f);
		mCurVal = ((!(mCurVal > 1f)) ? mCurVal : 1f);
		SetThumbPos();
		if (mOnChangeVal != null)
		{
			mOnChangeVal(this, GetValue());
		}
	}

	private void SetThumbPos()
	{
		mThumbButton.mZoneRect.y = mZoneRect.y + mCurVal * (mZoneRect.height - mThumbButton.mZoneRect.height);
	}
}
