using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class MpdConnection : NetSystem.ConnectionListener
	{
		public enum ConnectionError
		{
			CANNOT_CONNECT = 1,
			DISCONNECT,
			AUTHORIZATION_FAILED
		}

		public delegate void ConnectionErrorCallback(ConnectionError _err, string _host, string _port);

		private ConnectionErrorCallback mConnectionErrorCallback = delegate
		{
		};

		private NetSystem mNetSys;

		private ICtrlResponseHolder mResponses;

		private bool mWaitedDisconnect;

		private MemoryStream mInputBuffer = new MemoryStream();

		private int mOffset;

		private int mNextPartSize;

		private Formatter mInFormatter = new Formatter();

		private int mId;

		private string mSid;

		[CompilerGenerated]
		private static ConnectionErrorCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public MpdConnection(NetSystem _netSys, int _id, string _sid, ICtrlResponseHolder _responses)
		{
			if (_netSys == null)
			{
				throw new ArgumentNullException("_netSys");
			}
			if (_responses == null)
			{
				throw new ArgumentNullException("_responses");
			}
			mNetSys = _netSys;
			mId = _id;
			mSid = _sid;
			mResponses = _responses;
			NeedMicroReconnect(_needReconnect: true);
		}

		public void Disconnect()
		{
			mWaitedDisconnect = true;
			mNetSys.Disconnect(base.ConnectionId);
		}

		public override void ConnectionComplete(int _connectionId, int _port)
		{
			base.ConnectionComplete(_connectionId, _port);
			SendLogin();
		}

		public override void DisconnectionComplete()
		{
			base.DisconnectionComplete();
			if (mWaitedDisconnect)
			{
				mWaitedDisconnect = false;
			}
			else if (!mReconnecting)
			{
				if (mNeedMicroReconnect)
				{
					mReconnecting = true;
					ReconnectStart(0);
					Connect();
				}
				else
				{
					mConnectionErrorCallback(ConnectionError.DISCONNECT, base.EndPoint.Address.ToString(), base.EndPoint.Port.ToString());
				}
			}
		}

		public override void ConnectionFailed()
		{
			base.ConnectionFailed();
			if (mReconnecting && mCurrentAttempt < mAttemptMaxCount)
			{
				Thread.Sleep(mAttemptTime);
				mCurrentAttempt++;
				Log.Info("MicroReconnect attempt to JMPD #" + mCurrentAttempt);
				Connect();
			}
			else
			{
				mConnectionErrorCallback(ConnectionError.CANNOT_CONNECT, base.EndPoint.Address.ToString(), base.EndPoint.Port.ToString());
			}
		}

		private void Connect()
		{
			mNetSys.Connect(base.EndPoint.Address.ToString(), new int[1]
			{
				base.EndPoint.Port
			}, this);
		}

		public override void Parse(byte[] _buffer, int _offset, int _size)
		{
			mInputBuffer.Position = mInputBuffer.Length;
			mInputBuffer.Write(_buffer, _offset, _size);
			if (mInputBuffer.Length < 4)
			{
				return;
			}
			do
			{
				if (mNextPartSize <= 0)
				{
					mInputBuffer.Position = mOffset;
					if (mInputBuffer.Length - mInputBuffer.Position < 4)
					{
						break;
					}
					mNextPartSize = ReadSize();
					mOffset += 4;
					if (mNextPartSize <= 0)
					{
						Log.Warning("invalid chunk size: " + mNextPartSize);
						continue;
					}
				}
				if (mInputBuffer.Length - mOffset < mNextPartSize)
				{
					break;
				}
				mInputBuffer.Position = mOffset;
				mOffset += mNextPartSize;
				mNextPartSize = 0;
				while (mInputBuffer.Position != mOffset)
				{
					mInFormatter.ClearRefTables();
					long position = mInputBuffer.Position;
					Variable content = mInFormatter.Deserialize(mInputBuffer);
					if (content.ValueType == typeof(int))
					{
						int num = content;
						if (num != 100)
						{
							Log.Error("mpd authorization failed, error code: " + num);
							mConnectionErrorCallback(ConnectionError.AUTHORIZATION_FAILED, base.EndPoint.Address.ToString(), base.EndPoint.Port.ToString());
						}
						continue;
					}
					Log.Info(() => "mpd packet: " + content.ToString());
					MixedArray mixedArray = content;
					foreach (KeyValuePair<string, Variable> item in mixedArray.Associative)
					{
						Enum @enum = ParseCmd(item.Key);
						if (@enum == null)
						{
							Log.Warning("unsupported command: " + item.Key + " " + item.Value.ToString());
							continue;
						}
						CtrlPacket packet = new CtrlPacket(@enum, item.Value);
						mResponses.AddResponse(packet);
					}
					if (mInputBuffer.Position <= mOffset && mInputBuffer.Position > position)
					{
						continue;
					}
					Log.Warning("wrong buffer position");
					break;
				}
			}
			while (mInputBuffer.Length - mInputBuffer.Position > 0);
			mInFormatter.ClearRefTables();
			if (mOffset == mInputBuffer.Length)
			{
				mInputBuffer.SetLength(0L);
				mOffset = 0;
			}
		}

		private int ReadSize()
		{
			byte[] array = new byte[4];
			mInputBuffer.Read(array, 0, array.Length);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array);
			}
			return BitConverter.ToInt32(array, 0);
		}

		protected Enum ParseCmd(string _str)
		{
			return CtrlCmdId.Parse(_str + "_mpd");
		}

		public void SendLogin()
		{
			MemoryStream memoryStream = new MemoryStream();
			byte[] array = new byte[4];
			memoryStream.Write(array, 0, array.Length);
			MixedArray mixedArray = new MixedArray();
			mixedArray["id"] = mId;
			mixedArray["sid"] = mSid;
			Formatter formatter = new Formatter();
			long num = formatter.Serialize(mixedArray, memoryStream);
			array = BitConverter.GetBytes((int)num);
			memoryStream.Position = 0L;
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array);
			}
			memoryStream.Write(array, 0, array.Length);
			memoryStream.Position = memoryStream.Length;
			mNetSys.Send(base.ConnectionId, memoryStream.ToArray(), null);
		}

		public void SubscribeConnectionError(ConnectionErrorCallback _callback)
		{
			mConnectionErrorCallback = (ConnectionErrorCallback)Delegate.Combine(mConnectionErrorCallback, _callback);
		}

		public void UnsubscribeConnectionError(ConnectionErrorCallback _callback)
		{
			mConnectionErrorCallback = (ConnectionErrorCallback)Delegate.Remove(mConnectionErrorCallback, _callback);
		}
	}
}
