using System;
using System.Collections.Generic;
using UnityEngine;

internal class ComboBox : GuiElement
{
	private GuiButton mButton;

	private Rect mBoxRect;

	private List<string> mData;

	private int mSelectedData;

	private Texture2D[] mButtonTex;

	private Texture2D mBgTexture;

	private Texture2D mFrameTexture;

	private GuiButton[] mDataButtons;

	private bool mOpened;

	public Texture2D mSelectionFrame;

	public bool mRenderFrame = true;

	public string mFontStyle = "middle_center";

	public ComboBox(List<string> _data, Texture2D[] _buttonTex, Texture2D _tex, Texture2D _frame)
	{
		mData = _data;
		mButtonTex = _buttonTex;
		mBgTexture = _tex;
		mFrameTexture = _frame;
	}

	public string GetSelectedData()
	{
		if (mData.Count > mSelectedData && mSelectedData >= 0)
		{
			return mData[mSelectedData];
		}
		return string.Empty;
	}

	public int GetSelectedDataId()
	{
		return mSelectedData;
	}

	public override void Init()
	{
		mButton = new GuiButton();
		mButton.mElementId = "COMBO_BOX_BUTTON";
		mButton.mNormImg = mButtonTex[0];
		mButton.mOverImg = mButtonTex[1];
		mButton.mPressImg = mButtonTex[2];
		GuiButton guiButton = mButton;
		guiButton.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton.mOnMouseDown, new OnMouseDown(OnButton));
		mButton.Init();
		mDataButtons = new GuiButton[mData.Count];
		for (int i = 0; i < mDataButtons.Length; i++)
		{
			mDataButtons[i] = new GuiButton();
			mDataButtons[i].mElementId = "DATA_BUTTON";
			mDataButtons[i].mId = i;
			mDataButtons[i].mLabel = mData[i];
			GuiButton obj = mDataButtons[i];
			obj.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj.mOnMouseDown, new OnMouseDown(OnButton));
			mDataButtons[i].mLabelStyle = mFontStyle;
			mDataButtons[i].mOverImg = mSelectionFrame;
			mDataButtons[i].Init();
		}
	}

	public override void SetSize()
	{
		Rect _curRect = new Rect(0f, 0f, 18f, 13f);
		GuiSystem.GetRectScaled(ref _curRect);
		mButton.mZoneRect = new Rect(mZoneRect.x + mZoneRect.width - _curRect.width - _curRect.width / 4f, mZoneRect.y + mZoneRect.height / 2f - _curRect.height / 2f, _curRect.width, _curRect.height);
		mBoxRect = new Rect(mZoneRect.x, mZoneRect.y + mZoneRect.height, mZoneRect.width, (float)mData.Count * mZoneRect.height);
		for (int i = 0; i < mDataButtons.Length; i++)
		{
			mDataButtons[i].mZoneRect = new Rect(mZoneRect.x, mZoneRect.y + (float)(i + 1) * mZoneRect.height, mZoneRect.width, mZoneRect.height);
		}
	}

	public override void RenderElement()
	{
		if (mRenderFrame)
		{
			GuiSystem.DrawImage(mFrameTexture, mZoneRect, 11, 11, 10, 10);
		}
		GuiSystem.DrawString(GetSelectedData(), mZoneRect, mFontStyle);
		mButton.RenderElement();
		if (mOpened)
		{
			GuiSystem.DrawImage(mBgTexture, mBoxRect, 8, 8, 8, 8);
			GuiButton[] array = mDataButtons;
			foreach (GuiButton guiButton in array)
			{
				guiButton.RenderElement();
			}
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (_curEvent.type == EventType.Used)
		{
			return;
		}
		mButton.CheckEvent(_curEvent);
		if (mOpened)
		{
			GuiButton[] array = mDataButtons;
			foreach (GuiButton guiButton in array)
			{
				guiButton.CheckEvent(_curEvent);
			}
			if (_curEvent.type == EventType.MouseDown)
			{
				ChangeState();
			}
			base.CheckEvent(_curEvent);
		}
	}

	public void SetSelectedData(int _dataNum)
	{
		if (_dataNum < mData.Count && _dataNum >= 0)
		{
			mSelectedData = _dataNum;
		}
	}

	public void OnButton(GuiElement _sender, int _button)
	{
		if (_sender.mElementId == "COMBO_BOX_BUTTON" && _button == 0)
		{
			ChangeState();
		}
		else if (_sender.mElementId == "DATA_BUTTON" && _button == 0)
		{
			ChangeState();
			SetSelectedData(_sender.mId);
			if (mOnMouseDown != null)
			{
				mOnMouseDown(_sender, _button);
			}
		}
	}

	private void ChangeState()
	{
		mOpened = !mOpened;
	}
}
