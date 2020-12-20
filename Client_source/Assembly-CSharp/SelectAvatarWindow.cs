using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class SelectAvatarWindow : GuiElement
{
	private class ChatMsg
	{
		public string mMessage = string.Empty;

		public Rect mDrawRect = default(Rect);
	}

	private enum AvatarType
	{
		NONE,
		WARRIOR,
		KILLER,
		MAGE,
		SUPPORT
	}

	private class Skill
	{
		public Texture2D mIcon;

		public string mDesc = string.Empty;

		public string mName = string.Empty;

		public Rect mIconRect = default(Rect);

		public Rect mDescRect = default(Rect);

		public Rect mNameRect = default(Rect);
	}

	private class Avatar
	{
		public Texture2D mIcon;

		public string mName;

		public string mDesc;

		public Rect mIconRect;

		public Rect mNameRect;

		public Rect mDescRect;

		public List<Skill> mSkills;

		public Avatar()
		{
			mSkills = new List<Skill>();
		}
	}

	private class PlayerToAdd
	{
		public int mId;

		public string mName;

		public int mLvl;

		public int mTeam;

		public int mAvatar;

		public bool mReady;

		public bool mWaiting;

		public bool mSearching;
	}

	private class Player
	{
		public int mId = -1;

		public int mAvatarId = -1;

		public string mName = string.Empty;

		public string mStatus = string.Empty;

		public string mHeroLevel = string.Empty;

		public AvatarData mAvatarData;

		public Texture2D mFrame;

		public Texture2D mAvatarIcon;

		public Texture2D mStatusFrame;

		public Rect mFrameRect = default(Rect);

		public Rect mAvatarIconRect = default(Rect);

		public Rect mNameRect = default(Rect);

		public Rect mStatusRect = default(Rect);

		public Rect mStatusFrameRect = default(Rect);

		public GuiButton mReadyButton;

		public GuiElement mTutorGap;

		private bool mReady;

		public void Init()
		{
			mReadyButton = new GuiButton();
			mReadyButton.mElementId = "READY_BUTTON";
			mReadyButton.RegisterAction(UserActionType.READY_ROUND);
			mReadyButton.Init();
			mAvatarIcon = GuiSystem.GetImage("Gui/SelectAvatar/avatar_default");
			SetReadyButton();
		}

		public void SetStatus(bool _ready)
		{
			mReady = _ready;
			if (mReady)
			{
				mStatus = GuiSystem.GetLocaleText("SELECT_AVATAR_READY_TEXT");
				mStatusFrame = GuiSystem.GetImage("Gui/SelectAvatar/status_green");
			}
			else
			{
				mStatus = GuiSystem.GetLocaleText("SELECT_AVATAR_NOT_READY_TEXT");
				mStatusFrame = GuiSystem.GetImage("Gui/SelectAvatar/status_yellow");
			}
			SetReadyButton();
		}

		public void SetTutorialElement()
		{
			mTutorGap = new GuiElement();
			mTutorGap.mElementId = "AVATAR_TUTORIAL_BUTTON";
			mTutorGap.mId = 100500;
			mTutorGap.mZoneRect = mAvatarIconRect;
		}

		public void SetWaiting(bool _waiting)
		{
			if (_waiting)
			{
				mStatus = GuiSystem.GetLocaleText("SELECT_AVATAR_WAITING_TEXT");
				mStatusFrame = GuiSystem.GetImage("Gui/SelectAvatar/status_red");
			}
			else
			{
				SetStatus(mReady);
			}
		}

		public bool GetStatus()
		{
			return mReady;
		}

		public void RenderElement()
		{
			if (null != mAvatarIcon)
			{
				GuiSystem.DrawImage(mAvatarIcon, mAvatarIconRect);
			}
			GuiSystem.DrawImage(mFrame, mFrameRect);
			GuiSystem.DrawImage(mStatusFrame, mStatusFrameRect);
			GuiSystem.DrawString(mName, mNameRect, "middle_center");
			GuiSystem.DrawString(mStatus, mStatusRect, "middle_center");
			if (mReadyButton != null)
			{
				mReadyButton.RenderElement();
			}
		}

		public void SetAvatar(int _avaId, AvatarData _avatarData)
		{
			mAvatarId = _avaId;
			mAvatarData = _avatarData;
			if (mAvatarData != null)
			{
				mAvatarIcon = GuiSystem.GetImage(mAvatarData.mImg + "_03");
			}
			else if (mAvatarData == null && mAvatarId == -1)
			{
				mAvatarIcon = GuiSystem.GetImage("Gui/SelectAvatar/avatar_random");
			}
		}

		public void CleanInfo()
		{
			mAvatarIcon = GuiSystem.GetImage("Gui/SelectAvatar/avatar_default");
			mId = -1;
			mAvatarId = -1;
			mAvatarIcon = null;
			mName = string.Empty;
			mHeroLevel = string.Empty;
			mStatus = string.Empty;
			if (mReadyButton != null)
			{
				mReadyButton.mLocked = true;
			}
			mReady = false;
		}

		public void SetStatusSearching()
		{
			mStatus = GuiSystem.GetLocaleText("SELECT_AVATAR_SEARCHING_TEXT");
		}

		private void SetReadyButton()
		{
			if (mReadyButton != null)
			{
				string text = "Gui/SelectAvatar/button_" + ((!mReady) ? "2" : "1");
				mReadyButton.mLocked = mReady || (!mReady && mAvatarData == null);
				mReadyButton.mNormImg = GuiSystem.GetImage(text + "_norm");
				mReadyButton.mOverImg = GuiSystem.GetImage(text + "_over");
				mReadyButton.mPressImg = GuiSystem.GetImage(text + "_press");
				mReadyButton.Init();
			}
		}
	}

	public delegate void SelectAvatarCallback(int _id);

	public delegate void ReadyCallback();

	public delegate void DesertCallback();

	public delegate void OnSelectLockedAvatar(AvatarData _data, MapAvatarData _mapData);

	public SelectAvatarCallback mSelectAvatarCallback;

	public ReadyCallback mReadyCallback;

	public DesertCallback mDesertCallback;

	private Dictionary<int, List<Player>> mPlayers = new Dictionary<int, List<Player>>();

	private List<MapAvatarData> mAvatars = new List<MapAvatarData>();

	private int mMaxPlayersCnt = -1;

	private int mMinPlayersCnt = -1;

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private Texture2D mBackground;

	private Rect mFrame1Rect = default(Rect);

	private Rect mFrame2Rect = default(Rect);

	private int mTeam1Num = 1;

	private int mTeam2Num = 2;

	private int mMaxFrameCount;

	private int mCurFrame;

	private Texture2D mSelectImgLeft;

	private Texture2D mSelectImgRight;

	private List<Rect> mSelectDrawRectsLeft = new List<Rect>();

	private List<Rect> mSelectDrawRectsRight = new List<Rect>();

	private Rect mSelectFrameRect;

	private string mHelpText = string.Empty;

	private string mSelectText = string.Empty;

	private Rect mSelectTextRect = default(Rect);

	private List<int> mAvailableUsers = new List<int>();

	private bool mIsPlayerSelecting = true;

	private Dictionary<AvatarType, List<GuiButton>> mAvatarButtons = new Dictionary<AvatarType, List<GuiButton>>();

	private AvatarData mSelectedAvatar;

	private int mSelectedLockedAvatarId = -1;

	private Avatar mCurAvatar;

	private Player mSelfPlayer;

	private MapType mCurentType;

	private List<PlayerToAdd> mPlayersToAdd = new List<PlayerToAdd>();

	private GuiButton mBackButton;

	private GuiButton mReadyButton;

	private GuiButton mRandomButton;

	private float mTimeToStartBegin;

	private int mTimeToStart;

	private int mCurTimeToStart;

	private Rect mTimeRect;

	public int mBanTimer;

	private ChatChannel mCurMode = ChatChannel.area;

	private Chat mChatMgr;

	private ChatZone mChatZone;

	private StaticTextField mMsgTF;

	private GuiButton mSendBtn;

	private YesNoDialog mYesNoDialog;

	private OkDialog mOkDialog;

	private List<string> mMsgRecipients = new List<string>();

	private bool mUpdateFocus;

	public Dictionary<string, AvaParamData> mAvaParamData;

	public OnSelectLockedAvatar mOnSelectLockedAvatar;

	private bool mReadyPressed;

	private TimerRenderer mTimerRenderer;

	private bool mTimerStarted;

	private string mWarriorText;

	private string mKillerText;

	private string mMageText;

	private string mSupportText;

	private string mHealthText;

	private string mAttackText;

	private string mManaText;

	private string mHealthRegenText;

	private string mSpeedText;

	private string mManaRegenText;

	private string mFizArmorText;

	private string mMagArmorText;

	private string mSkillText;

	private Rect mWarriorRect;

	private Rect mKillerRect;

	private Rect mMageRect;

	private Rect mSupportRect;

	private Rect mHealthRect;

	private Rect mAttackRect;

	private Rect mManaRect;

	private Rect mHealthRegenRect;

	private Rect mSpeedRect;

	private Rect mManaRegenRect;

	private Rect mFizArmorRect;

	private Rect mMagArmorRect;

	private Rect mSkillRect;

	private Color mNormalBrown = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);

	private Color mHealthRed = new Color(101f / 255f, 23f / 255f, 11f / 255f);

	private Color mManaBlue = new Color(44f / 255f, 58f / 255f, 28f / 85f);

	private Color mAttackPurple = new Color(14f / 51f, 23f / 255f, 0.235294119f);

	private Color mClassBlue = new Color(41f / 85f, 32f / 51f, 57f / 85f);

	private bool mExitEnabled = true;

	private int mLastSelectedAvatar;

	private int mSelfPlayerId;

	private Hero mSelfHero;

	private CtrlAvatarStore mCtrlAvatarStore;

	public MapType CurrentType => mCurentType;

	public bool ExitEnabled
	{
		get
		{
			return mExitEnabled;
		}
		set
		{
			mExitEnabled = value;
		}
	}

	public override void Init()
	{
		mFrame1 = GuiSystem.GetImage("Gui/SelectAvatar/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/SelectAvatar/frame2");
		mBackground = GuiSystem.GetImage("Gui/misc/background1");
		mChatZone = new ChatZone(_hasScroll: false);
		mChatZone.mStyle = "middle_left_11_bold";
		mChatZone.mColor = mNormalBrown;
		ChatZone chatZone = mChatZone;
		chatZone.mOnNick = (ChatZone.OnNick)Delegate.Combine(chatZone.mOnNick, new ChatZone.OnNick(OnNickClick));
		mChatZone.Init();
		mMsgTF = new StaticTextField();
		mMsgTF.mElementId = "TEXT_FIELD_MSG";
		mMsgTF.mLength = 110;
		mMsgTF.mStyleId = "text_field_4";
		mSendBtn = GuiSystem.CreateButton("Gui/SelectAvatar/button_6_norm", "Gui/SelectAvatar/button_6_over", "Gui/SelectAvatar/button_6_press", string.Empty, string.Empty);
		mSendBtn.mElementId = "BUTTON_SEND";
		GuiButton guiButton = mSendBtn;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mSendBtn.Init();
		mSelectImgLeft = GuiSystem.GetImage("Gui/SelectAvatar/hint_selection_left");
		mSelectImgRight = GuiSystem.GetImage("Gui/SelectAvatar/hint_selection_right");
		mMaxFrameCount = 14;
		mCurAvatar = new Avatar();
		mBackButton = new GuiButton();
		mBackButton.mElementId = "BACK_BUTTON";
		mBackButton.mNormImg = GuiSystem.GetImage("Gui/SelectAvatar/button_4_norm");
		mBackButton.mOverImg = GuiSystem.GetImage("Gui/SelectAvatar/button_4_over");
		mBackButton.mPressImg = GuiSystem.GetImage("Gui/SelectAvatar/button_4_press");
		mBackButton.mLabelStyle = "middle_center_18";
		GuiButton guiButton2 = mBackButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mBackButton.mLabel = GuiSystem.GetLocaleText("Logout_Button_Name");
		mBackButton.RegisterAction(UserActionType.EXIT_FROM_LOBBY);
		mBackButton.Init();
		mRandomButton = GuiSystem.CreateButton("Gui/SelectAvatar/button_3_norm", "Gui/SelectAvatar/button_3_over", "Gui/SelectAvatar/button_3_press", string.Empty, string.Empty);
		mRandomButton.mId = -1;
		mRandomButton.mElementId = "RANDOM_BUTTON";
		GuiButton guiButton3 = mRandomButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnSelectButton));
		mRandomButton.Init();
		mReadyButton = new GuiButton();
		mReadyButton.mElementId = "READY_BUTTON";
		mReadyButton.mNormImg = GuiSystem.GetImage("Gui/SelectAvatar/button_5_norm");
		mReadyButton.mOverImg = GuiSystem.GetImage("Gui/SelectAvatar/button_5_over");
		mReadyButton.mPressImg = GuiSystem.GetImage("Gui/SelectAvatar/button_5_press");
		mReadyButton.mLabelStyle = "middle_center_18";
		GuiButton guiButton4 = mReadyButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mReadyButton.mLabel = GuiSystem.GetLocaleText("Ready_Button_Name");
		mReadyButton.mLockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		mReadyButton.mLocked = true;
		mReadyButton.RegisterAction(UserActionType.READY_BUTTON);
		mReadyButton.Init();
		AddTutorialElement(mReadyButton.mElementId, mReadyButton);
		for (AvatarType avatarType = AvatarType.WARRIOR; avatarType <= AvatarType.SUPPORT; avatarType++)
		{
			mAvatarButtons.Add(avatarType, new List<GuiButton>());
		}
		mYesNoDialog = new YesNoDialog();
		mYesNoDialog.Init();
		mYesNoDialog.SetActive(_active: false);
		mOkDialog = new OkDialog();
		mOkDialog.Init();
		mOkDialog.SetActive(_active: false);
		mAvaParamData = new Dictionary<string, AvaParamData>();
		mAvaParamData.Add("Health", new AvaParamData());
		mAvaParamData.Add("HealthRegen", new AvaParamData());
		mAvaParamData.Add("DamageMin", new AvaParamData());
		mAvaParamData.Add("AttackSpeed", new AvaParamData());
		mAvaParamData.Add("Mana", new AvaParamData());
		mAvaParamData.Add("ManaRegen", new AvaParamData());
		mAvaParamData.Add("PhysArmor", new AvaParamData());
		mAvaParamData.Add("MagicArmor", new AvaParamData());
		mPlayers.Add(mTeam1Num, new List<Player>());
		mPlayers.Add(mTeam2Num, new List<Player>());
		mTimerRenderer = new TimerRenderer();
		mTimerRenderer.Init();
		mWarriorText = GuiSystem.GetLocaleText("GUI_SEL_AVA_WARRIOR");
		mKillerText = GuiSystem.GetLocaleText("GUI_SEL_AVA_KILLER");
		mMageText = GuiSystem.GetLocaleText("GUI_SEL_AVA_MAGE");
		mSupportText = GuiSystem.GetLocaleText("GUI_SEL_AVA_SUPPORT");
		mHealthText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HEALTH");
		mAttackText = GuiSystem.GetLocaleText("GUI_SEL_AVA_ATTACK");
		mManaText = GuiSystem.GetLocaleText("GUI_SEL_AVA_MANA");
		mHealthRegenText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HEALTH_REGEN");
		mSpeedText = GuiSystem.GetLocaleText("GUI_SEL_AVA_SPEED");
		mManaRegenText = GuiSystem.GetLocaleText("GUI_SEL_AVA_MANA_REGEN");
		mFizArmorText = GuiSystem.GetLocaleText("GUI_SEL_AVA_FIZ_ARMOR");
		mMagArmorText = GuiSystem.GetLocaleText("GUI_SEL_AVA_MAG_ARMOR");
		mSkillText = GuiSystem.GetLocaleText("GUI_SEL_AVA_SKILL");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mBackground.width, mBackground.height);
		GuiSystem.GetRectScaled(ref mZoneRect, _ignoreLowRate: true);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mTimerRenderer.mZoneRect = mZoneRect;
		mTimerRenderer.SetSize();
		mFrame1Rect = new Rect(0f, 33f, mFrame1.width, mFrame1.height);
		GuiSystem.GetRectScaled(ref mFrame1Rect);
		mFrame1Rect.x = ((float)OptionsMgr.mScreenWidth - mFrame1Rect.width) / 2f;
		mFrame2Rect = new Rect(0f, -8f, mFrame2.width, mFrame2.height);
		GuiSystem.GetRectScaled(ref mFrame2Rect);
		mFrame2Rect.x = ((float)OptionsMgr.mScreenWidth - mFrame2Rect.width) / 2f;
		mMsgTF.mZoneRect = new Rect(41f, 883f, 708f, 26f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mMsgTF.mZoneRect);
		mSendBtn.mZoneRect = new Rect(749f, 877f, 80f, 38f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mSendBtn.mZoneRect);
		mRandomButton.mZoneRect = new Rect(400f, 335f, 56f, 56f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mRandomButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT97", mRandomButton.mZoneRect);
		mBackButton.mZoneRect = new Rect(10f, 895f, 198f, 49f);
		GuiSystem.GetRectScaled(ref mBackButton.mZoneRect);
		mReadyButton.mZoneRect = new Rect(319f, 705f, 218f, 49f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mReadyButton.mZoneRect);
		mTimeRect = new Rect(793f, 21f, 110f, 17f);
		GuiSystem.SetChildRect(mFrame2Rect, ref mTimeRect);
		InitAvatarRects(ref mCurAvatar);
		InitAvatarParamDataRects();
		InitPlayersRects();
		mChatZone.mZoneRect = new Rect(41f, 782f, 708f, 90f);
		mChatZone.mTextRect = new Rect(41f, 782f, 708f, 90f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mChatZone.mZoneRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mChatZone.mTextRect);
		mChatZone.SetSize();
		mYesNoDialog.SetSize();
		mOkDialog.SetSize();
		mSelectTextRect = new Rect(110f, 10f, 630f, 40f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mSelectTextRect);
		mIgnoreEventRect = mMsgTF.mZoneRect;
		mWarriorRect = new Rect(62f, 76f, 130f, 20f);
		mKillerRect = new Rect(262f, 76f, 130f, 20f);
		mMageRect = new Rect(462f, 76f, 130f, 20f);
		mSupportRect = new Rect(662f, 76f, 130f, 20f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mWarriorRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mKillerRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mMageRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mSupportRect);
		mHealthRect = new Rect(58f, 560f, 75f, 30f);
		mAttackRect = new Rect(173f, 560f, 75f, 30f);
		mManaRect = new Rect(288f, 560f, 75f, 30f);
		mHealthRegenRect = new Rect(30f, 619f, 150f, 20f);
		mSpeedRect = new Rect(145f, 619f, 150f, 20f);
		mManaRegenRect = new Rect(260f, 619f, 150f, 20f);
		mFizArmorRect = new Rect(30f, 658f, 170f, 20f);
		mMagArmorRect = new Rect(200f, 658f, 170f, 20f);
		mSkillRect = new Rect(440f, 395f, 390f, 30f);
		GuiSystem.SetChildRect(mFrame1Rect, ref mHealthRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mAttackRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mManaRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mHealthRegenRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mSpeedRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mManaRegenRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mFizArmorRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mMagArmorRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mSkillRect);
		Rect _rect = new Rect(15f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT85", _rect);
		_rect = new Rect(151f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT86", _rect);
		_rect = new Rect(287f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT87", _rect);
		_rect = new Rect(423f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT88", _rect);
		_rect = new Rect(559f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT89", _rect);
		_rect = new Rect(695f, 0f, 106f, 21f);
		GuiSystem.SetChildRect(mFrame1Rect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT90", _rect);
	}

	public override void Update()
	{
		mChatZone.Update();
		if (mPlayersToAdd.Count > 0 && mMaxPlayersCnt != -1)
		{
			foreach (PlayerToAdd item in mPlayersToAdd)
			{
				AddPlayer(item);
			}
			mPlayersToAdd.Clear();
		}
		if (mAvailableUsers.Count > 0)
		{
			SetAvailableHints(mAvailableUsers);
			mAvailableUsers.Clear();
		}
		if (mTimeToStart > 0 && mCurTimeToStart < mTimeToStart)
		{
			mCurTimeToStart = Mathf.FloorToInt(Time.time - mTimeToStartBegin);
			if (mCurTimeToStart > mTimeToStart)
			{
				mTimeToStart = 0;
				mCurTimeToStart = 0;
			}
			if (!IsClanWar())
			{
				if (mTimeToStart - mCurTimeToStart <= 5)
				{
					if (!mTimerStarted)
					{
						Lock();
						mTimerStarted = true;
						mTimerRenderer.Start();
					}
				}
				else if (mTimerStarted)
				{
					mTimerStarted = false;
					mTimerRenderer.Stop();
				}
			}
		}
		if (Time.frameCount % 5 == 0)
		{
			mCurFrame++;
			mCurFrame = ((mCurFrame < mMaxFrameCount) ? mCurFrame : 0);
			SetFrameRect();
		}
		mTimerRenderer.Update();
	}

	private void SetFrameRect()
	{
		float num = 1f / (float)mMaxFrameCount;
		mSelectFrameRect = new Rect(0f, (float)mCurFrame * num, 1f, num);
	}

	public void ClearLobby()
	{
		UninitCommonData();
		Lock();
		InitPlayers();
		mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_BATTLE_BROKEN"));
		mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP5");
	}

	private void UninitCommonData()
	{
		mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP1");
		mExitEnabled = true;
		if (mBackButton != null)
		{
			mBackButton.mLocked = false;
		}
		mLastSelectedAvatar = 0;
		ClearTimers();
		CleanPlayers();
		SetAvailable(new List<int>());
		mIsPlayerSelecting = true;
		mSelfPlayer = null;
		mPlayersToAdd.Clear();
		if (mTimerRenderer != null)
		{
			mTimerRenderer.Stop();
		}
		if (mMsgTF != null)
		{
			mMsgTF.Uninit();
		}
	}

	public override void Uninit()
	{
		UninitCommonData();
		ClearMapData();
		mSelectDrawRectsLeft.Clear();
		mSelectDrawRectsRight.Clear();
	}

	public void ClearTimers()
	{
		mTimeToStart = 0;
		mCurTimeToStart = 0;
		mTimeToStartBegin = 0f;
	}

	private void ClearMapData()
	{
		mAvatars.Clear();
		mMaxPlayersCnt = -1;
		mMinPlayersCnt = -1;
		if (mYesNoDialog != null)
		{
			mYesNoDialog.Clean();
		}
		if (mOkDialog != null)
		{
			mOkDialog.Clean();
		}
		mSelectedAvatar = null;
		InitAvatarInfo(mSelectedAvatar);
		if (mAvaParamData != null)
		{
			foreach (AvaParamData value in mAvaParamData.Values)
			{
				value.Clear();
			}
		}
		if (mAvatarButtons.Count > 0)
		{
			for (AvatarType avatarType = AvatarType.WARRIOR; avatarType <= AvatarType.SUPPORT; avatarType++)
			{
				mAvatarButtons[avatarType].Clear();
			}
		}
		if (mReadyButton != null)
		{
			mReadyButton.mLocked = true;
		}
		if (mRandomButton != null)
		{
			mRandomButton.mLocked = false;
		}
	}

	private void CleanPlayers()
	{
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			mPlayer.Value.Clear();
		}
	}

	private int AvatarSorter(MapAvatarData _avatar1, MapAvatarData _avatar2)
	{
		return _avatar1.mId.CompareTo(_avatar2.mId);
	}

	public void SetData(IEnumerable<MapAvatarData> _avatars, IDictionary<string, AddStat> _heroStats, int _minPlayers, int _maxPlayers, int _mapType)
	{
		ClearMapData();
		mAvatars.AddRange(_avatars);
		mMaxPlayersCnt = _maxPlayers;
		mMinPlayersCnt = _minPlayers;
		mReadyPressed = false;
		mAvatars.Sort(AvatarSorter);
		foreach (MapAvatarData mAvatar in mAvatars)
		{
			InitAvatarButton(mAvatar);
		}
		mCurentType = (MapType)_mapType;
		switch (mCurentType)
		{
		case MapType.DM:
		case MapType.DOTA:
		case MapType.CW_DOTA:
		case MapType.CW_SIEGE:
			mCurMode = ChatChannel.fight_request;
			break;
		case MapType.HUNT:
			mCurMode = ChatChannel.area;
			break;
		}
		mChatZone.AddChannel(mCurMode);
		mChatZone.SetCurChatMode(mCurMode);
		InitPlayers();
		mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP1");
	}

	private bool GetAvatarUnlocked(int _id)
	{
		MapAvatarData mapAvatarById = GetMapAvatarById(_id);
		if (mapAvatarById == null)
		{
			return false;
		}
		return mapAvatarById.mAvailable && !mapAvatarById.mDenyForMap;
	}

	private bool GetAvatarAvailable(int _id)
	{
		return GetMapAvatarById(_id)?.mAvailable ?? false;
	}

	private bool GetAvatarDenyForMap(int _id)
	{
		return GetMapAvatarById(_id)?.mDenyForMap ?? false;
	}

	public void AddPlayer(int _id, string _name, int _lvl, int _team)
	{
		PlayerToAdd playerToAdd = new PlayerToAdd();
		playerToAdd.mId = _id;
		playerToAdd.mName = _name;
		playerToAdd.mLvl = _lvl;
		playerToAdd.mTeam = _team;
		mPlayersToAdd.Add(playerToAdd);
	}

	public bool IsClanWar()
	{
		if (mCurentType == MapType.CW_DOTA || mCurentType == MapType.CW_SIEGE)
		{
			return true;
		}
		return false;
	}

	private void AddPlayer(PlayerToAdd _player)
	{
		if (!mPlayers.ContainsKey(_player.mTeam))
		{
			return;
		}
		foreach (Player item in mPlayers[_player.mTeam])
		{
			if (item.mId == -1)
			{
				item.mId = _player.mId;
				item.mName = _player.mName;
				item.mHeroLevel = _player.mLvl.ToString();
				PlayerReady(item.mId, _player.mReady);
				if (!IsClanWar())
				{
					PlayerInRequestState(item.mId, _player.mWaiting);
				}
				SelectAvatar(item.mId, _player.mAvatar);
				if (item.mId == mSelfPlayerId)
				{
					mSelfPlayer = item;
					mSelfPlayer.SetTutorialElement();
					AddTutorialElement("PLAYER_READY_BUTTON", mSelfPlayer.mReadyButton);
					AddTutorialElement("PLAYER_AVATAR_ROUND_BUTTON", mSelfPlayer.mTutorGap);
				}
				break;
			}
		}
	}

	public void RemovePlayer(int _id)
	{
		Player player = GetPlayer(_id);
		if (player == null)
		{
			PlayerToAdd playerToAdd = GetPlayerToAdd(_id);
			if (playerToAdd != null)
			{
				mPlayersToAdd.Remove(playerToAdd);
			}
		}
		else
		{
			player.CleanInfo();
			if (mCurentType == MapType.DM || mCurentType == MapType.DOTA)
			{
				SetPlayersSearchingStatus();
			}
		}
	}

	private void SetPlayersSearchingStatus()
	{
		int num = 0;
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			num = 0;
			foreach (Player item in mPlayer.Value)
			{
				if (num < mMinPlayersCnt / 2 && item.mId == -1)
				{
					item.SetStatusSearching();
				}
				num++;
			}
		}
	}

	public void PlayerReady(int _id, bool _ready)
	{
		Player player = GetPlayer(_id);
		if (player == null)
		{
			PlayerToAdd playerToAdd = GetPlayerToAdd(_id);
			if (playerToAdd != null)
			{
				playerToAdd.mReady = _ready;
			}
			return;
		}
		player.SetStatus(_ready);
		if (mSelfPlayer == player && _ready)
		{
			if (mCurentType != MapType.HUNT)
			{
				mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP3");
			}
			else
			{
				mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP4");
			}
			Lock();
			if (mReadyButton != null)
			{
				mReadyButton.mLocked = true;
			}
			if (mRandomButton != null)
			{
				mRandomButton.mLocked = true;
			}
		}
	}

	public void PlayerInRequestState(int _id, bool _inRequest)
	{
		Player player = GetPlayer(_id);
		if (player == null)
		{
			PlayerToAdd playerToAdd = GetPlayerToAdd(_id);
			if (playerToAdd != null)
			{
				playerToAdd.mWaiting = _inRequest;
			}
		}
		else
		{
			player.SetWaiting(!_inRequest);
		}
	}

	public void SelectAvatar(int _id, int _avaId)
	{
		if (mSelfPlayerId == _id && _avaId != 0)
		{
			mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP2");
		}
		Player player = GetPlayer(_id);
		if (player == null)
		{
			foreach (PlayerToAdd item in mPlayersToAdd)
			{
				if (_id == item.mId)
				{
					item.mAvatar = _avaId;
					break;
				}
			}
			return;
		}
		if (!mReadyPressed || mCurentType != MapType.HUNT)
		{
			AvatarData avatarById = GetAvatarById(_avaId);
			player.SetAvatar(_avaId, avatarById);
		}
		if (mRandomButton != null && player == mSelfPlayer)
		{
			mRandomButton.mLocked = _avaId == -1;
		}
		SetAvatarButtonsState();
	}

	public void SetReadyStatus(bool _status)
	{
		if (mReadyButton != null)
		{
			mReadyButton.mLocked = !_status;
		}
	}

	public void StartTimer(int _time)
	{
		mTimeToStartBegin = Time.time;
		mTimeToStart = _time;
		mCurTimeToStart = 0;
		mTimerStarted = false;
		if (mTimeToStart - mCurTimeToStart <= 5 && mTimeToStart > 0)
		{
			if (!mTimerStarted)
			{
				mHelpText = GuiSystem.GetLocaleText("GUI_SEL_AVA_HELP4");
				mTimerStarted = true;
				if (mTimerRenderer != null)
				{
					mTimerRenderer.Start();
				}
			}
		}
		else if (mTimerStarted)
		{
			mTimerStarted = false;
			if (mTimerRenderer != null)
			{
				mTimerRenderer.Stop();
			}
		}
	}

	private Player GetPlayer(int _id)
	{
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			foreach (Player item in mPlayer.Value)
			{
				if (item.mId == _id)
				{
					return item;
				}
			}
		}
		return null;
	}

	private PlayerToAdd GetPlayerToAdd(int _id)
	{
		foreach (PlayerToAdd item in mPlayersToAdd)
		{
			if (item.mId == _id)
			{
				return item;
			}
		}
		return null;
	}

	private MapAvatarData GetMapAvatarById(int _avatarId)
	{
		foreach (MapAvatarData mAvatar in mAvatars)
		{
			if (_avatarId == mAvatar.mId)
			{
				return mAvatar;
			}
		}
		return null;
	}

	private AvatarData GetAvatarById(int _avatarId)
	{
		if (mCtrlAvatarStore == null)
		{
			return null;
		}
		return mCtrlAvatarStore.TryGet(_avatarId);
	}

	private void InitAvatarButton(MapAvatarData _mapAvatarData)
	{
		AvatarData avatarData = mCtrlAvatarStore.TryGet(_mapAvatarData.mId);
		if (avatarData == null)
		{
			Log.Error("Can't find avatar data : " + _mapAvatarData.mId);
			return;
		}
		AvatarType mType = (AvatarType)avatarData.mType;
		if (!mAvatarButtons.ContainsKey(mType))
		{
			Log.Error(string.Concat("Unexpected avatar type : ", mType, " for : ", avatarData.mName));
			return;
		}
		GuiButton _btn = GuiSystem.CreateButton(string.Empty, "Gui/misc/button_3_over", "Gui/misc/button_3_press", avatarData.mImg + "_01", string.Empty);
		_btn.mElementId = "AVATAR_BUTTON";
		_btn.mId = avatarData.mId;
		InitAvatarButtonRect(ref _btn, mType);
		GuiButton guiButton = _btn;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnSelectButton));
		GuiButton guiButton2 = _btn;
		guiButton2.mOnMouseLockedUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseLockedUp, new OnMouseUp(OnSelectLockedButton));
		GuiButton guiButton3 = _btn;
		guiButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton3.mOnMouseEnter, new OnMouseEnter(OnSelectButtonEnter));
		GuiButton guiButton4 = _btn;
		guiButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton4.mOnMouseLeave, new OnMouseLeave(OnSelectButtonLeave));
		_btn.mIconOnTop = false;
		_btn.Init();
		_btn.mLocked = !_mapAvatarData.mAvailable;
		mAvatarButtons[mType].Add(_btn);
		if (_mapAvatarData.mId == 301)
		{
			AddTutorialElement(_btn.mElementId + "_" + _mapAvatarData.mId, _btn);
		}
	}

	private void InitAvatarButtonRect(ref GuiButton _btn, AvatarType _avaType)
	{
		if (mAvatarButtons.ContainsKey(_avaType))
		{
			int count = mAvatarButtons[_avaType].Count;
			float num = 59f;
			float num2 = 111f;
			num += (float)((int)(_avaType - 1) * 199);
			float num3 = (int)Mathf.Floor(count / 3);
			float num4 = (float)count - num3 * 3f;
			_btn.mZoneRect = new Rect(num + num4 * 48f, num2 + num3 * 49f, 43f, 44f);
			GuiSystem.SetChildRect(mFrame1Rect, ref _btn.mZoneRect);
		}
	}

	private void InitPlayers()
	{
		if (mMaxPlayersCnt == -1 || (mPlayers[mTeam1Num].Count == mMaxPlayersCnt / 2 && mPlayers[mTeam2Num].Count == mMaxPlayersCnt / 2))
		{
			return;
		}
		if (mCurentType != MapType.HUNT)
		{
			int i = 0;
			for (int num = mMaxPlayersCnt / 2; i < num; i++)
			{
				InitPlayer(i, mTeam1Num);
				InitPlayer(i, mTeam2Num);
			}
		}
		else
		{
			InitPlayer(0, mTeam1Num);
		}
		InitPlayersRects();
	}

	private void InitPlayer(int _playerNum, int _teamNum)
	{
		Player player = new Player();
		if (_teamNum == mTeam1Num)
		{
			player.mFrame = GuiSystem.GetImage("Gui/SelectAvatar/frame3");
		}
		else
		{
			player.mFrame = GuiSystem.GetImage("Gui/SelectAvatar/frame4");
		}
		player.Init();
		GuiButton guiButton = player.mReadyButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mPlayers[_teamNum].Add(player);
	}

	private void InitPlayersRects()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 70f * GuiSystem.mYRate;
		float num8 = 76f * GuiSystem.mYRate;
		int num9 = 0;
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			num9 = 0;
			foreach (Player item in mPlayer.Value)
			{
				if (mPlayer.Key == mTeam1Num)
				{
					num = mFrame1Rect.x - (float)item.mFrame.width * GuiSystem.mYRate;
					num2 = 11f;
					num5 = 160f;
					num3 = 65f;
					num4 = 65f;
					num6 = 65f;
				}
				else
				{
					num = mFrame1Rect.x + mFrame1Rect.width;
					num2 = 126f;
					num5 = 3f;
					num3 = 5f;
					num4 = 5f;
					num6 = 5f;
				}
				item.mFrameRect.x = num;
				item.mFrameRect.y = num7 + (float)num9 * num8;
				item.mFrameRect.width = (float)item.mFrame.width * GuiSystem.mYRate;
				item.mFrameRect.height = (float)item.mFrame.height * GuiSystem.mYRate;
				item.mAvatarIconRect = new Rect(num2, 10f, 52f, 52f);
				GuiSystem.SetChildRect(item.mFrameRect, ref item.mAvatarIconRect);
				item.mNameRect = new Rect(num3, 11f, 119f, 18f);
				GuiSystem.SetChildRect(item.mFrameRect, ref item.mNameRect);
				item.mStatusRect = new Rect(num4, 40f, 119f, 18f);
				GuiSystem.SetChildRect(item.mFrameRect, ref item.mStatusRect);
				item.mReadyButton.mZoneRect = new Rect(num5, 38f, 26f, 26f);
				GuiSystem.SetChildRect(item.mFrameRect, ref item.mReadyButton.mZoneRect);
				item.mStatusFrameRect = new Rect(num6, 41f, 119f, 18f);
				GuiSystem.SetChildRect(item.mFrameRect, ref item.mStatusFrameRect);
				num9++;
			}
		}
	}

	private void InitAvatarRects(ref Avatar _avatar)
	{
		_avatar.mIconRect = new Rect(36f, 437f, 91f, 92f);
		_avatar.mNameRect = new Rect(24f, 400f, 344f, 20f);
		_avatar.mDescRect = new Rect(147f, 428f, 215f, 132f);
		_avatar.mSkills.Clear();
		for (int i = 0; i < 4; i++)
		{
			Skill skill = new Skill();
			skill.mIconRect = new Rect(380f, 437 + i * 67, 46f, 46f);
			skill.mNameRect = new Rect(443f, 426 + i * 68, 382f, 16f);
			skill.mDescRect = new Rect(443f, 445 + i * 68, 382f, 46f);
			GuiSystem.SetChildRect(mFrame1Rect, ref skill.mIconRect);
			GuiSystem.SetChildRect(mFrame1Rect, ref skill.mNameRect);
			GuiSystem.SetChildRect(mFrame1Rect, ref skill.mDescRect);
			_avatar.mSkills.Add(skill);
		}
		GuiSystem.SetChildRect(mFrame1Rect, ref mCurAvatar.mIconRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mCurAvatar.mNameRect);
		GuiSystem.SetChildRect(mFrame1Rect, ref mCurAvatar.mDescRect);
	}

	private void InitAvatarParamDataRects()
	{
		Color mColor = mNormalBrown;
		foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
		{
			mAvaParamDatum.Value.mColor = mColor;
			switch (mAvaParamDatum.Key)
			{
			case "Health":
				mAvaParamDatum.Value.mDrawRect = new Rect(32f, 599f, 100f, 20f);
				mAvaParamDatum.Value.mAddColor = mHealthRed;
				break;
			case "HealthRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(32f, 634f, 100f, 18f);
				mAvaParamDatum.Value.mAddColor = mHealthRed;
				break;
			case "DamageMin":
				mAvaParamDatum.Value.mDrawRect = new Rect(147f, 599f, 100f, 20f);
				mAvaParamDatum.Value.mAddColor = mAttackPurple;
				break;
			case "AttackSpeed":
				mAvaParamDatum.Value.mDrawRect = new Rect(147f, 634f, 100f, 18f);
				mAvaParamDatum.Value.mAddColor = mAttackPurple;
				break;
			case "Mana":
				mAvaParamDatum.Value.mDrawRect = new Rect(262f, 599f, 100f, 20f);
				mAvaParamDatum.Value.mAddColor = mManaBlue;
				break;
			case "ManaRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(262f, 634f, 100f, 18f);
				mAvaParamDatum.Value.mAddColor = mManaBlue;
				break;
			case "PhysArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(32f, 675f, 157f, 14f);
				mAvaParamDatum.Value.mAddColor = mHealthRed;
				break;
			case "MagicArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(204f, 675f, 157f, 14f);
				mAvaParamDatum.Value.mAddColor = mManaBlue;
				break;
			}
			GuiSystem.SetChildRect(mFrame1Rect, ref mAvaParamDatum.Value.mDrawRect);
		}
	}

	private void InitAvatarParams(AvatarData _avatarData)
	{
		if (_avatarData == null)
		{
			foreach (AvaParamData value in mAvaParamData.Values)
			{
				value.Clear();
			}
		}
		else if (mSelfHero != null)
		{
			mAvaParamData["Health"].mValue = GetStringParamValue("Health", _avatarData.mHealth);
			mAvaParamData["Health"].mAdd = GetStringParamValueAdd("Health", _avatarData.mHealth);
			mAvaParamData["HealthRegen"].mValue = GetStringParamValue("HealthRegen", _avatarData.mHealthRegen);
			mAvaParamData["HealthRegen"].mAdd = GetStringParamValueAdd("HealthRegen", _avatarData.mHealthRegen);
			mAvaParamData["DamageMin"].mValue = GetStringParamValue("DamageMin", _avatarData.mMinDmg, _avatarData.mMaxDmg);
			mAvaParamData["DamageMin"].mAdd = GetStringParamValueAdd("DamageMin", _avatarData.mMinDmg);
			mAvaParamData["AttackSpeed"].mValue = GetStringParamValue("AttackSpeed", _avatarData.mAttackSpeed);
			mAvaParamData["AttackSpeed"].mAdd = GetStringParamValueAdd("AttackSpeed", _avatarData.mAttackSpeed);
			mAvaParamData["Mana"].mValue = GetStringParamValue("Mana", _avatarData.mMana);
			mAvaParamData["Mana"].mAdd = GetStringParamValueAdd("Mana", _avatarData.mMana);
			mAvaParamData["ManaRegen"].mValue = GetStringParamValue("ManaRegen", _avatarData.mManaRegen);
			mAvaParamData["ManaRegen"].mAdd = GetStringParamValueAdd("ManaRegen", _avatarData.mManaRegen);
			mAvaParamData["PhysArmor"].mValue = GetStringParamValue("PhysArmor", _avatarData.mArmour);
			mAvaParamData["PhysArmor"].mAdd = GetStringParamValueAdd("PhysArmor", _avatarData.mArmour);
			mAvaParamData["MagicArmor"].mValue = GetStringParamValue("MagicArmor", _avatarData.mMagicArmour);
			mAvaParamData["MagicArmor"].mAdd = GetStringParamValueAdd("MagicArmor", _avatarData.mMagicArmour);
		}
	}

	private string GetStringParamValue(string _param, float _defVal)
	{
		return GetStringParamValue(_param, _defVal, 0f);
	}

	private string GetStringParamValue(string _param, float _defVal, float _secondDefVal)
	{
		if (!mSelfHero.GameInfo.mAddStats.ContainsKey(_param))
		{
			switch (_param)
			{
			case "DamageMin":
				return Mathf.RoundToInt(_defVal) + "-" + Mathf.RoundToInt(_secondDefVal);
			case "Health":
			case "Mana":
				return Mathf.RoundToInt(_defVal).ToString();
			case "PhysArmor":
			case "MagicArmor":
				return (_defVal * 100f).ToString("0.##");
			case "ManaRegen":
			case "HealthRegen":
				return _defVal.ToString("0.##");
			case "AttackSpeed":
				return _defVal.ToString("0.##");
			default:
				return "--";
			}
		}
		AddStat addStat = mSelfHero.GameInfo.mAddStats[_param];
		switch (_param)
		{
		case "DamageMin":
			return Mathf.RoundToInt(_defVal * (addStat.mMul + 1f)) + "-" + Mathf.RoundToInt(_secondDefVal * (addStat.mMul + 1f));
		case "Health":
		case "Mana":
			return Mathf.RoundToInt(_defVal * (addStat.mMul + 1f)).ToString();
		case "PhysArmor":
		case "MagicArmor":
			return (_defVal * (addStat.mMul + 1f) * 100f).ToString("0.##");
		case "ManaRegen":
		case "HealthRegen":
			return (_defVal * (addStat.mMul + 1f)).ToString("0.##");
		case "AttackSpeed":
			return (_defVal + addStat.mMul).ToString("0.##");
		default:
			return "--";
		}
	}

	private string GetStringParamValueAdd(string _param, float _defVal)
	{
		if (!mSelfHero.GameInfo.mAddStats.ContainsKey(_param))
		{
			return "+0";
		}
		AddStat addStat = mSelfHero.GameInfo.mAddStats[_param];
		if (_param != "PhysArmor" && _param != "MagicArmor")
		{
			return "+" + addStat.mAdd.ToString("0.##");
		}
		return "+" + (_defVal + addStat.mAdd * 100f).ToString("0.##") + "%";
	}

	private void InitAvatarInfo(AvatarData _avatarData)
	{
		if (mCurAvatar == null)
		{
			return;
		}
		if (_avatarData != null && mCurAvatar != null)
		{
			mCurAvatar.mName = GuiSystem.GetLocaleText(_avatarData.mName);
			mCurAvatar.mDesc = GuiSystem.GetLocaleText(_avatarData.mLongDesc);
			mCurAvatar.mIcon = GuiSystem.GetImage(_avatarData.mImg + "_02");
			int i = 0;
			for (int count = _avatarData.mSkills.Count; i < count; i++)
			{
				mCurAvatar.mSkills[i].mIcon = GuiSystem.GetImage("Gui/Icons/skills/" + _avatarData.mSkills[i].mIcon);
				mCurAvatar.mSkills[i].mName = GuiSystem.GetLocaleText(_avatarData.mSkills[i].mName);
				mCurAvatar.mSkills[i].mDesc = GuiSystem.GetLocaleText(_avatarData.mSkills[i].mDesc);
			}
		}
		else
		{
			mCurAvatar.mName = string.Empty;
			mCurAvatar.mDesc = string.Empty;
			mCurAvatar.mIcon = null;
			foreach (Skill mSkill in mCurAvatar.mSkills)
			{
				mSkill.mName = string.Empty;
				mSkill.mDesc = string.Empty;
				mSkill.mIcon = null;
			}
		}
		InitAvatarParams(_avatarData);
	}

	private void SetAvatarButtonsState()
	{
		if ((mSelfPlayer != null && mSelfPlayer.GetStatus()) || !mIsPlayerSelecting)
		{
			return;
		}
		LockAvatarButtons(_lock: false);
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			foreach (Player item in mPlayer.Value)
			{
				if (item.mAvatarId != -1)
				{
					LockAvatarButton(item.mAvatarId, _lock: true);
				}
			}
		}
	}

	private void LockAvatarButton(int _id, bool _lock)
	{
		_lock = !GetAvatarUnlocked(_id) || _lock;
		foreach (KeyValuePair<AvatarType, List<GuiButton>> mAvatarButton in mAvatarButtons)
		{
			foreach (GuiButton item in mAvatarButton.Value)
			{
				if (item.mId == _id)
				{
					item.mLocked = _lock;
					return;
				}
			}
		}
	}

	private void LockAvatarButtons(bool _lock)
	{
		foreach (KeyValuePair<AvatarType, List<GuiButton>> mAvatarButton in mAvatarButtons)
		{
			foreach (GuiButton item in mAvatarButton.Value)
			{
				LockAvatarButton(item.mId, _lock);
			}
		}
	}

	private void Lock(bool _status)
	{
		LockAvatarButtons(_status);
		mReadyButton.mLocked = _status;
	}

	public void LockBackButton(bool _status)
	{
		mBackButton.mLocked = _status;
	}

	public void Lock()
	{
		Lock(_status: true);
		if (mSelfPlayer != null)
		{
			mSelfPlayer.SetStatus(_ready: true);
		}
	}

	public void SelectAvatar(int _avatarId)
	{
		mLastSelectedAvatar = _avatarId;
		if (mSelfPlayer == null)
		{
			Log.Error("No Self Player");
		}
		else if (!mSelfPlayer.GetStatus())
		{
			mReadyButton.mLocked = false;
			mSelfPlayer.mReadyButton.mLocked = false;
		}
	}

	public void SetAvailable(List<int> _usersId)
	{
		mAvailableUsers = _usersId;
	}

	private void SetAvailableHints(List<int> _usersId)
	{
		mSelectDrawRectsLeft.Clear();
		mSelectDrawRectsRight.Clear();
		float num = 16f * GuiSystem.mYRate;
		float num2 = 16f * GuiSystem.mYRate;
		float num3 = 22f * GuiSystem.mYRate;
		int userId;
		foreach (int item in _usersId)
		{
			userId = item;
			Player player = mPlayers[mTeam1Num].Find((Player p) => p.mId == userId);
			if (player != null)
			{
				mSelectDrawRectsLeft.Add(new Rect(player.mFrameRect.xMin - num, player.mFrameRect.yMin - num3, (float)mSelectImgLeft.width * GuiSystem.mYRate, (float)(mSelectImgLeft.height / mMaxFrameCount) * GuiSystem.mYRate));
			}
		}
		int userId2;
		foreach (int item2 in _usersId)
		{
			userId2 = item2;
			Player player2 = mPlayers[mTeam2Num].Find((Player p) => p.mId == userId2);
			if (player2 != null)
			{
				mSelectDrawRectsRight.Add(new Rect(player2.mFrameRect.xMin - num2, player2.mFrameRect.yMin - num3, (float)mSelectImgRight.width * GuiSystem.mYRate, (float)(mSelectImgRight.height / mMaxFrameCount) * GuiSystem.mYRate));
			}
		}
		if (_usersId.Contains(mSelfPlayerId))
		{
			mSelectText = GuiSystem.GetLocaleText("Castle_Select_Text");
			mIsPlayerSelecting = true;
			Lock(!mIsPlayerSelecting);
			mSelfPlayer.mReadyButton.mLocked = false;
			SetAvatarButtonsState();
		}
		else
		{
			mSelectText = GuiSystem.GetLocaleText("Castle_Another_Select_Text");
			mIsPlayerSelecting = false;
			Lock(!mIsPlayerSelecting);
		}
		mReadyButton.mLocked = true;
	}

	public bool IsPlayerDeserted()
	{
		if (IsClanWar())
		{
			return false;
		}
		int[] array = new int[2];
		int num = 0;
		foreach (Player item in mPlayers[mTeam1Num])
		{
			if (item.mId > 0)
			{
				array[0]++;
			}
			if (mSelfPlayerId == item.mId)
			{
				num = 0;
			}
		}
		foreach (Player item2 in mPlayers[mTeam2Num])
		{
			if (item2.mId > 0)
			{
				array[1]++;
			}
			if (mSelfPlayerId == item2.mId)
			{
				num = 1;
			}
		}
		int num2 = mMinPlayersCnt / 2;
		if (array[0] >= num2 && array[1] >= num2)
		{
			array[num]--;
			if (array[0] < num2 || array[1] < num2)
			{
				return true;
			}
		}
		return false;
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mBackground, mZoneRect);
		GuiSystem.DrawImage(mFrame1, mFrame1Rect);
		GuiSystem.DrawImage(mFrame2, mFrame2Rect);
		int num = mTimeToStart - mCurTimeToStart;
		int num2 = Mathf.FloorToInt(num / 60);
		int num3 = num - num2 * 60;
		if (!IsClanWar() && num > 5)
		{
			GuiSystem.DrawString(num2.ToString("0#") + ":" + num3.ToString("0#"), mTimeRect, "middle_center_11_bold");
		}
		else if (IsClanWar() && !mTimerRenderer.IsStarted && num > 0)
		{
			GuiSystem.DrawString(num2.ToString("0#") + ":" + num3.ToString("0#"), mTimeRect, "middle_center_11_bold");
		}
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			foreach (Player item in mPlayer.Value)
			{
				item.RenderElement();
			}
		}
		foreach (KeyValuePair<AvatarType, List<GuiButton>> mAvatarButton in mAvatarButtons)
		{
			foreach (GuiButton item2 in mAvatarButton.Value)
			{
				item2.RenderElement();
			}
		}
		if (null != mCurAvatar.mIcon)
		{
			GuiSystem.DrawImage(mCurAvatar.mIcon, mCurAvatar.mIconRect);
		}
		GUI.contentColor = mNormalBrown;
		GuiSystem.DrawString(mCurAvatar.mName, mCurAvatar.mNameRect, "label_bold");
		GuiSystem.DrawString(mCurAvatar.mDesc, mCurAvatar.mDescRect, "upper_left");
		foreach (Skill mSkill in mCurAvatar.mSkills)
		{
			if (null != mSkill.mIcon)
			{
				GuiSystem.DrawImage(mSkill.mIcon, mSkill.mIconRect);
			}
			GUI.contentColor = mHealthRed;
			GuiSystem.DrawString(mSkill.mName, mSkill.mNameRect, "middle_center");
			GUI.contentColor = mNormalBrown;
			GuiSystem.DrawString(mSkill.mDesc, mSkill.mDescRect, "upper_left_10");
		}
		if (mCurAvatar != null)
		{
			foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
			{
				GUI.contentColor = mAvaParamDatum.Value.mColor;
				GuiSystem.DrawString(mAvaParamDatum.Value.mValue, mAvaParamDatum.Value.mDrawRect, mAvaParamDatum.Value.mFormat);
				GUI.contentColor = mAvaParamDatum.Value.mAddColor;
				GuiSystem.DrawString(mAvaParamDatum.Value.mAdd, mAvaParamDatum.Value.mDrawRect, mAvaParamDatum.Value.mAddFormat);
			}
			GUI.contentColor = Color.white;
		}
		mSendBtn.RenderElement();
		if (mExitEnabled)
		{
			mBackButton.RenderElement();
		}
		mReadyButton.RenderElement();
		mChatZone.RenderElement();
		mRandomButton.RenderElement();
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.RenderElement();
		}
		if (mOkDialog.Active)
		{
			mOkDialog.RenderElement();
		}
		foreach (Rect item3 in mSelectDrawRectsLeft)
		{
			GuiSystem.DrawImage(mSelectImgLeft, item3, mSelectFrameRect);
		}
		foreach (Rect item4 in mSelectDrawRectsRight)
		{
			GuiSystem.DrawImage(mSelectImgRight, item4, mSelectFrameRect);
		}
		if (mSelectDrawRectsLeft.Count > 0 || mSelectDrawRectsRight.Count > 0)
		{
			GuiSystem.DrawString(mSelectText, mSelectTextRect, "label");
		}
		if (!IsClanWar())
		{
			GUI.contentColor = mNormalBrown;
			GuiSystem.DrawString(mHelpText, mSelectTextRect, "label");
		}
		mTimerRenderer.RenderElement();
		GUI.contentColor = mClassBlue;
		GuiSystem.DrawString(mWarriorText, mWarriorRect, "label_bold");
		GuiSystem.DrawString(mKillerText, mKillerRect, "label_bold");
		GuiSystem.DrawString(mMageText, mMageRect, "label_bold");
		GuiSystem.DrawString(mSupportText, mSupportRect, "label_bold");
		GUI.contentColor = mHealthRed;
		GuiSystem.DrawString(mHealthText, mHealthRect, "middle_center");
		GuiSystem.DrawString(mHealthRegenText, mHealthRegenRect, "upper_left");
		GuiSystem.DrawString(mFizArmorText, mFizArmorRect, "upper_left");
		GUI.contentColor = mAttackPurple;
		GuiSystem.DrawString(mAttackText, mAttackRect, "middle_center");
		GuiSystem.DrawString(mSpeedText, mSpeedRect, "upper_left");
		GUI.contentColor = mManaBlue;
		GuiSystem.DrawString(mManaText, mManaRect, "middle_center");
		GuiSystem.DrawString(mManaRegenText, mManaRegenRect, "upper_left");
		GuiSystem.DrawString(mMagArmorText, mMagArmorRect, "upper_left");
		GUI.contentColor = mNormalBrown;
		GuiSystem.DrawString(mSkillText, mSkillRect, "label_bold");
		GUI.contentColor = Color.white;
	}

	public override void OnInput()
	{
		mMsgTF.OnInput();
		mChatZone.OnInput();
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (KeyValuePair<int, List<Player>> mPlayer in mPlayers)
		{
			foreach (Player item in mPlayer.Value)
			{
				item.mReadyButton.CheckEvent(_curEvent);
			}
		}
		foreach (KeyValuePair<AvatarType, List<GuiButton>> mAvatarButton in mAvatarButtons)
		{
			foreach (GuiButton item2 in mAvatarButton.Value)
			{
				item2.CheckEvent(_curEvent);
			}
		}
		if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Escape && ExitEnabled && !mBackButton.mLocked)
		{
			UserLog.AddAction(UserActionType.EXIT_FROM_LOBBY, "Esc");
			DesertQuestion();
		}
		if (GUI.GetNameOfFocusedControl() == "TEXT_FIELD_MSG")
		{
			if (mUpdateFocus)
			{
				mUpdateFocus = false;
			}
			if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Return && mMsgTF.mData != string.Empty)
			{
				SendCurMessage();
			}
		}
		else if (mUpdateFocus)
		{
			GUI.FocusControl("TEXT_FIELD_MSG");
		}
		mSendBtn.CheckEvent(_curEvent);
		mChatZone.CheckEvent(_curEvent);
		mMsgTF.CheckEvent(_curEvent);
		if (mExitEnabled)
		{
			mBackButton.CheckEvent(_curEvent);
		}
		mReadyButton.CheckEvent(_curEvent);
		mRandomButton.CheckEvent(_curEvent);
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.CheckEvent(_curEvent);
		}
		if (mOkDialog.Active)
		{
			mOkDialog.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "BUTTON_SEND" && _buttonId == 0)
		{
			SendCurMessage();
		}
		else if (_sender.mElementId == "BACK_BUTTON" && _buttonId == 0)
		{
			DesertQuestion();
		}
		else if (_sender.mElementId == "READY_BUTTON" && _buttonId == 0)
		{
			ReadyButtonClicked();
		}
	}

	private void ReadyButtonClicked()
	{
		mReadyPressed = true;
		if (mReadyCallback != null)
		{
			mReadyCallback();
		}
	}

	private void DesertQuestion()
	{
		if (mTimeToStart > 0)
		{
			YesNoDialog.OnAnswer callback = delegate(bool _result)
			{
				if (_result && mDesertCallback != null)
				{
					mIsPlayerSelecting = true;
					mDesertCallback();
				}
			};
			string text = GuiSystem.GetLocaleText("GUI_ACCEPT_DESERT");
			if (IsPlayerDeserted())
			{
				text = string.Format(GuiSystem.GetLocaleText("GUI_ACCEPT_DESERT_WITH_BAN"), mBanTimer / 60);
			}
			mYesNoDialog.SetData(text, "Disconnect_Text", "Cancel_Button_Name", callback);
		}
		else
		{
			mIsPlayerSelecting = true;
			if (mDesertCallback != null)
			{
				mDesertCallback();
			}
		}
	}

	public void OnSelectButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && !mReadyPressed && mLastSelectedAvatar != _sender.mId)
		{
			mLastSelectedAvatar = _sender.mId;
			mSelectedAvatar = GetAvatarById(_sender.mId);
			InitAvatarInfo(mSelectedAvatar);
			if (mSelectedAvatar != null)
			{
				UserLog.AddAction(UserActionType.SELECT_AVATAR, _sender.mId, GuiSystem.GetLocaleText(mSelectedAvatar.mName));
			}
			if (mSelectAvatarCallback != null)
			{
				mSelectAvatarCallback(_sender.mId);
			}
		}
	}

	private void OnSelectLockedButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && mOnSelectLockedAvatar != null && !GetAvatarDenyForMap(_sender.mId) && !GetAvatarAvailable(_sender.mId))
		{
			mOnSelectLockedAvatar(GetAvatarById(_sender.mId), GetMapAvatarById(_sender.mId));
			mSelectedLockedAvatarId = _sender.mId;
		}
	}

	public void OnSelectButtonEnter(GuiElement _sender)
	{
		InitAvatarInfo(GetAvatarById(_sender.mId));
	}

	public void OnSelectButtonLeave(GuiElement _sender)
	{
		InitAvatarInfo(mSelectedAvatar);
	}

	private void SendCurMessage()
	{
		mMsgRecipients.Clear();
		if (!(mMsgTF.mData == string.Empty))
		{
			if (mMsgTF.mData.StartsWith("/"))
			{
				mChatMgr.ParseAndSendMessage(mMsgTF.mData);
			}
			else
			{
				string text = Chat.ExcludeRecepientsFromMessage(mMsgTF.mData, ref mMsgRecipients);
				mChatMgr.SendMessage(text, mCurMode, mMsgRecipients);
			}
			mMsgRecipients.Clear();
			mMsgTF.mData = string.Empty;
		}
	}

	private void OnMessage(IIncomingChatMsg _msg)
	{
		if (mChatZone != null)
		{
			mChatZone.OnMessage(_msg);
		}
	}

	private void OnNickClick(string _nick, int _buttonId, Vector2 _pos)
	{
		if (_buttonId == 0)
		{
			AddRecipientToMsg(_nick);
		}
	}

	private void AddRecipientToMsg(string _nick)
	{
		if (!mMsgRecipients.Contains(_nick))
		{
			mMsgRecipients.Add(_nick);
		}
		mMsgTF.mData = Chat.AddRecipientToMsg(mMsgTF.mData, _nick);
		mUpdateFocus = true;
	}

	public void BuyComplete(bool _success)
	{
		MapAvatarData mapAvatarById = GetMapAvatarById(mSelectedLockedAvatarId);
		mSelectedLockedAvatarId = -1;
		if (mapAvatarById != null)
		{
			mapAvatarById.mAvailable = _success;
		}
		SetAvatarButtonsState();
	}

	public void SetData(int _selfPlayerId, Chat _chat, Hero _selfHero, CtrlAvatarStore _avatarStore)
	{
		mSelfPlayerId = _selfPlayerId;
		mCtrlAvatarStore = _avatarStore;
		mChatMgr = _chat;
		Chat chat = mChatMgr;
		chat.mNewMessageCallback = (Chat.NewMessageCallback)Delegate.Combine(chat.mNewMessageCallback, new Chat.NewMessageCallback(OnMessage));
		mSelfHero = _selfHero;
		mReadyPressed = false;
	}

	public void Clear()
	{
		Uninit();
		if (mChatMgr != null)
		{
			Chat chat = mChatMgr;
			chat.mNewMessageCallback = (Chat.NewMessageCallback)Delegate.Remove(chat.mNewMessageCallback, new Chat.NewMessageCallback(OnMessage));
			mChatMgr = null;
		}
		if (mChatZone != null)
		{
			mChatZone.Uninit();
		}
		mCtrlAvatarStore = null;
		mSelfHero = null;
	}
}
