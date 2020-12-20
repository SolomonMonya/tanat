using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

internal class CustomizeMenu : GuiElement
{
	private class HeroMenu : GuiElement
	{
		private class CustomizePlate
		{
			public int mType;

			public string mName = string.Empty;

			public Rect mNameRect;

			public GuiButton mLeftButton;

			public GuiButton mRightButton;

			public void Init()
			{
				mLeftButton = new GuiButton();
				mLeftButton.mElementId = "LEFT_BUTTON";
				mLeftButton.mId = mType;
				mLeftButton.mNormImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_3_norm");
				mLeftButton.mOverImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_3_over");
				mLeftButton.mPressImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_3_press");
				mLeftButton.Init();
				mRightButton = new GuiButton();
				mRightButton.mElementId = "RIGHT_BUTTON";
				mRightButton.mId = mType;
				mRightButton.mNormImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_4_norm");
				mRightButton.mOverImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_4_over");
				mRightButton.mPressImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_4_press");
				mRightButton.Init();
			}

			public void SetSize(int _cnt)
			{
				mLeftButton.mZoneRect = new Rect(207f, 72 + _cnt * 43, 29f, 28f);
				mRightButton.mZoneRect = new Rect(250f, 72 + _cnt * 43, 29f, 28f);
				mNameRect = new Rect(35f, 70 + _cnt * 43, 158f, 28f);
			}
		}

		public delegate void ChangeHero(bool _genderChanged);

		private Texture2D mFrame;

		private List<GuiButton> mGenderButtons;

		private List<CustomizePlate> mCustomizePlates;

		private GuiButton mRandomButton;

		private string mUserName = string.Empty;

		private Rect mUserNameRect = default(Rect);

		private HeroView mPersParams;

		public HeroView mAvailablePersParams;

		public ChangeHero mOnChangeHero;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/CustomizeMenu/frame1");
			mGenderButtons = new List<GuiButton>();
			mCustomizePlates = new List<CustomizePlate>();
			mRandomButton = GuiSystem.CreateButton("Gui/CustomizeMenu/button_6_norm", "Gui/CustomizeMenu/button_6_over", "Gui/CustomizeMenu/button_6_press", string.Empty, string.Empty);
			mRandomButton.mElementId = "RANDOM_BUTTON";
			GuiButton guiButton = mRandomButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mRandomButton.mLabel = GuiSystem.GetLocaleText("Random_Button_Name");
			mRandomButton.Init();
			for (int i = 0; i <= 4; i++)
			{
				if (i != 1)
				{
					CustomizePlate customizePlate = new CustomizePlate();
					customizePlate.mType = i;
					customizePlate.mName = GuiSystem.GetLocaleText("Customize_Text" + i);
					customizePlate.Init();
					GuiButton mLeftButton = customizePlate.mLeftButton;
					mLeftButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mLeftButton.mOnMouseUp, new OnMouseUp(OnButton));
					GuiButton mRightButton = customizePlate.mRightButton;
					mRightButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mRightButton.mOnMouseUp, new OnMouseUp(OnButton));
					mCustomizePlates.Add(customizePlate);
				}
			}
			CreateGenderButtons();
		}

		public override void SetSize()
		{
			mUserNameRect = new Rect(32f, 17f, 246f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mUserNameRect);
			mGenderButtons[0].mZoneRect = new Rect(37f, 255f, 42f, 42f);
			GuiSystem.SetChildRect(mZoneRect, ref mGenderButtons[0].mZoneRect);
			mGenderButtons[1].mZoneRect = new Rect(94f, 255f, 42f, 42f);
			GuiSystem.SetChildRect(mZoneRect, ref mGenderButtons[1].mZoneRect);
			mRandomButton.mZoneRect = new Rect(176f, 262f, 100f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mRandomButton.mZoneRect);
			SetPlatesSize();
		}

		public void SetData(string _name, int _race, HeroView _availableParams)
		{
			mUserName = _name;
			mAvailablePersParams = _availableParams;
			mPersParams = new HeroView();
			mPersParams.mRace = _race;
			mPersParams.mGender = _availableParams.mGender;
			SetGenderButtonsState();
			SetParamsButtonState();
		}

		private void SetPlatesSize()
		{
			int num = 0;
			foreach (CustomizePlate mCustomizePlate in mCustomizePlates)
			{
				mCustomizePlate.SetSize(num);
				GuiSystem.SetChildRect(mZoneRect, ref mCustomizePlate.mNameRect);
				GuiSystem.SetChildRect(mZoneRect, ref mCustomizePlate.mLeftButton.mZoneRect);
				GuiSystem.SetChildRect(mZoneRect, ref mCustomizePlate.mRightButton.mZoneRect);
				num++;
			}
		}

		public HeroView GetHeroParams()
		{
			return mPersParams;
		}

		private void CreateGenderButtons()
		{
			mGenderButtons.Clear();
			GuiButton guiButton = new GuiButton();
			guiButton.mElementId = "GENDER_BUTTON";
			guiButton.mId = 1;
			guiButton.mNormImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_1_norm");
			guiButton.mOverImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_1_over");
			guiButton.mPressImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_1_press");
			GuiButton guiButton2 = guiButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			mGenderButtons.Add(guiButton);
			guiButton = new GuiButton();
			guiButton.mElementId = "GENDER_BUTTON";
			guiButton.mId = 0;
			guiButton.mNormImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_2_norm");
			guiButton.mOverImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_2_over");
			guiButton.mPressImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_2_press");
			GuiButton guiButton3 = guiButton;
			guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			mGenderButtons.Add(guiButton);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			foreach (GuiButton mGenderButton in mGenderButtons)
			{
				mGenderButton.RenderElement();
			}
			foreach (CustomizePlate mCustomizePlate in mCustomizePlates)
			{
				GuiSystem.DrawString(mCustomizePlate.mName, mCustomizePlate.mNameRect, "middle_center");
				mCustomizePlate.mLeftButton.RenderElement();
				mCustomizePlate.mRightButton.RenderElement();
			}
			GuiSystem.DrawString(mUserName, mUserNameRect, "label");
			mRandomButton.RenderElement();
		}

		public override void CheckEvent(Event _curEvent)
		{
			foreach (GuiButton mGenderButton in mGenderButtons)
			{
				mGenderButton.CheckEvent(_curEvent);
			}
			foreach (CustomizePlate mCustomizePlate in mCustomizePlates)
			{
				mCustomizePlate.mLeftButton.CheckEvent(_curEvent);
				mCustomizePlate.mRightButton.CheckEvent(_curEvent);
			}
			mRandomButton.CheckEvent(_curEvent);
		}

		private void SetGenderButtonsState()
		{
			foreach (GuiButton mGenderButton in mGenderButtons)
			{
				if (mGenderButton.mId == 1 == mPersParams.mGender)
				{
					mGenderButton.mLocked = true;
				}
				else
				{
					mGenderButton.mLocked = false;
				}
			}
		}

		private void SetParamButtonsState(CustomizePlate _plate)
		{
			if (_plate != null)
			{
				CustomType mType = (CustomType)_plate.mType;
				int paramValue = HeroMgr.GetParamValue(mAvailablePersParams, mType);
				int paramValue2 = HeroMgr.GetParamValue(mPersParams, mType);
				if (paramValue == -1 || paramValue2 == -1)
				{
					Log.Error("Bad customize plate parameter");
					return;
				}
				_plate.mLeftButton.mLocked = 0 == paramValue2;
				_plate.mRightButton.mLocked = paramValue - 1 == paramValue2;
			}
		}

		private CustomizePlate GetPlateByType(CustomType _type)
		{
			foreach (CustomizePlate mCustomizePlate in mCustomizePlates)
			{
				if (_type == (CustomType)mCustomizePlate.mType)
				{
					return mCustomizePlate;
				}
			}
			return null;
		}

		private void SetParamValue(CustomType _type, int _value)
		{
			if (_value < 0)
			{
				_value = 0;
			}
			int paramValue = HeroMgr.GetParamValue(mAvailablePersParams, _type);
			if (_value >= paramValue)
			{
				_value = paramValue - 1;
			}
			switch (_type)
			{
			case CustomType.FACE:
				mPersParams.mFace = _value;
				break;
			case CustomType.HAIR:
				mPersParams.mHair = _value;
				break;
			case CustomType.SPECIALITY:
				mPersParams.mDistMark = _value;
				break;
			case CustomType.HAIR_COLOR:
				mPersParams.mHairColor = _value;
				break;
			case CustomType.SKIN_COLOR:
				mPersParams.mSkinColor = _value;
				break;
			default:
				Log.Error("bad param value");
				break;
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "GENDER_BUTTON" && _buttonId == 0)
			{
				mPersParams.mGender = _sender.mId == 1;
				SetGenderButtonsState();
				if (mOnChangeHero != null)
				{
					mOnChangeHero(_genderChanged: true);
				}
			}
			else if (_sender.mElementId == "LEFT_BUTTON" && _buttonId == 0)
			{
				CustomType mId = (CustomType)_sender.mId;
				ChangeValue(mId, -1);
				if (mOnChangeHero != null)
				{
					mOnChangeHero(_genderChanged: false);
				}
			}
			else if (_sender.mElementId == "RIGHT_BUTTON" && _buttonId == 0)
			{
				CustomType mId2 = (CustomType)_sender.mId;
				ChangeValue(mId2, 1);
				if (mOnChangeHero != null)
				{
					mOnChangeHero(_genderChanged: false);
				}
			}
			else if (_sender.mElementId == "RANDOM_BUTTON" && _buttonId == 0)
			{
				RandomizeHero();
			}
		}

		private void ChangeValue(CustomType _type, int change)
		{
			int paramValue = HeroMgr.GetParamValue(mPersParams, _type);
			paramValue += change;
			SetParamValue(_type, paramValue);
			SetParamButtonsState(GetPlateByType(_type));
		}

		private void RandomizeHero()
		{
			int value = UnityEngine.Random.Range(0, mAvailablePersParams.mFace);
			int value2 = UnityEngine.Random.Range(0, mAvailablePersParams.mHair);
			int value3 = UnityEngine.Random.Range(0, mAvailablePersParams.mHairColor);
			int value4 = UnityEngine.Random.Range(0, mAvailablePersParams.mSkinColor);
			SetParamValue(CustomType.FACE, value);
			SetParamValue(CustomType.HAIR, value2);
			SetParamValue(CustomType.HAIR_COLOR, value3);
			SetParamValue(CustomType.SKIN_COLOR, value4);
			SetParamsButtonState();
			if (mOnChangeHero != null)
			{
				mOnChangeHero(_genderChanged: false);
			}
		}

		private void SetParamsButtonState()
		{
			foreach (CustomizePlate mCustomizePlate in mCustomizePlates)
			{
				SetParamButtonsState(mCustomizePlate);
			}
		}
	}

	public delegate void PlayCallback(HeroView _params);

	public delegate void BackCallback();

	public delegate void MenuCallback();

	public PlayCallback mPlayCallback;

	public BackCallback mBackCallback;

	public MenuCallback mMenuCallback;

	private GuiButton mPlayButton;

	private GuiButton mBackButton;

	private GuiButton mMenuButton;

	private Texture2D mEmblemImage;

	private Rect mEmblemRect = default(Rect);

	private GameObject mHero;

	private HeroMenu mHeroMenu;

	private bool mDragStarted;

	private Vector2 mDragPos = Vector2.zero;

	private HeroMgr mHeroMgr;

	private string mUserName;

	public override void Init()
	{
		mHeroMenu = new HeroMenu();
		HeroMenu heroMenu = mHeroMenu;
		heroMenu.mOnChangeHero = (HeroMenu.ChangeHero)Delegate.Combine(heroMenu.mOnChangeHero, new HeroMenu.ChangeHero(SetHero));
		mHeroMenu.Init();
		CreateButtons();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(56f, 330f, 311f, 447f);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mHeroMenu.mZoneRect = mZoneRect;
		mHeroMenu.SetSize();
		mPlayButton.mZoneRect = new Rect(47f, 318f, 217f, 93f);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayButton.mZoneRect);
		mBackButton.mZoneRect = new Rect(-7f, 450f, 162f, 54f);
		GuiSystem.SetChildRect(mZoneRect, ref mBackButton.mZoneRect);
		mMenuButton.mZoneRect = new Rect(155f, 450f, 162f, 54f);
		GuiSystem.SetChildRect(mZoneRect, ref mMenuButton.mZoneRect);
		if (mEmblemImage != null)
		{
			mEmblemRect = new Rect(0f, 0f, mEmblemImage.width, mEmblemImage.height);
			GuiSystem.GetRectScaled(ref mEmblemRect);
			mEmblemRect.x = mZoneRect.x + (mZoneRect.width - mEmblemRect.width) / 2f;
		}
	}

	private void CreateButtons()
	{
		mBackButton = GuiSystem.CreateButton("Gui/misc/button_12_norm", "Gui/misc/button_12_over", "Gui/misc/button_12_press", string.Empty, string.Empty);
		mBackButton.mElementId = "BACK_BUTTON";
		GuiButton guiButton = mBackButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mBackButton.mLabel = GuiSystem.GetLocaleText("Back_Button_Name");
		mBackButton.Init();
		mMenuButton = GuiSystem.CreateButton("Gui/misc/button_12_norm", "Gui/misc/button_12_over", "Gui/misc/button_12_press", string.Empty, string.Empty);
		mMenuButton.mElementId = "MENU_BUTTON";
		GuiButton guiButton2 = mMenuButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mMenuButton.mLabel = GuiSystem.GetLocaleText("Menu_Button_Name");
		mMenuButton.Init();
		mPlayButton = new GuiButton();
		mPlayButton.mElementId = "PLAY_BUTTON";
		mPlayButton.mNormImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_5_norm");
		mPlayButton.mOverImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_5_over");
		mPlayButton.mPressImg = GuiSystem.GetImage("Gui/CustomizeMenu/button_5_press");
		GuiButton guiButton3 = mPlayButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mPlayButton.Init();
	}

	public void SetHero(HeroRace _raceId, HeroView _availableParams)
	{
		mEmblemImage = GuiSystem.GetImage("Gui/CustomizeMenu/logo" + (int)_raceId);
		mHeroMenu.SetData(mUserName, (int)_raceId, _availableParams);
		mEmblemRect = new Rect(0f, 0f, mEmblemImage.width, mEmblemImage.height);
		GuiSystem.GetRectScaled(ref mEmblemRect);
		mEmblemRect.x = mZoneRect.x + (mZoneRect.width - mEmblemRect.width) / 2f;
		InitHero(_raceId, _gender: true);
	}

	private void InitHero(HeroRace _raceId, bool _gender)
	{
		if (null != mHero)
		{
			mHero.transform.parent = null;
			UnityEngine.Object.Destroy(mHero);
			mHero = null;
		}
		HeroMgr.CreateHeroData createHeroData = new HeroMgr.CreateHeroData();
		createHeroData.mOnHeroLoaded = (HeroMgr.OnHeroLoaded)Delegate.Combine(createHeroData.mOnHeroLoaded, new HeroMgr.OnHeroLoaded(OnHeroLoaded));
		mHeroMgr.CreateHero(_raceId, _gender, createHeroData);
	}

	private void OnHeroLoaded(GameObject _hero, object _data)
	{
		mHero = _hero;
		Vector3 position = Vector3.zero;
		Vector3 eulerAngles = Vector3.zero;
		if (GetHeroParams().mRace == 1)
		{
			position = new Vector3(-147f, -0.1f, -34.4f);
			eulerAngles = new Vector3(0f, 6f, 0f);
		}
		else if (GetHeroParams().mRace == 2)
		{
			position = new Vector3(-46.4f, -0.1f, -34.4f);
			eulerAngles = new Vector3(0f, 340f, 0f);
		}
		GameObject gameObject = GameObjUtil.GetGameObject("/heroes");
		if (null != gameObject)
		{
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.eulerAngles = Vector3.zero;
			gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			mHero.transform.parent = gameObject.transform;
			gameObject.transform.position = position;
			gameObject.transform.eulerAngles = eulerAngles;
		}
		SetHero(_genderChanged: false);
	}

	private void SetHero(bool _genderChanged)
	{
		if (!(null == mHero))
		{
			HeroWear component = mHero.GetComponent<HeroWear>();
			Animation component2 = mHero.GetComponent<Animation>();
			if (_genderChanged)
			{
				InitHero((HeroRace)GetHeroParams().mRace, GetHeroParams().mGender);
			}
			component.SetDefault((HeroRace)GetHeroParams().mRace, GetHeroParams().mGender);
			component.SetHero(GetHeroParams());
			component2.enabled = true;
			component2.Play();
		}
	}

	public HeroView GetHeroParams()
	{
		return mHeroMenu.GetHeroParams();
	}

	public override void RenderElement()
	{
		mHeroMenu.RenderElement();
		GuiSystem.DrawImage(mEmblemImage, mEmblemRect);
		mPlayButton.RenderElement();
		mBackButton.RenderElement();
		mMenuButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mPlayButton.CheckEvent(_curEvent);
		mBackButton.CheckEvent(_curEvent);
		mMenuButton.CheckEvent(_curEvent);
		mHeroMenu.CheckEvent(_curEvent);
		if (mHero != null)
		{
			if (_curEvent.type == EventType.MouseDown)
			{
				mDragPos = _curEvent.mousePosition;
				mDragStarted = true;
			}
			else if (_curEvent.type == EventType.MouseUp && mDragStarted)
			{
				mDragPos = Vector2.zero;
				mDragStarted = false;
			}
			else if (_curEvent.type == EventType.MouseDrag && mDragStarted)
			{
				float x = (mDragPos - _curEvent.mousePosition).x;
				mHero.transform.parent.transform.Rotate(0f, x, 0f);
				mDragPos = _curEvent.mousePosition;
			}
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "PLAY_BUTTON" && _buttonId == 0)
		{
			if (mPlayCallback != null)
			{
				mPlayCallback(mHeroMenu.GetHeroParams());
			}
		}
		else if (_sender.mElementId == "BACK_BUTTON" && _buttonId == 0)
		{
			if (mBackCallback != null)
			{
				mBackCallback();
			}
		}
		else if (_sender.mElementId == "MENU_BUTTON" && _buttonId == 0 && mMenuCallback != null)
		{
			mMenuCallback();
		}
	}

	public void SetData(HeroMgr _heroMgr, string _userName)
	{
		mHeroMgr = _heroMgr;
		mUserName = _userName;
	}

	public void Clear()
	{
		mHeroMgr = null;
	}
}
