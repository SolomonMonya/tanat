using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class OptionsMenu : GuiElement, EscapeListener
{
	private class OptionsSet
	{
		public bool mFullScreen;

		public int mScreenWidth;

		public int mScreenHeight;

		public float mBaseVolume;

		public float mMusicVolume;

		public float mSoundVolume;

		public float mGuiVolume;

		public bool mShowAvatarHealthBar = true;

		public bool mShowOtherHealthBar = true;

		public bool mShowDamageEffect = true;

		public bool mPlayAvatarVoices = true;

		public bool mShowAttackers = true;

		public float mCamSpeed;

		public QualityLevel mQuality = QualityLevel.Good;

		public int mTutorialSet;
	}

	public delegate void OnClose();

	private Texture2D mImage1;

	private GuiButton[] mButtons;

	private int mCurMode;

	public OnClose mOnClose;

	private Rect mFullScreenTextRect;

	private Rect mResolutionTextRect;

	private Rect mQualityTextRect;

	private Rect mCommonTextRect;

	private Rect mMusicTextRect;

	private Rect mSoundTextRect;

	private Rect mGuiTextRect;

	private string mFullScreenText;

	private string mResolutionText;

	private string mQualityText;

	private string mCommonText;

	private string mMusicText;

	private string mSoundText;

	private string mGuiText;

	private string mShowAvatarBarText;

	private string mShowOtherBarText;

	private string mShowDamageBarText;

	private string mShowAttackerText;

	private string mPlayAvatarVoicesText;

	private Rect mShowAvatarBarTextRect;

	private Rect mShowOtherBarTextRect;

	private Rect mShowDamageBarTextRect;

	private Rect mShowAttackerTextRect;

	private Rect mPlayAvatarVoicesTextRect;

	private ComboBox mResolutionBox;

	private ComboBox mQualityBox;

	private HorizontalSlider mCommonSlider;

	private HorizontalSlider mMusicSlider;

	private HorizontalSlider mSoundSlider;

	private HorizontalSlider mGuiSlider;

	private HorizontalSlider mCamSpeedSlider;

	private CheckBox mFullScreenBox;

	private CheckBox mShowAvatarBox;

	private CheckBox mShowOtherBox;

	private CheckBox mShowDamageEffectBox;

	private CheckBox mPlayAvatarVoicesBox;

	private CheckBox mShowAttackerSelectionBox;

	private GuiButton mTutorialButton;

	private List<Vector2> mAvailableResolution;

	private OptionsSet mOptionsSet;

	private string mHintText1;

	private Rect mHintText1Rect;

	private string mHintText2;

	private Rect mHintText2Rect;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			CancelCurOptions();
			Close();
			if (mOnClose != null)
			{
				mOnClose();
			}
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(50, this);
		mOptionsSet = new OptionsSet();
		mImage1 = GuiSystem.GetImage("Gui/OptionsMenu/tab_01");
		mFullScreenText = GuiSystem.GetLocaleText("Full_Screen_Text");
		mResolutionText = GuiSystem.GetLocaleText("Resolution_Text");
		mQualityText = GuiSystem.GetLocaleText("Quality_Text");
		mCommonText = GuiSystem.GetLocaleText("Common_Text");
		mMusicText = GuiSystem.GetLocaleText("Music_Text");
		mSoundText = GuiSystem.GetLocaleText("Sound_Text");
		mGuiText = GuiSystem.GetLocaleText("Gui_Text");
		mShowAvatarBarText = GuiSystem.GetLocaleText("Show_Avatar_Bar_Text");
		mShowOtherBarText = GuiSystem.GetLocaleText("Show_Other_Bar_Text");
		mShowDamageBarText = GuiSystem.GetLocaleText("Show_Damage_Bar_Text");
		mShowAttackerText = GuiSystem.GetLocaleText("Show_Attacker_Text");
		mPlayAvatarVoicesText = GuiSystem.GetLocaleText("Play_Avatar_Voices_Text");
		mHintText1 = GuiSystem.GetLocaleText("Options_Hint_Text1");
		mHintText2 = GuiSystem.GetLocaleText("Options_Hint_Text2");
		Texture2D[] buttonTex = new Texture2D[3]
		{
			GuiSystem.GetImage("Gui/misc/str_01_1"),
			GuiSystem.GetImage("Gui/misc/str_01_2"),
			GuiSystem.GetImage("Gui/misc/str_01_3")
		};
		Texture2D image = GuiSystem.GetImage("Gui/misc/popup_frame1");
		Texture2D image2 = GuiSystem.GetImage("Gui/misc/el_02");
		mAvailableResolution = new List<Vector2>();
		List<string> list = new List<string>();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Vector2 zero = Vector2.zero;
			zero.x = Screen.resolutions[i].width;
			zero.y = Screen.resolutions[i].height;
			if (!(zero.x < 1024f))
			{
				list.Add(zero.x + "x" + zero.y);
				mAvailableResolution.Add(zero);
			}
		}
		mResolutionBox = new ComboBox(list, buttonTex, image, image2);
		ComboBox comboBox = mResolutionBox;
		comboBox.mOnMouseDown = (OnMouseDown)Delegate.Combine(comboBox.mOnMouseDown, new OnMouseDown(OnSelectResolution));
		mResolutionBox.mSelectionFrame = GuiSystem.GetImage("Gui/misc/selection");
		mResolutionBox.Init();
		list = new List<string>();
		list.Add(QualityLevel.Fastest.ToString());
		list.Add(QualityLevel.Fast.ToString());
		list.Add(QualityLevel.Simple.ToString());
		list.Add(QualityLevel.Good.ToString());
		list.Add(QualityLevel.Beautiful.ToString());
		list.Add(QualityLevel.Fantastic.ToString());
		mQualityBox = new ComboBox(list, buttonTex, image, image2);
		ComboBox comboBox2 = mQualityBox;
		comboBox2.mOnMouseDown = (OnMouseDown)Delegate.Combine(comboBox2.mOnMouseDown, new OnMouseDown(OnSelectQuality));
		mQualityBox.mSelectionFrame = GuiSystem.GetImage("Gui/misc/selection");
		mQualityBox.Init();
		Texture2D image3 = GuiSystem.GetImage("Gui/misc/el_01");
		mCommonSlider = new HorizontalSlider(image2, image3, image3, image3);
		mCommonSlider.SetParams(0f, 1f, OptionsMgr.mBaseVolume);
		mCommonSlider.mElementId = "BASE_VOLUME_SLIDER";
		mCommonSlider.Init();
		mMusicSlider = new HorizontalSlider(image2, image3, image3, image3);
		mMusicSlider.SetParams(0f, 1f, OptionsMgr.mMusicVolume);
		mMusicSlider.mElementId = "MUSIC_VOLUME_SLIDER";
		mMusicSlider.Init();
		mSoundSlider = new HorizontalSlider(image2, image3, image3, image3);
		mSoundSlider.SetParams(0f, 1f, OptionsMgr.mSoundVolume);
		mSoundSlider.mElementId = "SOUND_VOLUME_SLIDER";
		mSoundSlider.Init();
		mGuiSlider = new HorizontalSlider(image2, image3, image3, image3);
		mGuiSlider.SetParams(0f, 1f, OptionsMgr.mGuiVolume);
		mGuiSlider.mElementId = "GUI_VOLUME_SLIDER";
		mGuiSlider.Init();
		mCamSpeedSlider = new HorizontalSlider(image2, image3, image3, image3);
		mCamSpeedSlider.SetParams(OptionsMgr.mMinCamSpeed, OptionsMgr.mMaxCamSpeed, OptionsMgr.mCamSpeed);
		mCamSpeedSlider.mElementId = "CAM_SPEED_SLIDER";
		mCamSpeedSlider.Init();
		HorizontalSlider horizontalSlider = mCommonSlider;
		horizontalSlider.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnVolumeChange));
		HorizontalSlider horizontalSlider2 = mMusicSlider;
		horizontalSlider2.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider2.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnVolumeChange));
		HorizontalSlider horizontalSlider3 = mSoundSlider;
		horizontalSlider3.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider3.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnVolumeChange));
		HorizontalSlider horizontalSlider4 = mGuiSlider;
		horizontalSlider4.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider4.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnVolumeChange));
		HorizontalSlider horizontalSlider5 = mCamSpeedSlider;
		horizontalSlider5.mOnChangeVal = (HorizontalSlider.OnChangeVal)Delegate.Combine(horizontalSlider5.mOnChangeVal, new HorizontalSlider.OnChangeVal(OnCamSpeedChange));
		mFullScreenBox = new CheckBox();
		mFullScreenBox.mElementId = "FULLSCREEN_CHECKBOX";
		mFullScreenBox.SetData(OptionsMgr.mFullScreen, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox = mFullScreenBox;
		checkBox.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mFullScreenBox.Init();
		mShowAvatarBox = new CheckBox();
		mShowAvatarBox.mElementId = "SHOW_AVATAR_BAR_CHECKBOX";
		mShowAvatarBox.SetData(OptionsMgr.mShowAvatarHealthBar, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox2 = mShowAvatarBox;
		checkBox2.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox2.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mShowAvatarBox.Init();
		mShowOtherBox = new CheckBox();
		mShowOtherBox.mElementId = "SHOW_OTHER_BAR_CHECKBOX";
		mShowOtherBox.SetData(OptionsMgr.mShowOtherHealthBar, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox3 = mShowOtherBox;
		checkBox3.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox3.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mShowOtherBox.Init();
		mShowDamageEffectBox = new CheckBox();
		mShowDamageEffectBox.mElementId = "SHOW_DAMAGE_BAR_CHECKBOX";
		mShowDamageEffectBox.SetData(OptionsMgr.mShowDamageEffect, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox4 = mShowDamageEffectBox;
		checkBox4.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox4.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mShowDamageEffectBox.Init();
		mShowAttackerSelectionBox = new CheckBox();
		mShowAttackerSelectionBox.mElementId = "SHOW_ATTACK_SELECTION_CHECKBOX";
		mShowAttackerSelectionBox.SetData(OptionsMgr.mShowAttackers, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox5 = mShowAttackerSelectionBox;
		checkBox5.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox5.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mShowAttackerSelectionBox.Init();
		mPlayAvatarVoicesBox = new CheckBox();
		mPlayAvatarVoicesBox.mElementId = "PLAY_AVATAR_VOICES_CHECKBOX";
		mPlayAvatarVoicesBox.SetData(OptionsMgr.mPlayAvatarVoices, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox6 = mPlayAvatarVoicesBox;
		checkBox6.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox6.mOnChecked, new CheckBox.OnChecked(OnCheckbox));
		mPlayAvatarVoicesBox.Init();
		mTutorialButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mTutorialButton.mElementId = "TUTORIAL_BUTTON";
		GuiButton guiButton = mTutorialButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mTutorialButton.Init();
		AddTutorialElement(mTutorialButton.mElementId, mTutorialButton);
		InitButtons();
		SetCurMode(mCurMode);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mImage1.width, mImage1.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mFullScreenBox.mZoneRect = new Rect(106f, 175f, 21f, 23f);
		mFullScreenTextRect = new Rect(mFullScreenBox.mZoneRect.x + mFullScreenBox.mZoneRect.width + 5f, mFullScreenBox.mZoneRect.y - 1f, 120f, 20f);
		mResolutionTextRect = new Rect(102f, 52f, 150f, 21f);
		mQualityTextRect = new Rect(102f, 92f, 150f, 21f);
		mCommonTextRect = new Rect(95f, 58f, 150f, 20f);
		mMusicTextRect = new Rect(95f, 99f, 150f, 20f);
		mSoundTextRect = new Rect(95f, 133f, 150f, 20f);
		mGuiTextRect = new Rect(95f, 166f, 150f, 20f);
		mShowAvatarBox.mZoneRect = new Rect(106f, 50f, 21f, 23f);
		mShowAvatarBarTextRect = new Rect(mShowAvatarBox.mZoneRect.x + mShowAvatarBox.mZoneRect.width + 5f, mShowAvatarBox.mZoneRect.y - 1f, 220f, 20f);
		mShowOtherBox.mZoneRect = new Rect(106f, 80f, 21f, 23f);
		mShowOtherBarTextRect = new Rect(mShowOtherBox.mZoneRect.x + mShowOtherBox.mZoneRect.width + 5f, mShowOtherBox.mZoneRect.y - 1f, 220f, 20f);
		mShowDamageEffectBox.mZoneRect = new Rect(106f, 110f, 21f, 23f);
		mShowDamageBarTextRect = new Rect(mShowDamageEffectBox.mZoneRect.x + mShowDamageEffectBox.mZoneRect.width + 5f, mShowDamageEffectBox.mZoneRect.y - 1f, 220f, 20f);
		mShowAttackerSelectionBox.mZoneRect = new Rect(106f, 140f, 21f, 23f);
		mShowAttackerTextRect = new Rect(mShowAttackerSelectionBox.mZoneRect.x + mShowAttackerSelectionBox.mZoneRect.width + 5f, mShowAttackerSelectionBox.mZoneRect.y - 1f, 220f, 20f);
		mPlayAvatarVoicesBox.mZoneRect = new Rect(95f, 200f, 21f, 23f);
		mPlayAvatarVoicesTextRect = new Rect(mPlayAvatarVoicesBox.mZoneRect.x + mPlayAvatarVoicesBox.mZoneRect.width + 5f, mPlayAvatarVoicesBox.mZoneRect.y - 1f, 220f, 20f);
		mHintText1Rect = new Rect(106f, 205f, 300f, 21f);
		mHintText2Rect = new Rect(106f, 255f, 300f, 14f);
		mTutorialButton.mZoneRect = new Rect(155f, 310f, 180f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mTutorialButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mFullScreenBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mFullScreenTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mResolutionTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mQualityTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mCommonTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMusicTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSoundTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mGuiTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowAvatarBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowOtherBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowDamageEffectBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowAttackerSelectionBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayAvatarVoicesBox.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowAvatarBarTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowOtherBarTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowDamageBarTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mShowAttackerTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayAvatarVoicesTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mHintText1Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mHintText2Rect);
		Rect _rect = new Rect(192f, 55f, 206f, 22f);
		Rect _rect2 = new Rect(192f, 95f, 206f, 22f);
		Rect _rect3 = new Rect(175f, 64f, 206f, 13f);
		Rect _rect4 = new Rect(175f, 105f, 206f, 13f);
		Rect _rect5 = new Rect(175f, 139f, 206f, 13f);
		Rect _rect6 = new Rect(175f, 172f, 206f, 13f);
		Rect _rect7 = new Rect(106f, 280f, 206f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		GuiSystem.SetChildRect(mZoneRect, ref _rect2);
		GuiSystem.SetChildRect(mZoneRect, ref _rect3);
		GuiSystem.SetChildRect(mZoneRect, ref _rect4);
		GuiSystem.SetChildRect(mZoneRect, ref _rect5);
		GuiSystem.SetChildRect(mZoneRect, ref _rect6);
		GuiSystem.SetChildRect(mZoneRect, ref _rect7);
		mResolutionBox.mZoneRect = _rect;
		mQualityBox.mZoneRect = _rect2;
		mCommonSlider.mZoneRect = _rect3;
		mMusicSlider.mZoneRect = _rect4;
		mSoundSlider.mZoneRect = _rect5;
		mGuiSlider.mZoneRect = _rect6;
		mCamSpeedSlider.mZoneRect = _rect7;
		mResolutionBox.SetSize();
		mQualityBox.SetSize();
		mCommonSlider.SetSize();
		mMusicSlider.SetSize();
		mSoundSlider.SetSize();
		mGuiSlider.SetSize();
		mCamSpeedSlider.SetSize();
		mFullScreenBox.SetSize();
		mShowAttackerSelectionBox.SetSize();
		mShowOtherBox.SetSize();
		mShowDamageEffectBox.SetSize();
		mShowAvatarBox.SetSize();
		mPlayAvatarVoicesBox.SetSize();
		for (int i = 0; i < 5; i++)
		{
			mButtons[i].mZoneRect = new Rect(33f, 52 + i * 64, 44f, 43f);
		}
		mButtons[5].mZoneRect = new Rect(400f, 12f, 23f, 23f);
		mButtons[6].mZoneRect = new Rect(24f, 384f, 125f, 28f);
		mButtons[7].mZoneRect = new Rect(149f, 384f, 88f, 28f);
		mButtons[8].mZoneRect = new Rect(237f, 384f, 88f, 28f);
		mButtons[9].mZoneRect = new Rect(324f, 384f, 88f, 28f);
		GuiButton[] array = mButtons;
		foreach (GuiButton guiButton in array)
		{
			GuiSystem.SetChildRect(mZoneRect, ref guiButton.mZoneRect);
		}
	}

	public void Show()
	{
		mOptionsSet.mBaseVolume = OptionsMgr.mBaseVolume;
		mOptionsSet.mMusicVolume = OptionsMgr.mMusicVolume;
		mOptionsSet.mSoundVolume = OptionsMgr.mSoundVolume;
		mOptionsSet.mGuiVolume = OptionsMgr.mGuiVolume;
		mOptionsSet.mScreenWidth = OptionsMgr.mScreenWidth;
		mOptionsSet.mScreenHeight = OptionsMgr.mScreenHeight;
		mOptionsSet.mFullScreen = OptionsMgr.mFullScreen;
		mOptionsSet.mQuality = OptionsMgr.mQuality;
		mOptionsSet.mShowAvatarHealthBar = OptionsMgr.mShowAvatarHealthBar;
		mOptionsSet.mShowOtherHealthBar = OptionsMgr.mShowOtherHealthBar;
		mOptionsSet.mShowDamageEffect = OptionsMgr.mShowDamageEffect;
		mOptionsSet.mCamSpeed = OptionsMgr.mCamSpeed;
		mOptionsSet.mTutorialSet = OptionsMgr.TutorialSetNum;
		mOptionsSet.mPlayAvatarVoices = OptionsMgr.mPlayAvatarVoices;
		mOptionsSet.mShowAttackers = OptionsMgr.mShowAttackers;
		SetGuiData();
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void SetGuiData()
	{
		mQualityBox.SetSelectedData((int)mOptionsSet.mQuality);
		int num = 0;
		foreach (Vector2 item in mAvailableResolution)
		{
			if (mOptionsSet.mScreenWidth == (int)item.x && mOptionsSet.mScreenHeight == (int)item.y)
			{
				mResolutionBox.SetSelectedData(num);
				break;
			}
			num++;
		}
		mCommonSlider.SetCurValue(mOptionsSet.mBaseVolume);
		mMusicSlider.SetCurValue(mOptionsSet.mMusicVolume);
		mSoundSlider.SetCurValue(mOptionsSet.mSoundVolume);
		mGuiSlider.SetCurValue(mOptionsSet.mGuiVolume);
		mCamSpeedSlider.SetCurValue(mOptionsSet.mCamSpeed);
		mFullScreenBox.SetValue(mOptionsSet.mFullScreen);
		mShowAvatarBox.SetValue(mOptionsSet.mShowAvatarHealthBar);
		mShowOtherBox.SetValue(mOptionsSet.mShowOtherHealthBar);
		mShowDamageEffectBox.SetValue(mOptionsSet.mShowDamageEffect);
		mPlayAvatarVoicesBox.SetValue(mOptionsSet.mPlayAvatarVoices);
		mShowAttackerSelectionBox.SetValue(mOptionsSet.mShowAttackers);
		mTutorialButton.mLabel = GetTutorialLabel();
	}

	private void InitButtons()
	{
		mButtons = new GuiButton[10];
		for (int i = 0; i < 5; i++)
		{
			mButtons[i] = new GuiButton();
			mButtons[i].mElementId = "OPTIONS_MODE_BUTTON";
			mButtons[i].mId = i;
			mButtons[i].mNormImg = GuiSystem.GetImage("Gui/OptionsMenu/kn_0" + (i + 1) + "_01");
			mButtons[i].mOverImg = GuiSystem.GetImage("Gui/OptionsMenu/kn_0" + (i + 1) + "_02");
			mButtons[i].mPressImg = GuiSystem.GetImage("Gui/OptionsMenu/kn_0" + (i + 1) + "_03");
			GuiButton obj = mButtons[i];
			obj.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj.mOnMouseDown, new OnMouseDown(OnButton));
			mButtons[i].mLockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			mButtons[i].RegisterAction((UserActionType)(1001 + i));
			if (i > 2)
			{
				mButtons[i].mLocked = true;
			}
			AddTutorialElement(mButtons[i].mElementId + "_" + i, mButtons[i]);
		}
		mButtons[5] = new GuiButton();
		mButtons[5].mElementId = "CLOSE_BUTTON";
		mButtons[5].mNormImg = GuiSystem.GetImage("Gui/misc/button_10_norm");
		mButtons[5].mOverImg = GuiSystem.GetImage("Gui/misc/button_10_over");
		mButtons[5].mPressImg = GuiSystem.GetImage("Gui/misc/button_10_press");
		GuiButton obj2 = mButtons[5];
		obj2.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj2.mOnMouseDown, new OnMouseDown(OnButton));
		mButtons[5].RegisterAction(UserActionType.OPTIONS_CLOSE);
		AddTutorialElement(mButtons[5].mElementId, mButtons[5]);
		mButtons[6] = new GuiButton();
		mButtons[6].mElementId = "DEFAULT_BUTTON";
		mButtons[6].mLabel = GuiSystem.GetLocaleText("Default_Button_Name");
		mButtons[6].mNormImg = GuiSystem.GetImage("Gui/misc/button_1_norm");
		mButtons[6].mOverImg = GuiSystem.GetImage("Gui/misc/button_1_over");
		mButtons[6].mPressImg = GuiSystem.GetImage("Gui/misc/button_1_press");
		GuiButton obj3 = mButtons[6];
		obj3.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj3.mOnMouseDown, new OnMouseDown(OnButton));
		mButtons[6].RegisterAction(UserActionType.OPTIONS_DEFAULT);
		AddTutorialElement(mButtons[6].mElementId, mButtons[6]);
		mButtons[7] = new GuiButton();
		mButtons[7].mElementId = "OK_BUTTON";
		mButtons[7].mLabel = GuiSystem.GetLocaleText("Ok_Button_Name");
		mButtons[7].mNormImg = GuiSystem.GetImage("Gui/misc/button_1_norm");
		mButtons[7].mOverImg = GuiSystem.GetImage("Gui/misc/button_1_over");
		mButtons[7].mPressImg = GuiSystem.GetImage("Gui/misc/button_1_press");
		GuiButton obj4 = mButtons[7];
		obj4.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj4.mOnMouseDown, new OnMouseDown(OnButton));
		mButtons[7].RegisterAction(UserActionType.OPTIONS_OK);
		AddTutorialElement(mButtons[7].mElementId, mButtons[7]);
		mButtons[8] = new GuiButton();
		mButtons[8].mElementId = "CANCEL_BUTTON";
		mButtons[8].mLabel = GuiSystem.GetLocaleText("Cancel_Button_Name");
		mButtons[8].mNormImg = GuiSystem.GetImage("Gui/misc/button_1_norm");
		mButtons[8].mOverImg = GuiSystem.GetImage("Gui/misc/button_1_over");
		mButtons[8].mPressImg = GuiSystem.GetImage("Gui/misc/button_1_press");
		GuiButton obj5 = mButtons[8];
		obj5.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj5.mOnMouseDown, new OnMouseDown(OnButton));
		mButtons[8].RegisterAction(UserActionType.OPTIONS_CANCEL);
		AddTutorialElement(mButtons[8].mElementId, mButtons[8]);
		mButtons[9] = new GuiButton();
		mButtons[9].mElementId = "APPLY_BUTTON";
		mButtons[9].mLabel = GuiSystem.GetLocaleText("Apply_Button_Name");
		mButtons[9].mNormImg = GuiSystem.GetImage("Gui/misc/button_1_norm");
		mButtons[9].mOverImg = GuiSystem.GetImage("Gui/misc/button_1_over");
		mButtons[9].mPressImg = GuiSystem.GetImage("Gui/misc/button_1_press");
		GuiButton obj6 = mButtons[9];
		obj6.mOnMouseDown = (OnMouseDown)Delegate.Combine(obj6.mOnMouseDown, new OnMouseDown(OnButton));
		mButtons[9].RegisterAction(UserActionType.OPTIONS_APPLY);
		AddTutorialElement(mButtons[9].mElementId, mButtons[9]);
		GuiButton[] array = mButtons;
		foreach (GuiButton guiButton in array)
		{
			guiButton.Init();
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mImage1, mZoneRect);
		GuiButton[] array = mButtons;
		foreach (GuiButton guiButton in array)
		{
			guiButton.RenderElement();
		}
		switch (mCurMode)
		{
		case 0:
			RenderScreenOptions();
			break;
		case 1:
			RenderSoundOptions();
			break;
		case 2:
			RenderGameOptions();
			break;
		}
	}

	public override void Update()
	{
		if (mCurMode == 1)
		{
			mCommonSlider.Update();
			mMusicSlider.Update();
			mSoundSlider.Update();
			mGuiSlider.Update();
		}
		else if (mCurMode == 2)
		{
			mCamSpeedSlider.Update();
		}
	}

	private void RenderScreenOptions()
	{
		GuiSystem.DrawString(mFullScreenText, mFullScreenTextRect, "middle_left");
		GuiSystem.DrawString(mResolutionText, mResolutionTextRect, "middle_left");
		GuiSystem.DrawString(mQualityText, mQualityTextRect, "middle_left");
		mQualityBox.RenderElement();
		mResolutionBox.RenderElement();
		mFullScreenBox.RenderElement();
	}

	private void RenderSoundOptions()
	{
		mCommonSlider.RenderElement();
		mMusicSlider.RenderElement();
		mSoundSlider.RenderElement();
		mGuiSlider.RenderElement();
		mPlayAvatarVoicesBox.RenderElement();
		GuiSystem.DrawString(mCommonText, mCommonTextRect, "middle_left");
		GuiSystem.DrawString(mMusicText, mMusicTextRect, "middle_left");
		GuiSystem.DrawString(mSoundText, mSoundTextRect, "middle_left");
		GuiSystem.DrawString(mGuiText, mGuiTextRect, "middle_left");
		GuiSystem.DrawString(mPlayAvatarVoicesText, mPlayAvatarVoicesTextRect, "middle_left");
	}

	private void RenderGameOptions()
	{
		GuiSystem.DrawString(mShowAvatarBarText, mShowAvatarBarTextRect, "middle_left");
		GuiSystem.DrawString(mShowOtherBarText, mShowOtherBarTextRect, "middle_left");
		GuiSystem.DrawString(mShowDamageBarText, mShowDamageBarTextRect, "middle_left");
		GuiSystem.DrawString(mShowAttackerText, mShowAttackerTextRect, "middle_left");
		GuiSystem.DrawString(mHintText1, mHintText1Rect, "middle_left");
		GuiSystem.DrawString(mHintText2, mHintText2Rect, "middle_left");
		mCamSpeedSlider.RenderElement();
		mShowOtherBox.RenderElement();
		mShowDamageEffectBox.RenderElement();
		mShowAttackerSelectionBox.RenderElement();
		mShowAvatarBox.RenderElement();
		mTutorialButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		switch (mCurMode)
		{
		case 0:
			mResolutionBox.CheckEvent(_curEvent);
			mQualityBox.CheckEvent(_curEvent);
			mFullScreenBox.CheckEvent(_curEvent);
			break;
		case 1:
			mCommonSlider.CheckEvent(_curEvent);
			mMusicSlider.CheckEvent(_curEvent);
			mSoundSlider.CheckEvent(_curEvent);
			mGuiSlider.CheckEvent(_curEvent);
			mPlayAvatarVoicesBox.CheckEvent(_curEvent);
			break;
		case 2:
			mCamSpeedSlider.CheckEvent(_curEvent);
			mShowOtherBox.CheckEvent(_curEvent);
			mShowAvatarBox.CheckEvent(_curEvent);
			mShowDamageEffectBox.CheckEvent(_curEvent);
			mShowAttackerSelectionBox.CheckEvent(_curEvent);
			mTutorialButton.CheckEvent(_curEvent);
			break;
		}
		GuiButton[] array = mButtons;
		foreach (GuiButton guiButton in array)
		{
			guiButton.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	private void OnVolumeChange(GuiElement _sender, float _newVal)
	{
		if (_sender.mElementId == "BASE_VOLUME_SLIDER")
		{
			OptionsMgr.mBaseVolume = _newVal;
		}
		else if (_sender.mElementId == "MUSIC_VOLUME_SLIDER")
		{
			OptionsMgr.mMusicVolume = _newVal;
		}
		else if (_sender.mElementId == "SOUND_VOLUME_SLIDER")
		{
			OptionsMgr.mSoundVolume = _newVal;
		}
		else if (_sender.mElementId == "GUI_VOLUME_SLIDER")
		{
			OptionsMgr.mGuiVolume = _newVal;
		}
		OptionsMgr.SetCurOptions(OptionsMgr.OptionsType.VOLUME_OPTIONS);
	}

	private void OnCamSpeedChange(GuiElement _sender, float _newVal)
	{
		if (_sender.mElementId == "CAM_SPEED_SLIDER")
		{
			mOptionsSet.mCamSpeed = _newVal;
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ("OPTIONS_MODE_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			SetCurMode(_sender.mId);
		}
		else if ("CLOSE_BUTTON" == _sender.mElementId || "CANCEL_BUTTON" == _sender.mElementId)
		{
			CancelCurOptions();
			Close();
			if (mOnClose != null)
			{
				mOnClose();
			}
		}
		else if ("OK_BUTTON" == _sender.mElementId)
		{
			ApplyChanges();
			SetActive(_active: false);
			Close();
			if (mOnClose != null)
			{
				mOnClose();
			}
		}
		else if ("APPLY_BUTTON" == _sender.mElementId)
		{
			ApplyChanges();
		}
		else if ("DEFAULT_BUTTON" == _sender.mElementId)
		{
			SetDefaultOptions();
		}
		else if (_sender.mElementId == "TUTORIAL_BUTTON")
		{
			mOptionsSet.mTutorialSet = ((mOptionsSet.mTutorialSet == 0) ? (-1) : 0);
			if (mOptionsSet.mTutorialSet == 0)
			{
				UserLog.AddAction(UserActionType.TUTORIAL_ON);
			}
			mTutorialButton.mLabel = GetTutorialLabel();
		}
	}

	private void OnCheckbox(GuiElement _sender, bool _value)
	{
		if (_sender.mElementId == "FULLSCREEN_CHECKBOX")
		{
			mOptionsSet.mFullScreen = _value;
		}
		else if (_sender.mElementId == "SHOW_AVATAR_BAR_CHECKBOX")
		{
			mOptionsSet.mShowAvatarHealthBar = _value;
		}
		else if (_sender.mElementId == "SHOW_OTHER_BAR_CHECKBOX")
		{
			mOptionsSet.mShowOtherHealthBar = _value;
		}
		else if (_sender.mElementId == "PLAY_AVATAR_VOICES_CHECKBOX")
		{
			mOptionsSet.mPlayAvatarVoices = _value;
		}
		else if (_sender.mElementId == "SHOW_ATTACK_SELECTION_CHECKBOX")
		{
			mOptionsSet.mShowAttackers = _value;
		}
		else if (_sender.mElementId == "SHOW_DAMAGE_BAR_CHECKBOX")
		{
			mOptionsSet.mShowDamageEffect = _value;
		}
	}

	private void OnSelectResolution(GuiElement _sender, int _button)
	{
		if (_sender.mId < mAvailableResolution.Count && _sender.mId >= 0 && _button == 0)
		{
			mOptionsSet.mScreenWidth = (int)mAvailableResolution[_sender.mId].x;
			mOptionsSet.mScreenHeight = (int)mAvailableResolution[_sender.mId].y;
			UserLog.AddAction(UserActionType.OPTIONS_CHANGE_RESOLUTION, mOptionsSet.mScreenWidth + "x" + mOptionsSet.mScreenHeight);
		}
	}

	private void OnSelectQuality(GuiElement _sender, int _button)
	{
		if (_sender.mId <= 5 && _sender.mId >= 0 && _button == 0)
		{
			UserLog.AddAction(UserActionType.OPTIONS_CHANGE_QUALITY, _sender.mId, ((QualityLevel)_sender.mId).ToString());
			mOptionsSet.mQuality = (QualityLevel)_sender.mId;
		}
	}

	private void SetCurMode(int _modeNum)
	{
		mCurMode = _modeNum;
		for (int i = 0; i < 5; i++)
		{
			if (mButtons[i].mId == _modeNum)
			{
				mButtons[i].Pressed = true;
			}
			else
			{
				mButtons[i].Pressed = false;
			}
			mButtons[i].SetCurBtnImage();
		}
	}

	private void ApplyChanges()
	{
		OptionsMgr.mFullScreen = mOptionsSet.mFullScreen;
		OptionsMgr.mScreenWidth = mOptionsSet.mScreenWidth;
		OptionsMgr.mScreenHeight = mOptionsSet.mScreenHeight;
		OptionsMgr.mQuality = mOptionsSet.mQuality;
		mOptionsSet.mBaseVolume = OptionsMgr.mBaseVolume;
		mOptionsSet.mMusicVolume = OptionsMgr.mMusicVolume;
		mOptionsSet.mSoundVolume = OptionsMgr.mSoundVolume;
		mOptionsSet.mGuiVolume = OptionsMgr.mGuiVolume;
		OptionsMgr.mShowAvatarHealthBar = mOptionsSet.mShowAvatarHealthBar;
		OptionsMgr.mShowOtherHealthBar = mOptionsSet.mShowOtherHealthBar;
		OptionsMgr.mShowDamageEffect = mOptionsSet.mShowDamageEffect;
		OptionsMgr.mCamSpeed = mOptionsSet.mCamSpeed;
		OptionsMgr.mPlayAvatarVoices = mOptionsSet.mPlayAvatarVoices;
		OptionsMgr.mShowAttackers = mOptionsSet.mShowAttackers;
		Log.Info(() => "ApplyChanges to : " + OptionsMgr.mScreenWidth + "x" + OptionsMgr.mScreenHeight + " from : " + Screen.width + "x" + Screen.height);
		if (OptionsMgr.mScreenWidth != Screen.width || OptionsMgr.mScreenHeight != Screen.height)
		{
			GuiSystem.mGuiSystem.ReinitGui();
		}
		OptionsMgr.TutorialSetNum = mOptionsSet.mTutorialSet;
		OptionsMgr.SetCurOptions(OptionsMgr.OptionsType.ALL_OPTIONS);
		OptionsMgr.SaveOptions();
	}

	private void CancelCurOptions()
	{
		if (mCurMode == 0)
		{
			mOptionsSet.mFullScreen = OptionsMgr.mFullScreen;
			mOptionsSet.mScreenWidth = OptionsMgr.mScreenWidth;
			mOptionsSet.mScreenHeight = OptionsMgr.mScreenHeight;
			mOptionsSet.mQuality = OptionsMgr.mQuality;
		}
		else if (mCurMode == 1)
		{
			OptionsMgr.mBaseVolume = mOptionsSet.mBaseVolume;
			OptionsMgr.mMusicVolume = mOptionsSet.mMusicVolume;
			OptionsMgr.mSoundVolume = mOptionsSet.mSoundVolume;
			OptionsMgr.mGuiVolume = mOptionsSet.mGuiVolume;
			mOptionsSet.mPlayAvatarVoices = OptionsMgr.mPlayAvatarVoices;
			OptionsMgr.SetCurOptions(OptionsMgr.OptionsType.VOLUME_OPTIONS);
		}
		else if (mCurMode == 2)
		{
			mOptionsSet.mShowAvatarHealthBar = OptionsMgr.mShowAvatarHealthBar;
			mOptionsSet.mShowOtherHealthBar = OptionsMgr.mShowOtherHealthBar;
			mOptionsSet.mShowDamageEffect = OptionsMgr.mShowDamageEffect;
			mOptionsSet.mShowAttackers = OptionsMgr.mShowAttackers;
			mOptionsSet.mCamSpeed = OptionsMgr.mCamSpeed;
			mOptionsSet.mTutorialSet = OptionsMgr.TutorialSetNum;
		}
		SetGuiData();
	}

	private void SetDefaultOptions()
	{
		if (mCurMode == 0)
		{
			int num = mAvailableResolution.Count - 1;
			mOptionsSet.mFullScreen = (OptionsMgr.mFullScreen = true);
			if (mAvailableResolution.Count > num && num >= 0)
			{
				mOptionsSet.mScreenWidth = (OptionsMgr.mScreenWidth = (int)mAvailableResolution[num].x);
				mOptionsSet.mScreenHeight = (OptionsMgr.mScreenHeight = (int)mAvailableResolution[num].y);
			}
			mOptionsSet.mQuality = (OptionsMgr.mQuality = QualityLevel.Good);
		}
		else if (mCurMode == 1)
		{
			OptionsMgr.mBaseVolume = (OptionsMgr.mMusicVolume = (OptionsMgr.mSoundVolume = (OptionsMgr.mGuiVolume = 1f)));
			mOptionsSet.mBaseVolume = (mOptionsSet.mMusicVolume = (mOptionsSet.mSoundVolume = (mOptionsSet.mGuiVolume = 1f)));
			mOptionsSet.mPlayAvatarVoices = (OptionsMgr.mPlayAvatarVoices = true);
			OptionsMgr.SetCurOptions(OptionsMgr.OptionsType.VOLUME_OPTIONS);
		}
		else if (mCurMode == 2)
		{
			mOptionsSet.mShowAvatarHealthBar = (OptionsMgr.mShowAvatarHealthBar = true);
			mOptionsSet.mShowOtherHealthBar = (OptionsMgr.mShowOtherHealthBar = true);
			mOptionsSet.mShowDamageEffect = (OptionsMgr.mShowDamageEffect = true);
			mOptionsSet.mShowAttackers = (OptionsMgr.mShowAttackers = true);
			mOptionsSet.mCamSpeed = (OptionsMgr.mCamSpeed = OptionsMgr.mDefaultCamSpeed);
			int num3 = (mOptionsSet.mTutorialSet = (OptionsMgr.TutorialSetNum = 0));
		}
		SetGuiData();
	}

	private string GetTutorialLabel()
	{
		if (mOptionsSet.mTutorialSet >= 0)
		{
			return GuiSystem.GetLocaleText("Tutorial_State2_Text");
		}
		if (mOptionsSet.mTutorialSet == -1)
		{
			return GuiSystem.GetLocaleText("Tutorial_State1_Text");
		}
		return string.Empty;
	}
}
