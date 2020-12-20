using System;

namespace TanatKernel
{
	public abstract class BaseBattleScreen : IScreen
	{
		protected Core mCore;

		private bool mFirstTimeInit;

		public BaseBattleScreen(Core _core)
		{
			mCore = _core;
		}

		public virtual void Show()
		{
			Core core = mCore;
			core.mBattleStartedCallback = (Core.BattleStartedCallback)Delegate.Combine(core.mBattleStartedCallback, new Core.BattleStartedCallback(OnBattleStarted));
		}

		public virtual void Hide()
		{
			Core core = mCore;
			core.mBattleStartedCallback = (Core.BattleStartedCallback)Delegate.Remove(core.mBattleStartedCallback, new Core.BattleStartedCallback(OnBattleStarted));
			if (mCore.IsBattleCreated())
			{
				SelfPlayer selfPlayer = mCore.Battle.SelfPlayer;
				selfPlayer.mInitedCallback = (SelfPlayer.InitedCallback)Delegate.Remove(selfPlayer.mInitedCallback, new SelfPlayer.InitedCallback(OnSelfPlayerInited));
			}
		}

		private void OnBattleStarted(Battle _battle)
		{
			if (_battle != null)
			{
				mFirstTimeInit = true;
				SelfPlayer selfPlayer = _battle.SelfPlayer;
				selfPlayer.mInitedCallback = (SelfPlayer.InitedCallback)Delegate.Combine(selfPlayer.mInitedCallback, new SelfPlayer.InitedCallback(OnSelfPlayerInited));
			}
		}

		private void OnSelfPlayerInited(SelfPlayer _selfPlayer)
		{
			if (mFirstTimeInit)
			{
				ShowGui();
				mFirstTimeInit = false;
			}
			else
			{
				ReinitSelfPlayerGui();
			}
		}

		protected abstract void ShowGui();

		protected virtual void ReinitSelfPlayerGui()
		{
		}
	}
}
