using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public enum CamMode
	{
		GAME_NONE_CAM_MODE,
		GAME_NORMAL_CAM_MODE,
		GAME_FLOAT_CAM_MODE
	}

	private enum ZoomTypes
	{
		ZOOM_NONE,
		ZOOM_IN,
		ZOOM_OUT
	}

	private struct Player
	{
		public GameObject curPlayer;

		public int playerId;

		public GameData mGameData;
	}

	public delegate void CameraMoved(Vector3 _lookPoint, float _zoom);

	private Vector3 mCamPosDirection;

	private Vector3 mCamLookPoint;

	private Vector3 mPrevCamLookPoint;

	private Vector3 mCamRot;

	private float mTargetYPos;

	private int mCamFov;

	private float mCamZoomSpeed;

	private bool mShakeEffect;

	private List<Vector3> mShakeOffsets;

	private int mShakeOffsetNum;

	private float mCurShakePower;

	private Player mCurPlayer;

	private bool mMoveCamToPlayer;

	private bool mSpacePressed;

	private float mStartMoveToPlayerTime;

	private float mMaxMoveToPlayerTime;

	private Vector3 mStartMovePlayerPos;

	private bool mRotated;

	private float mMaxDeltaYPos;

	public CameraMoved mCameraMoved;

	public GameObject mVisibilityObjPrefab;

	private GameObject mVisibilityObj;

	public Vector3 mLookPoint1;

	public Vector3 mLookPoint2;

	public Vector3 mLookPoint3;

	public Vector3 mLookPoint4;

	private Vector3 mUp;

	private Vector3 mDown;

	private Vector3 mRight;

	private Vector3 mLeft;

	private Vector3 mCurDir;

	private int mVerDir;

	private int mHorDir;

	private void Update()
	{
		if (OptionsMgr.CamMode == CamMode.GAME_NORMAL_CAM_MODE)
		{
			if (mCurPlayer.curPlayer != null)
			{
				if (mCurPlayer.mGameData != null && !mCurPlayer.mGameData.Data.Visible)
				{
					return;
				}
				SetCamLookPoint(mCamLookPoint * 0.85f + mCurPlayer.curPlayer.transform.position * 0.15f);
			}
		}
		else if (OptionsMgr.CamMode == CamMode.GAME_FLOAT_CAM_MODE)
		{
			if (mMoveCamToPlayer)
			{
				float num = Time.time - mStartMoveToPlayerTime;
				num = ((!(num > mStartMoveToPlayerTime)) ? num : mStartMoveToPlayerTime);
				float num2 = num / mMaxMoveToPlayerTime;
				if (num < mMaxMoveToPlayerTime)
				{
					if (!mSpacePressed)
					{
						SetCamLookPoint(mCamLookPoint * Mathf.Abs(num2 - 1f) + mStartMovePlayerPos * num2);
					}
					else
					{
						SetCamLookPoint(mCamLookPoint * 0.85f + mStartMovePlayerPos * 0.15f);
					}
				}
				else if (num >= mMaxMoveToPlayerTime)
				{
					if (!mSpacePressed)
					{
						SetCamLookPoint(mStartMovePlayerPos);
						CancelMoveToPlayer();
					}
					else
					{
						SetCamLookPoint(mCamLookPoint * 0.85f + mStartMovePlayerPos * 0.15f);
					}
				}
			}
			else
			{
				if (mVerDir == 0 && mHorDir == 0)
				{
					SetCamDirectionMove(Input.mousePosition);
				}
				Vector3 vector = mCurDir;
				vector.Normalize();
				if (mRotated)
				{
					vector = -vector;
				}
				MoveCam(vector);
			}
		}
		if (mShakeEffect)
		{
			if (mShakeOffsetNum > 0)
			{
				Vector3 moveOffset = mShakeOffsets[mShakeOffsetNum] * mCurShakePower;
				ShakeMoveCam(moveOffset);
				mShakeOffsetNum--;
			}
			else
			{
				mShakeEffect = false;
			}
		}
	}

	public void OnDisable()
	{
		if (mVisibilityObj != null)
		{
			Object.Destroy(mVisibilityObj);
		}
		mVisibilityObj = null;
	}

	public void OnGUI()
	{
		if (GuiSystem.InputIsFree())
		{
			SetCurDirByKey(Event.current.type, Event.current.keyCode);
		}
		switch (Event.current.type)
		{
		case EventType.KeyDown:
			if ((Event.current.keyCode == KeyCode.Plus || Event.current.keyCode == KeyCode.KeypadPlus) && GuiSystem.InputIsFree())
			{
				ZoomCam(ZoomTypes.ZOOM_OUT);
			}
			if ((Event.current.keyCode == KeyCode.Minus || Event.current.keyCode == KeyCode.KeypadMinus) && GuiSystem.InputIsFree())
			{
				ZoomCam(ZoomTypes.ZOOM_IN);
			}
			if (Event.current.keyCode == KeyCode.Space && GuiSystem.InputIsFree())
			{
				if (mCurPlayer.curPlayer != null)
				{
					mStartMovePlayerPos = mCurPlayer.curPlayer.transform.position;
				}
				mStartMoveToPlayerTime = Time.time;
				mMoveCamToPlayer = true;
				mSpacePressed = true;
			}
			break;
		case EventType.KeyUp:
			if (Event.current.keyCode == KeyCode.C && GuiSystem.InputIsFree())
			{
				SwitchCamMode();
			}
			if (Event.current.keyCode == KeyCode.Space && GuiSystem.InputIsFree())
			{
				mSpacePressed = false;
			}
			break;
		case EventType.ScrollWheel:
			if (GUI.GetNameOfFocusedControl() != "VERTICAL_SCROLLBAR")
			{
				if (Event.current.delta.y > 0f)
				{
					ZoomCam(ZoomTypes.ZOOM_IN);
				}
				else
				{
					ZoomCam(ZoomTypes.ZOOM_OUT);
				}
				Event.current.Use();
			}
			break;
		}
	}

	private void SetCurDirByKey(EventType _type, KeyCode _keyCode)
	{
		if (OptionsMgr.CamMode == CamMode.GAME_FLOAT_CAM_MODE)
		{
			mVerDir = 0;
			mHorDir = 0;
			if (Input.GetKey(KeyCode.UpArrow))
			{
				mVerDir++;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				mVerDir--;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				mHorDir++;
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				mHorDir--;
			}
			mCurDir = Vector3.zero;
			mCurDir = ((mVerDir != 1) ? mCurDir : (mCurDir + mDown));
			mCurDir = ((mVerDir != -1) ? mCurDir : (mCurDir + mUp));
			mCurDir = ((mHorDir != 1) ? mCurDir : (mCurDir + mRight));
			mCurDir = ((mHorDir != -1) ? mCurDir : (mCurDir + mLeft));
		}
	}

	public void Init(bool _rot)
	{
		mRotated = _rot;
		mMaxMoveToPlayerTime = 1f;
		if (mVisibilityObjPrefab != null && mVisibilityObj == null)
		{
			mVisibilityObj = Object.Instantiate(mVisibilityObjPrefab) as GameObject;
			GameObjUtil.TrySetParent(mVisibilityObj, "/cameras");
		}
		if (mRotated)
		{
			mCamPosDirection = new Vector3(-12f, 21.5f, -12f).normalized;
			mCamRot = new Vector3(55f, 45f, 0f);
			if (mVisibilityObj != null)
			{
				mVisibilityObj.transform.Rotate(new Vector3(0f, 180f, 0f));
			}
		}
		else
		{
			mCamPosDirection = new Vector3(12f, 21.5f, 12f).normalized;
			mCamRot = new Vector3(55f, 225f, 0f);
		}
		mUp = new Vector3(1f, 0f, 1f);
		mDown = new Vector3(-1f, 0f, -1f);
		mRight = new Vector3(-1f, 0f, 1f);
		mLeft = new Vector3(1f, 0f, -1f);
		mMaxDeltaYPos = 10f;
		mCamFov = 45;
		mCamZoomSpeed = 2f;
		mShakeEffect = false;
		mCurShakePower = 1f;
		base.camera.transform.eulerAngles = mCamRot;
		base.camera.fieldOfView = mCamFov;
		InitShakeOffsets();
	}

	private void InitShakeOffsets()
	{
		mShakeOffsets = new List<Vector3>();
		float num = 0.5f;
		Vector3 item = new Vector3(0f - num, 0f, 0f);
		Vector3 item2 = new Vector3(0f, 0f, num);
		Vector3 item3 = new Vector3(0f, 0f, 0f - num);
		Vector3 item4 = new Vector3(num, 0f, 0f - num);
		mShakeOffsets.Add(item);
		mShakeOffsets.Add(item2);
		mShakeOffsets.Add(item4);
		mShakeOffsets.Add(item3);
		mShakeOffsets.Add(item2);
		mShakeOffsets.Add(item2);
	}

	private void LookAtPoint(Vector3 lookPoint)
	{
		Vector3 position = lookPoint + mCamPosDirection * OptionsMgr.CamPosVal;
		base.transform.position = position;
		if (mCameraMoved != null)
		{
			mCameraMoved(lookPoint, OptionsMgr.CamPosVal);
		}
	}

	public void ForceMove(Vector3 _point)
	{
		mCamLookPoint = _point;
		LookAtPoint(mCamLookPoint);
	}

	private void SetCamDirectionMove(Vector3 _mousePos)
	{
		int num = 15;
		mCurDir = Vector3.zero;
		if (_mousePos.y < (float)num)
		{
			mCurDir += mUp;
		}
		else if (_mousePos.y > (float)(Screen.height - num))
		{
			mCurDir += mDown;
		}
		if (_mousePos.x > (float)(Screen.width - num))
		{
			mCurDir += mRight;
		}
		else if (_mousePos.x < (float)num)
		{
			mCurDir += mLeft;
		}
	}

	private void MoveCam(Vector3 _moveOffset)
	{
		SetCamLookPoint(mCamLookPoint + _moveOffset * OptionsMgr.mCamSpeed);
		SetCamYPos();
	}

	private void ShakeMoveCam(Vector3 _moveOffset)
	{
		SetCamLookPoint(mCamLookPoint + _moveOffset * OptionsMgr.mCamSpeed);
	}

	public void SetCamLookPoint(Vector3 _lookPoint)
	{
		mPrevCamLookPoint = mCamLookPoint;
		mCamLookPoint = _lookPoint;
		if (OptionsMgr.CamMode == CamMode.GAME_FLOAT_CAM_MODE)
		{
			SetCamTargetYPos();
		}
		LookAtPoint(mCamLookPoint);
		CalculateZonePoints();
		if (mVisibilityObj != null)
		{
			mVisibilityObj.transform.position = mCamLookPoint;
		}
	}

	public void CancelMoveToPlayer()
	{
		mMoveCamToPlayer = false;
		mStartMoveToPlayerTime = 0f;
		mStartMovePlayerPos = Vector3.zero;
	}

	public void SetPrevCamLookPoint()
	{
		SetCamLookPoint(mPrevCamLookPoint);
	}

	private void ZoomCam(ZoomTypes zoomType)
	{
		switch (zoomType)
		{
		case ZoomTypes.ZOOM_IN:
			OptionsMgr.CamPosVal += mCamZoomSpeed;
			break;
		case ZoomTypes.ZOOM_OUT:
			OptionsMgr.CamPosVal -= mCamZoomSpeed;
			break;
		}
	}

	private void SetCamTargetYPos()
	{
		if (Physics.Raycast(new Ray(base.transform.position + mCamPosDirection * 1024f, -mCamPosDirection), out var hitInfo, 2048f, 1024) && hitInfo.collider.gameObject != null)
		{
			mTargetYPos = hitInfo.point.y;
		}
	}

	private void SetCamYPos()
	{
		float num = mTargetYPos - mCamLookPoint.y;
		float num2 = Mathf.Abs(num / mMaxDeltaYPos);
		if (num > mMaxDeltaYPos)
		{
			mCamLookPoint.y = mTargetYPos - mMaxDeltaYPos;
		}
		else
		{
			mCamLookPoint.y = Mathf.Lerp(mCamLookPoint.y, mTargetYPos, Time.deltaTime * ((!(num2 > 1f)) ? 1f : num2));
		}
	}

	public void SetPlayer(GameObject _curPlayer, int _playerId)
	{
		Player player = default(Player);
		player.curPlayer = _curPlayer;
		player.playerId = _playerId;
		GameData gameData = (player.mGameData = player.curPlayer.GetComponent<GameData>());
		if (_curPlayer == null)
		{
			Log.Debug("player object is null");
			player.playerId = -1;
		}
		mCurPlayer = player;
	}

	public void SwitchCamMode()
	{
		CancelMoveToPlayer();
		mCurDir = Vector3.zero;
		mVerDir = 0;
		mHorDir = 0;
		if (OptionsMgr.CamMode == CamMode.GAME_FLOAT_CAM_MODE)
		{
			OptionsMgr.CamMode = CamMode.GAME_NORMAL_CAM_MODE;
		}
		else
		{
			OptionsMgr.CamMode = CamMode.GAME_FLOAT_CAM_MODE;
		}
		UserLog.AddAction(UserActionType.SWITCH_CAM_MODE, (int)OptionsMgr.CamMode, GuiSystem.GetLocaleText(OptionsMgr.CamMode.ToString()));
	}

	public void MakeShakeEffect(Vector3 _shakePos)
	{
		if (!mShakeEffect)
		{
			mShakeOffsetNum = mShakeOffsets.Count - 1;
			float num = 25f;
			_shakePos.y = mCamLookPoint.y;
			mCurShakePower = 1f - (_shakePos - mCamLookPoint).magnitude / num;
			if (mCurShakePower > 0f)
			{
				mShakeEffect = true;
			}
		}
	}

	public Vector3 GetCamLookPoint()
	{
		return mCamLookPoint;
	}

	private void CalculateZonePoints()
	{
		Ray ray = base.camera.ScreenPointToRay(Vector3.zero);
		Ray ray2 = base.camera.ScreenPointToRay(new Vector3(Screen.width, 0f, 0f));
		Ray ray3 = base.camera.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0f));
		Ray ray4 = base.camera.ScreenPointToRay(new Vector3(0f, Screen.height, 0f));
		mLookPoint1.x = (mCamLookPoint.y - ray.origin.y) / ray.direction.y * ray.direction.x + ray.origin.x;
		mLookPoint1.z = (mCamLookPoint.y - ray.origin.y) / ray.direction.y * ray.direction.z + ray.origin.z;
		mLookPoint2.x = (mCamLookPoint.y - ray2.origin.y) / ray2.direction.y * ray2.direction.x + ray2.origin.x;
		mLookPoint2.z = (mCamLookPoint.y - ray2.origin.y) / ray2.direction.y * ray2.direction.z + ray2.origin.z;
		mLookPoint3.x = (mCamLookPoint.y - ray3.origin.y) / ray3.direction.y * ray3.direction.x + ray3.origin.x;
		mLookPoint3.z = (mCamLookPoint.y - ray3.origin.y) / ray3.direction.y * ray3.direction.z + ray3.origin.z;
		mLookPoint4.x = (mCamLookPoint.y - ray4.origin.y) / ray4.direction.y * ray4.direction.x + ray4.origin.x;
		mLookPoint4.z = (mCamLookPoint.y - ray4.origin.y) / ray4.direction.y * ray4.direction.z + ray4.origin.z;
	}
}
