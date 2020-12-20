using System;
using System.Collections.Generic;
using Log4Tanat;

namespace TanatKernel
{
	public class Chat
	{
		public enum Error
		{
			GAG
		}

		public delegate void NewMessageCallback(IIncomingChatMsg _msg);

		public delegate void OnErrorCallback(Error _errorType);

		private class IncomingMsg : IIncomingChatMsg
		{
			private ChatChannel mChannel;

			private string mText;

			private DateTime mTime;

			private string mSender;

			private string[] mRecipients;

			public ChatChannel Channel => mChannel;

			public string Text => mText;

			public DateTime Time => mTime;

			public string Sender => mSender;

			public IEnumerable<string> Recipients => mRecipients;

			public IncomingMsg(ChatChannel _channel, ChatMsgArg _arg)
			{
				mChannel = _channel;
				mText = _arg.mMsg;
				mSender = _arg.mFrom;
				mTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				mTime = mTime.AddSeconds(_arg.mTime);
				mTime = mTime.ToLocalTime();
				mRecipients = new string[_arg.mRecipients.Length];
				Array.Copy(_arg.mRecipients, mRecipients, _arg.mRecipients.Length);
			}

			public IncomingMsg(ChatChannel _channel, string _txt)
			{
				mChannel = _channel;
				mText = _txt;
				mTime = DateTime.Now;
				mRecipients = new string[0];
			}

			public override string ToString()
			{
				return mChannel.ToString() + " [" + mSender + "] " + mText;
			}
		}

		public NewMessageCallback mNewMessageCallback;

		public OnErrorCallback mOnErrorCallback;

		private CtrlServerConnection mCtrlSrv;

		private Dictionary<char, ChatChannel> mShortcuts = new Dictionary<char, ChatChannel>();

		private int mMsgsLim = 50;

		private Dictionary<ChatChannel, Queue<IIncomingChatMsg>> mIncomings = new Dictionary<ChatChannel, Queue<IIncomingChatMsg>>();

		public Chat(CtrlServerConnection _ctrlSrv)
		{
			if (_ctrlSrv == null)
			{
				throw new ArgumentNullException("_ctrlSrv");
			}
			mCtrlSrv = _ctrlSrv;
			mShortcuts.Add('a', ChatChannel.area);
			mShortcuts.Add('t', ChatChannel.trade);
			mShortcuts.Add('c', ChatChannel.clan);
			mShortcuts.Add('g', ChatChannel.group);
			mShortcuts.Add('w', ChatChannel.private_msg);
			mShortcuts.Add('k', ChatChannel.team);
			mShortcuts.Add('f', ChatChannel.fight);
			mShortcuts.Add('r', ChatChannel.fight_request);
			mShortcuts.Add('s', ChatChannel.system);
			foreach (Enum value in Enum.GetValues(typeof(ChatChannel)))
			{
				mIncomings.Add((ChatChannel)(object)value, new Queue<IIncomingChatMsg>());
			}
		}

		public void Subscribe()
		{
			mCtrlSrv.EntryPoint.HandlerMgr.Subscribe<ChatMsgArg>(CtrlCmdId.chat.message_mpd, null, null, OnMessage);
			mCtrlSrv.EntryPoint.HandlerMgr.Subscribe(CtrlCmdId.chat.add, null, OnMessageError);
		}

		public void Unsubscribe()
		{
			mCtrlSrv.EntryPoint.HandlerMgr.Unsubscribe(this);
		}

		public void ParseAndSendMessage(string _text)
		{
			if (string.IsNullOrEmpty(_text))
			{
				Log.Warning("empty message");
				return;
			}
			_text = _text.TrimStart(' ');
			if (_text.Length < 2)
			{
				Log.Warning("empty message");
				return;
			}
			string text = "/gag";
			ChatChannel value;
			if (_text.StartsWith(text))
			{
				_text = _text.Substring(text.Length);
				List<string> list = new List<string>();
				ParseRecipients(ref _text, list);
				foreach (string item in list)
				{
					string[] array = item.Split(new char[1]
					{
						':'
					}, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length != 2)
					{
						Log.Warning("invalid gag format: " + item);
						continue;
					}
					if (!int.TryParse(array[1], out var result))
					{
						Log.Warning("cannot parse gag duration: " + item);
						continue;
					}
					mCtrlSrv.SendSetGag(array[0], result);
					Log.Debug("gag sent: user " + array[0] + " duration " + result);
				}
			}
			else if (_text[0] != '/' || !mShortcuts.TryGetValue(_text[1], out value))
			{
				Log.Warning("cannot parse channel from " + _text);
			}
			else
			{
				ParseAndSendMessage(_text.Substring(2), value);
			}
		}

		public void ParseAndSendMessage(string _text, ChatChannel _channel, IEnumerable<string> _addRecipients)
		{
			if (string.IsNullOrEmpty(_text))
			{
				Log.Warning("empty message");
				return;
			}
			List<string> list = new List<string>();
			ParseRecipients(ref _text, list);
			if (_addRecipients != null)
			{
				list.AddRange(_addRecipients);
			}
			SendMessage(_text, _channel, list);
		}

		public void ParseAndSendMessage(string _text, ChatChannel _channel)
		{
			ParseAndSendMessage(_text, _channel, new string[0]);
		}

		public void SendMessage(string _text, ChatChannel _channel, IEnumerable<string> _recipients)
		{
			mCtrlSrv.SendChatMessage(_text, _channel.ToString(), _recipients);
		}

		private void ParseRecipients(ref string _text, List<string> _recipients)
		{
			while (!string.IsNullOrEmpty(_text))
			{
				_text = _text.TrimStart(' ');
				if (!_text.StartsWith("["))
				{
					break;
				}
				int num = _text.IndexOf(']', 1);
				if (num == -1)
				{
					break;
				}
				string text = _text.Substring(1, num - 1);
				if (!string.IsNullOrEmpty(text))
				{
					_recipients.Add(text);
				}
				_text = _text.Substring(num + 1);
			}
		}

		public static string ExcludeRecepientsFromMessage(string _text, ref List<string> _names)
		{
			int _symbNum = -1;
			GetRecipients(_text, ref _names, ref _symbNum);
			_symbNum++;
			return _text.Substring(_symbNum, _text.Length - _symbNum);
		}

		public static string AddRecipientToMsg(string _msg, string _rec)
		{
			List<string> _names = new List<string>();
			string text = "[" + _rec + "]";
			int _symbNum = -1;
			GetRecipients(_msg, ref _names, ref _symbNum);
			_symbNum++;
			if (!_msg.Contains(text) && _symbNum > 0)
			{
				string text2 = _msg.Substring(0, _symbNum);
				string text3 = _msg.Substring(_symbNum, _msg.Length - _symbNum);
				_msg = text2 + text + text3;
			}
			else if (!_msg.Contains(text) && _symbNum == 0)
			{
				_msg += text;
			}
			return _msg;
		}

		public static void GetRecipients(string _msg, ref List<string> _names, ref int _symbNum)
		{
			bool flag = false;
			string text = "";
			int i = 0;
			for (int length = _msg.Length; i < length; i++)
			{
				if (!flag)
				{
					if ('[' == _msg[i])
					{
						flag = true;
						text = "";
					}
				}
				else if (']' != _msg[i])
				{
					text += _msg[i];
				}
				else
				{
					flag = false;
					_symbNum = i;
					_names.Add(text);
				}
			}
		}

		private void OnMessage(ChatMsgArg _arg)
		{
			ChatChannel channel;
			try
			{
				channel = (ChatChannel)Enum.Parse(typeof(ChatChannel), _arg.mType, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				Log.Warning("unknown chat channel " + _arg.mType);
				return;
			}
			IncomingMsg msg = new IncomingMsg(channel, _arg);
			Log.Info(() => "received: " + msg.ToString());
			AddMessage(msg);
		}

		private void AddMessage(IncomingMsg _msg)
		{
			Queue<IIncomingChatMsg> queue = mIncomings[_msg.Channel];
			queue.Enqueue(_msg);
			int num = queue.Count - mMsgsLim;
			while (num-- > 0)
			{
				queue.Dequeue();
			}
			if (mNewMessageCallback != null)
			{
				mNewMessageCallback(_msg);
			}
		}

		public void AddSystemMessage(string _txt)
		{
			IncomingMsg msg = new IncomingMsg(ChatChannel.system, _txt);
			AddMessage(msg);
		}

		private void OnMessageError(int _errorCode)
		{
			if (_errorCode == 4012 && mOnErrorCallback != null)
			{
				mOnErrorCallback(Error.GAG);
			}
		}

		public IEnumerable<IIncomingChatMsg> GetMessages(ChatChannel _channel)
		{
			return mIncomings[_channel];
		}

		public void Clear()
		{
			foreach (Queue<IIncomingChatMsg> value in mIncomings.Values)
			{
				value.Clear();
			}
		}
	}
}
