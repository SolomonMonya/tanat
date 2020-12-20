using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class PlayerTradeMenu : GuiInputListener, EscapeListener
{
	public delegate void CancelCallback();

	public delegate void ReadyCallback(bool _ready, int _money, Dictionary<int, int> _items);

	public delegate void BarterCallback();

	public CancelCallback mCancelCallback;

	public ReadyCallback mReadyCallback;

	public BarterCallback mBarterCallback;

	private bool mStatus;

	private bool mEnemyStatus;

	private Rect mEnemyStatusRect = default(Rect);

	private string mReadyString = string.Empty;

	private string mNotReadyString = string.Empty;

	private string mEnemyStatusString = string.Empty;

	private Texture2D mFrameImg;

	private GuiButton mCloseButton;

	private GuiButton mCancelButton;

	private GuiButton mReadyButton;

	private GuiButton mBarterButton;

	private GuiButton mUpButton1;

	private GuiButton mUpButton2;

	private GuiButton mUpButton3;

	private GuiButton mDownButton1;

	private GuiButton mDownButton2;

	private GuiButton mDownButton3;

	private Rect mTaxRect1 = default(Rect);

	private Rect mTaxRect2 = default(Rect);

	private Rect mTaxRect3 = default(Rect);

	private Rect mCostRect1 = default(Rect);

	private Rect mCostRect2 = default(Rect);

	private Rect mCostRect3 = default(Rect);

	private Rect mTraderCostRect1 = default(Rect);

	private Rect mTraderCostRect2 = default(Rect);

	private Rect mTraderCostRect3 = default(Rect);

	private int mTax;

	private int mTaxGold;

	private int mTaxSilver;

	private int mTaxBronze;

	private int mCost;

	private int mCostGold;

	private int mCostSilver;

	private int mCostBronze;

	private int mTraderCost;

	private int mTraderCostGold;

	private int mTraderCostSilver;

	private int mTraderCostBronze;

	private IStoreContentProvider<CtrlPrototype> mPrototypes;

	private FormatedTipMgr mFormatedTipMgr;

	private OkDialog mOkDialog;

	private Dictionary<int, List<Rect>> mRectZones = new Dictionary<int, List<Rect>>();

	private List<GuiInputMgr.DraggableButton> mItems = new List<GuiInputMgr.DraggableButton>();

	private List<GuiButton> mEnemyItems = new List<GuiButton>();

	public void SetData(IStoreContentProvider<CtrlPrototype> _protos, FormatedTipMgr _tipMgr, OkDialog _okDialog)
	{
		mPrototypes = _protos;
		mFormatedTipMgr = _tipMgr;
		mOkDialog = _okDialog;
	}

	public void Clean()
	{
		mPrototypes = null;
		mFormatedTipMgr = null;
		mOkDialog = null;
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
		mDragType = GuiInputMgr.ButtonDragFlags.PLAYER_TRADE_MENU_DRAG;
		GuiSystem.mGuiInputMgr.AddGuiDraggableButtonListener(this);
		GuiSystem.mGuiInputMgr.AddEscapeListener(250, this);
		mFrameImg = GuiSystem.GetImage("Gui/PlayerTradeMenu/tab_01");
		mReadyString = GuiSystem.GetLocaleText("SELECT_AVATAR_READY_TEXT");
		mNotReadyString = GuiSystem.GetLocaleText("SELECT_AVATAR_NOT_READY_TEXT");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "TRADE_CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton.mOnMouseDown, new OnMouseDown(OnButton));
		mCloseButton.Init();
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "TRADE_CANCEL_BUTTON";
		GuiButton guiButton2 = mCancelButton;
		guiButton2.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton2.mOnMouseDown, new OnMouseDown(OnButton));
		mCancelButton.mLabel = GuiSystem.GetLocaleText("CANCEL_TEXT");
		mCancelButton.Init();
		mReadyButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mReadyButton.mElementId = "TRADE_READY_BUTTON";
		GuiButton guiButton3 = mReadyButton;
		guiButton3.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton3.mOnMouseDown, new OnMouseDown(OnButton));
		mReadyButton.mLabel = GuiSystem.GetLocaleText("SELECT_AVATAR_READY_TEXT");
		mReadyButton.Init();
		mBarterButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBarterButton.mElementId = "TRADE_BARTER_BUTTON";
		GuiButton guiButton4 = mBarterButton;
		guiButton4.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton4.mOnMouseDown, new OnMouseDown(OnButton));
		mBarterButton.mLabel = GuiSystem.GetLocaleText("TRADE_TEXT");
		mBarterButton.mLocked = true;
		mBarterButton.Init();
		mUpButton1 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn4", string.Empty, string.Empty);
		mUpButton1.mElementId = "UP_BUTTON_1";
		GuiButton guiButton5 = mUpButton1;
		guiButton5.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton5.mOnMouseDown, new OnMouseDown(OnButton));
		mUpButton1.Init();
		mUpButton2 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn4", string.Empty, string.Empty);
		mUpButton2.mElementId = "UP_BUTTON_2";
		GuiButton guiButton6 = mUpButton2;
		guiButton6.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton6.mOnMouseDown, new OnMouseDown(OnButton));
		mUpButton2.Init();
		mUpButton3 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn2", "Gui/PlayerTradeMenu/kn4", string.Empty, string.Empty);
		mUpButton3.mElementId = "UP_BUTTON_3";
		GuiButton guiButton7 = mUpButton3;
		guiButton7.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton7.mOnMouseDown, new OnMouseDown(OnButton));
		mUpButton3.Init();
		mDownButton1 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn3", string.Empty, string.Empty);
		mDownButton1.mElementId = "DOWN_BUTTON_1";
		GuiButton guiButton8 = mDownButton1;
		guiButton8.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton8.mOnMouseDown, new OnMouseDown(OnButton));
		mDownButton1.Init();
		mDownButton2 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn3", string.Empty, string.Empty);
		mDownButton2.mElementId = "DOWN_BUTTON_2";
		GuiButton guiButton9 = mDownButton2;
		guiButton9.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton9.mOnMouseDown, new OnMouseDown(OnButton));
		mDownButton2.Init();
		mDownButton3 = GuiSystem.CreateButton("Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn1", "Gui/PlayerTradeMenu/kn3", string.Empty, string.Empty);
		mDownButton3.mElementId = "DOWN_BUTTON_3";
		GuiButton guiButton10 = mDownButton3;
		guiButton10.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton10.mOnMouseDown, new OnMouseDown(OnButton));
		mDownButton3.Init();
		SetStatus(mStatus);
		SetEnemyStatus(mEnemyStatus);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mCloseButton.mZoneRect = new Rect(415f, 5f, 23f, 23f);
		mCancelButton.mZoneRect = new Rect(216f, 331f, 111f, 28f);
		mReadyButton.mZoneRect = new Rect(46f, 301f, 76f, 28f);
		mBarterButton.mZoneRect = new Rect(105f, 331f, 111f, 28f);
		mTaxRect1 = new Rect(225f, 307f, 22f, 11f);
		mTaxRect2 = new Rect(257f, 307f, 22f, 11f);
		mTaxRect3 = new Rect(289f, 307f, 22f, 11f);
		mCostRect1 = new Rect(44f, 278f, 29f, 11f);
		mCostRect2 = new Rect(107f, 278f, 29f, 11f);
		mCostRect3 = new Rect(170f, 278f, 29f, 11f);
		mTraderCostRect1 = new Rect(336f, 278f, 22f, 11f);
		mTraderCostRect2 = new Rect(368f, 278f, 22f, 11f);
		mTraderCostRect3 = new Rect(400f, 278f, 22f, 11f);
		mEnemyStatusRect = new Rect(310f, 301f, 76f, 28f);
		mUpButton1.mZoneRect = new Rect(75f, 274f, 12f, 12f);
		mUpButton2.mZoneRect = new Rect(137f, 274f, 12f, 12f);
		mUpButton3.mZoneRect = new Rect(199f, 274f, 12f, 12f);
		mDownButton1.mZoneRect = new Rect(75f, 286f, 12f, 12f);
		mDownButton2.mZoneRect = new Rect(137f, 286f, 12f, 12f);
		mDownButton3.mZoneRect = new Rect(199f, 286f, 12f, 12f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mReadyButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBarterButton.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mTaxRect1);
		GuiSystem.SetChildRect(mZoneRect, ref mTaxRect2);
		GuiSystem.SetChildRect(mZoneRect, ref mTaxRect3);
		GuiSystem.SetChildRect(mZoneRect, ref mCostRect1);
		GuiSystem.SetChildRect(mZoneRect, ref mCostRect2);
		GuiSystem.SetChildRect(mZoneRect, ref mCostRect3);
		GuiSystem.SetChildRect(mZoneRect, ref mTraderCostRect1);
		GuiSystem.SetChildRect(mZoneRect, ref mTraderCostRect2);
		GuiSystem.SetChildRect(mZoneRect, ref mTraderCostRect3);
		GuiSystem.SetChildRect(mZoneRect, ref mEnemyStatusRect);
		GuiSystem.SetChildRect(mZoneRect, ref mUpButton1.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mUpButton2.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mUpButton3.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDownButton1.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDownButton2.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDownButton3.mZoneRect);
		SetRectZoneSizes();
		Rect _rect = new Rect(35f, 278f, 150f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT35", _rect);
		_rect = new Rect(320f, 278f, 120f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT36", _rect);
		PopupInfo.AddTip(this, "TIP_TEXT37", mReadyButton.mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT38", mEnemyStatusRect);
		_rect = new Rect(210f, 307f, 95f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		PopupInfo.AddTip(this, "TIP_TEXT39", _rect);
		PopupInfo.AddTip(this, "TIP_TEXT40", mBarterButton.mZoneRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		mCloseButton.RenderElement();
		mCancelButton.RenderElement();
		mReadyButton.RenderElement();
		mBarterButton.RenderElement();
		mUpButton1.RenderElement();
		mUpButton2.RenderElement();
		mUpButton3.RenderElement();
		mDownButton1.RenderElement();
		mDownButton2.RenderElement();
		mDownButton3.RenderElement();
		GuiSystem.DrawString(mTaxGold.ToString(), mTaxRect1, "middle_center");
		GuiSystem.DrawString(mTaxSilver.ToString(), mTaxRect2, "middle_center");
		GuiSystem.DrawString(mTaxBronze.ToString(), mTaxRect3, "middle_center");
		GuiSystem.DrawString(mCostGold.ToString(), mCostRect1, "middle_center");
		GuiSystem.DrawString(mCostSilver.ToString(), mCostRect2, "middle_center");
		GuiSystem.DrawString(mCostBronze.ToString(), mCostRect3, "middle_center");
		GuiSystem.DrawString(mTraderCostGold.ToString(), mTraderCostRect1, "middle_center");
		GuiSystem.DrawString(mTraderCostSilver.ToString(), mTraderCostRect2, "middle_center");
		GuiSystem.DrawString(mTraderCostBronze.ToString(), mTraderCostRect3, "middle_center");
		GuiSystem.DrawString(mEnemyStatusString, mEnemyStatusRect, "middle_center");
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			mItem.mButton.RenderElement();
			GuiInputMgr.DragItemData mData = mItem.mData;
			if (mData != null)
			{
				GuiSystem.DrawString(mData.mCount.ToString(), mItem.mButton.mZoneRect, "lower_right");
			}
		}
		foreach (GuiButton mEnemyItem in mEnemyItems)
		{
			mEnemyItem.RenderElement();
			GuiInputMgr.DragItemData mData2 = mEnemyItem.mDraggableButton.mData;
			if (mData2 != null)
			{
				GuiSystem.DrawString(mData2.mCount.ToString(), mEnemyItem.mZoneRect, "lower_right");
			}
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		mReadyButton.CheckEvent(_curEvent);
		mBarterButton.CheckEvent(_curEvent);
		mUpButton1.CheckEvent(_curEvent);
		mUpButton2.CheckEvent(_curEvent);
		mUpButton3.CheckEvent(_curEvent);
		mDownButton1.CheckEvent(_curEvent);
		mDownButton2.CheckEvent(_curEvent);
		mDownButton3.CheckEvent(_curEvent);
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			mItem.mButton.CheckEvent(_curEvent);
		}
		foreach (GuiButton mEnemyItem in mEnemyItems)
		{
			mEnemyItem.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public void Close()
	{
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			mItem.mButton.mOnMouseLeave(mItem.mButton);
		}
		foreach (GuiButton mEnemyItem in mEnemyItems)
		{
			mEnemyItem.mOnMouseLeave(mEnemyItem);
		}
		OnTradeSuccess();
		if (mCancelCallback != null)
		{
			mCancelCallback();
		}
		SetActive(_active: false);
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void OnTradeSuccess()
	{
		ClearItems();
		SetEnemyStatus(_ready: false);
		SetStatus(_ready: false);
		SetCost(0);
		SetTraderCost(0);
		CalculateTax();
		SetEnemyItems(null);
	}

	public void SetEnemyItems(IDictionary<int, int> _items)
	{
		mEnemyItems.Clear();
		if (_items == null)
		{
			return;
		}
		foreach (KeyValuePair<int, int> _item in _items)
		{
			CtrlPrototype ctrlPrototype = mPrototypes.Get(_item.Key);
			if (ctrlPrototype == null)
			{
				Log.Warning("unknown artikul " + _item.Key);
				continue;
			}
			GuiButton guiButton = new GuiButton();
			guiButton.mElementId = "ENEMY_ITEM_BUTTON";
			guiButton.mId = _item.Key;
			guiButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + ctrlPrototype.Desc.mIcon);
			guiButton.mIconOnTop = false;
			guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			guiButton.mZoneRect = GetFreeItemRect(1);
			guiButton.Init();
			guiButton.mDraggableButton = new GuiInputMgr.DraggableButton();
			GuiInputMgr.DragItemData dragItemData = new GuiInputMgr.DragItemData();
			dragItemData.mCount = _item.Value;
			dragItemData.mArticleId = _item.Key;
			guiButton.mDraggableButton.mData = dragItemData;
			mEnemyItems.Add(guiButton);
		}
	}

	public void SetStatus(bool _ready)
	{
		mStatus = _ready;
		mReadyButton.mLabel = ((!mStatus) ? mReadyString : mNotReadyString);
		mBarterButton.mLocked = !mEnemyStatus || !mStatus;
	}

	public void SetEnemyStatus(bool _ready)
	{
		mEnemyStatus = _ready;
		mEnemyStatusString = ((!mEnemyStatus) ? mNotReadyString : mReadyString);
		mBarterButton.mLocked = !mEnemyStatus || !mStatus;
	}

	public void SetTax(int _money)
	{
		mTax = _money;
		ShopVendor.SetMoney(mTax, ref mTaxGold, ref mTaxSilver, ref mTaxBronze);
	}

	public void SetTraderCost(int _money)
	{
		mTraderCost = _money;
		ShopVendor.SetMoney(mTraderCost, ref mTraderCostGold, ref mTraderCostSilver, ref mTraderCostBronze);
	}

	public override bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _num)
	{
		if (_btn == null)
		{
			Log.Error("Trying to add null dragged button");
			return false;
		}
		CtrlPrototype ctrlPrototype = mPrototypes.TryGet(_btn.mData.mArticleId);
		if (ctrlPrototype == null)
		{
			return false;
		}
		if (ctrlPrototype.Article.CheckFlag(CtrlPrototype.ArticleMask.NO_SELL))
		{
			mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_CANT_DRAG_ITEM_SELL"));
			return false;
		}
		if (mItems.Count == 20)
		{
			return false;
		}
		OnReadyChange(_status: false);
		GuiInputMgr.DraggableButton item = GetItem(_btn.mButton.mId);
		if (item != null)
		{
			if (_cnt > 0)
			{
				item.mData.mCount += _cnt;
			}
			return true;
		}
		GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
		draggableButton.mButtonHolder = this;
		draggableButton.mDragFlag = mDragType;
		draggableButton.mObjectType = _btn.mObjectType;
		draggableButton.mData = new GuiInputMgr.DragItemData(_btn.mData);
		draggableButton.mData.mCount = _cnt;
		draggableButton.mButton = new GuiButton();
		draggableButton.mButton.mElementId = "TRADE_ITEM_BUTTON";
		draggableButton.mButton.mId = _btn.mButton.mId;
		draggableButton.mButton.mIconImg = _btn.mButton.mIconImg;
		draggableButton.mButton.mIconOnTop = _btn.mButton.mIconOnTop;
		GuiButton mButton = draggableButton.mButton;
		mButton.mOnDragStart = (OnDragStart)Delegate.Combine(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
		GuiButton mButton2 = draggableButton.mButton;
		mButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
		GuiButton mButton3 = draggableButton.mButton;
		mButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
		draggableButton.mButton.mZoneRect = GetFreeItemRect(0);
		draggableButton.mButton.Init();
		draggableButton.mButton.mDraggableButton = draggableButton;
		mItems.Add(draggableButton);
		CalculateTax();
		return true;
	}

	public override void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			if (mItem.mButton != _btn.mButton)
			{
				continue;
			}
			mItem.mData.mCount -= _cnt;
			if (mItem.mData.mCount <= 0)
			{
				if (mItem.mButton.mOnMouseLeave != null)
				{
					mItem.mButton.mOnMouseLeave(mItem.mButton);
				}
				GuiButton mButton = mItem.mButton;
				mButton.mOnDragStart = (OnDragStart)Delegate.Remove(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
				GuiButton mButton2 = mItem.mButton;
				mButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Remove(mButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton mButton3 = mItem.mButton;
				mButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Remove(mButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				mItems.Remove(mItem);
				OnReadyChange(_status: false);
				break;
			}
		}
		CalculateTax();
	}

	public override int GetZoneNum(Vector2 _mousePos)
	{
		return (!mZoneRect.Contains(_mousePos)) ? (-1) : 0;
	}

	private GuiInputMgr.DraggableButton GetItem(int _id)
	{
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			if (mItem.mButton.mId == _id)
			{
				return mItem;
			}
		}
		return null;
	}

	private Dictionary<int, int> GetItems()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			dictionary.Add(mItem.mButton.mId, mItem.mData.mCount);
		}
		return dictionary;
	}

	private void CalculateTax()
	{
		int num = 20000;
		mTax = mCost;
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			CtrlPrototype ctrlPrototype = mPrototypes.Get(mItem.mData.mArticleId);
			if (ctrlPrototype != null)
			{
				mTax += ctrlPrototype.Article.mBuyCost;
			}
		}
		mTax = (int)((float)mTax * 0.01f) * ((mTax <= num) ? 1 : 2);
		SetTax(mTax);
	}

	private void OnBtnDragStart(GuiElement _sender)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null)
		{
			GuiSystem.mGuiInputMgr.AddDraggableButton(guiButton.mDraggableButton);
		}
	}

	private void SetCost(int _money)
	{
		mCost = _money;
		ShopVendor.SetMoney(mCost, ref mCostGold, ref mCostSilver, ref mCostBronze);
		CalculateTax();
	}

	private void SetRectZoneSizes()
	{
		mRectZones.Clear();
		mRectZones.Add(0, new List<Rect>());
		mRectZones.Add(1, new List<Rect>());
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					Rect _rect = new Rect(28 + j * 47 + k * 211, 38 + i * 48, 37f, 38f);
					GuiSystem.SetChildRect(mZoneRect, ref _rect);
					mRectZones[k].Add(_rect);
				}
			}
		}
	}

	private Rect GetFreeItemRect(int _num)
	{
		if (_num == 0)
		{
			return mRectZones[_num][mItems.Count];
		}
		return mRectZones[_num][mEnemyItems.Count];
	}

	private void LogTradeAction()
	{
		if (mItems.Count == 0 && mEnemyItems.Count == 0 && mCost == 0 && mTraderCost == 0)
		{
			return;
		}
		string text = string.Empty;
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			CtrlPrototype ctrlPrototype = mPrototypes.Get(mItem.mData.mArticleId);
			string id = ((ctrlPrototype != null) ? ctrlPrototype.Desc.mName : "LOG_UNKNOWN_ITEM");
			text = $"{text} (id: {mItem.mData.mArticleId}) {GuiSystem.GetLocaleText(id)} (count: {mItem.mData.mCount}).";
		}
		string text2 = string.Empty;
		foreach (GuiButton mEnemyItem in mEnemyItems)
		{
			CtrlPrototype ctrlPrototype2 = mPrototypes.Get(mEnemyItem.mDraggableButton.mData.mArticleId);
			string id2 = ((ctrlPrototype2 != null) ? ctrlPrototype2.Desc.mName : "LOG_UNKNOWN_ITEM");
			text2 = string.Format(GuiSystem.GetLocaleText("LOG_ONE_ITEM_INFO"), text2, mEnemyItem.mDraggableButton.mData.mArticleId, GuiSystem.GetLocaleText(id2), mEnemyItem.mDraggableButton.mData.mCount);
		}
		UserLog.AddAction(UserActionType.TRADE_DONE_CLICK, string.Format(GuiSystem.GetLocaleText("LOG_TRADE_INFO"), text, text2, mCost, mTraderCost));
	}

	private void ClearItems()
	{
		foreach (GuiInputMgr.DraggableButton mItem in mItems)
		{
			if (mItem.mButton.mOnMouseLeave != null)
			{
				mItem.mButton.mOnMouseLeave(mItem.mButton);
			}
			GuiButton mButton = mItem.mButton;
			mButton.mOnDragStart = (OnDragStart)Delegate.Remove(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
		}
		mEnemyItems.Clear();
		mItems.Clear();
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr == null)
		{
			return;
		}
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null && guiButton.mDraggableButton != null && guiButton.mDraggableButton.mData != null)
		{
			CtrlPrototype ctrlPrototype = mPrototypes.Get(guiButton.mDraggableButton.mData.mArticleId);
			if (ctrlPrototype != null)
			{
				mFormatedTipMgr.Show(null, ctrlPrototype, 999, 999, _sender.UId, true);
			}
		}
	}

	private void OnItemMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (("TRADE_CLOSE_BUTTON" == _sender.mElementId || "TRADE_CANCEL_BUTTON" == _sender.mElementId) && _buttonId == 0)
		{
			Close();
		}
		else if ("TRADE_READY_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			OnReadyChange(!mStatus);
		}
		else if ("TRADE_BARTER_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			LogTradeAction();
			if (mBarterCallback != null)
			{
				mBarterCallback();
			}
		}
		else if ("UP_BUTTON_1" == _sender.mElementId && _buttonId == 0)
		{
			mCost += 10000;
		}
		else if ("UP_BUTTON_2" == _sender.mElementId && _buttonId == 0)
		{
			mCost += 100;
		}
		else if ("UP_BUTTON_3" == _sender.mElementId && _buttonId == 0)
		{
			mCost++;
		}
		else if ("DOWN_BUTTON_1" == _sender.mElementId && _buttonId == 0)
		{
			mCost -= 10000;
		}
		else if ("DOWN_BUTTON_2" == _sender.mElementId && _buttonId == 0)
		{
			mCost -= 100;
		}
		else if ("DOWN_BUTTON_3" == _sender.mElementId && _buttonId == 0)
		{
			mCost--;
		}
		mCost = ((mCost >= 0) ? mCost : 0);
		mCost = ((mCost <= 999999) ? mCost : 999999);
		SetCost(mCost);
	}

	private void OnReadyChange(bool _status)
	{
		if (_status != mStatus)
		{
			mStatus = _status;
			SetStatus(mStatus);
			if (mReadyCallback != null)
			{
				mReadyCallback(mStatus, mCost, GetItems());
			}
		}
	}
}
