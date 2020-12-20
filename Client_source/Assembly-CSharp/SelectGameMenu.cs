using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class SelectGameMenu : GuiElement, EscapeListener
{
	private class Map : GuiElement
	{
		public GuiButton mJoinButton;

		public Rect mDescRect;

		public Rect mWinDescRect;

		public Rect mMapDescTextRect;

		public Rect mMapWinTextRect;

		private string mDesc;

		private string mWinDesc;

		public Texture2D mMapFrame;

		private Texture2D mMapImage;

		private Texture2D mSword;

		private Rect mMapImageRect;

		private MapData mMapData;

		private string mLabel;

		private string mLevel;

		private string mMapDescText;

		private string mMapWinText;

		private Rect mLabelRect;

		private Rect mLevelRect;

		private bool mSelected;

		public bool Selected
		{
			set
			{
				mSelected = value;
				SetJoinButtonOver();
			}
		}

		public override void Init()
		{
			if (mMapData != null)
			{
				if (mJoinButton == null)
				{
					mJoinButton = GuiSystem.CreateButton("Gui/SelectGameMenu/button_1_norm", "Gui/SelectGameMenu/button_1_over", "Gui/SelectGameMenu/button_1_press", string.Empty, string.Empty);
					mJoinButton.mId = mMapData.mId;
					mJoinButton.mElementId = "JOIN_MAP_BUTTON";
					GuiButton guiButton = mJoinButton;
					guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnButtonLeave));
					mJoinButton.mLabelStyle = "middle_center_18";
					mJoinButton.mNormColor = new Color(48f / 85f, 191f / 255f, 194f / 255f);
					mJoinButton.Init();
				}
				if (mMapImage == null)
				{
					mMapImage = GuiSystem.GetImage("Gui/SelectGameMenu/Map/" + mMapData.mScene + "/map");
				}
				if (mSword == null)
				{
					mSword = GuiSystem.GetImage("Gui/SelectGameMenu/sword");
				}
				mMapFrame = GuiSystem.GetImage("Gui/SelectGameMenu/frame2");
				mLabel = GuiSystem.GetLocaleText(mMapData.mName);
				if (mMapData.mAvailable)
				{
					mLevel = GuiSystem.GetLocaleText("Available_Text");
				}
				else
				{
					mLevel = GuiSystem.GetLocaleText("Available_From_Level_Text").Replace("{VALUE}", mMapData.mMinLevel.ToString());
				}
				mJoinButton.mLocked = !mMapData.mAvailable;
				mJoinButton.mLabel = ((!mMapData.mUsed) ? GuiSystem.GetLocaleText("Login_Button_Name") : GuiSystem.GetLocaleText("Logout_Button_Name"));
				mDesc = GuiSystem.GetLocaleText(mMapData.mDesc);
				mWinDesc = GuiSystem.GetLocaleText(mMapData.mWinDesc);
				mMapDescText = GuiSystem.GetLocaleText("Map_History_Text");
				mMapWinText = GuiSystem.GetLocaleText("Map_Win_Req_Text");
			}
		}

		public override void SetSize()
		{
			mJoinButton.mZoneRect = new Rect(3f, 144f, 198f, 49f);
			mLabelRect = new Rect(8f, 6f, 188f, 16f);
			mLevelRect = new Rect(9f, 126f, 186f, 16f);
			mMapImageRect = new Rect(8f, 31f, 188f, 96f);
			GuiSystem.SetChildRect(mZoneRect, ref mJoinButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
			GuiSystem.SetChildRect(mZoneRect, ref mMapImageRect);
		}

		public override void CheckEvent(Event _curEvent)
		{
			mJoinButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mMapFrame, mZoneRect);
			mJoinButton.RenderElement();
			GuiSystem.DrawString(mLabel, mLabelRect);
			if (!string.IsNullOrEmpty(mLevel))
			{
				GuiSystem.DrawString(mLevel, mLevelRect);
			}
			GuiSystem.DrawImage(mMapImage, mMapImageRect);
			if (mMapData.mUsed)
			{
				GuiSystem.DrawImage(mSword, mMapImageRect);
			}
			if (mSelected)
			{
				GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
				GuiSystem.DrawString(mMapDescText, mMapDescTextRect, "label");
				GuiSystem.DrawString(mMapWinText, mMapWinTextRect, "label");
				GuiSystem.DrawString(mDesc, mDescRect, "middle_center_11_bold");
				GuiSystem.DrawString(mWinDesc, mWinDescRect, "middle_center_11_bold");
				GUI.contentColor = Color.white;
			}
		}

		public void SetData(MapData _data)
		{
			mMapData = _data;
			mId = _data.mId;
		}

		private void OnButtonLeave(GuiElement _sender)
		{
			if (_sender.mElementId == "JOIN_MAP_BUTTON")
			{
				SetJoinButtonOver();
			}
		}

		private void SetJoinButtonOver()
		{
			if (mJoinButton != null)
			{
				mJoinButton.mCurBtnState = (mSelected ? GuiButton.GuiButtonStates.BTN_OVER : GuiButton.GuiButtonStates.BTN_NORM);
				mJoinButton.SetCurBtnImage();
			}
		}
	}

	public delegate void JoinCallback(int _mapId);

	public delegate void GetMapListCallback(int _mapType);

	public delegate void VoidCallback();

	private const int mMaxMapTypes = 4;

	private const int mMaxMaps = 4;

	public JoinCallback mJoinCallback;

	private Texture2D mFrame1;

	private List<GuiButton> mMapTypes;

	private Dictionary<MapType, List<Map>> mMaps;

	private int mMapTypesStartIndex;

	private int mMapsStartIndex;

	private GuiButton mCloseButton1;

	private GuiButton mCloseButton2;

	private GuiButton mScrollMapTypesLeft;

	private GuiButton mScrollMapTypesRight;

	private GuiButton mScrollMapsLeft;

	private GuiButton mScrollMapsRight;

	private MapType mCurMapType;

	private MapType mSelectedMapType;

	private string mMapTypeDescLabel;

	private string mMapTypeDescText;

	private Rect mMapTypeDescLabelRect;

	private Rect mMapTypeDescTextRect;

	private Rect mDescRect;

	private Rect mWinDescRect;

	private Rect mMapDescTextRect;

	private Rect mMapWinTextRect;

	private CtrlServerConnection.IMapTypeDescHolder mMapTypeDescs;

	private string mLabel1;

	private string mLabel2;

	private string mLabel3;

	private string mLabel4;

	private Rect mLabel1Rect;

	private Rect mLabel2Rect;

	private Rect mLabel3Rect;

	private Rect mLabel4Rect;

	private Map mSelectedMap;

	private List<MapData> mMapsData = new List<MapData>();

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			UserLog.AddAction(UserActionType.CLOSE_MODE_SELECTION, "Esc");
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mMapTypes = new List<GuiButton>();
		mMaps = new Dictionary<MapType, List<Map>>();
		GuiSystem.mGuiInputMgr.AddEscapeListener(400, this);
		mFrame1 = GuiSystem.GetImage("Gui/SelectGameMenu/frame1");
		mCloseButton1 = GuiSystem.CreateButton("Gui/SelectGameMenu/button_2_norm", "Gui/SelectGameMenu/button_2_over", "Gui/SelectGameMenu/button_2_press", string.Empty, string.Empty);
		mCloseButton1.mElementId = "CLOSE_BUTTON";
		mCloseButton1.mLabel = GuiSystem.GetLocaleText("Close_Button_Name");
		mCloseButton1.mLabelStyle = "middle_center_18";
		mCloseButton1.mNormColor = new Color(48f / 85f, 191f / 255f, 194f / 255f);
		GuiButton guiButton = mCloseButton1;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton1.RegisterAction(UserActionType.CLOSE_MODE_SELECTION, "LOG_CLOSE_BUTTON");
		mCloseButton1.Init();
		mCloseButton2 = GuiSystem.CreateButton("Gui/SelectGameMenu/button_3_norm", "Gui/SelectGameMenu/button_3_over", "Gui/SelectGameMenu/button_3_press", string.Empty, string.Empty);
		mCloseButton2.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton2 = mCloseButton2;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton2.RegisterAction(UserActionType.CLOSE_MODE_SELECTION, "LOG_CLOSE_BUTTON");
		mCloseButton2.Init();
		mScrollMapTypesLeft = GuiSystem.CreateButton("Gui/SelectGameMenu/button_4_norm", "Gui/SelectGameMenu/button_4_over", "Gui/SelectGameMenu/button_4_press", string.Empty, string.Empty);
		mScrollMapTypesLeft.mElementId = "SCROLL_MAP_TYPES_LEFT";
		GuiButton guiButton3 = mScrollMapTypesLeft;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollMapTypesLeft.Init();
		mScrollMapTypesRight = GuiSystem.CreateButton("Gui/SelectGameMenu/button_5_norm", "Gui/SelectGameMenu/button_5_over", "Gui/SelectGameMenu/button_5_press", string.Empty, string.Empty);
		mScrollMapTypesRight.mElementId = "SCROLL_MAP_TYPES_RIGHT";
		GuiButton guiButton4 = mScrollMapTypesRight;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollMapTypesRight.Init();
		mScrollMapsLeft = GuiSystem.CreateButton("Gui/SelectGameMenu/button_4_norm", "Gui/SelectGameMenu/button_4_over", "Gui/SelectGameMenu/button_4_press", string.Empty, string.Empty);
		mScrollMapsLeft.mElementId = "SCROLL_MAPS_LEFT";
		GuiButton guiButton5 = mScrollMapsLeft;
		guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollMapsLeft.Init();
		mScrollMapsRight = GuiSystem.CreateButton("Gui/SelectGameMenu/button_5_norm", "Gui/SelectGameMenu/button_5_over", "Gui/SelectGameMenu/button_5_press", string.Empty, string.Empty);
		mScrollMapsRight.mElementId = "SCROLL_MAPS_RIGHT";
		GuiButton guiButton6 = mScrollMapsRight;
		guiButton6.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton6.mOnMouseUp, new OnMouseUp(OnButton));
		mScrollMapsRight.Init();
		mMapTypeDescLabel = GuiSystem.GetLocaleText("Map_Type_Desc_Label_Text");
		mLabel1 = GuiSystem.GetLocaleText("SelectGame_Label1");
		mLabel2 = GuiSystem.GetLocaleText("SelectGame_Label2");
		mLabel3 = GuiSystem.GetLocaleText("SelectGame_Label3");
		mLabel4 = GuiSystem.GetLocaleText("SelectGame_Label4");
		mCurMapType = MapType.DOTA;
		InitMapTypeButtons();
		SetScrollMapTypesButtonsState();
		SetScrollMapsButtonsState();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mFrame1.width, mFrame1.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton1.mZoneRect = new Rect(320f, 776f, 198f, 49f);
		mCloseButton2.mZoneRect = new Rect(798f, 3f, 32f, 32f);
		mScrollMapTypesLeft.mZoneRect = new Rect(-9f, 108f, 26f, 85f);
		mScrollMapTypesRight.mZoneRect = new Rect(819f, 108f, 26f, 85f);
		mScrollMapsLeft.mZoneRect = new Rect(-9f, 470f, 26f, 85f);
		mScrollMapsRight.mZoneRect = new Rect(819f, 470f, 26f, 85f);
		mMapTypeDescLabelRect = new Rect(198f, 242f, 440f, 20f);
		mMapTypeDescTextRect = new Rect(155f, 263f, 526f, 65f);
		mMapDescTextRect = new Rect(198f, 621f, 440f, 20f);
		mDescRect = new Rect(155f, 642f, 526f, 55f);
		mMapWinTextRect = new Rect(155f, 696f, 526f, 20f);
		mWinDescRect = new Rect(155f, 708f, 526f, 42f);
		mLabel1Rect = new Rect(26f, 5f, 774f, 20f);
		mLabel2Rect = new Rect(20f, 355f, 796f, 20f);
		mLabel3Rect = new Rect(101f, 49f, 637f, 20f);
		mLabel4Rect = new Rect(101f, 388f, 637f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel1Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel2Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel3Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mLabel4Rect);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton1.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton2.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollMapTypesLeft.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollMapTypesRight.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollMapsLeft.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollMapsRight.mZoneRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMapTypeDescLabelRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMapTypeDescTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mDescRect);
		GuiSystem.SetChildRect(mZoneRect, ref mWinDescRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMapDescTextRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMapWinTextRect);
		foreach (GuiButton mMapType in mMapTypes)
		{
			mMapType.mZoneRect = new Rect(0f, 50f, 198f, 195f);
			GuiSystem.SetChildRect(mZoneRect, ref mMapType.mZoneRect);
		}
		foreach (KeyValuePair<MapType, List<Map>> mMap in mMaps)
		{
			foreach (Map item in mMap.Value)
			{
				item.mMapDescTextRect = mMapDescTextRect;
				item.mMapWinTextRect = mMapWinTextRect;
				item.mDescRect = mDescRect;
				item.mWinDescRect = mWinDescRect;
			}
		}
		SetMapsPos();
		SetMapTypesPos();
	}

	public override void Uninit()
	{
		mCurMapType = MapType.DOTA;
		mMaps.Clear();
		mMapTypesStartIndex = 0;
		mMapsStartIndex = 0;
		SetMapTypeButtonsState();
		SetScrollMapTypesButtonsState();
		SetScrollMapsButtonsState();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame1, mZoneRect);
		GuiSystem.DrawString(mLabel1, mLabel1Rect, "label");
		GuiSystem.DrawString(mLabel2, mLabel2Rect, "label");
		GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
		GuiSystem.DrawString(mLabel3, mLabel3Rect, "label");
		GuiSystem.DrawString(mLabel4, mLabel4Rect, "label");
		GuiSystem.DrawString(mMapTypeDescLabel, mMapTypeDescLabelRect, "label");
		GuiSystem.DrawString(mMapTypeDescText, mMapTypeDescTextRect, "middle_center_11_bold");
		GUI.contentColor = Color.white;
		mCloseButton1.RenderElement();
		mCloseButton2.RenderElement();
		if (!mScrollMapTypesLeft.mLocked || !mScrollMapTypesRight.mLocked)
		{
			mScrollMapTypesLeft.RenderElement();
			mScrollMapTypesRight.RenderElement();
		}
		if (!mScrollMapsLeft.mLocked || !mScrollMapsRight.mLocked)
		{
			mScrollMapsLeft.RenderElement();
			mScrollMapsRight.RenderElement();
		}
		int i = mMapTypesStartIndex;
		for (int num = mMapTypesStartIndex + 4; i < num; i++)
		{
			if (i < mMapTypes.Count)
			{
				mMapTypes[i].RenderElement();
			}
		}
		int j = mMapsStartIndex;
		for (int num2 = mMapsStartIndex + 4; j < num2; j++)
		{
			if (j < mMaps[mCurMapType].Count)
			{
				mMaps[mCurMapType][j].RenderElement();
			}
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton1.CheckEvent(_curEvent);
		mCloseButton2.CheckEvent(_curEvent);
		if (!mScrollMapTypesLeft.mLocked || !mScrollMapTypesRight.mLocked)
		{
			mScrollMapTypesLeft.CheckEvent(_curEvent);
			mScrollMapTypesRight.CheckEvent(_curEvent);
		}
		if (!mScrollMapsLeft.mLocked || !mScrollMapsRight.mLocked)
		{
			mScrollMapsLeft.CheckEvent(_curEvent);
			mScrollMapsRight.CheckEvent(_curEvent);
		}
		int i = mMapTypesStartIndex;
		for (int num = mMapTypesStartIndex + 4; i < num; i++)
		{
			if (i < mMapTypes.Count)
			{
				mMapTypes[i].CheckEvent(_curEvent);
			}
		}
		int j = mMapsStartIndex;
		for (int num2 = mMapsStartIndex + 4; j < num2; j++)
		{
			if (j < mMaps[mCurMapType].Count)
			{
				mMaps[mCurMapType][j].CheckEvent(_curEvent);
			}
		}
		base.CheckEvent(_curEvent);
	}

	public override void SetActive(bool _active)
	{
		if (_active)
		{
			SetMaps(mCurMapType);
		}
		base.SetActive(_active);
	}

	public bool IsInited()
	{
		return mMapsData.Count > 0;
	}

	public void InitMaps(List<MapData> _maps)
	{
		foreach (MapData _map in _maps)
		{
			mMapsData.Add(_map);
		}
		if (base.Active)
		{
			SetMaps();
		}
	}

	public void SetMaps(MapType _mapType)
	{
		mCurMapType = _mapType;
		SetMapTypeButtonsState();
		SetMapTypeDesc();
		foreach (MapData mMapsDatum in mMapsData)
		{
			if (mMapsDatum.mType == (int)_mapType)
			{
				InitMap(_mapType, mMapsDatum);
			}
		}
		if (mMaps[_mapType].Count > 0)
		{
			SetSelectedMap(mMaps[_mapType][0]);
		}
		SetMapsPos();
		SetScrollMapsButtonsState();
	}

	public void SetMaps()
	{
		SetMaps(mCurMapType);
	}

	private void InitMap(MapType _mapType, MapData _data)
	{
		Map mapById = GetMapById(_mapType, _data.mId);
		if (mapById != null)
		{
			mapById.SetData(_data);
			mapById.Init();
			return;
		}
		mapById = new Map();
		mapById.SetData(_data);
		mapById.Init();
		GuiButton mJoinButton = mapById.mJoinButton;
		mJoinButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mJoinButton.mOnMouseUp, new OnMouseUp(OnButton));
		Map map = mapById;
		map.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(map.mOnMouseEnter, new OnMouseEnter(OnMapEnter));
		if (_data.mId == 103)
		{
			AddTutorialElement("JOIN_MAP_BUTTON_103", mapById.mJoinButton);
		}
		mapById.mMapDescTextRect = mMapDescTextRect;
		mapById.mMapWinTextRect = mMapWinTextRect;
		mapById.mDescRect = mDescRect;
		mapById.mWinDescRect = mWinDescRect;
		mMaps[_mapType].Add(mapById);
	}

	private Map GetMapById(MapType _mapType, int _id)
	{
		if (!mMaps.TryGetValue(_mapType, out var value))
		{
			return null;
		}
		foreach (Map item in value)
		{
			if (item.mId == _id)
			{
				return item;
			}
		}
		return null;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON")
		{
			SetActive(_active: false);
		}
		else if (_sender.mElementId == "GAME_MODE_BUTTON")
		{
			UserLog.AddAction(UserActionType.BUTTON_MODE, _sender.mId, GuiSystem.GetLocaleText(((MapType)_sender.mId).ToString() + "_Text"));
			SetMaps((MapType)_sender.mId);
		}
		else if (_sender.mElementId == "SCROLL_MAP_TYPES_LEFT" || _sender.mElementId == "SCROLL_MAP_TYPES_RIGHT")
		{
			ScrollMapTypes(_sender.mElementId == "SCROLL_MAP_TYPES_LEFT");
		}
		else if (_sender.mElementId == "SCROLL_MAPS_LEFT" || _sender.mElementId == "SCROLL_MAPS_RIGHT")
		{
			ScrollMaps(_sender.mElementId == "SCROLL_MAPS_LEFT");
		}
		else if (_sender.mElementId == "JOIN_MAP_BUTTON" && mJoinCallback != null)
		{
			mJoinCallback(_sender.mId);
		}
	}

	private void SetSelectedMap(Map _map)
	{
		if (_map != null)
		{
			if (mSelectedMap != null)
			{
				mSelectedMap.Selected = false;
			}
			_map.Selected = true;
			mSelectedMap = _map;
		}
	}

	private void OnMapEnter(GuiElement _sender)
	{
		Map map = _sender as Map;
		if (map != null)
		{
			SetSelectedMap(map);
		}
	}

	private void OnButtonEnter(GuiElement _sender)
	{
		if (_sender.mElementId == "GAME_MODE_BUTTON")
		{
			mSelectedMapType = (MapType)_sender.mId;
		}
		SetMapTypeDesc();
	}

	private void OnButtonLeave(GuiElement _sender)
	{
		if (_sender.mElementId == "GAME_MODE_BUTTON" && _sender.mId == (int)mSelectedMapType)
		{
			mSelectedMapType = mCurMapType;
		}
		SetMapTypeDesc();
	}

	private void SetMapTypeDesc()
	{
		string mapTypeDesc = mMapTypeDescs.GetMapTypeDesc(mSelectedMapType);
		mMapTypeDescText = GuiSystem.GetLocaleText(mapTypeDesc);
	}

	private void ScrollMapTypes(bool _left)
	{
		if (_left)
		{
			mMapTypesStartIndex--;
		}
		else
		{
			mMapTypesStartIndex++;
		}
		SetMapTypesPos();
		SetScrollMapTypesButtonsState();
	}

	private void ScrollMaps(bool _left)
	{
		if (_left)
		{
			mMapsStartIndex--;
		}
		else
		{
			mMapsStartIndex++;
		}
		SetMapsPos();
		SetScrollMapsButtonsState();
	}

	private void InitMapTypeButtons()
	{
		AddMapTypeButton(MapType.DOTA);
		AddMapTypeButton(MapType.DM);
		AddMapTypeButton(MapType.HUNT);
	}

	private void AddMapTypeButton(MapType _type)
	{
		GuiButton guiButton = null;
		string empty = string.Empty;
		int num = (int)_type;
		empty = "Gui/SelectGameMenu/mod_" + num;
		guiButton = GuiSystem.CreateButton(empty + "_0", empty + "_1", empty + "_1", string.Empty, string.Empty);
		guiButton.mElementId = "GAME_MODE_BUTTON";
		guiButton.mId = (int)_type;
		GuiButton guiButton2 = guiButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		GuiButton guiButton3 = guiButton;
		guiButton3.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton3.mOnMouseEnter, new OnMouseEnter(OnButtonEnter));
		GuiButton guiButton4 = guiButton;
		guiButton4.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton4.mOnMouseLeave, new OnMouseLeave(OnButtonLeave));
		guiButton.mLockedColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
		guiButton.Init();
		if (_type == MapType.DM || _type == MapType.DOTA || _type == MapType.HUNT)
		{
			AddTutorialElement(guiButton.mElementId + "_" + guiButton.mId, guiButton);
		}
		mMapTypes.Add(guiButton);
		mMaps.Add(_type, new List<Map>());
	}

	private void SetScrollMapTypesButtonsState()
	{
		mScrollMapTypesLeft.mLocked = mMapTypesStartIndex == 0;
		mScrollMapTypesRight.mLocked = mMapTypesStartIndex + 4 >= mMapTypes.Count;
	}

	private void SetScrollMapsButtonsState()
	{
		mScrollMapsLeft.mLocked = mMapsStartIndex == 0;
		mScrollMapsRight.mLocked = mMapsStartIndex + 4 >= mMaps[mCurMapType].Count;
	}

	private void SetMapTypeButtonsState()
	{
		foreach (GuiButton mMapType in mMapTypes)
		{
			mMapType.Pressed = mMapType.mId == (int)mCurMapType;
			mMapType.SetCurBtnImage();
		}
	}

	private void SetMapTypesPos()
	{
		float num = mZoneRect.x + 31f * GuiSystem.mYRate;
		float num2 = 198f * GuiSystem.mYRate;
		GuiButton guiButton = null;
		int i = mMapTypesStartIndex;
		for (int num3 = mMapTypesStartIndex + 4; i < num3; i++)
		{
			if (i < mMapTypes.Count)
			{
				guiButton = mMapTypes[i];
				guiButton.mZoneRect.x = num + num2 * (float)(i - mMapTypesStartIndex);
			}
		}
	}

	private void SetMapsPos()
	{
		Map map = null;
		int i = mMapsStartIndex;
		for (int num = mMapsStartIndex + 4; i < num; i++)
		{
			if (i < mMaps[mCurMapType].Count)
			{
				map = mMaps[mCurMapType][i];
				map.mZoneRect = new Rect(19 + 198 * (i - mMapsStartIndex), 415f, 204f, 200f);
				GuiSystem.SetChildRect(mZoneRect, ref map.mZoneRect);
				map.SetSize();
			}
		}
	}

	public void SetData(CtrlServerConnection.IMapTypeDescHolder _mapTypeDescs)
	{
		mMapTypeDescs = _mapTypeDescs;
	}

	public void Clean()
	{
		mMapTypeDescs = null;
	}
}
