using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

public class GuiInputMgr
{
	public enum ButtonDragFlags
	{
		NONE = 0,
		INVENTORY_MENU_DRAG = 1,
		ACTIVE_ITEM_PANEL_DRAG = 2,
		FAST_ITEM_ACCESS_PANEL_DRAG = 3,
		PLAYER_TRADE_MENU_DRAG = 4,
		HERO_INFO_DRAG = 5,
		SHOP = 6,
		DROP_DRAG = 0x100
	}

	public enum ObjectType
	{
		NONE,
		POTION,
		ITEM,
		ACTIVE_SKILL,
		PASSIVE_SKILL
	}

	public enum ItemOrigin
	{
		BATTLE = 1,
		CTRL
	}

	public class DragItemData
	{
		public bool mUndraggable;

		public int mCount;

		public int mOrigin = -1;

		public int mArticleId = -1;

		public DragItemData()
		{
			mUndraggable = false;
			mCount = 0;
			mOrigin = -1;
			mArticleId = -1;
		}

		public DragItemData(DragItemData _data)
		{
			mUndraggable = _data.mUndraggable;
			mCount = _data.mCount;
			mOrigin = _data.mOrigin;
			mArticleId = _data.mArticleId;
		}
	}

	public class ItemToMove
	{
		public DraggableButton mButton;

		public ButtonDragFlags mDragFrom;

		public ButtonDragFlags mDragTo;

		public int mCount;
	}

	public class DraggableButton
	{
		public GuiButton mButton;

		public GuiInputListener mButtonHolder;

		public Rect mDrawDragRect = default(Rect);

		public DragItemData mData;

		public ObjectType mObjectType;

		public ButtonDragFlags mDragFlag;
	}

	public delegate void DropItemCallback(int _itemOrigin, int _itemId, int _count);

	public DropItemCallback mDropItemCallback;

	private DraggableButton mDraggableButton;

	private List<GuiInputListener> mGuiDraggableButtonListeners = new List<GuiInputListener>();

	private SortedDictionary<int, EscapeListener> mEscListeners = new SortedDictionary<int, EscapeListener>();

	private List<GuiElement> mCurElements;

	public List<ItemToMove> mItemsToMove = new List<ItemToMove>();

	private ItemCountMenu mCountMenu;

	private YesNoDialog mYesNoDialog;

	public void Uninit()
	{
		mItemsToMove.Clear();
		mCurElements = null;
		mDraggableButton = null;
	}

	public void SetCurElements(List<GuiElement> _elements)
	{
		mCurElements = _elements;
	}

	public void AddDraggableButton(DraggableButton _btn)
	{
		if (_btn == null)
		{
			Log.Error("DraggableButton adding in GuiInputMgr is null!");
			return;
		}
		if (mDraggableButton != null)
		{
			Log.Error("Dragged button in GuiInputMgr is already exist!");
			return;
		}
		mDraggableButton = new DraggableButton();
		mDraggableButton.mButton = _btn.mButton;
		mDraggableButton.mButtonHolder = _btn.mButtonHolder;
		mDraggableButton.mDragFlag = _btn.mDragFlag;
		mDraggableButton.mObjectType = _btn.mObjectType;
		mDraggableButton.mData = _btn.mData;
		mDraggableButton.mDrawDragRect.width = mDraggableButton.mButton.mZoneRect.width;
		mDraggableButton.mDrawDragRect.height = mDraggableButton.mButton.mZoneRect.height;
	}

	public void AddItemToMove(DraggableButton _btn, ButtonDragFlags _from, ButtonDragFlags _to, int _cnt, bool _countAccept)
	{
		if (_btn == null || _btn.mButton == null)
		{
			return;
		}
		ItemToMove itemToMove = new ItemToMove();
		itemToMove.mButton = _btn;
		itemToMove.mDragFrom = _from;
		itemToMove.mDragTo = _to;
		if (_countAccept)
		{
			if (mCountMenu == null || mYesNoDialog == null)
			{
				return;
			}
			YesNoDialog.OnAnswer callback = delegate(bool _yes)
			{
				if (_yes)
				{
					mCountMenu.SetData(1, _btn.mData.mCount, _btn, _from, _to);
				}
			};
			if (_to == ButtonDragFlags.DROP_DRAG)
			{
				mYesNoDialog.SetData(GuiSystem.GetLocaleText("GUI_DROP_ITEM"), "YES_TEXT", "NO_TEXT", callback);
			}
			else
			{
				mCountMenu.SetData(1, _cnt, _btn, _from, _to);
			}
		}
		else
		{
			itemToMove.mCount = _cnt;
			AddItemToMove(itemToMove);
		}
	}

	public void AddItemToMove(ItemToMove _item)
	{
		if (_item != null)
		{
			mItemsToMove.Add(_item);
		}
	}

	public void AddItemToMove(DraggableButton _btn, ButtonDragFlags _from, ButtonDragFlags _to, bool _countAccept)
	{
		if (_btn.mData == null)
		{
			Log.Error("Bad data in btn");
		}
		else
		{
			AddItemToMove(_btn, _from, _to, _btn.mData.mCount, _countAccept);
		}
	}

	public void AddGuiDraggableButtonListener(GuiInputListener _element)
	{
		if (_element == null)
		{
			Log.Error("Added dragged btn listener is null");
		}
		else
		{
			mGuiDraggableButtonListeners.Add(_element);
		}
	}

	public void AddEscapeListener(int _priority, EscapeListener _element)
	{
		if (_element != null)
		{
			mEscListeners.Add(GenerateUnicPriority(_priority), _element);
		}
	}

	public void CheckEvent(Event _curEvent)
	{
		if (mDraggableButton != null)
		{
			mDraggableButton.mDrawDragRect.x = _curEvent.mousePosition.x - mDraggableButton.mDrawDragRect.width / 2f;
			mDraggableButton.mDrawDragRect.y = _curEvent.mousePosition.y - mDraggableButton.mDrawDragRect.height / 2f;
			if (_curEvent.type == EventType.MouseUp)
			{
				TryToAddButton(mDraggableButton, _curEvent.mousePosition);
				mDraggableButton = null;
			}
		}
		if (_curEvent.type != EventType.KeyUp || _curEvent.keyCode != KeyCode.Escape || mCurElements == null)
		{
			return;
		}
		foreach (EscapeListener value in mEscListeners.Values)
		{
			if (mCurElements.Contains(value as GuiElement) && value.OnEscapeAction())
			{
				_curEvent.Use();
				break;
			}
		}
	}

	public void RenderElement()
	{
		if (mDraggableButton != null && (mDraggableButton.mDrawDragRect.x != 0f || mDraggableButton.mDrawDragRect.y != 0f))
		{
			if (null != mDraggableButton.mButton.mNormImg)
			{
				Graphics.DrawTexture(mDraggableButton.mDrawDragRect, mDraggableButton.mButton.mNormImg);
			}
			if (mDraggableButton.mButton.mIconImg != null)
			{
				Graphics.DrawTexture(mDraggableButton.mDrawDragRect, mDraggableButton.mButton.mIconImg);
			}
			if (mDraggableButton.mButton.mEffectImg != null)
			{
				Graphics.DrawTexture(mDraggableButton.mDrawDragRect, mDraggableButton.mButton.mEffectImg);
			}
		}
	}

	public void Update()
	{
		foreach (ItemToMove item in mItemsToMove)
		{
			if (MoveItem(item))
			{
				mItemsToMove.Remove(item);
				break;
			}
		}
	}

	private int GenerateUnicPriority(int _priority)
	{
		while (mEscListeners.ContainsKey(_priority))
		{
			_priority++;
		}
		return _priority;
	}

	private bool MoveItem(ItemToMove _item)
	{
		GuiInputListener listenerByType = GetListenerByType(_item.mDragFrom);
		GuiInputListener listenerByType2 = GetListenerByType(_item.mDragTo);
		if (_item.mDragTo == ButtonDragFlags.DROP_DRAG)
		{
			if (mDropItemCallback != null)
			{
				mDropItemCallback(_item.mButton.mData.mOrigin, _item.mButton.mButton.mId, _item.mCount);
			}
			return true;
		}
		bool flag = false;
		if (listenerByType2 != null)
		{
			if (_item.mCount == -1)
			{
				listenerByType2.RemoveDraggableButton(_item.mButton, _item.mCount);
			}
			else
			{
				flag = listenerByType2.AddDraggableButton(_item.mButton, _item.mCount, -1);
			}
		}
		if (listenerByType != null && _item.mDragTo != ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG && (listenerByType2 == null || (listenerByType2 != null && flag)))
		{
			listenerByType.RemoveDraggableButton(_item.mButton, _item.mCount);
		}
		return true;
	}

	private GuiInputListener GetListenerByType(ButtonDragFlags _type)
	{
		if (_type == ButtonDragFlags.NONE || _type == ButtonDragFlags.DROP_DRAG)
		{
			return null;
		}
		foreach (GuiInputListener mGuiDraggableButtonListener in mGuiDraggableButtonListeners)
		{
			if (mGuiDraggableButtonListener.GetListenerType() == _type && mGuiDraggableButtonListener.Active)
			{
				return mGuiDraggableButtonListener;
			}
		}
		return null;
	}

	private void RemoveSourceButton(DraggableButton _draggableBtn)
	{
		_draggableBtn.mButtonHolder.RemoveDraggableButton(_draggableBtn, -1);
	}

	private void TryToAddButton(DraggableButton _draggableBtn, Vector2 _mousePos)
	{
		foreach (GuiInputListener mGuiDraggableButtonListener in mGuiDraggableButtonListeners)
		{
			if (mGuiDraggableButtonListener.GetListenerType() == ButtonDragFlags.ACTIVE_ITEM_PANEL_DRAG && mGuiDraggableButtonListener.Active)
			{
				int zoneNum = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
				if (zoneNum != -1)
				{
					if (_draggableBtn.mObjectType == ObjectType.ITEM)
					{
						mGuiDraggableButtonListener.AddDraggableButton(_draggableBtn, -1, zoneNum);
					}
					return;
				}
			}
			else if (mGuiDraggableButtonListener.GetListenerType() == ButtonDragFlags.INVENTORY_MENU_DRAG && mGuiDraggableButtonListener.Active)
			{
				int zoneNum2 = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
				if (zoneNum2 != -1)
				{
					if (_draggableBtn.mDragFlag == ButtonDragFlags.PLAYER_TRADE_MENU_DRAG)
					{
						AddItemToMove(_draggableBtn, ButtonDragFlags.PLAYER_TRADE_MENU_DRAG, ButtonDragFlags.INVENTORY_MENU_DRAG, _countAccept: true);
					}
					else if (_draggableBtn.mDragFlag == ButtonDragFlags.INVENTORY_MENU_DRAG)
					{
						mGuiDraggableButtonListener.AddDraggableButton(_draggableBtn, -1, zoneNum2);
					}
					else if (_draggableBtn.mDragFlag == ButtonDragFlags.SHOP)
					{
						AddItemToMove(_draggableBtn, ButtonDragFlags.SHOP, ButtonDragFlags.INVENTORY_MENU_DRAG, _countAccept: true);
					}
					else if (mGuiDraggableButtonListener.AddDraggableButton(_draggableBtn, _draggableBtn.mData.mCount, zoneNum2))
					{
						RemoveSourceButton(_draggableBtn);
					}
					return;
				}
			}
			else
			{
				if (mGuiDraggableButtonListener.GetListenerType() == ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG && mGuiDraggableButtonListener.Active)
				{
					int zoneNum3 = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
					if (zoneNum3 == -1)
					{
						continue;
					}
					if (_draggableBtn.mObjectType == ObjectType.POTION)
					{
						if (_draggableBtn.mDragFlag == ButtonDragFlags.FAST_ITEM_ACCESS_PANEL_DRAG)
						{
							RemoveSourceButton(_draggableBtn);
						}
						mGuiDraggableButtonListener.AddDraggableButton(_draggableBtn, _draggableBtn.mData.mCount, zoneNum3);
					}
					return;
				}
				if (mGuiDraggableButtonListener.GetListenerType() == ButtonDragFlags.PLAYER_TRADE_MENU_DRAG && mGuiDraggableButtonListener.Active)
				{
					int zoneNum4 = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
					if (zoneNum4 != -1)
					{
						if (_draggableBtn.mDragFlag == ButtonDragFlags.INVENTORY_MENU_DRAG)
						{
							AddItemToMove(_draggableBtn, ButtonDragFlags.INVENTORY_MENU_DRAG, ButtonDragFlags.PLAYER_TRADE_MENU_DRAG, _countAccept: true);
						}
						return;
					}
				}
				else if (mGuiDraggableButtonListener.GetListenerType() == ButtonDragFlags.HERO_INFO_DRAG && mGuiDraggableButtonListener.Active)
				{
					int zoneNum5 = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
					if (zoneNum5 != -1)
					{
						mGuiDraggableButtonListener.AddDraggableButton(_draggableBtn, _draggableBtn.mData.mCount, zoneNum5);
						return;
					}
				}
				else
				{
					if (mGuiDraggableButtonListener.GetListenerType() != ButtonDragFlags.SHOP || !mGuiDraggableButtonListener.Active)
					{
						continue;
					}
					int zoneNum6 = mGuiDraggableButtonListener.GetZoneNum(_mousePos);
					if (zoneNum6 != -1)
					{
						if (_draggableBtn.mDragFlag == ButtonDragFlags.INVENTORY_MENU_DRAG)
						{
							AddItemToMove(_draggableBtn, ButtonDragFlags.INVENTORY_MENU_DRAG, ButtonDragFlags.SHOP, _countAccept: true);
						}
						return;
					}
				}
			}
		}
		if (_draggableBtn.mDragFlag == ButtonDragFlags.INVENTORY_MENU_DRAG)
		{
			AddItemToMove(_draggableBtn, ButtonDragFlags.INVENTORY_MENU_DRAG, ButtonDragFlags.DROP_DRAG, _countAccept: true);
		}
	}

	public void SetGui(ItemCountMenu _countMenu, YesNoDialog _yesNoDialog)
	{
		mCountMenu = _countMenu;
		mYesNoDialog = _yesNoDialog;
	}

	public void Clean()
	{
		mCountMenu = null;
		mDropItemCallback = null;
		mYesNoDialog = null;
	}
}
