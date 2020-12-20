using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class HeroInfo : GuiInputListener, EscapeListener
{
	public enum MenuTab
	{
		TAB_ITEMS = 1,
		TAB_STATS,
		TAB_PLAYER_INFO,
		TAB_AVATARS,
		TAB_PRESENTS
	}

	public delegate void DressCallback(int _itemId, int _slot);

	public delegate void UndressCallback(int _slot);

	public DressCallback mDressCallback;

	public UndressCallback mUndressCallback;

	public Action<string> mClanInfoCallback;

	private List<GuiButton> mTabButtons;

	private MenuTab mCurTab = MenuTab.TAB_ITEMS;

	private GuiButton mCloseButton;

	private Texture2D mBgFrame;

	private Texture2D mStatFrame;

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private Texture2D mFrame3;

	private Texture2D mExpFrame;

	private Texture2D mWinFrame;

	private Texture2D mLossFrame;

	private Texture2D mWinLoseThumb;

	private string mPlayerName = string.Empty;

	private string mRaitingText = string.Empty;

	private string mPrestigeText = string.Empty;

	private string mKillsText = string.Empty;

	private string mDeathsText = string.Empty;

	private string mAssistsText = string.Empty;

	private string mGamesText = string.Empty;

	private string mDiscsText = string.Empty;

	private string mCreepKillsText = string.Empty;

	private string mHonorText = string.Empty;

	private string mLevelString = string.Empty;

	private Rect mFrame1Rect;

	private Rect mFrame2Rect;

	private Rect mFrame3Rect;

	private Rect mStatFrameRect;

	private Rect mHeroRect;

	private Rect mExpRect;

	private Rect mNameRect;

	private Rect mNameRectInitial;

	private Rect mLevelRect;

	private Rect mPlayerNameRect;

	private Rect mYearRect;

	private Rect mMonthRect;

	private Rect mDayRect;

	private Rect mCityRect;

	private Rect mAboutRect;

	private Dictionary<int, Rect> mItemsRect;

	private Dictionary<int, GuiButton> mHeroItems;

	private Rect mWinRect;

	private Rect mLosRect;

	private Rect mWinLosFrameRect;

	private Rect mRatingRect;

	private Rect mKillRect;

	private Rect mDeathRect;

	private Rect mAssistRect;

	private Rect mHonorRect;

	private Rect mWinLoseThumbRect;

	private Rect mPrestigeRect;

	private Rect mGamesRect;

	private Rect mDiscRect;

	private Rect mCreepKillRect;

	private Rect mCreepKillCntRect;

	private Dictionary<int, int> mItemIdToArticles;

	private Dictionary<string, AvaParamData> mAvaParamData;

	private GuiButton mClanTagButton;

	private GameObject mHeroGameObject;

	private GameObject mHeroPreviewScene;

	private GameObject mHeroCamera;

	private RenderTexture mRenderHeroTexture;

	private GuiButton mHeroRotButton1;

	private GuiButton mHeroRotButton2;

	private float mHeroRotateSpeed = 2f;

	private bool mSelfHero;

	private int mCurHeroId;

	private HeroGameInfo mHeroGameInfo;

	private HeroDataListArg.HeroDataItem mHeroDataItem;

	private IStoreContentProvider<CtrlPrototype> mItemArticles;

	private bool mDragStarted;

	private Vector2 mDragPos = Vector2.zero;

	private FormatedTipMgr mFormatedTipMgr;

	private int mLastTipId;

	private HeroMgr mHeroMgr;

	private HeroStore mHeroStore;

	private bool mRecalc;

	private BuffRenderer mBuffRenderer = new BuffRenderer();

	private GuiElement mTipItemTab;

	private GuiElement mTipStatsTab;

	public HeroGameInfo HeroGameInfo
	{
		set
		{
			if (value != null)
			{
				mHeroGameInfo = value;
			}
		}
	}

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			Close();
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mDragType = GuiInputMgr.ButtonDragFlags.HERO_INFO_DRAG;
		GuiSystem.mGuiInputMgr.AddGuiDraggableButtonListener(this);
		GuiSystem.mGuiInputMgr.AddEscapeListener(150, this);
		mTabButtons = new List<GuiButton>();
		mItemsRect = new Dictionary<int, Rect>();
		mHeroItems = new Dictionary<int, GuiButton>();
		mBgFrame = GuiSystem.GetImage("Gui/HeroInfo/tabl_01");
		mStatFrame = GuiSystem.GetImage("Gui/HeroInfo/tabl_02");
		mFrame1 = GuiSystem.GetImage("Gui/HeroInfo/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/HeroInfo/frame2");
		mFrame3 = GuiSystem.GetImage("Gui/HeroInfo/frame3");
		mExpFrame = GuiSystem.GetImage("Gui/HeroInfo/el_02");
		mWinFrame = GuiSystem.GetImage("Gui/HeroInfo/elem_10");
		mLossFrame = GuiSystem.GetImage("Gui/HeroInfo/elem_11");
		mWinLoseThumb = GuiSystem.GetImage("Gui/HeroInfo/elem_08");
		mRaitingText = GuiSystem.GetLocaleText("RAITING_TEXT");
		mKillsText = GuiSystem.GetLocaleText("KILLS_TEXT");
		mDeathsText = GuiSystem.GetLocaleText("DEATHS_TEXT");
		mAssistsText = GuiSystem.GetLocaleText("ASSISTS_TEXT");
		mPrestigeText = GuiSystem.GetLocaleText("PRESTIGE_TEXT");
		mGamesText = GuiSystem.GetLocaleText("GAMES_TEXT");
		mDiscsText = GuiSystem.GetLocaleText("DISCS_TEXT");
		mCreepKillsText = GuiSystem.GetLocaleText("CREEP_KILLS_TEXT");
		mHonorText = GuiSystem.GetLocaleText("DOBLEST_TEXT");
		mLevelString = GuiSystem.GetLocaleText("Level_Text");
		GuiButton guiButton = null;
		for (MenuTab menuTab = MenuTab.TAB_ITEMS; menuTab <= MenuTab.TAB_PRESENTS; menuTab++)
		{
			guiButton = GuiSystem.CreateButton("Gui/HeroInfo/Zn_0" + (int)menuTab + "_1", "Gui/HeroInfo/Zn_0" + (int)menuTab + "_2", "Gui/HeroInfo/Zn_0" + (int)menuTab + "_3", string.Empty, string.Empty);
			guiButton.mId = (int)menuTab;
			guiButton.mElementId = "TAB_BUTTON";
			GuiButton guiButton2 = guiButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			guiButton.mLocked = menuTab == MenuTab.TAB_AVATARS || menuTab == MenuTab.TAB_PRESENTS || menuTab == MenuTab.TAB_PLAYER_INFO;
			guiButton.mLockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			mTabButtons.Add(guiButton);
		}
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton3 = mCloseButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mClanTagButton = new GuiButton();
		mClanTagButton.mElementId = "CLAN_TAG_BUTTON";
		GuiButton guiButton4 = mClanTagButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mClanTagButton.mLabelStyle = "label_left";
		mClanTagButton.mOverColor = new Color(14f / 15f, 0.235294119f, 0.294117659f);
		mClanTagButton.Init();
		AddTutorialElement(mCloseButton);
		mRenderHeroTexture = new RenderTexture(512, 512, 24);
		mRenderHeroTexture.format = RenderTextureFormat.ARGB32;
		mRenderHeroTexture.useMipMap = false;
		mRenderHeroTexture.isCubemap = false;
		mRenderHeroTexture.Create();
		mItemIdToArticles = new Dictionary<int, int>();
		mAvaParamData = new Dictionary<string, AvaParamData>();
		string[] names = Enum.GetNames(typeof(AvatarInfoWindow.ParamTypes));
		foreach (string key in names)
		{
			mAvaParamData.Add(key, new AvaParamData());
		}
		mHeroRotButton1 = GuiSystem.CreateButton("Gui/misc/str_03_1", "Gui/misc/str_03_2", "Gui/misc/str_03_3", string.Empty, string.Empty);
		mHeroRotButton1.mElementId = "HERO_ROT_BUTTON_1";
		GuiButton guiButton5 = mHeroRotButton1;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mHeroRotButton1.Init();
		mHeroRotButton2 = GuiSystem.CreateButton("Gui/misc/str_04_1", "Gui/misc/str_04_2", "Gui/misc/str_04_3", string.Empty, string.Empty);
		mHeroRotButton2.mElementId = "HERO_ROT_BUTTON_2";
		GuiButton guiButton6 = mHeroRotButton2;
		guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
		mHeroRotButton2.Init();
		mTipItemTab = new GuiElement();
		mTipStatsTab = new GuiElement();
		mTipItemTab.mGuiSetId = mGuiSetId;
		mTipStatsTab.mGuiSetId = mGuiSetId;
		mTipItemTab.SetActive(_active: false);
		mTipStatsTab.SetActive(_active: false);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 50f, mBgFrame.width, mBgFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mFrame1Rect = new Rect(16f, 45f, mFrame1.width, mFrame1.height);
		mFrame2Rect = new Rect(24f, 45f, mFrame2.width, mFrame2.height);
		mFrame3Rect = new Rect(37f, 38f, mFrame3.width, mFrame3.height);
		mStatFrameRect = new Rect(-7f, 477f, mStatFrame.width, mStatFrame.height);
		mHeroRect = new Rect(23f, 90f, 328f, 328f);
		mExpRect = new Rect(30f, 50f, 315f, 16f);
		mNameRect = new Rect(104f, 3f, 175f, 18f);
		mNameRectInitial = new Rect(104f, 3f, 175f, 18f);
		mLevelRect = new Rect(136f, 30f, 100f, 13f);
		mPlayerNameRect = new Rect(31f, 2f, 265f, 14f);
		mYearRect = new Rect(268f, 32f, 32f, 14f);
		mMonthRect = new Rect(243f, 32f, 18f, 14f);
		mDayRect = new Rect(218f, 32f, 18f, 14f);
		mCityRect = new Rect(41f, 60f, 256f, 14f);
		mAboutRect = new Rect(10f, 166f, 278f, 243f);
		mClanTagButton.mZoneRect = new Rect(104f, 3f, 64f, 18f);
		mBuffRenderer.mSize = 30f;
		mBuffRenderer.mStartX = 85f;
		mBuffRenderer.mStartY = 360f;
		mBuffRenderer.mOffsetX = 180f;
		mBuffRenderer.mOffsetY = -35f;
		mBuffRenderer.mRowSize = 2f;
		GuiSystem.SetChildRect(mZoneRect, ref mFrame1Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mFrame2Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mFrame3Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mStatFrameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mHeroRect);
		GuiSystem.SetChildRect(mZoneRect, ref mExpRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNameRectInitial);
		GuiSystem.SetChildRect(mZoneRect, ref mClanTagButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mPlayerNameRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mYearRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mMonthRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mDayRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mCityRect);
		GuiSystem.SetChildRect(mFrame3Rect, ref mAboutRect);
		int i = 0;
		for (int count = mTabButtons.Count; i < count; i++)
		{
			GuiButton guiButton = mTabButtons[i];
			guiButton.mZoneRect = new Rect(351f, 76 + i * 34, 33f, 33f);
			GuiSystem.SetChildRect(mZoneRect, ref guiButton.mZoneRect);
		}
		mCloseButton.mZoneRect = new Rect(340f, 10f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mWinRect = new Rect(5f, 57f, 21f, 21f);
		mLosRect = new Rect(298f, 57f, 21f, 21f);
		mWinLosFrameRect = new Rect(30f, 64f, 264f, 11f);
		mWinLoseThumbRect = new Rect(0f, mWinLosFrameRect.y + (mWinLosFrameRect.height - (float)mWinLoseThumb.height) / 2f, mWinLoseThumb.width, mWinLoseThumb.height);
		mRatingRect = new Rect(10f, 31f, 175f, 16f);
		mKillRect = new Rect(10f, 140f, 175f, 16f);
		mDeathRect = new Rect(10f, 165f, 175f, 16f);
		mAssistRect = new Rect(10f, 190f, 175f, 16f);
		mHonorRect = new Rect(10f, 113f, 175f, 14f);
		mPrestigeRect = new Rect(105f, 85f, 115f, 16f);
		mGamesRect = new Rect(10f, 252f, 132f, 16f);
		mDiscRect = new Rect(10f, 272f, 132f, 16f);
		mCreepKillRect = new Rect(180f, 252f, 135f, 16f);
		mCreepKillCntRect = new Rect(180f, 268f, 135f, 16f);
		mHeroRotButton1.mZoneRect = new Rect(205f, 71f, 16f, 17f);
		GuiSystem.SetChildRect(mZoneRect, ref mHeroRotButton1.mZoneRect);
		mHeroRotButton2.mZoneRect = new Rect(155f, 71f, 16f, 17f);
		GuiSystem.SetChildRect(mZoneRect, ref mHeroRotButton2.mZoneRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mWinRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mLosRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mWinLosFrameRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mWinLoseThumbRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mRatingRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mKillRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mDeathRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mAssistRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mPrestigeRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mGamesRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mDiscRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mCreepKillRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mCreepKillCntRect);
		GuiSystem.SetChildRect(mFrame2Rect, ref mHonorRect);
		SetItemsRect();
		foreach (KeyValuePair<int, GuiButton> mHeroItem in mHeroItems)
		{
			if (mItemsRect.ContainsKey(mHeroItem.Key))
			{
				mHeroItem.Value.mZoneRect = mItemsRect[mHeroItem.Key];
			}
		}
		Color mColor = new Color(208f / 255f, 113f / 255f, 21f / 85f);
		Color mColor2 = new Color(16f / 51f, 158f / 255f, 193f / 255f);
		Color mColor3 = new Color(169f / 255f, 52f / 255f, 46f / 255f);
		foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
		{
			switch (mAvaParamDatum.Key)
			{
			case "Health":
				mAvaParamDatum.Value.mDrawRect = new Rect(21f, 45f, 102f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "HealthRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(65f, 61f, 58f, 12f);
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "DamageMin":
				mAvaParamDatum.Value.mDrawRect = new Rect(143f, 45f, 102f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "AttackSpeed":
				mAvaParamDatum.Value.mDrawRect = new Rect(206f, 61f, 45f, 12f);
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "Mana":
				mAvaParamDatum.Value.mDrawRect = new Rect(265f, 45f, 102f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "ManaRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(309f, 61f, 58f, 12f);
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "PhysArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(159f, 116f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiPhysArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(282f, 116f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "MagicArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(159f, 132f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiMagicArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(282f, 132f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "AntiCritChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(159f, 149f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "CritChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(282f, 149f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "DodgeChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(159f, 165f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiDodgeChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(282f, 165f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "BlockChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(159f, 181f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiBlock":
				mAvaParamDatum.Value.mDrawRect = new Rect(282f, 181f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			}
			GuiSystem.SetChildRect(mStatFrameRect, ref mAvaParamDatum.Value.mDrawRect);
			PopupInfo.AddTip(mTipItemTab, GetAvatarParamTipTextId(mAvaParamDatum.Key), mAvaParamDatum.Value.mDrawRect);
		}
		mBuffRenderer.mBaseRect = mZoneRect;
		PopupInfo.AddTip(this, "TIP_TEXT55", mExpRect);
		PopupInfo.AddTip(this, "TIP_TEXT56", mTabButtons[0]);
		PopupInfo.AddTip(this, "TIP_TEXT57", mTabButtons[1]);
		Rect _rect = new Rect(11f, 9f, 121f, 72f);
		GuiSystem.SetChildRect(mStatFrameRect, ref _rect);
		PopupInfo.AddTip(mTipItemTab, "TIP_TEXT72", _rect);
		_rect = new Rect(133f, 9f, 121f, 72f);
		GuiSystem.SetChildRect(mStatFrameRect, ref _rect);
		PopupInfo.AddTip(mTipItemTab, "TIP_TEXT73", _rect);
		_rect = new Rect(255f, 9f, 121f, 72f);
		GuiSystem.SetChildRect(mStatFrameRect, ref _rect);
		PopupInfo.AddTip(mTipItemTab, "TIP_TEXT74", _rect);
		_rect = new Rect(2f, 56f, 27f, 27f);
		GuiSystem.SetChildRect(mFrame2Rect, ref _rect);
		PopupInfo.AddTip(mTipStatsTab, "TIP_TEXT57_1", _rect);
		_rect = new Rect(295f, 59f, 21f, 21f);
		GuiSystem.SetChildRect(mFrame2Rect, ref _rect);
		PopupInfo.AddTip(mTipStatsTab, "TIP_TEXT57_2", _rect);
		_rect = new Rect(2f, 30f, 193f, 22f);
		GuiSystem.SetChildRect(mFrame2Rect, ref _rect);
		PopupInfo.AddTip(mTipStatsTab, "TIP_TEXT57_3", _rect);
		_rect = new Rect(97f, 84f, 132f, 22f);
		GuiSystem.SetChildRect(mFrame2Rect, ref _rect);
		PopupInfo.AddTip(mTipStatsTab, "TIP_TEXT57_4", _rect);
		_rect = new Rect(3f, 111f, 189f, 22f);
		GuiSystem.SetChildRect(mFrame2Rect, ref _rect);
		PopupInfo.AddTip(mTipStatsTab, "TIP_TEXT57_5", _rect);
	}

	public void Close()
	{
		mCurTab = MenuTab.TAB_ITEMS;
		GameObjUtil.DestroyGameObject(ref mHeroGameObject);
		GameObjUtil.DestroyGameObject(ref mHeroCamera);
		GameObjUtil.DestroyGameObject(ref mHeroPreviewScene);
		if (mFormatedTipMgr != null && mLastTipId != 0)
		{
			mFormatedTipMgr.Hide(mLastTipId);
		}
		mLastTipId = 0;
		if (mTipItemTab != null)
		{
			mTipItemTab.SetActive(_active: false);
		}
		if (mTipStatsTab != null)
		{
			mTipStatsTab.SetActive(_active: false);
		}
		SetActive(_active: false);
	}

	public void Open()
	{
		SetActive(_active: true);
		SetCurTab(mCurTab);
		CreateHeroPreviewScene();
		CreateHeroCamera();
	}

	private string GetAvatarParamTipTextId(string paramId)
	{
		return paramId switch
		{
			"PhysArmor" => "TIP_TEXT75", 
			"AntiPhysArmor" => "TIP_TEXT76", 
			"MagicArmor" => "TIP_TEXT77", 
			"AntiMagicArmor" => "TIP_TEXT78", 
			"CritChance" => "TIP_TEXT79", 
			"AntiCritChance" => "TIP_TEXT80", 
			"DodgeChance" => "TIP_TEXT81", 
			"AntiDodgeChance" => "TIP_TEXT82", 
			"BlockChance" => "TIP_TEXT83", 
			"AntiBlock" => "TIP_TEXT84", 
			_ => string.Empty, 
		};
	}

	public override void CheckEvent(Event _curEvent)
	{
		mBuffRenderer.CheckEvent(_curEvent);
		foreach (GuiButton mTabButton in mTabButtons)
		{
			mTabButton.CheckEvent(_curEvent);
		}
		if (mCurTab == MenuTab.TAB_ITEMS)
		{
			foreach (KeyValuePair<int, GuiButton> mHeroItem in mHeroItems)
			{
				mHeroItem.Value.CheckEvent(_curEvent);
			}
			mHeroRotButton1.CheckEvent(_curEvent);
			mHeroRotButton2.CheckEvent(_curEvent);
			if (_curEvent.type == EventType.MouseDown && mHeroRect.Contains(_curEvent.mousePosition))
			{
				mDragPos = _curEvent.mousePosition;
				mDragStarted = true;
			}
			else if (_curEvent.type == EventType.MouseUp && mDragStarted)
			{
				mDragPos = Vector2.zero;
				mDragStarted = false;
			}
			else if (_curEvent.type == EventType.MouseDrag && mHeroRect.Contains(_curEvent.mousePosition) && mDragStarted)
			{
				float x = (mDragPos - _curEvent.mousePosition).x;
				RotateHeroPreview(x);
				mDragPos = _curEvent.mousePosition;
			}
			if (mHeroRotButton1.mCurBtnState == GuiButton.GuiButtonStates.BTN_PRESS)
			{
				RotateHeroPreview(0f - mHeroRotateSpeed);
			}
			else if (mHeroRotButton2.mCurBtnState == GuiButton.GuiButtonStates.BTN_PRESS)
			{
				RotateHeroPreview(mHeroRotateSpeed);
			}
		}
		mCloseButton.CheckEvent(_curEvent);
		mClanTagButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mBgFrame, mZoneRect);
		mCloseButton.RenderElement();
		GuiSystem.DrawString(mPlayerName, mNameRect, "label");
		if (!string.IsNullOrEmpty(mClanTagButton.mLabel))
		{
			mClanTagButton.RenderElement();
		}
		switch (mCurTab)
		{
		case MenuTab.TAB_ITEMS:
			RenderItemsTab();
			break;
		case MenuTab.TAB_STATS:
			RenderStatsTab();
			break;
		}
		foreach (GuiButton mTabButton in mTabButtons)
		{
			mTabButton.RenderElement();
		}
		if (mRecalc)
		{
			GUIStyle style = GUI.skin.GetStyle("label_left");
			Vector2 vector = style.CalcSize(new GUIContent(mPlayerName));
			Vector2 vector2 = style.CalcSize(new GUIContent(mClanTagButton.mLabel));
			mNameRect = new Rect(mNameRectInitial.xMin + vector2.x / 2f, mNameRectInitial.yMin, mNameRectInitial.width, mNameRectInitial.height);
			float left = mNameRect.xMin + mNameRect.width / 2f - vector2.x - vector.x / 2f;
			mClanTagButton.mZoneRect = new Rect(left, mNameRect.yMin, vector2.x, mNameRect.height);
			mRecalc = false;
		}
	}

	public override void Update()
	{
		mBuffRenderer.Update();
	}

	public override void Uninit()
	{
		base.Uninit();
		mSelfHero = false;
		mFormatedTipMgr = null;
		mHeroMgr = null;
		mItemArticles = null;
		mBuffRenderer.Clean();
		mItemIdToArticles.Clear();
		mHeroItems.Clear();
	}

	public void UpdateHeroPreview()
	{
		if (mHeroGameObject == null || mHeroDataItem == null)
		{
			return;
		}
		HeroWear componentInChildren = mHeroGameObject.GetComponentInChildren<HeroWear>();
		componentInChildren.SetDefault((HeroRace)mHeroDataItem.mView.mRace, mHeroDataItem.mView.mGender);
		componentInChildren.SetHero(mHeroDataItem.mView);
		List<string> list = new List<string>();
		CtrlPrototype ctrlPrototype = null;
		foreach (DressedItemsArg.DressedItem mItem in mHeroDataItem.mItems)
		{
			ctrlPrototype = mItemArticles.TryGet(mItem.mArticleId);
			if (ctrlPrototype != null)
			{
				list.Add(ctrlPrototype.Prefab.mValue);
			}
		}
		componentInChildren.SetItems(list);
	}

	private void CreateHeroObject()
	{
		if (!(mHeroMgr == null) && mHeroDataItem != null)
		{
			HeroMgr.CreateHeroData createHeroData = new HeroMgr.CreateHeroData();
			createHeroData.mOnHeroLoaded = (HeroMgr.OnHeroLoaded)Delegate.Combine(createHeroData.mOnHeroLoaded, new HeroMgr.OnHeroLoaded(OnHeroLoaded));
			mHeroMgr.CreateHero((HeroRace)mHeroDataItem.mView.mRace, mHeroDataItem.mView.mGender, createHeroData);
		}
	}

	private void OnHeroLoaded(GameObject _hero, object _data)
	{
		if (_hero == null)
		{
			return;
		}
		Vector3 eulerAngles = Vector3.zero;
		if (mHeroGameObject != null)
		{
			eulerAngles = mHeroGameObject.transform.eulerAngles;
		}
		GameObject gameObject = new GameObject("HeroPreviewObject");
		_hero.transform.parent = gameObject.transform;
		HeroWear component = _hero.GetComponent<HeroWear>();
		component.SetDefault((HeroRace)mHeroDataItem.mView.mRace, mHeroDataItem.mView.mGender);
		component.SetHero(mHeroDataItem.mView);
		AnimationExt component2 = _hero.GetComponent<AnimationExt>();
		UnityEngine.Object.DestroyImmediate(component2);
		Animation component3 = _hero.GetComponent<Animation>();
		component3.wrapMode = WrapMode.Loop;
		component3.Play("Idle1");
		List<string> list = new List<string>();
		CtrlPrototype ctrlPrototype = null;
		foreach (DressedItemsArg.DressedItem mItem in mHeroDataItem.mItems)
		{
			ctrlPrototype = mItemArticles.TryGet(mItem.mArticleId);
			if (ctrlPrototype != null)
			{
				list.Add(ctrlPrototype.Prefab.mValue);
			}
		}
		component.SetItems(list);
		gameObject.transform.position = new Vector3(0f, 1000f, 0f);
		gameObject.transform.Rotate(eulerAngles);
		GameObjUtil.TrySetParent(gameObject, "/level");
		SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			skinnedMeshRenderer.gameObject.layer = 14;
			skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		GameObjUtil.DestroyGameObject(ref mHeroGameObject);
		mHeroGameObject = gameObject;
	}

	private void CreateHeroPreviewScene()
	{
		if (!(mHeroPreviewScene != null) && mHeroDataItem != null)
		{
			string path = "Prefabs/HeroPreview/HeroPreviewScene_" + ((mHeroDataItem.mView.mRace != 1) ? "Elf" : "Human");
			GameObject original = Resources.Load(path) as GameObject;
			mHeroPreviewScene = UnityEngine.Object.Instantiate(original) as GameObject;
			mHeroPreviewScene.transform.position = new Vector3(0f, 999.86f, 0f);
			mHeroPreviewScene.transform.Rotate(new Vector3(0f, 180f, 0f));
			Renderer[] componentsInChildren = mHeroPreviewScene.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.gameObject.layer = 14;
			}
			GameObjUtil.TrySetParent(mHeroPreviewScene, "/level");
		}
	}

	private void CreateHeroCamera()
	{
		if (!(mHeroCamera != null))
		{
			mHeroCamera = new GameObject("HeroPreviewCamera");
			Camera camera = mHeroCamera.AddComponent<Camera>();
			camera.targetTexture = mRenderHeroTexture;
			camera.orthographic = false;
			camera.fieldOfView = 30f;
			camera.aspect = 1f;
			camera.backgroundColor = Color.black;
			camera.cullingMask = 16384;
			camera.renderingPath = RenderingPath.VertexLit;
			mHeroCamera.transform.position = new Vector3(0f, 1000.9f, 3.98f);
			mHeroCamera.transform.eulerAngles = new Vector3(0f, 180f, 0f);
			GameObjUtil.TrySetParent(mHeroCamera, "/cameras");
		}
	}

	private void InitHeroItems()
	{
		if (mHeroDataItem == null || mHeroItems == null)
		{
			return;
		}
		mHeroItems.Clear();
		foreach (DressedItemsArg.DressedItem mItem in mHeroDataItem.mItems)
		{
			CtrlPrototype ctrlPrototype = mItemArticles.Get(mItem.mArticleId);
			if (ctrlPrototype != null)
			{
				if (!mItemIdToArticles.ContainsKey(mItem.mId))
				{
					mItemIdToArticles.Add(mItem.mId, mItem.mArticleId);
				}
				GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
				draggableButton.mObjectType = GuiInputMgr.ObjectType.ITEM;
				draggableButton.mData = new GuiInputMgr.DragItemData();
				draggableButton.mData.mCount = mItem.mCount;
				draggableButton.mButton = GuiSystem.CreateButton(string.Empty, string.Empty, string.Empty, "Gui/Icons/Items/" + ctrlPrototype.Desc.mIcon, string.Empty);
				draggableButton.mButton.mId = mItem.mId;
				draggableButton.mButton.mIconOnTop = false;
				GuiButton mButton = draggableButton.mButton;
				mButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton mButton2 = draggableButton.mButton;
				mButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				draggableButton.mButton.mLockedColor = Color.grey;
				draggableButton.mButton.mLocked = !mSelfHero;
				AddDraggableButton(draggableButton, mItem.mSlot, _add: true);
			}
		}
	}

	public void RemoveItem(int _slot)
	{
		if (mHeroItems != null && mHeroDataItem != null)
		{
			if (mHeroItems.TryGetValue(_slot, out var value))
			{
				RemoveDraggableButton(value.mDraggableButton, _add: true);
			}
			mHeroDataItem.RemoveItemBySlot(_slot);
			UpdateHeroPreview();
		}
	}

	public void AddItem(CtrlThing _item, int _slot, bool _hero)
	{
		if (_item == null || mHeroDataItem == null)
		{
			return;
		}
		CtrlPrototype ctrlProto = _item.CtrlProto;
		if (ctrlProto != null)
		{
			int num = ((_item.Id != -1) ? _item.Id : ctrlProto.Id);
			DressedItemsArg.DressedItem dressedItem = new DressedItemsArg.DressedItem();
			dressedItem.mId = num;
			dressedItem.mArticleId = ctrlProto.Id;
			dressedItem.mCount = 1;
			dressedItem.mSlot = _slot;
			mHeroDataItem.AddItem(dressedItem);
			if (!mItemIdToArticles.ContainsKey(num))
			{
				mItemIdToArticles.Add(num, ctrlProto.Id);
			}
			GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
			draggableButton.mObjectType = GuiInputMgr.ObjectType.ITEM;
			draggableButton.mData = new GuiInputMgr.DragItemData();
			draggableButton.mData.mCount = _item.Count;
			draggableButton.mButton = GuiSystem.CreateButton(string.Empty, string.Empty, string.Empty, "Gui/Icons/Items/" + ctrlProto.Desc.mIcon, string.Empty);
			draggableButton.mButton.mId = num;
			draggableButton.mButton.mIconOnTop = false;
			GuiButton mButton = draggableButton.mButton;
			mButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton mButton2 = draggableButton.mButton;
			mButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			draggableButton.mButton.mLockedColor = Color.grey;
			draggableButton.mButton.mLocked = !_hero;
			AddDraggableButton(draggableButton, _slot, _add: true);
			UpdateHeroPreview();
		}
	}

	private void InitGameInfo()
	{
		if (mHeroGameInfo != null && mHeroGameInfo.mAddStats != null)
		{
			mAvaParamData["Health"].mValue = GetStringParamValue("Health");
			mAvaParamData["HealthRegen"].mValue = GetStringParamValue("HealthRegen");
			mAvaParamData["DamageMin"].mValue = GetStringParamValue("DamageMin");
			mAvaParamData["AttackSpeed"].mValue = GetStringParamValue("AttackSpeed");
			mAvaParamData["Mana"].mValue = GetStringParamValue("Mana");
			mAvaParamData["ManaRegen"].mValue = GetStringParamValue("ManaRegen");
			mAvaParamData["PhysArmor"].mValue = GetStringParamValue("PhysArmor");
			mAvaParamData["AntiPhysArmor"].mValue = GetStringParamValue("AntiPhysArmor");
			mAvaParamData["MagicArmor"].mValue = GetStringParamValue("MagicArmor");
			mAvaParamData["AntiMagicArmor"].mValue = GetStringParamValue("AntiMagicArmor");
			mAvaParamData["CritChance"].mValue = GetStringParamValue("CritChance");
			mAvaParamData["AntiCritChance"].mValue = GetStringParamValue("AntiCritChance");
			mAvaParamData["DodgeChance"].mValue = GetStringParamValue("DodgeChance");
			mAvaParamData["AntiDodgeChance"].mValue = GetStringParamValue("AntiDodgeChance");
			mAvaParamData["BlockChance"].mValue = GetStringParamValue("BlockChance");
			mAvaParamData["AntiBlock"].mValue = GetStringParamValue("AntiBlock");
			if (!string.IsNullOrEmpty(mHeroGameInfo.mClanTag))
			{
				mClanTagButton.mLabel = "[" + mHeroGameInfo.mClanTag + "]";
			}
			else
			{
				mClanTagButton.mLabel = string.Empty;
			}
		}
	}

	private string GetStringParamValue(string _param)
	{
		if (!mHeroGameInfo.mAddStats.ContainsKey(_param))
		{
			return "--";
		}
		string empty = string.Empty;
		AddStat addStat = mHeroGameInfo.mAddStats[_param];
		switch (_param)
		{
		case "Health":
		case "Mana":
		case "DamageMin":
			empty = "+" + addStat.mAdd.ToString("0.##");
			break;
		case "HealthRegen":
		case "AttackSpeed":
		case "ManaRegen":
			empty = "+" + addStat.mAdd.ToString("0.##");
			break;
		case "CritChance":
			empty = "+" + (addStat.mAdd * 100f).ToString("0.##") + "%";
			if (mHeroGameInfo.mAddStats.ContainsKey("CritSize"))
			{
				empty = empty + "(x" + mHeroGameInfo.mAddStats["CritSize"].mAdd.ToString("0.##") + ")";
			}
			break;
		case "BlockChance":
			empty = "+" + (addStat.mAdd * 100f).ToString("0.##") + "%";
			if (mHeroGameInfo.mAddStats.ContainsKey("BlockSize"))
			{
				empty = empty + "(" + mHeroGameInfo.mAddStats["BlockSize"].mAdd.ToString("0.##") + ")";
			}
			break;
		default:
			empty = "+" + (addStat.mAdd * 100f).ToString("0.##") + "%";
			break;
		}
		return empty;
	}

	public static string ConvertMulToString(float _mul)
	{
		return ConvertMulToFloat(_mul).ToString("0.##");
	}

	public static string FloatToFormatString(float _mul)
	{
		return _mul.ToString("0.##");
	}

	public static float ConvertMulToFloat(float _mul)
	{
		return Mathf.Round((!(_mul > 0f)) ? 0f : (_mul * 100f - 100f));
	}

	public static float ConvertMulToInverseMul(float _mul)
	{
		return (!(_mul > 0f)) ? 0f : (_mul - 1f);
	}

	public override int GetZoneNum(Vector2 _pos)
	{
		if (!mFrame1Rect.Contains(_pos))
		{
			return -1;
		}
		foreach (KeyValuePair<int, Rect> item in mItemsRect)
		{
			if (item.Value.Contains(_pos))
			{
				return item.Key;
			}
		}
		return 0;
	}

	public override bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _num)
	{
		if (!mSelfHero)
		{
			return false;
		}
		if (_num == -1)
		{
			return AddDraggableButton(_btn, 0, _add: false);
		}
		return AddDraggableButton(_btn, _num, _add: false);
	}

	public bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _mask, bool _add)
	{
		if (_btn == null)
		{
			Log.Error("Trying to add null dragged button");
			return false;
		}
		if ((_mask > 0 && !mItemsRect.ContainsKey(_mask)) || (_mask == 0 && _add))
		{
			return false;
		}
		if (!_add)
		{
			if (mDressCallback != null)
			{
				mDressCallback(_btn.mButton.mId, _mask);
			}
			return false;
		}
		GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
		draggableButton.mButtonHolder = this;
		draggableButton.mDragFlag = mDragType;
		draggableButton.mObjectType = _btn.mObjectType;
		draggableButton.mData = _btn.mData;
		draggableButton.mButton = new GuiButton();
		draggableButton.mButton.mElementId = "HERO_INFO_ITEM_BUTTON";
		draggableButton.mButton.mId = _btn.mButton.mId;
		draggableButton.mButton.mIconImg = _btn.mButton.mIconImg;
		draggableButton.mButton.mNormImg = _btn.mButton.mNormImg;
		draggableButton.mButton.mOverImg = _btn.mButton.mOverImg;
		draggableButton.mButton.mPressImg = _btn.mButton.mPressImg;
		draggableButton.mButton.mIconOnTop = _btn.mButton.mIconOnTop;
		draggableButton.mButton.mLocked = _btn.mButton.mLocked;
		draggableButton.mButton.mLockedColor = _btn.mButton.mLockedColor;
		if (!draggableButton.mButton.mLocked)
		{
			GuiButton mButton = draggableButton.mButton;
			mButton.mOnDragStart = (OnDragStart)Delegate.Combine(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
			GuiButton mButton2 = draggableButton.mButton;
			mButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton2.mOnMouseUp, new OnMouseUp(OnItem));
		}
		GuiButton mButton3 = draggableButton.mButton;
		mButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton3.mOnMouseEnter, _btn.mButton.mOnMouseEnter);
		GuiButton mButton4 = draggableButton.mButton;
		mButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton4.mOnMouseLeave, _btn.mButton.mOnMouseLeave);
		if (mItemsRect.ContainsKey(_mask))
		{
			draggableButton.mButton.mZoneRect = mItemsRect[_mask];
		}
		draggableButton.mButton.Init();
		draggableButton.mButton.mDraggableButton = draggableButton;
		if (mHeroItems.ContainsKey(_mask))
		{
			RemoveDraggableButton(mHeroItems[_mask].mDraggableButton, _add);
		}
		mHeroItems.Add(_mask, draggableButton.mButton);
		return true;
	}

	public override void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
		RemoveDraggableButton(_btn, _add: false);
	}

	public void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, bool _add)
	{
		foreach (KeyValuePair<int, GuiButton> mHeroItem in mHeroItems)
		{
			if (mHeroItem.Value == _btn.mButton)
			{
				if (mHeroItem.Value.mOnMouseLeave != null)
				{
					mHeroItem.Value.mOnMouseLeave(mHeroItem.Value);
				}
				mHeroItems.Remove(mHeroItem.Key);
				mItemIdToArticles.Remove(mHeroItem.Key);
				if (mUndressCallback != null && !_add)
				{
					mUndressCallback(mHeroItem.Key);
				}
				break;
			}
		}
	}

	private void SetItemsRect()
	{
		mItemsRect.Clear();
		for (int num = 1; num <= 8192; num <<= 1)
		{
			Rect _rect = default(Rect);
			if (num <= 32)
			{
				_rect = new Rect(13f, 54f + Mathf.Log(num, 2f) * 54f, 37f, 38f);
			}
			else if (num > 32 && num < 4096)
			{
				_rect = new Rect(294f, 54f + (Mathf.Log(num, 2f) - 6f) * 54f, 37f, 38f);
			}
			else
			{
				switch (num)
				{
				case 4096:
					_rect = new Rect(83f, 377f, 37f, 38f);
					break;
				case 8192:
					_rect = new Rect(224f, 377f, 37f, 38f);
					break;
				}
			}
			GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
			mItemsRect.Add(num, _rect);
			PopupInfo.AddTip(mTipItemTab, GetItemSlotTextId(num), _rect);
		}
	}

	private string GetItemSlotTextId(int _slot)
	{
		return _slot switch
		{
			1 => "TIP_TEXT58", 
			2 => "TIP_TEXT59", 
			4 => "TIP_TEXT60", 
			8 => "TIP_TEXT61", 
			16 => "TIP_TEXT62", 
			32 => "TIP_TEXT63", 
			64 => "TIP_TEXT64", 
			128 => "TIP_TEXT65", 
			256 => "TIP_TEXT66", 
			512 => "TIP_TEXT67", 
			1024 => "TIP_TEXT68", 
			2048 => "TIP_TEXT69", 
			4096 => "TIP_TEXT70", 
			8192 => "TIP_TEXT71", 
			_ => string.Empty, 
		};
	}

	private void OnBtnDragStart(GuiElement _sender)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null)
		{
			GuiSystem.mGuiInputMgr.AddDraggableButton(guiButton.mDraggableButton);
		}
	}

	private void OnItem(GuiElement _sender, int _buttonId)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null && _buttonId == 1)
		{
			GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.HERO_INFO_DRAG, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, _countAccept: true);
		}
	}

	private void RenderItemsTab()
	{
		if (mHeroGameInfo == null)
		{
			return;
		}
		float num = (float)mHeroGameInfo.mExp / (float)mHeroGameInfo.mNextExp;
		GuiSystem.DrawImage(mRenderHeroTexture, mHeroRect);
		Rect rect = new Rect(mExpRect.x, mExpRect.y, mExpRect.width * num, mExpRect.height);
		GuiSystem.DrawImage(mExpFrame, rect, (int)rect.width, 0, 0, 0);
		GuiSystem.DrawImage(mStatFrame, mStatFrameRect);
		GuiSystem.DrawImage(mFrame1, mFrame1Rect);
		rect.y -= 2f;
		rect.width = mExpRect.width;
		GuiSystem.DrawString(mHeroGameInfo.mExp + "/" + mHeroGameInfo.mNextExp, rect, "middle_center");
		GuiSystem.DrawString(mLevelString + " " + mHeroGameInfo.mLevel, mLevelRect, "middle_center");
		foreach (KeyValuePair<int, GuiButton> mHeroItem in mHeroItems)
		{
			mHeroItem.Value.RenderElement();
		}
		foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
		{
			GUI.contentColor = mAvaParamDatum.Value.mColor;
			GuiSystem.DrawString(mAvaParamDatum.Value.mValue, mAvaParamDatum.Value.mDrawRect, mAvaParamDatum.Value.mFormat);
		}
		GUI.contentColor = Color.white;
		mHeroRotButton1.RenderElement();
		mHeroRotButton2.RenderElement();
		mBuffRenderer.RenderElement();
	}

	private void RenderStatsTab()
	{
		if (mHeroGameInfo != null)
		{
			float num = (float)mHeroGameInfo.mExp / (float)mHeroGameInfo.mNextExp;
			Rect rect = mExpRect;
			rect.width *= num;
			GuiSystem.DrawImage(mExpFrame, rect, (int)rect.width, 0, 0, 0);
			Rect rect2 = mWinLosFrameRect;
			Rect rect3 = mWinLosFrameRect;
			float num2 = mHeroGameInfo.mWinFights + mHeroGameInfo.mLoseFights;
			if (num2 > 0f)
			{
				rect2.width *= (float)mHeroGameInfo.mWinFights / num2;
				rect3.width *= (float)mHeroGameInfo.mLoseFights / num2;
				rect3.x = rect2.x + rect2.width;
			}
			GuiSystem.DrawImage(mWinFrame, rect2);
			if (num2 > 0f)
			{
				GuiSystem.DrawImage(mLossFrame, rect3);
			}
			GuiSystem.DrawImage(mFrame2, mFrame2Rect);
			if (num2 > 0f)
			{
				mWinLoseThumbRect.x = rect2.x + rect2.width - mWinLoseThumbRect.width / 2f;
				GuiSystem.DrawImage(mWinLoseThumb, mWinLoseThumbRect);
			}
			rect.y -= 2f;
			rect.width = mExpRect.width;
			GuiSystem.DrawString(mHeroGameInfo.mExp + "/" + mHeroGameInfo.mNextExp, rect, "middle_center");
			GuiSystem.DrawString(mLevelString + " " + mHeroGameInfo.mLevel, mLevelRect, "middle_center");
			GuiSystem.DrawString(mRaitingText + ":", mRatingRect, "middle_left");
			GuiSystem.DrawString(mKillsText + ":", mKillRect, "middle_left");
			GuiSystem.DrawString(mDeathsText + ":", mDeathRect, "middle_left");
			GuiSystem.DrawString(mAssistsText + ":", mAssistRect, "middle_left");
			GuiSystem.DrawString(mPrestigeText + ":", mPrestigeRect, "middle_left");
			GuiSystem.DrawString(mGamesText + ":", mGamesRect, "middle_left");
			GuiSystem.DrawString(mDiscsText + ":", mDiscRect, "middle_left");
			GuiSystem.DrawString(mCreepKillsText + ":", mCreepKillRect, "middle_center");
			GuiSystem.DrawString(mHonorText + ":", mHonorRect, "middle_left");
			float num3 = ((mHeroGameInfo.mLoseFights <= 0) ? ((float)mHeroGameInfo.mWinFights) : ((float)mHeroGameInfo.mWinFights / (float)mHeroGameInfo.mLoseFights));
			GuiSystem.DrawString(mHeroGameInfo.mHonor.ToString(), mHonorRect, "middle_right");
			GuiSystem.DrawString(num3.ToString("0.##"), mPrestigeRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mWinFights.ToString(), mWinRect, "middle_center");
			GuiSystem.DrawString(mHeroGameInfo.mLoseFights.ToString(), mLosRect, "middle_center");
			GuiSystem.DrawString(mHeroGameInfo.mRating.ToString(), mRatingRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mAvatarKills.ToString(), mKillRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mDeaths.ToString(), mDeathRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mAssists.ToString(), mAssistRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mFights.ToString(), mGamesRect, "middle_right");
			GuiSystem.DrawString(mHeroGameInfo.mCreepKills.ToString(), mCreepKillCntRect, "middle_center");
			GuiSystem.DrawString(mHeroGameInfo.mLeaves.ToString(), mDiscRect, "middle_right");
		}
	}

	public void SetCurTab(MenuTab _tab)
	{
		mCurTab = _tab;
		foreach (GuiButton mTabButton in mTabButtons)
		{
			if (mTabButton.mId == (int)mCurTab)
			{
				mTabButton.Pressed = true;
			}
			else
			{
				mTabButton.Pressed = false;
			}
			mTabButton.SetCurBtnImage();
		}
		mTipItemTab.SetActive(_tab == MenuTab.TAB_ITEMS);
		mTipStatsTab.SetActive(_tab == MenuTab.TAB_STATS);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "TAB_BUTTON" && _buttonId == 0)
		{
			SetCurTab((MenuTab)_sender.mId);
		}
		else if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "CLAN_TAG_BUTTON" && mClanInfoCallback != null)
		{
			mClanInfoCallback(mClanTagButton.mLabel.Substring(1, mClanTagButton.mLabel.Length - 2));
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && mHeroGameInfo != null && mItemIdToArticles.TryGetValue(_sender.mId, out var value))
		{
			CtrlPrototype ctrlPrototype = mItemArticles.Get(value);
			if (ctrlPrototype != null)
			{
				mFormatedTipMgr.Show(null, ctrlPrototype, -1, mHeroGameInfo.mLevel, _sender.UId, true);
				mLastTipId = _sender.UId;
			}
		}
	}

	private void OnItemMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
			if (_sender.UId != mLastTipId)
			{
				mFormatedTipMgr.Hide(mLastTipId);
			}
			mLastTipId = 0;
		}
	}

	private void RotateHeroPreview(float _val)
	{
		if (mHeroGameObject != null)
		{
			mHeroGameObject.transform.Rotate(new Vector3(0f, _val, 0f));
		}
	}

	public int GetCurHeroId()
	{
		return mCurHeroId;
	}

	public void SetData(FormatedTipMgr _tipMgr, IStoreContentProvider<CtrlPrototype> _prov, HeroStore _heroStore, HeroMgr _heroMgr)
	{
		mHeroMgr = _heroMgr;
		mFormatedTipMgr = _tipMgr;
		mItemArticles = _prov;
		mHeroStore = _heroStore;
	}

	public void SetHeroData(string _name, HeroGameInfo _gameInfo, HeroDataListArg.HeroDataItem _dataItem, bool _selfHero)
	{
		mHeroGameInfo = _gameInfo;
		mHeroDataItem = _dataItem;
		mPlayerName = _name;
		mCurHeroId = mHeroDataItem.mHeroId;
		mSelfHero = _selfHero;
		mRecalc = true;
		InitGameInfo();
		InitHeroItems();
		CreateHeroObject();
		mBuffRenderer.SetData(mItemArticles, mFormatedTipMgr, mHeroStore, _gameInfo.mBuffs);
	}
}
