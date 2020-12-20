using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class TradeManager
	{
		public delegate void OnStateChangedCallback(TradeManager _tradeMgr);

		public enum Text
		{
			START_REQUEST = 0,
			START_DENIED = 1,
			START_ERROR = 2,
			START_ERROR_SELF_LVL = 3019,
			START_ERROR_PLYER_LVL = 3020,
			START_ERROR_PLYER_IGNORE = 3021
		}

		public enum State
		{
			STARTED = 1,
			READY_SELF = 2,
			READY_OPPONENT = 4,
			CONFIRM_SELF = 8,
			CONFIRM_OPPONENT = 0x10
		}

		private class Session
		{
			public int mOpponentId;

			public int mState;

			public int mOpponentMoney;

			public Dictionary<int, int> mOpponentItems = new Dictionary<int, int>();

			public int mSelfMoney;

			public Dictionary<int, int> mSelfItems = new Dictionary<int, int>();

			public bool mCheckCrossStart;
		}

		private OnStateChangedCallback mOnStateChangedCallback = delegate
		{
		};

		private Session mCurSession;

		private IGuiView mGuiView;

		private TradeSender mSender;

		private IStoreContentProvider<Player> mPlayerProv;

		private HandlerManager<CtrlPacket, Enum> mHandlerMgr;

		[CompilerGenerated]
		private static OnStateChangedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public bool IsSessionOpened => mCurSession != null;

		public TradeManager(TradeSender _sender, IStoreContentProvider<Player> _playerProv)
		{
			if (_sender == null)
			{
				throw new ArgumentNullException("_sender");
			}
			if (_playerProv == null)
			{
				throw new ArgumentNullException("_playerProv");
			}
			mSender = _sender;
			mPlayerProv = _playerProv;
		}

		public void Subscribe(HandlerManager<CtrlPacket, Enum> _handlerMgr, IGuiView _guiView)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			if (_guiView == null)
			{
				throw new ArgumentNullException("_guiView");
			}
			mHandlerMgr = _handlerMgr;
			mGuiView = _guiView;
			mHandlerMgr.Subscribe(CtrlCmdId.trade.start, OnTradeStart, OnTradeStartError);
			mHandlerMgr.Subscribe<TradeStartArg>(CtrlCmdId.trade.start_mpd, null, null, OnTradeStartOffer);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.start_answer, OnTradeStartAnswere, OnCommonError);
			mHandlerMgr.Subscribe<TradeStartAnsArg>(CtrlCmdId.trade.start_answer_mpd, null, null, OnTradeStartAnswered);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.cancel, OnTradeCancel, OnCommonError);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.cancel_mpd, OnTradeCancelOpponent, null);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.ready, OnTradeReady, OnCommonError);
			mHandlerMgr.Subscribe<TradeReadyArg>(CtrlCmdId.trade.ready_mpd, null, null, OnTradeReadyOpponent);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.not_ready, OnTradeNotReady, OnCommonError);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.not_ready_mpd, OnTradeNotReadyOpponent, null);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.confirm, OnTradeConfirm, OnCommonError);
			mHandlerMgr.Subscribe(CtrlCmdId.trade.confirm_mpd, OnTradeConfirmOpponent, null);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
			mGuiView = null;
		}

		public void SubscribeOnStateChanged(OnStateChangedCallback _callback)
		{
			mOnStateChangedCallback = (OnStateChangedCallback)Delegate.Combine(mOnStateChangedCallback, _callback);
		}

		public void UnsubscribeOnStateChanged(OnStateChangedCallback _callback)
		{
			mOnStateChangedCallback = (OnStateChangedCallback)Delegate.Remove(mOnStateChangedCallback, _callback);
		}

		private void OnTradeStart()
		{
			if (mCurSession == null)
			{
				Log.Error("session not created");
			}
			else if (mCurSession.mCheckCrossStart)
			{
				if (mGuiView != null)
				{
					mGuiView.Skip(GetType());
				}
				AddToState(State.STARTED);
			}
		}

		private void OnCommonError(int _errorCode)
		{
			CloseSession(_skipGui: true);
		}

		private void OnTradeStartError(int _errorCode)
		{
			OnCommonError(_errorCode);
			mSender.Cancel();
			if (Enum.IsDefined(typeof(Text), _errorCode))
			{
				mGuiView.Inform(GetType(), _errorCode, null);
			}
			else
			{
				mGuiView.Inform(GetType(), 2, null);
			}
		}

		private void OnTradeStartOffer(TradeStartArg _arg)
		{
			Player player = mPlayerProv.Get(_arg.mUserId);
			if (player == null)
			{
				return;
			}
			mCurSession = new Session();
			mCurSession.mOpponentId = _arg.mUserId;
			mCurSession.mCheckCrossStart = true;
			Notifier<IGuiView, object> notifier = new Notifier<IGuiView, object>();
			notifier.mCallback = delegate(bool _success, IGuiView _owner, object _data)
			{
				mSender.Answare(_success);
				if (_success)
				{
					AddToState(State.STARTED);
				}
				else
				{
					CloseSession(_skipGui: true);
				}
			};
			mGuiView.Ask(GetType(), 0, player.Name, notifier);
		}

		private void OnTradeStartAnswere()
		{
		}

		private void OnTradeStartAnswered(TradeStartAnsArg _arg)
		{
			if (_arg.mAnswer)
			{
				AddToState(State.STARTED);
				return;
			}
			string textData = "";
			if (mCurSession != null)
			{
				Player player = mPlayerProv.Get(mCurSession.mOpponentId);
				if (player != null)
				{
					textData = player.Name;
				}
			}
			mGuiView.Inform(GetType(), 1, textData);
			CloseSession(_skipGui: false);
		}

		private void OnTradeCancel()
		{
			CloseSession(_skipGui: true);
			mSender.UpdateBag();
		}

		private void OnTradeCancelOpponent()
		{
			UserLog.AddAction(UserActionType.TRADE_BROKEN);
			if (mCurSession != null)
			{
				string textData = "";
				Player player = mPlayerProv.Get(mCurSession.mOpponentId);
				if (player != null)
				{
					textData = player.Name;
				}
				mGuiView.Inform(GetType(), 1, textData);
			}
			CloseSession(_skipGui: false);
			mSender.UpdateBag();
		}

		private void OnTradeReady()
		{
			AddToState(State.READY_SELF);
		}

		private void OnTradeReadyOpponent(TradeReadyArg _arg)
		{
			if (mCurSession != null)
			{
				mCurSession.mOpponentMoney = _arg.mMoney;
				mCurSession.mOpponentItems = new Dictionary<int, int>(_arg.mItems);
			}
			AddToState(State.READY_OPPONENT);
		}

		private void OnTradeNotReady()
		{
			RemoveFromState(State.READY_SELF);
		}

		private void OnTradeNotReadyOpponent()
		{
			if (mCurSession != null)
			{
				mCurSession.mOpponentMoney = 0;
				mCurSession.mOpponentItems.Clear();
			}
			RemoveFromState(State.READY_OPPONENT);
		}

		private void OnTradeConfirm()
		{
			AddToState(State.CONFIRM_SELF);
			mSender.UpdateBag();
			if (CheckState(State.READY_OPPONENT))
			{
				CloseSession(_skipGui: true);
			}
		}

		private void OnTradeConfirmOpponent()
		{
			mSender.UpdateBag();
			if (mCurSession != null)
			{
				AddToState(State.CONFIRM_OPPONENT);
			}
		}

		private void AddToState(State _s)
		{
			if (mCurSession == null)
			{
				Log.Error("trade session not opened");
				return;
			}
			mCurSession.mState |= (int)_s;
			mOnStateChangedCallback(this);
		}

		private void RemoveFromState(State _s)
		{
			if (mCurSession == null)
			{
				Log.Error("trade session not opened");
				return;
			}
			mCurSession.mState &= (int)(~_s);
			mOnStateChangedCallback(this);
		}

		public int GetCurState()
		{
			if (mCurSession == null)
			{
				return 0;
			}
			return mCurSession.mState;
		}

		public bool CheckState(State _s)
		{
			return ((uint)GetCurState() & (uint)_s) != 0;
		}

		public int GetOpponentMoney()
		{
			if (mCurSession == null)
			{
				return 0;
			}
			return mCurSession.mOpponentMoney;
		}

		public IDictionary<int, int> GetOpponentItems()
		{
			if (mCurSession == null)
			{
				return new Dictionary<int, int>();
			}
			return mCurSession.mOpponentItems;
		}

		public int GetSelfMoney()
		{
			if (mCurSession == null)
			{
				return 0;
			}
			return mCurSession.mSelfMoney;
		}

		public IDictionary<int, int> GetSelfItems()
		{
			if (mCurSession == null)
			{
				return new Dictionary<int, int>();
			}
			return mCurSession.mSelfItems;
		}

		public void SetSelfMoney(int _money)
		{
			if (mCurSession != null)
			{
				mCurSession.mSelfMoney = _money;
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}

		public void SetSelfItems(IDictionary<int, int> _items)
		{
			if (_items == null)
			{
				throw new ArgumentNullException("_items");
			}
			if (mCurSession != null)
			{
				mCurSession.mSelfItems = new Dictionary<int, int>(_items);
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}

		public void OpenSession(int _playerId)
		{
			Player player = mPlayerProv.Get(_playerId);
			if (player == null || player.IsSelf)
			{
				return;
			}
			if (mCurSession != null)
			{
				if (!mCurSession.mCheckCrossStart)
				{
					return;
				}
			}
			else
			{
				mCurSession = new Session();
			}
			mCurSession.mOpponentId = _playerId;
			mSender.Start(_playerId);
			mOnStateChangedCallback(this);
		}

		private void CloseSession(bool _skipGui)
		{
			if (_skipGui)
			{
				mGuiView.Skip(GetType());
			}
			mCurSession = null;
			mOnStateChangedCallback(this);
		}

		public void Confirm()
		{
			if (mCurSession != null)
			{
				mSender.Confirm();
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}

		public void Cancel()
		{
			if (mCurSession != null)
			{
				mSender.Cancel();
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}

		public void Ready()
		{
			if (mCurSession != null)
			{
				mSender.Ready(mCurSession.mSelfMoney, mCurSession.mSelfItems);
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}

		public void NotReady()
		{
			if (mCurSession != null)
			{
				mSender.NotReady();
			}
			else
			{
				Log.Warning("trade session not opened");
			}
		}
	}
}
