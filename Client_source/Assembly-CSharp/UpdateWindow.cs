using System;
using UnityEngine;

public class UpdateWindow : GuiElement
{
	public delegate void CancelUpdateCallback();

	public CancelUpdateCallback mCancelUpdateCallback;

	private OkDialog mOkDialogWnd;

	public string mLocalVersion;

	public string mLastVersion;

	public string mPrepareString;

	public float mTransferedSize;

	public float mFullSize;

	public float mSpeed;

	public int mTime;

	private bool mCkecking = true;

	private Texture2D mFrame;

	private Texture2D mProgressLine;

	private Texture2D mBg;

	private string mVersionLocalText;

	private string mVersionLastText;

	private string mUpdateProgress;

	private string mUpdateSpeedText;

	private string mUpdateTimeText;

	private string mMbText;

	private string mSecText;

	private string mMbSecText;

	private GuiButton mCancelButton;

	private Rect mBgRect;

	private Rect mProgressRect;

	private Rect mLocalVersionRect;

	private Rect mLastVersionRect;

	private Rect mProgressTxtRect;

	private Rect mSpeedRect;

	private Rect mTimeRect;

	private Rect mMbRect;

	private Rect mSecRect;

	private Rect mMbSecRect;

	private Rect mPrepareRect;

	private float mProgress;

	public float Progress
	{
		get
		{
			return mProgress;
		}
		set
		{
			mProgress = Mathf.Clamp01(value);
		}
	}

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/UpdateWindow/up_01");
		mProgressLine = GuiSystem.GetImage("Gui/UpdateWindow/up_02");
		mBg = GuiSystem.GetImage("Gui/UpdateWindow/upd_background");
		mVersionLocalText = GuiSystem.GetLocaleText("GUI_UPD_VERSION_LOCAL");
		mVersionLastText = GuiSystem.GetLocaleText("GUI_UPD_VERSION_LAST");
		mUpdateProgress = GuiSystem.GetLocaleText("GUI_UPD_PROGRESS");
		mUpdateSpeedText = GuiSystem.GetLocaleText("GUI_UPD_SPEED");
		mUpdateTimeText = GuiSystem.GetLocaleText("GUI_UPD_TIME");
		mMbText = GuiSystem.GetLocaleText("GUI_UPD_MB");
		mSecText = GuiSystem.GetLocaleText("GUI_UPD_SEC");
		mMbSecText = GuiSystem.GetLocaleText("GUI_UPD_MB_SEC");
		mPrepareString = GuiSystem.GetLocaleText("GUI_UPD_PREPARE");
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "UPDATE_CANCEL_BUTTON";
		GuiButton guiButton = mCancelButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.mLabel = GuiSystem.GetLocaleText("Cancel_Button_Name");
		mCancelButton.Init();
		ScreenManager screenManager = GameObjUtil.FindObjectOfType<ScreenManager>();
		GuiSystem.GuiSet guiSet = screenManager.Gui.GetGuiSet("update_screen");
		mOkDialogWnd = guiSet.GetElementById<OkDialog>("OK_DAILOG");
	}

	public override void SetSize()
	{
		mBgRect = new Rect(0f, 0f, mBg.width, mBg.height);
		GuiSystem.GetRectScaled(ref mBgRect, _ignoreLowRate: true);
		mBgRect.x = ((float)OptionsMgr.mScreenWidth - mBgRect.width) / 2f;
		mZoneRect = new Rect(0f, 140f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect, _ignoreLowRate: true);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCancelButton.mZoneRect = new Rect(111f, 109f, 79f, 25f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		mProgressRect = new Rect(35f, 143f, 237f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mProgressRect);
		mLocalVersionRect = new Rect(62f, 44f, 65f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mLocalVersionRect);
		mLastVersionRect = new Rect(182f, 44f, 65f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mLastVersionRect);
		mProgressTxtRect = new Rect(40f, 61f, 180f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mProgressTxtRect);
		mMbRect = new Rect(230f, 61f, 50f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mMbRect);
		mSpeedRect = new Rect(40f, 76f, 180f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mSpeedRect);
		mMbSecRect = new Rect(230f, 76f, 50f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mMbSecRect);
		mTimeRect = new Rect(40f, 91f, 180f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
		mSecRect = new Rect(230f, 91f, 50f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mSecRect);
		mPrepareRect = new Rect(20f, 145f, 255f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mPrepareRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mBg, mBgRect);
		GuiSystem.DrawImage(mFrame, mZoneRect);
		Rect rect = mProgressRect;
		rect.width *= mProgress;
		GuiSystem.DrawImage(mProgressLine, rect);
		GuiSystem.DrawString(mUpdateProgress, mProgressTxtRect, "middle_left");
		GuiSystem.DrawString(mUpdateSpeedText, mSpeedRect, "middle_left");
		GuiSystem.DrawString(mUpdateTimeText, mTimeRect, "middle_left");
		GuiSystem.DrawString(mVersionLocalText, mLocalVersionRect, "middle_left");
		GuiSystem.DrawString(mVersionLastText, mLastVersionRect, "middle_left");
		GuiSystem.DrawString(mLocalVersion, mLocalVersionRect, "middle_right");
		GuiSystem.DrawString(mLastVersion, mLastVersionRect, "middle_right");
		GuiSystem.DrawString($"{mTransferedSize:0.##}" + "/" + $"{mFullSize:0.##}", mProgressTxtRect, "middle_right");
		GuiSystem.DrawString($"{mSpeed:0.00}", mSpeedRect, "middle_right");
		GuiSystem.DrawString(mTime.ToString(), mTimeRect, "middle_right");
		GuiSystem.DrawString(mMbText, mMbRect, "middle_left");
		GuiSystem.DrawString(mMbSecText, mMbSecRect, "middle_left");
		GuiSystem.DrawString(mSecText, mSecRect, "middle_left");
		if (mCkecking)
		{
			GuiSystem.DrawString(mPrepareString, mPrepareRect, "middle_center");
		}
		mCancelButton.RenderElement();
	}

	public void CheckVersion(bool _checking)
	{
		mCkecking = _checking;
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCancelButton.CheckEvent(_curEvent);
	}

	public void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "UPDATE_CANCEL_BUTTON" && _buttonId == 0 && mCancelUpdateCallback != null)
		{
			mCancelUpdateCallback();
		}
	}

	public void ShowError()
	{
		mOkDialogWnd.SetData(GuiSystem.GetLocaleText("GUI_UPD_ERROR"));
	}
}
