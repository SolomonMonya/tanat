using System;
using System.Collections.Generic;
using System.Diagnostics;
using Log4Tanat;
using UnityEngine;

public class LoginWindow : GuiElement
{
	public delegate void LoginCallback(string _email, string _password);

	public delegate void ShowDebugJoinCallback();

	public delegate void ShowOptionsCallback();

	public delegate void QuitCallback();

	public LoginCallback mLoginCallback;

	public ShowDebugJoinCallback mShowDebugJoinCallback;

	public ShowOptionsCallback mShowOptionsCallback;

	public QuitCallback mQuitCallback;

	private GuiButton mLoginBtn;

	private List<GuiButton> mButtons = new List<GuiButton>();

	private StaticTextField mLoginTF;

	private StaticTextField mPassTF;

	private Texture2D mLogoImage;

	private string mUserMailText = string.Empty;

	private string mUserPassText = string.Empty;

	private Rect mUserMailTextRect;

	private Rect mUserMailPassRect;

	private Rect mLogoRect;

	private bool mDisableLogin;

	public override void Init()
	{
		mLoginTF = new StaticTextField();
		mLoginTF.mElementId = "CURSED_TEXT_FIELD_NAME";
		mLoginTF.mLength = 60;
		mLoginTF.mStyleId = "text_field_2";
		mPassTF = new StaticTextField();
		mPassTF.mElementId = "TEXT_FIELD_PASS";
		mPassTF.mLength = 20;
		mPassTF.mPassword = true;
		mPassTF.mStyleId = "text_field_2";
		mLoginBtn = GuiSystem.CreateButton("Gui/LoginWindow/button_1_norm", "Gui/LoginWindow/button_1_over", "Gui/LoginWindow/button_1_press", string.Empty, string.Empty);
		mLoginBtn.mElementId = "BUTTON_LOGIN";
		GuiButton guiButton = mLoginBtn;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mLoginBtn.Init();
		mLoginBtn.mLocked = mDisableLogin;
		InitButtons();
		mLoginTF.mData = "e-mail";
		if (PlayerPrefs.HasKey("user_name"))
		{
			mLoginTF.mData = PlayerPrefs.GetString("user_name");
		}
		mLogoImage = GuiSystem.GetImage("Gui/LoginWindow/logo");
		mUserMailText = GuiSystem.GetLocaleText("User_Mail_Text");
		mUserPassText = GuiSystem.GetLocaleText("User_Pass_Text");
	}

	public override void Uninit()
	{
		if (mLoginTF != null)
		{
			mLoginTF.Uninit();
		}
		if (mPassTF != null)
		{
			mPassTF.Uninit();
		}
	}

	public override void SetSize()
	{
		mLogoRect = new Rect(0f, 70f, mLogoImage.width, mLogoImage.height);
		GuiSystem.GetRectScaled(ref mLogoRect, _ignoreLowRate: true);
		mLogoRect.x = ((float)OptionsMgr.mScreenWidth - mLogoRect.width) / 2f;
		mLoginTF.mZoneRect = new Rect(0f, 635f, 180f, 38f);
		GuiSystem.GetRectScaled(ref mLoginTF.mZoneRect, _ignoreLowRate: true);
		mLoginTF.mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mLoginTF.mZoneRect.width) / 2f;
		mPassTF.mZoneRect = new Rect(0f, 694f, 180f, 38f);
		GuiSystem.GetRectScaled(ref mPassTF.mZoneRect, _ignoreLowRate: true);
		mPassTF.mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mPassTF.mZoneRect.width) / 2f;
		mLoginBtn.mZoneRect = new Rect(0f, 762f, 217f, 93f);
		GuiSystem.GetRectScaled(ref mLoginBtn.mZoneRect, _ignoreLowRate: true);
		mLoginBtn.mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mLoginBtn.mZoneRect.width) / 2f;
		mUserMailTextRect = new Rect(mLoginTF.mZoneRect.x, mLoginTF.mZoneRect.y - 21f, mLoginTF.mZoneRect.width, 20f);
		mUserMailPassRect = new Rect(mPassTF.mZoneRect.x, mPassTF.mZoneRect.y - 21f, mPassTF.mZoneRect.width, 20f);
		SetButtonsRect();
	}

	private void SetButtonsRect()
	{
		int num = 0;
		foreach (GuiButton mButton in mButtons)
		{
			mButton.mZoneRect = new Rect(0f, -90f, 162f, 54f);
			GuiSystem.GetRectScaled(ref mButton.mZoneRect);
			mButton.mZoneRect.x = (float)OptionsMgr.mScreenWidth / 2f + (-2.5f + (float)num) * 162f * GuiSystem.mYRate;
			mButton.mZoneRect.y += OptionsMgr.mScreenHeight;
			num++;
		}
	}

	public void DisableLogin()
	{
		mDisableLogin = true;
	}

	private void InitButtons()
	{
		GuiButton guiButton = null;
		mButtons.Clear();
		for (int i = 0; i < 5; i++)
		{
			guiButton = GuiSystem.CreateButton("Gui/misc/button_12_norm", "Gui/misc/button_12_over", "Gui/misc/button_12_press", string.Empty, string.Empty);
			guiButton.mElementId = "BUTTON_" + i;
			guiButton.mLabel = GuiSystem.GetLocaleText("Login_Button_" + i + "_Name");
			GuiButton guiButton2 = guiButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			mButtons.Add(guiButton);
		}
	}

	public override void OnInput()
	{
		if (GUI.GetNameOfFocusedControl() == mLoginTF.mElementId && mLoginTF.mData == "e-mail")
		{
			mLoginTF.mData = string.Empty;
		}
		mLoginTF.OnInput();
		mPassTF.OnInput();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mLogoImage, mLogoRect);
		GuiSystem.DrawString(mUserMailText, mUserMailTextRect, "login_label");
		GuiSystem.DrawString(mUserPassText, mUserMailPassRect, "login_label");
		mLoginBtn.RenderElement();
		foreach (GuiButton mButton in mButtons)
		{
			mButton.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mLoginBtn.CheckEvent(_curEvent);
		mLoginTF.CheckEvent(_curEvent);
		mPassTF.CheckEvent(_curEvent);
		foreach (GuiButton mButton in mButtons)
		{
			mButton.CheckEvent(_curEvent);
		}
		if (_curEvent.type == EventType.KeyUp && _curEvent.keyCode == KeyCode.Return && !mLoginBtn.Pressed && mLoginCallback != null && !mDisableLogin)
		{
			mLoginCallback(mLoginTF.mData, mPassTF.mData);
		}
	}

	public void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "BUTTON_LOGIN")
		{
			if (Event.current.control)
			{
				if (mShowDebugJoinCallback != null)
				{
					mShowDebugJoinCallback();
				}
			}
			else if (mLoginCallback != null)
			{
				mLoginCallback(mLoginTF.mData, mPassTF.mData);
			}
		}
		else if (_sender.mElementId == "BUTTON_0")
		{
			Process.Start("http://www.tanatonline.ru/register.php");
		}
		else if (_sender.mElementId == "BUTTON_1")
		{
			Process.Start("http://www.tanatonline.ru/restore_pass.php");
		}
		else if (_sender.mElementId == "BUTTON_2")
		{
			if (mShowOptionsCallback != null)
			{
				mShowOptionsCallback();
			}
		}
		else if (_sender.mElementId == "BUTTON_3")
		{
			Process.Start("http://www.tanatonline.ru/about_game/");
		}
		else if (_sender.mElementId == "BUTTON_4" && mQuitCallback != null)
		{
			mQuitCallback();
		}
	}

	public void Lock()
	{
		try
		{
			PlayerPrefs.SetString("user_name", mLoginTF.mData);
		}
		catch (PlayerPrefsException ex)
		{
			Log.Error("failed save parameters. exception detected " + ex.ToString());
		}
		if (mPassTF != null)
		{
			mPassTF.mData = string.Empty;
			mPassTF.mLocked = true;
		}
		if (mLoginTF != null)
		{
			mLoginTF.mLocked = true;
		}
		if (mLoginBtn != null)
		{
			mLoginBtn.Pressed = true;
		}
	}

	public void Unlock()
	{
		if (mPassTF != null)
		{
			mPassTF.mLocked = false;
		}
		if (mLoginTF != null)
		{
			mLoginTF.mLocked = false;
		}
		if (mLoginBtn != null)
		{
			mLoginBtn.Pressed = false;
		}
	}
}
