using System;
using System.Collections.Generic;
using Network;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("Map Input/BattleInput")]
public class BattleInput : MonoBehaviour
{
	public enum AfkState
	{
		UNINITED,
		ALLOWABLE,
		WARN,
		KICK
	}

	private enum CompareType
	{
		NONE,
		NO_PRIORITY,
		ENEMY_PRIORITY
	}

	private class BattleObjectComparer : IComparer<RaycastHit>
	{
		private Vector3 camPos = Camera.main.transform.position;

		private CompareType mCompareType;

		public BattleObjectComparer(CompareType _type)
		{
			mCompareType = _type;
		}

		public int Compare(RaycastHit _obj1, RaycastHit _obj2)
		{
			if (mCompareType == CompareType.NONE)
			{
				return 0;
			}
			int layer = _obj1.collider.gameObject.layer;
			int layer2 = _obj2.collider.gameObject.layer;
			int num = LayerMask.NameToLayer("Character");
			int num2 = LayerMask.NameToLayer("Building");
			if (layer == num && layer2 != num)
			{
				return -1;
			}
			if (layer != num && layer2 == num)
			{
				return 1;
			}
			if (layer == num2 && layer2 != num2)
			{
				return -1;
			}
			if (layer != num2 && layer2 == num2)
			{
				return 1;
			}
			if (layer == layer2 && layer == num)
			{
				GameData component = _obj1.collider.gameObject.GetComponent<GameData>();
				GameData component2 = _obj2.collider.gameObject.GetComponent<GameData>();
				bool flag = component.Proto != null && component.Proto.Avatar != null;
				bool flag2 = component2.Proto != null && component2.Proto.Avatar != null;
				if (mCompareType == CompareType.ENEMY_PRIORITY)
				{
					bool flag3 = component.Data.IsPlayerBinded && component.Data.Player.IsSelf;
					bool flag4 = component2.Data.IsPlayerBinded && component2.Data.Player.IsSelf;
					Friendliness friendliness = component.Data.GetFriendliness();
					Friendliness friendliness2 = component2.Data.GetFriendliness();
					if (flag3 && !flag4)
					{
						return 1;
					}
					if (!flag3 && flag4)
					{
						return -1;
					}
					if (flag && !flag2)
					{
						return -1;
					}
					if (!flag && flag2)
					{
						return 1;
					}
					if (friendliness == Friendliness.ENEMY && friendliness2 != Friendliness.ENEMY)
					{
						return -1;
					}
					if (friendliness != Friendliness.ENEMY && friendliness2 == Friendliness.ENEMY)
					{
						return 1;
					}
				}
				else if (mCompareType == CompareType.NO_PRIORITY)
				{
					if (flag && !flag2)
					{
						return -1;
					}
					if (!flag && flag2)
					{
						return 1;
					}
				}
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

	public delegate void AfkCallback(AfkState _state);

	public AfkCallback mAfkCallback;

	private float mBeaconTimeLim = 0.2f;

	private float mPrevBeaconSetTime;

	private int mBeaconGroupCount = 3;

	private int mBeaconGroupCurrent;

	private float mBeaconGroupTimeLim = 10f;

	private float mPrevBeaconGroupSetTime;

	private float mAfkWarnTime;

	private float mAfkKickTime;

	private AfkState mCurAfkState;

	private ScreenManager mScreenMgr;

	private PlayerControl mPlayerCtrl;

	private MiniMap mMiniMap;

	private SkillZone mSkillZone;

	private SkillZone mSkillZoneLine;

	private CommonInput mCommonInput;

	private float mAvatarAiUpdateRate;

	private float mAvatarAiViewRadius;

	private float mAvatarAiWaitAfterSkill = 3f;

	private float mPrevAiUpdateTime;

	private AvatarAI mAvatarAi;

	private float mSendMoveTime;

	private float mMinDragDeltaPos = 3f;

	private Vector3 mLastDragMovePos = Vector3.zero;

	private bool mMoved;

	private bool mAttackStarted;

	private GameObject mCurrentObject;

	private ScreenManager.Cursor mCursorArrow;

	private ScreenManager.Cursor mCursorArrowEnemy;

	private ScreenManager.Cursor mCursorArrowFriend;

	private ScreenManager.Cursor mCursorSkill;

	private ScreenManager.Cursor mCursorSkillNegative;

	private ScreenManager.Cursor mCursorSkillNegativeBanned;

	private ScreenManager.Cursor mCursorSkillPositive;

	private ScreenManager.Cursor mCursorSkillPositiveBanned;

	private CursorType mLastCursorType;

	public void Init(ScreenManager _screenMgr, PlayerControl _playerCtrl, MiniMap _miniMap)
	{
		Uninit();
		if (_screenMgr == null)
		{
			throw new ArgumentNullException("_screenMgr");
		}
		if (_playerCtrl == null)
		{
			throw new ArgumentNullException("_playerCtrl");
		}
		if (_miniMap == null)
		{
			throw new ArgumentNullException("_miniMap");
		}
		mScreenMgr = _screenMgr;
		mPlayerCtrl = _playerCtrl;
		mMiniMap = _miniMap;
		mSkillZone = new SkillZone();
		mSkillZone.Init("VFX_SkillZone");
		mSkillZoneLine = new SkillZone();
		mSkillZoneLine.Init("VFX_SkillZoneLine");
		mPlayerCtrl.SetSkillZone(mSkillZone);
		mPlayerCtrl.SetSkillZoneLine(mSkillZoneLine);
		mCommonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (mCommonInput != null)
		{
			mCommonInput.SubscribeMouseClick(OnMouseClickLeft, OnMouseClickRight);
		}
		MiniMap miniMap = mMiniMap;
		miniMap.mOnClick = (MiniMap.OnMiniMapClick)Delegate.Combine(miniMap.mOnClick, new MiniMap.OnMiniMapClick(OnMiniMapClick));
		InitCursors();
		SetCursor(CursorType.ARROW);
	}

	public void InitAfk(float _kickTime, float _warnTime)
	{
		mAfkKickTime = _kickTime;
		mAfkWarnTime = _warnTime;
		mCurAfkState = AfkState.ALLOWABLE;
	}

	public void InitAvatarAi(float _updateRate, float _viewRadius, HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
	{
		if (_handlerMgr == null)
		{
			throw new ArgumentNullException("_handlerMgr");
		}
		if (mAvatarAi != null)
		{
			throw new InvalidOperationException("avatar ai already inited");
		}
		mAvatarAiUpdateRate = _updateRate;
		mAvatarAiViewRadius = _viewRadius;
		mAvatarAi = new AvatarAI();
		mAvatarAi.Subscribe(_handlerMgr);
	}

	public void Uninit()
	{
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
		if (mSkillZone != null)
		{
			mSkillZone.Destroy();
			mSkillZone = null;
		}
		if (mSkillZoneLine != null)
		{
			mSkillZoneLine.Destroy();
			mSkillZoneLine = null;
		}
		if (mAvatarAi != null)
		{
			mAvatarAi.Uninit();
			mAvatarAi.Unsubscribe();
			mAvatarAi = null;
		}
		if (mMiniMap != null)
		{
			MiniMap miniMap = mMiniMap;
			miniMap.mOnClick = (MiniMap.OnMiniMapClick)Delegate.Remove(miniMap.mOnClick, new MiniMap.OnMiniMapClick(OnMiniMapClick));
			mMiniMap = null;
		}
		mCurAfkState = AfkState.UNINITED;
		mAfkCallback = null;
		mPlayerCtrl = null;
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		Uninit();
	}

	private void OnMouseClickLeft(List<RaycastHit> _hits, EventType _type)
	{
		if (mPlayerCtrl == null || _type != 0)
		{
			return;
		}
		Vector3 _point = Vector3.zero;
		GameObject clickedObject = GetClickedObject(_hits, ref _point, CompareType.NO_PRIORITY);
		if (Event.current.alt || mMiniMap.ForceBeaconClick)
		{
			TrySetBeacon(_point.x, _point.z);
			mMiniMap.ForceBeaconClick = false;
		}
		GameData gameData = null;
		gameData = mPlayerCtrl.UpdateSelection(clickedObject);
		if (mPlayerCtrl.HasActiveAbility)
		{
			mPlayerCtrl.TryUseActiveAbility(_point.x, _point.z);
		}
		else if (mPlayerCtrl.HasSelectedObj && mCurrentObject != clickedObject)
		{
			if (mPlayerCtrl.ForceAttack(gameData))
			{
				mCurrentObject = clickedObject;
			}
		}
		else if (mPlayerCtrl.ForceAttackMode)
		{
			if (mAvatarAi.SetTarget(_point.x, _point.z))
			{
				mAvatarAi.SetMode(AvatarAI.Mode.AGRESSIVE);
			}
			mPlayerCtrl.RemoveForceAttack();
			mPrevAiUpdateTime = 0f;
			mCurrentObject = null;
		}
	}

	private void OnMouseClickRight(List<RaycastHit> _hits, EventType _type)
	{
		if (mPlayerCtrl == null || mPlayerCtrl.RemoveActiveAbility() || mPlayerCtrl.RemoveForceAttack())
		{
			return;
		}
		bool flag = false;
		Vector3 _point = Vector3.zero;
		GameObject clickedObject = GetClickedObject(_hits, ref _point, CompareType.ENEMY_PRIORITY);
		switch (_type)
		{
		case EventType.MouseDown:
		{
			mSendMoveTime = 0f;
			GameData gameData = mPlayerCtrl.UpdateSelection(clickedObject);
			if (gameData != null)
			{
				if (mCurrentObject != null)
				{
					GameData component = mCurrentObject.GetComponent<GameData>();
					flag = component.Id != gameData.Id;
				}
				mAvatarAi.SetMode(AvatarAI.Mode.NEUTRAL);
				if (flag || !mAvatarAi.IsAttacking())
				{
					mAttackStarted = mPlayerCtrl.Attack(gameData);
					mCurrentObject = clickedObject;
					if (!mAttackStarted && gameData.Data.GetFriendliness() == Friendliness.FRIEND)
					{
						mAvatarAi.SetTarget(gameData);
					}
				}
			}
			else
			{
				mAvatarAi.SetMode(AvatarAI.Mode.DEFENCE);
				mAvatarAi.AbortCurTarget();
				mAvatarAi.SetDefencePosition(_point.x, _point.z);
				mMoved = true;
				mAttackStarted = false;
				mSendMoveTime = Time.time;
				mLastDragMovePos = _point;
				mPlayerCtrl.Move(_point);
				mCurrentObject = null;
			}
			break;
		}
		case EventType.MouseUp:
			if (mMoved && !mAttackStarted)
			{
				mMoved = false;
				mPlayerCtrl.Stop(_point, _stop: false, mSendMoveTime > 0f);
				mSendMoveTime = 0f;
				mLastDragMovePos = Vector3.zero;
				mAvatarAi.SetMode(AvatarAI.Mode.DEFENCE);
				mAvatarAi.AbortCurTarget();
				mAvatarAi.SetDefencePosition(_point.x, _point.z);
			}
			break;
		case EventType.MouseDrag:
		{
			float magnitude = (mLastDragMovePos - _point).magnitude;
			if (magnitude > mMinDragDeltaPos && !mAttackStarted)
			{
				mMoved = true;
				mSendMoveTime = 0f;
				mLastDragMovePos = _point;
				if (mLastDragMovePos != Vector3.zero)
				{
					mPlayerCtrl.Move(_point);
					mAvatarAi.SetMode(AvatarAI.Mode.NEUTRAL);
					mAvatarAi.AbortCurTarget();
				}
			}
			break;
		}
		}
	}

	private GameObject GetClickedObject(List<RaycastHit> _hits, ref Vector3 _point, CompareType _type)
	{
		if (_hits.Count == 0)
		{
			return null;
		}
		BattleObjectComparer comparer = new BattleObjectComparer(_type);
		_hits.Sort(comparer);
		_point = _hits[0].point;
		return _hits[0].collider.gameObject;
	}

	private void OnMiniMapClick(int _btnNum, Vector2 _pos)
	{
		switch (_btnNum)
		{
		case 0:
			if (Event.current.alt || mMiniMap.ForceBeaconClick)
			{
				TrySetBeacon(_pos.x, _pos.y);
				mMiniMap.ForceBeaconClick = false;
				break;
			}
			if (mPlayerCtrl.ForceAttackMode)
			{
				if (mAvatarAi.SetTarget(_pos.x, _pos.y))
				{
					mAvatarAi.SetMode(AvatarAI.Mode.AGRESSIVE);
				}
				mPlayerCtrl.RemoveForceAttack();
			}
			if (mPlayerCtrl.HasActiveAbility)
			{
				mPlayerCtrl.TryUseActiveAbilityByPoint(_pos.x, _pos.y);
			}
			break;
		case 1:
			mPlayerCtrl.SelfPlayer.Move(_pos.x, _pos.y, _rel: false);
			mAvatarAi.SetMode(AvatarAI.Mode.DEFENCE);
			mAvatarAi.AbortCurTarget();
			mAvatarAi.SetDefencePosition(_pos.x, _pos.y);
			break;
		}
	}

	private void TrySetBeacon(float _x, float _y)
	{
		float num = Time.realtimeSinceStartup - mPrevBeaconGroupSetTime;
		if (!(num > mBeaconGroupTimeLim))
		{
			return;
		}
		float num2 = Time.realtimeSinceStartup - mPrevBeaconSetTime;
		if (num2 > mBeaconTimeLim)
		{
			mPlayerCtrl.SelfPlayer.SetBeacon(_x, _y);
			mPrevBeaconSetTime = Time.realtimeSinceStartup;
			mBeaconGroupCurrent++;
			if (mBeaconGroupCurrent >= mBeaconGroupCount)
			{
				mBeaconGroupCurrent = 0;
				mPrevBeaconSetTime = Time.realtimeSinceStartup + mBeaconGroupTimeLim;
			}
		}
	}

	public void Update()
	{
		if (mPlayerCtrl != null && mPlayerCtrl.SelfPlayer != null)
		{
			CursorType battleCursorType = mPlayerCtrl.GetBattleCursorType();
			SetCursor(battleCursorType);
			if (!mAvatarAi.IsInited)
			{
				mAvatarAi.Init(mPlayerCtrl.SelfPlayer);
			}
			TimeSpan timeSpan = DateTime.Now - mPlayerCtrl.SelfPlayer.LastActionTime;
			switch (mCurAfkState)
			{
			case AfkState.ALLOWABLE:
				if (timeSpan.TotalSeconds > (double)(mAfkKickTime - mAfkWarnTime))
				{
					SetAfkState(AfkState.WARN);
				}
				break;
			case AfkState.WARN:
				if (timeSpan.TotalSeconds > (double)mAfkKickTime)
				{
					SetAfkState(AfkState.KICK);
				}
				else if (timeSpan.TotalSeconds < (double)(mAfkKickTime - mAfkWarnTime))
				{
					SetAfkState(AfkState.ALLOWABLE);
				}
				break;
			}
		}
		if (mSkillZone != null)
		{
			mSkillZone.UpdatePosition();
		}
		if (mSkillZoneLine != null)
		{
			mSkillZoneLine.UpdatePosition();
		}
		if (mAvatarAi != null && Time.realtimeSinceStartup - mPrevAiUpdateTime > mAvatarAiUpdateRate)
		{
			mPrevAiUpdateTime = Time.realtimeSinceStartup;
			if (!mAvatarAi.TryAutoAttack(mAvatarAiViewRadius, mAvatarAiWaitAfterSkill))
			{
				mAvatarAi.TryUpdateFollowing(mAvatarAiWaitAfterSkill);
			}
		}
	}

	private void SetAfkState(AfkState _state)
	{
		if (mCurAfkState != _state)
		{
			mCurAfkState = _state;
			if (mAfkCallback != null)
			{
				mAfkCallback(mCurAfkState);
			}
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
