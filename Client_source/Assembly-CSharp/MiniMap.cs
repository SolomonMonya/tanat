using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class MiniMap : GuiElement
{
	private class MapObject
	{
		public Rect mRect = default(Rect);

		public Texture2D mImage;

		public bool mValid = true;
	}

	private class Beacon : MapObject
	{
		public class Fx
		{
			public bool mStarted;

			public float mDelay;

			public float mCurScale;

			public float mSpeed;

			public float mScaleLim;

			public Rect mDrawRect;
		}

		public float mStartTime;

		public MapObject mFxBase = new MapObject();

		public Queue<Fx> mFxSteps = new Queue<Fx>();
	}

	public delegate void OnShowWorldMap();

	public delegate void OnMiniMapClick(int _btnNum, Vector2 _pos);

	private const int mGuiMapSize = 180;

	public OnMiniMapClick mOnClick;

	private Texture2D mMiniMapBgImg;

	private Texture2D mMiniMapImg;

	private Texture2D mPlayerAvaImg;

	private Texture2D mEnemyAvaImg;

	private Texture2D mFriendAvaImg;

	private Texture2D mFriendBuildingImg;

	private Texture2D mEnemyBuildingImg;

	private Texture2D mShopImg;

	private Texture2D mFriendCreepImg;

	private Texture2D mEnemyCreepImg;

	private Texture2D mBeaconImg;

	private Texture2D mBeaconImgFx;

	private Matrix4x4 mPosMatrix = Matrix4x4.zero;

	private Matrix4x4 mFogMatrix = Matrix4x4.zero;

	private Rect mMapRect = default(Rect);

	private Vector2 mMapOffset = Vector2.zero;

	private float mMapSize;

	private float mMapScale;

	private float mMapRot;

	private RenderTexture mFogTexture;

	private Dictionary<int, MapObject> mMapObjects = new Dictionary<int, MapObject>();

	private List<int> mMapObjectsToRemove = new List<int>();

	private Material mLineMat;

	private GameCamera mMainCamera;

	private Vector3[] mCamRectPoints;

	private GuiButton[] mMapButtons;

	public OnShowWorldMap mOnShowWorldMap;

	private Queue<Beacon> mBeacons = new Queue<Beacon>();

	private bool mForceBeaconClick;

	private GameObjectsProvider mObjProv;

	public bool ForceBeaconClick
	{
		get
		{
			return mForceBeaconClick;
		}
		set
		{
			mForceBeaconClick = value;
			mMapButtons[1].Pressed = mForceBeaconClick;
		}
	}

	public override void Init()
	{
		mMiniMapBgImg = GuiSystem.GetImage("Gui/Maps/mini_map_frame1");
		mEnemyAvaImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Target_Red");
		mPlayerAvaImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Target_Self");
		mFriendAvaImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Target_Green");
		mFriendBuildingImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Big_green");
		mEnemyBuildingImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Big_red");
		mShopImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Big_yellow");
		mFriendCreepImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Small_green");
		mEnemyCreepImg = GuiSystem.GetImage("Gui/Maps/MiniMapIcons/Small_red");
		mBeaconImg = GuiSystem.GetImage("Gui/Maps/map_marker_01");
		mBeaconImgFx = GuiSystem.GetImage("Gui/Maps/map_marker_02");
		mLineMat = new Material(Shader.Find("Tanat/Fx/Wireframe"));
		mLineMat.hideFlags = HideFlags.HideAndDontSave;
		mLineMat.shader.hideFlags = HideFlags.HideAndDontSave;
		mCamRectPoints = new Vector3[8];
		for (int i = 0; i < mCamRectPoints.Length; i++)
		{
			ref Vector3 reference = ref mCamRectPoints[i];
			reference = Vector3.zero;
		}
		mMapButtons = new GuiButton[3];
		for (int j = 0; j < 3; j++)
		{
			string text = "Gui/Maps/button_" + (j + 1);
			mMapButtons[j] = GuiSystem.CreateButton(text + "_norm", text + "_over", text + "_press", string.Empty, string.Empty);
			mMapButtons[j].mElementId = "MAP_BUTTON_" + (j + 1);
			GuiButton obj = mMapButtons[j];
			obj.mOnMouseUp = (OnMouseUp)Delegate.Combine(obj.mOnMouseUp, new OnMouseUp(OnButton));
			mMapButtons[j].Init();
			mMapButtons[j].mLocked = j == 0 && mOnShowWorldMap == null;
			if (j == 2)
			{
				AddTutorialElement(mMapButtons[j].mElementId, mMapButtons[j]);
			}
		}
	}

	public override void Uninit()
	{
		mFogTexture = null;
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 0f, mMiniMapBgImg.width, mMiniMapBgImg.height);
		mMapRect = new Rect(10f, 8f, 180f, 180f);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width + 3f * GuiSystem.mYRate;
		mZoneRect.y = (float)OptionsMgr.mScreenHeight - mZoneRect.height;
		GuiSystem.SetChildRect(mZoneRect, ref mMapRect);
		mFogMatrix = Matrix4x4.zero;
		mFogMatrix.SetTRS(new Vector3(mMapRect.width / 2f, mMapRect.height / 2f, 0f), Quaternion.AngleAxis(mMapRot, Vector3.forward), new Vector3(mMapScale, 0f - mMapScale, mMapScale));
		for (int i = 0; i < 3; i++)
		{
			mMapButtons[i].mZoneRect = new Rect(67 + i * 23, -8f, 23f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref mMapButtons[i].mZoneRect);
		}
		PopupInfo.AddTip(this, "TIP_TEXT5_1", mMapButtons[0].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT4", mMapButtons[1].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT5", mMapButtons[2].mZoneRect);
	}

	public void SetMap(string _mapId, float _mapSize, Vector2 _offset, float _scale, float _rot)
	{
		lock (mBeacons)
		{
			mBeacons.Clear();
		}
		mMiniMapImg = null;
		mMapObjectsToRemove.Clear();
		mMapObjects.Clear();
		mMainCamera = Camera.main.GetComponent<GameCamera>();
		if (!string.IsNullOrEmpty(_mapId))
		{
			string file = "Gui/Maps/MiniMaps/" + _mapId;
			mMiniMapImg = GuiSystem.GetImage(file);
		}
		else
		{
			mMiniMapImg = null;
		}
		mMapSize = _mapSize;
		mMapOffset = _offset;
		mMapScale = _scale;
		mMapRot = _rot;
		float num = 180f / mMapSize;
		mMapOffset *= num;
		mPosMatrix = (mFogMatrix = Matrix4x4.zero);
		mPosMatrix.SetTRS(Vector3.zero, Quaternion.AngleAxis(mMapRot, Vector3.forward), new Vector3(mMapScale * num, mMapScale * num, mMapScale * num));
		mFogMatrix.SetTRS(new Vector3(mMapRect.width / 2f, mMapRect.height / 2f, 0f), Quaternion.AngleAxis(mMapRot, Vector3.forward), new Vector3(mMapScale, 0f - mMapScale, mMapScale));
	}

	public void SetTexture(RenderTexture _fogTex)
	{
		if (_fogTex.IsCreated())
		{
			mFogTexture = _fogTex;
		}
	}

	public override void RenderElement()
	{
		if (MainInfoWindow.mHidden)
		{
			return;
		}
		if ((bool)mMiniMapImg)
		{
			GuiSystem.DrawImage(mMiniMapImg, mMapRect);
		}
		foreach (KeyValuePair<int, MapObject> mMapObject in mMapObjects)
		{
			GuiSystem.DrawImage(mMapObject.Value.mImage, mMapObject.Value.mRect);
		}
		RenderCurFogTexture();
		RenderCamRect();
		if ((bool)mMiniMapBgImg)
		{
			GuiSystem.DrawImage(mMiniMapBgImg, mZoneRect);
		}
		lock (mBeacons)
		{
			RenderBeacons();
		}
		GuiButton[] array = mMapButtons;
		foreach (GuiButton guiButton in array)
		{
			guiButton.RenderElement();
		}
	}

	private void RenderBeacons()
	{
		foreach (Beacon mBeacon in mBeacons)
		{
			GuiSystem.DrawImage(mBeacon.mImage, mBeacon.mRect);
			foreach (Beacon.Fx mFxStep in mBeacon.mFxSteps)
			{
				GuiSystem.DrawImage(mBeacon.mFxBase.mImage, mFxStep.mDrawRect);
			}
		}
	}

	public override void Update()
	{
		SetCamPos();
		if (Time.frameCount % 4 == 0)
		{
			InitMapObjects();
			CleanMapObjects();
			SetCamRect();
		}
		lock (mBeacons)
		{
			UpdateBeacons();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		GuiButton[] array = mMapButtons;
		foreach (GuiButton guiButton in array)
		{
			guiButton.CheckEvent(_curEvent);
		}
		if (mMapRect.Contains(_curEvent.mousePosition) && (_curEvent.type == EventType.MouseDown || _curEvent.type == EventType.MouseDrag))
		{
			Vector4 vector = new Vector4(_curEvent.mousePosition.x - mMapRect.x, mMapRect.height - _curEvent.mousePosition.y + mMapRect.y, 0f, 0f);
			vector /= GuiSystem.mYRate;
			vector.x += mMapOffset.x;
			vector.y += mMapOffset.y;
			vector = mPosMatrix.inverse * vector;
			if (_curEvent.type == EventType.MouseDown && mOnClick != null)
			{
				mOnClick(_curEvent.button, new Vector2(vector.x, vector.y));
			}
			if ((_curEvent.type == EventType.MouseDown || _curEvent.type == EventType.MouseDrag) && _curEvent.button == 0 && !_curEvent.alt && OptionsMgr.CamMode == GameCamera.CamMode.GAME_FLOAT_CAM_MODE)
			{
				mMainCamera.SetCamLookPoint(new Vector3(vector.x, mMainCamera.GetCamLookPoint().y, vector.y));
				mMainCamera.CancelMoveToPlayer();
			}
			_curEvent.Use();
		}
		base.CheckEvent(_curEvent);
	}

	private void UpdateBeacons()
	{
		if (mBeacons.Count == 0)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (Beacon mBeacon in mBeacons)
		{
			if (mBeacon.mFxSteps.Count == 0)
			{
				continue;
			}
			foreach (Beacon.Fx mFxStep in mBeacon.mFxSteps)
			{
				float num = realtimeSinceStartup - mBeacon.mStartTime;
				if (!(num <= mFxStep.mDelay))
				{
					mFxStep.mStarted = true;
					Rect mRect = mBeacon.mFxBase.mRect;
					mRect.width *= mFxStep.mCurScale;
					mRect.height *= mFxStep.mCurScale;
					mRect.x += (mBeacon.mFxBase.mRect.width - mRect.width) * 0.5f;
					mRect.y += (mBeacon.mFxBase.mRect.height - mRect.height) * 0.5f;
					mFxStep.mDrawRect = mRect;
					mFxStep.mCurScale += mFxStep.mSpeed * Time.deltaTime;
				}
			}
			Beacon.Fx fx = mBeacon.mFxSteps.Peek();
			if (fx.mStarted && fx.mCurScale >= fx.mScaleLim)
			{
				mBeacon.mFxSteps.Dequeue();
			}
		}
		Beacon beacon = mBeacons.Peek();
		if (beacon.mFxSteps.Count == 0)
		{
			mBeacons.Dequeue();
		}
	}

	private void InitMapObjects()
	{
		if (mObjProv == null)
		{
			return;
		}
		IEnumerable<IGameObject> visibleObjects = mObjProv.VisibleObjects;
		foreach (IGameObject item in visibleObjects)
		{
			GameData gameData = (GameData)item;
			if (gameData.Symbol == GameData.MapSymbol.UNDEFINED || item.Data == null)
			{
				continue;
			}
			MapObject mapObject = new MapObject();
			if (!mMapObjects.ContainsKey(item.Id))
			{
				if (item.Data.IsPlayerBinded && item.Data.Player.IsSelf)
				{
					mapObject.mImage = mPlayerAvaImg;
				}
				else
				{
					SetObjImage(ref mapObject.mImage, gameData.Symbol, item.Data.GetFriendliness() == Friendliness.ENEMY);
				}
				mMapObjects.Add(item.Id, mapObject);
			}
			else
			{
				mapObject = mMapObjects[item.Id];
				mapObject.mValid = true;
			}
			item.Data.GetPosition(out var _x, out var _y);
			SetMapObjectRect(mapObject, _x, _y);
		}
	}

	private void SetMapObjectRect(MapObject _mapObject, float _x, float _y)
	{
		Vector4 vector = new Vector4(_x, _y, 0f, 0f);
		vector = mPosMatrix * vector;
		int width = _mapObject.mImage.width;
		int height = _mapObject.mImage.height;
		float left = vector.x - (float)width / 2f - mMapOffset.x;
		float top = 180f + mMapOffset.y - vector.y - (float)height / 2f;
		_mapObject.mRect = new Rect(left, top, width, height);
		GuiSystem.SetChildRect(mMapRect, ref _mapObject.mRect);
	}

	private void SetCamPos()
	{
		if (!(mMainCamera == null))
		{
			Vector3 camLookPoint = mMainCamera.GetCamLookPoint();
			Vector4 vector = new Vector4(camLookPoint.x, camLookPoint.z, 0f, 0f);
			vector = mPosMatrix * vector;
			Rect _rect = new Rect(vector.x - mMapOffset.x, 180f + mMapOffset.y - vector.y, 0f, 0f);
			GuiSystem.SetChildRect(mMapRect, ref _rect);
			Vector2 point = new Vector2(_rect.x, _rect.y);
			if (!mMapRect.Contains(point))
			{
				mMainCamera.SetPrevCamLookPoint();
			}
		}
	}

	private void CleanMapObjects()
	{
		foreach (KeyValuePair<int, MapObject> mMapObject in mMapObjects)
		{
			if (!mMapObject.Value.mValid)
			{
				mMapObjectsToRemove.Add(mMapObject.Key);
			}
			else
			{
				mMapObject.Value.mValid = false;
			}
		}
		foreach (int item in mMapObjectsToRemove)
		{
			mMapObjects.Remove(item);
		}
		mMapObjectsToRemove.Clear();
	}

	private void RenderCurFogTexture()
	{
		if (!(mFogTexture == null))
		{
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, mMapRect.width, 0f, mMapRect.height);
			GL.Viewport(new Rect(mMapRect.x, (float)OptionsMgr.mScreenHeight - mMapRect.height - mMapRect.y, mMapRect.width, mMapRect.height));
			GL.MultMatrix(mFogMatrix);
			GL.Clear(clearDepth: true, clearColor: false, Color.clear);
			GuiSystem.DrawImage(mFogTexture, new Rect((0f - mMapRect.width) / 2f, (0f - mMapRect.height) / 2f, mMapRect.width, mMapRect.height));
			GL.Viewport(new Rect(0f, 0f, OptionsMgr.mScreenWidth, OptionsMgr.mScreenHeight));
			GL.PopMatrix();
		}
	}

	private void RenderCamRect()
	{
		if (!(mMainCamera == null))
		{
			mLineMat.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, mMapRect.width, 0f, mMapRect.height);
			GL.Viewport(new Rect(mMapRect.x, (float)Screen.height - mMapRect.height - mMapRect.y, mMapRect.width, mMapRect.height));
			GL.Begin(1);
			GL.Color(Color.white);
			Vector3[] array = mCamRectPoints;
			foreach (Vector3 v in array)
			{
				GL.Vertex(v);
			}
			GL.End();
			GL.Viewport(new Rect(0f, 0f, Screen.width, Screen.height));
			GL.PopMatrix();
		}
	}

	private void SetCamRect()
	{
		if (!(mMainCamera == null))
		{
			Vector4 vector = new Vector4(mMainCamera.mLookPoint1.x, mMainCamera.mLookPoint1.z, 0f, 0f);
			Vector4 vector2 = new Vector4(mMainCamera.mLookPoint2.x, mMainCamera.mLookPoint2.z, 0f, 0f);
			Vector4 vector3 = new Vector4(mMainCamera.mLookPoint3.x, mMainCamera.mLookPoint3.z, 0f, 0f);
			Vector4 vector4 = new Vector4(mMainCamera.mLookPoint4.x, mMainCamera.mLookPoint4.z, 0f, 0f);
			vector = mPosMatrix * vector;
			vector2 = mPosMatrix * vector2;
			vector3 = mPosMatrix * vector3;
			vector4 = mPosMatrix * vector4;
			mCamRectPoints[0].x = (vector.x - mMapOffset.x) * GuiSystem.mYRate;
			mCamRectPoints[0].y = (vector.y - mMapOffset.y) * GuiSystem.mYRate;
			mCamRectPoints[1].x = (vector2.x - mMapOffset.x) * GuiSystem.mYRate;
			mCamRectPoints[1].y = (vector2.y - mMapOffset.y) * GuiSystem.mYRate;
			ref Vector3 reference = ref mCamRectPoints[2];
			reference = mCamRectPoints[1];
			mCamRectPoints[3].x = (vector3.x - mMapOffset.x) * GuiSystem.mYRate;
			mCamRectPoints[3].y = (vector3.y - mMapOffset.y) * GuiSystem.mYRate;
			ref Vector3 reference2 = ref mCamRectPoints[4];
			reference2 = mCamRectPoints[3];
			mCamRectPoints[5].x = (vector4.x - mMapOffset.x) * GuiSystem.mYRate;
			mCamRectPoints[5].y = (vector4.y - mMapOffset.y) * GuiSystem.mYRate;
			ref Vector3 reference3 = ref mCamRectPoints[6];
			reference3 = mCamRectPoints[5];
			ref Vector3 reference4 = ref mCamRectPoints[7];
			reference4 = mCamRectPoints[0];
		}
	}

	private void SetObjImage(ref Texture2D _img, GameData.MapSymbol _symbol, bool _enemy)
	{
		switch (_symbol)
		{
		case GameData.MapSymbol.UNIT:
			if (_enemy)
			{
				_img = mEnemyCreepImg;
			}
			else
			{
				_img = mFriendCreepImg;
			}
			break;
		case GameData.MapSymbol.AVATAR:
			if (_enemy)
			{
				_img = mEnemyAvaImg;
			}
			else
			{
				_img = mFriendAvaImg;
			}
			break;
		case GameData.MapSymbol.BUILDING:
			if (_enemy)
			{
				_img = mEnemyBuildingImg;
			}
			else
			{
				_img = mFriendBuildingImg;
			}
			break;
		case GameData.MapSymbol.SHOP:
			_img = mShopImg;
			break;
		default:
			_img = null;
			break;
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "MAP_BUTTON_1" && _buttonId == 0)
		{
			if (mOnShowWorldMap != null)
			{
				mOnShowWorldMap();
			}
		}
		else if (_sender.mElementId == "MAP_BUTTON_2" && _buttonId == 0)
		{
			ForceBeaconClick = true;
		}
		else if (_sender.mElementId == "MAP_BUTTON_3" && _buttonId == 0)
		{
			mMainCamera.SwitchCamMode();
		}
	}

	public void AddBeacon(float _x, float _y)
	{
		Beacon beacon = new Beacon();
		beacon.mImage = mBeaconImg;
		SetMapObjectRect(beacon, _x, _y);
		beacon.mFxBase.mImage = mBeaconImgFx;
		SetMapObjectRect(beacon.mFxBase, _x, _y);
		Beacon.Fx fx = new Beacon.Fx();
		fx.mDelay = 0.1f;
		fx.mCurScale = 0f;
		fx.mSpeed = 0.7f;
		fx.mScaleLim = 1f;
		beacon.mFxSteps.Enqueue(fx);
		fx = new Beacon.Fx();
		fx.mDelay = 0.7f;
		fx.mCurScale = 0f;
		fx.mSpeed = 0.5f;
		fx.mScaleLim = 0.5f;
		beacon.mFxSteps.Enqueue(fx);
		fx = new Beacon.Fx();
		fx.mDelay = 1.1f;
		fx.mCurScale = 0f;
		fx.mSpeed = 0.3f;
		fx.mScaleLim = 0.3f;
		beacon.mFxSteps.Enqueue(fx);
		fx = new Beacon.Fx();
		fx.mDelay = 2.4f;
		fx.mCurScale = 0f;
		fx.mSpeed = 0f;
		fx.mScaleLim = -1f;
		beacon.mFxSteps.Enqueue(fx);
		beacon.mStartTime = Time.realtimeSinceStartup;
		lock (mBeacons)
		{
			mBeacons.Enqueue(beacon);
		}
	}

	public void SetData(GameObjectsProvider _objProv)
	{
		mObjProv = _objProv;
	}

	public void Clear()
	{
		mObjProv = null;
		Uninit();
	}
}
