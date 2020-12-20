using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Map Input/CommonInput")]
public class CommonInput : MonoBehaviour, ScreenManager.IEventListener
{
	public delegate void MouseClickCallback(List<RaycastHit> _objects, EventType _clickType);

	public RenderingPath mRenderingPath = RenderingPath.Forward;

	public bool mCameraRotation;

	public float mMapSize;

	public bool mCacheTerrainHeight;

	public bool mAvatarLight;

	public float mUpperBound;

	private MouseClickCallback mOnMouseClickLeft;

	private MouseClickCallback mOnMouseClickRight;

	private ScreenManager mScreenMgr;

	private static int mHitMask;

	private float mMinDeltaMousePos = 8f;

	private Vector2 mLastMouseClickPos = Vector2.zero;

	public void Init(ScreenManager _screenMgr)
	{
		Uninit();
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		mScreenMgr = _screenMgr;
		mScreenMgr.AddEventListener(this);
	}

	public void Uninit()
	{
		mOnMouseClickLeft = null;
		mOnMouseClickRight = null;
		if (mScreenMgr != null)
		{
			mScreenMgr.RemoveEventListener(this);
			mScreenMgr = null;
		}
	}

	public void SubscribeMouseClick(MouseClickCallback _left, MouseClickCallback _right)
	{
		mOnMouseClickLeft = (MouseClickCallback)Delegate.Combine(mOnMouseClickLeft, _left);
		mOnMouseClickRight = (MouseClickCallback)Delegate.Combine(mOnMouseClickRight, _right);
	}

	public void UnsubscribeMouseClick(MouseClickCallback _left, MouseClickCallback _right)
	{
		mOnMouseClickLeft = (MouseClickCallback)Delegate.Remove(mOnMouseClickLeft, _left);
		mOnMouseClickRight = (MouseClickCallback)Delegate.Remove(mOnMouseClickRight, _right);
	}

	public void OnEnable()
	{
		if (Camera.main != null)
		{
			Camera.main.renderingPath = mRenderingPath;
			GameCamera component = Camera.main.GetComponent<GameCamera>();
			if (component != null)
			{
				component.Init(mCameraRotation);
			}
		}
		mHitMask = 0;
		GameObjUtil.InitLayerMask("Ignore Raycast", ref mHitMask);
		GameObjUtil.InitLayerMask("Fx", ref mHitMask);
		GameObjUtil.InitLayerMask("box collider", ref mHitMask);
		mHitMask = 0x7FFFFFFF ^ mHitMask;
	}

	public void OnDisable()
	{
		SoundSystem instance = SoundSystem.Instance;
		if (instance != null)
		{
			instance.StopAllMusic();
		}
		Uninit();
	}

	public void CheckEvents(Event _curEvent)
	{
		if (Camera.main == null)
		{
			return;
		}
		if (_curEvent.type == EventType.Used)
		{
			if (_curEvent.button == 1)
			{
				List<RaycastHit> hit = GetHit();
				mLastMouseClickPos = _curEvent.mousePosition;
				if (mOnMouseClickRight != null)
				{
					mOnMouseClickRight(hit, EventType.MouseUp);
				}
			}
		}
		else if (_curEvent.type == EventType.MouseDown || _curEvent.type == EventType.MouseUp)
		{
			List<RaycastHit> hit = GetHit();
			if (_curEvent.button == 0)
			{
				if (mOnMouseClickLeft != null)
				{
					mOnMouseClickLeft(hit, _curEvent.type);
				}
			}
			else if (_curEvent.button == 1)
			{
				mLastMouseClickPos = _curEvent.mousePosition;
				if (mOnMouseClickRight != null)
				{
					mOnMouseClickRight(hit, _curEvent.type);
				}
			}
		}
		else
		{
			if (_curEvent.type != EventType.MouseDrag || _curEvent.button != 1)
			{
				return;
			}
			float magnitude = (mLastMouseClickPos - _curEvent.mousePosition).magnitude;
			if (magnitude >= mMinDeltaMousePos)
			{
				List<RaycastHit> hit = GetHit();
				mLastMouseClickPos = _curEvent.mousePosition;
				if (mOnMouseClickRight != null)
				{
					mOnMouseClickRight(hit, _curEvent.type);
				}
			}
		}
	}

	private List<RaycastHit> GetHit()
	{
		List<RaycastHit> _hits = new List<RaycastHit>();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		SetAllRaycastHits(ref _hits, ray, mHitMask);
		return _hits;
	}

	public static void SetAllRaycastHits(ref List<RaycastHit> _hits, Ray _ray, int _mask)
	{
		RaycastHit[] array = Physics.RaycastAll(_ray, 1024f, _mask);
		RaycastHit[] array2 = array;
		foreach (RaycastHit item in array2)
		{
			_hits.Add(item);
		}
	}
}
