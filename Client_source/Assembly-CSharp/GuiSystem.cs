using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class GuiSystem : MonoBehaviour
{
	public class GuiSet
	{
		public Dictionary<string, GuiElement> mGuiIdElements;

		public List<GuiElement> mGuiElements;

		private bool mInited;

		public bool Inited => mInited;

		public GuiSet()
		{
			mGuiIdElements = new Dictionary<string, GuiElement>();
			mGuiElements = new List<GuiElement>();
		}

		public void InitElements()
		{
			if (mInited)
			{
				return;
			}
			foreach (GuiElement mGuiElement in mGuiElements)
			{
				mGuiElement.Init();
				mGuiElement.SetSize();
			}
			mInited = true;
		}

		public void AddElement(GuiElement _element)
		{
			if (_element == null)
			{
				Log.Error("trying to add null guiElement");
				return;
			}
			if (mGuiIdElements.ContainsValue(_element) || mGuiIdElements.ContainsKey(_element.mElementId) || mGuiElements.Contains(_element))
			{
				Log.Error("guiElement with elementId : " + _element.mElementId + " already exist");
				return;
			}
			mGuiElements.Add(_element);
			mGuiIdElements.Add(_element.mElementId, _element);
		}

		public GuiElement GetElementById(string _elementId)
		{
			if (_elementId == string.Empty || _elementId == null)
			{
				Log.Error("Wrong elementId");
				return null;
			}
			if (mGuiIdElements.ContainsKey(_elementId))
			{
				return mGuiIdElements[_elementId];
			}
			Log.Warning("guiElement with elementId : " + _elementId + " doesn't exist.");
			return null;
		}

		public T GetElementById<T>(string _elementId) where T : GuiElement
		{
			return GetElementById(_elementId) as T;
		}
	}

	private Dictionary<string, GuiSet> mGuiSets = new Dictionary<string, GuiSet>();

	public List<GuiElement> mLowLevelElements = new List<GuiElement>();

	public GuiSet mCurGuiSet;

	public GUISkin mCurSkin;

	public static LocaleState mLocaleState;

	public static float mBaseScreenWidth = 1280f;

	public static float mBaseScreenHeight = 960f;

	public static float mXRate;

	public static float mYRate;

	public static float mNativeYRate;

	public static float mAspect;

	public static GuiSystem mGuiSystem;

	public static GuiInputMgr mGuiInputMgr;

	public static bool mRenderLowLevelElements;

	private static bool mUpdateFocus;

	private static string mFocusedControl = string.Empty;

	private string mNewSetId = string.Empty;

	private string mCurSetId = string.Empty;

	private TaskQueue mTaskQueue;

	private static string mLastStyleId = string.Empty;

	private static GUIStyle mLastStyle;

	public static string GetLocaleText(string _id)
	{
		return mLocaleState.GetText(_id);
	}

	public void Init()
	{
		Uninit();
		OptionsMgr.LoadOptions();
		if (Application.isEditor)
		{
			mXRate = (OptionsMgr.mScreenWidth = Screen.width);
			mYRate = (OptionsMgr.mScreenHeight = Screen.height);
		}
		CalcGuiRate();
		InitGuiSets();
		mGuiSystem = this;
	}

	public void SetData(TaskQueue _taskQueue)
	{
		mTaskQueue = _taskQueue;
	}

	public void Uninit()
	{
		mCurGuiSet = null;
		mGuiSets = new Dictionary<string, GuiSet>();
		mGuiInputMgr = new GuiInputMgr();
		mGuiSystem = null;
		mCurSetId = string.Empty;
		mXRate = 0f;
		mYRate = 0f;
		mAspect = 0f;
		mNativeYRate = 0f;
		mLowLevelElements.Clear();
		mGuiSets.Clear();
	}

	public void ReinitGui()
	{
		CalcGuiRate();
		Log.Debug("ReinitGui : " + mXRate + " " + mYRate);
		foreach (KeyValuePair<string, GuiSet> mGuiSet in mGuiSets)
		{
			foreach (GuiElement mGuiElement in mGuiSet.Value.mGuiElements)
			{
				if (mGuiSet.Value.Inited)
				{
					mGuiElement.SetSize();
				}
			}
		}
	}

	private void CalcGuiRate()
	{
		mAspect = (float)OptionsMgr.mScreenWidth / (float)OptionsMgr.mScreenHeight;
		mXRate = (float)OptionsMgr.mScreenWidth / mBaseScreenWidth;
		mNativeYRate = (float)OptionsMgr.mScreenHeight / mBaseScreenHeight;
		if (mAspect > 1.33f)
		{
			mYRate = mNativeYRate;
			return;
		}
		float num = Mathf.Round(mBaseScreenWidth / mAspect);
		mYRate = (float)OptionsMgr.mScreenHeight / num;
	}

	public void UpdateGUI()
	{
		OptionsMgr.UpdateScreen();
		mGuiInputMgr.Update();
		if (mNewSetId != mCurSetId)
		{
			UpdateGuiSet();
		}
		if (mCurGuiSet == null)
		{
			return;
		}
		foreach (GuiElement mLowLevelElement in mLowLevelElements)
		{
			if (mLowLevelElement.Active)
			{
				if (mLowLevelElement.mElementId == "AVATAR" && (OptionsMgr.mShowAvatarHealthBar || mRenderLowLevelElements))
				{
					mLowLevelElement.Update();
				}
				else if (mLowLevelElement.mElementId == "NOT_AVATAR" && (OptionsMgr.mShowOtherHealthBar || mRenderLowLevelElements))
				{
					mLowLevelElement.Update();
				}
				else if (mLowLevelElement.mElementId == string.Empty)
				{
					mLowLevelElement.Update();
				}
			}
		}
		foreach (GuiElement mGuiElement in mCurGuiSet.mGuiElements)
		{
			if (mGuiElement.Active)
			{
				mGuiElement.Update();
			}
		}
	}

	public void OnGui()
	{
		if (mCurGuiSet == null)
		{
			return;
		}
		if (GUI.skin != mCurSkin)
		{
			GUI.skin = mCurSkin;
		}
		GUI.SetNextControlName(string.Empty);
		GUI.Box(default(Rect), string.Empty);
		mFocusedControl = GUI.GetNameOfFocusedControl();
		if (mUpdateFocus && mFocusedControl != string.Empty)
		{
			GUI.FocusControl(string.Empty);
		}
		if (mUpdateFocus && mFocusedControl == string.Empty)
		{
			mUpdateFocus = false;
		}
		bool flag = Event.current.type <= EventType.ScrollWheel || Event.current.type == EventType.Layout;
		bool flag2 = Event.current.type == EventType.Repaint;
		mRenderLowLevelElements = Event.current.alt;
		if (flag)
		{
			mGuiInputMgr.CheckEvent(Event.current);
		}
		foreach (GuiElement mLowLevelElement in mLowLevelElements)
		{
			if (mLowLevelElement.Active && flag2)
			{
				if (mLowLevelElement.mElementId == "AVATAR" && (OptionsMgr.mShowAvatarHealthBar || mRenderLowLevelElements))
				{
					mLowLevelElement.RenderElement();
				}
				else if (mLowLevelElement.mElementId == "NOT_AVATAR" && (OptionsMgr.mShowOtherHealthBar || mRenderLowLevelElements))
				{
					mLowLevelElement.RenderElement();
				}
				else if (mLowLevelElement.mElementId == string.Empty)
				{
					mLowLevelElement.RenderElement();
				}
			}
		}
		GuiElement guiElement = null;
		GuiElement guiElement2 = null;
		int i = 0;
		for (int count = mCurGuiSet.mGuiElements.Count; i < count; i++)
		{
			guiElement = mCurGuiSet.mGuiElements[i];
			guiElement2 = mCurGuiSet.mGuiElements[count - i - 1];
			if (guiElement2.Active && flag)
			{
				guiElement2.CheckEvent(Event.current);
			}
			if (guiElement.Active && flag2)
			{
				guiElement.RenderElement();
			}
			if (guiElement2.Active)
			{
				guiElement2.OnInput();
			}
		}
		if (flag2)
		{
			mGuiInputMgr.RenderElement();
		}
	}

	public static bool InputIsFree()
	{
		return mFocusedControl == string.Empty;
	}

	public static void UpdateFocus()
	{
		mUpdateFocus = true;
	}

	public void SetCurGuiSet(string _setId)
	{
		if (!(mNewSetId == _setId))
		{
			Log.Debug("SetCurGuiSet : " + _setId);
			if (!mGuiSets.ContainsKey(_setId))
			{
				Log.Error("There is no gui set with id : " + _setId);
				return;
			}
			mNewSetId = _setId;
			mGuiInputMgr.Uninit();
			mGuiSets[_setId].InitElements();
		}
	}

	private void UpdateGuiSet()
	{
		mCurSetId = mNewSetId;
		mCurGuiSet = mGuiSets[mCurSetId];
		UpdateFocus();
		foreach (GuiElement mLowLevelElement in mLowLevelElements)
		{
			mLowLevelElement.SetActive(_active: false);
		}
		mLowLevelElements.Clear();
		if (mCurSetId == "loading_screen")
		{
			LoadingScreen loadingScreen = mCurGuiSet.GetElementById("LOADING_SCREEN") as LoadingScreen;
			loadingScreen.SetData(mTaskQueue);
		}
		mGuiInputMgr.SetCurElements(mCurGuiSet.mGuiElements);
	}

	public string GetCurGuiSetId()
	{
		return mCurSetId;
	}

	public GuiSet GetGuiSet(string _name)
	{
		GuiSet value = null;
		mGuiSets.TryGetValue(_name, out value);
		return value;
	}

	public T GetGuiElement<T>(string _guiSetName, string _guiElementName) where T : GuiElement
	{
		GuiSet guiSet = GetGuiSet(_guiSetName);
		if (guiSet == null)
		{
			return (T)null;
		}
		return guiSet.GetElementById(_guiElementName) as T;
	}

	public static Texture2D GetImage(string _file)
	{
		Texture2D result = null;
		if (_file == null || string.Empty == _file)
		{
			return result;
		}
		result = Resources.Load(_file, typeof(Texture2D)) as Texture2D;
		if (result == null)
		{
			Log.Error("Can't load texture : " + _file + " " + Log.StackTrace());
		}
		return result;
	}

	public static void GetRectScaled(ref Rect _curRect, bool _ignoreLowRate)
	{
		float num = ((!_ignoreLowRate) ? mYRate : mNativeYRate);
		_curRect.x *= num;
		_curRect.y *= num;
		_curRect.width *= num;
		_curRect.height *= num;
	}

	public static void GetRectScaled(ref Rect _curRect)
	{
		GetRectScaled(ref _curRect, _ignoreLowRate: false);
	}

	public static void RecalculateRect(Vector2 _pos, ref Rect _rect)
	{
		_rect = GetUltimateRectScaled(_pos, _rect);
	}

	public static Rect GetUltimateRectScaled(Vector2 _pos, Rect _rect)
	{
		float num = Mathf.Floor(_rect.width * mXRate);
		float num2 = Mathf.Floor(_rect.width * mYRate);
		float num3 = num - num2;
		float num4 = _pos.x / (mBaseScreenWidth - num2);
		float f = num3 * num4;
		f = Mathf.Floor(f);
		_pos.x *= mXRate;
		_pos.y *= mYRate;
		_pos.x += f;
		GetRectScaled(ref _rect);
		_rect.x += _pos.x;
		_rect.y += _pos.y;
		return _rect;
	}

	public static void SetChildRect(Rect _baseRect, ref Rect _rect)
	{
		_rect.width *= mYRate;
		_rect.height *= mYRate;
		_rect.x = _baseRect.x + _rect.x * mYRate;
		_rect.y = _baseRect.y + _rect.y * mYRate;
	}

	public static void DrawImage(Texture _image, Rect _rect)
	{
		if (!(null == _image))
		{
			GUI.DrawTexture(_rect, _image);
		}
	}

	public static void DrawImage(Texture2D _image, Rect _rect, Color _color)
	{
		if (!(null == _image))
		{
			DrawImage(_image, _rect, 0, 0, 0, 0, _color);
		}
	}

	public static void DrawImage(Texture2D _image, Rect _rect, int _leftBorder, int _rightBorder, int _topBorder, int _bottomBorder)
	{
		if (!(null == _image))
		{
			Graphics.DrawTexture(_rect, _image, _leftBorder, _rightBorder, _topBorder, _bottomBorder);
		}
	}

	public static void DrawImage(Texture2D _image, Rect _rect, int _leftBorder, int _rightBorder, int _topBorder, int _bottomBorder, Color _color)
	{
		if (!(null == _image))
		{
			Graphics.DrawTexture(_rect, _image, new Rect(0f, 0f, 1f, 1f), _leftBorder, _rightBorder, _topBorder, _bottomBorder, _color);
		}
	}

	public static void DrawImage(Texture2D _image, Rect _drawRect, Rect _sourceRect)
	{
		if (!(null == _image))
		{
			Graphics.DrawTexture(_drawRect, _image, _sourceRect, 0, 0, 0, 0);
		}
	}

	public static void DrawString(string _string, Rect _rect)
	{
		if (!(_string == string.Empty))
		{
			GUI.Label(_rect, _string);
		}
	}

	public static void DrawString(string _string, Rect _rect, string _style)
	{
		if (!(_string == string.Empty))
		{
			if (_style != mLastStyleId)
			{
				mLastStyleId = _style;
				mLastStyle = GUI.skin.GetStyle(mLastStyleId);
			}
			if (mLastStyle != null)
			{
				GUI.Label(_rect, _string, mLastStyle);
			}
			else
			{
				GUI.Label(_rect, _string);
			}
		}
	}

	public static void DrawString(string _string, Rect _rect, GUIStyle _style)
	{
		if (_style != null)
		{
			GUI.Label(_rect, _string, _style);
		}
		else
		{
			GUI.Label(_rect, _string);
		}
	}

	public static GuiButton CreateButton(string _normImg, string _overImg, string _pressImg, string _iconImg, string _label)
	{
		GuiButton guiButton = new GuiButton();
		guiButton.mNormImg = GetImage(_normImg);
		guiButton.mOverImg = GetImage(_overImg);
		guiButton.mPressImg = GetImage(_pressImg);
		guiButton.mIconImg = GetImage(_iconImg);
		guiButton.mLabel = _label;
		return guiButton;
	}

	public static T CreateGuiObject<T>(string _elementId, bool _active) where T : new()
	{
		T val = new T();
		GuiElement guiElement = val as GuiElement;
		if (guiElement != null)
		{
			if (_elementId != string.Empty)
			{
				guiElement.mElementId = _elementId;
			}
			guiElement.SetActive(_active);
		}
		return val;
	}

	private void InitGuiSets()
	{
		GuiSet guiSet = null;
		GuiElement guiElement = null;
		guiSet = new GuiSet();
		AddGuiSet("null", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<LoadingScreen>("LOADING_SCREEN", _active: true);
		guiSet.AddElement(guiElement);
		AddGuiSet("loading_screen", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<LoginWindow>("LOGIN_WINDOW", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OptionsMenu>("OPTIONS_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ReconnectDialog>("RECONNECT_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("login_menu", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<DebugJoinWindow>("DEBUG_JOIN_WINDOW", _active: true);
		guiSet.AddElement(guiElement);
		AddGuiSet("debug_menu", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<SelectAvatarWindow>("SELECT_AVATAR_WINDOW", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<AvatarBuyMenu>("AVATAR_BUY_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PopupInfo>("POPUP_INFO", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TutorialWindow>("TUTORIAL_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<YesNoDialog>("YES_NO_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("select_avatar", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<SelectRaceWindow>("SELECT_RACE_WINDOW", _active: true);
		guiSet.AddElement(guiElement);
		AddGuiSet("select_race", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<CustomizeMenu>("CUSTOMIZE_MENU", _active: true);
		guiSet.AddElement(guiElement);
		AddGuiSet("customize_menu", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<BattleStats>("STATS_PLATE_GAME_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<MapJoinQueueMenu>("MAP_JOIN_QUEUE_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<MainInfoWindow>("MAIN_INFO_WINDOW", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ObjectInfo>("PLAYER_AVATAR_INFO", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ObjectInfo>("ENEMY_INFO", _active: false);
		guiElement.mPos = new Vector2(180f, 0f);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<MiniMap>("MINI_MAP_GAME_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ShopMenu>("SHOP_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ShopGUI>("SHOP_WINDOW", _active: false);
		ShopGUI shopGUI = guiElement as ShopGUI;
		shopGUI.SetType(ShopGUI.ShopType.BATTLE_SHOP);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ChatWindow>("CHAT_WINDOW", _active: false);
		ChatWindow chatWindow = guiElement as ChatWindow;
		chatWindow.SetChatType(ChatWindow.ChatType.BATTLE_CHAT);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestProgressMenu>("QUEST_PROGRESS_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestJournal>("QUEST_JOURNAL_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<GameEndMenu>("GAME_END_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<EscapeMenu>("ESCAPE_MENU", _active: false);
		EscapeMenu escapeMenu = guiElement as EscapeMenu;
		escapeMenu.mBattle = true;
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OptionsMenu>("OPTIONS_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TipMgr>("TIP_MGR", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ShortGameInfo>("SHORT_GAME_INFO", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestStatusMenu>("QUEST_STATUS_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<AvatarInfoWindow>("AVATAR_INFO_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<EnemyInfoWindow>("ENEMY_INFO_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<DropMenu>("DROP_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<AfkWarnDialog>("AFK_WARN_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ItemCountMenu>("ITEM_COUNT_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<FormatedTipMgr>("FORMATED_TIP", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<InventoryMenu>("INVENTORY_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PopupInfo>("POPUP_INFO", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TutorialWindow>("TUTORIAL_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<LeaveInfo>("LEAVE_INFO", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TimerWarningDialog>("TIMER_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<YesNoDialog>("YES_NO_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("battle", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<CSMainMenu>("CENTRAL_SQUARE_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CSStats>("CENTRAL_SQUARE_STATS", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ChatWindow>("CHAT_WINDOW", _active: false);
		chatWindow = guiElement as ChatWindow;
		chatWindow.SetChatType(ChatWindow.ChatType.CS_CHAT);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<MiniMap>("MINI_MAP_GAME_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<WorldMapMenu>("WORLD_MAP_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CastleMenu>("CASTLE_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CastleRequestMenu>("CASTLE_REQUEST_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CastleHistoryMenu>("CASTLE_HISTORY_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<SelectGameMenu>("SELECT_GAME_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<MapJoinQueueMenu>("MAP_JOIN_QUEUE_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<NPCMenu>("NPC_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestJournal>("QUEST_JOURNAL_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestMenu>("QUEST_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ClanMenu>("CLAN_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ClanMenuCreate>("CLAN_MENU_CREATE", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ShopGUI>("SHOP_WINDOW", _active: false);
		(guiElement as ShopGUI).SetType(ShopGUI.ShopType.CS_SHOP);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ShopGUI>("SHOP_REAL_WINDOW", _active: false);
		shopGUI = guiElement as ShopGUI;
		shopGUI.SetType(ShopGUI.ShopType.CS_REAL_SHOP);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<HeroInfo>("HERO_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CurseInfo>("CURSE_INFO", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<CompensationMenu>("COMPENSATION_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<EscapeMenu>("ESCAPE_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OptionsMenu>("OPTIONS_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<GroupWindow>("GROUP_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<QuestProgressMenu>("QUEST_PROGRESS_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PlayerTradeMenu>("TRADE_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ItemCountMenu>("ITEM_COUNT_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PopUpMenu>("POPUP_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<Bank>("BANK", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<InventoryMenu>("INVENTORY_MENU", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PopupInfo>("POPUP_INFO", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TutorialWindow>("TUTORIAL_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<YesNoDialog>("YES_NO_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<FormatedTipMgr>("FORMATED_TIP", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<RewardInfo>("REWARD_INFO", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("central_square", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<UpdateWindow>("UPDATE_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("update_screen", guiSet);
		guiSet = new GuiSet();
		guiElement = CreateGuiObject<MiniMap>("MINI_MAP_GAME_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<BattleStats>("STATS_PLATE_GAME_MENU", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<ObjectInfo>("ENEMY_INFO", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<EnemyInfoWindow>("ENEMY_INFO_WINDOW", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<TipMgr>("TIP_MGR", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<PopupInfo>("POPUP_INFO", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<FormatedTipMgr>("FORMATED_TIP", _active: true);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<OkDialog>("OK_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		guiElement = CreateGuiObject<YesNoDialog>("YES_NO_DAILOG", _active: false);
		guiSet.AddElement(guiElement);
		AddGuiSet("observer", guiSet);
	}

	private void AddGuiSet(string _setId, GuiSet _set)
	{
		if (_set == null || string.IsNullOrEmpty(_setId))
		{
			return;
		}
		if (mGuiSets.ContainsKey(_setId))
		{
			Log.Error("Try to add already existing set");
			return;
		}
		foreach (GuiElement mGuiElement in _set.mGuiElements)
		{
			mGuiElement.mGuiSetId = _setId;
		}
		mGuiSets.Add(_setId, _set);
	}
}
