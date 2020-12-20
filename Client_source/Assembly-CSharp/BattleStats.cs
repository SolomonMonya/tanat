using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class BattleStats : GuiElement
{
	private class StatPlatesComparer : IComparer<StatPlate>
	{
		public int Compare(StatPlate _sp1, StatPlate _sp2)
		{
			if (_sp1.IsTeamPlate && !_sp2.IsTeamPlate)
			{
				return -1;
			}
			if (!_sp1.IsTeamPlate && _sp2.IsTeamPlate)
			{
				return 1;
			}
			return string.Compare(_sp1.Name, _sp2.Name);
		}
	}

	private class StatPlate : GuiElement
	{
		public enum Status
		{
			NONE,
			ONLINE,
			DISCONNECTED,
			OFFLINE
		}

		private Texture2D mIcon;

		private Texture2D mIconFrame;

		private Texture2D mTeamFrame;

		private Texture2D mOfflineIcon;

		private GuiButton mNickButton;

		private string mLevel;

		private string mTeam;

		private string mTeamShort;

		private string mKills;

		private string mDeaths;

		private string mAssists;

		private Rect mTeamRect;

		private Rect mIconRect;

		private Rect mIconFrameRect;

		private Rect mLevelRect;

		private Rect mKillsRect;

		private Rect mDeathsRect;

		private Rect mAssistsRect;

		private Rect mSlashRect1;

		private Rect mSlashRect2;

		private bool mMinimized;

		private Player mData;

		private int mTeamNum;

		private List<StatPlate> mTeamData;

		private bool mTeamPlate;

		private int mTimeToRespawn;

		public Status mStatus;

		public OnNickClick mOnNickClick;

		public bool IsTeamPlate => mTeamPlate;

		public string Name => (mNickButton != null) ? mNickButton.mLabel : string.Empty;

		public int Id => (mData != null) ? mData.Id : (-1);

		public int Kills => (mData != null) ? mData.KillsCount : 0;

		public int Deaths => (mData != null) ? mData.DeathsCount : 0;

		public int Assists => (mData != null) ? mData.AssistsCount : 0;

		public StatPlate(bool _teamPlate)
		{
			mTeamPlate = _teamPlate;
		}

		public override void Init()
		{
			mLevel = "??";
			mKills = "??";
			mDeaths = "??";
			mAssists = "??";
			mNickButton = new GuiButton();
			mNickButton.mElementId = "NICK_BUTTON";
			mNickButton.mLabel = "??????";
			mNickButton.mOverColor = Color.yellow;
			mNickButton.mLabelStyle = "nick";
			GuiButton guiButton = mNickButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mNickButton.Init();
			mOfflineIcon = GuiSystem.GetImage("Gui/BattleStats/offline");
			mIconFrame = GuiSystem.GetImage("Gui/BattleStats/frame3");
			if (mTeamPlate)
			{
				mTeam = GuiSystem.GetLocaleText("STATS_PLATE_TEAM_ID") + " " + mTeamNum;
				mTeamShort = GuiSystem.GetLocaleText("STATS_PLATE_TEAM_SHORT_ID") + " " + mTeamNum;
				mTeamFrame = GuiSystem.GetImage("Gui/BattleStats/frame2");
			}
			else if (mData != null)
			{
				mNickButton.mLabel = mData.Name;
				if (mData.AvatarData != null)
				{
					mIcon = GuiSystem.GetImage(mData.AvatarData.mImg + "_01");
				}
			}
		}

		public override void SetSize()
		{
			mIconRect = new Rect(3f, 3f, 18f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mIconRect);
			mIconFrameRect = new Rect(2f, 2f, 20f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mIconFrameRect);
			mTeamRect = new Rect(0f, -1f, (!mMinimized) ? 100 : 44, 22f);
			GuiSystem.SetChildRect(mZoneRect, ref mTeamRect);
			mLevelRect = new Rect(25f, 0f, 18f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
			mNickButton.mZoneRect = new Rect(47f, -1f, 120f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mNickButton.mZoneRect);
			mKillsRect = new Rect(169f, 0f, 20f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mKillsRect);
			mDeathsRect = new Rect(194f, 0f, 20f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mDeathsRect);
			mAssistsRect = new Rect(219f, 0f, 20f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mAssistsRect);
			mSlashRect1 = new Rect(188f, 0f, 7f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mSlashRect1);
			mSlashRect2 = new Rect(213f, 0f, 7f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mSlashRect2);
		}

		public override void Update()
		{
			InitData();
			InitTeamData();
		}

		public override void RenderElement()
		{
			if (mTeamPlate)
			{
				GuiSystem.DrawImage(mTeamFrame, mZoneRect);
				if (mMinimized)
				{
					GuiSystem.DrawString(mTeamShort, mTeamRect, "label");
				}
				else
				{
					GuiSystem.DrawString(mTeam, mTeamRect, "label");
				}
			}
			else
			{
				bool flag = mStatus == Status.ONLINE && mTimeToRespawn > 0;
				if (mStatus == Status.DISCONNECTED || mStatus == Status.OFFLINE || flag)
				{
					GUI.contentColor = Color.gray;
				}
				GuiSystem.DrawImage(mIconFrame, mIconFrameRect);
				GuiSystem.DrawImage(mIcon, mIconRect);
				if (mStatus == Status.OFFLINE)
				{
					GuiSystem.DrawImage(mOfflineIcon, mIconRect);
				}
				GuiSystem.DrawString(mLevel, mLevelRect, "middle_left");
				if (!mMinimized)
				{
					mNickButton.RenderElement();
				}
				if (mStatus == Status.DISCONNECTED || flag)
				{
					GUI.contentColor = Color.white;
				}
				if (flag)
				{
					GuiSystem.DrawString(mTimeToRespawn.ToString(), mIconRect, "middle_center");
				}
			}
			if (!mMinimized)
			{
				GuiSystem.DrawString(mKills, mKillsRect, "middle_center");
				GuiSystem.DrawString("/", mSlashRect1, "middle_center");
				GuiSystem.DrawString(mDeaths, mDeathsRect, "middle_center");
				GuiSystem.DrawString("/", mSlashRect2, "middle_center");
				GuiSystem.DrawString(mAssists, mAssistsRect, "middle_center");
			}
			GUI.contentColor = Color.white;
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (!mTeamPlate && !mMinimized)
			{
				mNickButton.CheckEvent(_curEvent);
			}
			base.CheckEvent(_curEvent);
		}

		public void SetData(Player _player)
		{
			mData = _player;
		}

		public void SetTeamData(List<StatPlate> _teamPlayers, int _team)
		{
			mTeamData = _teamPlayers;
			mTeamNum = _team;
		}

		public void SetMinimized(bool _minimized)
		{
			mMinimized = _minimized;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "NICK_BUTTON" && _buttonId == 0 && mOnNickClick != null && mData != null)
			{
				mOnNickClick(mData.Name);
			}
		}

		private void InitData()
		{
			if (mData != null && !mTeamPlate)
			{
				mKills = mData.KillsCount.ToString();
				mDeaths = mData.DeathsCount.ToString();
				mAssists = mData.AssistsCount.ToString();
				mStatus = (mData.IsOnline ? Status.ONLINE : mStatus);
				mStatus = ((mData.IsOnline || mStatus != Status.ONLINE) ? mStatus : Status.DISCONNECTED);
				mLevel = (mData.Level + 1).ToString();
				if (mData.RespTime != null)
				{
					mTimeToRespawn = mData.RespTime.GetTimeToRespawn();
				}
				else
				{
					mTimeToRespawn = 0;
				}
			}
		}

		private void InitTeamData()
		{
			if (mTeamData == null || !mTeamPlate)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (StatPlate mTeamDatum in mTeamData)
			{
				num += mTeamDatum.Kills;
				num2 += mTeamDatum.Deaths;
				num3 += mTeamDatum.Assists;
			}
			mKills = num.ToString();
			mDeaths = num2.ToString();
			mAssists = num3.ToString();
		}
	}

	public delegate void MenuCallback();

	public delegate void OnNickClick(string _nick);

	private MapType mMapType;

	private IStoreContentProvider<Player> mPlayerProv;

	private Texture2D mZoneFrame;

	private Texture2D mBgFrame;

	private Texture2D mIconsFrame;

	private GuiButton mMenuButton;

	private GuiButton mPlayerListButton;

	private GuiButton mMinimizeButton;

	private bool mHided;

	private bool mMinimized;

	private bool mTeamRequired;

	private int mPlayersCount;

	private Rect mBgRect;

	private Rect mIconsRect;

	private GuiElement mTipVisible;

	private SortedDictionary<int, List<StatPlate>> mStatPlates;

	public MenuCallback mMenuCallback;

	public OnNickClick mOnNickClick;

	public override void Init()
	{
		mZoneFrame = GuiSystem.GetImage("Gui/CSStats/frame1");
		mBgFrame = GuiSystem.GetImage("Gui/BattleStats/frame1");
		mIconsFrame = GuiSystem.GetImage("Gui/BattleStats/icons");
		mStatPlates = new SortedDictionary<int, List<StatPlate>>();
		mTipVisible = new GuiElement();
		mTipVisible.mElementId = "TEMP_TIP_ELEMENT";
		mPlayerListButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mPlayerListButton.mElementId = "PLAYER_LIST_BUTTON";
		GuiButton guiButton = mPlayerListButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mPlayerListButton.mLabel = GuiSystem.GetLocaleText("Player_List_Button_Name");
		mPlayerListButton.Init();
		AddTutorialElement(mPlayerListButton);
		mMenuButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mMenuButton.mElementId = "MENU_BUTTON";
		GuiButton guiButton2 = mMenuButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mMenuButton.mLabel = GuiSystem.GetLocaleText("Menu_Button_Name");
		mMenuButton.Init();
		AddTutorialElement(mMenuButton);
		mMinimizeButton = new GuiButton();
		mMinimizeButton.mElementId = "MINIMIZE_BUTTON";
		GuiButton guiButton3 = mMinimizeButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		SetMinizeButtonImages();
	}

	public override void Uninit()
	{
		mPlayerProv = null;
		mHided = false;
		mMinimized = false;
		mTeamRequired = false;
		mPlayersCount = 0;
		if (mStatPlates != null)
		{
			mStatPlates.Clear();
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mZoneFrame.width, mZoneFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width;
		mPlayerListButton.mZoneRect = new Rect(16f, 16f, 132f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayerListButton.mZoneRect);
		mMenuButton.mZoneRect = new Rect(165f, 16f, 84f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mMenuButton.mZoneRect);
		SetBgSize();
		SetStatPlatesSize();
		PopupInfo.AddTip(this, "TIP_TEXT25", mMenuButton.mZoneRect);
	}

	public override void RenderElement()
	{
		if (MainInfoWindow.mHidden)
		{
			return;
		}
		if (!mHided)
		{
			GuiSystem.DrawImage(mBgFrame, mBgRect, 8, 8, 8, 20);
			mMinimizeButton.RenderElement();
			if (!mMinimized)
			{
				GuiSystem.DrawImage(mIconsFrame, mIconsRect);
			}
			foreach (List<StatPlate> value in mStatPlates.Values)
			{
				foreach (StatPlate item in value)
				{
					item.RenderElement();
				}
			}
		}
		GuiSystem.DrawImage(mZoneFrame, mZoneRect);
		mMenuButton.RenderElement();
		mPlayerListButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mMenuButton.CheckEvent(_curEvent);
		mPlayerListButton.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.BackQuote && GuiSystem.InputIsFree())
		{
			ShowPlayerList();
		}
		else if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Escape && GuiSystem.InputIsFree() && mMenuCallback != null)
		{
			mMenuCallback();
		}
		if (!mHided)
		{
			mMinimizeButton.CheckEvent(_curEvent);
			foreach (List<StatPlate> value in mStatPlates.Values)
			{
				foreach (StatPlate item in value)
				{
					item.CheckEvent(_curEvent);
				}
			}
		}
		base.CheckEvent(_curEvent);
	}

	public override void Update()
	{
		if (mHided || mPlayerProv == null)
		{
			return;
		}
		if (mPlayerProv.Count != mPlayersCount)
		{
			mPlayersCount = mPlayerProv.Count;
			InitStatPlates();
			SortStatPlates();
			SetStatPlatesSize();
		}
		foreach (List<StatPlate> value in mStatPlates.Values)
		{
			foreach (StatPlate item in value)
			{
				item.Update();
			}
		}
	}

	public void SetData(IStoreContentProvider<Player> _playerProv, MapType _mapType)
	{
		if (_playerProv != null)
		{
			mPlayersCount = 0;
			mPlayerProv = _playerProv;
			mMapType = _mapType;
			mTeamRequired = mMapType == MapType.CW_DOTA || mMapType == MapType.CW_SIEGE || mMapType == MapType.DOTA;
			mStatPlates.Clear();
			MinimizePlayerList(mMinimized);
		}
	}

	private void InitStatPlates()
	{
		StatPlate statPlate = null;
		List<StatPlate> value = null;
		foreach (List<StatPlate> value2 in mStatPlates.Values)
		{
			foreach (StatPlate item in value2)
			{
				item.mStatus = StatPlate.Status.OFFLINE;
			}
		}
		foreach (Player item2 in mPlayerProv.Content)
		{
			if (!mStatPlates.TryGetValue(item2.Team, out value))
			{
				mStatPlates.Add(item2.Team, value = new List<StatPlate>());
				if (mTeamRequired)
				{
					statPlate = new StatPlate(_teamPlate: true);
					statPlate.SetTeamData(value, item2.Team);
					statPlate.Init();
					value.Add(statPlate);
				}
			}
			statPlate = GetStatPlateById(value, item2.Id);
			if (statPlate == null)
			{
				statPlate = new StatPlate(_teamPlate: false);
				statPlate.SetData(item2);
				statPlate.Init();
				StatPlate statPlate2 = statPlate;
				statPlate2.mOnNickClick = (OnNickClick)Delegate.Combine(statPlate2.mOnNickClick, new OnNickClick(NickClicked));
				value.Add(statPlate);
			}
			statPlate.mStatus = StatPlate.Status.ONLINE;
		}
		CheckStatPlates();
	}

	private void CheckStatPlates()
	{
		if (mMapType != MapType.HUNT)
		{
			return;
		}
		Dictionary<int, List<StatPlate>> dictionary = new Dictionary<int, List<StatPlate>>();
		foreach (KeyValuePair<int, List<StatPlate>> mStatPlate in mStatPlates)
		{
			dictionary.Add(mStatPlate.Key, new List<StatPlate>());
			foreach (StatPlate item in mStatPlate.Value)
			{
				if (item.mStatus == StatPlate.Status.OFFLINE)
				{
					dictionary[mStatPlate.Key].Add(item);
				}
			}
		}
		foreach (KeyValuePair<int, List<StatPlate>> item2 in dictionary)
		{
			foreach (StatPlate item3 in item2.Value)
			{
				item3.mOnNickClick = (OnNickClick)Delegate.Remove(item3.mOnNickClick, new OnNickClick(NickClicked));
				mStatPlates[item2.Key].Remove(item3);
			}
			if (mStatPlates[item2.Key].Count == 0)
			{
				mStatPlates.Remove(item2.Key);
			}
		}
		dictionary.Clear();
	}

	private void NickClicked(string _nick)
	{
		if (mOnNickClick != null && !string.IsNullOrEmpty(_nick))
		{
			mOnNickClick(_nick);
		}
	}

	private StatPlate GetStatPlateById(List<StatPlate> _statPlates, int _id)
	{
		if (_statPlates == null)
		{
			return null;
		}
		foreach (StatPlate _statPlate in _statPlates)
		{
			if (_statPlate.Id == _id)
			{
				return _statPlate;
			}
		}
		return null;
	}

	private void SortStatPlates()
	{
		StatPlatesComparer comparer = new StatPlatesComparer();
		foreach (List<StatPlate> value in mStatPlates.Values)
		{
			value.Sort(comparer);
		}
	}

	private void SetBgSize()
	{
		if (!(mZoneFrame == null) && !(mBgFrame == null))
		{
			float num = 60f;
			float left = ((!mMinimized) ? 0f : ((float)mZoneFrame.width - num));
			float width = ((!mMinimized) ? ((float)mBgFrame.width) : num);
			mBgRect = new Rect(left, 50f, width, 60f);
			GuiSystem.SetChildRect(mZoneRect, ref mBgRect);
			mMinimizeButton.mZoneRect = new Rect(7f, 9f, 28f, 28f);
			GuiSystem.SetChildRect(mBgRect, ref mMinimizeButton.mZoneRect);
			mIconsRect = new Rect(175f, 10f, mIconsFrame.width, mIconsFrame.height);
			GuiSystem.SetChildRect(mBgRect, ref mIconsRect);
			Rect _rect = new Rect(175f, 10f, 20f, 20f);
			GuiSystem.SetChildRect(mBgRect, ref _rect);
			PopupInfo.AddTip(mTipVisible, "TIP_TEXT94", _rect);
			_rect = new Rect(201f, 10f, 20f, 20f);
			GuiSystem.SetChildRect(mBgRect, ref _rect);
			PopupInfo.AddTip(mTipVisible, "TIP_TEXT95", _rect);
			_rect = new Rect(227f, 10f, 20f, 20f);
			GuiSystem.SetChildRect(mBgRect, ref _rect);
			PopupInfo.AddTip(mTipVisible, "TIP_TEXT96", _rect);
		}
	}

	private void SetStatPlatesSize()
	{
		float left = 7f;
		float num = 37f;
		float width = ((!mMinimized) ? 250 : 44);
		float num2 = 22f;
		int num3 = 0;
		mBgRect.height = 60f * GuiSystem.mYRate;
		foreach (List<StatPlate> value in mStatPlates.Values)
		{
			foreach (StatPlate item in value)
			{
				item.SetMinimized(mMinimized);
				item.mZoneRect = new Rect(left, num + (float)num3 * num2, width, num2);
				GuiSystem.SetChildRect(mBgRect, ref item.mZoneRect);
				mBgRect.height += item.mZoneRect.height;
				item.SetSize();
				num3++;
			}
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "MENU_BUTTON" && _buttonId == 0)
		{
			if (mMenuCallback != null)
			{
				mMenuCallback();
			}
		}
		else if (_sender.mElementId == "PLAYER_LIST_BUTTON" && _buttonId == 0)
		{
			ShowPlayerList();
		}
		else if (_sender.mElementId == "MINIMIZE_BUTTON" && _buttonId == 0)
		{
			MinimizePlayerList(!mMinimized);
		}
	}

	private void ShowPlayerList()
	{
		mHided = !mHided;
		mPlayerListButton.Pressed = !mHided;
		mTipVisible.SetActive(!mHided);
		string id = (mHided ? "CLOSE_LOG_TXT" : "OPEN_LOG_TXT");
		UserLog.AddAction(UserActionType.SWITCH_PLAYER_LIST_IN_BATTLE, (!mHided) ? 1 : 0, GuiSystem.GetLocaleText(id));
	}

	private void MinimizePlayerList(bool _minimized)
	{
		mMinimized = _minimized;
		mTipVisible.SetActive(!mMinimized);
		SetBgSize();
		SetStatPlatesSize();
		SetMinizeButtonImages();
	}

	private void SetMinizeButtonImages()
	{
		if (mMinimizeButton != null)
		{
			string text = "Gui/BattleStats/button_" + ((!mMinimized) ? "2" : "1");
			mMinimizeButton.mNormImg = GuiSystem.GetImage(text + "_norm");
			mMinimizeButton.mOverImg = GuiSystem.GetImage(text + "_over");
			mMinimizeButton.mPressImg = GuiSystem.GetImage(text + "_press");
			mMinimizeButton.Init();
		}
	}
}
