using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class FastItemAccessPanel : GuiInputListener
{
	private class ActiveItem
	{
		public GuiInputMgr.DraggableButton mButton;
	}

	private class AutoUseRenderData
	{
		public Texture2D mFrameImage;

		public Rect mFrameRect1;

		public Rect mFrameRect2;

		public int mFrameNum;
	}

	private IItemUsageMgr mItemUsageMgr;

	private ThingAutoUseListener mAutoUseListener;

	private IEnumerable<MainInfoWindow.CooldownView> mCooldownViews;

	private Dictionary<int, ActiveItem> mActiveItems;

	private Dictionary<int, AutoUseRenderData> mAutoUseRenderDatas;

	private Texture2D mRenderDataFXImage;

	private static int mMaxItemCount;

	private static List<Rect> mSlotRects;

	private SelfPlayer mSelfPlayer;

	private OkDialog mOkDialog;

	public override void Init()
	{
		mSlotRects = new List<Rect>();
		mActiveItems = new Dictionary<int, ActiveItem>();
		mAutoUseRenderDatas = new Dictionary<int, AutoUseRenderData>();
		mRenderDataFXImage = GuiSystem.GetImage("Gui/MainInfo/skill_fx1");
		mDragType = GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG;
		GuiSystem.mGuiInputMgr.AddGuiDraggableButtonListener(this);
		mMaxItemCount = 5;
		GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet("battle");
		mOkDialog = guiSet.GetElementById<OkDialog>("OK_DAILOG");
	}

	public override void SetSize()
	{
		InitSlotRects();
		foreach (KeyValuePair<int, ActiveItem> mActiveItem in mActiveItems)
		{
			mActiveItem.Value.mButton.mButton.mZoneRect = GetItemRect(mActiveItem.Key);
		}
	}

	public override void Uninit()
	{
		mActiveItems.Clear();
		mAutoUseRenderDatas.Clear();
	}

	public override void RenderElement()
	{
		foreach (ActiveItem value in mActiveItems.Values)
		{
			value.mButton.mButton.RenderElement();
			GuiSystem.DrawString(value.mButton.mData.mCount.ToString(), value.mButton.mButton.mZoneRect, "lower_right");
			if (mItemUsageMgr == null || mCooldownViews == null)
			{
				continue;
			}
			double cooldownProgress = mItemUsageMgr.GetCooldownProgress(value.mButton.mButton.mId);
			if (!(cooldownProgress > 0.0) || !(cooldownProgress <= 1.0))
			{
				continue;
			}
			foreach (MainInfoWindow.CooldownView mCooldownView in mCooldownViews)
			{
				if (cooldownProgress <= (double)mCooldownView.mUpperBound)
				{
					GuiSystem.DrawImage(mCooldownView.mTex, value.mButton.mButton.mZoneRect);
					break;
				}
			}
		}
		foreach (KeyValuePair<int, AutoUseRenderData> mAutoUseRenderData in mAutoUseRenderDatas)
		{
			GuiSystem.DrawImage(mAutoUseRenderData.Value.mFrameImage, mAutoUseRenderData.Value.mFrameRect1, mAutoUseRenderData.Value.mFrameRect2);
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (ActiveItem value in mActiveItems.Values)
		{
			value.mButton.mButton.CheckEvent(_curEvent);
		}
		if (_curEvent.type != EventType.KeyDown || GUI.GetNameOfFocusedControl() == "TEXT_FIELD_MSG")
		{
			return;
		}
		switch (_curEvent.keyCode)
		{
		case KeyCode.Alpha1:
			if (mActiveItems.ContainsKey(0) && !mActiveItems[0].mButton.mButton.mLocked)
			{
				TryUseItem(mActiveItems[0].mButton.mButton.mId);
			}
			_curEvent.Use();
			break;
		case KeyCode.Alpha2:
			if (mActiveItems.ContainsKey(1) && !mActiveItems[1].mButton.mButton.mLocked)
			{
				TryUseItem(mActiveItems[1].mButton.mButton.mId);
			}
			_curEvent.Use();
			break;
		case KeyCode.Alpha3:
			if (mActiveItems.ContainsKey(2) && !mActiveItems[2].mButton.mButton.mLocked)
			{
				TryUseItem(mActiveItems[2].mButton.mButton.mId);
			}
			_curEvent.Use();
			break;
		case KeyCode.Alpha4:
			if (mActiveItems.ContainsKey(3) && !mActiveItems[3].mButton.mButton.mLocked)
			{
				TryUseItem(mActiveItems[3].mButton.mButton.mId);
			}
			_curEvent.Use();
			break;
		case KeyCode.Alpha5:
			if (mActiveItems.ContainsKey(4) && !mActiveItems[4].mButton.mButton.mLocked)
			{
				TryUseItem(mActiveItems[4].mButton.mButton.mId);
			}
			_curEvent.Use();
			break;
		}
	}

	public override bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _num)
	{
		if (_btn == null)
		{
			Log.Error("Trying to add null dragged button");
			return false;
		}
		if (_btn.mData.mUndraggable)
		{
			mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_CANT_DRAG_ITEM_BY_LVL"));
			return false;
		}
		if (GetItemById(_btn.mButton.mId) != null)
		{
			return false;
		}
		if (_num == -1)
		{
			_num = OptionsMgr.GetActiveItemNum(_btn.mData.mArticleId);
			if (_num == -1)
			{
				_num = GetFreeSlotNum();
				if (_num == -1)
				{
					return false;
				}
				OptionsMgr.SetActiveItem(_btn.mData.mArticleId, _num);
			}
		}
		else
		{
			OptionsMgr.SetActiveItem(_btn.mData.mArticleId, _num);
		}
		if (mActiveItems.ContainsKey(_num))
		{
			return false;
		}
		ActiveItem activeItem = new ActiveItem();
		GuiInputMgr.DraggableButton draggableButton = new GuiInputMgr.DraggableButton();
		draggableButton.mButtonHolder = this;
		draggableButton.mDragFlag = mDragType;
		draggableButton.mObjectType = _btn.mObjectType;
		draggableButton.mData = _btn.mData;
		draggableButton.mButton = new GuiButton();
		draggableButton.mButton.mElementId = "FAST_ACCESS_ITEM_BUTTON";
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
		mButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(mButton2.mOnMouseUp, new OnMouseUp(OnButton));
		GuiButton mButton3 = draggableButton.mButton;
		mButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(mButton3.mOnMouseEnter, _btn.mButton.mOnMouseEnter);
		GuiButton mButton4 = draggableButton.mButton;
		mButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(mButton4.mOnMouseLeave, _btn.mButton.mOnMouseLeave);
		draggableButton.mButton.mZoneRect = GetItemRect(_num);
		draggableButton.mButton.Init();
		activeItem.mButton = draggableButton;
		mActiveItems.Add(_num, activeItem);
		return true;
	}

	public override void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
		foreach (KeyValuePair<int, ActiveItem> mActiveItem in mActiveItems)
		{
			if (_btn.mButton.mId != mActiveItem.Value.mButton.mButton.mId)
			{
				continue;
			}
			if (_cnt == -1)
			{
				if (mActiveItem.Value.mButton.mButton.mOnMouseLeave != null)
				{
					mActiveItem.Value.mButton.mButton.mOnMouseLeave(mActiveItem.Value.mButton.mButton);
				}
				GuiButton mButton = mActiveItem.Value.mButton.mButton;
				mButton.mOnDragStart = (OnDragStart)Delegate.Remove(mButton.mOnDragStart, new OnDragStart(OnBtnDragStart));
				GuiButton mButton2 = mActiveItem.Value.mButton.mButton;
				mButton2.mOnMouseUp = (OnMouseUp)Delegate.Remove(mButton2.mOnMouseUp, new OnMouseUp(OnButton));
				mActiveItems.Remove(mActiveItem.Key);
				OptionsMgr.SetActiveItem(-1, mActiveItem.Key);
				break;
			}
			int num = mActiveItem.Value.mButton.mData.mCount - _cnt;
			if (num <= 0)
			{
				if (mActiveItem.Value.mButton.mButton.mOnMouseLeave != null)
				{
					mActiveItem.Value.mButton.mButton.mOnMouseLeave(mActiveItem.Value.mButton.mButton);
				}
				GuiButton mButton3 = mActiveItem.Value.mButton.mButton;
				mButton3.mOnDragStart = (OnDragStart)Delegate.Remove(mButton3.mOnDragStart, new OnDragStart(OnBtnDragStart));
				GuiButton mButton4 = mActiveItem.Value.mButton.mButton;
				mButton4.mOnMouseUp = (OnMouseUp)Delegate.Remove(mButton4.mOnMouseUp, new OnMouseUp(OnButton));
				mActiveItems.Remove(mActiveItem.Key);
				OptionsMgr.SetActiveItem(-1, mActiveItem.Key);
			}
			break;
		}
	}

	public override void Update()
	{
		foreach (ActiveItem value2 in mActiveItems.Values)
		{
			int mId = value2.mButton.mButton.mId;
			mAutoUseListener.CheckAutoUse(mId);
		}
		foreach (KeyValuePair<int, AutoUseRenderData> mAutoUseRenderData in mAutoUseRenderDatas)
		{
			ActiveItem itemById = GetItemById(mAutoUseRenderData.Key);
			if (itemById == null)
			{
				continue;
			}
			AutoUseRenderData value = mAutoUseRenderData.Value;
			Rect mZoneRect = itemById.mButton.mButton.mZoneRect;
			float num = 0.03125f;
			float num2 = 64f * GuiSystem.mYRate;
			float num3 = 64f * GuiSystem.mYRate;
			value.mFrameRect1 = new Rect(mZoneRect.x + (mZoneRect.width - num2) / 2f, mZoneRect.y + (mZoneRect.height - num3) / 2f, num2, num3);
			value.mFrameRect2 = new Rect(0f, (float)value.mFrameNum * num, 1f, num);
			if (Time.frameCount % 4 == 0)
			{
				value.mFrameNum++;
				if (value.mFrameNum == 32)
				{
					value.mFrameNum = 0;
				}
			}
		}
	}

	public int GetFreeSlotNum()
	{
		for (int i = 0; i < mMaxItemCount; i++)
		{
			if (!mActiveItems.ContainsKey(i))
			{
				return i;
			}
		}
		return -1;
	}

	public override int GetZoneNum(Vector2 _mousePos)
	{
		if (!mZoneRect.Contains(_mousePos))
		{
			return -1;
		}
		Rect rect = default(Rect);
		for (int i = 0; i < mMaxItemCount; i++)
		{
			if (GetItemRect(i).Contains(_mousePos) && !mActiveItems.ContainsKey(i))
			{
				return i;
			}
		}
		return -1;
	}

	private ActiveItem GetItemById(int _id)
	{
		foreach (KeyValuePair<int, ActiveItem> mActiveItem in mActiveItems)
		{
			if (_id == mActiveItem.Value.mButton.mButton.mId)
			{
				return mActiveItem.Value;
			}
		}
		return null;
	}

	private void TryUseItem(int _id)
	{
		if (mItemUsageMgr != null && mSelfPlayer != null)
		{
			double cooldownProgress = mItemUsageMgr.GetCooldownProgress(_id);
			if ((cooldownProgress < 0.0 || cooldownProgress > 1.0) && mSelfPlayer.Player.IsAvatarBinded)
			{
				mItemUsageMgr.UseItem(_id);
			}
		}
	}

	public static Rect GetItemRect(int _slotNum)
	{
		if (_slotNum >= mMaxItemCount)
		{
			return default(Rect);
		}
		if (mSlotRects.Count < _slotNum)
		{
			return default(Rect);
		}
		return mSlotRects[_slotNum];
	}

	private Rect GetSlotRect(int _slotNum)
	{
		Rect _rect = default(Rect);
		_rect.x = 4 + _slotNum * 45;
		_rect.y = 4f;
		_rect.width = 36f;
		_rect.height = 38f;
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		return _rect;
	}

	private void InitSlotRects()
	{
		mSlotRects.Clear();
		for (int i = 0; i < mMaxItemCount; i++)
		{
			mSlotRects.Add(GetSlotRect(i));
		}
	}

	private void OnBtnDragStart(GuiElement _sender)
	{
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null)
		{
			GuiSystem.mGuiInputMgr.AddDraggableButton(guiButton.mDraggableButton);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "FAST_ACCESS_ITEM_BUTTON" && _buttonId == 0)
		{
			TryUseItem(_sender.mId);
		}
		else
		{
			if (!(_sender.mElementId == "FAST_ACCESS_ITEM_BUTTON") || _buttonId != 1)
			{
				return;
			}
			if (Event.current.control)
			{
				SetAutoUseData(_sender as GuiButton, SwitchAutoUse(_sender.mId));
				return;
			}
			GuiButton guiButton = _sender as GuiButton;
			if (guiButton != null)
			{
				GuiSystem.mGuiInputMgr.AddItemToMove(guiButton.mDraggableButton, GuiInputMgr.ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG, GuiInputMgr.ButtonDragFlags.INVENTORY_MENU_DRAG, _countAccept: false);
			}
		}
	}

	private bool SwitchAutoUse(int _itemId)
	{
		if (mAutoUseListener == null)
		{
			return false;
		}
		bool autoUse = mAutoUseListener.GetAutoUse(_itemId);
		return mAutoUseListener.SetAutoUse(_itemId, !autoUse);
	}

	private void SetAutoUseData(GuiButton _itemButton, bool _autoUse)
	{
		if (_itemButton != null)
		{
			if (_autoUse)
			{
				AutoUseRenderData autoUseRenderData = new AutoUseRenderData();
				autoUseRenderData.mFrameImage = mRenderDataFXImage;
				mAutoUseRenderDatas.Add(_itemButton.mId, autoUseRenderData);
			}
			else if (mAutoUseRenderDatas.ContainsKey(_itemButton.mId))
			{
				mAutoUseRenderDatas.Remove(_itemButton.mId);
			}
		}
	}

	public void SetData(IItemUsageMgr _mgr, SelfPlayer _player)
	{
		mItemUsageMgr = _mgr;
		mSelfPlayer = _player;
		mAutoUseListener = _player.GetAutoUseListener();
	}

	public void Clear()
	{
		mItemUsageMgr = null;
		mAutoUseListener = null;
		mSelfPlayer = null;
		if (mActiveItems != null)
		{
			mActiveItems.Clear();
		}
		if (mAutoUseRenderDatas != null)
		{
			mAutoUseRenderDatas.Clear();
		}
	}

	public void SetCooldownViews(IEnumerable<MainInfoWindow.CooldownView> _views)
	{
		mCooldownViews = _views;
	}
}
