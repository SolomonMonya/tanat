using System;

namespace TanatKernel
{
	public class BaseLoginScreen : IScreen
	{
		private ScreenHolder mHolder;

		private ILoginInitiator mLoginInitiator;

		public BaseLoginScreen(ScreenHolder _holder, ILoginInitiator _loginInitiator)
		{
			if (_holder == null)
			{
				throw new ArgumentNullException("_holder");
			}
			if (_loginInitiator == null)
			{
				throw new ArgumentNullException("_loginInitiator");
			}
			mHolder = _holder;
			mLoginInitiator = _loginInitiator;
		}

		public virtual void Show()
		{
		}

		public virtual void Hide()
		{
		}

		protected virtual void Lock()
		{
		}

		protected virtual void Unlock()
		{
		}

		protected virtual void ShowLoginFailed(int _failReason, CtrlEntryPoint _entryPoint)
		{
		}

		protected void OnLoginInfo(string _email, string _pass)
		{
			Lock();
			mLoginInitiator.StartLogin(_email, _pass, new Notifier<LoginPerformer, object>(OnLoggedIn, null));
		}

		private void OnLoggedIn(bool _success, LoginPerformer _loginPerformer, object _data)
		{
			Unlock();
			if (_success)
			{
				if (_loginPerformer.InProgressCore.CtrlServer.IsHeroExists)
				{
					mHolder.HideCurScreen();
				}
				else
				{
					mHolder.ShowScreen(ScreenType.CUSTOMIZE_HERO);
				}
			}
			else
			{
				CtrlEntryPoint entryPoint = _loginPerformer.InProgressCore.CtrlServer.EntryPoint;
				ShowLoginFailed(_loginPerformer.FailReason, entryPoint);
			}
		}
	}
}
