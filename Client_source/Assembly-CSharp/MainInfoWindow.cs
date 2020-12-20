using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class MainInfoWindow : GuiElement, EscapeListener
{
	public struct CooldownView
	{
		public float mUpperBound;

		public Texture2D mTex;
	}

	private class UpdateSkillFXData
	{
		public int mFrameNum;

		public Rect mRect1;

		public Rect mRect2;
	}

	private class SkillButtonState
	{
		public bool mAlive;

		public bool mAvailable;

		public bool mManaEnought;

		public bool mUpgradeAvailable;

		public bool mInUse;

		public bool mInProgress;

		public bool mSilenced;
	}

	private class SkillButtonRenderData
	{
		public Texture2D mFXImage;

		public Texture2D mInUseImage;

		public Rect mInUseImageRect1;

		public Rect mInUseImageRect2;
	}

	public delegate void VoidCallback();

	public delegate void ShowQuestJournalCallback(int _questId);

	public delegate void BattleItemCallback(int _itemId);

	public delegate void OnInitInventory();

	public BattleItemCallback mOnBuyItem;

	public BattleItemCallback mOnRemoveItem;

	public ShowQuestJournalCallback mOnQuestJournal;

	public VoidCallback mOnForceRespawn;

	public VoidCallback mOnPortal;

	public VoidCallback mOnShop;

	public OnInitInventory mOnInitInventory;

	private FastItemAccessPanel mFastItemAccessPanel;

	private int mSkillFX1Frame;

	private Texture2D mBgImage;

	private Texture2D mBgImage2;

	private Texture2D mSkillFX1Image;

	private Texture2D mSkillFX2Image;

	private Texture2D mSkillManaImage;

	private Texture2D mSkillLockImage;

	private Texture2D mHPImage;

	private Texture2D mManaImage;

	private Texture2D mExpImage;

	private Rect mHPRect;

	private Rect mManaRect;

	private Rect mExpRect;

	private Rect mHPTextRect;

	private Rect mManaTextRect;

	private Rect mExpTextRect;

	private Rect mBgImage1Rect;

	private Rect mBgImage2Rect;

	private Rect mLevelRect;

	private Rect mNameRect;

	private Rect mSkillPointsRect;

	private List<GuiButton> mSkillButtons;

	private GuiButton mInventoryButton;

	private GuiButton mPortalButton;

	private GuiButton mQuestJournalButton;

	private GuiButton mShopButton;

	private Texture2D mFaceImage;

	private Rect mFaceRect;

	private BattleItemMenu mBattleItemMenu;

	private AvatarInfoWindow mAvaInfo;

	private BuffRenderer mBuffRenderer = new BuffRenderer();

	private Dictionary<int, Effector> mCurrentEffectors = new Dictionary<int, Effector>();

	private int mMoney = -1;

	private int mDiamondMoney = -1;

	private string mGold;

	private string mSilver;

	private string mBronze;

	private string mDiamonds;

	private Rect mGoldRect;

	private Rect mSilverRect;

	private Rect mBronzeRect;

	private Rect mDiamondsRect;

	private List<GuiButton> mUpgradeSkillButtons;

	private List<int> mSkillsToUpgrade;

	private float mMinUpgradeSkillY;

	private float mMaxUpgradeSkillY;

	private Texture2D mRebornFrame;

	private GuiButton mRebornButton;

	private int mTimeToRespawn;

	private string mRebornText;

	private Rect mRebornFrameRect;

	private Rect mRebornTextRect;

	private Rect mTimeToRespawnRect;

	private MoneyRenderer mRebornMoneyRenderer;

	private MapType mMapType;

	private YesNoDialog mDialog;

	private string mHealthString;

	private string mManaString;

	private string mExpString;

	private string mLvlString;

	private string mSkillPointsString;

	private FormatedTipMgr mFormatedTipMgr;

	public static bool mHidden;

	private List<CooldownView> mCooldownViews;

	private Dictionary<int, UpdateSkillFXData> mUpdateSkillFXs;

	private Dictionary<int, SkillButtonState> mSkillButtonStates;

	private List<Effector> mSkillEffectors;

	private Dictionary<int, SkillButtonRenderData> mSkillButtonRenderDatas;

	private PlayerControl mPlayerCtrl;

	private InstanceData mAvatarData;

	private BattlePrototype mAvatarProto;

	public override void Uninit()
	{
		if (mFastItemAccessPanel != null)
		{
			mFastItemAccessPanel.Uninit();
		}
		mHidden = false;
		mPlayerCtrl = null;
		mAvatarProto = null;
		if (mAvatarData != null)
		{
			mAvatarData.UnsubscribeEffectorsChanged(OnEffectorsChanged);
			mAvatarData = null;
		}
		mFaceImage = null;
		if (mAvaInfo != null)
		{
			mAvaInfo.mInited = false;
		}
		mAvaInfo = null;
		mBuffRenderer.Clean();
	}

	public bool OnEscapeAction()
	{
		return Close();
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(525, this);
		mBgImage = GuiSystem.GetImage("Gui/MainInfo/main_info_frame1");
		mBgImage2 = GuiSystem.GetImage("Gui/misc/background2");
		mHPImage = GuiSystem.GetImage("Gui/MainInfo/hp_frame1");
		mManaImage = GuiSystem.GetImage("Gui/MainInfo/mana_frame1");
		mExpImage = GuiSystem.GetImage("Gui/MainInfo/exp_frame1");
		mSkillFX1Image = GuiSystem.GetImage("Gui/MainInfo/skill_fx1");
		mSkillFX2Image = GuiSystem.GetImage("Gui/MainInfo/skill_fx2");
		mSkillManaImage = GuiSystem.GetImage("Gui/MainInfo/btn_blue");
		mSkillLockImage = GuiSystem.GetImage("Gui/MainInfo/btn_grey");
		mRebornText = GuiSystem.GetLocaleText("Reborn_Text");
		GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet("battle");
		mDialog = guiSet.GetElementById<YesNoDialog>("YES_NO_DAILOG");
		mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		mBattleItemMenu = new BattleItemMenu();
		mBattleItemMenu.Init();
		BattleItemMenu battleItemMenu = mBattleItemMenu;
		battleItemMenu.mOnBuyItem = (BattleItemCallback)Delegate.Combine(battleItemMenu.mOnBuyItem, new BattleItemCallback(OnBuyBattleItem));
		BattleItemMenu battleItemMenu2 = mBattleItemMenu;
		battleItemMenu2.mOnRemoveItem = (BattleItemCallback)Delegate.Combine(battleItemMenu2.mOnRemoveItem, new BattleItemCallback(OnRemoveBattleItem));
		List<GuiButton> itemButtons = mBattleItemMenu.GetItemButtons();
		if (itemButtons.Count > 0)
		{
			AddTutorialElement(itemButtons[0]);
		}
		itemButtons = mBattleItemMenu.GetTreeTypeButtons();
		for (int i = 0; i < itemButtons.Count; i++)
		{
			AddTutorialElement(itemButtons[i].mElementId + "_" + i, itemButtons[i]);
		}
		itemButtons = mBattleItemMenu.GetCloseButtons();
		for (int j = 0; j < itemButtons.Count; j++)
		{
			AddTutorialElement(itemButtons[j].mElementId + "_" + j, itemButtons[j]);
		}
		mFastItemAccessPanel = new FastItemAccessPanel();
		mFastItemAccessPanel.Init();
		mSkillButtonStates = new Dictionary<int, SkillButtonState>();
		mCooldownViews = new List<CooldownView>();
		mSkillsToUpgrade = new List<int>();
		mSkillButtonRenderDatas = new Dictionary<int, SkillButtonRenderData>();
		mSkillEffectors = new List<Effector>();
		mUpdateSkillFXs = new Dictionary<int, UpdateSkillFXData>();
		for (int num = 31; num >= 0; num--)
		{
			CooldownView item = default(CooldownView);
			item.mTex = GuiSystem.GetImage("Gui/progress/progress1_" + (num + 1));
			if (!(null == item.mTex))
			{
				item.mUpperBound = 1f - 0.03125f * (float)num;
				mCooldownViews.Add(item);
			}
		}
		mInventoryButton = new GuiButton();
		mInventoryButton.mElementId = "INVENTORY_BUTTON";
		mInventoryButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/button_1_norm");
		mInventoryButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/button_1_over");
		mInventoryButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/button_1_press");
		mInventoryButton.mOnMouseUp = OnButton;
		mInventoryButton.RegisterAction(UserActionType.BAG_CLICK, "LOG_BATTLE");
		mInventoryButton.Init();
		AddTutorialElement(mInventoryButton);
		mPortalButton = GuiSystem.CreateButton("Gui/MainInfo/button_4_norm", "Gui/MainInfo/button_4_over", "Gui/MainInfo/button_4_press", string.Empty, string.Empty);
		mPortalButton.mElementId = "PORTAL_BUTTON";
		GuiButton guiButton = mPortalButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mPortalButton.RegisterAction(UserActionType.BATTLE_EXIT_BUTTON_CLICK);
		mPortalButton.Init();
		AddTutorialElement(mPortalButton);
		mQuestJournalButton = GuiSystem.CreateButton("Gui/MainInfo/button_5_norm", "Gui/MainInfo/button_5_over", "Gui/MainInfo/button_5_press", string.Empty, string.Empty);
		mQuestJournalButton.mElementId = "QUEST_JOURNAL_BUTTON";
		GuiButton guiButton2 = mQuestJournalButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mQuestJournalButton.RegisterAction(UserActionType.QUEST_JOURNAL_BUTTON);
		mQuestJournalButton.Init();
		mShopButton = GuiSystem.CreateButton("Gui/MainInfo/button_6_norm", "Gui/MainInfo/button_6_over", "Gui/MainInfo/button_6_press", string.Empty, string.Empty);
		mShopButton.mElementId = "SHOP_BUTTON";
		GuiButton guiButton3 = mShopButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mShopButton.RegisterAction(UserActionType.SHOP_CLICK, "LOG_BATTLE");
		mShopButton.Init();
		AddTutorialElement(mShopButton);
		mFastItemAccessPanel.SetCooldownViews(mCooldownViews);
		mBattleItemMenu.SetCooldownViews(mCooldownViews);
		InitSkillButtons();
		InitUpdateSkillButtons();
		InitRebornData();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(455f, 0f, mBgImage.width, mBgImage.height);
		float num = (float)OptionsMgr.mScreenWidth - 650f * GuiSystem.mYRate;
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.y = (float)OptionsMgr.mScreenHeight - mZoneRect.height;
		mZoneRect.x += (num - mZoneRect.width) / 2f;
		mHPRect = new Rect(263f, 75f, 219f, 15f);
		mManaRect = new Rect(263f, 92f, 219f, 15f);
		mExpRect = new Rect(244f, 169f, 236f, 15f);
		mFaceRect = new Rect(73f, 20f, 128f, 128f);
		mHPTextRect = new Rect(mHPRect.x, mHPRect.y - 1f, mHPRect.width, mHPRect.height);
		mManaTextRect = new Rect(mManaRect.x, mManaRect.y - 1f, mManaRect.width, mManaRect.height);
		mExpTextRect = new Rect(mExpRect.x, mExpRect.y - 1f, mExpRect.width, mExpRect.height);
		mLevelRect = new Rect(214f, 158f, 27f, 27f);
		mNameRect = new Rect(66f, 168f, 140f, 13f);
		mSkillPointsRect = new Rect(486f, 19f, 26f, 26f);
		mBgImage2Rect = new Rect(0f, 814f, mBgImage2.width, mBgImage2.height);
		GuiSystem.GetRectScaled(ref mBgImage2Rect, _ignoreLowRate: true);
		mBgImage2Rect.x = ((float)OptionsMgr.mScreenWidth - mBgImage2Rect.width) / 2f;
		mGoldRect = new Rect(538f, 151f, 34f, 12f);
		mSilverRect = new Rect(583f, 151f, 19f, 12f);
		mBronzeRect = new Rect(613f, 151f, 19f, 12f);
		mDiamondsRect = new Rect(583f, 168f, 53f, 12f);
		mInventoryButton.mZoneRect = new Rect(642f, 151f, 34f, 33f);
		mRebornFrameRect = new Rect(68f, -33f, mRebornFrame.width, mRebornFrame.height);
		mRebornButton.mZoneRect = new Rect(50f, 5f, 33f, 33f);
		mTimeToRespawnRect = new Rect(0f, 98f, mRebornFrame.width, 40f);
		mRebornTextRect = new Rect(17f, 59f, 102f, 11f);
		GuiSystem.SetChildRect(mZoneRect, ref mBgImage1Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mHPRect);
		GuiSystem.SetChildRect(mZoneRect, ref mManaRect);
		GuiSystem.SetChildRect(mZoneRect, ref mExpRect);
		GuiSystem.SetChildRect(mZoneRect, ref mFaceRect);
		GuiSystem.SetChildRect(mZoneRect, ref mHPTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mManaTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mExpTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSkillPointsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mGoldRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSilverRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBronzeRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDiamondsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mInventoryButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRebornFrameRect);
		GuiSystem.SetChildRect(mRebornFrameRect, ref mRebornButton.mZoneRect);
		GuiSystem.SetChildRect(mRebornFrameRect, ref mTimeToRespawnRect);
		GuiSystem.SetChildRect(mRebornFrameRect, ref mRebornTextRect);
		SetMoneyRendererSize();
		GuiElement guiElement = new GuiElement();
		guiElement.mElementId = "AVATAR_EXP";
		guiElement.mZoneRect = mExpRect;
		AddTutorialElement(guiElement);
		guiElement = new GuiElement();
		guiElement.mElementId = "AVATAR_FACE";
		guiElement.mZoneRect = mFaceRect;
		AddTutorialElement(guiElement);
		Rect[] array = new Rect[3]
		{
			new Rect(531f, 28f, 46f, 46f),
			new Rect(596f, 28f, 46f, 46f),
			new Rect(565f, 92f, 46f, 46f)
		};
		for (int i = 0; i < 3; i++)
		{
			GuiSystem.SetChildRect(mZoneRect, ref array[i]);
		}
		mBattleItemMenu.SetSize();
		mBattleItemMenu.SetBattleItemSize(array);
		mBattleItemMenu.SetOffsetX(mZoneRect.x);
		mFastItemAccessPanel.mZoneRect = new Rect(260f, 116f, 226f, 46f);
		GuiSystem.SetChildRect(mZoneRect, ref mFastItemAccessPanel.mZoneRect);
		mFastItemAccessPanel.SetSize();
		guiElement = new GuiElement();
		guiElement.mElementId = "FAST_ITEM_ACCESS_PANEL";
		guiElement.mZoneRect = mFastItemAccessPanel.mZoneRect;
		AddTutorialElement(guiElement);
		mPortalButton.mZoneRect = new Rect(0f, 0f, 119f, 84f);
		GuiSystem.GetRectScaled(ref mPortalButton.mZoneRect);
		mPortalButton.mZoneRect.x = ((float)OptionsMgr.mScreenWidth + 229f * GuiSystem.mYRate) / 2f;
		mQuestJournalButton.mZoneRect = new Rect(35f, 7f, 32f, 32f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestJournalButton.mZoneRect);
		mShopButton.mZoneRect = new Rect(0f, 0f, 119f, 84f);
		GuiSystem.GetRectScaled(ref mShopButton.mZoneRect);
		mShopButton.mZoneRect.x = ((float)OptionsMgr.mScreenWidth - 229f * GuiSystem.mYRate) / 2f - mShopButton.mZoneRect.width;
		int num2 = 0;
		float num3 = 56f;
		float num4 = 265f;
		float top = 20f;
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			if (mSkillButton.mElementId == "SKILL_BUTTON")
			{
				mSkillButton.mZoneRect = new Rect(num4 + (float)num2 * num3, top, 46f, 46f);
				GuiSystem.SetChildRect(mZoneRect, ref mSkillButton.mZoneRect);
				num2++;
			}
			else if (mSkillButton.mElementId == "SKILL_BOOST_BUTTON")
			{
				mSkillButton.mZoneRect = new Rect(229f, 21f, 28f, 28f);
				GuiSystem.SetChildRect(mZoneRect, ref mSkillButton.mZoneRect);
			}
		}
		mMinUpgradeSkillY = 0f;
		mMaxUpgradeSkillY = -27f;
		GuiButton guiButton = null;
		for (int j = 0; j < 5; j++)
		{
			guiButton = mUpgradeSkillButtons[j];
			guiButton.mZoneRect = new Rect(215 + j * 55, mMinUpgradeSkillY, 41f, 51f);
			guiButton.mIconRect = new Rect(6f, 3f, 29f, 29f);
			guiButton.mIconOnTop = false;
			GuiSystem.SetChildRect(mZoneRect, ref guiButton.mZoneRect);
			GuiSystem.SetChildRect(guiButton.mZoneRect, ref guiButton.mIconRect);
		}
		mMinUpgradeSkillY = mMinUpgradeSkillY * GuiSystem.mYRate + mZoneRect.y;
		mMaxUpgradeSkillY = mMaxUpgradeSkillY * GuiSystem.mYRate + mZoneRect.y;
		mBuffRenderer.mSize = 30f;
		mBuffRenderer.mOffsetX = 35f;
		mBuffRenderer.mOffsetY = -35f;
		mBuffRenderer.mStartX = 50f;
		mBuffRenderer.mStartY = -65f;
		mBuffRenderer.mBaseRect = mZoneRect;
		mBuffRenderer.SetBuffSize();
		PopupInfo.AddTip(this, "TIP_TEXT26", mShopButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT27", mPortalButton);
		PopupInfo.AddTip(this, "TIP_TEXT28", mQuestJournalButton);
		PopupInfo.AddTip(this, "TIP_TEXT29", mLevelRect);
		PopupInfo.AddTip(this, "TIP_TEXT30", mExpRect);
		PopupInfo.AddTip(this, "TIP_TEXT31", mSkillPointsRect);
		PopupInfo.AddTip(this, "TIP_TEXT7", mInventoryButton.mZoneRect);
		Rect _rect = new Rect(530f, 151f, 100f, 30f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT24", _rect);
	}

	public bool Close()
	{
		if (mBattleItemMenu != null && OptionsMgr.TutorialSetNum == -1)
		{
			return mBattleItemMenu.Close();
		}
		return false;
	}

	private void InitUpdateSkillButtons()
	{
		mUpgradeSkillButtons = new List<GuiButton>();
		GuiButton guiButton = null;
		for (int i = 0; i < 5; i++)
		{
			guiButton = GuiSystem.CreateButton("Gui/MainInfo/button_2_norm", "Gui/MainInfo/button_2_over", "Gui/MainInfo/button_2_press", string.Empty, string.Empty);
			if (i == 0)
			{
				guiButton.mIconImg = GuiSystem.GetImage("Gui/MainInfo/Icons/boost_norm");
			}
			guiButton.mElementId = "UPGRADE_SKILL_BUTTON";
			GuiButton guiButton2 = guiButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			GuiButton guiButton3 = guiButton;
			guiButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton3.mOnMouseEnter, new OnMouseEnter(OnSkillMouseEnter));
			GuiButton guiButton4 = guiButton;
			guiButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton4.mOnMouseLeave, new OnMouseLeave(OnTipMouseLeave));
			guiButton.Init();
			mUpgradeSkillButtons.Add(guiButton);
			if (i == 1)
			{
				AddTutorialElement(guiButton.mElementId + "_" + i, guiButton);
			}
		}
	}

	private void InitRebornData()
	{
		mRebornFrame = GuiSystem.GetImage("Gui/MainInfo/main_info_frame3");
		mRebornButton = GuiSystem.CreateButton("Gui/MainInfo/button_3_norm", "Gui/MainInfo/button_3_over", "Gui/MainInfo/button_3_press", string.Empty, string.Empty);
		mRebornButton.mElementId = "REBORN_BUTTON";
		GuiButton guiButton = mRebornButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mRebornButton.RegisterAction(UserActionType.REBORN_BUTTON_CLICK);
		mRebornButton.Init();
	}

	private void InitSkillButtons()
	{
		mSkillButtons = new List<GuiButton>();
		GuiButton guiButton = null;
		for (int i = 0; i < 5; i++)
		{
			guiButton = new GuiButton();
			if (i == 0)
			{
				guiButton.mElementId = "SKILL_BOOST_BUTTON";
				guiButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/Icons/boost_norm");
				guiButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/Icons/boost_over");
				guiButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/Icons/boost_press");
			}
			else
			{
				guiButton.mElementId = "SKILL_BUTTON";
			}
			AddTutorialElement(guiButton.mElementId + "_" + i, guiButton);
			GuiButton guiButton2 = guiButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnSkillButton));
			GuiButton guiButton3 = guiButton;
			guiButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton3.mOnMouseEnter, new OnMouseEnter(OnSkillMouseEnter));
			GuiButton guiButton4 = guiButton;
			guiButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton4.mOnMouseLeave, new OnMouseLeave(OnTipMouseLeave));
			GuiButton guiButton5 = guiButton;
			guiButton5.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton5.mOnMouseEnter, new OnMouseEnter(PopupEvent.OnShowAOE));
			GuiButton guiButton6 = guiButton;
			guiButton6.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton6.mOnMouseLeave, new OnMouseLeave(PopupEvent.OnMouseLeave));
			guiButton.Init();
			mSkillButtons.Add(guiButton);
		}
	}

	public void SetSkillAbility(int _id, SkillType _type)
	{
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			if (mSkillButton.mId == _id && mSkillButton.mNormImg == null)
			{
				mSkillButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/btn_norm");
				mSkillButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/btn_over");
				mSkillButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/btn_press");
				mSkillButton.Init();
			}
		}
	}

	public override void Update()
	{
		if (mAvatarData == null)
		{
			return;
		}
		SetParamsRect();
		SetParamsString();
		UpdateSkillFX2();
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			SetSkillButtonState(mSkillButton);
		}
		mFastItemAccessPanel.Update();
		mBattleItemMenu.Update();
		if (mPlayerCtrl != null)
		{
			if (mPlayerCtrl.SelfPlayer.VirtualMoney != mMoney)
			{
				mMoney = mPlayerCtrl.SelfPlayer.VirtualMoney;
				int _gold = 0;
				int _silver = 0;
				int _bronze = 0;
				ShopVendor.SetMoney(mMoney, ref _gold, ref _silver, ref _bronze);
				mGold = _gold.ToString();
				mSilver = _silver.ToString();
				mBronze = _bronze.ToString();
			}
			if (mPlayerCtrl.SelfPlayer.RealMoney != mDiamondMoney)
			{
				mDiamondMoney = mPlayerCtrl.SelfPlayer.RealMoney;
				mDiamonds = ((float)mDiamondMoney / 100f).ToString("0.##");
			}
		}
		if (mAvatarData.IsPlayerBinded)
		{
			Player player = mAvatarData.Player;
			if (player != null)
			{
				if (player.RespTime != null)
				{
					mTimeToRespawn = player.RespTime.GetTimeToRespawn();
					if (player.RespCost != null)
					{
						mRebornButton.mLocked = false;
						if (mRebornMoneyRenderer == null)
						{
							InitMoneyRenderer(player.RespCost.Currency == Currency.REAL, player.RespCost.Cost);
							SetMoneyRendererSize();
						}
					}
					else
					{
						mRebornButton.mLocked = true;
					}
				}
				else
				{
					mTimeToRespawn = 0;
					if (mRebornMoneyRenderer != null)
					{
						mRebornMoneyRenderer = null;
					}
				}
			}
		}
		SetUpdateSkillButtonsPos();
		mBuffRenderer.Update();
	}

	private void InitMoneyRenderer(bool _diamonds, int _money)
	{
		mRebornMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, _diamonds);
		mRebornMoneyRenderer.SetMoney(_money);
	}

	private void SetMoneyRendererSize()
	{
		if (mRebornMoneyRenderer != null)
		{
			mRebornMoneyRenderer.SetSize(mRebornFrameRect);
			mRebornMoneyRenderer.SetOffset(new Vector2(25f, 74f) * GuiSystem.mYRate);
		}
	}

	private void UpdateSkillFX2()
	{
		if (mUpdateSkillFXs.Count == 0 || Time.frameCount % 2 != 0)
		{
			return;
		}
		foreach (KeyValuePair<int, UpdateSkillFXData> mUpdateSkillFX in mUpdateSkillFXs)
		{
			UpdateSkillFXData value = mUpdateSkillFX.Value;
			if (value.mFrameNum == 14)
			{
				mUpdateSkillFXs.Remove(mUpdateSkillFX.Key);
				break;
			}
			float num = 0.0714285746f;
			value.mRect2 = new Rect((float)value.mFrameNum * num, 0f, num, 1f);
			value.mFrameNum++;
		}
	}

	private void SetUpdateSkillButtonsPos()
	{
		foreach (GuiButton mUpgradeSkillButton in mUpgradeSkillButtons)
		{
			mUpgradeSkillButton.mLocked = !mSkillsToUpgrade.Contains(mUpgradeSkillButton.mId);
			mUpgradeSkillButton.mIconRect = new Rect(6f, 3f, 29f, 29f);
			if (!mUpgradeSkillButton.mLocked)
			{
				if (mUpgradeSkillButton.mZoneRect.y != mMaxUpgradeSkillY)
				{
					mUpgradeSkillButton.mZoneRect.y = Mathf.Lerp(mMinUpgradeSkillY, mMaxUpgradeSkillY, Time.time);
				}
			}
			else if (mUpgradeSkillButton.mZoneRect.y != mMinUpgradeSkillY)
			{
				mUpgradeSkillButton.mZoneRect.y = Mathf.Lerp(mMaxUpgradeSkillY, mMinUpgradeSkillY, Time.time);
			}
			GuiSystem.SetChildRect(mUpgradeSkillButton.mZoneRect, ref mUpgradeSkillButton.mIconRect);
		}
	}

	private void OnEffectorsChanged(InstanceData _data, int _effectorId)
	{
		if (_data == null)
		{
			Log.Error("InstanceData == null");
			return;
		}
		if (_effectorId >= 0)
		{
			Effector selfEffector = _data.GetSelfEffector(_effectorId);
			if (selfEffector == null)
			{
				Log.Error("Try to add bad effector : " + _effectorId);
				return;
			}
			mCurrentEffectors[selfEffector.Proto.Id] = selfEffector;
			SkillType skillType = selfEffector.SkillType;
			switch (skillType)
			{
			case SkillType.SKILL:
			case SkillType.PARAMS:
				mSkillEffectors.Add(selfEffector);
				mSkillButtonRenderDatas[selfEffector.Proto.Id] = new SkillButtonRenderData();
				if (mSkillEffectors.Count == mSkillButtons.Count)
				{
					InitSkills();
				}
				break;
			case SkillType.ACTIVE:
			case SkillType.TOGGLE:
				SetSkillAbility(selfEffector.Parent.Proto.Id, skillType);
				break;
			case SkillType.BUFF:
				mBuffRenderer.AddBattleBuff(selfEffector.Id, selfEffector.Proto);
				break;
			}
			return;
		}
		_effectorId = Math.Abs(_effectorId);
		int num = -1;
		foreach (Effector value in mCurrentEffectors.Values)
		{
			if (value.Id == _effectorId)
			{
				num = value.Proto.Id;
				break;
			}
		}
		if (num > 0 && mCurrentEffectors.ContainsKey(num))
		{
			mCurrentEffectors.Remove(num);
		}
		if (mBuffRenderer.ContainsBattleBuff(_effectorId))
		{
			mBuffRenderer.RemoveBattleBuff(_effectorId);
			return;
		}
		Effector effector = mSkillEffectors.Find((Effector e) => e.Id == _effectorId);
		if (effector != null)
		{
			mSkillEffectors.Remove(effector);
			mSkillButtonRenderDatas.Remove(effector.Proto.Id);
		}
	}

	public override void RenderElement()
	{
		if (mHidden)
		{
			return;
		}
		Player player = mAvatarData.Player;
		if (mAvatarData == null || player == null)
		{
			return;
		}
		GuiSystem.DrawImage(mBgImage2, mBgImage2Rect);
		GuiSystem.DrawImage(mHPImage, mHPRect);
		GuiSystem.DrawImage(mManaImage, mManaRect);
		GuiSystem.DrawImage(mExpImage, mExpRect);
		GuiSystem.DrawImage(mFaceImage, mFaceRect);
		foreach (GuiButton mUpgradeSkillButton in mUpgradeSkillButtons)
		{
			mUpgradeSkillButton.RenderElement();
		}
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			RenderSkillButton(mSkillButton);
		}
		GuiSystem.DrawImage(mBgImage, mZoneRect);
		foreach (GuiButton mSkillButton2 in mSkillButtons)
		{
			Effector selfEffectorByProto = mAvatarData.GetSelfEffectorByProto(mSkillButton2.mId);
			if (selfEffectorByProto != null)
			{
				GuiSystem.DrawString(selfEffectorByProto.Level.ToString(), mSkillButton2.mZoneRect, "lower_right");
			}
		}
		GuiSystem.DrawString(mGold, mGoldRect, "middle_center");
		GuiSystem.DrawString(mSilver, mSilverRect, "middle_center");
		GuiSystem.DrawString(mBronze, mBronzeRect, "middle_center");
		GuiSystem.DrawString(mDiamonds, mDiamondsRect, "middle_center");
		RenderAvatarParamsValue();
		foreach (GuiButton mSkillButton3 in mSkillButtons)
		{
			RenderSkillFX2(mSkillButton3);
		}
		GuiSystem.DrawString(mLvlString, mLevelRect, "middle_center");
		GuiSystem.DrawString(player.Name, mNameRect, "middle_center");
		GuiSystem.DrawString(mSkillPointsString, mSkillPointsRect, "middle_center");
		mInventoryButton.RenderElement();
		mFastItemAccessPanel.RenderElement();
		mBattleItemMenu.RenderElement();
		if (mTimeToRespawn > 0)
		{
			GuiSystem.DrawImage(mRebornFrame, mRebornFrameRect);
			GuiSystem.DrawString(mTimeToRespawn.ToString(), mTimeToRespawnRect, "middle_center_36");
			if (mRebornMoneyRenderer != null)
			{
				GuiSystem.DrawString(mRebornText, mRebornTextRect, "middle_center");
				mRebornMoneyRenderer.Render();
			}
			mRebornButton.RenderElement();
		}
		if (mMapType == MapType.HUNT)
		{
			mPortalButton.RenderElement();
		}
		mQuestJournalButton.RenderElement();
		mShopButton.RenderElement();
		mBuffRenderer.RenderElement();
	}

	private void RenderAvatarParamsValue()
	{
		GuiSystem.DrawString(mHealthString, mHPTextRect, "middle_center");
		GuiSystem.DrawString(mManaString, mManaTextRect, "middle_center");
		GuiSystem.DrawString(mExpString, mExpTextRect, "middle_center");
	}

	private void SetSkillButtonState(GuiButton _btn)
	{
		if (_btn == null || mAvatarData.Params == null)
		{
			return;
		}
		int num = 0;
		SkillButtonState _sbs = null;
		bool manaEnought = false;
		bool toggable = false;
		foreach (Effector mSkillEffector in mSkillEffectors)
		{
			num = mSkillEffector.Proto.Id;
			if (num != _btn.mId || !mSkillButtonStates.TryGetValue(num, out _sbs))
			{
				continue;
			}
			if (mSkillEffector.Child != null)
			{
				manaEnought = mSkillEffector.Child.IsEnoughMana(mAvatarData.Params);
				toggable = mSkillEffector.Child.SkillType == SkillType.TOGGLE;
			}
			SetSkillButtonState(ref _sbs, num, mSkillEffector.Level, manaEnought, toggable);
			break;
		}
		SetSkillButtonRenderData(_btn, _sbs);
	}

	private void SetSkillButtonState(ref SkillButtonState _sbs, int _id, int _lvl, bool _manaEnought, bool _toggable)
	{
		_sbs.mAlive = mAvatarData.Params.Health > 0;
		if (mPlayerCtrl.HasActiveSkill)
		{
			_sbs.mInUse = mPlayerCtrl.GetActiveSkill().Parent.Proto.Id == _id;
		}
		else
		{
			_sbs.mInUse = false;
		}
		_sbs.mAvailable = _lvl > 0;
		_sbs.mInProgress = false;
		if (_sbs.mAvailable)
		{
			TargetedAction action = mAvatarData.GetAction(_id);
			if (action != null)
			{
				if (_toggable)
				{
					_sbs.mInUse = true;
				}
				else
				{
					_sbs.mInProgress = true;
				}
			}
		}
		_sbs.mManaEnought = _manaEnought;
		if (mPlayerCtrl.SelfPlayer.SkillPoints > 0)
		{
			if (mCurrentEffectors.ContainsKey(_id))
			{
				_sbs.mUpgradeAvailable = mPlayerCtrl.SelfPlayer.IsAvailEffector(mCurrentEffectors[_id]);
			}
			else
			{
				_sbs.mUpgradeAvailable = false;
			}
		}
		else
		{
			_sbs.mUpgradeAvailable = false;
		}
		if (_sbs.mUpgradeAvailable)
		{
			if (!mSkillsToUpgrade.Contains(_id))
			{
				mSkillsToUpgrade.Add(_id);
			}
		}
		else if (mSkillsToUpgrade.Contains(_id))
		{
			mSkillsToUpgrade.Remove(_id);
		}
		_sbs.mSilenced = mAvatarData.Params.mSilence > 0;
	}

	private void SetSkillButtonRenderData(GuiButton _btn, SkillButtonState _sbs)
	{
		if (_btn == null || _sbs == null)
		{
			return;
		}
		SkillButtonRenderData value = null;
		if (!mSkillButtonRenderDatas.TryGetValue(_btn.mId, out value))
		{
			return;
		}
		_btn.mLocked = !_sbs.mAlive || _sbs.mSilenced;
		value.mFXImage = null;
		if (!_sbs.mAlive || !_sbs.mAvailable || _sbs.mSilenced || _sbs.mInProgress)
		{
			if (_btn.mElementId != "SKILL_BOOST_BUTTON")
			{
				value.mFXImage = mSkillLockImage;
			}
		}
		else if (!_sbs.mManaEnought)
		{
			value.mFXImage = mSkillManaImage;
		}
		value.mInUseImage = null;
		if (!_sbs.mInUse)
		{
			return;
		}
		value.mInUseImage = mSkillFX1Image;
		if (Time.frameCount % 4 == 0)
		{
			mSkillFX1Frame++;
			if (mSkillFX1Frame == 32)
			{
				mSkillFX1Frame = 0;
			}
		}
		float num = 0.03125f;
		float num2 = (float)mSkillFX1Image.width * GuiSystem.mYRate;
		float num3 = (float)mSkillFX1Image.height / 32f * GuiSystem.mYRate;
		value.mInUseImageRect1 = new Rect(_btn.mZoneRect.x + (_btn.mZoneRect.width - num2) / 2f, _btn.mZoneRect.y + (_btn.mZoneRect.height - num3) / 2f, num2, num3);
		value.mInUseImageRect2 = new Rect(0f, (float)mSkillFX1Frame * num, 1f, num);
	}

	private void RenderSkillButton(GuiButton _btn)
	{
		if (_btn == null)
		{
			return;
		}
		_btn.RenderElement();
		SkillButtonRenderData value = null;
		if (mSkillButtonRenderDatas.TryGetValue(_btn.mId, out value))
		{
			if (null != value.mFXImage)
			{
				GuiSystem.DrawImage(value.mFXImage, _btn.mZoneRect);
			}
			if (null != value.mInUseImage)
			{
				GuiSystem.DrawImage(value.mInUseImage, value.mInUseImageRect1, value.mInUseImageRect2);
			}
			RenderSkillCooldown(_btn);
		}
	}

	private void RenderSkillFX2(GuiButton _skillBtn)
	{
		if (_skillBtn != null && mUpdateSkillFXs.TryGetValue(_skillBtn.mId, out var value))
		{
			Rect drawRect = new Rect(value.mRect1);
			drawRect.x = _skillBtn.mZoneRect.x - 17f * GuiSystem.mYRate;
			drawRect.y = _skillBtn.mZoneRect.y - 56f * GuiSystem.mYRate;
			GuiSystem.DrawImage(mSkillFX2Image, drawRect, value.mRect2);
		}
	}

	private void RenderSkillCooldown(GuiButton _btn)
	{
		if (mAvatarData == null || mCooldownViews == null)
		{
			return;
		}
		double cooldownProgress = mAvatarData.GetCooldownProgress(_btn.mId);
		if (!(cooldownProgress > 0.0) || !(cooldownProgress <= 1.0))
		{
			return;
		}
		foreach (CooldownView mCooldownView in mCooldownViews)
		{
			if (cooldownProgress <= (double)mCooldownView.mUpperBound)
			{
				GuiSystem.DrawImage(mCooldownView.mTex, _btn.mZoneRect);
				break;
			}
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mFaceRect.Contains(_curEvent.mousePosition))
		{
			if (!mAvaInfo.mInited)
			{
				mAvaInfo.SetData(mAvatarData.Params);
			}
			mAvaInfo.SetActive(_active: true);
		}
		else
		{
			mAvaInfo.SetActive(_active: false);
		}
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			mSkillButton.CheckEvent(_curEvent);
		}
		foreach (GuiButton mUpgradeSkillButton in mUpgradeSkillButtons)
		{
			mUpgradeSkillButton.CheckEvent(_curEvent);
		}
		mBuffRenderer.CheckEvent(_curEvent);
		if (_curEvent.type == EventType.KeyDown)
		{
			if (GUI.GetNameOfFocusedControl() != string.Empty)
			{
				return;
			}
			switch (_curEvent.keyCode)
			{
			case KeyCode.Q:
				OnSkillButton(GetButtonByNum(1), _curEvent.control ? 1 : 0);
				_curEvent.Use();
				break;
			case KeyCode.W:
				OnSkillButton(GetButtonByNum(2), _curEvent.control ? 1 : 0);
				_curEvent.Use();
				break;
			case KeyCode.E:
				OnSkillButton(GetButtonByNum(3), _curEvent.control ? 1 : 0);
				_curEvent.Use();
				break;
			case KeyCode.R:
				OnSkillButton(GetButtonByNum(4), _curEvent.control ? 1 : 0);
				_curEvent.Use();
				break;
			case KeyCode.T:
				if (_curEvent.control)
				{
					OnSkillButton(GetButtonByNum(0), 1);
				}
				_curEvent.Use();
				break;
			case KeyCode.A:
				if (mPlayerCtrl != null)
				{
					mPlayerCtrl.ForceAttackMode = true;
				}
				_curEvent.Use();
				break;
			case KeyCode.I:
				if (mOnInitInventory != null)
				{
					UserLog.AddAction(UserActionType.BAG_CLICK, string.Format("{0} ({1})", GuiSystem.GetLocaleText("LOG_BATTLE"), GuiSystem.GetLocaleText("LOG_HOTKEY")));
					mOnInitInventory();
					_curEvent.Use();
				}
				break;
			case KeyCode.H:
				mHidden = ((!_curEvent.alt) ? mHidden : (!mHidden));
				break;
			}
		}
		mBattleItemMenu.CheckEvent(_curEvent);
		mFastItemAccessPanel.CheckEvent(_curEvent);
		mInventoryButton.CheckEvent(_curEvent);
		if (mTimeToRespawn > 0)
		{
			mRebornButton.CheckEvent(_curEvent);
		}
		if (mMapType == MapType.HUNT)
		{
			mPortalButton.CheckEvent(_curEvent);
		}
		mQuestJournalButton.CheckEvent(_curEvent);
		mShopButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	private void InitSkills()
	{
		Effector effector = null;
		GuiButton guiButton = null;
		List<Effector> list = new List<Effector>();
		mAvatarData.GetEffectorsByType(SkillType.PARAMS, list);
		mAvatarData.GetEffectorsByType(SkillType.SKILL, list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			effector = list[i];
			guiButton = mSkillButtons[i];
			guiButton.mId = effector.Proto.Id;
			mUpgradeSkillButtons[i].mId = guiButton.mId;
			if (!mSkillButtonStates.ContainsKey(guiButton.mId))
			{
				mSkillButtonStates.Add(guiButton.mId, new SkillButtonState());
			}
			if (effector.SkillType != SkillType.PARAMS)
			{
				guiButton.mIconImg = GuiSystem.GetImage(effector.Proto.EffectDesc.mDesc.mIcon);
				guiButton.mIconOnTop = false;
				guiButton.Init();
				mUpgradeSkillButtons[i].mIconImg = GuiSystem.GetImage(effector.Proto.EffectDesc.mDesc.mIcon + "_1");
				mUpgradeSkillButtons[i].mLocked = false;
				mUpgradeSkillButtons[i].Init();
			}
		}
	}

	private GuiButton GetButtonByNum(int _num)
	{
		if (_num < 0 || _num > mSkillButtons.Count - 1)
		{
			return null;
		}
		return mSkillButtons[_num];
	}

	private GuiButton GetButtonById(int _id)
	{
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			if (mSkillButton.mId == _id)
			{
				return mSkillButton;
			}
		}
		return null;
	}

	private bool CanUseSkill(GuiButton _button)
	{
		if (_button == null)
		{
			return false;
		}
		if (mAvatarData != null && mAvatarData.GetCooldownProgress(_button.mId) > 0.0)
		{
			return false;
		}
		SkillButtonState value = null;
		if (!mSkillButtonStates.TryGetValue(_button.mId, out value))
		{
			return false;
		}
		if (!value.mAlive || !value.mAvailable || (!value.mManaEnought && !value.mInUse) || value.mInProgress)
		{
			return false;
		}
		return true;
	}

	private void SetParamsRect()
	{
		if (mAvatarData != null)
		{
			float healthProgress = mAvatarData.Params.HealthProgress;
			mHPRect.width = 239f * healthProgress * GuiSystem.mYRate;
			healthProgress = mAvatarData.Params.ManaProgress;
			mManaRect.width = 239f * healthProgress * GuiSystem.mYRate;
			GameData.GetExp(mAvatarProto, mAvatarData.Level, out var _prevLvlExp, out var _nextLvlExp);
			float num = _nextLvlExp - _prevLvlExp;
			float num2 = mAvatarData.Params.CurExp - _prevLvlExp;
			healthProgress = Mathf.Clamp01(num2 / num);
			mExpRect.width = 236f * healthProgress * GuiSystem.mYRate;
		}
	}

	private void SetParamsString()
	{
		mHealthString = mAvatarData.Params.Health + "/" + mAvatarData.Params.MaxHealth;
		mManaString = mAvatarData.Params.Mana + "/" + mAvatarData.Params.MaxMana;
		GameData.GetExp(mAvatarProto, mAvatarData.Level, out var _prevLvlExp, out var _nextLvlExp);
		int num = (int)Math.Round(_nextLvlExp - _prevLvlExp);
		int num2 = (int)Math.Round(mAvatarData.Params.CurExp - _prevLvlExp);
		if (num2 < 0)
		{
			num2 = 0;
		}
		mExpString = num2 + "/" + num;
		mLvlString = (mAvatarData.Level + 1).ToString();
		mSkillPointsString = mPlayerCtrl.SelfPlayer.SkillPoints.ToString();
	}

	private void OnSkillButton(GuiElement _sender, int _buttonId)
	{
		if (!_sender.InputEnabled())
		{
			return;
		}
		switch (_buttonId)
		{
		case 0:
		{
			GuiButton guiButton = _sender as GuiButton;
			if (CanUseSkill(guiButton) && mPlayerCtrl != null)
			{
				mPlayerCtrl.SetActiveSkillByProto(_sender.mId);
				PopupEvent.OnShowAOE(guiButton);
			}
			break;
		}
		case 1:
			UpgradeSkill(_sender.mId);
			break;
		}
	}

	private void OnSkillMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr == null || _sender.mLocked)
		{
			return;
		}
		Effector selfEffectorByProto = mAvatarData.GetSelfEffectorByProto(_sender.mId);
		if (selfEffectorByProto == null)
		{
			return;
		}
		int skillNum = -1;
		for (int i = 0; i < mSkillButtons.Count; i++)
		{
			if (mSkillButtons[i].mId == _sender.mId)
			{
				skillNum = i;
				break;
			}
		}
		if (_sender.mElementId == "SKILL_BUTTON" || _sender.mElementId == "SKILL_BOOST_BUTTON")
		{
			if (selfEffectorByProto.Child != null)
			{
				mFormatedTipMgr.Show(selfEffectorByProto.Child, FormatedTipMgr.TipType.SKILL_ACTIVE, mAvatarData.Level + 1, skillNum, _sender.UId);
			}
			else
			{
				mFormatedTipMgr.Show(selfEffectorByProto, FormatedTipMgr.TipType.SKILL_UPGRADE, mAvatarData.Level + 1, skillNum, _sender.UId);
			}
		}
		else if (_sender.mElementId == "UPGRADE_SKILL_BUTTON")
		{
			mFormatedTipMgr.Show(selfEffectorByProto, FormatedTipMgr.TipType.SKILL_UPGRADE, mAvatarData.Level + 1, skillNum, _sender.UId);
		}
	}

	private void OnBuffMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			Effector selfEffectorByProto = mAvatarData.GetSelfEffectorByProto(_sender.mId);
			if (selfEffectorByProto != null && _sender.mElementId == "BUFF_BUTTON")
			{
				mFormatedTipMgr.Show(selfEffectorByProto, FormatedTipMgr.TipType.SKILL_ACTIVE, mAvatarData.Level + 1, -1, _sender.UId);
			}
		}
	}

	private void OnTipMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && _sender.mElementId == "INVENTORY_BUTTON")
		{
			if (mOnInitInventory != null)
			{
				mOnInitInventory();
			}
		}
		else if (_buttonId == 0 && _sender.mElementId == "UPGRADE_SKILL_BUTTON")
		{
			UpgradeSkill(_sender.mId);
		}
		else if (_buttonId == 0 && _sender.mElementId == "REBORN_BUTTON")
		{
			if (mOnForceRespawn != null)
			{
				mOnForceRespawn();
			}
		}
		else if (_buttonId == 0 && _sender.mElementId == "PORTAL_BUTTON")
		{
			string localeText = GuiSystem.GetLocaleText("GUI_EXIT_TO_CS_QUESTION");
			mDialog.SetData(localeText, "YES_TEXT", "NO_TEXT", OnDialogAnswer);
		}
		else if (_sender.mElementId == "QUEST_JOURNAL_BUTTON" && _buttonId == 0)
		{
			if (mOnQuestJournal != null)
			{
				mOnQuestJournal(-1);
			}
		}
		else if (_sender.mElementId == "SHOP_BUTTON" && _buttonId == 0 && mOnShop != null)
		{
			mOnShop();
		}
	}

	private void OnDialogAnswer(bool _yes)
	{
		if (_yes && mOnPortal != null)
		{
			mOnPortal();
		}
	}

	private void UpgradeSkill(int _id)
	{
		if (mPlayerCtrl == null || (mPlayerCtrl.HasActiveSkill && mPlayerCtrl.GetActiveSkill().Parent != null && mPlayerCtrl.GetActiveSkill().Parent.Proto != null && mPlayerCtrl.GetActiveSkill().Parent.Proto.Id == _id))
		{
			return;
		}
		SkillButtonState skillButtonState = mSkillButtonStates[_id];
		if (skillButtonState != null && !skillButtonState.mInProgress && skillButtonState.mAlive && skillButtonState.mUpgradeAvailable)
		{
			mPlayerCtrl.SelfPlayer.UpgradeSkill(_id);
			AddUpgradeSkillFX(_id);
			if (mPlayerCtrl.HasActiveSkill && mPlayerCtrl.GetActiveSkill().Parent.Proto.Id == _id)
			{
				mPlayerCtrl.RemoveActiveAbility();
			}
		}
	}

	private void AddUpgradeSkillFX(int _id)
	{
		GuiButton buttonById = GetButtonById(_id);
		if (buttonById != null && !(buttonById.mElementId == "SKILL_BOOST_BUTTON"))
		{
			if (mUpdateSkillFXs.ContainsKey(_id))
			{
				mUpdateSkillFXs[_id].mFrameNum = 0;
				return;
			}
			float width = mSkillFX2Image.width / 14;
			float height = mSkillFX2Image.height;
			UpdateSkillFXData updateSkillFXData = new UpdateSkillFXData();
			updateSkillFXData.mRect1 = new Rect(0f, 0f, width, height);
			GuiSystem.GetRectScaled(ref updateSkillFXData.mRect1);
			mUpdateSkillFXs.Add(_id, updateSkillFXData);
		}
	}

	public void SetData(IItemUsageMgr _mgr, PlayerControl _playerCtrl, MapType _mapType, IStoreContentProvider<CtrlPrototype> _items, IDictionary<int, ICollection<CtrlPrototype>> _treeItems, IStoreContentProvider<BattlePrototype> _battleItems, IDictionary<int, int> _itemsToBattleItems, HeroStore _heroStore)
	{
		Uninit();
		mPlayerCtrl = _playerCtrl;
		GameData gameData = mPlayerCtrl.SelfPlayer.Player.Avatar as GameData;
		mAvatarData = gameData.Data;
		mAvatarProto = gameData.Proto;
		mMapType = _mapType;
		if (mPortalButton != null)
		{
			mPortalButton.SetActive(mMapType == MapType.HUNT);
		}
		mFastItemAccessPanel.SetData(_mgr, mPlayerCtrl.SelfPlayer);
		mBattleItemMenu.SetData(_treeItems, _items, mAvatarData, mPlayerCtrl, _battleItems, _itemsToBattleItems, _mgr);
		mAvatarData.SubscribeEffectorsChanged(OnEffectorsChanged);
		Effector[] upgradeAvails = mPlayerCtrl.SelfPlayer.GetUpgradeAvails();
		foreach (Effector effector in upgradeAvails)
		{
			mCurrentEffectors[effector.Proto.Id] = effector;
		}
		mFaceImage = GuiSystem.GetImage(mAvatarProto.Desc.mIcon + "_04");
		if (mAvaInfo == null)
		{
			mAvaInfo = GuiSystem.mGuiSystem.GetGuiElement<AvatarInfoWindow>("battle", "AVATAR_INFO_WINDOW");
		}
		mBuffRenderer.SetData(mPlayerCtrl.SelfPlayer.Player.Hero, _items, mFormatedTipMgr, _heroStore);
		mBuffRenderer.SetAvatarData(mAvatarData);
	}

	private void OnBuyBattleItem(int _id)
	{
		if (mOnBuyItem != null)
		{
			mOnBuyItem(_id);
		}
	}

	private void OnRemoveBattleItem(int _id)
	{
		if (mOnRemoveItem != null)
		{
			mOnRemoveItem(_id);
		}
	}

	public void Clear()
	{
		Uninit();
		mBuffRenderer.Clean();
		if (mSkillButtonStates != null)
		{
			mSkillButtonStates.Clear();
		}
		if (mSkillEffectors != null)
		{
			mSkillEffectors.Clear();
		}
		if (mBattleItemMenu != null)
		{
			mBattleItemMenu.Clear();
		}
		if (mFastItemAccessPanel != null)
		{
			mFastItemAccessPanel.Clear();
		}
		if (mSkillButtons == null)
		{
			return;
		}
		foreach (GuiButton mSkillButton in mSkillButtons)
		{
			if (mSkillButton.mElementId != "SKILL_BOOST_BUTTON")
			{
				mSkillButton.mNormImg = null;
				mSkillButton.mOverImg = null;
				mSkillButton.mPressImg = null;
				mSkillButton.mIconImg = null;
			}
		}
	}

	public void AddBattleItem(int _itemId)
	{
		if (mBattleItemMenu != null)
		{
			mBattleItemMenu.AddBattleItem(_itemId);
		}
	}

	public void RemoveBattleItem(int _itemId)
	{
		if (mBattleItemMenu != null)
		{
			mBattleItemMenu.RemoveBattleItem(_itemId);
		}
	}
}
