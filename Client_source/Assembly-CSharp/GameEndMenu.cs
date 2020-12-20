using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class GameEndMenu : GuiElement, EscapeListener
{
	private class BattleEndMenuData
	{
		public BattleEndData mData;

		public Texture2D mIconImg;

		public Texture2D mFrameImg;

		public Texture2D mRankImg;

		public Texture2D mRankChangeImg;

		public Color mColor = Color.white;

		public string mGold;

		public string mSilver;

		public string mBronze;

		public Rect mFrameImgRect = default(Rect);

		public Rect mIconImgRect = default(Rect);

		public Rect mRankImgRect = default(Rect);

		public Rect mRankChangeImgRect = default(Rect);

		public Rect mNameRect = default(Rect);

		public Rect mKillRect = default(Rect);

		public Rect mKillAssistRect = default(Rect);

		public Rect mDeathRect = default(Rect);

		public Rect mHonorExpRect = default(Rect);

		public Rect mAvaLvlRect = default(Rect);

		public Rect mDeltaRatingRect = default(Rect);

		public Rect mNewRatingRect = default(Rect);

		public Rect mGoldRect = default(Rect);

		public Rect mSilverRect = default(Rect);

		public Rect mBronzeRect = default(Rect);

		public void Init()
		{
			mFrameImg = GuiSystem.GetImage("Gui/GameEnd/frame_04");
			if (mData.mNewRating != mData.mOldRating)
			{
				mRankChangeImg = GuiSystem.GetImage("Gui/misc/arrow_" + ((mData.mNewRating > mData.mOldRating) ? 1 : 0));
			}
			if (mData.mKillRank > 10)
			{
				mData.mKillRank = 10;
			}
			if (mData.mKillRank >= 3)
			{
				mRankImg = GuiSystem.GetImage("Gui/GameEnd/KillRank/rank_" + mData.mKillRank);
			}
			int _gold = 0;
			int _silver = 0;
			int _bronze = 0;
			ShopVendor.SetMoney(mData.mMoney, ref _gold, ref _silver, ref _bronze);
			mGold = _gold.ToString();
			mSilver = _silver.ToString();
			mBronze = _bronze.ToString();
			if (mData.mMoney < 0)
			{
				mGold = "-" + mGold;
			}
		}
	}

	private class BattleEndTeamData
	{
		public int mTeam;

		public int mKill;

		public int mKillAssist;

		public int mDeath;

		public int mHonor;

		public int mGold;

		public int mSilver;

		public int mBronze;

		public int mMoney;

		public string mTeamName;

		public string mGoldStr;

		public string mSilverStr;

		public string mBronzeStr;

		public Color mTeamColor;

		public Rect mTeamNameRect = default(Rect);

		public Rect mKillRect = default(Rect);

		public Rect mKillAssistRect = default(Rect);

		public Rect mDeathRect = default(Rect);

		public Rect mHonorRect = default(Rect);

		public Rect mGoldRect = default(Rect);

		public Rect mSilverRect = default(Rect);

		public Rect mBronzeRect = default(Rect);

		public void InitMoney()
		{
			ShopVendor.SetMoney(mMoney, ref mGold, ref mSilver, ref mBronze);
			mGoldStr = mGold.ToString();
			mSilverStr = mSilver.ToString();
			mBronzeStr = mBronze.ToString();
			if (mMoney < 0)
			{
				mGoldStr = "-" + mGoldStr;
			}
		}
	}

	private enum MenuType
	{
		BY_TEAMS,
		BY_AVATARS
	}

	private enum SortType
	{
		BY_NAME,
		BY_KILL,
		BY_KILL_ASSIST,
		BY_DEATH,
		BY_HONOR,
		BY_AVA_LEVEL,
		BY_NEW_RATING,
		BY_DELTA_RATING,
		BY_MONEY,
		BY_KILL_RANK
	}

	private class BattleEndComparer : IComparer<BattleEndMenuData>
	{
		private MenuType mMenuType;

		private SortType mSortType;

		private int mInverce = 1;

		public BattleEndComparer(MenuType _menuType, SortType _sortType, int _inverse)
		{
			mMenuType = _menuType;
			mSortType = _sortType;
			mInverce = _inverse;
		}

		private int CompareInt(int _val1, int _val2)
		{
			if (_val1 > _val2)
			{
				return 1 * mInverce;
			}
			if (_val1 < _val2)
			{
				return -1 * mInverce;
			}
			return 0;
		}

		public int Compare(BattleEndMenuData _data1, BattleEndMenuData _data2)
		{
			if (mMenuType == MenuType.BY_TEAMS)
			{
				if (_data1.mData.mTeam > _data2.mData.mTeam)
				{
					return 1;
				}
				if (_data1.mData.mTeam < _data2.mData.mTeam)
				{
					return -1;
				}
			}
			if (mSortType == SortType.BY_NAME)
			{
				return string.Compare(_data1.mData.mPlayerName, _data2.mData.mPlayerName) * mInverce;
			}
			if (mSortType == SortType.BY_AVA_LEVEL)
			{
				return CompareInt(_data1.mData.mAvaLvl, _data2.mData.mAvaLvl);
			}
			if (mSortType == SortType.BY_DEATH)
			{
				return CompareInt(_data1.mData.mDeath, _data2.mData.mDeath);
			}
			if (mSortType == SortType.BY_HONOR)
			{
				return CompareInt(_data1.mData.mHonor, _data2.mData.mHonor);
			}
			if (mSortType == SortType.BY_KILL)
			{
				return CompareInt(_data1.mData.mKill, _data2.mData.mKill);
			}
			if (mSortType == SortType.BY_KILL_ASSIST)
			{
				return CompareInt(_data1.mData.mKillAssist, _data2.mData.mKillAssist);
			}
			if (mSortType == SortType.BY_NEW_RATING)
			{
				return CompareInt(_data1.mData.mNewRating, _data2.mData.mNewRating);
			}
			if (mSortType == SortType.BY_DELTA_RATING)
			{
				int val = _data1.mData.mNewRating - _data1.mData.mOldRating;
				int val2 = _data2.mData.mNewRating - _data2.mData.mOldRating;
				return CompareInt(val, val2);
			}
			if (mSortType == SortType.BY_KILL_RANK)
			{
				return CompareInt(_data1.mData.mKillRank, _data2.mData.mKillRank);
			}
			if (mSortType == SortType.BY_MONEY)
			{
				return CompareInt(_data1.mData.mMoney, _data2.mData.mMoney);
			}
			return 0;
		}
	}

	public delegate void ExitCallback();

	public delegate void StartExitTimer();

	public ExitCallback mExitCallback;

	public StartExitTimer mStartExitTimerCallback;

	private string mLabel;

	private Rect mLabelRect;

	private GuiButton mExitButton;

	private GuiButton mMode1Button;

	private GuiButton mMode2Button;

	private GuiButton mCloseButton;

	private Texture2D mFrameImage;

	private MenuType mMenuType;

	private SortType mSortType = SortType.BY_AVA_LEVEL;

	private int mInverse = 1;

	private List<BattleEndMenuData> mBattleEndData = new List<BattleEndMenuData>();

	private List<GuiButton> mSortButtons = new List<GuiButton>();

	private Texture2D mTeam1Frame;

	private Texture2D mTeam2Frame;

	private Color mTeam1Color;

	private Color mTeam2Color;

	private Rect mTeam1FrameRect;

	private Rect mTeam2FrameRect;

	private Dictionary<int, BattleEndTeamData> mTeamsData = new Dictionary<int, BattleEndTeamData>();

	private CtrlAvatarStore mCtrlAvatarStore;

	private MapType mMapType;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private Rect mDrawRect;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			SetActive(_active: false);
			UserLog.AddAction(UserActionType.BATTLE_END);
			if (mExitCallback != null)
			{
				mExitCallback();
			}
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(425, this);
		mFrameImage = GuiSystem.GetImage("Gui/GameEnd/frame_01");
		mTeam1Frame = GuiSystem.GetImage("Gui/GameEnd/frame_02");
		mTeam2Frame = GuiSystem.GetImage("Gui/GameEnd/frame_03");
		mMapType = MapType.DOTA;
		mTeam1Color = new Color(49f / 255f, 61f / 85f, 202f / 255f);
		mTeam2Color = new Color(218f / 255f, 4f / 255f, 4f / 255f);
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		mLabel = GuiSystem.GetLocaleText("Game_End_Label");
		InitButtons();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrameImage.width, mFrameImage.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mExitButton.mZoneRect = new Rect(996f, 425f, 115f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mExitButton.mZoneRect);
		mMode1Button.mZoneRect = new Rect(156f, 46f, 163f, 35f);
		GuiSystem.SetChildRect(mZoneRect, ref mMode1Button.mZoneRect);
		mMode2Button.mZoneRect = new Rect(22f, 46f, 163f, 35f);
		GuiSystem.SetChildRect(mZoneRect, ref mMode2Button.mZoneRect);
		mLabelRect = new Rect(27f, 11f, 1078f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		foreach (GuiButton mSortButton in mSortButtons)
		{
			switch (mSortButton.mId)
			{
			case 0:
				mSortButton.mZoneRect = new Rect(30f, 80f, 158f, 18f);
				break;
			case 5:
				mSortButton.mZoneRect = new Rect(190f, 80f, 78f, 18f);
				break;
			case 1:
				mSortButton.mZoneRect = new Rect(270f, 80f, 81f, 18f);
				break;
			case 2:
				mSortButton.mZoneRect = new Rect(353f, 80f, 75f, 18f);
				break;
			case 3:
				mSortButton.mZoneRect = new Rect(430f, 80f, 71f, 18f);
				break;
			case 4:
				mSortButton.mZoneRect = new Rect(503f, 80f, 123f, 18f);
				break;
			case 8:
				mSortButton.mZoneRect = new Rect(628f, 80f, 110f, 18f);
				break;
			case 9:
				mSortButton.mZoneRect = new Rect(740f, 80f, 135f, 18f);
				break;
			case 6:
				mSortButton.mZoneRect = new Rect(877f, 80f, 125f, 18f);
				break;
			case 7:
				mSortButton.mZoneRect = new Rect(1004f, 80f, 75f, 18f);
				break;
			}
			GuiSystem.SetChildRect(mZoneRect, ref mSortButton.mZoneRect);
		}
		mStartScrollOffset = 309f;
		mDrawRect = new Rect(28f, 104f, 1051f, 312f);
		GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
		mCloseButton.mZoneRect = new Rect(1095f, 7f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mScrollbar.mZoneRect = new Rect(1079f, 104f, 22f, 309f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		SetRects();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImage, mZoneRect);
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		mExitButton.RenderElement();
		if (mMode1Button.mId == (int)mMenuType)
		{
			mMode2Button.RenderElement();
			if (mMapType != 0)
			{
				mMode1Button.RenderElement();
			}
		}
		else if (mMode2Button.mId == (int)mMenuType)
		{
			if (mMapType != 0)
			{
				mMode1Button.RenderElement();
			}
			mMode2Button.RenderElement();
		}
		mCloseButton.RenderElement();
		foreach (GuiButton mSortButton in mSortButtons)
		{
			mSortButton.RenderElement();
		}
		foreach (BattleEndMenuData mBattleEndDatum in mBattleEndData)
		{
			if (!(mBattleEndDatum.mFrameImgRect.y + 1f < mDrawRect.y) && !(mBattleEndDatum.mFrameImgRect.y + mBattleEndDatum.mFrameImgRect.height > mDrawRect.y + mDrawRect.height))
			{
				GuiSystem.DrawImage(mBattleEndDatum.mFrameImg, mBattleEndDatum.mFrameImgRect);
				GuiSystem.DrawImage(mBattleEndDatum.mIconImg, mBattleEndDatum.mIconImgRect);
				if (null != mBattleEndDatum.mRankImg)
				{
					GuiSystem.DrawImage(mBattleEndDatum.mRankImg, mBattleEndDatum.mRankImgRect);
				}
				if (null != mBattleEndDatum.mRankChangeImg)
				{
					GuiSystem.DrawImage(mBattleEndDatum.mRankChangeImg, mBattleEndDatum.mRankChangeImgRect);
				}
				GUI.contentColor = mBattleEndDatum.mColor;
				GuiSystem.DrawString(mBattleEndDatum.mData.mPlayerName, mBattleEndDatum.mNameRect, "middle_left");
				GUI.contentColor = Color.white;
				GuiSystem.DrawString(mBattleEndDatum.mData.mKill.ToString(), mBattleEndDatum.mKillRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mData.mKillAssist.ToString(), mBattleEndDatum.mKillAssistRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mData.mDeath.ToString(), mBattleEndDatum.mDeathRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mData.mHonor.ToString(), mBattleEndDatum.mHonorExpRect, "middle_center");
				GuiSystem.DrawString((mBattleEndDatum.mData.mAvaLvl + 1).ToString(), mBattleEndDatum.mAvaLvlRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mData.mNewRating.ToString(), mBattleEndDatum.mNewRatingRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mGold.ToString(), mBattleEndDatum.mGoldRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mSilver.ToString(), mBattleEndDatum.mSilverRect, "middle_center");
				GuiSystem.DrawString(mBattleEndDatum.mBronze.ToString(), mBattleEndDatum.mBronzeRect, "middle_center");
				GuiSystem.DrawString((mBattleEndDatum.mData.mNewRating - mBattleEndDatum.mData.mOldRating).ToString(), mBattleEndDatum.mDeltaRatingRect, "middle_right");
			}
		}
		if (mMenuType == MenuType.BY_TEAMS)
		{
			GuiSystem.DrawImage(mTeam1Frame, mTeam1FrameRect);
			GuiSystem.DrawImage(mTeam2Frame, mTeam2FrameRect);
			foreach (BattleEndTeamData value in mTeamsData.Values)
			{
				GUI.contentColor = value.mTeamColor;
				GuiSystem.DrawString(value.mTeamName, value.mTeamNameRect, "middle_left");
				GuiSystem.DrawString(value.mKill.ToString(), value.mKillRect, "middle_center");
				GuiSystem.DrawString(value.mKillAssist.ToString(), value.mKillAssistRect, "middle_center");
				GuiSystem.DrawString(value.mDeath.ToString(), value.mDeathRect, "middle_center");
				GuiSystem.DrawString(value.mHonor.ToString(), value.mHonorRect, "middle_center");
				GUI.contentColor = Color.white;
				GuiSystem.DrawString(value.mGoldStr, value.mGoldRect, "middle_center");
				GuiSystem.DrawString(value.mSilverStr, value.mSilverRect, "middle_center");
				GuiSystem.DrawString(value.mBronzeStr, value.mBronzeRect, "middle_center");
			}
		}
		mScrollbar.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mExitButton.CheckEvent(_curEvent);
		if (mMode1Button.mId == (int)mMenuType)
		{
			if (mMapType != 0)
			{
				mMode1Button.CheckEvent(_curEvent);
			}
			mMode2Button.CheckEvent(_curEvent);
		}
		else if (mMode2Button.mId == (int)mMenuType)
		{
			mMode2Button.CheckEvent(_curEvent);
			if (mMapType != 0)
			{
				mMode1Button.CheckEvent(_curEvent);
			}
		}
		mCloseButton.CheckEvent(_curEvent);
		foreach (GuiButton mSortButton in mSortButtons)
		{
			mSortButton.CheckEvent(_curEvent);
		}
		mScrollbar.CheckEvent(_curEvent);
	}

	public override void OnInput()
	{
		mScrollbar.OnInput();
	}

	public void SetData(MapType _mapType, Dictionary<int, BattleEndData> _data, CtrlAvatarStore _avatarStore)
	{
		mScrollOffset = 0f;
		mScrollbar.Refresh();
		mMapType = _mapType;
		mCtrlAvatarStore = _avatarStore;
		mBattleEndData.Clear();
		mTeamsData.Clear();
		foreach (KeyValuePair<int, BattleEndData> _datum in _data)
		{
			InitPlayerData(_datum.Value);
		}
		foreach (KeyValuePair<int, BattleEndTeamData> mTeamsDatum in mTeamsData)
		{
			mTeamsDatum.Value.InitMoney();
		}
		mSortType = SortType.BY_NAME;
		mMenuType = ((mMapType == MapType.DM) ? MenuType.BY_AVATARS : MenuType.BY_TEAMS);
		SortBattleData();
		SetRects();
		SetModeButtonImages();
	}

	public void Open()
	{
		SetActive(_active: true);
		if (mStartExitTimerCallback != null)
		{
			mStartExitTimerCallback();
		}
	}

	private void SetModeButtonImages()
	{
		if (mMode2Button.mId == (int)mMenuType)
		{
			mMode2Button.mNormImg = GuiSystem.GetImage("Gui/GameEnd/button_1_norm");
			mMode2Button.mOverImg = GuiSystem.GetImage("Gui/GameEnd/button_1_over");
			mMode2Button.mPressImg = GuiSystem.GetImage("Gui/GameEnd/button_1_over");
			mMode1Button.mNormImg = GuiSystem.GetImage("Gui/GameEnd/button_4_norm");
			mMode1Button.mOverImg = GuiSystem.GetImage("Gui/GameEnd/button_4_over");
			mMode1Button.mPressImg = GuiSystem.GetImage("Gui/GameEnd/button_4_over");
		}
		else if (mMode1Button.mId == (int)mMenuType)
		{
			mMode2Button.mNormImg = GuiSystem.GetImage("Gui/GameEnd/button_2_norm");
			mMode2Button.mOverImg = GuiSystem.GetImage("Gui/GameEnd/button_2_over");
			mMode2Button.mPressImg = GuiSystem.GetImage("Gui/GameEnd/button_2_over");
			mMode1Button.mNormImg = GuiSystem.GetImage("Gui/GameEnd/button_3_norm");
			mMode1Button.mOverImg = GuiSystem.GetImage("Gui/GameEnd/button_3_over");
			mMode1Button.mPressImg = GuiSystem.GetImage("Gui/GameEnd/button_3_over");
		}
		mMode1Button.SetCurBtnImage();
		mMode2Button.SetCurBtnImage();
	}

	private void SetRects()
	{
		float num = 24f;
		float num2 = 26f;
		float num3 = 0f;
		int i = 0;
		for (int count = mBattleEndData.Count; i < count; i++)
		{
			BattleEndMenuData battleEndMenuData = mBattleEndData[i];
			if (mMenuType == MenuType.BY_TEAMS)
			{
				battleEndMenuData.mColor = ((battleEndMenuData.mData.mTeam != 1) ? mTeam2Color : mTeam1Color);
				if (battleEndMenuData.mData.mTeam == 1)
				{
					battleEndMenuData.mFrameImgRect = new Rect(29f, 130f + (float)i * num, battleEndMenuData.mFrameImg.width, battleEndMenuData.mFrameImg.height);
					if (mMapType != 0)
					{
						mTeam2FrameRect = new Rect(29f, 130f + (float)(i + 1) * num, mTeam2Frame.width, mTeam2Frame.height);
						mTeam2FrameRect.y -= mScrollOffset;
					}
				}
				else
				{
					battleEndMenuData.mFrameImgRect = new Rect(29f, 156f + (float)i * num, battleEndMenuData.mFrameImg.width, battleEndMenuData.mFrameImg.height);
				}
			}
			else if (mMenuType == MenuType.BY_AVATARS)
			{
				battleEndMenuData.mFrameImgRect = new Rect(29f, 104f + (float)i * num, battleEndMenuData.mFrameImg.width, battleEndMenuData.mFrameImg.height);
			}
			battleEndMenuData.mFrameImgRect.y -= mScrollOffset;
			GuiSystem.SetChildRect(mZoneRect, ref battleEndMenuData.mFrameImgRect);
			num3 += num;
			SetDataRects(battleEndMenuData);
		}
		if (mMenuType == MenuType.BY_TEAMS)
		{
			mTeam1FrameRect = new Rect(29f, 104f, mTeam1Frame.width, mTeam1Frame.height);
			GuiSystem.SetChildRect(mZoneRect, ref mTeam1FrameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mTeam2FrameRect);
			num3 += num2 * 2f;
			SetTeamRects();
		}
		mScrollbar.SetData(mStartScrollOffset, num3);
	}

	private void InitPlayerData(BattleEndData _data)
	{
		if (_data == null || mCtrlAvatarStore == null)
		{
			return;
		}
		BattleEndMenuData battleEndMenuData = new BattleEndMenuData();
		battleEndMenuData.mData = _data;
		AvatarData avatarData = mCtrlAvatarStore.TryGet(battleEndMenuData.mData.mAvatarId);
		if (avatarData != null)
		{
			battleEndMenuData.mIconImg = GuiSystem.GetImage(avatarData.mImg + "_01");
		}
		else
		{
			Log.Error("avatar data not exist for : " + battleEndMenuData.mData.mAvatarId);
		}
		battleEndMenuData.Init();
		mBattleEndData.Add(battleEndMenuData);
		if (mMapType != 0)
		{
			if (!mTeamsData.ContainsKey(_data.mTeam))
			{
				mTeamsData.Add(_data.mTeam, new BattleEndTeamData());
				mTeamsData[_data.mTeam].mTeamName = GuiSystem.GetLocaleText("STATS_PLATE_TEAM_ID") + " " + _data.mTeam;
				mTeamsData[_data.mTeam].mTeam = _data.mTeam;
				mTeamsData[_data.mTeam].mTeamColor = ((_data.mTeam != 1) ? mTeam2Color : mTeam1Color);
			}
			mTeamsData[_data.mTeam].mKill += _data.mKill;
			mTeamsData[_data.mTeam].mKillAssist += _data.mKillAssist;
			mTeamsData[_data.mTeam].mDeath += _data.mDeath;
			mTeamsData[_data.mTeam].mHonor += _data.mHonor;
			mTeamsData[_data.mTeam].mMoney += _data.mMoney;
		}
	}

	private void SetDataRects(BattleEndMenuData _menuData)
	{
		_menuData.mIconImgRect = new Rect(4f, 4f, 18f, 18f);
		_menuData.mNameRect = new Rect(28f, 4f, 130f, 18f);
		_menuData.mAvaLvlRect = new Rect(160f, 4f, 78f, 18f);
		_menuData.mKillRect = new Rect(240f, 4f, 81f, 18f);
		_menuData.mKillAssistRect = new Rect(323f, 4f, 75f, 18f);
		_menuData.mDeathRect = new Rect(400f, 4f, 71f, 18f);
		_menuData.mHonorExpRect = new Rect(473f, 4f, 123f, 18f);
		_menuData.mRankImgRect = new Rect(739f, 9f, 77f, 8f);
		_menuData.mNewRatingRect = new Rect(847f, 4f, 125f, 18f);
		_menuData.mDeltaRatingRect = new Rect(977f, 4f, 40f, 18f);
		_menuData.mRankChangeImgRect = new Rect(1022f, 6f, 15f, 15f);
		_menuData.mGoldRect = new Rect(616f, 4f, 25f, 18f);
		_menuData.mSilverRect = new Rect(651f, 4f, 25f, 18f);
		_menuData.mBronzeRect = new Rect(686f, 4f, 25f, 18f);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mIconImgRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mNameRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mAvaLvlRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mKillRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mKillAssistRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mDeathRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mHonorExpRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mRankImgRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mNewRatingRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mRankChangeImgRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mDeltaRatingRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mGoldRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mSilverRect);
		GuiSystem.SetChildRect(_menuData.mFrameImgRect, ref _menuData.mBronzeRect);
	}

	private void SetTeamRects()
	{
		foreach (KeyValuePair<int, BattleEndTeamData> mTeamsDatum in mTeamsData)
		{
			Rect baseRect = ((mTeamsDatum.Key != 1) ? mTeam2FrameRect : mTeam1FrameRect);
			mTeamsDatum.Value.mTeamNameRect = new Rect(28f, 4f, 130f, 18f);
			mTeamsDatum.Value.mKillRect = new Rect(240f, 4f, 81f, 18f);
			mTeamsDatum.Value.mKillAssistRect = new Rect(323f, 4f, 75f, 18f);
			mTeamsDatum.Value.mDeathRect = new Rect(400f, 4f, 71f, 18f);
			mTeamsDatum.Value.mGoldRect = new Rect(616f, 4f, 25f, 18f);
			mTeamsDatum.Value.mSilverRect = new Rect(651f, 4f, 25f, 18f);
			mTeamsDatum.Value.mBronzeRect = new Rect(686f, 4f, 25f, 18f);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mTeamNameRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mKillRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mKillAssistRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mDeathRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mGoldRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mSilverRect);
			GuiSystem.SetChildRect(baseRect, ref mTeamsDatum.Value.mBronzeRect);
		}
	}

	private void SortBattleData()
	{
		BattleEndComparer comparer = new BattleEndComparer(mMenuType, mSortType, mInverse);
		mBattleEndData.Sort(comparer);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (("GAME_END_EXIT_BUTTON" == _sender.mElementId || _sender.mElementId == "CLOSE_BUTTON") && _buttonId == 0)
		{
			SetActive(_active: false);
			UserLog.AddAction(UserActionType.BATTLE_END);
			if (mExitCallback != null)
			{
				mExitCallback();
			}
		}
		else if ("GAME_END_MODE_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			mMenuType = (MenuType)_sender.mId;
			SetModeButtonImages();
			SortBattleData();
			SetRects();
		}
		else if ("SORT_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			if (_sender.mId == (int)mSortType)
			{
				mInverse = -mInverse;
			}
			else
			{
				mSortType = (SortType)_sender.mId;
			}
			SortBattleData();
			SetRects();
		}
	}

	private void InitButtons()
	{
		mExitButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mExitButton.mElementId = "GAME_END_EXIT_BUTTON";
		GuiButton guiButton = mExitButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mExitButton.mLabel = GuiSystem.GetLocaleText("GAME_END_MENU_LABEL");
		mExitButton.Init();
		mMode1Button = new GuiButton();
		mMode1Button.mId = 0;
		mMode1Button.mElementId = "GAME_END_MODE_BUTTON";
		GuiButton guiButton2 = mMode1Button;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mMode1Button.mLabel = GuiSystem.GetLocaleText("BY_TEAMS_TEXT");
		mMode2Button = new GuiButton();
		mMode2Button.mId = 1;
		mMode2Button.mElementId = "GAME_END_MODE_BUTTON";
		GuiButton guiButton3 = mMode2Button;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mMode2Button.mLabel = GuiSystem.GetLocaleText("BY_AVATARS_TEXT");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton4 = mCloseButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		GuiButton guiButton5 = null;
		for (int i = 0; i <= 9; i++)
		{
			guiButton5 = GuiSystem.CreateButton(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			guiButton5.mId = i;
			guiButton5.mElementId = "SORT_BUTTON";
			GuiButton guiButton6 = guiButton5;
			guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton5.mLabel = GuiSystem.GetLocaleText(((SortType)i).ToString() + "_TEXT");
			guiButton5.mNormColor = new Color(0.75f, 0.75f, 0.75f, 1f);
			guiButton5.mOverColor = new Color(253f / 255f, 194f / 255f, 20f / 51f, 1f);
			guiButton5.Init();
			mSortButtons.Add(guiButton5);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetRects();
		}
	}
}
