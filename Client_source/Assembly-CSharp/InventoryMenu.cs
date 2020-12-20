using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class InventoryMenu : GuiInputListener, EscapeListener
{
	public class InventoryItem
	{
		public bool mChecked;

		public int mSlotNum;

		public bool mUsable;

		public GuiInputMgr.DraggableButton mButton;
	}

	public class InventoryTab
	{
		private Dictionary<int, InventoryItem> mItems = new Dictionary<int, InventoryItem>();

		public Dictionary<int, int> mItemsAtSlot = new Dictionary<int, int>();

		public static int mMaxItemsOnPage = 24;

		public static int mMaxPages = 99;

		public static int mItemsWidthCount = 4;

		public static int mItemsHeightCount = 6;

		public int mLastFreeSlot;

		public int mCurPage;

		public Dictionary<int, InventoryItem> Items => mItems;

		public void Close()
		{
			int num = mCurPage * mMaxItemsOnPage;
			int num2 = mCurPage * mMaxItemsOnPage + mMaxItemsOnPage;
			InventoryItem value = null;
			for (int i = num; i < num2; i++)
			{
				if (mItems.TryGetValue(i, out value))
				{
					value.mButton.mButton.mOnMouseLeave(value.mButton.mButton);
				}
			}
		}

		public void Clear()
		{
			Close();
			mItems.Clear();
			mItemsAtSlot.Clear();
		}

		public void RenderElement()
		{
			int num = mCurPage * mMaxItemsOnPage;
			int num2 = mCurPage * mMaxItemsOnPage + mMaxItemsOnPage;
			InventoryItem value = null;
			for (int i = num; i < num2; i++)
			{
				if (mItems.TryGetValue(i, out value))
				{
					value.mButton.mButton.RenderElement();
					GuiSystem.DrawString(value.mButton.mData.mCount.ToString(), value.mButton.mButton.mZoneRect, "lower_right");
				}
			}
		}

		public void CheckEvent(Event _curEvent)
		{
			int num = mCurPage * mMaxItemsOnPage;
			int num2 = mCurPage * mMaxItemsOnPage + mMaxItemsOnPage;
			InventoryItem value = null;
			for (int i = num; i < num2; i++)
			{
				if (mItems.TryGetValue(i, out value))
				{
					value.mButton.mButton.CheckEvent(_curEvent);
				}
			}
		}

		public void IncPageNum(bool _left)
		{
			if (_left)
			{
				mCurPage--;
			}
			else
			{
				mCurPage++;
			}
			mCurPage = ((mCurPage >= 0) ? mCurPage : 0);
			mCurPage = ((mCurPage < mMaxPages) ? mCurPage : (mMaxPages - 1));
		}

		public int GetSlotNum(Vector2 _mousePos)
		{
			if (!mSlotZoneRect.Contains(_mousePos))
			{
				return -1;
			}
			Rect rect = default(Rect);
			int result = -1;
			Vector2 zero = Vector2.zero;
			float num = float.PositiveInfinity;
			float num2 = 0f;
			for (int i = 0; i < mMaxItemsOnPage; i++)
			{
				rect = GetItemRect(i);
				zero.x = rect.x + rect.width / 2f;
				zero.y = rect.y + rect.height / 2f;
				if (rect.Contains(_mousePos))
				{
					return i + mCurPage * mMaxItemsOnPage;
				}
				num2 = (zero - _mousePos).magnitude;
				if (num2 < num)
				{
					num = num2;
					result = i + mCurPage * mMaxItemsOnPage;
				}
			}
			return result;
		}

		public void UpdateItemData(GuiInputMgr.DraggableButton _btn, int _cnt, int _slotNew, int _slotOld)
		{
			if (mItems.TryGetValue(_slotOld, out var value))
			{
				if (_slotNew != -1 && _slotNew != _slotOld)
				{
					value.mSlotNum = _slotNew;
					value.mButton.mButton.mZoneRect = GetItemRect(_slotNew);
					RemoveItem(_slotOld);
					AddItem(_slotNew, value);
				}
				if (_cnt > 0 && _btn.mDragFlag != GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG)
				{
					value.mButton.mData.mCount += _cnt;
				}
			}
		}

		public void AddItem(int _slot, InventoryItem _item)
		{
			mItems.Add(_slot, _item);
			int mId = _item.mButton.mButton.mId;
			if (mItemsAtSlot.ContainsKey(mId))
			{
				mItemsAtSlot[mId] = _slot;
			}
			else
			{
				mItemsAtSlot.Add(mId, _slot);
			}
		}

		public void RemoveItem(int _slot)
		{
			InventoryItem value = null;
			if (mItems.TryGetValue(_slot, out value))
			{
				GuiButton mButton = value.mButton.mButton;
				mButton.mOnMouseLeave(mButton);
				OptionsMgr.RemoveActiveItem(mButton.mId);
				mItems.Remove(_slot);
				mItemsAtSlot.Remove(mButton.mId);
				if (mLastFreeSlot > _slot)
				{
					mLastFreeSlot = _slot;
				}
			}
		}

		public InventoryItem GetItemById(int _id)
		{
			if (!mItemsAtSlot.TryGetValue(_id, out var value))
			{
				return null;
			}
			mItems.TryGetValue(value, out var value2);
			return value2;
		}
	}

	public delegate void UseCtrlItemCallback(int _itemId);

	public UseCtrlItemCallback mUseCtrlItemCallback;

	private Texture2D mFrameImage;

	private Thing.PlaceType mCurTab = Thing.PlaceType.AVATAR;

	private Dictionary<Thing.PlaceType, InventoryTab> mTabs = new Dictionary<Thing.PlaceType, InventoryTab>();

	private Dictionary<Thing.PlaceType, GuiButton> mTabButtons = new Dictionary<Thing.PlaceType, GuiButton>();

	private GuiButton mScrollButtonL;

	private GuiButton mScrollButtonR;

	private Rect mPageNumRect;

	private int mMoney;

	private int mGold;

	private int mSilver;

	private int mBronze;

	private Rect mGoldRect;

	private Rect mSilverRect;

	private Rect mBronzeRect;

	private GuiButton mCloseButton;

	private static List<Rect> mSlotRects;

	private static Rect mSlotZoneRect;

	private ShopGUI mShopWnd;

	private ShopGUI mShopRealWnd;

	private PlayerTradeMenu mTradeMenu;

	private HeroInfo mHeroInfo;

	private GuiButton mUsableButton;

	private FormatedTipMgr mFormatedTipMgr;

	private PlayerControl mPlayerCtrl;

	private IEnumerable<Thing> mItems;

	private List<GuiInputMgr.DraggableButton> mPotionsToDrag;

	private IMoneyHolder mMoneyHolder;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(80, this);
		mFrameImage = GuiSystem.GetImage("Gui/Inventory/inventory_frame1");
		GuiSystem.mGuiInputMgr.AddGuiDraggableButtonListener(this);
		mTabs = new Dictionary<Thing.PlaceType, InventoryTab>();
		mTabButtons = new Dictionary<Thing.PlaceType, GuiButton>();
		mSlotRects = new List<Rect>();
		mDragType = GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG;
		string empty = string.Empty;
		foreach (int value in Enum.GetValues(typeof(Thing.PlaceType)))
		{
			empty = "Gui/Inventory/button_" + (value + 1);
			GuiButton guiButton = new GuiButton();
			guiButton.mId = value;
			guiButton.mElementId = "TAB_BUTTON";
			guiButton.mNormImg = GuiSystem.GetImage(empty + "_norm");
			guiButton.mOverImg = GuiSystem.GetImage(empty + "_over");
			guiButton.mPressImg = GuiSystem.GetImage(empty + "_press");
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			mTabButtons.Add((Thing.PlaceType)value, guiButton);
			string mElementId = guiButton.mElementId;
			int num = value;
			AddTutorialElement(mElementId + "_" + num, guiButton);
		}
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "INVENTORY_CLOSE_BUTTON";
		GuiButton guiButton2 = mCloseButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mScrollButtonL = GuiSystem.CreateButton("Gui/misc/button_15_norm", "Gui/misc/button_15_over", "Gui/misc/button_15_press", string.Empty, string.Empty);
		mScrollButtonL.mElementId = "SCROLL_LEFT";
		GuiButton guiButton3 = mScrollButtonL;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollButtonL.Init();
		AddTutorialElement(mScrollButtonL);
		mScrollButtonR = GuiSystem.CreateButton("Gui/misc/button_16_norm", "Gui/misc/button_16_over", "Gui/misc/button_16_press", string.Empty, string.Empty);
		mScrollButtonR.mElementId = "SCROLL_RIGHT";
		GuiButton guiButton4 = mScrollButtonR;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollButtonR.Init();
		AddTutorialElement(mScrollButtonR);
		mShopRealWnd = GuiSystem.mGuiSystem.GetGuiElement<ShopGUI>("central_square", "SHOP_REAL_WINDOW");
		mShopWnd = GuiSystem.mGuiSystem.GetGuiElement<ShopGUI>("central_square", "SHOP_WINDOW");
		mTradeMenu = GuiSystem.mGuiSystem.GetGuiElement<PlayerTradeMenu>("central_square", "TRADE_MENU");
		mHeroInfo = GuiSystem.mGuiSystem.GetGuiElement<HeroInfo>("central_square", "HERO_MENU");
		foreach (int value2 in Enum.GetValues(typeof(Thing.PlaceType)))
		{
			mTabs.Add((Thing.PlaceType)value2, new InventoryTab());
		}
		mPotionsToDrag = new List<GuiInputMgr.DraggableButton>();
		SetPageButtonState();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImage.width, mFrameImage.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		int num = 0;
		foreach (KeyValuePair<Thing.PlaceType, GuiButton> mTabButton in mTabButtons)
		{
			mTabButton.Value.mZoneRect.x = 29 + num * 49;
			mTabButton.Value.mZoneRect.y = 5f;
			mTabButton.Value.mZoneRect.width = 38f;
			mTabButton.Value.mZoneRect.height = 38f;
			GuiSystem.SetChildRect(mZoneRect, ref mTabButton.Value.mZoneRect);
			num++;
		}
		mCloseButton.mZoneRect = new Rect(175f, 21f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mGoldRect = new Rect(66f, 363f, 61f, 15f);
		mSilverRect = new Rect(138f, 363f, 21f, 15f);
		mBronzeRect = new Rect(170f, 363f, 21f, 15f);
		mScrollButtonL.mZoneRect = new Rect(63f, 52f, 15f, 15f);
		mScrollButtonR.mZoneRect = new Rect(118f, 52f, 15f, 15f);
		mPageNumRect = new Rect(78f, 50f, 40f, 15f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollButtonL.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollButtonR.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mPageNumRect);
		GuiSystem.SetChildRect(mZoneRect, ref mGoldRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSilverRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBronzeRect);
		mSlotZoneRect = new Rect(12f, 77f, 184f, 288f);
		GuiSystem.SetChildRect(mZoneRect, ref mSlotZoneRect);
		InitSlotRects();
		SetItemsSize();
		PopupInfo.AddTip(this, "TIP_TEXT20", mTabButtons[Thing.PlaceType.HERO].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT21", mTabButtons[Thing.PlaceType.AVATAR].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT22", mTabButtons[Thing.PlaceType.QUEST].mZoneRect);
		Rect _rect = new Rect(63f, 52f, 70f, 15f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT23", _rect);
		_rect = new Rect(106f, 363f, 100f, 15f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT24", _rect);
	}

	private void SetItemsSize()
	{
		foreach (InventoryTab value in mTabs.Values)
		{
			foreach (KeyValuePair<int, InventoryItem> item in value.Items)
			{
				item.Value.mButton.mButton.mZoneRect = GetItemRect(item.Key);
			}
		}
	}

	public override void Update()
	{
		int num = ((mMoneyHolder != null) ? mMoneyHolder.VirtualMoney : 0);
		if (num != mMoney)
		{
			mMoney = num;
			ShopVendor.SetMoney(mMoney, ref mGold, ref mSilver, ref mBronze);
		}
	}

	public void Clear()
	{
		foreach (InventoryTab value in mTabs.Values)
		{
			value.mCurPage = 0;
			value.Clear();
		}
		SetCurTab(Thing.PlaceType.AVATAR);
		if (mPotionsToDrag != null)
		{
			mPotionsToDrag.Clear();
		}
		SetPageButtonState();
	}

	public void SetItems(IEnumerable<Thing> _items)
	{
		mUsableButton = null;
		mItems = _items;
		foreach (InventoryTab value in mTabs.Values)
		{
			value.mLastFreeSlot = 0;
			foreach (KeyValuePair<int, InventoryItem> item2 in value.Items)
			{
				item2.Value.mChecked = false;
			}
		}
		foreach (Thing _item in _items)
		{
			Thing.PlaceType place = _item.Place;
			int availCount = _item.AvailCount;
			InventoryItem item = GetItem(place, _item.Id);
			if (availCount == 0)
			{
				continue;
			}
			if (item != null)
			{
				item.mChecked = true;
				item.mButton.mData.mCount = availCount;
				continue;
			}
			item = new InventoryItem();
			item.mChecked = true;
			item.mSlotNum = GetFreeSlotNum(place);
			if (item.mSlotNum == -1)
			{
				continue;
			}
			item.mUsable = _item.CtrlProto.Article.mAction != null;
			Prototype.PDesc pDesc = null;
			pDesc = ((!(_item is BattleThing)) ? _item.CtrlProto.Desc : (_item as BattleThing).BattleProto.Desc);
			int mOrigin = -1;
			bool flag = _item is BattleThing;
			bool flag2 = _item is CtrlThing;
			if (flag)
			{
				mOrigin = 1;
			}
			else if (flag2)
			{
				mOrigin = 2;
			}
			item.mButton = new GuiInputMgr.DraggableButton();
			item.mButton.mDragFlag = mDragType;
			item.mButton.mData = new GuiInputMgr.DragItemData();
			item.mButton.mData.mCount = _item.Count;
			item.mButton.mData.mOrigin = mOrigin;
			item.mButton.mData.mArticleId = _item.CtrlProto.Id;
			if (flag)
			{
				BattleThing battleThing = _item as BattleThing;
				if (battleThing.BattleProto.Item.mBattleItemType == BattleItemType.WEARABLE)
				{
					item.mButton.mObjectType = GuiInputMgr.ObjectType.ITEM;
				}
				else if (battleThing.BattleProto.Item.mBattleItemType == BattleItemType.CONSUMABLE)
				{
					item.mButton.mObjectType = GuiInputMgr.ObjectType.POTION;
					if (mPlayerCtrl.SelfPlayer.Player.Hero.GameInfo.mLevel != -1)
					{
						item.mButton.mData.mUndraggable = _item.CtrlProto.Article.mMinHeroLvl > mPlayerCtrl.SelfPlayer.Player.Hero.GameInfo.mLevel;
					}
					int activeItemNum = OptionsMgr.GetActiveItemNum(_item.CtrlProto.Id);
					if (activeItemNum != -1 && !item.mButton.mData.mUndraggable)
					{
						mPotionsToDrag.Add(item.mButton);
					}
				}
			}
			InitItemButton(ref item, _item.Id, pDesc.mIcon);
			mTabs[place].AddItem(item.mSlotNum, item);
		}
		UnsetItems();
		CheckPotions();
	}

	public override bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _slot)
	{
		if (_btn == null)
		{
			Log.Error("Trying to add null dragged button");
			return false;
		}
		Thing.PlaceType placeType = mCurTab;
		if (_btn.mObjectType == GuiInputMgr.ObjectType.POTION)
		{
			placeType = Thing.PlaceType.AVATAR;
		}
		else if (_btn.mObjectType == GuiInputMgr.ObjectType.ITEM)
		{
			placeType = Thing.PlaceType.HERO;
		}
		InventoryItem itemBySlot = GetItemBySlot(placeType, _slot);
		if (itemBySlot != null)
		{
			return false;
		}
		InventoryItem item = GetItem(placeType, _btn.mButton.mId);
		if (item != null)
		{
			UpdateItemInfo(_btn, _cnt, _slot, item.mSlotNum);
			return true;
		}
		_slot = ((_slot != -1) ? _slot : GetFreeSlotNum(placeType));
		if (_slot == -1)
		{
			return false;
		}
		item = new InventoryItem();
		item.mSlotNum = _slot;
		GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
		draggableButton.mButtonHolder = this;
		draggableButton.mDragFlag = mDragType;
		draggableButton.mObjectType = _btn.mObjectType;
		draggableButton.mData = new GuiInputMgr.DragItemData();
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
		mButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
		GuiButton mButton3 = draggableButton.mButton;
		mButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
		GuiButton mButton4 = draggableButton.mButton;
		mButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton4.mOnMouseUp, new OnMouseUp(OnItem));
		draggableButton.mButton.mZoneRect = GetItemRect(_slot);
		draggableButton.mButton.Init();
		item.mButton = draggableButton;
		mTabs[placeType].AddItem(item.mSlotNum, item);
		return true;
	}

	public void UpdatePotions()
	{
		if (!mTabs.TryGetValue(Thing.PlaceType.AVATAR, out var value))
		{
			return;
		}
		GuiInputMgr.DraggableButton draggableButton = null;
		foreach (KeyValuePair<int, InventoryItem> item in value.Items)
		{
			draggableButton = item.Value.mButton;
			if (draggableButton.mObjectType == GuiInputMgr.ObjectType.POTION && !draggableButton.mData.mUndraggable)
			{
				int activeItemNum = OptionsMgr.GetActiveItemNum(draggableButton.mData.mArticleId);
				if (activeItemNum != -1)
				{
					mPotionsToDrag.Add(draggableButton);
				}
			}
		}
		CheckPotions();
	}

	private void UpdateItemInfo(GuiInputMgr.DraggableButton _btn, int _cnt, int _slotNew, int slotOld)
	{
		if (mTabs.TryGetValue(mCurTab, out var value))
		{
			value.UpdateItemData(_btn, _cnt, _slotNew, slotOld);
		}
	}

	private void CheckPotions()
	{
		foreach (GuiInputMgr.DraggableButton item in mPotionsToDrag)
		{
			GuiSystem.mGuiInputMgr.AddItemToMove(item, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG, _countAccept: false);
		}
		mPotionsToDrag.Clear();
	}

	private void UnsetItems()
	{
		List<int> list = new List<int>();
		foreach (InventoryTab value in mTabs.Values)
		{
			foreach (KeyValuePair<int, InventoryItem> item in value.Items)
			{
				if (!item.Value.mChecked)
				{
					if (item.Value.mButton.mObjectType == GuiInputMgr.ObjectType.POTION)
					{
						GuiSystem.mGuiInputMgr.AddItemToMove(item.Value.mButton, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG, -1, _countAccept: false);
					}
					list.Add(item.Key);
				}
			}
			foreach (int item2 in list)
			{
				value.RemoveItem(item2);
			}
			list.Clear();
		}
	}

	private InventoryItem GetItem(Thing.PlaceType _tab, int _id)
	{
		InventoryTab value = null;
		if (!mTabs.TryGetValue(_tab, out value))
		{
			return null;
		}
		return value.GetItemById(_id);
	}

	private InventoryItem GetItemBySlot(Thing.PlaceType _tab, int _slot)
	{
		InventoryTab value = null;
		if (!mTabs.TryGetValue(_tab, out value))
		{
			return null;
		}
		InventoryItem value2 = null;
		value.Items.TryGetValue(_slot, out value2);
		return value2;
	}

	private int GetFreeSlotNum(Thing.PlaceType _tab)
	{
		InventoryTab value = null;
		if (!mTabs.TryGetValue(_tab, out value))
		{
			return -1;
		}
		InventoryItem value2 = null;
		int i = value.mLastFreeSlot;
		for (int num = InventoryTab.mMaxItemsOnPage * InventoryTab.mMaxPages; i < num; i++)
		{
			if (!value.Items.TryGetValue(i, out value2))
			{
				value.mLastFreeSlot = i;
				return i;
			}
		}
		return -1;
	}

	private void InitItemButton(ref InventoryItem _item, int _id, string _icon)
	{
		_item.mButton.mButton = new GuiButton();
		_item.mButton.mButton.mElementId = "INVENTORY_ITEM_BUTTON";
		_item.mButton.mButton.mId = _id;
		SetItemPos(ref _item);
		_item.mButton.mButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + _icon);
		_item.mButton.mButton.mIconOnTop = false;
		GuiButton mButton = _item.mButton.mButton;
		mButton.mOnDragStart = (OnDragStart)Delegate.Combine(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
		GuiButton mButton2 = _item.mButton.mButton;
		mButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
		GuiButton mButton3 = _item.mButton.mButton;
		mButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
		GuiButton mButton4 = _item.mButton.mButton;
		mButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton4.mOnMouseUp, new OnMouseUp(OnItem));
		if (_item.mUsable)
		{
			GuiButton mButton5 = _item.mButton.mButton;
			mButton5.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton5.mOnMouseEnter, new OnMouseEnter(OnUsableItemEnter));
			GuiButton mButton6 = _item.mButton.mButton;
			mButton6.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton6.mOnMouseLeave, new OnMouseLeave(OnUsableItemLeave));
		}
		_item.mButton.mButton.mDraggableButton = _item.mButton;
		_item.mButton.mButton.Init();
		_item.mButton.mButtonHolder = this;
		if (_item.mButton.mData.mUndraggable)
		{
			_item.mButton.mButton.mEffectImg = GuiSystem.GetImage("Gui/misc/item_not_available");
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr == null)
		{
			return;
		}
		Thing itemById = GetItemById(_sender.mId);
		if (itemById == null)
		{
			return;
		}
		Player player = mPlayerCtrl.SelfPlayer.Player;
		if (player == null || !player.IsAvatarBinded)
		{
			return;
		}
		IGameObject avatar = player.Avatar;
		if (avatar != null)
		{
			InstanceData data = (avatar as GameData).Data;
			BattleThing battleThing = itemById as BattleThing;
			int avaLvl = data.Level + 1;
			int mLevel = mPlayerCtrl.SelfPlayer.Player.Hero.GameInfo.mLevel;
			if (battleThing != null)
			{
				mFormatedTipMgr.Show(battleThing.BattleProto, itemById.CtrlProto, avaLvl, mLevel, _sender.UId, true);
			}
			else
			{
				mFormatedTipMgr.Show(null, itemById.CtrlProto, avaLvl, mLevel, _sender.UId, true);
			}
		}
	}

	private Thing GetItemById(int _id)
	{
		foreach (Thing mItem in mItems)
		{
			if (mItem.Id == _id)
			{
				return mItem;
			}
		}
		return null;
	}

	private void OnItemMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && !_sender.mLocked)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	public void SetItemPos(ref InventoryItem _item)
	{
		_item.mButton.mButton.mZoneRect = GetItemRect(_item.mSlotNum);
	}

	public static Rect GetItemRect(int _slotNum)
	{
		if (_slotNum >= InventoryTab.mMaxItemsOnPage)
		{
			int num = Mathf.FloorToInt((float)_slotNum / (float)InventoryTab.mMaxItemsOnPage);
			_slotNum -= num * InventoryTab.mMaxItemsOnPage;
		}
		if (mSlotRects.Count < _slotNum || _slotNum == -1)
		{
			return default(Rect);
		}
		return mSlotRects[_slotNum];
	}

	private Rect GetSlotRect(int _slotNum)
	{
		Rect _rect = default(Rect);
		int num = (int)Mathf.Floor((float)_slotNum / (float)InventoryTab.mItemsWidthCount);
		int num2 = _slotNum - num * InventoryTab.mItemsWidthCount;
		_rect.x = 12 + num2 * 46;
		_rect.y = 77 + num * 48;
		_rect.width = 38f;
		_rect.height = 38f;
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		return _rect;
	}

	private void InitSlotRects()
	{
		mSlotRects.Clear();
		for (int i = 0; i < InventoryTab.mMaxItemsOnPage; i++)
		{
			mSlotRects.Add(GetSlotRect(i));
		}
	}

	public override void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
		GuiInputMgr.DraggableButton draggableButton = null;
		foreach (InventoryTab value in mTabs.Values)
		{
			foreach (KeyValuePair<int, InventoryItem> item in value.Items)
			{
				draggableButton = item.Value.mButton;
				if (draggableButton.mButton.UId == _btn.mButton.UId)
				{
					draggableButton.mData.mCount -= _cnt;
					if (draggableButton.mData.mCount <= 0)
					{
						value.RemoveItem(item.Key);
					}
					break;
				}
			}
		}
	}

	public void OnBtnDragStart(GuiElement _sender)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null)
		{
			GuiSystem.mGuiInputMgr.AddDraggableButton(guiButton.mDraggableButton);
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mUsableButton != null)
		{
			mUsableButton.CheckEvent(_curEvent);
		}
		mTabs[mCurTab].CheckEvent(_curEvent);
		foreach (GuiButton value in mTabButtons.Values)
		{
			value.CheckEvent(_curEvent);
		}
		mCloseButton.CheckEvent(_curEvent);
		mScrollButtonL.CheckEvent(_curEvent);
		mScrollButtonR.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImage, mZoneRect);
		mTabs[mCurTab].RenderElement();
		foreach (GuiButton value in mTabButtons.Values)
		{
			value.RenderElement();
		}
		GuiSystem.DrawString(mGold.ToString(), mGoldRect, "middle_center");
		GuiSystem.DrawString(mSilver.ToString(), mSilverRect, "middle_left");
		GuiSystem.DrawString(mBronze.ToString(), mBronzeRect, "middle_left");
		mCloseButton.RenderElement();
		mScrollButtonL.RenderElement();
		mScrollButtonR.RenderElement();
		GuiSystem.DrawString(mTabs[mCurTab].mCurPage + 1 + "/" + InventoryTab.mMaxPages, mPageNumRect, "middle_center");
		if (mUsableButton != null)
		{
			mUsableButton.RenderElement();
		}
		base.RenderElement();
	}

	public override int GetZoneNum(Vector2 _mousePos)
	{
		return mTabs[mCurTab].GetSlotNum(_mousePos);
	}

	public void Close()
	{
		if (mTabs.ContainsKey(mCurTab))
		{
			mTabs[mCurTab].Close();
		}
		if (mShopWnd != null && mTradeMenu != null && mShopRealWnd != null)
		{
			SetActive(mShopWnd.Active || mTradeMenu.Active || mShopRealWnd.Active);
		}
		else
		{
			SetActive(_active: false);
		}
	}

	public void Open()
	{
		SetActive(_active: true);
		SetCurTab(mCurTab);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "INVENTORY_CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "TAB_BUTTON" && _buttonId == 0)
		{
			if (mTabButtons.ContainsKey((Thing.PlaceType)_sender.mId))
			{
				SetCurTab((Thing.PlaceType)_sender.mId);
			}
		}
		else if (("SCROLL_LEFT" == _sender.mElementId || "SCROLL_RIGHT" == _sender.mElementId) && _buttonId == 0)
		{
			mTabs[mCurTab].IncPageNum("SCROLL_LEFT" == _sender.mElementId);
			SetPageButtonState();
		}
		else if (_sender.mElementId == "USABLE_BUTTON" && _buttonId == 0 && mUseCtrlItemCallback != null)
		{
			mUseCtrlItemCallback(_sender.mId);
		}
	}

	private void SetCurTab(Thing.PlaceType _type)
	{
		mCurTab = _type;
		foreach (KeyValuePair<Thing.PlaceType, GuiButton> mTabButton in mTabButtons)
		{
			mTabButton.Value.Pressed = mTabButton.Key == mCurTab;
			mTabButton.Value.SetCurBtnImage();
		}
		SetPageButtonState();
	}

	private void OnItem(GuiElement _sender, int _buttonId)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton == null || _buttonId != 1)
		{
			return;
		}
		if (mCurTab == Thing.PlaceType.HERO)
		{
			if (mHeroInfo != null && mHeroInfo.Active)
			{
				GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.HERO_INFO_DRAG, _countAccept: false);
			}
			else if (mShopWnd != null && mShopWnd.Active)
			{
				GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.SHOP, _countAccept: true);
			}
		}
		else if (mCurTab == Thing.PlaceType.AVATAR)
		{
			if (mShopWnd != null && !mShopWnd.Active)
			{
				GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG, _countAccept: false);
			}
			else if (mShopWnd != null && mShopWnd.Active)
			{
				GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, GuiInputMgr.ButtonDragFlags.SHOP, _countAccept: true);
			}
		}
	}

	private void SetPageButtonState()
	{
		if (mTabs.ContainsKey(mCurTab))
		{
			mScrollButtonL.mLocked = mTabs[mCurTab].mCurPage == 0;
			mScrollButtonR.mLocked = mTabs[mCurTab].mCurPage == InventoryTab.mMaxPages - 1;
		}
	}

	private void OnUsableItemEnter(GuiElement _sender)
	{
		if (mUsableButton == null)
		{
			mUsableButton = GuiSystem.CreateButton("Gui/Inventory/active_item_icon", "Gui/Inventory/active_item_icon", "Gui/Inventory/active_item_icon_active", string.Empty, string.Empty);
			mUsableButton.mElementId = "USABLE_BUTTON";
			mUsableButton.mId = _sender.mId;
			mUsableButton.mZoneRect = _sender.mZoneRect;
			GuiButton guiButton = mUsableButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			GuiButton guiButton2 = mUsableButton;
			guiButton2.mOnDragStart = (OnDragStart)Delegate.Combine(guiButton2.mOnDragStart, new OnDragStart(OnUsableItemDragStart));
			mUsableButton.Init();
		}
	}

	private void OnUsableItemLeave(GuiElement _sender)
	{
		if (mUsableButton != null)
		{
			GuiButton guiButton = mUsableButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Remove(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mUsableButton = null;
		}
	}

	private void OnUsableItemDragStart(GuiElement _sender)
	{
		InventoryItem item = GetItem(mCurTab, _sender.mId);
		if (item != null)
		{
			GuiSystem.mGuiInputMgr.AddDraggableButton(item.mButton);
		}
	}

	public void SetData(IMoneyHolder _holder, FormatedTipMgr _tipMgr, PlayerControl _playerCtrl)
	{
		mMoneyHolder = _holder;
		mFormatedTipMgr = _tipMgr;
		mPlayerCtrl = _playerCtrl;
	}

	public void Clean()
	{
		Clear();
		mMoneyHolder = null;
		mFormatedTipMgr = null;
		mPlayerCtrl = null;
	}
}
