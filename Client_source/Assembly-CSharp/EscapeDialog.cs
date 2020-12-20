using System;
using TanatKernel;
using UnityEngine;

public class EscapeDialog
{
	public delegate void ExitCallback();

	public ExitCallback mExitCallback;

	public EscapeMenu mEscapeWnd;

	public YesNoDialog mYesNoDialogWnd;

	public OptionsMenu mOptionsWnd;

	private LeaveInfo mLeaveWnd;

	private MapType mBattleType;

	private UserLeaveInfoArg mLeaveInfo;

	public bool Active => mEscapeWnd.Active;

	public EscapeDialog(EscapeMenu _escapeWnd, YesNoDialog _yesNoDialog, OptionsMenu _optionsWnd, LeaveInfo _leaveWnd)
	{
		if (_escapeWnd == null)
		{
			throw new ArgumentNullException("_escapeWnd");
		}
		if (_yesNoDialog == null)
		{
			throw new ArgumentNullException("_yesNoDialog");
		}
		if (_optionsWnd == null)
		{
			throw new ArgumentNullException("_optionsWnd");
		}
		mEscapeWnd = _escapeWnd;
		mYesNoDialogWnd = _yesNoDialog;
		mOptionsWnd = _optionsWnd;
		mLeaveWnd = _leaveWnd;
	}

	public void Show()
	{
		Hide();
		EscapeMenu escapeMenu = mEscapeWnd;
		escapeMenu.mExitCallback = (EscapeMenu.BtnCallback)Delegate.Combine(escapeMenu.mExitCallback, new EscapeMenu.BtnCallback(Exit));
		EscapeMenu escapeMenu2 = mEscapeWnd;
		escapeMenu2.mReturnCallback = (EscapeMenu.BtnCallback)Delegate.Combine(escapeMenu2.mReturnCallback, new EscapeMenu.BtnCallback(Hide));
		EscapeMenu escapeMenu3 = mEscapeWnd;
		escapeMenu3.mOptionsCallback = (EscapeMenu.BtnCallback)Delegate.Combine(escapeMenu3.mOptionsCallback, new EscapeMenu.BtnCallback(ShowOptions));
		EscapeMenu escapeMenu4 = mEscapeWnd;
		escapeMenu4.mMainMenuCallback = (EscapeMenu.BtnCallback)Delegate.Combine(escapeMenu4.mMainMenuCallback, new EscapeMenu.BtnCallback(ReturnToMainMenu));
		if (mLeaveWnd != null)
		{
			LeaveInfo leaveInfo = mLeaveWnd;
			leaveInfo.mOnAccept = (LeaveInfo.OnAccept)Delegate.Combine(leaveInfo.mOnAccept, new LeaveInfo.OnAccept(LeaveAccepted));
		}
		mEscapeWnd.SetActive(_active: true);
	}

	public void Hide()
	{
		EscapeMenu escapeMenu = mEscapeWnd;
		escapeMenu.mExitCallback = (EscapeMenu.BtnCallback)Delegate.Remove(escapeMenu.mExitCallback, new EscapeMenu.BtnCallback(Exit));
		EscapeMenu escapeMenu2 = mEscapeWnd;
		escapeMenu2.mReturnCallback = (EscapeMenu.BtnCallback)Delegate.Remove(escapeMenu2.mReturnCallback, new EscapeMenu.BtnCallback(Hide));
		EscapeMenu escapeMenu3 = mEscapeWnd;
		escapeMenu3.mOptionsCallback = (EscapeMenu.BtnCallback)Delegate.Remove(escapeMenu3.mOptionsCallback, new EscapeMenu.BtnCallback(ShowOptions));
		EscapeMenu escapeMenu4 = mEscapeWnd;
		escapeMenu4.mMainMenuCallback = (EscapeMenu.BtnCallback)Delegate.Remove(escapeMenu4.mMainMenuCallback, new EscapeMenu.BtnCallback(ReturnToMainMenu));
		if (mLeaveWnd != null)
		{
			LeaveInfo leaveInfo = mLeaveWnd;
			leaveInfo.mOnAccept = (LeaveInfo.OnAccept)Delegate.Remove(leaveInfo.mOnAccept, new LeaveInfo.OnAccept(LeaveAccepted));
		}
		mEscapeWnd.SetActive(_active: false);
	}

	public void SetBattleType(MapType _type)
	{
		mBattleType = _type;
	}

	public void SetLeaveInfo(UserLeaveInfoArg _leaveInfo)
	{
		mLeaveInfo = _leaveInfo;
	}

	private void Exit()
	{
		if (mBattleType != MapType.HUNT && mBattleType != MapType.CS && mLeaveWnd != null && mBattleType != MapType.CW_DOTA && mBattleType != MapType.CW_SIEGE)
		{
			mLeaveWnd.SetData(mLeaveInfo, LeaveInfo.Type.FROM_GAME);
			mLeaveWnd.Open();
			return;
		}
		YesNoDialog.OnAnswer callback = delegate(bool _answer)
		{
			if (_answer)
			{
				LeaveAccepted(LeaveInfo.Type.FROM_GAME);
			}
		};
		string localeText = GuiSystem.GetLocaleText("GUI_EXIT_QUESTION");
		mYesNoDialogWnd.SetData(localeText, "YES_TEXT", "NO_TEXT", callback);
	}

	private void ShowOptions()
	{
		mOptionsWnd.Show();
	}

	private void ReturnToMainMenu()
	{
		if (mBattleType != MapType.HUNT && mBattleType != MapType.CS && mLeaveWnd != null && mBattleType != MapType.CW_DOTA && mBattleType != MapType.CW_SIEGE)
		{
			mLeaveWnd.SetData(mLeaveInfo, LeaveInfo.Type.FROM_BATTLE);
			mLeaveWnd.Open();
			return;
		}
		YesNoDialog.OnAnswer callback = delegate(bool _answer)
		{
			if (_answer)
			{
				LeaveAccepted(LeaveInfo.Type.FROM_BATTLE);
			}
		};
		string localeText = GuiSystem.GetLocaleText("GUI_EXIT_TO_CS_QUESTION");
		if (mBattleType == MapType.HUNT)
		{
			localeText = GuiSystem.GetLocaleText("GUI_EXIT_TO_CS_QUESTION");
		}
		else if (mBattleType == MapType.CS)
		{
			localeText = GuiSystem.GetLocaleText("GUI_EXIT_QUESTION");
		}
		mYesNoDialogWnd.SetData(localeText, "YES_TEXT", "NO_TEXT", callback);
	}

	private void LeaveAccepted(LeaveInfo.Type _type)
	{
		switch (_type)
		{
		case LeaveInfo.Type.FROM_BATTLE:
			Hide();
			if (mExitCallback != null)
			{
				mExitCallback();
			}
			break;
		case LeaveInfo.Type.FROM_GAME:
			Application.Quit();
			break;
		}
	}
}
