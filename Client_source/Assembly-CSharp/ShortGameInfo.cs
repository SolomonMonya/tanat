using TanatKernel;
using UnityEngine;

public class ShortGameInfo : GuiElement
{
	private Texture2D mBgImage;

	private Rect mTimeRect = default(Rect);

	private string mTimeString = string.Empty;

	private string mKills = "0";

	private string mDeaths = "0";

	private string mAssists = "0";

	private string mTeam1Kills = "0";

	private string mTeam2Kills = "0";

	private Rect mKillsRect = default(Rect);

	private Rect mDeathsRect = default(Rect);

	private Rect mAssistsRect = default(Rect);

	private Rect mTeam1KillsRect = default(Rect);

	private Rect mTeam2KillsRect = default(Rect);

	private Color mKillsColor = Color.red;

	private Color mDeathsColor = new Color(13f / 15f, 22f / 51f, 23f / 255f);

	private Color mAssistsColor = new Color(103f / 255f, 227f / 255f, 223f / 255f);

	private Color mTeam1KillsColor = Color.red;

	private Color mTeam2KillsColor = new Color(103f / 255f, 227f / 255f, 223f / 255f);

	private Rect mSlashRect1 = default(Rect);

	private Rect mSlashRect2 = default(Rect);

	private Rect mSlashRect3 = default(Rect);

	private KillEventManager mKillEventMgr;

	private Player mPlayer;

	private BattleTimer mTimer;

	private float mTimeOffset;

	public override void Init()
	{
		mBgImage = GuiSystem.GetImage("Gui/ShortStats/frame1");
	}

	public override void SetSize()
	{
		mTimeRect = new Rect(92f, 10f, 43f, 43f);
		mKillsRect = new Rect(18f, 15f, 20f, 20f);
		mDeathsRect = new Rect(38f, 15f, 20f, 20f);
		mAssistsRect = new Rect(58f, 15f, 20f, 20f);
		mSlashRect1 = new Rect(28f, 15f, 20f, 20f);
		mSlashRect2 = new Rect(48f, 15f, 20f, 20f);
		mSlashRect3 = new Rect(172f, 15f, 20f, 20f);
		mTeam1KillsRect = new Rect(162f, 15f, 20f, 20f);
		mTeam2KillsRect = new Rect(182f, 15f, 20f, 20f);
		mZoneRect = new Rect(0f, -4f, mBgImage.width, mBgImage.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
		GuiSystem.SetChildRect(mZoneRect, ref mKillsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDeathsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mAssistsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mTeam1KillsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mTeam2KillsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSlashRect1);
		GuiSystem.SetChildRect(mZoneRect, ref mSlashRect2);
		GuiSystem.SetChildRect(mZoneRect, ref mSlashRect3);
		Rect _rect = new Rect(18f, 15f, 60f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT32", _rect);
		PopupInfo.AddTip(this, "TIP_TEXT33", mTimeRect);
		_rect = new Rect(162f, 15f, 40f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT34", _rect);
	}

	public override void Update()
	{
		SetTimeString();
		SetStatsString();
	}

	public override void RenderElement()
	{
		if (!MainInfoWindow.mHidden)
		{
			if ((bool)mBgImage)
			{
				GuiSystem.DrawImage(mBgImage, mZoneRect);
			}
			GuiSystem.DrawString(mTimeString, mTimeRect, "middle_center");
			GUI.contentColor = mKillsColor;
			GuiSystem.DrawString(mKills, mKillsRect, "middle_center");
			GUI.contentColor = Color.white;
			GuiSystem.DrawString("/", mSlashRect1, "middle_center");
			GUI.contentColor = mDeathsColor;
			GuiSystem.DrawString(mDeaths, mDeathsRect, "middle_center");
			GUI.contentColor = Color.white;
			GuiSystem.DrawString("/", mSlashRect2, "middle_center");
			GUI.contentColor = mAssistsColor;
			GuiSystem.DrawString(mAssists, mAssistsRect, "middle_center");
			GUI.contentColor = Color.white;
			GUI.contentColor = mTeam1KillsColor;
			GuiSystem.DrawString(mTeam1Kills, mTeam1KillsRect, "middle_center");
			GUI.contentColor = Color.white;
			GuiSystem.DrawString("/", mSlashRect3, "middle_center");
			GUI.contentColor = mTeam2KillsColor;
			GuiSystem.DrawString(mTeam2Kills, mTeam2KillsRect, "middle_center");
			GUI.contentColor = Color.white;
		}
	}

	private void SetTimeString()
	{
		int num = (int)(mTimer.Time - mTimeOffset);
		int num2 = num / 60;
		int num3 = num - num2 * 60;
		if (num3 > 9)
		{
			mTimeString = num2 + ":" + num3;
		}
		else
		{
			mTimeString = num2 + ":0" + num3;
		}
	}

	private void SetStatsString()
	{
		mKills = mPlayer.KillsCount.ToString();
		mDeaths = mPlayer.DeathsCount.ToString();
		mAssists = mPlayer.AssistsCount.ToString();
		mTeam1Kills = mKillEventMgr.GetFriendTeamKills().ToString();
		mTeam2Kills = mKillEventMgr.GetEnemyTeamKills().ToString();
	}

	public void SetData(Player _pl, BattleTimer _timer, KillEventManager _killEvtMgr)
	{
		mPlayer = _pl;
		mTimer = _timer;
		mKillEventMgr = _killEvtMgr;
	}

	public void Clear()
	{
		mPlayer = null;
		mTimer = null;
		mKillEventMgr = null;
	}

	public void SetTimeOffset(float _timeOffset)
	{
		mTimeOffset = _timeOffset;
	}
}
