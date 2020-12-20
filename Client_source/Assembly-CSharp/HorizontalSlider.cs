using TanatKernel;
using UnityEngine;

internal class HorizontalSlider : GuiElement
{
	public delegate void OnChangeVal(GuiElement _sender, float _newVal);

	public Texture2D mSliderTexture;

	public Texture2D mThumbNormImage;

	public Texture2D mThumbOverImage;

	public Texture2D mThumbPresImage;

	public GuiButton mThumb;

	public float mMinVal;

	public float mMaxVal;

	private float mCurVal;

	public bool mDragStart;

	public OnChangeVal mOnChangeVal;

	public HorizontalSlider(Texture2D _slider, Texture2D _thumbNorm, Texture2D _thumbOver, Texture2D _thumbPress)
	{
		mSliderTexture = _slider;
		mThumbNormImage = _thumbNorm;
		mThumbOverImage = _thumbOver;
		mThumbPresImage = _thumbPress;
	}

	public void SetParams(float _min, float _max, float _cur)
	{
		mMinVal = _min;
		mMaxVal = _max;
		mCurVal = _cur * (_min / _max);
	}

	public void SetCurValue(float _cur)
	{
		mCurVal = (_cur - mMinVal) / (mMaxVal - mMinVal);
	}

	public override void Init()
	{
		mThumb = new GuiButton();
		mThumb.mElementId = "SLIDER_THUMB";
		mThumb.mNormImg = mThumbNormImage;
		mThumb.mOverImg = mThumbOverImage;
		mThumb.mPressImg = mThumbPresImage;
		mThumb.Init();
	}

	public override void SetSize()
	{
		mThumb.mZoneRect = new Rect(0f, 0f, mThumb.mNormImg.width, mThumb.mNormImg.height);
		GuiSystem.GetRectScaled(ref mThumb.mZoneRect);
		mThumb.mZoneRect.y = mZoneRect.y + (mZoneRect.height - mThumb.mZoneRect.height) / 2f;
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (_curEvent.type == EventType.MouseDown && mZoneRect.Contains(_curEvent.mousePosition))
		{
			mDragStart = true;
			mCurVal = (_curEvent.mousePosition.x - mZoneRect.x) / mZoneRect.width;
			SendCurVal();
		}
		else if (_curEvent.type == EventType.MouseDrag && mDragStart)
		{
			mCurVal = (_curEvent.mousePosition.x - mZoneRect.x) / mZoneRect.width;
			mCurVal = ((!(mCurVal < 0f)) ? mCurVal : 0f);
			mCurVal = ((!(mCurVal > 1f)) ? mCurVal : 1f);
			SendCurVal();
		}
		else if (_curEvent.type == EventType.MouseUp)
		{
			RelaxDrag();
		}
		mThumb.CheckEvent(_curEvent);
	}

	private void SendCurVal()
	{
		if (mOnChangeVal != null)
		{
			float newVal = mMinVal + mCurVal * (mMaxVal - mMinVal);
			mOnChangeVal(this, newVal);
		}
	}

	private void RelaxDrag()
	{
		if (mDragStart)
		{
			float num = mMinVal + mCurVal * (mMaxVal - mMinVal);
			string comment = $"{GuiSystem.GetLocaleText(mElementId)}({num})";
			UserLog.AddAction(UserActionType.OPTIONS_SLIDER_CHANGED, comment);
		}
		mDragStart = false;
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mSliderTexture, mZoneRect, 11, 11, 3, 3);
		mThumb.RenderElement();
	}

	public override void Update()
	{
		SetSliderThumbPos();
	}

	private void SetSliderThumbPos()
	{
		mThumb.mZoneRect.x = mZoneRect.x + mCurVal * (mZoneRect.width - mThumb.mZoneRect.width);
	}
}
