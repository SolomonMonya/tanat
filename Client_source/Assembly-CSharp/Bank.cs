using System;
using System.Diagnostics;
using TanatKernel;
using UnityEngine;

public class Bank : GuiElement, EscapeListener
{
	public delegate void ChangeMoneyCallback(int _diamondMoney, int _goldMoney);

	public ChangeMoneyCallback mChangeMoneyCallback;

	private Texture2D mFrame1Image;

	private GuiButton mBarter1Button;

	private GuiButton mCloseButton;

	private GuiButton mHelpButton;

	private GuiButton mDiamondUpButton;

	private GuiButton mDiamondDownButton;

	private IRealMoneyHolder mMoneyHolder;

	private Rect mPlayerDiamondsRect;

	private int mCurDiamonds;

	private Rect mCurDiamondsRect;

	private string mMoneyText;

	private string mDiamondsText;

	private string mBankText;

	private string mExchangeRateText;

	private string mYourMoneyText;

	private string mTradeText;

	private Rect mMoneyTextRect;

	private Rect mDiamondsTextRect;

	private Rect mBankTextRect;

	private Rect mExchangeRateTextRect;

	private Rect mYourMoneyTextRect;

	private Rect mTradeTextRect1;

	private Rect mMoneyRateRect1;

	private Rect mMoneyRateRect2;

	private Rect mMoneyRateRect3;

	private Rect mGoldRect;

	private Rect mSilverRect;

	private Rect mBronzeRect;

	private Rect mExGoldRect;

	private Rect mExSilverRect;

	private Rect mExBronzeRect;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mFrame1Image = GuiSystem.GetImage("Gui/Bank/frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(200, this);
		mBarter1Button = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBarter1Button.mElementId = "BARTER1";
		GuiButton guiButton = mBarter1Button;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mBarter1Button.mLabel = GuiSystem.GetLocaleText("DO_TRADE_TEXT");
		mBarter1Button.Init();
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton2 = mCloseButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mHelpButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mHelpButton.mElementId = "HELP";
		GuiButton guiButton3 = mHelpButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mHelpButton.mLabel = GuiSystem.GetLocaleText("HOW_TO_BUY_TEXT");
		mHelpButton.RegisterAction(UserActionType.BANK_HELP_CLICK);
		mHelpButton.Init();
		mDiamondUpButton = GuiSystem.CreateButton("Gui/misc/str_02_1", "Gui/misc/str_02_2", "Gui/misc/str_02_3", string.Empty, string.Empty);
		mDiamondUpButton.mElementId = "DIAMONDUP";
		GuiButton guiButton4 = mDiamondUpButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mDiamondUpButton.Init();
		mDiamondDownButton = GuiSystem.CreateButton("Gui/misc/str_01_1", "Gui/misc/str_01_2", "Gui/misc/str_01_3", string.Empty, string.Empty);
		mDiamondDownButton.mElementId = "DIAMONDDOWN";
		GuiButton guiButton5 = mDiamondDownButton;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mDiamondDownButton.Init();
		mMoneyText = GuiSystem.GetLocaleText("MONEY_TEXT") + ":";
		mDiamondsText = GuiSystem.GetLocaleText("DIAMONDS_TEXT") + ":";
		mTradeText = GuiSystem.GetLocaleText("TRADE_TEXT") + ":";
		mBankText = GuiSystem.GetLocaleText("BANK_TEXT");
		mExchangeRateText = GuiSystem.GetLocaleText("EXCHANGE_RATE_TEXT");
		mYourMoneyText = GuiSystem.GetLocaleText("YOUR_MONEY");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrame1Image.width, mFrame1Image.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mCloseButton.mZoneRect = new Rect(314f, 7f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mBarter1Button.mZoneRect = new Rect(130f, 271f, 91f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mBarter1Button.mZoneRect);
		mHelpButton.mZoneRect = new Rect(181f, 190f, 127f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mHelpButton.mZoneRect);
		mDiamondDownButton.mZoneRect = new Rect(169f, 248f, 13f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref mDiamondDownButton.mZoneRect);
		mDiamondUpButton.mZoneRect = new Rect(169f, 237f, 13f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref mDiamondUpButton.mZoneRect);
		mBankTextRect = new Rect(24f, 9f, 290f, 19f);
		GuiSystem.SetChildRect(mZoneRect, ref mBankTextRect);
		mYourMoneyTextRect = new Rect(39f, 49f, 268f, 19f);
		GuiSystem.SetChildRect(mZoneRect, ref mYourMoneyTextRect);
		mExchangeRateTextRect = new Rect(26f, 153f, 295f, 19f);
		GuiSystem.SetChildRect(mZoneRect, ref mExchangeRateTextRect);
		mTradeTextRect1 = new Rect(33f, 236f, 66f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mTradeTextRect1);
		mMoneyTextRect = new Rect(25f, 84f, 154f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyTextRect);
		mDiamondsTextRect = new Rect(25f, 112f, 154f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mDiamondsTextRect);
		mCurDiamondsRect = new Rect(124f, 240f, 41f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mCurDiamondsRect);
		mPlayerDiamondsRect = new Rect(210f, 112f, 100f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayerDiamondsRect);
		mMoneyRateRect1 = new Rect(80f, 196f, 16f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyRateRect1);
		mMoneyRateRect2 = new Rect(96f, 196f, 16f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyRateRect2);
		mMoneyRateRect3 = new Rect(122f, 196f, 16f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyRateRect3);
		mGoldRect = new Rect(202f, 84f, 39f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mGoldRect);
		mSilverRect = new Rect(252f, 84f, 24f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mSilverRect);
		mBronzeRect = new Rect(287f, 84f, 21f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mBronzeRect);
		mExGoldRect = new Rect(202f, 240f, 39f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mExGoldRect);
		mExSilverRect = new Rect(252f, 240f, 24f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mExSilverRect);
		mExBronzeRect = new Rect(287f, 240f, 21f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mExBronzeRect);
		Rect _rect = new Rect(182f, 236f, 131f, 24f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT1", mHelpButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT2", mCurDiamondsRect);
		PopupInfo.AddTip(this, "TIP_TEXT3", _rect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame1Image, mZoneRect);
		mBarter1Button.RenderElement();
		mCloseButton.RenderElement();
		mHelpButton.RenderElement();
		mDiamondDownButton.RenderElement();
		mDiamondUpButton.RenderElement();
		GuiSystem.DrawString(mBankText, mBankTextRect, "label");
		GuiSystem.DrawString(mExchangeRateText, mExchangeRateTextRect, "label");
		GuiSystem.DrawString(mYourMoneyText, mYourMoneyTextRect, "label");
		GuiSystem.DrawString(string.Format("{0,0}", mMoneyHolder.RealDiamondMoney), mPlayerDiamondsRect, "middle_left");
		GuiSystem.DrawString(mCurDiamonds.ToString(), mCurDiamondsRect, "middle_center");
		GuiSystem.DrawString(mMoneyText, mMoneyTextRect, "middle_center");
		GuiSystem.DrawString(mDiamondsText, mDiamondsTextRect, "middle_center");
		GuiSystem.DrawString(mTradeText, mTradeTextRect1, "middle_center");
		GuiSystem.DrawString("1", mMoneyRateRect1, "middle_center");
		GuiSystem.DrawString("-", mMoneyRateRect2, "middle_center");
		GuiSystem.DrawString("1", mMoneyRateRect3, "middle_center");
		int _gold = 0;
		int _silver = 0;
		int _bronze = 0;
		ShopVendor.SetMoney(mCurDiamonds * 100, ref _gold, ref _silver, ref _bronze);
		GuiSystem.DrawString(_gold.ToString(), mExGoldRect, "middle_center");
		GuiSystem.DrawString(_silver.ToString(), mExSilverRect, "middle_center");
		GuiSystem.DrawString(_bronze.ToString(), mExBronzeRect, "middle_center");
		ShopVendor.SetMoney(mMoneyHolder.VirtualMoney, ref _gold, ref _silver, ref _bronze);
		GuiSystem.DrawString(_gold.ToString(), mGoldRect, "middle_center");
		GuiSystem.DrawString(_silver.ToString(), mSilverRect, "middle_center");
		GuiSystem.DrawString(_bronze.ToString(), mBronzeRect, "middle_center");
	}

	public override void CheckEvent(Event _curEvent)
	{
		mBarter1Button.CheckEvent(_curEvent);
		mCloseButton.CheckEvent(_curEvent);
		mHelpButton.CheckEvent(_curEvent);
		mDiamondDownButton.CheckEvent(_curEvent);
		mDiamondUpButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && _sender.mElementId == "BARTER1")
		{
			Exchange(mCurDiamonds * 100, 0);
		}
		else if (_buttonId == 0 && _sender.mElementId == "CLOSE_BUTTON")
		{
			SetActive(_active: false);
		}
		else if (_buttonId == 0 && _sender.mElementId == "HELP")
		{
			Process.Start("http://www.tanatonline.ru/payment");
		}
		else if (_buttonId == 0 && _sender.mElementId == "DIAMONDUP")
		{
			AddTo(ref mCurDiamonds, mMoneyHolder.RealDiamondMoney, 1);
		}
		else if (_buttonId == 0 && _sender.mElementId == "DIAMONDDOWN")
		{
			AddTo(ref mCurDiamonds, mMoneyHolder.RealDiamondMoney, -1);
		}
	}

	public override void SetActive(bool _active)
	{
		base.SetActive(_active);
	}

	private void Exchange(int _diamons, int _pearls)
	{
		if (mChangeMoneyCallback != null && (_diamons > 0 || _pearls > 0))
		{
			mChangeMoneyCallback(_diamons, _pearls);
			mCurDiamonds = 0;
		}
	}

	private void AddTo(ref int _money, float _maxMoney, int _val)
	{
		int num = Mathf.FloorToInt(_maxMoney);
		if (_money + _val <= num && _money + _val > 0)
		{
			_money += _val;
		}
		else
		{
			_money = ((_money + _val > num && _money + _val > 0) ? num : 0);
		}
	}

	public void SetData(IRealMoneyHolder _moneyHolder)
	{
		mMoneyHolder = _moneyHolder;
	}

	public void Clean()
	{
		mMoneyHolder = null;
	}
}
