using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class EscapeMenu : GuiElement, EscapeListener
{
	private enum MenuButton
	{
		RETUN,
		OPTIONS,
		MAIN_MENU,
		EXIT
	}

	public delegate void BtnCallback();

	public BtnCallback mReturnCallback;

	public BtnCallback mOptionsCallback;

	public BtnCallback mMainMenuCallback;

	public BtnCallback mExitCallback;

	private Texture2D mFrame;

	private List<GuiButton> mButtons;

	public bool mBattle;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			if (mReturnCallback != null)
			{
				mReturnCallback();
			}
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/misc/frame_4");
		mButtons = new List<GuiButton>();
		GuiSystem.mGuiInputMgr.AddEscapeListener(60, this);
		for (int i = 0; i < 4; i++)
		{
			GuiButton guiButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			switch (i)
			{
			case 0:
				guiButton.mLabel = GuiSystem.GetLocaleText("RETUN_TO_GAME_TEXT");
				guiButton.RegisterAction(UserActionType.ESC_MENU_RETURN_TO_GAME);
				break;
			case 1:
				guiButton.mLabel = GuiSystem.GetLocaleText("OPTIONS_TEXT");
				guiButton.RegisterAction(UserActionType.ESC_MENU_OPTIONS);
				break;
			case 2:
				if (mBattle)
				{
					guiButton.mLabel = GuiSystem.GetLocaleText("EXIT_BATTLE_TEXT");
					guiButton.RegisterAction(UserActionType.ESC_MENU_CENTRAL_SQUARE);
				}
				else
				{
					guiButton.mLabel = GuiSystem.GetLocaleText("MAIN_MENU_TEXT");
					guiButton.RegisterAction(UserActionType.ESC_MENU_MAIN_MENU);
				}
				break;
			case 3:
				guiButton.RegisterAction(UserActionType.ESC_MENU_EXIT);
				guiButton.mLabel = GuiSystem.GetLocaleText("EXIT_GAME_TEXT");
				break;
			}
			guiButton.mElementId = "ESCAPE_MENU_BUTTON";
			guiButton.mId = i;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.Init();
			AddTutorialElement(guiButton.mElementId + "_" + i, guiButton);
			mButtons.Add(guiButton);
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mZoneRect.y = ((float)OptionsMgr.mScreenHeight - mZoneRect.height) / 2f;
		int num = 0;
		foreach (GuiButton mButton in mButtons)
		{
			mButton.mZoneRect = new Rect(39f, 38 + num * 35, 165f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mButton.mZoneRect);
			num++;
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		foreach (GuiButton mButton in mButtons)
		{
			mButton.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (GuiButton mButton in mButtons)
		{
			mButton.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public void OnButton(GuiElement _sender, int _buttonId)
	{
		if (!("ESCAPE_MENU_BUTTON" == _sender.mElementId) || _buttonId != 0)
		{
			return;
		}
		switch (_sender.mId)
		{
		case 0:
			if (mReturnCallback != null)
			{
				mReturnCallback();
			}
			break;
		case 1:
			if (mOptionsCallback != null)
			{
				mOptionsCallback();
			}
			break;
		case 2:
			if (mMainMenuCallback != null)
			{
				mMainMenuCallback();
			}
			break;
		case 3:
			if (mExitCallback != null)
			{
				mExitCallback();
			}
			break;
		}
	}
}
