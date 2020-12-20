using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CentralSquareInput : MonoBehaviour
{
	private class CentralSquareObjectComparer : IComparer<RaycastHit>
	{
		private Vector3 camPos = Camera.main.transform.position;

		public int Compare(RaycastHit _obj1, RaycastHit _obj2)
		{
			int layer = _obj1.collider.gameObject.layer;
			int layer2 = _obj2.collider.gameObject.layer;
			int num = LayerMask.NameToLayer("Hero");
			if (layer == num && layer2 != num)
			{
				return -1;
			}
			if (layer != num && layer2 == num)
			{
				return 1;
			}
			float magnitude = (camPos - _obj1.point).magnitude;
			float magnitude2 = (camPos - _obj2.point).magnitude;
			if (magnitude < magnitude2)
			{
				return -1;
			}
			if (magnitude > magnitude2)
			{
				return 1;
			}
			return 0;
		}
	}

	public delegate void ShowPopUpMenuCallback(Player _player, Vector2 _pos);

	public delegate void PlayerClickedCallback(Player _player);

	private ShowPopUpMenuCallback mShowPopUpMenuCallback = delegate
	{
	};

	private PlayerControl mPlayerCtrl;

	private CommonInput mCommonInput;

	private ScreenManager mScreenMgr;

	private PlayerClickedCallback OnPlayerClicked = delegate
	{
	};

	private float mSendMoveTime;

	private float mMinDragDeltaPos = 2f;

	private Vector3 mLastDragMovePos = Vector3.zero;

	private bool mMoved;

	private ScreenManager.Cursor mCursorArrow;

	private ScreenManager.Cursor mCursorArrowEnemy;

	private ScreenManager.Cursor mCursorArrowFriend;

	private ScreenManager.Cursor mCursorSkill;

	private ScreenManager.Cursor mCursorSkillNegative;

	private ScreenManager.Cursor mCursorSkillNegativeBanned;

	private ScreenManager.Cursor mCursorSkillPositive;

	private ScreenManager.Cursor mCursorSkillPositiveBanned;

	private CursorType mLastCursorType;

	public void Init(ScreenManager _screenMgr, PlayerControl _playerCtrl)
	{
		Uninit();
		if (_playerCtrl == null)
		{
			throw new ArgumentNullException("_playerCtrl");
		}
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		mPlayerCtrl = _playerCtrl;
		mScreenMgr = _screenMgr;
		mCommonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (mCommonInput != null)
		{
			mCommonInput.SubscribeMouseClick(OnMouseClickLeft, OnMouseClickRight);
		}
		InitCursors();
		SetCursor(CursorType.ARROW);
		Selector[] array = UnityEngine.Object.FindObjectsOfType(typeof(Selector)) as Selector[];
		if (array != null)
		{
			Selector[] array2 = array;
			foreach (Selector selector in array2)
			{
				selector.mOnMouseEnter = (Selector.VoidCallback)Delegate.Combine(selector.mOnMouseEnter, new Selector.VoidCallback(OnObjectMouseEnter));
				selector.mOnMouseLeave = (Selector.VoidCallback)Delegate.Combine(selector.mOnMouseLeave, new Selector.VoidCallback(OnObjectMouseLeave));
			}
		}
	}

	public void Uninit()
	{
		mPlayerCtrl = null;
		if (mCommonInput != null)
		{
			mCommonInput.UnsubscribeMouseClick(OnMouseClickLeft, OnMouseClickRight);
			mCommonInput = null;
		}
		if (mScreenMgr != null)
		{
			if (mScreenMgr.WinCursorMgr != null)
			{
				mScreenMgr.WinCursorMgr.Use(0);
			}
			mScreenMgr.SetCursor(null);
			mScreenMgr = null;
		}
		Selector[] array = UnityEngine.Object.FindObjectsOfType(typeof(Selector)) as Selector[];
		if (array != null)
		{
			Selector[] array2 = array;
			foreach (Selector selector in array2)
			{
				selector.mOnMouseEnter = (Selector.VoidCallback)Delegate.Remove(selector.mOnMouseEnter, new Selector.VoidCallback(OnObjectMouseEnter));
				selector.mOnMouseLeave = (Selector.VoidCallback)Delegate.Remove(selector.mOnMouseLeave, new Selector.VoidCallback(OnObjectMouseLeave));
			}
		}
		mShowPopUpMenuCallback = delegate
		{
		};
		OnPlayerClicked = delegate
		{
		};
	}

	public void SubscribePopUpMenu(ShowPopUpMenuCallback _callback)
	{
		if (_callback != null)
		{
			mShowPopUpMenuCallback = (ShowPopUpMenuCallback)Delegate.Combine(mShowPopUpMenuCallback, _callback);
		}
	}

	public void SubscribePlayerClick(PlayerClickedCallback _callback)
	{
		if (_callback != null)
		{
			OnPlayerClicked = (PlayerClickedCallback)Delegate.Combine(OnPlayerClicked, _callback);
		}
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		Uninit();
	}

	private void OnObjectMouseEnter()
	{
		SetCursor(CursorType.USE);
	}

	private void OnObjectMouseLeave()
	{
		SetCursor(CursorType.ARROW);
	}

	private void OnMouseClickLeft(List<RaycastHit> _hits, EventType _type)
	{
		Vector3 _point = Vector3.zero;
		Player player = null;
		GameObject clickedObject = GetClickedObject(_hits, ref _point);
		if (clickedObject != null)
		{
			GameData component = clickedObject.GetComponent<GameData>();
			if (component != null)
			{
				player = component.Data.TryGetPlayer;
			}
		}
		OnPlayerClicked(player);
	}

	private void OnMouseClickRight(List<RaycastHit> _hits, EventType _type)
	{
		if (mPlayerCtrl == null)
		{
			return;
		}
		Vector3 _point = Vector3.zero;
		GameObject clickedObject = GetClickedObject(_hits, ref _point);
		switch (_type)
		{
		case EventType.MouseDown:
		{
			mSendMoveTime = 0f;
			GameData gameData = mPlayerCtrl.UpdateSelection(clickedObject);
			if (gameData != null)
			{
				TryShowPopUpMenu(gameData);
				break;
			}
			mMoved = true;
			mSendMoveTime = Time.time;
			mLastDragMovePos = _point;
			mPlayerCtrl.Move(_point);
			break;
		}
		case EventType.MouseUp:
			if (mMoved)
			{
				mMoved = false;
				mPlayerCtrl.Stop(_point, _stop: false, mSendMoveTime > 0f);
				mSendMoveTime = 0f;
				mLastDragMovePos = Vector3.zero;
			}
			break;
		case EventType.MouseDrag:
		{
			float magnitude = (mLastDragMovePos - _point).magnitude;
			if (magnitude > mMinDragDeltaPos)
			{
				mMoved = true;
				mSendMoveTime = 0f;
				mLastDragMovePos = _point;
				if (mLastDragMovePos != Vector3.zero)
				{
					mPlayerCtrl.Move(_point);
				}
			}
			break;
		}
		}
	}

	private GameObject GetClickedObject(List<RaycastHit> _hits, ref Vector3 _point)
	{
		if (_hits.Count == 0)
		{
			return null;
		}
		CentralSquareObjectComparer comparer = new CentralSquareObjectComparer();
		_hits.Sort(comparer);
		_point = _hits[0].point;
		return _hits[0].collider.gameObject;
	}

	private void TryShowPopUpMenu(GameData _gd)
	{
		if (_gd.Data.IsPlayerBinded)
		{
			mShowPopUpMenuCallback(_gd.Data.Player, Event.current.mousePosition);
		}
	}

	private void SetCursor(CursorType _cursorType)
	{
		if (mLastCursorType == _cursorType || mScreenMgr == null)
		{
			return;
		}
		mLastCursorType = _cursorType;
		if (mScreenMgr.WinCursorMgr != null)
		{
			mScreenMgr.WinCursorMgr.Use((int)_cursorType);
			return;
		}
		switch (_cursorType)
		{
		case CursorType.ARROW:
			mScreenMgr.SetCursor(mCursorArrow);
			break;
		case CursorType.ARROW_ENEMY:
			mScreenMgr.SetCursor(mCursorArrowEnemy);
			break;
		case CursorType.ARROW_FRIEND:
			mScreenMgr.SetCursor(mCursorArrowFriend);
			break;
		case CursorType.SKILL:
			mScreenMgr.SetCursor(mCursorSkill);
			break;
		case CursorType.SKILL_NEGATIVE:
			mScreenMgr.SetCursor(mCursorSkillNegative);
			break;
		case CursorType.SKILL_NEGATIVE_BANNED:
			mScreenMgr.SetCursor(mCursorSkillNegativeBanned);
			break;
		case CursorType.SKILL_POSITIVE:
			mScreenMgr.SetCursor(mCursorSkillPositive);
			break;
		case CursorType.SKILL_POSITIVE_BANNED:
			mScreenMgr.SetCursor(mCursorSkillPositiveBanned);
			break;
		}
	}

	private void InitCursors()
	{
		if (mScreenMgr.WinCursorMgr == null)
		{
			mCursorArrow = new ScreenManager.Cursor("arrow", -2f, -1f);
			mCursorArrowEnemy = new ScreenManager.Cursor("arrow_enemy", -2f, -1f);
			mCursorArrowFriend = new ScreenManager.Cursor("arrow_friend", -2f, -1f);
			mCursorSkill = new ScreenManager.Cursor("skill", -16f, -16f);
			mCursorSkillNegative = new ScreenManager.Cursor("skill_negative", -16f, -16f);
			mCursorSkillNegativeBanned = new ScreenManager.Cursor("skill_negative_banned", -16f, -16f);
			mCursorSkillPositive = new ScreenManager.Cursor("skill_positive", -16f, -16f);
			mCursorSkillPositiveBanned = new ScreenManager.Cursor("skill_positive_banned", -16f, -16f);
		}
	}
}
