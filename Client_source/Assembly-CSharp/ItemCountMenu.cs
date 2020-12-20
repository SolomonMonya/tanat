using System;
using Log4Tanat;
using UnityEngine;

public class ItemCountMenu : GuiElement, EscapeListener
{
	private Texture2D mFrame;

	private Texture2D mCountFrame;

	private int mMinCnt;

	private int mMaxCnt;

	private int mCurCnt;

	private GuiButton mOkButton;

	private GuiButton mCancelButton;

	private Rect mCountRect;

	private Rect mCountTextRect;

	private string mCountText = string.Empty;

	private object mDataObject;

	private HorizontalSlider mCountSlider;

	private GuiInputMgr.ButtonDragFlags mDragFrom;

	private GuiInputMgr.ButtonDragFlags mDragTo;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/misc/frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(35, this);
		mOkButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mOkButton.mElementId = "OK_BUTTON";
		GuiButton guiButton = mOkButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mOkButton.mLabel = GuiSystem.GetLocaleText("Ok_Button_Name");
		mOkButton.Init();
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "CANCEL_BUTTON";
		GuiButton guiButton2 = mCancelButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.mLabel = GuiSystem.GetLocaleText("Cancel_Button_Name");
		mCancelButton.Init();
		mCountFrame = GuiSystem.GetImage("Gui/misc/el_02");
		mCountText = GuiSystem.GetLocaleText("COUNT_TEXT");
		Texture2D image = GuiSystem.GetImage("Gui/misc/el_01");
		Texture2D image2 = GuiSystem.GetImage("Gui/misc/el_02");
		mCountSlider = new HorizontalSlider(image2, image, image, image);
		mCountSlider.mElementId = "COUNT_SLIDER";
		HorizontalSlider horizontalSlider = mCountSlider;
		horizontalSlider.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnValueChange));
		mCountSlider.Init();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mOkButton.mZoneRect = new Rect(35f, 95f, 77f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mOkButton.mZoneRect);
		mCancelButton.mZoneRect = new Rect(133f, 95f, 77f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		mCountRect = new Rect(89f, 35f, 77f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCountRect);
		mCountTextRect = new Rect(80f, 10f, 95f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mCountTextRect);
		Rect _rect = new Rect(35f, 70f, 180f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		mCountSlider.mZoneRect = _rect;
		mCountSlider.SetSize();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mOkButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		mCountSlider.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Return)
		{
			OnMoveItems();
			SetActive(_active: false);
		}
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		mOkButton.RenderElement();
		mCancelButton.RenderElement();
		GuiSystem.DrawImage(mCountFrame, mCountRect, 10, 10, 0, 0);
		GuiSystem.DrawString(mCurCnt.ToString(), mCountRect, "middle_center");
		GuiSystem.DrawString(mCountText, mCountTextRect, "middle_center");
		mCountSlider.RenderElement();
	}

	public override void Update()
	{
		mCountSlider.Update();
	}

	public void SetData(int _minCnt, int _maxCnt, object _data, GuiInputMgr.ButtonDragFlags _from, GuiInputMgr.ButtonDragFlags _to)
	{
		SetActive(_active: true);
		mDragFrom = _from;
		mDragTo = _to;
		mMinCnt = _minCnt;
		mMaxCnt = _maxCnt;
		mCurCnt = mMinCnt;
		mDataObject = _data;
		mCountSlider.SetParams(mMinCnt, mMaxCnt, mCurCnt);
		if (mMinCnt == mMaxCnt)
		{
			OnMoveItems();
			SetActive(_active: false);
		}
	}

	private void OnValueChange(GuiElement _sender, float _newVal)
	{
		if (_sender.mElementId == "COUNT_SLIDER")
		{
			mCurCnt = Mathf.RoundToInt(_newVal);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ("OK_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			OnMoveItems();
			SetActive(_active: false);
		}
		else if ("CANCEL_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			SetActive(_active: false);
		}
	}

	private void OnMoveItems()
	{
		GuiInputMgr.DraggableButton draggableButton = mDataObject as GuiInputMgr.DraggableButton;
		if (draggableButton == null)
		{
			Log.Error("OnMoveItems button is null");
		}
		else
		{
			GuiSystem.mGuiInputMgr.AddItemToMove(draggableButton, mDragFrom, mDragTo, mCurCnt, _countAccept: false);
		}
	}
}
