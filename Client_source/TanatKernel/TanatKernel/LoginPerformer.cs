using System;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class LoginPerformer
	{
		public enum LoginFailReason
		{
			SRV_NOT_AVAILABLE = 2,
			BANNED = 6011,
			KICKED = 6012,
			WRONG_SESSION = 6013,
			WRONG_PASS = 6014,
			ILLEGAL_VERSION = 6015
		}

		private class DeferredDone
		{
			public bool mSuccess;

			public DeferredDone(bool _success)
			{
				mSuccess = _success;
			}
		}

		private Core mInProgressCore;

		private IReconnectChecker mReconnectChecker;

		private bool mStarted;

		private bool mLoggedIn;

		private bool mReconnected;

		private bool mHeroReceived;

		private int mFailReason = -1;

		private Notifier<LoginPerformer, object>.Group mNotifiers = new Notifier<LoginPerformer, object>.Group();

		private DeferredDone mDeferredDone;

		public Core InProgressCore => mInProgressCore;

		public int FailReason => mFailReason;

		public bool IsReconnected => mReconnected;

		public Notifier<LoginPerformer, object>.Group Notifiers => mNotifiers;

		public LoginPerformer(Core _core)
		{
			if (_core == null)
			{
				throw new NullReferenceException("_core");
			}
			if (_core.CtrlServer.IsLoggedIn)
			{
				throw new InvalidOperationException("core already logged in");
			}
			mInProgressCore = _core;
		}

		public void EnableAskForReconnect(IReconnectChecker _checker)
		{
			mReconnectChecker = _checker;
		}

		public void Start()
		{
			if (mStarted)
			{
				throw new InvalidOperationException("login process already started");
			}
			mStarted = true;
			CtrlServerConnection ctrlServer = mInProgressCore.CtrlServer;
			HandlerManager<CtrlPacket, Enum> handlerMgr = ctrlServer.EntryPoint.HandlerMgr;
			handlerMgr.Subscribe(CtrlCmdId.user.login, OnLogin, OnLoginFailed);
			handlerMgr.Subscribe<CanReconnectArg>(CtrlCmdId.common.can_reconnect, null, OnCanReconnectFailed, OnCanReconnectData);
			handlerMgr.Subscribe<ReconnectArg>(CtrlCmdId.common.reconnect, null, OnReconnectDataFailed, OnReconnectData);
			handlerMgr.Subscribe<HeroDataArg>(CtrlCmdId.common.hero_conf, null, null, OnHeroData);
			ctrlServer.EntryPoint.SubscribeConnectionError(OnConnectionError);
			Config config = mInProgressCore.Config;
			string fn = config.SavesDir + config.SessionPath;
			if (ctrlServer.EntryPoint.TryReadSession(fn))
			{
				ctrlServer.SendCanReconnect();
			}
			else
			{
				ctrlServer.Login();
			}
		}

		public void UpdateInProgressCore()
		{
			if (mInProgressCore != null)
			{
				mInProgressCore.Update();
			}
			if ((mLoggedIn && mHeroReceived) || mReconnected)
			{
				mDeferredDone = new DeferredDone(_success: true);
			}
			if (mDeferredDone != null)
			{
				Done(mDeferredDone.mSuccess);
				mDeferredDone = null;
			}
		}

		private void Done(bool _success)
		{
			mInProgressCore.CtrlServer.EntryPoint.HandlerMgr.Unsubscribe(this);
			mInProgressCore.CtrlServer.EntryPoint.UnsubscribeConnectionError(OnConnectionError);
			mNotifiers.Call(_success, this);
			mInProgressCore = null;
		}

		private void SetFailReason(int _errorCode)
		{
			if (Enum.IsDefined(typeof(LoginFailReason), _errorCode))
			{
				mFailReason = _errorCode;
				return;
			}
			Log.Warning("Set unexisting error code : " + _errorCode);
			mFailReason = 6014;
		}

		private void OnConnectionError()
		{
			SetFailReason(2);
			mDeferredDone = new DeferredDone(_success: false);
		}

		private void OnLogin()
		{
			mLoggedIn = true;
		}

		private void OnLoginFailed(int _errorCode)
		{
			SetFailReason(_errorCode);
			mDeferredDone = new DeferredDone(_success: false);
		}

		private void OnReconnectData(ReconnectArg _arg)
		{
			mReconnected = true;
		}

		private void OnReconnectDataFailed(int _errorCode)
		{
			SetFailReason(_errorCode);
			mDeferredDone = new DeferredDone(_success: false);
		}

		private void OnHeroData(HeroDataArg _arg)
		{
			mHeroReceived = true;
		}

		private void OnCanReconnectData(CanReconnectArg _arg)
		{
			if (_arg.mAnswer)
			{
				if (mReconnectChecker == null)
				{
					mInProgressCore.CtrlServer.SendReconnect();
					return;
				}
				Notifier<IReconnectChecker, object> notifier = new Notifier<IReconnectChecker, object>();
				notifier.mCallback = OnReconnectAsked;
				mReconnectChecker.AskForReconnect(_arg.mTimer, notifier);
			}
			else
			{
				mInProgressCore.CtrlServer.EntryPoint.Session.Clean();
				mInProgressCore.CtrlServer.Login();
			}
		}

		private void OnCanReconnectFailed(int _errorCode)
		{
			if (_errorCode == 6013)
			{
				mInProgressCore.CtrlServer.EntryPoint.TryCleanSavedSession();
				mInProgressCore.CtrlServer.EntryPoint.Session.Clean();
				mInProgressCore.CtrlServer.Login();
			}
			else
			{
				SetFailReason(_errorCode);
				mDeferredDone = new DeferredDone(_success: false);
			}
		}

		private void OnReconnectAsked(bool _success, IReconnectChecker _checker, object _data)
		{
			if (_success)
			{
				mInProgressCore.CtrlServer.SendReconnect();
				return;
			}
			mInProgressCore.CtrlServer.EntryPoint.Session.Clean();
			mInProgressCore.CtrlServer.Login();
		}
	}
}
