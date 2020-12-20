using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ChatZone : GuiElement
{
	private class ChatMsg
	{
		public bool mVisible = true;

		public GuiButton mNickButton;

		public string mMessage = string.Empty;

		public Rect mDrawRect = default(Rect);

		public string mFromNick = string.Empty;

		public string mStyle = string.Empty;
	}

	public delegate void OnNick(string _nick, int _buttonId, Vector2 _pos);

	public OnNick mOnNick;

	public Rect mTextRect;

	public string mStyle = "middle_left";

	public Color mColor = Color.white;

	private Dictionary<ChatChannel, Queue<ChatMsg>> mChatMsgs;

	private List<IIncomingChatMsg> mMsgsToAdd;

	private ChatChannel mCurMode = ChatChannel.area;

	private int mMaxMsgs = 64;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private float mLastTextHeight;

	private bool mHasScroll;

	public ChatZone(bool _hasScroll)
	{
		mHasScroll = _hasScroll;
	}

	public override void Init()
	{
		mChatMsgs = new Dictionary<ChatChannel, Queue<ChatMsg>>();
		mMsgsToAdd = new List<IIncomingChatMsg>();
		if (mHasScroll)
		{
			mScrollbar = new VerticalScrollbar();
			mScrollbar.Init();
			VerticalScrollbar verticalScrollbar = mScrollbar;
			verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		}
	}

	public override void SetSize()
	{
		mTextRect.x = Mathf.RoundToInt(mTextRect.x);
		mTextRect.y = Mathf.RoundToInt(mTextRect.y);
		mTextRect.width = Mathf.RoundToInt(mTextRect.width);
		mTextRect.height = Mathf.RoundToInt(mTextRect.height);
		mStartScrollOffset = mTextRect.height;
		if (mScrollbar != null)
		{
			mScrollbar.mZoneRect = new Rect(mTextRect.x + mTextRect.width, mTextRect.y, 22f * GuiSystem.mYRate, mTextRect.height);
			mScrollbar.SetSize();
		}
		SetMsgsRect();
	}

	public override void Uninit()
	{
		if (mChatMsgs != null)
		{
			mChatMsgs.Clear();
		}
		if (mMsgsToAdd != null)
		{
			mMsgsToAdd.Clear();
		}
	}

	public override void OnInput()
	{
		UpdateMsgs();
		if (mScrollbar != null)
		{
			mScrollbar.OnInput();
		}
	}

	public override void RenderElement()
	{
		if (!mChatMsgs.TryGetValue(mCurMode, out var value))
		{
			return;
		}
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, mTextRect.width, mTextRect.height, 0f);
		GL.Viewport(new Rect(mTextRect.x, (float)OptionsMgr.mScreenHeight - mTextRect.height - mTextRect.y, mTextRect.width, mTextRect.height));
		foreach (ChatMsg item in value)
		{
			if (item.mVisible)
			{
				GUI.contentColor = mColor;
				GuiSystem.DrawString(item.mMessage, item.mDrawRect, item.mStyle);
				if (item.mNickButton != null)
				{
					item.mNickButton.RenderElement();
				}
			}
		}
		GUI.contentColor = Color.white;
		GL.Viewport(new Rect(0f, 0f, OptionsMgr.mScreenWidth, OptionsMgr.mScreenHeight));
		GL.PopMatrix();
		if (mScrollbar != null)
		{
			mScrollbar.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mScrollbar != null)
		{
			mScrollbar.CheckEvent(_curEvent);
		}
		if (!mChatMsgs.TryGetValue(mCurMode, out var value))
		{
			return;
		}
		Vector2 mousePosition = _curEvent.mousePosition;
		mousePosition.x -= mTextRect.x;
		mousePosition.y -= mTextRect.y;
		_curEvent.mousePosition = mousePosition;
		foreach (ChatMsg item in value)
		{
			if (item.mVisible && item.mNickButton != null)
			{
				item.mNickButton.CheckEvent(_curEvent);
			}
		}
		mousePosition.x += mTextRect.x;
		mousePosition.y += mTextRect.y;
		_curEvent.mousePosition = mousePosition;
	}

	public void AddChannel(ChatChannel _channel)
	{
		if (!mChatMsgs.ContainsKey(_channel))
		{
			mChatMsgs.Add(_channel, new Queue<ChatMsg>());
		}
	}

	public void SetCurChatMode(ChatChannel _mode)
	{
		mCurMode = _mode;
		mLastTextHeight = 0f;
		mScrollOffset = -1f;
		if (mScrollbar != null)
		{
			mScrollbar.SetData(mStartScrollOffset, mLastTextHeight);
			mScrollbar.Refresh(_inverse: true);
		}
	}

	public void OnMessage(IIncomingChatMsg _arg)
	{
		if (_arg.Channel == ChatChannel.system || !PlayersListController.IsIgnore(_arg.Sender))
		{
			mMsgsToAdd.Add(_arg);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (mScrollOffset != _offset)
		{
			mScrollOffset = _offset;
			SetMsgsRect();
		}
	}

	private void SetMsgsRect()
	{
		if (!mChatMsgs.TryGetValue(mCurMode, out var value))
		{
			return;
		}
		mLastTextHeight = 0f;
		foreach (ChatMsg item in value)
		{
			item.mDrawRect.y = mLastTextHeight - mScrollOffset;
			if (item.mNickButton != null)
			{
				item.mNickButton.mZoneRect.y = item.mDrawRect.y;
			}
			item.mVisible = item.mDrawRect.y + item.mDrawRect.height >= 0f && item.mDrawRect.y <= mTextRect.height;
			mLastTextHeight += item.mDrawRect.height;
		}
		float num = mLastTextHeight - mStartScrollOffset;
		if (mScrollbar != null)
		{
			mScrollbar.SetData(mStartScrollOffset, mLastTextHeight);
			if (num > 0f && num - mScrollbar.GetValue() < 5f)
			{
				mScrollbar.Refresh(_inverse: true);
			}
		}
		else if (num > 0f)
		{
			OnScrollbar(null, num);
		}
	}

	private void UpdateMsgs()
	{
		if (mMsgsToAdd.Count == 0)
		{
			return;
		}
		foreach (IIncomingChatMsg item in mMsgsToAdd)
		{
			AddMsg(item);
		}
		mMsgsToAdd.Clear();
		SetMsgsRect();
	}

	private void AddMsg(IIncomingChatMsg _msg)
	{
		ChatChannel channel = _msg.Channel;
		bool flag = channel == ChatChannel.system;
		channel = ((!flag) ? channel : (mChatMsgs.ContainsKey(ChatChannel.area) ? ChatChannel.area : ChatChannel.fight));
		if (!mChatMsgs.ContainsKey(channel))
		{
			return;
		}
		if (mChatMsgs[channel].Count == mMaxMsgs)
		{
			ChatMsg chatMsg = mChatMsgs[channel].Peek();
			if (chatMsg.mNickButton != null)
			{
				GuiButton mNickButton = chatMsg.mNickButton;
				mNickButton.mOnMouseUp = (OnMouseUp)Delegate.Remove(mNickButton.mOnMouseUp, new OnMouseUp(OnButton));
			}
			mChatMsgs[channel].Dequeue();
		}
		ChatMsg item = InitChatMsg(_msg, channel, flag);
		mChatMsgs[channel].Enqueue(item);
	}

	private ChatMsg InitChatMsg(IIncomingChatMsg _arg, ChatChannel _channel, bool _system)
	{
		DateTime time = _arg.Time;
		GUIStyle gUIStyle = ((!_system) ? GUI.skin.GetStyle(mStyle) : GUI.skin.GetStyle("middle_left_italic"));
		string text = string.Empty;
		ChatMsg chatMsg = new ChatMsg();
		chatMsg.mStyle = ((!_system) ? mStyle : "middle_left_italic");
		string text2 = time.Hour.ToString("0#");
		string text3 = time.Minute.ToString("0#");
		string text4 = text2 + ":" + text3 + " ";
		Vector2 vector = gUIStyle.CalcSize(new GUIContent(text4));
		Vector2 vector2 = gUIStyle.CalcSize(new GUIContent(_arg.Sender));
		if (!_system)
		{
			chatMsg.mNickButton = new GuiButton();
			chatMsg.mNickButton.mElementId = "NICK_BUTTON";
			chatMsg.mNickButton.mLabel = _arg.Sender;
			chatMsg.mNickButton.mZoneRect = new Rect(vector.x + 3f, 0f, vector2.x, vector2.y);
			chatMsg.mNickButton.mNormColor = mColor;
			chatMsg.mNickButton.mOverColor = Color.yellow;
			chatMsg.mNickButton.mPressColor = Color.green;
			chatMsg.mNickButton.mLabelStyle = mStyle;
			GuiButton mNickButton = chatMsg.mNickButton;
			mNickButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mNickButton.mOnMouseUp, new OnMouseUp(OnButton));
			chatMsg.mNickButton.Init();
		}
		chatMsg.mFromNick = _arg.Sender;
		foreach (string recipient in _arg.Recipients)
		{
			text = text + "[" + recipient + "]";
		}
		if (text.Length == 0)
		{
			chatMsg.mMessage = text4 + _arg.Sender + ": " + _arg.Text;
		}
		else
		{
			chatMsg.mMessage = text4 + _arg.Sender + ">>" + text + ": " + _arg.Text;
		}
		chatMsg.mDrawRect = new Rect(0f, 0f, mTextRect.width, 0f);
		float height = gUIStyle.CalcHeight(new GUIContent(chatMsg.mMessage), chatMsg.mDrawRect.width);
		chatMsg.mDrawRect.height = height;
		return chatMsg;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (!("NICK_BUTTON" == _sender.mElementId))
		{
			return;
		}
		GuiButton guiButton = _sender as GuiButton;
		if (guiButton != null)
		{
			string mLabel = guiButton.mLabel;
			if (mOnNick != null)
			{
				mOnNick(mLabel, _buttonId, new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y));
			}
		}
	}
}
