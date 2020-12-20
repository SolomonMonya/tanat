using System;
using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

public class PopUpMenu : GuiElement
{
	public enum PopUpMenuItem
	{
		NONE,
		PRIVATE_MSG_ITEM,
		HERO_INFO_ITEM,
		TRADE_HERO_ITEM,
		GROUP_INVITE,
		GROUP_JOIN_REQUEST,
		GROUP_LEAVE,
		GROUP_REMOVE,
		GROUP_CHANGE_LEADER,
		ADD_FRIEND,
		ADD_IGNORE,
		REMOVE_FRIEND,
		REMOVE_IGNORE
	}

	public delegate void OnMenuItem(PopUpMenuItem _itemId, object _data);

	public OnMenuItem mOnMenuItem;

	private Texture2D mFrame;

	private List<GuiButton> mMenuItems = new List<GuiButton>();

	private int mMenuWidth = 128;

	private int mMenuItemHeight = 35;

	private object mData;

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/misc/popup_frame1");
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect, 4, 4, 4, 4);
		foreach (GuiButton mMenuItem in mMenuItems)
		{
			mMenuItem.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (GuiButton mMenuItem in mMenuItems)
		{
			mMenuItem.CheckEvent(_curEvent);
		}
		if (_curEvent.type == EventType.MouseDown && !mZoneRect.Contains(_curEvent.mousePosition))
		{
			SetActive(_active: false);
		}
		base.CheckEvent(_curEvent);
	}

	public void Deinit()
	{
		foreach (GuiButton mMenuItem in mMenuItems)
		{
			mMenuItem.mOnMouseUp = (OnMouseUp)Delegate.Remove(mMenuItem.mOnMouseUp, new OnMouseUp(OnButton));
		}
		mMenuItems.Clear();
	}

	public void ShowMenu(Vector2 _pos)
	{
		mZoneRect.x = _pos.x;
		mZoneRect.y = _pos.y;
		foreach (GuiButton mMenuItem in mMenuItems)
		{
			mMenuItem.mZoneRect.x += _pos.x;
			mMenuItem.mZoneRect.y += _pos.y;
		}
		SetActive(_active: true);
	}

	public void SetMenuItems(PopUpMenuItem[] _menuItems, object _data)
	{
		mData = _data;
		Deinit();
		SetMenuSize(_menuItems.Length);
		foreach (PopUpMenuItem menuItem in _menuItems)
		{
			AddMenuItem(menuItem);
		}
	}

	private void AddMenuItem(PopUpMenuItem _menuItem)
	{
		GuiButton guiButton = new GuiButton();
		guiButton.mElementId = "MENU_ITEM";
		guiButton.mId = (int)_menuItem;
		guiButton.mLabel = GetMenuItemText(_menuItem);
		Color[] menuItemColor = GetMenuItemColor(_menuItem);
		if (menuItemColor != null)
		{
			guiButton.mNormColor = menuItemColor[0];
			guiButton.mOverColor = menuItemColor[1];
			guiButton.mPressColor = menuItemColor[2];
		}
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		guiButton.mZoneRect = GetItemRect();
		guiButton.Init();
		mMenuItems.Add(guiButton);
	}

	private void SetMenuSize(int _cnt)
	{
		mZoneRect.x = 0f;
		mZoneRect.y = 0f;
		mZoneRect.width = mMenuWidth;
		mZoneRect.height = mMenuItemHeight * _cnt + 20;
		GuiSystem.GetRectScaled(ref mZoneRect);
	}

	private Rect GetItemRect()
	{
		Rect _rect = new Rect(10f, 10 + mMenuItems.Count * mMenuItemHeight, mMenuWidth - 20, mMenuItemHeight);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		return _rect;
	}

	private string GetMenuItemText(PopUpMenuItem _menuItem)
	{
		switch (_menuItem)
		{
		case PopUpMenuItem.PRIVATE_MSG_ITEM:
			return GuiSystem.GetLocaleText("PRIVATE_TEXT");
		case PopUpMenuItem.HERO_INFO_ITEM:
			return GuiSystem.GetLocaleText("INFO_TEXT");
		case PopUpMenuItem.TRADE_HERO_ITEM:
			return GuiSystem.GetLocaleText("TRADE_TEXT");
		case PopUpMenuItem.GROUP_INVITE:
			return GuiSystem.GetLocaleText("GUI_MENU_GROUP_INVITE");
		case PopUpMenuItem.GROUP_JOIN_REQUEST:
			return GuiSystem.GetLocaleText("GUI_MENU_GROUP_JOIN_REQUEST");
		case PopUpMenuItem.GROUP_LEAVE:
			return GuiSystem.GetLocaleText("GUI_MENU_GROUP_LEAVE");
		case PopUpMenuItem.GROUP_REMOVE:
			return GuiSystem.GetLocaleText("GUI_MENU_GROUP_REMOVE");
		case PopUpMenuItem.GROUP_CHANGE_LEADER:
			return GuiSystem.GetLocaleText("GUI_MENU_GROUP_CHANGE_LEADER");
		case PopUpMenuItem.ADD_FRIEND:
			return GuiSystem.GetLocaleText("GUI_MENU_ADD_FRIEND");
		case PopUpMenuItem.ADD_IGNORE:
			return GuiSystem.GetLocaleText("GUI_MENU_ADD_IGNORE");
		case PopUpMenuItem.REMOVE_FRIEND:
			return GuiSystem.GetLocaleText("GUI_MENU_REMOVE_FRIEND");
		case PopUpMenuItem.REMOVE_IGNORE:
			return GuiSystem.GetLocaleText("GUI_MENU_REMOVE_IGNORE");
		default:
			Log.Warning("Getting bad menu item text");
			return string.Empty;
		}
	}

	private Color[] GetMenuItemColor(PopUpMenuItem _menuItem)
	{
		Color[] result = null;
		switch (_menuItem)
		{
		case PopUpMenuItem.PRIVATE_MSG_ITEM:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.HERO_INFO_ITEM:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.TRADE_HERO_ITEM:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.GROUP_INVITE:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.GROUP_JOIN_REQUEST:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.GROUP_LEAVE:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.GROUP_REMOVE:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.GROUP_CHANGE_LEADER:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.ADD_FRIEND:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.ADD_IGNORE:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.REMOVE_FRIEND:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		case PopUpMenuItem.REMOVE_IGNORE:
			result = new Color[3]
			{
				Color.white,
				Color.yellow,
				Color.green
			};
			break;
		default:
			Log.Warning("Getting bad menu item text");
			break;
		}
		return result;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ("MENU_ITEM" == _sender.mElementId && _buttonId == 0)
		{
			if (mOnMenuItem != null)
			{
				mOnMenuItem((PopUpMenuItem)_sender.mId, mData);
			}
			SetActive(_active: false);
		}
	}
}
