using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ChatWindow : GuiElement
{
	public enum ChatType
	{
		BATTLE_CHAT,
		CS_CHAT
	}

	public delegate void ShowPopUpMenuCallback(object _palyerName, Vector2 _pos);

	public ShowPopUpMenuCallback mShowPopUpMenuCallback;

	private StaticTextField mMsgTF;

	private GuiButton mSendBtn;

	private Texture2D mChatBgImage;

	private Dictionary<ChatChannel, GuiButton> mChatModeButtons;

	private List<Rect> mButtonsRect;

	private ChatChannel mCurMode = ChatChannel.area;

	private ChatZone mChatZone;

	private List<string> mMsgRecipients = new List<string>();

	private PopUpMenu mPopUpMenu;

	private bool mUpdateFocus;

	private ChatType mChatType;

	private Chat mChatMgr;

	public override void Uninit()
	{
		if (mChatMgr != null)
		{
			Chat chat = mChatMgr;
			chat.mNewMessageCallback = (Chat.NewMessageCallback)Delegate.Remove(chat.mNewMessageCallback, new Chat.NewMessageCallback(OnMessage));
			mChatMgr = null;
		}
		if (mChatZone != null)
		{
			mChatZone.Uninit();
		}
		if (mMsgTF != null)
		{
			mMsgTF.Uninit();
		}
	}

	public override void Init()
	{
		mChatZone = new ChatZone(_hasScroll: true);
		ChatZone chatZone = mChatZone;
		chatZone.mOnNick = (ChatZone.OnNick)Delegate.Combine(chatZone.mOnNick, new ChatZone.OnNick(OnNickClick));
		mChatZone.Init();
		mChatModeButtons = new Dictionary<ChatChannel, GuiButton>();
		mButtonsRect = new List<Rect>();
		foreach (int value in Enum.GetValues(typeof(ChatChannel)))
		{
			if (value != 0 && value != 9 && value != 8)
			{
				int num = value;
				string text = num.ToString();
				GuiButton guiButton = GuiSystem.CreateButton("Gui/chat/ico_" + text + "_1", "Gui/chat/ico_" + text + "_2", "Gui/chat/ico_" + text + "_3", string.Empty, string.Empty);
				guiButton.mId = value;
				guiButton.mElementId = "CHAT_MODE_BUTTON";
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				guiButton.mLocked = true;
				guiButton.SetCurBtnImage();
				guiButton.Init();
				mChatModeButtons.Add((ChatChannel)value, guiButton);
				string mElementId = guiButton.mElementId;
				int num2 = value;
				AddTutorialElement(mElementId + "_" + num2, guiButton);
			}
		}
		switch (mChatType)
		{
		case ChatType.BATTLE_CHAT:
			mChatBgImage = GuiSystem.GetImage("Gui/Chat/chat_frame1");
			break;
		case ChatType.CS_CHAT:
			mChatBgImage = GuiSystem.GetImage("Gui/Chat/chat_frame2");
			break;
		}
		mMsgTF = new StaticTextField();
		mMsgTF.mElementId = "TEXT_FIELD_MSG";
		mMsgTF.mLength = 256;
		mMsgTF.mStyleId = "text_field_1";
		mMsgTF.mAutoFocus = true;
		mSendBtn = GuiSystem.CreateButton("Gui/Chat/button_1_norm", "Gui/Chat/button_1_over", "Gui/Chat/button_1_press", string.Empty, string.Empty);
		mSendBtn.mElementId = "SEND_BUTTON";
		GuiButton guiButton2 = mSendBtn;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mSendBtn.Init();
		AddTutorialElement(mSendBtn);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(-3f, 0f, mChatBgImage.width, mChatBgImage.height);
		mMsgTF.mZoneRect = new Rect(18f, 167f, mZoneRect.width - 80f, 23f);
		mSendBtn.mZoneRect = new Rect(mZoneRect.width - 55f, 170f, 39f, 21f);
		mChatZone.mTextRect = new Rect(20f, 15f, mZoneRect.width - 60f, mZoneRect.height - 65f);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.y = (float)OptionsMgr.mScreenHeight - mZoneRect.height;
		GuiSystem.SetChildRect(mZoneRect, ref mChatZone.mTextRect);
		mChatZone.mZoneRect = mZoneRect;
		mChatZone.SetSize();
		GuiSystem.SetChildRect(mZoneRect, ref mMsgTF.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mSendBtn.mZoneRect);
		mButtonsRect.Clear();
		for (int i = 0; i < 6; i++)
		{
			Rect _rect = new Rect(47 + i * 23, -7f, 23f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref _rect);
			mButtonsRect.Add(_rect);
		}
		mIgnoreEventRect = mMsgTF.mZoneRect;
		SetChatModeButtonPos();
	}

	public override void RenderElement()
	{
		if (MainInfoWindow.mHidden)
		{
			return;
		}
		if ((bool)mChatBgImage)
		{
			GuiSystem.DrawImage(mChatBgImage, mZoneRect);
		}
		mSendBtn.RenderElement();
		mChatZone.RenderElement();
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton in mChatModeButtons)
		{
			if (!mChatModeButton.Value.mLocked)
			{
				mChatModeButton.Value.RenderElement();
			}
		}
	}

	public override void OnInput()
	{
		if (!MainInfoWindow.mHidden)
		{
			mChatZone.OnInput();
			mMsgTF.OnInput();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (GUI.GetNameOfFocusedControl() == "TEXT_FIELD_MSG")
		{
			if (mUpdateFocus)
			{
				mUpdateFocus = false;
			}
			if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Return && mMsgTF.mData != string.Empty)
			{
				SendCurMessage();
			}
		}
		else if (mUpdateFocus)
		{
			GUI.FocusControl("TEXT_FIELD_MSG");
		}
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton in mChatModeButtons)
		{
			if (!mChatModeButton.Value.mLocked)
			{
				mChatModeButton.Value.CheckEvent(_curEvent);
			}
		}
		mSendBtn.CheckEvent(_curEvent);
		mChatZone.CheckEvent(_curEvent);
		mMsgTF.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void Update()
	{
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton in mChatModeButtons)
		{
			if (!mChatModeButton.Value.mLocked)
			{
				mChatModeButton.Value.Update();
			}
		}
	}

	public void SetChatType(ChatType _type)
	{
		mChatType = _type;
	}

	public void SetChatModes(ChatChannel[] _modes)
	{
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton in mChatModeButtons)
		{
			mChatModeButton.Value.mLocked = true;
		}
		foreach (ChatChannel chatChannel in _modes)
		{
			if (mChatModeButtons.ContainsKey(chatChannel))
			{
				mChatModeButtons[chatChannel].mLocked = false;
				mChatZone.AddChannel(chatChannel);
			}
		}
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton2 in mChatModeButtons)
		{
			mChatModeButton2.Value.SetCurBtnImage();
		}
		SetChatModeButtonPos();
		if (_modes.Length > 0)
		{
			SetCurChatMode(_modes[0]);
		}
	}

	private void SetChatModeButtonPos()
	{
		int num = 0;
		foreach (KeyValuePair<ChatChannel, GuiButton> mChatModeButton in mChatModeButtons)
		{
			if (!mChatModeButton.Value.mLocked && num <= mButtonsRect.Count - 1)
			{
				mChatModeButton.Value.mZoneRect = mButtonsRect[num];
				num++;
				PopupInfo.AddTip(this, GetChannelTipText(mChatModeButton.Key), mChatModeButton.Value.mZoneRect);
			}
		}
	}

	private string GetChannelTipText(ChatChannel _channel)
	{
		return _channel switch
		{
			ChatChannel.area => "TIP_TEXT14", 
			ChatChannel.trade => "TIP_TEXT15", 
			ChatChannel.private_msg => "TIP_TEXT16", 
			ChatChannel.group => "TIP_TEXT17", 
			ChatChannel.team => "TIP_TEXT18", 
			ChatChannel.clan => "TIP_TEXT19", 
			_ => string.Empty, 
		};
	}

	private void SetCurChatMode(ChatChannel _mode)
	{
		if (mChatModeButtons.ContainsKey(_mode) && !mChatModeButtons[_mode].mLocked)
		{
			mChatModeButtons[mCurMode].Pressed = false;
			mChatModeButtons[mCurMode].SetCurBtnImage();
			mCurMode = _mode;
			mChatModeButtons[mCurMode].Pressed = true;
			mChatModeButtons[mCurMode].SetCurBtnImage();
			mChatZone.SetCurChatMode(_mode);
			UnsignalChannel(mCurMode);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ("SEND_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			SendCurMessage();
		}
		else if ("CHAT_MODE_BUTTON" == _sender.mElementId && _buttonId == 0)
		{
			SetCurChatMode((ChatChannel)_sender.mId);
		}
	}

	private void OnNickClick(string _nick, int _buttonId, Vector2 _pos)
	{
		switch (_buttonId)
		{
		case 0:
			AddRecipientToMsg(_nick);
			break;
		case 1:
			if (mPopUpMenu != null && mShowPopUpMenuCallback != null)
			{
				mShowPopUpMenuCallback(_nick, _pos);
			}
			break;
		}
	}

	private void SendCurMessage()
	{
		mMsgRecipients.Clear();
		if (mMsgTF.mData == string.Empty)
		{
			return;
		}
		if (mMsgTF.mData.StartsWith("/"))
		{
			mChatMgr.ParseAndSendMessage(mMsgTF.mData);
		}
		else
		{
			string text = Chat.ExcludeRecepientsFromMessage(mMsgTF.mData, ref mMsgRecipients);
			if (string.IsNullOrEmpty(text.Trim()))
			{
				return;
			}
			mChatMgr.SendMessage(text, mCurMode, mMsgRecipients);
		}
		mMsgRecipients.Clear();
		mMsgTF.mData = string.Empty;
	}

	private void OnMessage(IIncomingChatMsg _msg)
	{
		if (mChatZone != null && (_msg.Channel == ChatChannel.system || !PlayersListController.IsIgnore(_msg.Sender)))
		{
			mChatZone.OnMessage(_msg);
			ChatChannel channel = _msg.Channel;
			if (mCurMode != channel && channel != ChatChannel.system)
			{
				SignalChannel(channel);
			}
		}
	}

	public void AddRecipientToMsg(string _nick)
	{
		if (!mMsgRecipients.Contains(_nick))
		{
			mMsgRecipients.Add(_nick);
		}
		mMsgTF.mData = Chat.AddRecipientToMsg(mMsgTF.mData, _nick);
		mUpdateFocus = true;
	}

	private void SignalChannel(ChatChannel _mode)
	{
		if (mChatModeButtons.ContainsKey(_mode))
		{
			mChatModeButtons[_mode].Signal(0.5f);
		}
	}

	private void UnsignalChannel(ChatChannel _mode)
	{
		if (mChatModeButtons.ContainsKey(_mode))
		{
			mChatModeButtons[_mode].StopSignal();
		}
	}

	public void SetData(Chat _chat)
	{
		mChatMgr = _chat;
		Chat chat = mChatMgr;
		chat.mNewMessageCallback = (Chat.NewMessageCallback)Delegate.Combine(chat.mNewMessageCallback, new Chat.NewMessageCallback(OnMessage));
		foreach (IIncomingChatMsg message in mChatMgr.GetMessages(ChatChannel.system))
		{
			OnMessage(message);
		}
	}
}
