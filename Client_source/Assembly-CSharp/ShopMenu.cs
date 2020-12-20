using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ShopMenu : GuiElement, EscapeListener
{
	private class Vendor
	{
		public ShopVendor mShopVendor;

		public GuiButton mVendorButton;
	}

	private Texture2D mFrame1;

	private List<Vendor> mVendors = new List<Vendor>();

	private Vendor mCurVendor;

	private GuiButton mCloseButton;

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
		mFrame1 = GuiSystem.GetImage("Gui/Shop/shop_frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(500, this);
		if (mCloseButton == null)
		{
			mCloseButton = new GuiButton();
			mCloseButton.mElementId = "CLOSE_BUTTON";
			mCloseButton.mNormImg = GuiSystem.GetImage("Gui/misc/button_10_norm");
			mCloseButton.mOverImg = GuiSystem.GetImage("Gui/misc/button_10_over");
			mCloseButton.mPressImg = GuiSystem.GetImage("Gui/misc/button_10_press");
			mCloseButton.mOnMouseUp = OnCloseButton;
			mCloseButton.Init();
		}
	}

	public override void SetSize()
	{
		mZoneRect.x = 0f;
		mZoneRect.y = 250f;
		mZoneRect.width = mFrame1.width;
		mZoneRect.height = mFrame1.height;
		GuiSystem.GetRectScaled(ref mZoneRect);
		mCloseButton.mZoneRect = new Rect(6f, 10f, 17f, 17f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		SetShopVendorsSize();
	}

	public void SetData(int _shopId, BattlePrototype _shopData, IStoreContentProvider<BattlePrototype> _prototypes, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes, IMoneyHolder _moneyHolder, ICustomer _customer, PlayerControl _playerCtrl)
	{
		mVendors.Clear();
		mCurVendor = null;
		InitShopVendors(_shopId, _shopData, _prototypes, _ctrlPrototypes, _playerCtrl, _moneyHolder, _customer);
		SetShopVendorsSize();
		if (mVendors.Count > 0)
		{
			SetCurVendor(mVendors[0]);
		}
	}

	public override void RenderElement()
	{
		if (mFrame1 != null)
		{
			GuiSystem.DrawImage(mFrame1, mZoneRect);
		}
		foreach (Vendor mVendor in mVendors)
		{
			mVendor.mVendorButton.RenderElement();
		}
		if (mCurVendor != null && mCurVendor.mShopVendor.Active)
		{
			mCurVendor.mShopVendor.RenderElement();
		}
		mCloseButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (Vendor mVendor in mVendors)
		{
			mVendor.mVendorButton.CheckEvent(_curEvent);
		}
		if (mCurVendor != null && mCurVendor.mShopVendor.Active)
		{
			mCurVendor.mShopVendor.CheckEvent(_curEvent);
		}
		mCloseButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	private void InitShopVendors(int _shopId, BattlePrototype _shopData, IStoreContentProvider<BattlePrototype> _prototypes, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes, PlayerControl _playerCtrl, IMoneyHolder _moneyHolder, ICustomer _customer)
	{
		int[] mSellers = _shopData.Shop.mSellers;
		foreach (int id in mSellers)
		{
			Vendor vendor = new Vendor();
			ShopVendor shopVendor = new ShopVendor(id, _shopId, mVendors.Count);
			BattlePrototype vendorData = _prototypes.Get(id);
			shopVendor.Init();
			shopVendor.SetData(vendorData, _shopData, _prototypes, _ctrlPrototypes, _playerCtrl, _moneyHolder, _customer);
			vendor.mShopVendor = shopVendor;
			vendor.mVendorButton = new GuiButton();
			vendor.mVendorButton.mElementId = "VENDOR_BUTTON_" + mVendors.Count;
			vendor.mVendorButton.mId = mVendors.Count;
			vendor.mVendorButton.mIconImg = GuiSystem.GetImage("Gui/Shop/vender_icon" + mVendors.Count);
			vendor.mVendorButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/btn_norm");
			vendor.mVendorButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/btn_over");
			vendor.mVendorButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/btn_press");
			vendor.mVendorButton.mIconOnTop = false;
			vendor.mVendorButton.mOnMouseUp = OnSelectVendor;
			vendor.mVendorButton.Init();
			mVendors.Add(vendor);
		}
	}

	private void SetShopVendorsSize()
	{
		int i = 0;
		for (int count = mVendors.Count; i < count; i++)
		{
			mVendors[i].mShopVendor.mZoneRect.x = 122f;
			mVendors[i].mShopVendor.mZoneRect.y = 0f;
			mVendors[i].mShopVendor.mZoneRect.width = mVendors[i].mShopVendor.mFrame1.width;
			mVendors[i].mShopVendor.mZoneRect.height = mVendors[i].mShopVendor.mFrame1.height;
			GuiSystem.SetChildRect(mZoneRect, ref mVendors[i].mShopVendor.mZoneRect);
			mVendors[i].mShopVendor.SetSize();
			mVendors[i].mVendorButton.mZoneRect = new Rect(14f, 34 + i * 53, 45f, 45f);
			GuiSystem.SetChildRect(mZoneRect, ref mVendors[i].mVendorButton.mZoneRect);
		}
	}

	private void SetCurVendor(Vendor _vendor)
	{
		if (_vendor != null)
		{
			mCurVendor = _vendor;
		}
	}

	private void OnSelectVendor(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			SetCurVendor(mVendors[_sender.mId]);
			mCurVendor.mShopVendor.SetActive(_active: true);
		}
	}

	private void OnCloseButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			SetActive(_active: false);
		}
	}
}
