using UnityEngine;

public class EditSceneCamera : MonoBehaviour
{
	private enum ZoomTypes
	{
		ZOOM_NONE,
		ZOOM_IN,
		ZOOM_OUT
	}

	private float mCamPosVal;

	private float mMinCamPosVal;

	private float mMaxCamPosVal;

	private Vector3 mCamPosDirection = Vector3.zero;

	private Vector3 mCamLookPoint = Vector3.zero;

	private float mCamZoomSpeed;

	private float mCurMoveSpeed;

	private float mStartMoveSpeed;

	private float mDeltaMoveSpeed;

	private Rect mZoneRect = default(Rect);

	public Camera mCurCamera;

	public Vector3 mLookPoint1 = Vector3.zero;

	public Vector3 mLookPoint2 = Vector3.zero;

	public Vector3 mLookPoint3 = Vector3.zero;

	public Vector3 mLookPoint4 = Vector3.zero;

	public void Start()
	{
		mCamPosDirection = new Vector3(10.6f, 21.5f, 10.6f).normalized;
		mMinCamPosVal = 15f;
		mMaxCamPosVal = 100f;
		mCamZoomSpeed = 2f;
		mCamPosVal = mMaxCamPosVal / 2f;
		mStartMoveSpeed = 0.5f;
		mCurMoveSpeed = mStartMoveSpeed;
		mDeltaMoveSpeed = 0.1f;
		base.camera.transform.eulerAngles = new Vector3(55f, 225f, 0f);
		base.camera.fieldOfView = 30f;
		mCurCamera = base.gameObject.GetComponent<Camera>();
	}

	public void OnGUI()
	{
		if (Event.current == null)
		{
			return;
		}
		if (Event.current.type == EventType.ScrollWheel && !Event.current.alt && !Event.current.control)
		{
			if (Event.current.delta.y > 0f)
			{
				ZoomCam(ZoomTypes.ZOOM_IN);
			}
			else
			{
				ZoomCam(ZoomTypes.ZOOM_OUT);
			}
			LookAtPoint();
		}
		else if (Event.current.type == EventType.KeyDown)
		{
			switch (Event.current.keyCode)
			{
			case KeyCode.UpArrow:
				if (SetCamLookPoint(mCamLookPoint + new Vector3(-1f, 0f, -1f).normalized * mCurMoveSpeed))
				{
					mCurMoveSpeed += mDeltaMoveSpeed;
				}
				else
				{
					mCurMoveSpeed = mStartMoveSpeed;
				}
				break;
			case KeyCode.DownArrow:
				if (SetCamLookPoint(mCamLookPoint + new Vector3(1f, 0f, 1f).normalized * mCurMoveSpeed))
				{
					mCurMoveSpeed += mDeltaMoveSpeed;
				}
				else
				{
					mCurMoveSpeed = mStartMoveSpeed;
				}
				break;
			case KeyCode.RightArrow:
				if (SetCamLookPoint(mCamLookPoint + new Vector3(-1f, 0f, 1f).normalized * mCurMoveSpeed))
				{
					mCurMoveSpeed += mDeltaMoveSpeed;
				}
				else
				{
					mCurMoveSpeed = mStartMoveSpeed;
				}
				break;
			case KeyCode.LeftArrow:
				if (SetCamLookPoint(mCamLookPoint + new Vector3(1f, 0f, -1f).normalized * mCurMoveSpeed))
				{
					mCurMoveSpeed += mDeltaMoveSpeed;
				}
				else
				{
					mCurMoveSpeed = mStartMoveSpeed;
				}
				break;
			default:
				mCurMoveSpeed = mStartMoveSpeed;
				break;
			}
			LookAtPoint();
		}
		else if (Event.current.type == EventType.KeyUp)
		{
			switch (Event.current.keyCode)
			{
			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
			case KeyCode.RightArrow:
			case KeyCode.LeftArrow:
				mCurMoveSpeed = mStartMoveSpeed;
				break;
			}
		}
		else if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
		{
			Vector3 vector = new Vector3(1f, 0f, 1f);
			Vector3 vector2 = Vector3.zero;
			Vector3 zero = Vector3.zero;
			if (Event.current.delta.x > 0f)
			{
				vector2 = new Vector3(1f, 0f, -1f);
			}
			else if (Event.current.delta.x < 0f)
			{
				vector2 = new Vector3(-1f, 0f, 1f);
			}
			vector *= 0f - Event.current.delta.y;
			vector2 *= Mathf.Abs(Event.current.delta.x);
			zero = vector + vector2;
			SetCamLookPoint(mCamLookPoint + zero.normalized * mStartMoveSpeed);
		}
	}

	public bool SetCamLookPoint(Vector3 _lookPoint)
	{
		Vector2 point = new Vector2(_lookPoint.x, _lookPoint.z);
		if (!mZoneRect.Contains(point))
		{
			return false;
		}
		mCamLookPoint = _lookPoint;
		LookAtPoint();
		CalculateZonePoints();
		return true;
	}

	public Vector3 GetCamLookPoint()
	{
		return mCamLookPoint;
	}

	public void SetLookZone(Rect _zoneRect)
	{
		mZoneRect = _zoneRect;
	}

	public Vector3 ScreenPointToWorldY(Vector3 _screenPoint, float _targetY)
	{
		Vector3 zero = Vector3.zero;
		Ray ray = mCurCamera.ScreenPointToRay(_screenPoint);
		zero.x = (_targetY - ray.origin.y) / ray.direction.y * ray.direction.x + ray.origin.x;
		zero.y = _targetY;
		zero.z = (_targetY - ray.origin.y) / ray.direction.y * ray.direction.z + ray.origin.z;
		return zero;
	}

	private void CalculateZonePoints()
	{
		Ray ray = mCurCamera.ScreenPointToRay(new Vector3(0f, 0f, 0f));
		Ray ray2 = mCurCamera.ScreenPointToRay(new Vector3(Screen.width, 0f, 0f));
		Ray ray3 = mCurCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0f));
		Ray ray4 = mCurCamera.ScreenPointToRay(new Vector3(0f, Screen.height, 0f));
		mLookPoint1.x = (0f - ray.origin.y) / ray.direction.y * ray.direction.x + ray2.origin.x;
		mLookPoint1.z = (0f - ray.origin.y) / ray.direction.y * ray.direction.z + ray2.origin.z;
		mLookPoint2.x = (0f - ray2.origin.y) / ray2.direction.y * ray2.direction.x + ray2.origin.x;
		mLookPoint2.z = (0f - ray2.origin.y) / ray2.direction.y * ray2.direction.z + ray2.origin.z;
		mLookPoint3.x = (0f - ray3.origin.y) / ray3.direction.y * ray3.direction.x + ray3.origin.x;
		mLookPoint3.z = (0f - ray3.origin.y) / ray3.direction.y * ray3.direction.z + ray3.origin.z;
		mLookPoint4.x = (0f - ray4.origin.y) / ray4.direction.y * ray4.direction.x + ray4.origin.x;
		mLookPoint4.z = (0f - ray4.origin.y) / ray4.direction.y * ray4.direction.z + ray4.origin.z;
	}

	private void ZoomCam(ZoomTypes zoomType)
	{
		switch (zoomType)
		{
		case ZoomTypes.ZOOM_IN:
			if (mCamPosVal + mCamZoomSpeed <= mMaxCamPosVal)
			{
				mCamPosVal += mCamZoomSpeed;
			}
			else
			{
				mCamPosVal = mMaxCamPosVal;
			}
			break;
		case ZoomTypes.ZOOM_OUT:
			if (mCamPosVal - mCamZoomSpeed >= mMinCamPosVal)
			{
				mCamPosVal -= mCamZoomSpeed;
			}
			else
			{
				mCamPosVal = mMinCamPosVal;
			}
			break;
		}
		CalculateZonePoints();
	}

	private void LookAtPoint()
	{
		base.camera.transform.position = mCamLookPoint + mCamPosDirection * mCamPosVal;
	}
}
