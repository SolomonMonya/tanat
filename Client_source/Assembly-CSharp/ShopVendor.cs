using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ShopVendor : GuiElement
{
	private class VendorItem
	{
		public GuiButton mButton;

		public int mGold;

		public int mSilver;

		public int mBronze;

		public string mName = string.Empty;

		public Rect mRect = default(Rect);

		public Rect mNameRect = default(Rect);

		public Rect mGoldRect = default(Rect);

		public Rect mSilverRect = default(Rect);

		public Rect mBronzeRect = default(Rect);

		public void CalcCost(int _cost)
		{
			SetMoney(_cost, ref mGold, ref mSilver, ref mBronze);
		}
	}

	public Texture2D mFrame1;

	private Texture2D mItemFrame1;

	private Texture2D mVendorImage;

	private int mShopId = -1;

	private int mGold;

	private int mSilver;

	private int mBronze;

	private Rect mGoldRect = default(Rect);

	private Rect mSilverRect = default(Rect);

	private Rect mBronzeRect = default(Rect);

	private Rect mVendorRect = default(Rect);

	private int mMoney;

	private List<VendorItem> mVendorItems = new List<VendorItem>();

	private GuiButton mCloseButton;

	private FormatedTipMgr mFormatedTipMgr;

	private IStoreContentProvider<BattlePrototype> mPrototypes;

	private IStoreContentProvider<CtrlPrototype> mCtrlPrototypes;

	private IMoneyHolder mMoneyHolder;

	private ICustomer mCustomer;

	private PlayerControl mPlayerCtrl;

	public ShopVendor(int _id, int _shopId, int _num)
	{
		mId = _id;
		mShopId = _shopId;
		mFrame1 = GuiSystem.GetImage("Gui/Shop/shop_frame2");
		mItemFrame1 = GuiSystem.GetImage("Gui/Shop/item_frame1");
		mVendorImage = GuiSystem.GetImage("Gui/Shop/vender_image" + _num);
	}

	public static void SetMoney(int _money, ref int _gold, ref int _silver, ref int _bronze)
	{
		_money = Mathf.Abs(_money);
		_gold = (int)Mathf.Floor((float)_money / 10000f);
		_silver = (int)Mathf.Floor((float)(_money - _gold * 10000) / 100f);
		_bronze = _money - _gold * 10000 - _silver * 100;
	}

	public override void Init()
	{
		mCloseButton = new GuiButton();
		mCloseButton.mElementId = "INVENTORY_CLOSE_BUTTON";
		mCloseButton.mNormImg = GuiSystem.GetImage("Gui/misc/button_10_norm");
		mCloseButton.mOverImg = GuiSystem.GetImage("Gui/misc/button_10_over");
		mCloseButton.mPressImg = GuiSystem.GetImage("Gui/misc/button_10_press");
		mCloseButton.mOnMouseUp = OnCloseButton;
		mCloseButton.Init();
		GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet("battle");
		mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
	}

	public void SetData(BattlePrototype _vendorData, BattlePrototype _shopData, IStoreContentProvider<BattlePrototype> _prototypes, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes, PlayerControl _playerCtrl, IMoneyHolder _moneyHolder, ICustomer _customer)
	{
		mMoneyHolder = _moneyHolder;
		mMoney = mMoneyHolder.VirtualMoney;
		SetMoney(mMoney, ref mGold, ref mSilver, ref mBronze);
		mCustomer = _customer;
		mPrototypes = _prototypes;
		mCtrlPrototypes = _ctrlPrototypes;
		mPlayerCtrl = _playerCtrl;
		int num = 0;
		int[] mItems = _vendorData.ShopSeller.mItems;
		foreach (int num2 in mItems)
		{
			VendorItem vendorItem = new VendorItem();
			BattlePrototype battlePrototype = mPrototypes.Get(num2);
			vendorItem.CalcCost((int)((float)battlePrototype.Item.mBuyCost * _shopData.Shop.mBuyCoef));
			vendorItem.mName = GuiSystem.GetLocaleText(battlePrototype.Desc.mName);
			vendorItem.mButton = new GuiButton();
			vendorItem.mButton.mElementId = "VENDOR_ITEM_BUTTON_" + mVendorItems.Count;
			vendorItem.mButton.mId = num2;
			vendorItem.mButton.mIconImg = GuiSystem.GetImage("Gui/Icons/items/" + battlePrototype.Desc.mIcon);
			vendorItem.mButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/btn_norm");
			vendorItem.mButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/btn_over");
			vendorItem.mButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/btn_press");
			vendorItem.mButton.mIconOnTop = false;
			GuiButton mButton = vendorItem.mButton;
			mButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton.mOnMouseUp, new OnMouseUp(OnItem));
			GuiButton mButton2 = vendorItem.mButton;
			mButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton mButton3 = vendorItem.mButton;
			mButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			vendorItem.mButton.Init();
			num++;
			mVendorItems.Add(vendorItem);
		}
	}

	public override void SetSize()
	{
		mGoldRect = new Rect(236f, 377f, 21f, 15f);
		mSilverRect = new Rect(268f, 377f, 21f, 15f);
		mBronzeRect = new Rect(300f, 377f, 21f, 15f);
		mVendorRect = new Rect(-6f, -mVendorImage.height + 24, mVendorImage.width, mVendorImage.height);
		GuiSystem.SetChildRect(mZoneRect, ref mGoldRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSilverRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBronzeRect);
		GuiSystem.SetChildRect(mZoneRect, ref mVendorRect);
		int num = 0;
		int width = mItemFrame1.width;
		int height = mItemFrame1.height;
		foreach (VendorItem mVendorItem in mVendorItems)
		{
			int num2 = (int)Mathf.Floor((float)num / 2f);
			int num3 = num - num2 * 2;
			mVendorItem.mRect = new Rect(7 + num3 * 158, 35 + num2 * 56, width, height);
			mVendorItem.mNameRect = new Rect(54f, 1f, 100f, 34f);
			mVendorItem.mGoldRect = new Rect(71f, 36f, 21f, 15f);
			mVendorItem.mSilverRect = new Rect(103f, 36f, 21f, 15f);
			mVendorItem.mBronzeRect = new Rect(136f, 36f, 21f, 15f);
			GuiSystem.SetChildRect(mZoneRect, ref mVendorItem.mRect);
			GuiSystem.SetChildRect(mVendorItem.mRect, ref mVendorItem.mNameRect);
			GuiSystem.SetChildRect(mVendorItem.mRect, ref mVendorItem.mGoldRect);
			GuiSystem.SetChildRect(mVendorItem.mRect, ref mVendorItem.mSilverRect);
			GuiSystem.SetChildRect(mVendorItem.mRect, ref mVendorItem.mBronzeRect);
			mVendorItem.mButton.mZoneRect = new Rect(4f, 4f, 45f, 45f);
			GuiSystem.SetChildRect(mVendorItem.mRect, ref mVendorItem.mButton.mZoneRect);
			num++;
		}
		mCloseButton.mZoneRect = new Rect(306f, 9f, 19f, 19f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
	}

	public override void RenderElement()
	{
		if (mMoneyHolder.VirtualMoney != mMoney)
		{
			mMoney = mMoneyHolder.VirtualMoney;
			SetMoney(mMoney, ref mGold, ref mSilver, ref mBronze);
		}
		if (mVendorImage != null)
		{
			GuiSystem.DrawImage(mVendorImage, mVendorRect);
		}
		if (mFrame1 != null)
		{
			GuiSystem.DrawImage(mFrame1, mZoneRect);
		}
		foreach (VendorItem mVendorItem in mVendorItems)
		{
			GuiSystem.DrawImage(mItemFrame1, mVendorItem.mRect);
			GuiSystem.DrawString(mVendorItem.mName, mVendorItem.mNameRect, "upper_left");
			GuiSystem.DrawString(mVendorItem.mGold.ToString(), mVendorItem.mGoldRect, "middle_left");
			GuiSystem.DrawString(mVendorItem.mSilver.ToString(), mVendorItem.mSilverRect, "middle_left");
			GuiSystem.DrawString(mVendorItem.mBronze.ToString(), mVendorItem.mBronzeRect, "middle_left");
			if (mVendorItem.mButton != null)
			{
				mVendorItem.mButton.RenderElement();
			}
		}
		GuiSystem.DrawString(mGold.ToString(), mGoldRect, "middle_left");
		GuiSystem.DrawString(mSilver.ToString(), mSilverRect, "middle_left");
		GuiSystem.DrawString(mBronze.ToString(), mBronzeRect, "middle_left");
		mCloseButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (VendorItem mVendorItem in mVendorItems)
		{
			if (mVendorItem.mButton != null)
			{
				mVendorItem.mButton.CheckEvent(_curEvent);
			}
		}
		mCloseButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			InstanceData data = (mPlayerCtrl.SelfPlayer.Player.Avatar as GameData).Data;
			BattlePrototype battlePrototype = mPrototypes.Get(_sender.mId);
			CtrlPrototype article = mCtrlPrototypes.Get(battlePrototype.Item.mArticle);
			mFormatedTipMgr.Show(battlePrototype, article, data.Level + 1, -1, _sender.UId, false);
		}
	}

	private void OnItemMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && !_sender.mLocked)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	private bool TryBuyItem(int _itemId)
	{
		return true;
	}

	private void OnCloseButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			SetActive(_active: false);
		}
	}

	private void OnItem(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && TryBuyItem(_sender.mId))
		{
			mCustomer.Buy(mShopId, mId, _sender.mId, 1);
		}
	}
}
