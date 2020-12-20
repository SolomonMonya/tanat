using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class ShopGUI : GuiInputListener, EscapeListener
{
	public enum ShopType
	{
		CS_SHOP,
		CS_REAL_SHOP,
		BATTLE_SHOP
	}

	private enum ShopMode
	{
		BUY,
		SELL
	}

	public enum ItemType
	{
		QUEST_ITEM = 0,
		HELM = 1,
		ARMOR = 2,
		TROUSERS = 3,
		BOOTS = 4,
		RING = 5,
		NECKLACE = 6,
		SHOULDERS = 7,
		CLOAK = 8,
		BRACERS = 9,
		CLOTHERS = 10,
		BELT = 11,
		WEAPON = 12,
		SOULSHOT = 13,
		CONSUMEBLE = 14,
		RUNE = 0xF,
		TOTEM = 0x10,
		ELIXIR = 18,
		POTION = 19,
		AVATAR = 20,
		FORTUNE_CHESS = 21,
		CURSE = 22,
		BLESS = 23
	}

	private class ItemCategoryComparer : IComparer<KeyValuePair<int, CtrlItem>>
	{
		public int Compare(KeyValuePair<int, CtrlItem> _data1, KeyValuePair<int, CtrlItem> _data2)
		{
			return _data1.Value.mItemGroupWeight.CompareTo(_data2.Value.mItemGroupWeight);
		}
	}

	private class CtrlPrototypeWeigthComparer : IComparer<CtrlPrototype>
	{
		public int Compare(CtrlPrototype _data1, CtrlPrototype _data2)
		{
			return _data1.Article.mWeigth.CompareTo(_data2.Article.mWeigth);
		}
	}

	private class SellItem : GuiElement
	{
		private GuiInputMgr.DraggableButton mItemButton;

		private string mItemCountStr;

		private SelfHero mSelfHero;

		private FormatedTipMgr mFormatedTipMgr;

		private CtrlPrototype mItemData;

		public int ItemCount
		{
			get
			{
				if (mItemButton == null)
				{
					return -1;
				}
				return mItemButton.mData.mCount;
			}
			set
			{
				if (mItemButton != null)
				{
					mItemButton.mData.mCount = value;
					mItemCountStr = value.ToString();
				}
			}
		}

		public override void Init()
		{
			if (mItemButton != null)
			{
				mId = mItemButton.mButton.mId;
				GuiButton mButton = mItemButton.mButton;
				mButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton mButton2 = mItemButton.mButton;
				mButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				mItemButton.mButton.Init();
			}
		}

		public override void Uninit()
		{
			if (mItemButton != null)
			{
				mItemButton.mButton.mOnMouseLeave(mItemButton.mButton);
				GuiButton mButton = mItemButton.mButton;
				mButton.mOnMouseEnter = (OnMouseEnter)Delegate.Remove(mButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton mButton2 = mItemButton.mButton;
				mButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Remove(mButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				mItemButton = null;
				mItemData = null;
				mSelfHero = null;
				mFormatedTipMgr = null;
				mId = -1;
			}
		}

		public override void SetSize()
		{
			if (mItemButton != null)
			{
				mItemButton.mButton.mZoneRect = mZoneRect;
			}
		}

		public override void RenderElement()
		{
			mItemButton.mButton.RenderElement();
			GuiSystem.DrawString(mItemCountStr, mItemButton.mButton.mZoneRect, "lower_right");
		}

		public override void CheckEvent(Event _curEvent)
		{
			mItemButton.mButton.CheckEvent(_curEvent);
		}

		public void SetData(GuiInputMgr.DraggableButton _btn, CtrlPrototype _itemData, SelfHero _selfHero, FormatedTipMgr _tipMgr)
		{
			if (_btn != null)
			{
				mItemButton = _btn;
				mItemData = _itemData;
				mSelfHero = _selfHero;
				mFormatedTipMgr = _tipMgr;
				ItemCount = mItemButton.mData.mCount;
			}
		}

		public bool HasItem()
		{
			return mItemData != null;
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Show(null, mItemData, -1, mSelfHero.Hero.GameInfo.mLevel, _sender.UId, false);
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && !_sender.mLocked)
			{
				mFormatedTipMgr.Hide(_sender.UId);
			}
		}
	}

	private class ItemBuyMenu : GuiElement
	{
		public BuyRequestCallback mBuyRequestCallback;

		private Texture2D mFrame;

		private GuiButton mCloseButton;

		private GuiButton mBuyButton;

		private GuiButton mItemDecButton;

		private GuiButton mItemIncButton;

		private GuiButton mCancelButton;

		private CtrlPrototype mItemData;

		private int mCurCount;

		private MoneyRenderer mItemCost;

		private MoneyRenderer mSumCost;

		private GuiButton mItemButton;

		private string mItemDesc;

		private Rect mItemDescRect;

		private string mSummText;

		private string mCountText;

		private Rect mSummTextRect;

		private Rect mCountTextRect;

		private StaticTextField mCountField;

		private FormatedTipMgr mFormatedTipMgr;

		private IMoneyHolder mMoneyHolder;

		private SelfHero mSelfHero;

		private OkDialog mOkDialog;

		private string mCountCoefStr;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/ShopGUI/frame5");
			mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
			mCloseButton.mElementId = "CLOSE_BUTTON";
			GuiButton guiButton = mCloseButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mCloseButton.Init();
			mBuyButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			mBuyButton.mElementId = "BUY_ACCEPT_BUTTON";
			GuiButton guiButton2 = mBuyButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			mBuyButton.mLabel = GuiSystem.GetLocaleText("SHOP_BUTTON_BUY_TEXT");
			mBuyButton.Init();
			mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			mCancelButton.mElementId = "BUY_CANCEL_BUTTON";
			GuiButton guiButton3 = mCancelButton;
			guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
			mCancelButton.mLabel = GuiSystem.GetLocaleText("Cancel_Button_Name");
			mCancelButton.Init();
			mItemDecButton = GuiSystem.CreateButton("Gui/misc/button_6_norm", "Gui/misc/button_6_over", "Gui/misc/button_6_press", string.Empty, string.Empty);
			mItemDecButton.mElementId = "ITEM_DEC_BUTTON";
			GuiButton guiButton4 = mItemDecButton;
			guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
			mItemDecButton.Init();
			mItemIncButton = GuiSystem.CreateButton("Gui/misc/button_7_norm", "Gui/misc/button_7_over", "Gui/misc/button_7_press", string.Empty, string.Empty);
			mItemIncButton.mElementId = "ITEM_INC_BUTTON";
			GuiButton guiButton5 = mItemIncButton;
			guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
			mItemIncButton.Init();
			mItemButton = new GuiButton();
			GuiButton guiButton6 = mItemButton;
			guiButton6.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton6.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton guiButton7 = mItemButton;
			guiButton7.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton7.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			mSummText = GuiSystem.GetLocaleText("SUMM_TEXT");
			mCountText = GuiSystem.GetLocaleText("COUNT_TEXT");
			mCountField = new StaticTextField();
			mCountField.mElementId = "COUNT_TEXT_FIELD";
			mCountField.mLength = 3;
			mCountField.mStyleId = "text_field_3";
			mCountField.mData = mCurCount.ToString();
			mOkDialog = new OkDialog();
			mOkDialog.SetActive(_active: false);
			mOkDialog.Init();
		}

		public override void Uninit()
		{
			if (mCountField != null)
			{
				mCountField.Uninit();
			}
		}

		public override void SetSize()
		{
			mZoneRect = new Rect(0f, 100f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
			mCloseButton.mZoneRect = new Rect(532f, 142f, 23f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
			mBuyButton.mZoneRect = new Rect(282f, 366f, 130f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mBuyButton.mZoneRect);
			mCancelButton.mZoneRect = new Rect(414f, 366f, 130f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
			mItemDecButton.mZoneRect = new Rect(344f, 287f, 21f, 40f);
			GuiSystem.SetChildRect(mZoneRect, ref mItemDecButton.mZoneRect);
			mItemIncButton.mZoneRect = new Rect(473f, 287f, 21f, 40f);
			GuiSystem.SetChildRect(mZoneRect, ref mItemIncButton.mZoneRect);
			mItemButton.mZoneRect = new Rect(299f, 184f, 38f, 38f);
			GuiSystem.SetChildRect(mZoneRect, ref mItemButton.mZoneRect);
			mItemDescRect = new Rect(348f, 180f, 182f, 44f);
			GuiSystem.SetChildRect(mZoneRect, ref mItemDescRect);
			mSummTextRect = new Rect(288f, 342f, 100f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mSummTextRect);
			mCountField.mZoneRect = new Rect(372f, 290f, 94f, 30f);
			GuiSystem.SetChildRect(mZoneRect, ref mCountField.mZoneRect);
			mCountTextRect = new Rect(288f, 261f, 250f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mCountTextRect);
			mOkDialog.SetSize();
			if (mItemCost != null)
			{
				mItemCost.SetSize(mZoneRect);
				mItemCost.SetOffset(new Vector2(350f, 222f) * GuiSystem.mYRate);
			}
			if (mSumCost != null)
			{
				mSumCost.SetSize(mZoneRect);
				mSumCost.SetOffset(new Vector2(392f, 342f) * GuiSystem.mYRate);
			}
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mItemDesc, mItemDescRect, "upper_left");
			GuiSystem.DrawString(mSummText, mSummTextRect, "middle_center");
			GuiSystem.DrawString(mCountText, mCountTextRect, "middle_center");
			mCloseButton.RenderElement();
			mBuyButton.RenderElement();
			mCancelButton.RenderElement();
			mItemButton.RenderElement();
			mItemDecButton.RenderElement();
			mItemIncButton.RenderElement();
			GuiSystem.DrawString(mCountCoefStr, mItemButton.mZoneRect, "lower_right");
			if (mItemCost != null)
			{
				mItemCost.Render();
			}
			if (mSumCost != null)
			{
				mSumCost.Render();
			}
			if (mOkDialog.Active)
			{
				mOkDialog.RenderElement();
			}
		}

		public override void OnInput()
		{
			mCountField.OnInput();
			int result = 0;
			if (int.TryParse(mCountField.mData, out result) && result != mCurCount)
			{
				SetCurCount(result);
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			mCloseButton.CheckEvent(_curEvent);
			mBuyButton.CheckEvent(_curEvent);
			mCancelButton.CheckEvent(_curEvent);
			mItemButton.CheckEvent(_curEvent);
			mItemDecButton.CheckEvent(_curEvent);
			mItemIncButton.CheckEvent(_curEvent);
			mCountField.CheckEvent(_curEvent);
			if (mOkDialog.Active)
			{
				mOkDialog.CheckEvent(_curEvent);
			}
			base.CheckEvent(_curEvent);
		}

		public override void SetActive(bool _active)
		{
			if (!_active)
			{
				mItemDesc = string.Empty;
				mItemCost = null;
				mItemData = null;
				mSumCost = null;
				if (mOkDialog != null)
				{
					mOkDialog.Clean();
				}
				SetCurCount(1);
			}
			base.SetActive(_active);
		}

		public void SetData(IMoneyHolder _moneyHolder, SelfHero _selfHero, FormatedTipMgr _tipMgr)
		{
			mMoneyHolder = _moneyHolder;
			mFormatedTipMgr = _tipMgr;
			mSelfHero = _selfHero;
		}

		public void Clear()
		{
			mMoneyHolder = null;
			mFormatedTipMgr = null;
			mSelfHero = null;
		}

		public void SetItemData(CtrlPrototype _itemData)
		{
			if (_itemData != null)
			{
				mItemData = _itemData;
				mBuyButton.mId = mItemData.Id;
				mItemButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mItemData.Desc.mIcon);
				mItemDesc = GuiSystem.GetLocaleText(mItemData.Desc.mName);
				mItemCost = new MoneyRenderer(_renderMoneyImage: true, mItemData.Article.mPriceType == 2);
				mItemCost.SetMoney(mItemData.Article.mBuyCost * mItemData.Article.mCountCoef);
				mItemCost.SetSize(mZoneRect);
				mItemCost.SetOffset(new Vector2(350f, 222f) * GuiSystem.mYRate);
				mSumCost = new MoneyRenderer(_renderMoneyImage: true, mItemData.Article.mPriceType == 2);
				mSumCost.SetSize(mZoneRect);
				mSumCost.SetOffset(new Vector2(392f, 342f) * GuiSystem.mYRate);
				mCountCoefStr = mItemData.Article.mCountCoef.ToString();
				SetCurCount(1);
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if ((_sender.mElementId == "CLOSE_BUTTON" || _sender.mElementId == "BUY_CANCEL_BUTTON") && _buttonId == 0)
			{
				SetActive(_active: false);
			}
			else if (_sender.mElementId == "BUY_ACCEPT_BUTTON" && _buttonId == 0)
			{
				TryBuyItem();
			}
			else if (_sender.mElementId == "ITEM_INC_BUTTON" && _buttonId == 0)
			{
				SetCurCount(mCurCount + 1);
			}
			else if (_sender.mElementId == "ITEM_DEC_BUTTON" && _buttonId == 0)
			{
				SetCurCount(mCurCount - 1);
			}
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && mSelfHero != null)
			{
				mFormatedTipMgr.Show(null, mItemData, -1, mSelfHero.Hero.GameInfo.mLevel, _sender.UId, false);
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && !_sender.mLocked)
			{
				mFormatedTipMgr.Hide(_sender.UId);
			}
		}

		private void SetCurCount(int _count)
		{
			mCurCount = _count;
			mCurCount = ((mCurCount < 1) ? 1 : mCurCount);
			mCurCount = ((mCurCount <= 999) ? mCurCount : 999);
			mItemDecButton.mLocked = mCurCount == 1;
			mItemIncButton.mLocked = mCurCount == 999;
			mCountField.mData = mCurCount.ToString();
			SetSumCost();
		}

		private void SetSumCost()
		{
			if (mSumCost != null && mItemData != null)
			{
				mSumCost.SetMoney(mItemData.Article.mBuyCost * mCurCount * mItemData.Article.mCountCoef);
			}
		}

		private void TryBuyItem()
		{
			if (mItemData == null)
			{
				return;
			}
			if (CheckMoney())
			{
				if (mBuyRequestCallback != null)
				{
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					UserLog.AddAction(UserActionType.BUY, mItemData.Id, $"{GuiSystem.GetLocaleText(mItemData.Desc.mName)}: {mCurCount}");
					dictionary.Add(mItemData.Id, mCurCount);
					mBuyRequestCallback(dictionary);
					SetActive(_active: false);
				}
				else
				{
					Log.Error("can't buy item, buy callback == null");
				}
			}
			else
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_BUY_NOT_ENOUGH_MONEY"));
			}
		}

		private bool CheckMoney()
		{
			if (mMoneyHolder == null)
			{
				return false;
			}
			int num = mItemData.Article.mBuyCost * mCurCount * mItemData.Article.mCountCoef;
			return num <= ((mItemData.Article.mPriceType != 1) ? mMoneyHolder.RealMoney : mMoneyHolder.VirtualMoney);
		}
	}

	private class ShopItem : GuiElement
	{
		public delegate void OnBuyItem(CtrlPrototype _itemData);

		public OnBuyItem mOnBuyItem;

		private GuiButton mItemButton;

		private GuiButton mBuyButton;

		private Texture2D mItemFrame;

		private Texture2D mLockedImage;

		private Rect mItemFrameRect;

		private MoneyRenderer mMoneyRenderer;

		private string mDesc;

		private Rect mDescRect;

		private CtrlPrototype mItemData;

		private FormatedTipMgr mFormatedTipMgr;

		private SelfHero mSelfHero;

		private string mCountCoefStr;

		private PopupInfo mPopupInfo;

		private GuiButton mItemFlagButton;

		public override void Init()
		{
			if (mItemData != null)
			{
				mItemFrame = GuiSystem.GetImage("Gui/ShopGUI/frame3");
				mDesc = GuiSystem.GetLocaleText(mItemData.Desc.mName);
				mMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, mItemData.Article.mPriceType == 2);
				mMoneyRenderer.SetMoney(mItemData.Article.mBuyCost * mItemData.Article.mCountCoef);
				mBuyButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
				mBuyButton.mId = mItemData.Id;
				mBuyButton.mElementId = "BUY_ITEM_BUTTON";
				GuiButton guiButton = mBuyButton;
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				mBuyButton.mLabel = GuiSystem.GetLocaleText("SHOP_BUTTON_BUY_TEXT");
				mBuyButton.Init();
				mItemButton = new GuiButton();
				mItemButton.mElementId = "ITEM_BUTTON";
				GuiButton guiButton2 = mItemButton;
				guiButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton guiButton3 = mItemButton;
				guiButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				mItemButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mItemData.Desc.mIcon);
				if (mSelfHero.Hero.GameInfo.mLevel < mItemData.Article.mMinHeroLvl)
				{
					mLockedImage = GuiSystem.GetImage("Gui/ShopGUI/frame4");
				}
				mCountCoefStr = mItemData.Article.mCountCoef.ToString();
				mItemButton.Init();
				if (mItemData.Article.ContainsFlags())
				{
					mItemFlagButton = new GuiButton();
					mItemFlagButton.mElementId = "ITEM_FLAG_BUTTON";
					GuiButton guiButton4 = mItemFlagButton;
					guiButton4.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton4.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
					GuiButton guiButton5 = mItemFlagButton;
					guiButton5.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton5.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
					mItemFlagButton.mIconImg = GetItemFlagIcon(mItemData);
					mItemFlagButton.Init();
				}
			}
		}

		public override void SetSize()
		{
			mDescRect = new Rect(59f, 9f, 189f, 39f);
			mItemFrameRect = new Rect(9f, 9f, 47f, 48f);
			mItemButton.mZoneRect = new Rect(13f, 14f, 38f, 38f);
			mBuyButton.mZoneRect = new Rect(178f, 47f, 67f, 28f);
			mMoneyRenderer.SetSize(mZoneRect);
			mMoneyRenderer.SetOffset(new Vector2(75f, 52f) * GuiSystem.mYRate);
			GuiSystem.SetChildRect(mZoneRect, ref mDescRect);
			GuiSystem.SetChildRect(mZoneRect, ref mItemFrameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mItemButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mBuyButton.mZoneRect);
			if (mItemFlagButton != null)
			{
				mItemFlagButton.mZoneRect = new Rect(9f, 9f, 34f, 33f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemFlagButton.mZoneRect);
			}
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mItemFrame, mItemFrameRect);
			mItemButton.RenderElement();
			GuiSystem.DrawString(mCountCoefStr, mItemButton.mZoneRect, "lower_right");
			if (mLockedImage != null)
			{
				GuiSystem.DrawImage(mLockedImage, mItemButton.mZoneRect);
			}
			if (mItemFlagButton != null)
			{
				mItemFlagButton.RenderElement();
			}
			mBuyButton.RenderElement();
			mMoneyRenderer.Render();
			GuiSystem.DrawString(mDesc, mDescRect, "upper_left");
		}

		public override void CheckEvent(Event _curEvent)
		{
			mItemButton.CheckEvent(_curEvent);
			mBuyButton.CheckEvent(_curEvent);
			if (mItemFlagButton != null)
			{
				mItemFlagButton.CheckEvent(_curEvent);
			}
			base.CheckEvent(_curEvent);
		}

		public void SetItemData(CtrlPrototype _itemData)
		{
			mItemData = _itemData;
		}

		public void SetData(SelfHero _selfHero, FormatedTipMgr _tipMgr, PopupInfo _popupInfo)
		{
			mSelfHero = _selfHero;
			mFormatedTipMgr = _tipMgr;
			mPopupInfo = _popupInfo;
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && mItemData != null)
			{
				if (_sender.mElementId == "ITEM_BUTTON")
				{
					mFormatedTipMgr.Show(null, mItemData, -1, mSelfHero.Hero.GameInfo.mLevel, _sender.UId, false);
				}
				else if (_sender.mElementId == "ITEM_FLAG_BUTTON")
				{
					mPopupInfo.ShowInfo(GetItemFlagDesc(mItemData), new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y));
				}
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && !_sender.mLocked)
			{
				if (_sender.mElementId == "ITEM_BUTTON")
				{
					mFormatedTipMgr.Hide(_sender.UId);
				}
				else if (_sender.mElementId == "ITEM_FLAG_BUTTON")
				{
					mPopupInfo.Hide();
				}
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "BUY_ITEM_BUTTON" && _buttonId == 0 && mOnBuyItem != null)
			{
				mOnBuyItem(mItemData);
			}
		}

		private Texture2D GetItemFlagIcon(CtrlPrototype _itemData)
		{
			if (_itemData == null)
			{
				return null;
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.NEW))
			{
				return GuiSystem.GetImage("Gui/ShopGUI/ItemFlags/" + CtrlPrototype.ArticleMask.NEW.ToString().ToLower());
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.POPULAR))
			{
				return GuiSystem.GetImage("Gui/ShopGUI/ItemFlags/" + CtrlPrototype.ArticleMask.POPULAR.ToString().ToLower());
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.SALE))
			{
				return GuiSystem.GetImage("Gui/ShopGUI/ItemFlags/" + CtrlPrototype.ArticleMask.SALE.ToString().ToLower());
			}
			return null;
		}

		private string GetItemFlagDesc(CtrlPrototype _itemData)
		{
			if (_itemData == null)
			{
				return null;
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.NEW))
			{
				return GuiSystem.GetLocaleText("ITEM_FLAG_" + CtrlPrototype.ArticleMask.NEW);
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.POPULAR))
			{
				return GuiSystem.GetLocaleText("ITEM_FLAG_" + CtrlPrototype.ArticleMask.POPULAR);
			}
			if (_itemData.Article.CheckFlag(CtrlPrototype.ArticleMask.SALE))
			{
				return GuiSystem.GetLocaleText("ITEM_FLAG_" + CtrlPrototype.ArticleMask.SALE);
			}
			return null;
		}
	}

	private class ItemGroup : GuiElement
	{
		public delegate void SelectItemGroup(int _itemGroupId);

		public Rect mRenderZone;

		public GuiButton mGroupButton;

		public Dictionary<int, GuiButton> mItemTypeButtons;

		public SelectItemGroup mOnSelectItemGroup;

		private int mItemGroupId;

		private bool mOpened;

		private Texture2D mItemSelectionImg;

		private Vector2 mOffset = Vector2.zero;

		public bool Open
		{
			get
			{
				return mOpened;
			}
			set
			{
				mOpened = value;
				SetGroupButtonImage();
			}
		}

		public ItemGroup(int _groupId)
		{
			mItemGroupId = _groupId;
		}

		public override void Init()
		{
			mOpened = false;
			mGroupButton = new GuiButton();
			mGroupButton.mId = mItemGroupId;
			mGroupButton.mElementId = "GROUP_BUTTON";
			mGroupButton.mLockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			mGroupButton.mIconImg = GuiSystem.GetImage("Gui/ShopGUI/ItemGroups/item_group_" + mItemGroupId);
			GuiButton guiButton = mGroupButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mGroupButton.Init();
			mItemSelectionImg = GuiSystem.GetImage("Gui/QuestJournal/selection");
			mItemTypeButtons = new Dictionary<int, GuiButton>();
			SetGroupButtonImage();
		}

		public override void SetSize()
		{
			SetItemTypesSize();
		}

		public override void RenderElement()
		{
			if (ButtonInRenderZone(mGroupButton))
			{
				mGroupButton.RenderElement();
			}
			if (!mOpened)
			{
				return;
			}
			foreach (KeyValuePair<int, GuiButton> mItemTypeButton in mItemTypeButtons)
			{
				if (ButtonInRenderZone(mItemTypeButton.Value))
				{
					mItemTypeButton.Value.RenderElement();
				}
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			mGroupButton.CheckEvent(_curEvent);
			if (!mOpened)
			{
				return;
			}
			foreach (KeyValuePair<int, GuiButton> mItemTypeButton in mItemTypeButtons)
			{
				if (ButtonInRenderZone(mItemTypeButton.Value))
				{
					mItemTypeButton.Value.CheckEvent(_curEvent);
				}
			}
		}

		public void ClearItems()
		{
			mItemTypeButtons.Clear();
			Open = false;
			SetGroupButtonState();
		}

		public void SetItems(List<CtrlPrototype> _items)
		{
			if (_items == null)
			{
				return;
			}
			mItemTypeButtons.Clear();
			foreach (CtrlPrototype _item in _items)
			{
				if (!mItemTypeButtons.ContainsKey(_item.Article.mKindId))
				{
					GuiButton guiButton = new GuiButton();
					guiButton.mId = _item.Article.mKindId;
					guiButton.mElementId = "ITEM_TYPE_BUTTON";
					guiButton.mOverImg = mItemSelectionImg;
					guiButton.mLabel = GetItemTypeLabel((ItemType)_item.Article.mKindId);
					guiButton.mLabelStyle = "middle_left";
					guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
					mItemTypeButtons.Add(_item.Article.mKindId, guiButton);
				}
			}
			SetGroupButtonState();
		}

		public void SetItemTypesSize()
		{
			int num = 0;
			foreach (GuiButton value in mItemTypeButtons.Values)
			{
				value.mZoneRect = new Rect(mOffset.x, mOffset.y + 42f + (float)(num * mItemSelectionImg.height), mItemSelectionImg.width, mItemSelectionImg.height);
				GuiSystem.SetChildRect(mZoneRect, ref value.mZoneRect);
				num++;
			}
		}

		public float GetGroupLength()
		{
			if (mOpened)
			{
				return 42 + mItemTypeButtons.Count * mItemSelectionImg.height;
			}
			return 42f;
		}

		public void SetOffset(Vector2 _offset)
		{
			mOffset = _offset;
			mGroupButton.mZoneRect = new Rect(mOffset.x, mOffset.y, 173f, 42f);
			GuiSystem.SetChildRect(mZoneRect, ref mGroupButton.mZoneRect);
		}

		public void SetSelectedItemType(int _id)
		{
			foreach (GuiButton value in mItemTypeButtons.Values)
			{
				value.Pressed = _id == value.mId;
				if (value.Pressed)
				{
					Open = true;
				}
			}
		}

		private void OnButton(GuiElement _sender, int _elementId)
		{
			if (_sender.mElementId == "ITEM_TYPE_BUTTON" && _elementId == 0)
			{
				if (mOnSelectItemGroup != null)
				{
					mOnSelectItemGroup(mItemGroupId);
				}
				mOnMouseUp(_sender, _elementId);
			}
			else if (_sender.mElementId == "GROUP_BUTTON" && _elementId == 0)
			{
				mOnMouseUp(mGroupButton, _elementId);
			}
		}

		private bool ButtonInRenderZone(GuiButton _btn)
		{
			return _btn.mZoneRect.y >= mRenderZone.y && _btn.mZoneRect.y <= mRenderZone.y + mRenderZone.height - _btn.mZoneRect.height;
		}

		private void SetGroupButtonState()
		{
			if (mGroupButton != null)
			{
				mGroupButton.mLocked = mItemTypeButtons.Count == 0;
			}
		}

		private void SetGroupButtonImage()
		{
			string text = "Gui/ShopGUI/button_" + ((!mOpened) ? "2" : "1");
			mGroupButton.mNormImg = GuiSystem.GetImage(text + "_norm");
			mGroupButton.mOverImg = GuiSystem.GetImage(text + "_over");
			mGroupButton.mPressImg = GuiSystem.GetImage(text + "_press");
			mGroupButton.SetCurBtnImage();
		}
	}

	public delegate void BuyRequestCallback(Dictionary<int, int> _basket);

	public delegate void SellRequestCallback(Dictionary<int, int> _basket);

	public delegate void OnVoidAction();

	public BuyRequestCallback mBuyRequestCallback;

	public SellRequestCallback mSellRequestCallback;

	public OnVoidAction mOnUpdateInventory;

	public OnVoidAction mOnShowBank;

	private ShopType mShopType;

	private ShopMode mShopMode;

	private string mShopTypeStr;

	private Rect mShopTypeStrRect;

	private GuiButton mCloseButton;

	private Texture2D mCurFrame;

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private FormatedTipMgr mFormatedTipMgr;

	private SelfHero mSelfHero;

	private GuiButton mBuyModeButton;

	private GuiButton mSellModeButton;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private Rect mGroupsZoneRect;

	private Dictionary<int, ItemGroup> mItemGroups;

	private List<KeyValuePair<int, CtrlItem>> mItemsData;

	private int mSelectedItemGroupId;

	private int mSelectedItemTypeId;

	private List<ShopItem> mShopItems;

	private int mCurPage;

	private int mMaxPages;

	private int mMaxItemsOnPage;

	private string mPagesStr;

	private Rect mPagesRect;

	private GuiButton mPageLeftButton;

	private GuiButton mPageRightButton;

	private IMoneyHolder mMoneyHolder;

	private int mLastMoney;

	private int mLastDiamonds;

	private Rect mGoldRect;

	private Rect mSilverRect;

	private Rect mBronzeRect;

	private Rect mDiamondsRect;

	private string mGold;

	private string mSilver;

	private string mBronze;

	private string mDiamonds;

	private string mPlayerMoneyText;

	private Rect mPlayerMoneyTextRect;

	private GuiButton mBankButton;

	private CheckBox mCheckBox;

	private string mLvlFilterText;

	private Rect mLvlFilterTextRect;

	private ItemBuyMenu mItemBuyMenu;

	private Dictionary<int, SellItem> mSellItems;

	private Rect mDragItemsZone;

	private GuiButton mSellButton;

	private GuiButton mCancelSellButton;

	private int mSellCost;

	private MoneyRenderer mSellCostMoney;

	private string mSellCostText;

	private Rect mSellCostTextRect;

	private PopupInfo mPopupInfo;

	private OkDialog mOkDialog;

	public static string GetItemTypeLabel(ItemType _itemType)
	{
		return _itemType switch
		{
			ItemType.HELM => GuiSystem.GetLocaleText("HELM_TEXT"), 
			ItemType.ARMOR => GuiSystem.GetLocaleText("ARMOR_TEXT"), 
			ItemType.TROUSERS => GuiSystem.GetLocaleText("TROUSERS_TEXT"), 
			ItemType.BOOTS => GuiSystem.GetLocaleText("BOOTS_TEXT"), 
			ItemType.RING => GuiSystem.GetLocaleText("RING_TEXT"), 
			ItemType.NECKLACE => GuiSystem.GetLocaleText("NECKLACE_TEXT"), 
			ItemType.SHOULDERS => GuiSystem.GetLocaleText("SHOULDERS_TEXT"), 
			ItemType.CLOAK => GuiSystem.GetLocaleText("CLOAK_TEXT"), 
			ItemType.BRACERS => GuiSystem.GetLocaleText("BRACERS_TEXT"), 
			ItemType.CLOTHERS => GuiSystem.GetLocaleText("CLOTHERS_TEXT"), 
			ItemType.BELT => GuiSystem.GetLocaleText("BELT_TEXT"), 
			ItemType.WEAPON => GuiSystem.GetLocaleText("WEAPON_TEXT"), 
			ItemType.SOULSHOT => GuiSystem.GetLocaleText("SOULSHOTS_TEXT"), 
			ItemType.CONSUMEBLE => GuiSystem.GetLocaleText("CONSUMEBLE_TEXT"), 
			ItemType.QUEST_ITEM => GuiSystem.GetLocaleText("QUEST_ITEM_TEXT"), 
			ItemType.RUNE => GuiSystem.GetLocaleText("RUNE_ITEM_TEXT"), 
			ItemType.TOTEM => GuiSystem.GetLocaleText("TOTEM_ITEM_TEXT"), 
			ItemType.ELIXIR => GuiSystem.GetLocaleText("ELIXIR_ITEM_TEXT"), 
			ItemType.POTION => GuiSystem.GetLocaleText("POTION_ITEM_TEXT"), 
			ItemType.AVATAR => GuiSystem.GetLocaleText("AVATAR_ITEM_TEXT"), 
			ItemType.FORTUNE_CHESS => GuiSystem.GetLocaleText("FORTUNE_CHESS_ITEM_TEXT"), 
			ItemType.CURSE => GuiSystem.GetLocaleText("CURSE_TEXT"), 
			ItemType.BLESS => GuiSystem.GetLocaleText("BLESS_TEXT"), 
			_ => "NO_LABEL", 
		};
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
		mDragType = GuiInputMgr.ButtonDragFlags.SHOP;
		GuiSystem.mGuiInputMgr.AddEscapeListener(75, this);
		if (mShopType == ShopType.CS_SHOP)
		{
			GuiSystem.mGuiInputMgr.AddGuiDraggableButtonListener(this);
		}
		mSelectedItemGroupId = -1;
		mSelectedItemTypeId = -1;
		mLastMoney = -1;
		mLastDiamonds = -1;
		mMaxItemsOnPage = 8;
		mFrame1 = GuiSystem.GetImage("Gui/ShopGUI/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/ShopGUI/frame2");
		mLvlFilterText = GuiSystem.GetLocaleText("SHOW_FILTERED_BY_LEVEL");
		mPlayerMoneyText = GuiSystem.GetLocaleText("EXISTING_PLAYER_MONEY");
		mSellCostText = GuiSystem.GetLocaleText("SHOP_SELL_COST_TEXT");
		mItemGroups = new Dictionary<int, ItemGroup>();
		mShopItems = new List<ShopItem>();
		mSellItems = new Dictionary<int, SellItem>();
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mBuyModeButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBuyModeButton.mElementId = "BUY_MODE_BUTTON";
		GuiButton guiButton2 = mBuyModeButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mBuyModeButton.mLabel = GuiSystem.GetLocaleText("SHOP_MODE_BUY_BUTTON");
		mBuyModeButton.Init();
		mSellModeButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mSellModeButton.mElementId = "SELL_MODE_BUTTON";
		GuiButton guiButton3 = mSellModeButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mSellModeButton.mLabel = GuiSystem.GetLocaleText("SHOP_MODE_SELL_BUTTON");
		mSellModeButton.Init();
		mBankButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBankButton.mElementId = "BANK_BUTTON";
		GuiButton guiButton4 = mBankButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mBankButton.mLabel = GuiSystem.GetLocaleText("IN_BANK_TEXT");
		mBankButton.Init();
		AddTutorialElement(mBankButton);
		mPageLeftButton = GuiSystem.CreateButton("Gui/misc/button_6_norm", "Gui/misc/button_6_over", "Gui/misc/button_6_press", string.Empty, string.Empty);
		mPageLeftButton.mElementId = "PAGE_LEFT_BUTTON";
		GuiButton guiButton5 = mPageLeftButton;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mPageLeftButton.Init();
		mPageRightButton = GuiSystem.CreateButton("Gui/misc/button_7_norm", "Gui/misc/button_7_over", "Gui/misc/button_7_press", string.Empty, string.Empty);
		mPageRightButton.mElementId = "PAGE_RIGHT_BUTTON";
		GuiButton guiButton6 = mPageRightButton;
		guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
		mPageRightButton.Init();
		mCheckBox = new CheckBox();
		mCheckBox.mElementId = "LEVEL_FILTER_CHECKBOX";
		mCheckBox.SetData(OptionsMgr.mShopFilterLvl, "Gui/misc/button_11_norm", "Gui/misc/button_11_over", "Gui/misc/button_11_press", "Gui/misc/button_11_check");
		CheckBox checkBox = mCheckBox;
		checkBox.mOnChecked = (CheckBox.OnChecked)Delegate.Combine(checkBox.mOnChecked, new CheckBox.OnChecked(OnFilter));
		mCheckBox.Init();
		mSellButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mSellButton.mElementId = "SELL_BUTTON";
		GuiButton guiButton7 = mSellButton;
		guiButton7.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton7.mOnMouseUp, new OnMouseUp(OnButton));
		mSellButton.mLabel = GuiSystem.GetLocaleText("SHOP_BUTTON_SELL_TEXT");
		mSellButton.Init();
		mCancelSellButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelSellButton.mElementId = "CANCEL_SELL_BUTTON";
		GuiButton guiButton8 = mCancelSellButton;
		guiButton8.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton8.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelSellButton.mLabel = GuiSystem.GetLocaleText("CLEAR_BASKET_BUTTON");
		mCancelSellButton.Init();
		if (mPopupInfo == null)
		{
			if (mShopType == ShopType.CS_SHOP || mShopType == ShopType.CS_REAL_SHOP)
			{
				mPopupInfo = GuiSystem.mGuiSystem.GetGuiElement<PopupInfo>("central_square", "POPUP_INFO");
			}
			else
			{
				mPopupInfo = GuiSystem.mGuiSystem.GetGuiElement<PopupInfo>("battle", "POPUP_INFO");
			}
		}
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		mSellCostMoney = new MoneyRenderer(_renderMoneyImage: true, _diamonds: false);
		SetSellCostMoney(0);
		mItemBuyMenu = new ItemBuyMenu();
		mItemBuyMenu.Init();
		mItemBuyMenu.SetActive(_active: false);
		mOkDialog = new OkDialog();
		mOkDialog.SetActive(_active: false);
		mOkDialog.Init();
		SetShopMode(ShopMode.BUY);
		mCurFrame = GetFrame(mShopType, mShopMode);
		SetCurPage(0);
		for (int i = 0; i < 36; i++)
		{
			mSellItems.Add(i, new SellItem());
		}
	}

	public override void Uninit()
	{
		if (mItemBuyMenu != null)
		{
			mItemBuyMenu.Uninit();
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 100f, mCurFrame.width, mCurFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(789f, 6f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mBuyModeButton.mZoneRect = new Rect(299f, 39f, 118f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mBuyModeButton.mZoneRect);
		mSellModeButton.mZoneRect = new Rect(413f, 39f, 118f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mSellModeButton.mZoneRect);
		mScrollbar.mZoneRect = new Rect(39f, 74f, 22f, 465f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mGroupsZoneRect = new Rect(61f, 74f, 182f, 465f);
		GuiSystem.SetChildRect(mZoneRect, ref mGroupsZoneRect);
		mShopTypeStrRect = new Rect(25f, 10f, 775f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mShopTypeStrRect);
		mPagesRect = new Rect(475f, 499f, 102f, 34f);
		GuiSystem.SetChildRect(mZoneRect, ref mPagesRect);
		mPageLeftButton.mZoneRect = new Rect(451f, 496f, 21f, 40f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageLeftButton.mZoneRect);
		mPageRightButton.mZoneRect = new Rect(581f, 496f, 21f, 40f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageRightButton.mZoneRect);
		mSellButton.mZoneRect = new Rect(296f, 384f, 116f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mSellButton.mZoneRect);
		mCancelSellButton.mZoneRect = new Rect(413f, 384f, 116f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelSellButton.mZoneRect);
		mCheckBox.mZoneRect = new Rect(264f, 475f, 21f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCheckBox.mZoneRect);
		mCheckBox.SetSize();
		mLvlFilterTextRect = new Rect(290f, 476f, 300f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mLvlFilterTextRect);
		mDragItemsZone = new Rect(70f, 140f, 680f, 287f);
		GuiSystem.SetChildRect(mZoneRect, ref mDragItemsZone);
		mSellCostMoney.SetSize(mZoneRect);
		mSellCostMoney.SetOffset(new Vector2(400f, 340f) * GuiSystem.mYRate);
		mSellCostTextRect = new Rect(280f, 340f, 120f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mSellCostTextRect);
		mItemBuyMenu.SetSize();
		mOkDialog.SetSize();
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (mSellItem.Value.HasItem())
			{
				mSellItem.Value.mZoneRect = GetItemRect(mSellItem.Key);
				mSellItem.Value.SetSize();
			}
		}
		SetShopParamsSize();
		SetItemTypesSize();
		SetGroupOffsets();
		SetShopItemsSize();
		PopupInfo.AddTip(this, "TIP_TEXT41", mBuyModeButton);
		PopupInfo.AddTip(this, "TIP_TEXT42", mSellModeButton);
		PopupInfo.AddTip(this, "TIP_TEXT43", mBankButton);
		PopupInfo.AddTip(this, "TIP_TEXT44", mSellButton);
		PopupInfo.AddTip(this, "TIP_TEXT45", mCancelSellButton);
	}

	private void SetShopParamsSize()
	{
		if (mShopMode == ShopMode.BUY)
		{
			mBankButton.mZoneRect = new Rect(691f, 96f, 81f, 28f);
			mGoldRect = new Rect(415f, 101f, 60f, 14f);
			mSilverRect = new Rect(489f, 101f, 25f, 14f);
			mBronzeRect = new Rect(527f, 101f, 25f, 14f);
			mDiamondsRect = new Rect(586f, 101f, 60f, 14f);
			mPlayerMoneyTextRect = new Rect(268f, 82f, 508f, 16f);
		}
		else if (mShopMode == ShopMode.SELL)
		{
			mBankButton.mZoneRect = new Rect(580f, 96f, 81f, 28f);
			mGoldRect = new Rect(330f, 102f, 60f, 14f);
			mSilverRect = new Rect(404f, 102f, 25f, 14f);
			mBronzeRect = new Rect(442f, 102f, 25f, 14f);
			mDiamondsRect = new Rect(501f, 102f, 60f, 14f);
			mPlayerMoneyTextRect = new Rect(185f, 100f, 128f, 16f);
		}
		GuiSystem.SetChildRect(mZoneRect, ref mBankButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mGoldRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSilverRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBronzeRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDiamondsRect);
		GuiSystem.SetChildRect(mZoneRect, ref mPlayerMoneyTextRect);
	}

	public override void SetActive(bool _active)
	{
		base.SetActive(_active);
	}

	public override void OnInput()
	{
		mScrollbar.OnInput();
		if (mItemBuyMenu.Active)
		{
			mItemBuyMenu.OnInput();
		}
	}

	public override void Update()
	{
		SetMoney();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mCurFrame, mZoneRect);
		GuiSystem.DrawString(mShopTypeStr, mShopTypeStrRect, "label");
		if (mShopType == ShopType.CS_SHOP)
		{
			mBuyModeButton.RenderElement();
			mSellModeButton.RenderElement();
			mBankButton.RenderElement();
		}
		mCloseButton.RenderElement();
		GuiSystem.DrawString(mPlayerMoneyText, mPlayerMoneyTextRect, "middle_center");
		GuiSystem.DrawString(mGold, mGoldRect, "middle_left");
		GuiSystem.DrawString(mSilver, mSilverRect, "middle_left");
		GuiSystem.DrawString(mBronze, mBronzeRect, "middle_left");
		GuiSystem.DrawString(mDiamonds, mDiamondsRect, "middle_left");
		if (mShopMode == ShopMode.BUY)
		{
			mScrollbar.RenderElement();
			mPageLeftButton.RenderElement();
			mPageRightButton.RenderElement();
			mCheckBox.RenderElement();
			GuiSystem.DrawString(mPagesStr, mPagesRect, "middle_center");
			foreach (KeyValuePair<int, ItemGroup> mItemGroup in mItemGroups)
			{
				mItemGroup.Value.RenderElement();
			}
			int i = mCurPage * mMaxItemsOnPage;
			for (int num = (mCurPage + 1) * mMaxItemsOnPage; i < num && mShopItems.Count > i; i++)
			{
				mShopItems[i].RenderElement();
			}
			GuiSystem.DrawString(mLvlFilterText, mLvlFilterTextRect, "middle_left");
		}
		else if (mShopMode == ShopMode.SELL)
		{
			mSellButton.RenderElement();
			mCancelSellButton.RenderElement();
			mSellCostMoney.Render();
			GuiSystem.DrawString(mSellCostText, mSellCostTextRect, "middle_center");
			foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
			{
				if (mSellItem.Value.HasItem())
				{
					mSellItem.Value.RenderElement();
				}
			}
		}
		if (mOkDialog.Active)
		{
			mOkDialog.RenderElement();
		}
		if (mItemBuyMenu.Active)
		{
			mItemBuyMenu.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mBankButton.CheckEvent(_curEvent);
		if (mItemBuyMenu.Active)
		{
			mItemBuyMenu.CheckEvent(_curEvent);
			return;
		}
		if (mShopType == ShopType.CS_SHOP)
		{
			mBuyModeButton.CheckEvent(_curEvent);
			mSellModeButton.CheckEvent(_curEvent);
		}
		if (mShopMode == ShopMode.BUY)
		{
			mScrollbar.CheckEvent(_curEvent);
			mPageLeftButton.CheckEvent(_curEvent);
			mPageRightButton.CheckEvent(_curEvent);
			mCheckBox.CheckEvent(_curEvent);
			foreach (KeyValuePair<int, ItemGroup> mItemGroup in mItemGroups)
			{
				mItemGroup.Value.CheckEvent(_curEvent);
			}
			int i = mCurPage * mMaxItemsOnPage;
			for (int num = (mCurPage + 1) * mMaxItemsOnPage; i < num && mShopItems.Count > i; i++)
			{
				mShopItems[i].CheckEvent(_curEvent);
			}
		}
		else if (mShopMode == ShopMode.SELL)
		{
			mSellButton.CheckEvent(_curEvent);
			mCancelSellButton.CheckEvent(_curEvent);
			foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
			{
				if (mSellItem.Value.HasItem())
				{
					mSellItem.Value.CheckEvent(_curEvent);
				}
			}
		}
		if (mOkDialog.Active)
		{
			mOkDialog.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public override bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _num)
	{
		if (mShopMode != ShopMode.SELL)
		{
			return false;
		}
		if (_btn == null)
		{
			Log.Error("Trying to add null dragged button");
			return false;
		}
		SellItem sellItem = null;
		if (_num == -1)
		{
			_num = GetFreeSlotNum();
			if (_num == -1)
			{
				return false;
			}
		}
		CtrlPrototype itemDataById = GetItemDataById(_btn.mButton.mId);
		sellItem = GetSellItemById(_btn.mButton.mId);
		if (sellItem != null)
		{
			sellItem.ItemCount += _cnt;
			AddSellCostMoney(itemDataById.Article.mSellCost * _cnt);
			return true;
		}
		if (itemDataById.Article.CheckFlag(CtrlPrototype.ArticleMask.NO_SELL))
		{
			mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_CANT_DRAG_ITEM_SELL"));
			return false;
		}
		if (GetSellItemBySlot(_num) != null)
		{
			return false;
		}
		sellItem = new SellItem();
		GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
		draggableButton.mData = new GuiInputMgr.DragItemData();
		draggableButton.mButtonHolder = this;
		draggableButton.mDragFlag = mDragType;
		draggableButton.mData.mCount = _cnt;
		draggableButton.mData.mOrigin = _btn.mData.mOrigin;
		draggableButton.mButton = new GuiButton();
		draggableButton.mButton.mElementId = "INVENTORY_ITEM_BUTTON";
		draggableButton.mButton.mId = _btn.mButton.mId;
		draggableButton.mButton.mDraggableButton = draggableButton;
		draggableButton.mButton.mIconImg = _btn.mButton.mIconImg;
		draggableButton.mButton.mNormImg = _btn.mButton.mNormImg;
		draggableButton.mButton.mOverImg = _btn.mButton.mOverImg;
		draggableButton.mButton.mPressImg = _btn.mButton.mPressImg;
		draggableButton.mButton.mIconOnTop = _btn.mButton.mIconOnTop;
		GuiButton mButton = draggableButton.mButton;
		mButton.mOnDragStart = (OnDragStart)Delegate.Combine(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
		GuiButton mButton2 = draggableButton.mButton;
		mButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton2.mOnMouseUp, new OnMouseUp(OnItem));
		draggableButton.mButton.Init();
		sellItem.SetData(draggableButton, itemDataById, mSelfHero, mFormatedTipMgr);
		sellItem.Init();
		sellItem.mZoneRect = GetItemRect(_num);
		sellItem.SetSize();
		AddSellCostMoney(itemDataById.Article.mSellCost * _cnt);
		mSellItems[_num] = sellItem;
		return true;
	}

	public override void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
		SellItem sellItemById = GetSellItemById(_btn.mButton.mId);
		if (sellItemById != null)
		{
			sellItemById.ItemCount -= _cnt;
			CtrlPrototype itemDataById = GetItemDataById(_btn.mButton.mId);
			AddSellCostMoney(-itemDataById.Article.mSellCost * _cnt);
			if (itemDataById != null && (sellItemById.ItemCount <= 0 || _cnt == -1))
			{
				ClearSellItemById(_btn.mButton.mId);
			}
		}
	}

	public override int GetZoneNum(Vector2 _mousePos)
	{
		if (mShopMode != ShopMode.SELL)
		{
			return -1;
		}
		if (mDragItemsZone.Contains(_mousePos))
		{
			return 1;
		}
		return -1;
	}

	public void Close()
	{
		mSelectedItemTypeId = -1;
		mSelectedItemGroupId = -1;
		mItemGroups.Clear();
		mShopItems.Clear();
		ClearSellItems();
		mSellCost = 0;
		mSellCostMoney.SetMoney(mSellCost);
		SetActive(_active: false);
		if (mItemBuyMenu != null)
		{
			mItemBuyMenu.SetActive(_active: false);
		}
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void SetType(ShopType _type)
	{
		mShopType = _type;
		mShopTypeStr = GuiSystem.GetLocaleText("SHOP_TYPE_" + mShopType);
	}

	public void SetData(SelfHero _selfHero, FormatedTipMgr _tipMgr, IMoneyHolder _holder)
	{
		mSelfHero = _selfHero;
		mFormatedTipMgr = _tipMgr;
		mMoneyHolder = _holder;
		if (mItemBuyMenu != null)
		{
			mItemBuyMenu.SetData(mMoneyHolder, mSelfHero, mFormatedTipMgr);
			ItemBuyMenu itemBuyMenu = mItemBuyMenu;
			itemBuyMenu.mBuyRequestCallback = (BuyRequestCallback)Delegate.Combine(itemBuyMenu.mBuyRequestCallback, mBuyRequestCallback);
		}
	}

	public void SetItems(List<KeyValuePair<int, CtrlItem>> _itemsData)
	{
		if (_itemsData == null || mItemGroups == null)
		{
			return;
		}
		ItemCategoryComparer comparer = new ItemCategoryComparer();
		_itemsData.Sort(comparer);
		mItemsData = _itemsData;
		ItemGroup itemGroup = null;
		foreach (KeyValuePair<int, CtrlItem> mItemsDatum in mItemsData)
		{
			itemGroup = new ItemGroup(mItemsDatum.Key);
			itemGroup.Init();
			ItemGroup itemGroup2 = itemGroup;
			itemGroup2.mOnMouseUp = (OnMouseUp)Delegate.Combine(itemGroup2.mOnMouseUp, new OnMouseUp(OnButton));
			itemGroup.SetItems(mItemsDatum.Value.mItems);
			ItemGroup itemGroup3 = itemGroup;
			itemGroup3.mOnSelectItemGroup = (ItemGroup.SelectItemGroup)Delegate.Combine(itemGroup3.mOnSelectItemGroup, new ItemGroup.SelectItemGroup(OnSelectedItemGroup));
			PopupInfo.AddTip(this, GetGroupTipById(mItemsDatum.Key), itemGroup.mGroupButton);
			if (mItemGroups.ContainsKey(mItemsDatum.Key))
			{
				mItemGroups[mItemsDatum.Key] = itemGroup;
			}
			else
			{
				mItemGroups.Add(mItemsDatum.Key, itemGroup);
			}
			if (mSelectedItemGroupId == -1 && mSelectedItemTypeId == -1 && mItemsDatum.Value.mItems.Count > 0)
			{
				OnSelectedItemGroup(mItemsDatum.Key);
				SetItemGroupState(mItemsDatum.Key);
				SetSelectedItemType(mItemsDatum.Value.mItems[0].Article.mKindId);
			}
		}
		SetItemTypesSize();
		SetGroupOffsets();
		SetCurPage(0);
	}

	public void Clear()
	{
		mSelfHero = null;
		mFormatedTipMgr = null;
		mMoneyHolder = null;
		if (mOkDialog != null)
		{
			mOkDialog.Clean();
		}
		if (mItemBuyMenu != null)
		{
			mItemBuyMenu.Clear();
			ItemBuyMenu itemBuyMenu = mItemBuyMenu;
			itemBuyMenu.mBuyRequestCallback = (BuyRequestCallback)Delegate.Remove(itemBuyMenu.mBuyRequestCallback, mBuyRequestCallback);
		}
	}

	private string GetGroupTipById(int _groupId)
	{
		return _groupId switch
		{
			1 => "TIP_TEXT46", 
			2 => "TIP_TEXT47", 
			3 => "TIP_TEXT48", 
			4 => "TIP_TEXT49", 
			5 => "TIP_TEXT50", 
			6 => "TIP_TEXT51", 
			7 => "TIP_TEXT52", 
			8 => "TIP_TEXT53", 
			9 => "TIP_TEXT54", 
			_ => string.Empty, 
		};
	}

	private CtrlPrototype GetItemDataById(int _id)
	{
		if (mSelfHero == null)
		{
			return null;
		}
		return mSelfHero.Inventory.TryGet(_id)?.CtrlProto;
	}

	private int GetFreeSlotNum()
	{
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (!mSellItem.Value.HasItem())
			{
				return mSellItem.Key;
			}
		}
		return -1;
	}

	private SellItem GetSellItemById(int _id)
	{
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (mSellItem.Value.HasItem() && mSellItem.Value.mId == _id)
			{
				return mSellItem.Value;
			}
		}
		return null;
	}

	private void ClearSellItemById(int _id)
	{
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (mSellItem.Value.HasItem() && mSellItem.Value.mId == _id)
			{
				mSellItems[mSellItem.Key].Uninit();
				break;
			}
		}
	}

	private void ClearSellItems()
	{
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (mSellItems[mSellItem.Key].HasItem())
			{
				mSellItems[mSellItem.Key].Uninit();
			}
		}
		SetSellCostMoney(0);
		if (mOnUpdateInventory != null)
		{
			mOnUpdateInventory();
		}
	}

	private void AddSellCostMoney(int _money)
	{
		mSellCost += _money;
		mSellCostMoney.SetMoney(mSellCost);
	}

	private void SetSellCostMoney(int _money)
	{
		mSellCost = _money;
		mSellCostMoney.SetMoney(mSellCost);
	}

	private SellItem GetSellItemBySlot(int _num)
	{
		if (mSellItems.TryGetValue(_num, out var value) && value.HasItem())
		{
			return value;
		}
		return null;
	}

	private Rect GetItemRect(int _num)
	{
		if (_num < 0 || _num >= 36)
		{
			return default(Rect);
		}
		int num = Mathf.FloorToInt((float)_num / 12f);
		int num2 = _num - num * 12;
		int num3 = 54;
		int num4 = 56;
		Rect _rect = new Rect(94 + num2 * num3, 160 + num * num4, 38f, 38f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		return _rect;
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
			GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.SHOP, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, _countAccept: true);
		}
	}

	private void SetCurPage(int _page)
	{
		mCurPage = _page;
		mCurPage = ((mCurPage >= 0) ? mCurPage : 0);
		mCurPage = ((mCurPage <= mMaxPages) ? mCurPage : mMaxPages);
		SetPageStr();
		SetPageButtonState();
	}

	private void SetPageButtonState()
	{
		mPageLeftButton.mLocked = mCurPage == 0;
		mPageRightButton.mLocked = mCurPage == mMaxPages;
	}

	private void SetPageStr()
	{
		mPagesStr = mCurPage + 1 + "/" + (mMaxPages + 1);
	}

	private void OnSelectedItemGroup(int _itemGroupId)
	{
		mSelectedItemGroupId = _itemGroupId;
	}

	private void SetItemGroupState(int _itemGroupId)
	{
		if (mItemGroups.TryGetValue(_itemGroupId, out var value))
		{
			value.Open = !value.Open;
			SetGroupOffsets();
		}
	}

	private void SetSelectedItemType(int _itemType)
	{
		mSelectedItemTypeId = _itemType;
		foreach (KeyValuePair<int, ItemGroup> mItemGroup in mItemGroups)
		{
			if (mItemGroup.Key == mSelectedItemGroupId || mSelectedItemTypeId == -1)
			{
				mItemGroup.Value.SetSelectedItemType(mSelectedItemTypeId);
			}
			else
			{
				mItemGroup.Value.SetSelectedItemType(-1);
			}
		}
		InitSelectedItems();
	}

	private void InitSelectedItems()
	{
		mShopItems.Clear();
		List<CtrlPrototype> itemsByGroupAndType = GetItemsByGroupAndType(mSelectedItemGroupId, mSelectedItemTypeId);
		mMaxPages = Mathf.FloorToInt(itemsByGroupAndType.Count / mMaxItemsOnPage);
		CtrlPrototypeWeigthComparer comparer = new CtrlPrototypeWeigthComparer();
		itemsByGroupAndType.Sort(comparer);
		foreach (CtrlPrototype item in itemsByGroupAndType)
		{
			AddItemToShop(item);
		}
		SetShopItemsSize();
		SetCurPage(0);
	}

	private List<CtrlPrototype> GetItemsByGroupAndType(int _itemGroupId, int _itemType)
	{
		List<CtrlPrototype> list = new List<CtrlPrototype>();
		List<CtrlPrototype> list2 = new List<CtrlPrototype>();
		foreach (KeyValuePair<int, CtrlItem> mItemsDatum in mItemsData)
		{
			if (mItemsDatum.Key == _itemGroupId)
			{
				list = mItemsDatum.Value.mItems;
				break;
			}
		}
		foreach (CtrlPrototype item in list)
		{
			if (item.Article.mKindId == _itemType && (!OptionsMgr.mShopFilterLvl || (OptionsMgr.mShopFilterLvl && mSelfHero.Hero.GameInfo.mLevel >= item.Article.mMinHeroLvl)))
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private void AddItemToShop(CtrlPrototype _item)
	{
		if (_item != null)
		{
			ShopItem shopItem = new ShopItem();
			shopItem.mOnBuyItem = (ShopItem.OnBuyItem)Delegate.Combine(shopItem.mOnBuyItem, new ShopItem.OnBuyItem(OnBuyItem));
			shopItem.SetData(mSelfHero, mFormatedTipMgr, mPopupInfo);
			shopItem.SetItemData(_item);
			shopItem.Init();
			mShopItems.Add(shopItem);
		}
	}

	private void SetShopItemsSize()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 264;
		int num5 = 135;
		int num6 = 261;
		int num7 = 85;
		foreach (ShopItem mShopItem in mShopItems)
		{
			mShopItem.mZoneRect = new Rect(num4 + num2 * num6, num5 + num3 * num7, 256f, 84f);
			GuiSystem.SetChildRect(mZoneRect, ref mShopItem.mZoneRect);
			mShopItem.SetSize();
			num++;
			if (num % 2 == 0)
			{
				if (num % mMaxItemsOnPage == 0)
				{
					num3 = 0;
					num2 = 0;
				}
				else
				{
					num3++;
					num2 = 0;
				}
			}
			else
			{
				num2++;
			}
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON")
		{
			Close();
		}
		else if (_sender.mElementId == "BUY_MODE_BUTTON")
		{
			SetShopMode(ShopMode.BUY);
		}
		else if (_sender.mElementId == "SELL_MODE_BUTTON")
		{
			SetShopMode(ShopMode.SELL);
		}
		else if (_sender.mElementId == "GROUP_BUTTON" && _buttonId == 0)
		{
			SetItemGroupState(_sender.mId);
		}
		else if (_sender.mElementId == "ITEM_TYPE_BUTTON" && _buttonId == 0)
		{
			SetSelectedItemType(_sender.mId);
		}
		else if (_sender.mElementId == "PAGE_LEFT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPage - 1);
		}
		else if (_sender.mElementId == "PAGE_RIGHT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPage + 1);
		}
		else if (_sender.mElementId == "BANK_BUTTON" && _buttonId == 0)
		{
			TryShowBank();
		}
		else if (_sender.mElementId == "SELL_BUTTON" && _buttonId == 0)
		{
			TrySellItems();
		}
		else if (_sender.mElementId == "CANCEL_SELL_BUTTON" && _buttonId == 0)
		{
			ClearSellItems();
		}
	}

	private void TrySellItems()
	{
		if (mSellRequestCallback == null)
		{
			Log.Error("sell callback is null");
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, SellItem> mSellItem in mSellItems)
		{
			if (mSellItem.Value.HasItem())
			{
				dictionary.Add(mSellItem.Value.mId, mSellItem.Value.ItemCount);
			}
		}
		mSellRequestCallback(dictionary);
		ClearSellItems();
		Close();
	}

	private void TryShowBank()
	{
		if (mOnShowBank != null)
		{
			mOnShowBank();
		}
	}

	private void OnBuyItem(CtrlPrototype _itemData)
	{
		if (_itemData != null)
		{
			mItemBuyMenu.SetItemData(_itemData);
			mItemBuyMenu.SetActive(_active: true);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetGroupOffsets();
		}
	}

	private void SetItemTypesSize()
	{
		foreach (KeyValuePair<int, ItemGroup> mItemGroup in mItemGroups)
		{
			mItemGroup.Value.mZoneRect = mZoneRect;
			mItemGroup.Value.mRenderZone = mGroupsZoneRect;
			mItemGroup.Value.SetSize();
		}
	}

	private void OnFilter(GuiElement _sender, bool _value)
	{
		if (_sender.mElementId == "LEVEL_FILTER_CHECKBOX")
		{
			OptionsMgr.mShopFilterLvl = _value;
			InitSelectedItems();
		}
	}

	private void SetGroupOffsets()
	{
		float num = 0f;
		float x = 64f;
		float num2 = 83f;
		float maxHeight = 465f;
		foreach (KeyValuePair<int, ItemGroup> mItemGroup in mItemGroups)
		{
			mItemGroup.Value.SetOffset(new Vector2(x, num2 + num - mScrollOffset));
			mItemGroup.Value.SetItemTypesSize();
			num += mItemGroup.Value.GetGroupLength();
		}
		mScrollbar.SetData(maxHeight, num);
	}

	private Texture2D GetFrame(ShopType _type, ShopMode _mode)
	{
		if (_type == ShopType.CS_SHOP)
		{
			switch (_mode)
			{
			case ShopMode.BUY:
				return mFrame1;
			case ShopMode.SELL:
				return mFrame2;
			}
		}
		return mFrame1;
	}

	private void SetShopMode(ShopMode _mode)
	{
		if (_mode != mShopMode)
		{
			mShopMode = _mode;
			ClearSellItems();
			SetShopParamsSize();
		}
		mCurFrame = GetFrame(mShopType, mShopMode);
		SetShopModeButtonState();
	}

	private void SetShopModeButtonState()
	{
		mBuyModeButton.Pressed = mShopMode == ShopMode.BUY;
		mSellModeButton.Pressed = mShopMode == ShopMode.SELL;
	}

	private void SetMoney()
	{
		if (mLastMoney != mMoneyHolder.VirtualMoney)
		{
			int _gold = 0;
			int _silver = 0;
			int _bronze = 0;
			ShopVendor.SetMoney(mMoneyHolder.VirtualMoney, ref _gold, ref _silver, ref _bronze);
			mGold = _gold.ToString();
			mSilver = _silver.ToString();
			mBronze = _bronze.ToString();
			mLastMoney = mMoneyHolder.VirtualMoney;
		}
		if (mLastDiamonds != mMoneyHolder.RealMoney)
		{
			mLastDiamonds = mMoneyHolder.RealMoney;
			mDiamonds = ((float)mMoneyHolder.RealMoney * 0.01f).ToString("0.##");
		}
	}
}
