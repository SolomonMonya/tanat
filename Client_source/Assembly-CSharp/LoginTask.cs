using System;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class LoginTask : MonoBehaviour
{
	private LoginPerformer mPerformer;

	private TanatApp mApp;

	public void Init(Core _core, TanatApp _app, Notifier<LoginPerformer, object> _notifier, IReconnectChecker _reconnectChecker)
	{
		if (_core == null)
		{
			throw new ArgumentNullException("_core");
		}
		if (_app == null)
		{
			throw new ArgumentNullException("_app");
		}
		mApp = _app;
		mPerformer = new LoginPerformer(_core);
		mPerformer.EnableAskForReconnect(_reconnectChecker);
		Notifier<LoginPerformer, object> notifier = new Notifier<LoginPerformer, object>(OnLoginPerformed, _core);
		mPerformer.Notifiers.Add(notifier);
		mPerformer.Notifiers.Add(_notifier);
		mPerformer.Start();
	}

	public void Update()
	{
		if (mPerformer != null)
		{
			mPerformer.UpdateInProgressCore();
		}
	}

	private void OnLoginPerformed(bool _success, LoginPerformer _loginPerformer, object _data)
	{
		mPerformer = null;
		if (_success)
		{
			mApp.SetCore(_loginPerformer.InProgressCore);
		}
		else
		{
			Log.Warning("cannot login");
		}
		UnityEngine.Object.Destroy(this);
	}
}
