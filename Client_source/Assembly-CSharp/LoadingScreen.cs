using TanatKernel;
using UnityEngine;

public class LoadingScreen : GuiElement
{
	public Texture2D mBackgroung;

	public Texture2D mProgress;

	private Rect mProgressRect;

	private Rect mTextProgressRect;

	private Rect mVersionRect;

	private IProgressProvider mProgressProv;

	public override void Init()
	{
		mProgress = GuiSystem.GetImage("Gui/Loading/progress");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, 1707f, 960f);
		GuiSystem.GetRectScaled(ref mZoneRect, _ignoreLowRate: true);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mProgressRect = new Rect(0f, 789f, mProgress.width, mProgress.height);
		mTextProgressRect = new Rect(0f, 850f, mProgress.width, mProgress.height);
		GuiSystem.GetRectScaled(ref mProgressRect, _ignoreLowRate: true);
		GuiSystem.GetRectScaled(ref mTextProgressRect, _ignoreLowRate: true);
		mProgressRect.x = ((float)OptionsMgr.mScreenWidth - mProgressRect.width) / 2f;
		mTextProgressRect.x = ((float)OptionsMgr.mScreenWidth - mTextProgressRect.width) / 2f;
		mVersionRect = new Rect(0f, 0f, 100f, 20f);
		GuiSystem.GetRectScaled(ref mVersionRect, _ignoreLowRate: true);
		mVersionRect.x = (float)OptionsMgr.mScreenWidth - mVersionRect.width - 5f;
		mVersionRect.y = (float)OptionsMgr.mScreenHeight - mVersionRect.height - 5f;
	}

	public override void RenderElement()
	{
		if (null != mBackgroung)
		{
			GuiSystem.DrawImage(mBackgroung, mZoneRect);
		}
		if (mProgressProv != null)
		{
			float progress = mProgressProv.GetProgress();
			Rect drawRect = mProgressRect;
			drawRect.width *= progress;
			GuiSystem.DrawImage(mProgress, drawRect, new Rect(0f, 0f, progress, 1f));
			string @string = ((!BattleServerConnection.mIsReady) ? ((int)(progress * 100f) + "%") : GuiSystem.GetLocaleText("GUI_WAIT_PLAYERS"));
			GuiSystem.DrawString(@string, mTextProgressRect, "loading");
			GuiSystem.DrawString(TanatApp.mVersion, mVersionRect, "middle_right");
		}
	}

	public void SetData(IProgressProvider _progressProv)
	{
		int num = Random.Range(1, 5);
		mBackgroung = GuiSystem.GetImage("Gui/Loading/" + num);
		mProgressProv = _progressProv;
		if (mProgressProv != null)
		{
			mProgressProv.BeginProgress();
		}
	}

	public void Clear()
	{
		if (mProgressProv != null)
		{
			mProgressProv.EndProgress();
			mProgressProv = null;
		}
	}
}
