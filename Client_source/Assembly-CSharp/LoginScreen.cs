using System;
using System.Collections.Generic;
using System.Timers;
using TanatKernel;
using UnityEngine;

public class LoginScreen : BaseLoginScreen, IReconnectChecker
{
	private class DeferredPopup
	{
		public string mTextId;

		public IDictionary<string, string> mReplace;
	}

	private ScreenManager mScreenMgr;

	private SceneManager mSceneMgr;

	private IDebugJoinInitiator mDebugJoinInitiator;

	private LoginWindow mLoginWnd;

	private OptionsMenu mOptionsWnd;

	private OkDialog mOkDialog;

	private ReconnectDialog mReconnectDialog;

	private DebugJoinWindow mDebugJoinWnd;

	private bool mIsUpdating;

	private Notifier<IReconnectChecker, object> mReconnectAnsNotifier;

	private DeferredPopup mDeferredPopup;

	public bool IsUpdating
	{
		get
		{
			return mIsUpdating;
		}
		set
		{
			mIsUpdating = value;
		}
	}

	public LoginScreen(ILoginInitiator _loginInitiator, ScreenManager _screenMgr, SceneManager _sceneMgr)
		: base(_screenMgr.Holder, _loginInitiator)
	{
		mScreenMgr = _screenMgr;
		mSceneMgr = _sceneMgr;
		GuiSystem.GuiSet guiSet = mScreenMgr.Gui.GetGuiSet("login_menu");
		mLoginWnd = guiSet.GetElementById<LoginWindow>("LOGIN_WINDOW");
		mOptionsWnd = guiSet.GetElementById<OptionsMenu>("OPTIONS_MENU");
		mOkDialog = guiSet.GetElementById<OkDialog>("OK_DAILOG");
		mReconnectDialog = guiSet.GetElementById<ReconnectDialog>("RECONNECT_DAILOG");
		GuiSystem.GuiSet guiSet2 = mScreenMgr.Gui.GetGuiSet("debug_menu");
		mDebugJoinWnd = guiSet2.GetElementById<DebugJoinWindow>("DEBUG_JOIN_WINDOW");
	}

	public void EnableDebugJoin(IDebugJoinInitiator _debugJoinInitiator)
	{
		mDebugJoinInitiator = _debugJoinInitiator;
	}

	public override void Show()
	{
		base.Show();
		mScreenMgr.Gui.SetCurGuiSet("loading_screen");
		LoginWindow loginWindow = mLoginWnd;
		loginWindow.mLoginCallback = (LoginWindow.LoginCallback)Delegate.Combine(loginWindow.mLoginCallback, new LoginWindow.LoginCallback(base.OnLoginInfo));
		LoginWindow loginWindow2 = mLoginWnd;
		loginWindow2.mShowDebugJoinCallback = (LoginWindow.ShowDebugJoinCallback)Delegate.Combine(loginWindow2.mShowDebugJoinCallback, new LoginWindow.ShowDebugJoinCallback(OnShowDebugJoin));
		LoginWindow loginWindow3 = mLoginWnd;
		loginWindow3.mShowOptionsCallback = (LoginWindow.ShowOptionsCallback)Delegate.Combine(loginWindow3.mShowOptionsCallback, new LoginWindow.ShowOptionsCallback(OnShowOptions));
		LoginWindow loginWindow4 = mLoginWnd;
		loginWindow4.mQuitCallback = (LoginWindow.QuitCallback)Delegate.Combine(loginWindow4.mQuitCallback, new LoginWindow.QuitCallback(OnQuit));
		OptionsMenu optionsMenu = mOptionsWnd;
		optionsMenu.mOnClose = (OptionsMenu.OnClose)Delegate.Combine(optionsMenu.mOnClose, new OptionsMenu.OnClose(CloseOptionsWnd));
		DebugJoinWindow debugJoinWindow = mDebugJoinWnd;
		debugJoinWindow.mBackCallback = (DebugJoinWindow.BackCallback)Delegate.Combine(debugJoinWindow.mBackCallback, new DebugJoinWindow.BackCallback(OnBackFromDebugJoin));
		DebugJoinWindow debugJoinWindow2 = mDebugJoinWnd;
		debugJoinWindow2.mJoinCallback = (DebugJoinWindow.JoinCallback)Delegate.Combine(debugJoinWindow2.mJoinCallback, new DebugJoinWindow.JoinCallback(OnDebugJoin));
		ReconnectDialog reconnectDialog = mReconnectDialog;
		reconnectDialog.mOnAnswer = (ReconnectDialog.OnAnswer)Delegate.Combine(reconnectDialog.mOnAnswer, new ReconnectDialog.OnAnswer(OnReconnectAnswer));
		mSceneMgr.LoadScene("main_menu", new Notifier<IMapManager, object>(OnSceneLoaded, null));
		if (mIsUpdating)
		{
			mLoginWnd.DisableLogin();
		}
	}

	public override void Hide()
	{
		base.Hide();
		mScreenMgr.Gui.SetCurGuiSet("loading_screen");
		LoginWindow loginWindow = mLoginWnd;
		loginWindow.mLoginCallback = (LoginWindow.LoginCallback)Delegate.Remove(loginWindow.mLoginCallback, new LoginWindow.LoginCallback(base.OnLoginInfo));
		LoginWindow loginWindow2 = mLoginWnd;
		loginWindow2.mShowDebugJoinCallback = (LoginWindow.ShowDebugJoinCallback)Delegate.Remove(loginWindow2.mShowDebugJoinCallback, new LoginWindow.ShowDebugJoinCallback(OnShowDebugJoin));
		LoginWindow loginWindow3 = mLoginWnd;
		loginWindow3.mShowOptionsCallback = (LoginWindow.ShowOptionsCallback)Delegate.Remove(loginWindow3.mShowOptionsCallback, new LoginWindow.ShowOptionsCallback(OnShowOptions));
		LoginWindow loginWindow4 = mLoginWnd;
		loginWindow4.mQuitCallback = (LoginWindow.QuitCallback)Delegate.Remove(loginWindow4.mQuitCallback, new LoginWindow.QuitCallback(OnQuit));
		mLoginWnd.Uninit();
		OptionsMenu optionsMenu = mOptionsWnd;
		optionsMenu.mOnClose = (OptionsMenu.OnClose)Delegate.Remove(optionsMenu.mOnClose, new OptionsMenu.OnClose(CloseOptionsWnd));
		ReconnectDialog reconnectDialog = mReconnectDialog;
		reconnectDialog.mOnAnswer = (ReconnectDialog.OnAnswer)Delegate.Remove(reconnectDialog.mOnAnswer, new ReconnectDialog.OnAnswer(OnReconnectAnswer));
		DebugJoinWindow debugJoinWindow = mDebugJoinWnd;
		debugJoinWindow.mBackCallback = (DebugJoinWindow.BackCallback)Delegate.Remove(debugJoinWindow.mBackCallback, new DebugJoinWindow.BackCallback(OnBackFromDebugJoin));
		DebugJoinWindow debugJoinWindow2 = mDebugJoinWnd;
		debugJoinWindow2.mJoinCallback = (DebugJoinWindow.JoinCallback)Delegate.Remove(debugJoinWindow2.mJoinCallback, new DebugJoinWindow.JoinCallback(OnDebugJoin));
		mDebugJoinWnd.Uninit();
	}

	protected override void Lock()
	{
		base.Lock();
		mLoginWnd.Lock();
	}

	protected override void Unlock()
	{
		base.Unlock();
		mLoginWnd.Unlock();
	}

	private void OnSceneLoaded(bool _success, IMapManager _mapMgr, object _data)
	{
		if (_success)
		{
			mScreenMgr.Gui.SetCurGuiSet("login_menu");
			if (mDeferredPopup != null)
			{
				ShowPopup(mDeferredPopup.mTextId, mDeferredPopup.mReplace);
				mDeferredPopup = null;
			}
		}
	}

	private void OnShowDebugJoin()
	{
		mScreenMgr.Gui.SetCurGuiSet("debug_menu");
	}

	private void OnShowOptions()
	{
		mLoginWnd.SetActive(_active: false);
		mOptionsWnd.Show();
	}

	private void CloseOptionsWnd()
	{
		
		mLoginWnd.SetActive(_active: true);
		OnShowDebugJoin(); // FOR TEST
	}

	private void OnQuit()
	{
		Application.Quit();
	}

	private void OnBackFromDebugJoin()
	{
		mScreenMgr.Gui.SetCurGuiSet("login_menu");
	}

	private void OnDebugJoin(BattleServerData _battleSrvData, BattleMapData _battleMapData)
	{
		if (mDebugJoinInitiator != null)
		{
			mDebugJoinInitiator.StartDebugJoin(_battleSrvData, _battleMapData);
			mScreenMgr.Gui.SetCurGuiSet("loading_screen");
		}
	}

	private void OnOkPressed()
	{
		Unlock();
	}

	public void AskForReconnect(float _time, Notifier<IReconnectChecker, object> _notifier)
	{
		mReconnectAnsNotifier = _notifier;
		mReconnectDialog.SetData(_time);
		if (_time > 0f)
		{
			Timer timer = new Timer();
			timer.AutoReset = false;
			timer.Interval = _time * 1000f;
			timer.Enabled = true;
			timer.Elapsed += OnStopReconnectTime;
		}
	}

	private void OnStopReconnectTime(object sender, ElapsedEventArgs e)
	{
		OnReconnectAnswer(_result: false);
	}

	private void OnReconnectAnswer(bool _result)
	{
		if (mReconnectAnsNotifier != null)
		{
			mReconnectAnsNotifier.Call(_result, this);
			mReconnectAnsNotifier = null;
		}
	}

	public void ShowPopup(string _txtId)
	{
		ShowPopup(_txtId, null);
	}

	public void ShowPopup(string _txtId, IDictionary<string, string> _replaceData)
	{
		Lock();
		string text = GuiSystem.GetLocaleText(_txtId);
		if (_replaceData != null)
		{
			foreach (KeyValuePair<string, string> _replaceDatum in _replaceData)
			{
				text = text.Replace(_replaceDatum.Key, _replaceDatum.Value);
			}
		}
		mOkDialog.SetData(text, OnOkPressed);
	}

	public void ShowDeferredPopup(string _txtId, IDictionary<string, string> _replaceData)
	{
		mDeferredPopup = new DeferredPopup();
		mDeferredPopup.mTextId = _txtId;
		mDeferredPopup.mReplace = _replaceData;
	}

	protected override void ShowLoginFailed(int _failReason, CtrlEntryPoint _entryPoint)
	{
		base.ShowLoginFailed(_failReason, _entryPoint);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["{HOST}"] = _entryPoint.Host;
		dictionary["{PORT}"] = _entryPoint.Port;
		switch (_failReason)
		{
		case 6014:
			ShowPopup("GUI_CANNOT_LOGIN", dictionary);
			break;
		case 6011:
			ShowPopup("GUI_BANNED", dictionary);
			break;
		case 6012:
			ShowPopup("GUI_KICKED", dictionary);
			break;
		case 2:
			ShowPopup("GUI_SERVER_NOT_AVAILABLE", dictionary);
			break;
		case 6015:
			ShowPopup("GUI_ILLEGAL_VERSION", dictionary);
			break;
		}
	}
}
