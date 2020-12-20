using System;
using TanatKernel;

public class CustomizeHeroScreen : BaseCustomizeHeroScreen
{
	private ScreenManager mScreenMgr;

	private CtrlServerConnection mCtrlSrv;

	private HeroMgr mHeroMgr;

	private TanatApp mApp;

	private MainMenu mMainMenu;

	private CustomizeMenu mCustomizeWnd;

	private SelectRaceWindow mSelectRaceWnd;

	public CustomizeHeroScreen(ScreenManager _screenMgr, CtrlServerConnection _ctrlSrv, HeroMgr _heroMgr, TanatApp _app)
	{
		mScreenMgr = _screenMgr;
		mCtrlSrv = _ctrlSrv;
		mHeroMgr = _heroMgr;
		mApp = _app;
		mSelectRaceWnd = mScreenMgr.Gui.GetGuiElement<SelectRaceWindow>("select_race", "SELECT_RACE_WINDOW");
		mCustomizeWnd = mScreenMgr.Gui.GetGuiElement<CustomizeMenu>("customize_menu", "CUSTOMIZE_MENU");
	}

	public override void Show()
	{
		base.Show();
		mMainMenu = GameObjUtil.FindObjectOfType<MainMenu>();
		mCustomizeWnd.SetData(mHeroMgr, mCtrlSrv.EntryPoint.UserData.UserName);
		SelectRaceWindow selectRaceWindow = mSelectRaceWnd;
		selectRaceWindow.mRaceSelected = (SelectRaceWindow.RaceSelected)Delegate.Combine(selectRaceWindow.mRaceSelected, new SelectRaceWindow.RaceSelected(ShowCustomizeHero));
		CustomizeMenu customizeMenu = mCustomizeWnd;
		customizeMenu.mBackCallback = (CustomizeMenu.BackCallback)Delegate.Combine(customizeMenu.mBackCallback, new CustomizeMenu.BackCallback(ShowSelectRace));
		CustomizeMenu customizeMenu2 = mCustomizeWnd;
		customizeMenu2.mPlayCallback = (CustomizeMenu.PlayCallback)Delegate.Combine(customizeMenu2.mPlayCallback, new CustomizeMenu.PlayCallback(CreateHero));
		CustomizeMenu customizeMenu3 = mCustomizeWnd;
		customizeMenu3.mMenuCallback = (CustomizeMenu.MenuCallback)Delegate.Combine(customizeMenu3.mMenuCallback, new CustomizeMenu.MenuCallback(Logout));
		MainMenu mainMenu = mMainMenu;
		mainMenu.mRaceSelectedCallback = (Action<HeroRace>)Delegate.Combine(mainMenu.mRaceSelectedCallback, new Action<HeroRace>(ShowCustomizeHero));
		ShowSelectRace();
	}

	public override void Hide()
	{
		base.Hide();
		mCustomizeWnd.Clear();
		SelectRaceWindow selectRaceWindow = mSelectRaceWnd;
		selectRaceWindow.mRaceSelected = (SelectRaceWindow.RaceSelected)Delegate.Remove(selectRaceWindow.mRaceSelected, new SelectRaceWindow.RaceSelected(ShowCustomizeHero));
		CustomizeMenu customizeMenu = mCustomizeWnd;
		customizeMenu.mBackCallback = (CustomizeMenu.BackCallback)Delegate.Remove(customizeMenu.mBackCallback, new CustomizeMenu.BackCallback(ShowSelectRace));
		CustomizeMenu customizeMenu2 = mCustomizeWnd;
		customizeMenu2.mPlayCallback = (CustomizeMenu.PlayCallback)Delegate.Remove(customizeMenu2.mPlayCallback, new CustomizeMenu.PlayCallback(CreateHero));
		CustomizeMenu customizeMenu3 = mCustomizeWnd;
		customizeMenu3.mMenuCallback = (CustomizeMenu.MenuCallback)Delegate.Remove(customizeMenu3.mMenuCallback, new CustomizeMenu.MenuCallback(ShowSelectRace));
		CustomizeMenu customizeMenu4 = mCustomizeWnd;
		customizeMenu4.mMenuCallback = (CustomizeMenu.MenuCallback)Delegate.Remove(customizeMenu4.mMenuCallback, new CustomizeMenu.MenuCallback(Logout));
		MainMenu mainMenu = mMainMenu;
		mainMenu.mRaceSelectedCallback = (Action<HeroRace>)Delegate.Remove(mainMenu.mRaceSelectedCallback, new Action<HeroRace>(ShowCustomizeHero));
	}

	private void ShowSelectRace()
	{
		mMainMenu.SetMenuState(MainMenu.MainMenuState.SELECT_RACE);
		mScreenMgr.Gui.SetCurGuiSet("select_race");
	}

	private void ShowCustomizeHero(HeroRace _race)
	{
		mScreenMgr.Gui.SetCurGuiSet("customize_menu");
		mCustomizeWnd.SetHero(_race, mCtrlSrv.SelfHero.Hero.View);
		switch (_race)
		{
		case HeroRace.HUMAN:
			mMainMenu.SetMenuState(MainMenu.MainMenuState.CUSTOMIZE_HUMAN);
			break;
		case HeroRace.ELF:
			mMainMenu.SetMenuState(MainMenu.MainMenuState.CUSTOMIZE_ELF);
			break;
		}
	}

	private void CreateHero(HeroView _view)
	{
		mCtrlSrv.HeroSender.Create(_view.mRace, _view.mGender, _view.mFace, _view.mHair, _view.mDistMark, _view.mSkinColor, _view.mHairColor);
		mScreenMgr.Gui.SetCurGuiSet("loading_screen");
	}

	private void Logout()
	{
		mApp.Exit();
	}
}
