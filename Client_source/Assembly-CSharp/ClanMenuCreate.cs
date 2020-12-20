using System;
using System.Diagnostics;
using TanatKernel;
using UnityEngine;

public class ClanMenuCreate : GuiElement, EscapeListener
{
	private enum State
	{
		NONE,
		QUESTION,
		CREATING
	}

	public delegate void OnClanCreate(string _name, string _tag);

	private Texture2D mFrame;

	private int mCurClanCost;

	private bool mClanCostDiamonds;

	private string mLabel;

	private string mClanCost;

	private Rect mClanCostRect;

	private Rect mLabelRect;

	private GuiButton mCloseButton;

	private GuiButton mHowToBuyButton;

	private GuiButton mCreateClanButton;

	private MoneyRenderer mMoneyRenderer;

	private State mState;

	private string mQuestion;

	private Rect mQuestionRect;

	private StaticTextField mClanNameField;

	private StaticTextField mClanTagField;

	private string mClanLeadName;

	private Texture2D mClanLeadFrame;

	private Rect mClanLeadFrameRect;

	private string mClanLeadText;

	private string mClanNameText;

	private string mClanTagText;

	private Rect mClanLeadTextRect;

	private Rect mClanNameTextRect;

	private Rect mClanTagTextRect;

	private SelfHero mSelfHero;

	private OkDialog mOkDialog;

	public OnClanCreate mOnClanCreate;

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
		mClanCostDiamonds = false;
		mCurClanCost = 200000;
		GuiSystem.mGuiInputMgr.AddEscapeListener(510, this);
		mFrame = GuiSystem.GetImage("Gui/ClanMenu/frame1");
		mClanLeadFrame = GuiSystem.GetImage("Gui/ClanMenu/frame3");
		mQuestion = GuiSystem.GetLocaleText("Clan_Create_Question_Text");
		mClanCost = GuiSystem.GetLocaleText("Clan_Cost_Text");
		mClanLeadText = GuiSystem.GetLocaleText("Clan_Lead_Text");
		mClanNameText = GuiSystem.GetLocaleText("Clan_Name_Text");
		mClanTagText = GuiSystem.GetLocaleText("Clan_Tag_Text");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mCreateClanButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCreateClanButton.mElementId = "CREATE_CLAN_BUTTON";
		GuiButton guiButton2 = mCreateClanButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCreateClanButton.mLabel = GuiSystem.GetLocaleText("Clan_Create_Text");
		mCreateClanButton.Init();
		mHowToBuyButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mHowToBuyButton.mElementId = "HOW_TO_BUY_BUTTON";
		GuiButton guiButton3 = mHowToBuyButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mHowToBuyButton.mLabel = GuiSystem.GetLocaleText("Get_Diamonds_Text");
		mHowToBuyButton.Init();
		mClanNameField = new StaticTextField();
		mClanNameField.mElementId = "CLAN_NAME_FIELD";
		mClanNameField.mLength = 30;
		mClanNameField.mStyleId = "text_field_1";
		mClanTagField = new StaticTextField();
		mClanTagField.mElementId = "CLAN_TAG_FIELD";
		mClanTagField.mLength = 4;
		mClanTagField.mStyleId = "text_field_1";
		mMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, mClanCostDiamonds);
		mMoneyRenderer.SetMoney(mCurClanCost);
		SetState(State.QUESTION);
	}

	public override void Uninit()
	{
		mSelfHero = null;
		if (mClanNameField != null)
		{
			mClanNameField.Uninit();
		}
		if (mClanTagField != null)
		{
			mClanTagField.Uninit();
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mLabelRect = new Rect(36f, 11f, 350f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		mCloseButton.mZoneRect = new Rect(386f, 8f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mCreateClanButton.mZoneRect = new Rect(125f, 239f, 172f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCreateClanButton.mZoneRect);
		mHowToBuyButton.mZoneRect = new Rect(125f, 296f, 172f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mHowToBuyButton.mZoneRect);
		mQuestionRect = new Rect(12f, 58f, 400f, 102f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestionRect);
		mClanCostRect = new Rect(45f, 208f, 200f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanCostRect);
		mMoneyRenderer.SetSize(mZoneRect);
		mMoneyRenderer.SetOffset(new Vector2(249f, 208f) * GuiSystem.mYRate);
		mClanLeadTextRect = new Rect(25f, 43f, 373f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanLeadTextRect);
		mClanLeadFrameRect = new Rect(130f, 64f, 162f, 24f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanLeadFrameRect);
		mClanNameTextRect = new Rect(25f, 90f, 373f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanNameTextRect);
		mClanNameField.mZoneRect = new Rect(25f, 111f, 373f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanNameField.mZoneRect);
		mClanTagTextRect = new Rect(25f, 135f, 373f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanTagTextRect);
		mClanTagField.mZoneRect = new Rect(175f, 156f, 67f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanTagField.mZoneRect);
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mCreateClanButton.CheckEvent(_curEvent);
		mHowToBuyButton.CheckEvent(_curEvent);
		if (mState == State.CREATING)
		{
			mClanNameField.CheckEvent(_curEvent);
			mClanTagField.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public override void OnInput()
	{
		if (mState == State.CREATING)
		{
			mClanNameField.OnInput();
			mClanTagField.OnInput();
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		GuiSystem.DrawString(mClanCost, mClanCostRect, "middle_right");
		mMoneyRenderer.Render();
		mCloseButton.RenderElement();
		mCreateClanButton.RenderElement();
		mHowToBuyButton.RenderElement();
		if (mState == State.QUESTION)
		{
			GuiSystem.DrawString(mQuestion, mQuestionRect, "middle_center");
		}
		else if (mState == State.CREATING)
		{
			GuiSystem.DrawImage(mClanLeadFrame, mClanLeadFrameRect);
			GuiSystem.DrawString(mClanLeadName, mClanLeadFrameRect, "middle_center");
			GuiSystem.DrawString(mClanLeadText, mClanLeadTextRect, "middle_center");
			GuiSystem.DrawString(mClanNameText, mClanNameTextRect, "middle_center");
			GuiSystem.DrawString(mClanTagText, mClanTagTextRect, "middle_center");
		}
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void Open()
	{
		SetActive(_active: true);
		SetState(State.QUESTION);
		mClanNameField.mData = string.Empty;
		mClanTagField.mData = string.Empty;
	}

	public void SetData(SelfHero _hero, OkDialog _okDialog, string _playerName)
	{
		mSelfHero = _hero;
		mOkDialog = _okDialog;
		mClanLeadName = _playerName;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "CREATE_CLAN_BUTTON" && _buttonId == 0)
		{
			if (mState == State.QUESTION)
			{
				SetState(State.CREATING);
			}
			else if (mState == State.CREATING && CheckClanCreate() && mOnClanCreate != null)
			{
				mOnClanCreate(mClanNameField.mData, mClanTagField.mData);
			}
		}
		else if (_sender.mElementId == "HOW_TO_BUY_BUTTON" && _buttonId == 0)
		{
			Process.Start("http://www.tanatonline.ru/payment");
		}
	}

	private void SetState(State _state)
	{
		if (mState != _state)
		{
			mState = _state;
			switch (_state)
			{
			case State.QUESTION:
				mLabel = GuiSystem.GetLocaleText("Clan_Label_Text");
				break;
			case State.CREATING:
				mLabel = GuiSystem.GetLocaleText("Clan_Create_Label_Text");
				break;
			}
		}
	}

	private bool CheckClanCreate()
	{
		if (!CheckMoney())
		{
			return false;
		}
		if (!Clan.IsValidName(mClanNameField.mData))
		{
			if (mOkDialog != null)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("Clan_Name_Error_Text"));
			}
			return false;
		}
		if (!Clan.IsValidTag(mClanTagField.mData))
		{
			if (mOkDialog != null)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("Clan_Tag_Error_Text"));
			}
			return false;
		}
		return true;
	}

	private bool CheckMoney()
	{
		if (mSelfHero == null)
		{
			return false;
		}
		int num = ((!mClanCostDiamonds) ? mSelfHero.VirtualMoney : mSelfHero.RealMoney);
		if (num < mCurClanCost)
		{
			if (mOkDialog != null)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("Clan_Money_Error_Text"));
			}
			return false;
		}
		return true;
	}
}
