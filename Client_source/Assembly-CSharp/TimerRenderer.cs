using UnityEngine;

public class TimerRenderer : GuiElement
{
	private Texture2D mCurImage;

	private Rect mCurImageRect;

	private Texture2D[] mTimerImages;

	private bool mStarted;

	private int mCurPart;

	private int mMaxPart;

	private float mStartTime;

	private float mStartScaleTime;

	private Color mCurColor;

	private SoundSystem.GuiSound mTimeSound;

	public bool IsStarted => mStarted;

	public override void Init()
	{
		mTimerImages = new Texture2D[5];
		mTimerImages[0] = GuiSystem.GetImage("Gui/SelectAvatar/timer/5");
		mTimerImages[1] = GuiSystem.GetImage("Gui/SelectAvatar/timer/4");
		mTimerImages[2] = GuiSystem.GetImage("Gui/SelectAvatar/timer/3");
		mTimerImages[3] = GuiSystem.GetImage("Gui/SelectAvatar/timer/2");
		mTimerImages[4] = GuiSystem.GetImage("Gui/SelectAvatar/timer/1");
		mMaxPart = 5;
		mCurPart = -1;
		mStartTime = 0f;
		mStarted = false;
		mCurColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		mTimeSound = new SoundSystem.GuiSound();
		mTimeSound.mName = "timer";
		mTimeSound.mPath = "gui/";
		mTimeSound.mOptions = new SoundSystem.SoundOptions();
	}

	public override void Update()
	{
		if (mStarted)
		{
			int curPart = Mathf.FloorToInt(Time.time - mStartTime);
			SetCurPart(curPart);
			SetImageRect();
		}
	}

	public override void RenderElement()
	{
		if (mStarted)
		{
			GuiSystem.DrawImage(mCurImage, mCurImageRect, mCurColor);
		}
	}

	public void Start()
	{
		if (!mStarted)
		{
			mStarted = true;
			mCurPart = -1;
			mStartTime = Time.time;
		}
	}

	public void Stop()
	{
		mStarted = false;
		mCurPart = -1;
		mStartTime = 0f;
	}

	private void SetCurPart(int _part)
	{
		if (_part != mCurPart)
		{
			if (_part == mMaxPart)
			{
				Stop();
				return;
			}
			mCurPart = _part;
			mCurImage = mTimerImages[mCurPart];
			mStartScaleTime = Time.time;
			SoundSystem.Instance.PlayGuiSound(mTimeSound);
		}
	}

	private void SetImageRect()
	{
		float t = (Time.time - mStartScaleTime) * 2f;
		float num = Mathf.PingPong(t, 1f);
		mCurColor.a = Mathf.PingPong(t, 1f);
		float width = (float)mCurImage.width * num;
		float num2 = (float)mCurImage.height * num;
		mCurImageRect = new Rect(0f, 90f - num2 / 2f, width, num2);
		GuiSystem.SetChildRect(mZoneRect, ref mCurImageRect);
		mCurImageRect.x = ((float)OptionsMgr.mScreenWidth - mCurImageRect.width + 5f * GuiSystem.mYRate) / 2f;
	}
}
