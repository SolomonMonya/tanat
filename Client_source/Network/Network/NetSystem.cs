using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Log4Tanat;

namespace Network
{
	public class NetSystem
	{
		public abstract class ConnectionListener
		{
			protected int mLastPort = -1;

			private IPEndPoint mEndPoint;

			private int mConnectionId = -1;

			protected bool mReconnecting;

			protected bool mNeedMicroReconnect;

			protected int mAttemptMaxCount = 40;

			protected int mAttemptTime = 1000;

			protected int mCurrentAttempt;

			public int ConnectionId => mConnectionId;

			public bool IsConnectedMode => mConnectionId != -1;

			public IPEndPoint EndPoint
			{
				get
				{
					return mEndPoint;
				}
				set
				{
					mEndPoint = value;
				}
			}

			public event Action<int> mReconnectStarted;

			public event Action<bool> mReconnectStoped;

			protected void ReconnectStart(int _attempt)
			{
				if (this.mReconnectStarted != null)
				{
					this.mReconnectStarted(mAttemptMaxCount - _attempt);
				}
			}

			protected void ReconnectStop(bool _isConnected)
			{
				if (this.mReconnectStoped != null)
				{
					this.mReconnectStoped(_isConnected);
				}
			}

			public void SetAttemptTime(int _milliseconds)
			{
				mAttemptTime = _milliseconds;
			}

			public void SetAttemptCount(int _count)
			{
				mAttemptMaxCount = _count;
			}

			public void NeedMicroReconnect(bool _needReconnect)
			{
				mNeedMicroReconnect = _needReconnect;
			}

			public virtual void ConnectionComplete(int _id, int _port)
			{
				mConnectionId = _id;
				mLastPort = _port;
				if (mReconnecting)
				{
					ReconnectStop(_isConnected: true);
				}
				mReconnecting = false;
				mCurrentAttempt = 0;
			}

			public virtual void DisconnectionComplete()
			{
				mConnectionId = -1;
			}

			public virtual void ConnectionFailed()
			{
				if (mReconnecting && mCurrentAttempt >= mAttemptMaxCount)
				{
					ReconnectStop(_isConnected: false);
				}
			}

			public abstract void Parse(byte[] _buffer, int _offset, int _size);
		}

		public interface ITimeRegisterer
		{
			void RegisterSendingTime();

			void RegisterReceivingTime();
		}

		private class Connection
		{
			public ConnectionListener mListener;

			public Socket mSocket;

			public byte[] mBuffer = new byte[1024];

			public int mId = -1;

			public ManualResetEvent mSig = new ManualResetEvent(initialState: false);

			public int[] mPorts;

			public string mHost;
		}

		private class State
		{
			private Connection mConnection;

			private DateTime mBeginTime;

			private DateTime mEndTime;

			public int mSentBytes;

			public IEnumerable<ITimeRegisterer> mTimeRegisterers;

			public Connection Connection => mConnection;

			public Socket Socket => mConnection.mSocket;

			public double Time => mEndTime.Subtract(mBeginTime).TotalSeconds;

			public State(Connection _connection)
			{
				mConnection = _connection;
			}

			public void RegBegin()
			{
				mBeginTime = DateTime.Now;
			}

			public void RegEnd()
			{
				mEndTime = DateTime.Now;
			}
		}

		private Dictionary<int, Connection> mConnections = new Dictionary<int, Connection>();

		private int mNextConnectionId = 1;

		private Connection GetConnection(int _connectionId)
		{
			Connection value = null;
			bool flag = false;
			lock (mConnections)
			{
				if (mConnections.TryGetValue(_connectionId, out value))
				{
					if (!value.mSocket.Connected)
					{
						Log.Warning("connection " + _connectionId + " lost");
						flag = true;
						value = null;
					}
				}
				else
				{
					Log.Warning("connection " + _connectionId + " not exists");
				}
			}
			if (flag)
			{
				Disconnect(_connectionId);
			}
			return value;
		}

		public bool IsConnectionExists(int _connectionId)
		{
			lock (mConnections)
			{
				return mConnections.ContainsKey(_connectionId);
			}
		}

		public bool IsConnected(int _connectionId)
		{
			lock (mConnections)
			{
				Connection value = null;
				if (mConnections.TryGetValue(_connectionId, out value))
				{
					return value.mSocket.Connected;
				}
				return false;
			}
		}

		public bool IsConnected(ConnectionListener _listener)
		{
			return IsConnected(_listener.ConnectionId);
		}

		public bool Connect(string _host, int[] _ports, ConnectionListener _listener)
		{
			if (_listener == null)
			{
				throw new ArgumentNullException("_listener");
			}
			if (string.IsNullOrEmpty(_host))
			{
				throw new ArgumentNullException("_host");
			}
			if (_ports.Length == 0)
			{
				throw new ArgumentNullException("_ports");
			}
			lock (mConnections)
			{
				foreach (Connection value in mConnections.Values)
				{
					if (value.mListener == _listener)
					{
						Log.Warning("connection with the same listener already exists");
						return false;
					}
				}
			}
			Connection connection = new Connection();
			connection.mListener = _listener;
			connection.mPorts = _ports;
			connection.mHost = _host;
			IPAddress address;
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(_host);
				IPAddress[] addressList = hostEntry.AddressList;
				if (addressList.Length == 0)
				{
					Log.Error("wrong host " + _host);
					return false;
				}
				address = addressList[0];
			}
			catch (SocketException)
			{
				Log.Warning("Dns.GetHostEntry failed");
				try
				{
					address = IPAddress.Parse(_host);
				}
				catch (FormatException)
				{
					Log.Error("wrong ip address " + _host);
					return false;
				}
			}
			try
			{
				connection.mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint iPEndPoint = new IPEndPoint(address, _ports[0]);
				connection.mListener.EndPoint = iPEndPoint;
				Log.Notice("begin connect to : " + _host + ":" + _ports[0]);
				State state = new State(connection);
				state.RegBegin();
				connection.mSocket.BeginConnect(iPEndPoint, OnConnect, state);
			}
			catch (SocketException ex3)
			{
				Log.Error("begin connect failed: " + ex3.Message);
				return false;
			}
			return true;
		}

		private void OnConnect(IAsyncResult _ar)
		{
			State state = _ar.AsyncState as State;
			bool flag = true;
			try
			{
				state.Socket.EndConnect(_ar);
				if (state.Socket.Connected)
				{
					WaitReceive(state.Connection);
					lock (mConnections)
					{
						int num = mNextConnectionId++;
						state.Connection.mId = num;
						mConnections.Add(num, state.Connection);
					}
					int port = state.Connection.mPorts[0];
					Log.Notice("connection complete id : " + state.Connection.mId + " on port : " + port.ToString());
					state.Connection.mListener.ConnectionComplete(state.Connection.mId, port);
					state.Connection.mSig.Set();
					flag = false;
				}
			}
			catch (SocketException ex)
			{
				Log.Warning("socket exception: " + ex.Message);
			}
			state.RegEnd();
			if (flag)
			{
				int num2 = 0;
				state.Socket.Close();
				if (state.Connection.mPorts.Length > 1)
				{
					List<int> list = new List<int>(state.Connection.mPorts);
					num2 = list[0];
					list.RemoveAt(0);
					state.Connection.mPorts = list.ToArray();
					Log.Warning("can't connect on port : " + num2 + " trying next port : " + state.Connection.mPorts[0]);
					Connect(state.Connection.mHost, state.Connection.mPorts, state.Connection.mListener);
				}
				else
				{
					Log.Warning("cannot connect to all existing ports");
					state.Connection.mListener.ConnectionFailed();
				}
			}
		}

		public void Disconnect(int _connectionId)
		{
			Connection value;
			lock (mConnections)
			{
				if (!mConnections.TryGetValue(_connectionId, out value))
				{
					return;
				}
				mConnections.Remove(_connectionId);
			}
			try
			{
				if (value.mSocket.Connected)
				{
					value.mSocket.Shutdown(SocketShutdown.Both);
				}
				value.mSocket.Close();
				Log.Debug("socket closed");
			}
			catch (SocketException ex)
			{
				Log.Warning("socket exception: " + ex.Message);
			}
			catch (ObjectDisposedException)
			{
				Log.Warning("socket not exists");
			}
			Log.Notice("disconnection complete id : " + _connectionId);
			value.mListener.DisconnectionComplete();
		}

		public void DisconnectAll()
		{
			Log.Debug("begin");
			List<int> list = null;
			lock (mConnections)
			{
				if (mConnections.Count > 0)
				{
					list = new List<int>(mConnections.Keys);
				}
			}
			if (list != null)
			{
				foreach (int item in list)
				{
					mConnections[item].mListener.NeedMicroReconnect(_needReconnect: false);
					Disconnect(item);
				}
			}
			Log.Debug("end");
		}

		private void WaitReceive(Connection _connection)
		{
			State state = new State(_connection);
			state.RegBegin();
			state.Socket.BeginReceive(state.Connection.mBuffer, 0, state.Connection.mBuffer.Length, SocketFlags.None, OnReceived, state);
		}

		private void OnReceived(IAsyncResult _ar)
		{
			State state = _ar.AsyncState as State;
			bool flag = true;
			try
			{
				int num = state.Socket.EndReceive(_ar);
				state.Connection.mSig.WaitOne();
				if (num > 0)
				{
					state.Connection.mListener.Parse(state.Connection.mBuffer, 0, num);
					WaitReceive(state.Connection);
					flag = false;
				}
				else
				{
					if (num < 0)
					{
						Log.Warning("error code " + num + " received");
					}
					state.Socket.Close();
					Log.Debug("socket closed");
				}
			}
			catch (SocketException ex)
			{
				Log.Warning("socket exception: " + ex.Message);
			}
			catch (ObjectDisposedException)
			{
			}
			state.RegEnd();
			if (flag)
			{
				Disconnect(state.Connection.mId);
			}
		}

		public bool Send(int _connectionId, byte[] _data, IEnumerable<ITimeRegisterer> _timeRegisterers)
		{
			Connection connection = GetConnection(_connectionId);
			if (connection == null)
			{
				return false;
			}
			State state = new State(connection);
			state.mSentBytes = _data.Length;
			state.mTimeRegisterers = _timeRegisterers;
			if (state.mTimeRegisterers != null)
			{
				foreach (ITimeRegisterer mTimeRegisterer in state.mTimeRegisterers)
				{
					mTimeRegisterer.RegisterSendingTime();
				}
			}
			state.RegBegin();
			try
			{
				state.Socket.BeginSend(_data, 0, _data.Length, SocketFlags.None, OnSent, state);
				return true;
			}
			catch (SocketException ex)
			{
				Log.Warning("socket exception: " + ex.Message);
			}
			catch (ObjectDisposedException)
			{
				Log.Warning("socket not exists");
			}
			Disconnect(_connectionId);
			return false;
		}

		private void OnSent(IAsyncResult _ar)
		{
			State state = _ar.AsyncState as State;
			try
			{
				int num = state.Socket.EndSend(_ar);
				if (num != state.mSentBytes)
				{
					Log.Warning("sending error in connection " + state.Connection.mId);
				}
			}
			catch (SocketException ex)
			{
				Log.Warning("socket exception: " + ex.Message);
			}
			catch (ObjectDisposedException)
			{
				Log.Warning("socket not exists");
			}
			state.RegEnd();
		}
	}
}
