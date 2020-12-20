using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class WorldMapMenu : GuiElement, EscapeListener
{
	public class MapElement
	{
		public int mId;

		public MapType? mMapType;

		public Location? mLocation;

		public Vector2 mPos;
	}

	private class MapElementTip
	{
		public MapElement mMapData;

		public GuiElement mElement;

		public Texture2D mFrame;

		public string mLabel;

		public Rect mLabelRect;

		public Rect mFrameRect;

		public List<KeyValuePair<string, Rect>> mData;

		public MapElementTip()
		{
			mFrame = null;
			mElement = null;
			mMapData = null;
			mLabel = string.Empty;
			mData = new List<KeyValuePair<string, Rect>>();
			mLabelRect = default(Rect);
			mFrameRect = default(Rect);
		}
	}

	private class MapJoinMenu : GuiElement
	{
		private GuiButton mJoinButton;

		private Texture2D mFrame;

		private Texture2D mMapImage;

		private string mLabel;

		private string mLevel;

		private string mJoinedText;

		private string mNeedJoinedText;

		private Rect mLabelRect;

		private Rect mLevelRect;

		private Rect mJoinedRect;

		private Rect mNeedJoinedRect;

		private Rect mMapImageRect;

		private Rect mJoinedTextRect;

		private Rect mNeedJoinedTextRect;

		private MapType mMapType;

		private MapData mMapData;

		public BattleMapCallback mJoinCallback;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/WorldMapMenu/frame5");
			if (mMapData != null)
			{
				mJoinButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
				mJoinButton.mElementId = "JOIN_BUTTON";
				GuiButton guiButton = mJoinButton;
				guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
				mJoinButton.mLabel = ((!mMapData.mUsed) ? GuiSystem.GetLocaleText("Login_Button_Name") : GuiSystem.GetLocaleText("Logout_Button_Name"));
				mJoinButton.Init();
				mMapImage = GuiSystem.GetImage("Gui/SelectGameMenu/Map/" + mMapData.mScene + "/map");
				mJoinedText = GuiSystem.GetLocaleText("Joined_Text");
				mNeedJoinedText = GuiSystem.GetLocaleText("Need_Joined_Text");
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
			}
		}

		public override void SetSize()
		{
			mZoneRect = new Rect(0f, 0f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = mPos.x;
			mZoneRect.y = mPos.y;
			mLabelRect = new Rect(5f, 3f, 188f, 16f);
			mLevelRect = new Rect(5f, 127f, 188f, 16f);
			mJoinedRect = new Rect(79f, 146f, 50f, 16f);
			mNeedJoinedRect = new Rect(79f, 164f, 50f, 16f);
			mMapImageRect = new Rect(5f, 26f, 190f, 98f);
			mJoinedTextRect = new Rect(5f, 146f, 70f, 16f);
			mNeedJoinedTextRect = new Rect(5f, 164f, 70f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
			GuiSystem.SetChildRect(mZoneRect, ref mJoinedRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNeedJoinedRect);
			GuiSystem.SetChildRect(mZoneRect, ref mMapImageRect);
			GuiSystem.SetChildRect(mZoneRect, ref mJoinedTextRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNeedJoinedTextRect);
			if (mJoinButton != null)
			{
				mJoinButton.mZoneRect = new Rect(128f, 148f, 67f, 35f);
				GuiSystem.SetChildRect(mZoneRect, ref mJoinButton.mZoneRect);
			}
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawImage(mMapImage, mMapImageRect);
			GuiSystem.DrawString(mLabel, mLabelRect);
			GuiSystem.DrawString(mLevel, mLevelRect);
			GuiSystem.DrawString(mJoinedText, mJoinedTextRect, "middle_right");
			GuiSystem.DrawString(mNeedJoinedText, mNeedJoinedTextRect, "middle_right");
			if (mJoinButton != null)
			{
				mJoinButton.RenderElement();
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mJoinButton != null)
			{
				mJoinButton.CheckEvent(_curEvent);
			}
			base.CheckEvent(_curEvent);
		}

		public void SetData(MapType _type, MapData _data)
		{
			if (_data != null)
			{
				mMapType = _type;
				mMapData = _data;
			}
		}

		public void SetPos(Vector2 _pos)
		{
			mPos = _pos;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "JOIN_BUTTON" && _buttonId == 0 && mJoinCallback != null && mMapData != null)
			{
				mJoinCallback(mMapType, mMapData.mId);
			}
		}
	}

	public delegate void ChangeLocation(int _location);

	public delegate void BattleMapCallback(MapType _type, int _mapId);

	public delegate void CastleCallback(CastleInfo _castleData);

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private string mLabel;

	private Rect mLabelRect;

	private List<MapElement> mMapElementsData;

	private Location mCurLocation;

	private YesNoDialog mYesNoDialog;

	private OkDialog mOkDialog;

	private MapElementTip mMapElementTip;

	private List<MapData> mMapDescs;

	private MapJoinMenu mMapJoinMenu;

	private MapType? mWaitMapType;

	private int mWaitMapId = -1;

	private Vector2 mWaitMapPos;

	private Dictionary<int, CastleInfo> mCastleInfo;

	public ChangeLocation mChangeLocation;

	public BattleMapCallback mJoinCallback;

	public BattleMapCallback mGetMapInfo;

	public CastleCallback mCastleCallback;

	private List<GuiButton> mMapElementButtons;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			Close();
			return true;
		}
		return false;
	}

	public override void Init()
	{
		mMapElementButtons = new List<GuiButton>();
		mCastleInfo = new Dictionary<int, CastleInfo>();
		mFrame = GuiSystem.GetImage("Gui/WorldMapMenu/frame1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(375, this);
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mLabel = GuiSystem.GetLocaleText("World_Map_Label_Text");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 100f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(818f, 10f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mLabelRect = new Rect(28f, 8f, 790f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		SetMapElementsSize();
		SetMapElementTipSize();
		if (mMapJoinMenu != null)
		{
			mMapJoinMenu.SetSize();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		foreach (GuiButton mMapElementButton in mMapElementButtons)
		{
			mMapElementButton.CheckEvent(_curEvent);
		}
		if (mMapJoinMenu != null)
		{
			mMapJoinMenu.CheckEvent(_curEvent);
		}
		if (_curEvent.type == EventType.MouseDown)
		{
			RemoveMapJoinMenu();
		}
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		foreach (GuiButton mMapElementButton in mMapElementButtons)
		{
			mMapElementButton.RenderElement();
		}
		if (mMapElementTip != null)
		{
			GuiSystem.DrawImage(mMapElementTip.mFrame, mMapElementTip.mFrameRect);
			GuiSystem.DrawString(mMapElementTip.mLabel, mMapElementTip.mLabelRect, "middle_center");
			foreach (KeyValuePair<string, Rect> mDatum in mMapElementTip.mData)
			{
				GuiSystem.DrawString(mDatum.Key, mDatum.Value, "middle_center");
			}
		}
		if (mMapJoinMenu != null)
		{
			mMapJoinMenu.RenderElement();
		}
		mCloseButton.RenderElement();
	}

	public override void Uninit()
	{
		mYesNoDialog = null;
		mOkDialog = null;
		mWaitMapType = null;
		mWaitMapId = -1;
		mWaitMapPos = Vector2.zero;
		RemoveMapJoinMenu();
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
		mWaitMapType = null;
		mWaitMapId = -1;
		mWaitMapPos = Vector2.zero;
		RemoveMapJoinMenu();
	}

	private void RemoveMapJoinMenu()
	{
		if (mMapJoinMenu != null)
		{
			MapJoinMenu mapJoinMenu = mMapJoinMenu;
			mapJoinMenu.mJoinCallback = (BattleMapCallback)Delegate.Remove(mapJoinMenu.mJoinCallback, new BattleMapCallback(OnJoin));
			mMapJoinMenu = null;
		}
	}

	public void SetData(List<MapElement> _mapElements, YesNoDialog _yesNoDialog, OkDialog _okDialog, Location _curLocation)
	{
		mCurLocation = _curLocation;
		mYesNoDialog = _yesNoDialog;
		mOkDialog = _okDialog;
		mMapElementsData = _mapElements;
		mMapElementButtons.Clear();
		foreach (MapElement mMapElementsDatum in mMapElementsData)
		{
			AddMapElement(mMapElementsDatum);
		}
		SetMapElementsSize();
	}

	public void SetMapData(MapType _type, IEnumerable<MapData> _data)
	{
		MapType? mapType = mWaitMapType;
		if (!mapType.HasValue || mWaitMapId == -1 || mWaitMapType != _type || _data == null)
		{
			return;
		}
		mMapJoinMenu = new MapJoinMenu();
		foreach (MapData _datum in _data)
		{
			if (_datum.mId == mWaitMapId)
			{
				mMapJoinMenu.SetData(_type, _datum);
			}
		}
		mMapJoinMenu.Init();
		mMapJoinMenu.SetPos(mWaitMapPos);
		mMapJoinMenu.SetSize();
		MapJoinMenu mapJoinMenu = mMapJoinMenu;
		mapJoinMenu.mJoinCallback = (BattleMapCallback)Delegate.Combine(mapJoinMenu.mJoinCallback, new BattleMapCallback(OnJoin));
		mWaitMapType = null;
		mWaitMapId = -1;
		mWaitMapPos = Vector2.zero;
	}

	public void SetMapDescription(List<MapData> _mapDescs)
	{
		mMapDescs = _mapDescs;
	}

	public void SetCastleInfo(CastleInfo _castleInfo)
	{
		if (_castleInfo != null)
		{
			if (!mCastleInfo.ContainsKey(_castleInfo.mId))
			{
				mCastleInfo.Add(_castleInfo.mId, _castleInfo);
			}
			else
			{
				mCastleInfo[_castleInfo.mId] = _castleInfo;
			}
		}
	}

	private Vector2 GetMapJoinMenuPos(Rect _zone)
	{
		Vector2 zero = Vector2.zero;
		float num = 203f * GuiSystem.mYRate;
		float num2 = 196f * GuiSystem.mYRate;
		if (_zone.x + _zone.width + num <= mZoneRect.x + mZoneRect.width)
		{
			zero.x = _zone.x + _zone.width;
		}
		else
		{
			zero.x = _zone.x - num;
		}
		if (_zone.y + _zone.height + num2 <= mZoneRect.y + mZoneRect.height)
		{
			zero.y = _zone.y + _zone.height;
		}
		else
		{
			zero.y = _zone.y - num2;
		}
		return zero;
	}

	private void SetMapElementsSize()
	{
		MapElement mapElement = null;
		foreach (GuiButton mMapElementButton in mMapElementButtons)
		{
			mapElement = GetMapElement(mMapElementButton.mElementId, mMapElementButton.mId);
			if (mapElement != null)
			{
				mMapElementButton.mZoneRect = new Rect(mapElement.mPos.x - 24f, mapElement.mPos.y - 24f, 48f, 48f);
				GuiSystem.SetChildRect(mZoneRect, ref mMapElementButton.mZoneRect);
			}
		}
	}

	private void AddMapElement(MapElement _elementData)
	{
		GuiButton guiButton = new GuiButton();
		guiButton.mId = _elementData.mId;
		Location? mLocation = _elementData.mLocation;
		if (mLocation.HasValue)
		{
			guiButton.mElementId = _elementData.mLocation.ToString();
		}
		else
		{
			MapType? mMapType = _elementData.mMapType;
			if (mMapType.HasValue)
			{
				guiButton.mElementId = _elementData.mMapType.ToString();
			}
		}
		guiButton.mNormImg = GuiSystem.GetImage("Gui/WorldMapMenu/Icons/" + guiButton.mElementId + "_norm");
		guiButton.mOverImg = GuiSystem.GetImage("Gui/WorldMapMenu/Icons/" + guiButton.mElementId + "_over");
		guiButton.mPressImg = GuiSystem.GetImage("Gui/WorldMapMenu/Icons/" + guiButton.mElementId + "_press");
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnElementMouseEnter));
		guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnElementMouseLeave));
		guiButton.Init();
		mMapElementButtons.Add(guiButton);
	}

	private MapElement GetMapElement(string _strId, int _id)
	{
		foreach (MapElement mMapElementsDatum in mMapElementsData)
		{
			Location? mLocation = mMapElementsDatum.mLocation;
			if (!mLocation.HasValue || !(_strId == mMapElementsDatum.mLocation.ToString()))
			{
				MapType? mMapType = mMapElementsDatum.mMapType;
				if (!mMapType.HasValue || !(_strId == mMapElementsDatum.mMapType.ToString()))
				{
					continue;
				}
			}
			if (mMapElementsDatum.mId == _id)
			{
				return mMapElementsDatum;
			}
		}
		return null;
	}

	private MapDataDesc GetMapDesc(int _id)
	{
		foreach (MapData mMapDesc in mMapDescs)
		{
			MapDataDesc mapDataDesc = mMapDesc;
			if (mapDataDesc.mMapId == _id)
			{
				return mapDataDesc;
			}
		}
		return null;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		CastleInfo value;
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if ((_sender.mElementId == Location.CS_HUMAN.ToString() || _sender.mElementId == Location.CS_ELF.ToString()) && _buttonId == 0)
		{
			TryChangeLocation((Location)_sender.mId);
			RemoveMapJoinMenu();
		}
		else if (_sender.mElementId == MapType.DOTA.ToString() || _sender.mElementId == MapType.DM.ToString() || _sender.mElementId == MapType.HUNT.ToString())
		{
			mWaitMapPos = GetMapJoinMenuPos(_sender.mZoneRect);
			RemoveMapJoinMenu();
			if (mGetMapInfo != null)
			{
				MapType mapType = (MapType)(int)Enum.Parse(typeof(MapType), _sender.mElementId);
				mWaitMapType = mapType;
				mWaitMapId = _sender.mId;
				mGetMapInfo(mapType, _sender.mId);
			}
		}
		else if (_sender.mElementId == Location.CASTLE.ToString() && mCastleCallback != null && mCastleInfo.TryGetValue(_sender.mId, out value))
		{
			mCastleCallback(value);
		}
	}

	public void TryChangeLocation(int _location)
	{
		TryChangeLocation((Location)_location);
	}

	public void TryChangeLocation(Location _location)
	{
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			if (_yes && mChangeLocation != null)
			{
				mChangeLocation((int)_location);
				Close();
			}
		};
		string empty = string.Empty;
		if (mCurLocation != _location)
		{
			empty = GuiSystem.GetLocaleText("Change_Location_Question");
			empty = empty.Replace("{LOCATION}", GuiSystem.GetLocaleText(_location.ToString() + "_Location"));
			mYesNoDialog.SetData(empty, "Ok_Button_Name", "Cancel_Button_Name", callback);
		}
		else
		{
			empty = GuiSystem.GetLocaleText("Already_In_Location");
			empty = empty.Replace("{LOCATION}", GuiSystem.GetLocaleText(_location.ToString() + "_Location"));
			mOkDialog.SetData(empty);
		}
	}

	private void OnElementMouseEnter(GuiElement _sender)
	{
		MapElement mapElement = GetMapElement(_sender.mElementId, _sender.mId);
		if (mapElement != null)
		{
			ShowMapElementTip(_sender, mapElement);
		}
	}

	private void OnElementMouseLeave(GuiElement _sender)
	{
		ShowMapElementTip(_sender, null);
	}

	private void ShowMapElementTip(GuiElement _element, MapElement _data)
	{
		if (_data == null)
		{
			if (mMapElementTip != null && mMapElementTip.mElement == _element)
			{
				mMapElementTip = null;
			}
			return;
		}
		mMapElementTip = new MapElementTip();
		mMapElementTip.mElement = _element;
		mMapElementTip.mMapData = _data;
		MapType? mMapType = _data.mMapType;
		if (mMapType.HasValue)
		{
			mMapElementTip.mFrame = GuiSystem.GetImage("Gui/WorldMapMenu/frame3");
			MapDataDesc mapDesc = GetMapDesc(_data.mId);
			if (mapDesc == null)
			{
				return;
			}
			string empty = string.Empty;
			mMapElementTip.mLabel = GuiSystem.GetLocaleText(mapDesc.mName);
			empty = GuiSystem.GetLocaleText("Map_Type_Text") + ":" + GuiSystem.GetLocaleText(mapDesc.mType.ToString() + "_Text");
			mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(empty, default(Rect)));
			empty = ((!mapDesc.mAvailable) ? GuiSystem.GetLocaleText("Available_From_Level_Text").Replace("{VALUE}", mapDesc.mMinLevel.ToString()) : GuiSystem.GetLocaleText("Available_Text"));
			mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(empty, default(Rect)));
		}
		else
		{
			Location? mLocation = _data.mLocation;
			if (mLocation.HasValue)
			{
				if (_data.mLocation == Location.CASTLE)
				{
					mMapElementTip.mFrame = GuiSystem.GetImage("Gui/WorldMapMenu/frame4");
					if (!mCastleInfo.TryGetValue(_data.mId, out var value))
					{
						return;
					}
					string localeText = GuiSystem.GetLocaleText(value.mName);
					mMapElementTip.mLabel = localeText;
					string localeText2 = GuiSystem.GetLocaleText("Castle_Clan_Level_Text");
					string text = localeText2;
					localeText2 = text + " " + value.mLevelMin + "-" + value.mLevelMax;
					mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(localeText2, default(Rect)));
					localeText2 = GuiSystem.GetLocaleText("Castle_Clan_Control_Text");
					mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(localeText2, default(Rect)));
					localeText2 = ((!string.IsNullOrEmpty(value.mOwnerName)) ? value.mOwnerName : GuiSystem.GetLocaleText("Castle_Clan_No_Control_Text"));
					mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(localeText2, default(Rect)));
				}
				else
				{
					mMapElementTip.mFrame = GuiSystem.GetImage("Gui/WorldMapMenu/frame2");
					string localeText3 = GuiSystem.GetLocaleText("City_Text");
					localeText3 = localeText3.Replace("{LOCATION}", GuiSystem.GetLocaleText(_data.mLocation.ToString() + "_Location"));
					mMapElementTip.mLabel = localeText3;
					Location? mLocation2 = _data.mLocation;
					string id = ((mLocation2.GetValueOrDefault() != mCurLocation || !mLocation2.HasValue) ? "Change_Location_Text" : "Already_In_Location_Text");
					id = GuiSystem.GetLocaleText(id);
					mMapElementTip.mData.Add(new KeyValuePair<string, Rect>(id, default(Rect)));
				}
			}
		}
		SetMapElementTipSize();
	}

	private void SetMapElementTipSize()
	{
		if (mMapElementTip != null && mMapElementTip.mMapData != null)
		{
			mMapElementTip.mFrameRect = new Rect(0f, 0f, mMapElementTip.mFrame.width, mMapElementTip.mFrame.height);
			GuiSystem.GetRectScaled(ref mMapElementTip.mFrameRect);
			mMapElementTip.mFrameRect.x = mMapElementTip.mElement.mZoneRect.x + (mMapElementTip.mElement.mZoneRect.width - mMapElementTip.mFrameRect.width) / 2f;
			mMapElementTip.mFrameRect.y = mMapElementTip.mElement.mZoneRect.y - mMapElementTip.mFrameRect.height;
			mMapElementTip.mLabelRect = new Rect(2f, 0f, 132f, 29f);
			GuiSystem.SetChildRect(mMapElementTip.mFrameRect, ref mMapElementTip.mLabelRect);
			int num = ((mMapElementTip.mData.Count != 1) ? 20 : 39);
			for (int i = 0; i < mMapElementTip.mData.Count; i++)
			{
				Rect _rect = new Rect(2f, 29 + i * num, 132f, num);
				GuiSystem.SetChildRect(mMapElementTip.mFrameRect, ref _rect);
				mMapElementTip.mData[i] = new KeyValuePair<string, Rect>(mMapElementTip.mData[i].Key, _rect);
			}
		}
	}

	private void OnJoin(MapType _type, int _mapId)
	{
		if (mJoinCallback != null)
		{
			mJoinCallback(_type, _mapId);
		}
		RemoveMapJoinMenu();
	}
}
