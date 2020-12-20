using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class GuiElement
{
	private enum GuiElementMouseState
	{
		MOUSE_NONE,
		MOUSE_OVER
	}

	public enum GuiElementType
	{
		NONE,
		WINDOW,
		BUTTON,
		TEXT_DATA
	}

	public delegate void OnMouseEnter(GuiElement _sender);

	public delegate void OnMouseLeave(GuiElement _sender);

	public delegate void OnMouseDown(GuiElement _sender, int _buttonId);

	public delegate void OnMouseUp(GuiElement _sender, int _buttonId);

	public delegate void OnDragStart(GuiElement _sender);

	public delegate void OnDoubleClick(GuiElement _sender);

	public string mGuiSetId = string.Empty;

	public string mElementId = string.Empty;

	public int mId;

	public GuiElementType mType;

	public Vector2 mPos = Vector2.zero;

	public Rect mZoneRect = default(Rect);

	public Rect mIgnoreEventRect = default(Rect);

	private GuiElementMouseState mGuiElementMouseState;

	private bool mMousePress;

	public OnMouseEnter mOnMouseEnter;

	public OnMouseLeave mOnMouseLeave;

	public OnMouseDown mOnMouseDown;

	public OnMouseDown mOnMouseLockedDown;

	public OnMouseUp mOnMouseUp;

	public OnMouseUp mOnMouseLockedUp;

	public OnDragStart mOnDragStart;

	public OnDoubleClick mOnDoubleClick;

	private float mLastClickTime;

	public bool mLocked;

	private bool mActive = true;

	private int mUId;

	private static int mLastUId;

	private Dictionary<string, GuiElement> mTutorialElements = new Dictionary<string, GuiElement>();

	private UserActionType mActionType;

	private string mActionMessage = string.Empty;

	public bool Active => mActive;

	public int UId => mUId;

	public virtual void Init()
	{
		mUId = ++mLastUId;
	}

	public virtual void Uninit()
	{
	}

	public virtual void SetSize()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void RenderElement()
	{
	}

	public virtual void OnInput()
	{
	}

	public virtual void SetActive(bool _active)
	{
		mActive = _active;
		if (!mActive && mOnMouseLeave != null)
		{
			mOnMouseLeave(this);
		}
		if (!mActive)
		{
			PopupInfo.CloseParentTip(this);
		}
	}

	public void RegisterAction(UserActionType _action)
	{
		mActionType = _action;
	}

	public void RegisterAction(UserActionType _action, string _message)
	{
		mActionType = _action;
		mActionMessage = _message;
	}

	public virtual void CheckEvent(Event _curEvent)
	{
		if (mZoneRect.Contains(_curEvent.mousePosition) && !mIgnoreEventRect.Contains(_curEvent.mousePosition))
		{
			if (_curEvent.type == EventType.MouseDrag)
			{
				_curEvent.Use();
			}
			if (!mActive || _curEvent.type == EventType.Used)
			{
				return;
			}
			if (mGuiElementMouseState == GuiElementMouseState.MOUSE_NONE)
			{
				mGuiElementMouseState = GuiElementMouseState.MOUSE_OVER;
				if (mOnMouseEnter != null)
				{
					mOnMouseEnter(this);
					_curEvent.Use();
				}
			}
			if (_curEvent.type == EventType.MouseDown)
			{
				if (!mMousePress && InputEnabled())
				{
					mMousePress = true;
					if (!mLocked)
					{
						if (mActionType != 0)
						{
							string comment = string.Empty;
							if (!string.IsNullOrEmpty(mActionMessage))
							{
								comment = GuiSystem.GetLocaleText(mActionMessage);
							}
							UserLog.AddAction(mActionType, comment);
						}
						if (Time.time - mLastClickTime <= 0.5f && mOnDoubleClick != null)
						{
							mOnDoubleClick(this);
							mLastClickTime = 0f;
						}
						else
						{
							if (mOnMouseDown != null)
							{
								mOnMouseDown(this, _curEvent.button);
							}
							mLastClickTime = Time.time;
						}
					}
					else if (mOnMouseLockedDown != null)
					{
						mOnMouseLockedDown(this, _curEvent.button);
					}
				}
				_curEvent.Use();
			}
			else
			{
				if (_curEvent.type != EventType.MouseUp)
				{
					return;
				}
				if (mMousePress && InputEnabled())
				{
					mMousePress = false;
					if (!mLocked)
					{
						if (mOnMouseUp != null)
						{
							mOnMouseUp(this, _curEvent.button);
						}
					}
					else if (mOnMouseLockedUp != null)
					{
						mOnMouseLockedUp(this, _curEvent.button);
					}
				}
				_curEvent.Use();
			}
		}
		else
		{
			if (mGuiElementMouseState != GuiElementMouseState.MOUSE_OVER)
			{
				return;
			}
			mGuiElementMouseState = GuiElementMouseState.MOUSE_NONE;
			if (mMousePress)
			{
				mMousePress = false;
				if (mOnDragStart != null && InputEnabled())
				{
					mOnDragStart(this);
					_curEvent.Use();
				}
			}
			if (mOnMouseLeave != null)
			{
				mOnMouseLeave(this);
			}
		}
	}

	public void AddTutorialElement(string _id, GuiElement _element)
	{
		if (mTutorialElements.ContainsKey(_id))
		{
			Log.Debug("Adding gui child with already existing id : " + _id);
			mTutorialElements[_id] = _element;
		}
		else
		{
			mTutorialElements.Add(_id, _element);
		}
	}

	public void AddTutorialElement(GuiElement _element)
	{
		AddTutorialElement(_element.mElementId, _element);
	}

	public GuiElement GetChild(string _id)
	{
		if (mElementId == _id)
		{
			return this;
		}
		mTutorialElements.TryGetValue(_id, out var value);
		return value;
	}

	public bool InputEnabled()
	{
		return TutorialMgr.IsGuiEnabled(this);
	}
}
