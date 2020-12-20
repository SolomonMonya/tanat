using UnityEngine;

public class GuiButton : GuiElement
{
	public enum GuiButtonStates
	{
		BTN_NORM,
		BTN_OVER,
		BTN_PRESS
	}

	public Texture2D mCurImg;

	public Texture2D mNormImg;

	public Texture2D mOverImg;

	public Texture2D mPressImg;

	public Texture2D mIconImg;

	public Texture2D mEffectImg;

	public string mLabel;

	public string mClickSoundFile;

	public string mClickSoundPath;

	public Color mLockedColor;

	public Color mNormColor;

	public Color mOverColor;

	public Color mPressColor;

	public bool mIconOnTop = true;

	public GuiButtonStates mCurBtnState;

	public GuiInputMgr.DraggableButton mDraggableButton;

	public string mLabelStyle = string.Empty;

	public Rect mIconRect;

	private Color mCurColor;

	private bool mPressed;

	private bool mSignal;

	private float mMaxSignalTime;

	private float mLastSignalTime;

	private SoundSystem.GuiSound mClickSound;

	public bool Pressed
	{
		get
		{
			return mPressed;
		}
		set
		{
			mPressed = value;
			SetCurBtnImage();
		}
	}

	public GuiButton()
	{
		mType = GuiElementType.BUTTON;
		mCurImg = null;
		mNormImg = null;
		mOverImg = null;
		mPressImg = null;
		mIconImg = null;
		mEffectImg = null;
		mLabel = string.Empty;
		mOnMouseUp = null;
		mOnMouseDown = null;
		mOnMouseEnter = null;
		mOnMouseLeave = null;
		mOnDragStart = null;
		mDraggableButton = null;
		mId = -1;
		mIconRect = default(Rect);
		mLockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
		mNormColor = Color.clear;
		mOverColor = Color.clear;
		mPressColor = Color.clear;
		mCurColor = Color.clear;
		mLabelStyle = "middle_center";
		mCurBtnState = GuiButtonStates.BTN_NORM;
		mOnMouseEnter = MouseEnter;
		mOnMouseLeave = MouseLeave;
		mOnMouseDown = MouseDown;
		mOnMouseUp = MouseUp;
		mClickSoundFile = "click1";
		mClickSoundPath = "gui/";
	}

	public override void RenderElement()
	{
		if (!mLocked)
		{
			if (mCurColor != Color.clear)
			{
				GUI.contentColor = mCurColor;
			}
			if (mIconOnTop)
			{
				GuiSystem.DrawImage(mCurImg, mZoneRect);
			}
			if (null != mIconImg)
			{
				if (mIconRect.width == 0f || mIconRect.height == 0f)
				{
					GuiSystem.DrawImage(mIconImg, mZoneRect);
				}
				else
				{
					GuiSystem.DrawImage(mIconImg, mIconRect);
				}
			}
			if (!mIconOnTop)
			{
				GuiSystem.DrawImage(mCurImg, mZoneRect);
			}
		}
		else
		{
			if (mLockedColor != Color.clear)
			{
				GUI.contentColor = mLockedColor;
			}
			if (mIconOnTop)
			{
				GuiSystem.DrawImage(mCurImg, mZoneRect, mLockedColor);
			}
			if (null != mIconImg && null != mIconImg)
			{
				if (mIconRect.width == 0f || mIconRect.height == 0f)
				{
					GuiSystem.DrawImage(mIconImg, mZoneRect, mLockedColor);
				}
				else
				{
					GuiSystem.DrawImage(mIconImg, mIconRect, mLockedColor);
				}
			}
			if (!mIconOnTop)
			{
				GuiSystem.DrawImage(mCurImg, mZoneRect, mLockedColor);
			}
		}
		if (null != mEffectImg)
		{
			GuiSystem.DrawImage(mEffectImg, mZoneRect);
		}
		if (string.Empty != mLabel)
		{
			GuiSystem.DrawString(mLabel, mZoneRect, mLabelStyle);
		}
		GUI.contentColor = Color.white;
	}

	public override void Update()
	{
		if (mLocked && mCurBtnState != 0)
		{
			mCurBtnState = GuiButtonStates.BTN_NORM;
			SetCurBtnImage();
		}
		if (mSignal && !mLocked && !mPressed && Time.time - mLastSignalTime >= mMaxSignalTime)
		{
			mLastSignalTime = Time.time;
			mCurBtnState = ((mCurBtnState == GuiButtonStates.BTN_NORM) ? GuiButtonStates.BTN_OVER : GuiButtonStates.BTN_NORM);
			SetCurBtnImage();
		}
	}

	public override void Init()
	{
		mCurBtnState = GuiButtonStates.BTN_NORM;
		SetCurBtnImage();
		if (!string.IsNullOrEmpty(mClickSoundFile) && !string.IsNullOrEmpty(mClickSoundPath))
		{
			mClickSound = new SoundSystem.GuiSound();
			mClickSound.mName = mClickSoundFile;
			mClickSound.mPath = mClickSoundPath;
			mClickSound.mOptions = new SoundSystem.SoundOptions();
		}
		base.Init();
	}

	public void SetCurBtnImage()
	{
		if (!mPressed)
		{
			if (mCurBtnState == GuiButtonStates.BTN_NORM)
			{
				mCurImg = mNormImg;
				mCurColor = mNormColor;
			}
			else if (mCurBtnState == GuiButtonStates.BTN_OVER)
			{
				mCurImg = mOverImg;
				mCurColor = mOverColor;
			}
			else if (mCurBtnState == GuiButtonStates.BTN_PRESS)
			{
				mCurImg = mPressImg;
				mCurColor = mPressColor;
			}
		}
		else if (mPressed)
		{
			mCurImg = mOverImg;
			mCurColor = mOverColor;
		}
	}

	public void MouseUp(GuiElement _sender, int _buttonId)
	{
		if (!mLocked)
		{
			if (mClickSound != null)
			{
				SoundSystem.Instance.PlayGuiSound(mClickSound);
			}
			mCurBtnState = GuiButtonStates.BTN_NORM;
			SetCurBtnImage();
		}
	}

	public void MouseDown(GuiElement _sender, int _buttonId)
	{
		if (!mLocked)
		{
			mCurBtnState = GuiButtonStates.BTN_PRESS;
			SetCurBtnImage();
		}
	}

	public void MouseEnter(GuiElement _sender)
	{
		if (!mLocked)
		{
			mCurBtnState = GuiButtonStates.BTN_OVER;
			SetCurBtnImage();
		}
	}

	public void MouseLeave(GuiElement _sender)
	{
		if (!mLocked)
		{
			mCurBtnState = GuiButtonStates.BTN_NORM;
			SetCurBtnImage();
		}
	}

	public void Signal(float _time)
	{
		mLastSignalTime = 0f;
		mMaxSignalTime = _time;
		mSignal = true;
	}

	public void StopSignal()
	{
		mSignal = false;
		mMaxSignalTime = 0f;
		mLastSignalTime = 0f;
	}
}
