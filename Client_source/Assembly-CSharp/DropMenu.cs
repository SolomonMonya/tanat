using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class DropMenu : GuiElement, EscapeListener
{
	public Texture2D mFrameImg;

	private GuiButton mCloseButton;

	private List<Rect> mItemZoneRects;

	private List<GuiButton> mItemButtons;

	private FormatedTipMgr mFormatedTipMgr;

	private IStoreContentProvider<BattlePrototype> mBattleItemData;

	private IStoreContentProvider<CtrlPrototype> mCtrlItemData;

	private PlayerControl mPlayerCtrl;

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
		mCloseButton = null;
		mItemZoneRects = new List<Rect>();
		mItemButtons = new List<GuiButton>();
		GuiSystem.mGuiInputMgr.AddEscapeListener(535, this);
		mFrameImg = GuiSystem.GetImage("Gui/DropMenu/frame1");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseDown = (OnMouseDown)Delegate.Combine(guiButton.mOnMouseDown, new OnMouseDown(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrameImg.width, mFrameImg.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		mCloseButton.mZoneRect = new Rect(253f, 10f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		SetItemZoneRects();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrameImg, mZoneRect);
		mCloseButton.RenderElement();
		foreach (GuiButton mItemButton in mItemButtons)
		{
			mItemButton.RenderElement();
			GuiSystem.DrawString(mItemButton.mDraggableButton.mData.mCount.ToString(), mItemButton.mZoneRect, "lower_right");
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		foreach (GuiButton mItemButton in mItemButtons)
		{
			mItemButton.CheckEvent(_curEvent);
		}
	}

	public void Close()
	{
		foreach (GuiButton mItemButton in mItemButtons)
		{
			mFormatedTipMgr.Hide(mItemButton.UId);
		}
		SetActive(_active: false);
	}

	public void SetData(List<DroppedItem> _items)
	{
		mItemButtons.Clear();
		if (_items.Count > 10)
		{
			Log.Error("Too many items in drop list : " + _items.Count);
			return;
		}
		foreach (DroppedItem _item in _items)
		{
			BattlePrototype battlePrototype = mBattleItemData.TryGet(_item.mProtoId);
			if (battlePrototype == null)
			{
				Log.Warning("No battle data item with id : " + _item.mProtoId);
				continue;
			}
			GuiButton guiButton = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", "Gui/Icons/Items/" + battlePrototype.Desc.mIcon, string.Empty);
			guiButton.mElementId = "DROP_ITEM_BUTTON";
			guiButton.mId = _item.mId;
			guiButton.mZoneRect = GetItemZoneRect(mItemButtons.Count);
			guiButton.mDraggableButton = new GuiInputMgr.DraggableButton();
			guiButton.mDraggableButton.mData = new GuiInputMgr.DragItemData();
			guiButton.mDraggableButton.mData.mCount = _item.mCount;
			guiButton.mDraggableButton.mData.mArticleId = battlePrototype.Item.mArticle;
			int avatarObjId = mPlayerCtrl.SelfPlayer.Player.AvatarObjId;
			guiButton.mLocked = !_item.mAllowed.Contains(avatarObjId);
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			guiButton.Init();
			AddTutorialElement(guiButton);
			mItemButtons.Add(guiButton);
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			GuiButton guiButton = _sender as GuiButton;
			CtrlPrototype ctrlPrototype = mCtrlItemData.TryGet(guiButton.mDraggableButton.mData.mArticleId);
			if (ctrlPrototype != null)
			{
				mFormatedTipMgr.Show(null, ctrlPrototype, -1, mPlayerCtrl.SelfPlayer.Player.Hero.GameInfo.mLevel, _sender.UId, true);
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

	private void SetItemZoneRects()
	{
		mItemZoneRects.Clear();
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				Rect _rect = new Rect(9 + j * 53, 39 + i * 53, 47f, 47f);
				GuiSystem.SetChildRect(mZoneRect, ref _rect);
				mItemZoneRects.Add(_rect);
			}
		}
	}

	private Rect GetItemZoneRect(int _num)
	{
		if (mItemZoneRects.Count <= _num)
		{
			Log.Error("bad item num : " + _num);
			return default(Rect);
		}
		return mItemZoneRects[_num];
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ("CLOSE_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			Close();
		}
		else if ("DROP_ITEM_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			mPlayerCtrl.SelfPlayer.PickUp(_sender.mId);
			Close();
		}
	}

	public void SetData(IStoreContentProvider<BattlePrototype> _battleItemData, IStoreContentProvider<CtrlPrototype> _ctrlItemData, PlayerControl _pl, FormatedTipMgr _tipMg)
	{
		mBattleItemData = _battleItemData;
		mCtrlItemData = _ctrlItemData;
		mPlayerCtrl = _pl;
		mFormatedTipMgr = _tipMg;
	}

	public void Clear()
	{
		mBattleItemData = null;
		mCtrlItemData = null;
		mPlayerCtrl = null;
		mFormatedTipMgr = null;
	}
}
