using System;
using System.Collections.Generic;
using Network;

namespace TanatKernel
{
	public class AvatarAI
	{
		public enum Mode
		{
			AGRESSIVE,
			NEUTRAL,
			DEFENCE
		}

		private class Order
		{
			public virtual void Reset()
			{
			}
		}

		private class FriendFollowingTarget : Order
		{
			public int mFollowedFriendId;
		}

		private class DeferredAttackTarget : Order
		{
			public int mEnemyObjId;

			public bool mStarted;

			public Order mPostTarg;
		}

		private class PointTarget : Order
		{
			public float mAimPointX;

			public float mAimPointY;

			public bool mHaveSent;

			public override void Reset()
			{
				mHaveSent = false;
			}
		}

		private Mode mCurMode = Mode.NEUTRAL;

		private float mDeffenceX;

		private float mDeffenceY;

		private IGameObject mDeffenderTarget;

		private IGameObject mLastTarget;

		private bool mSyncPosition;

		private Dictionary<int, IGameObject> mAttackers = new Dictionary<int, IGameObject>();

		private Order mCurTarget;

		private SelfPlayer mSelfPlayer;

		private bool mIsInited;

		private Dictionary<int, SkillType> mSkillTypes = new Dictionary<int, SkillType>();

		private int mLastTargetId = -1;

		private DateTime mLastClear = DateTime.Now;

		private HandlerManager<BattlePacket, BattleCmdId> mHandlerMgr;

		private bool mSelfInvisibled;

		private Order mOldTarget;

		private List<int> mOrders = new List<int>();

		private List<int> mCurrentActions = new List<int>();

		public bool IsInited => mIsInited;

		public void Init(SelfPlayer _selfPlayer)
		{
			Uninit();
			if (_selfPlayer == null)
			{
				throw new ArgumentNullException("_selfPlayer");
			}
			mSelfPlayer = _selfPlayer;
			mIsInited = true;
		}

		public void Uninit()
		{
			AbortCurTarget();
			SetMode(Mode.NEUTRAL);
			mSelfPlayer = null;
			mIsInited = false;
		}

		public void SetDefencePosition(float _x, float _y)
		{
			mDeffenceX = _x;
			mDeffenceY = _y;
			mLastTarget = (mDeffenderTarget = null);
			if (!((DateTime.Now - mLastClear).TotalSeconds > 5.0))
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, IGameObject> mAttacker in mAttackers)
			{
				if (TargetToRemove(mAttacker.Value))
				{
					list.Add(mAttacker.Key);
				}
			}
			foreach (int item in list)
			{
				mAttackers.Remove(item);
			}
			mLastClear = DateTime.Now;
		}

		public void SetMode(Mode _mode)
		{
			if (_mode == Mode.DEFENCE)
			{
				if (mSelfInvisibled)
				{
					_mode = Mode.NEUTRAL;
				}
				mLastTarget = null;
				mOrders.Clear();
			}
			mCurMode = _mode;
		}

		private void InitTarget(Order _target)
		{
			mCurTarget = _target;
			if (mCurTarget != null)
			{
				mCurTarget.Reset();
			}
		}

		private IGameObject GetAvatar()
		{
			if (mSelfPlayer == null)
			{
				return null;
			}
			return mSelfPlayer.Player?.Avatar;
		}

		public IGameObject[] SearchEnemies(float _viewRadius)
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return new IGameObject[0];
			}
			float mAttackRange = avatar.Data.Params.mAttackRange;
			if (mAttackRange > _viewRadius)
			{
				_viewRadius = mAttackRange;
			}
			avatar.Data.GetPosition(out var _x, out var _y);
			SortedDictionary<double, IGameObject> sortedDictionary = new SortedDictionary<double, IGameObject>();
			GameObjectsProvider gameObjProv = avatar.Data.GameObjProv;
			foreach (IGameObject visibleObject in gameObjProv.VisibleObjects)
			{
				if (visibleObject.Proto.Destructible != null && visibleObject.Data.GetFriendliness() == Friendliness.ENEMY && visibleObject.Data.Params.Health != 0 && visibleObject.Data.Params.mPhysImm == 0)
				{
					visibleObject.Data.GetPosition(out var _x2, out var _y2);
					float num = _x2 - _x;
					float num2 = _y2 - _y;
					double num3 = Math.Sqrt(num * num + num2 * num2);
					if (!(num3 > (double)_viewRadius))
					{
						sortedDictionary[num3] = visibleObject;
					}
				}
			}
			if (sortedDictionary.Count == 0)
			{
				return new IGameObject[0];
			}
			List<IGameObject> list = new List<IGameObject>(sortedDictionary.Values);
			return list.ToArray();
		}

		public void SetTarget(IGameObject _friend)
		{
			AbortCurTarget();
			if (_friend == null)
			{
				throw new ArgumentNullException("_friend");
			}
			FriendFollowingTarget friendFollowingTarget = new FriendFollowingTarget();
			friendFollowingTarget.mFollowedFriendId = _friend.Id;
			InitTarget(friendFollowingTarget);
		}

		public bool SetTarget(float _x, float _y)
		{
			AbortCurTarget();
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return false;
			}
			PointTarget pointTarget = new PointTarget();
			pointTarget.mAimPointX = _x;
			pointTarget.mAimPointY = _y;
			TargetedAction action = avatar.Data.GetAction(avatar.Data.AttackActionId);
			if (action != null)
			{
				DeferredAttackTarget deferredAttackTarget = new DeferredAttackTarget();
				deferredAttackTarget.mEnemyObjId = action.TargetId;
				deferredAttackTarget.mPostTarg = pointTarget;
				deferredAttackTarget.mStarted = true;
				SetMode(Mode.NEUTRAL);
				InitTarget(deferredAttackTarget);
				return false;
			}
			InitTarget(pointTarget);
			return true;
		}

		public void AbortCurTarget()
		{
			InitTarget(null);
		}

		private bool TargetToRemove(IGameObject _go)
		{
			if (_go.Proto.Destructible == null)
			{
				return true;
			}
			if (_go.Data.Params.Health == 0)
			{
				return true;
			}
			if (!_go.Data.Visible)
			{
				return true;
			}
			return false;
		}

		private bool IsGoodTarget(IGameObject _go)
		{
			if (TargetToRemove(_go))
			{
				return false;
			}
			if (_go.Data.Params.mPhysImm != 0)
			{
				return false;
			}
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return false;
			}
			float mAttackRange = avatar.Data.Params.mAttackRange;
			_go.Data.GetPosition(out var _x, out var _y);
			float num = _x - mDeffenceX;
			float num2 = _y - mDeffenceY;
			double num3 = Math.Sqrt(num * num + num2 * num2);
			num3 -= (double)avatar.Data.Params.mRadius;
			num3 -= (double)_go.Data.Params.mRadius;
			if (num3 > (double)mAttackRange)
			{
				return false;
			}
			return true;
		}

		private bool NeedSyncPos()
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return false;
			}
			avatar.Data.GetPosition(out var _x, out var _y);
			float num = _x - mDeffenceX;
			float num2 = _y - mDeffenceY;
			double num3 = Math.Sqrt(num * num + num2 * num2);
			if (num3 > (double)avatar.Data.Params.mRadius)
			{
				return true;
			}
			return false;
		}

		private IGameObject FindTarget(IGameObject _current)
		{
			if (_current != null && IsGoodTarget(_current))
			{
				return _current;
			}
			List<int> list = new List<int>();
			IGameObject result = null;
			foreach (KeyValuePair<int, IGameObject> mAttacker in mAttackers)
			{
				if (IsGoodTarget(mAttacker.Value))
				{
					result = mAttacker.Value;
					break;
				}
				if (TargetToRemove(mAttacker.Value))
				{
					list.Add(mAttacker.Key);
				}
			}
			foreach (int item in list)
			{
				mAttackers.Remove(item);
			}
			return result;
		}

		public bool TryAutoAttack(float _viewRadius, float _waitAfterSkill)
		{
			if (!mIsInited)
			{
				return false;
			}
			IGameObject gameObject = null;
			if (gameObject == null)
			{
				gameObject = GetAvatar();
			}
			if (gameObject == null)
			{
				return false;
			}
			if (mCurMode == Mode.DEFENCE)
			{
				if ((double)gameObject.Data.mSpeed > 0.1)
				{
					return true;
				}
				if (mOrders.Count > 0)
				{
					return true;
				}
				bool flag = false;
				if (mCurrentActions.Count > 0)
				{
					Effector[] effectors = gameObject.Data.Effectors;
					foreach (Effector effector in effectors)
					{
						if (mCurrentActions.Contains(effector.Proto.Id))
						{
							flag = effector.Child == null || effector.Child.SkillType != SkillType.TOGGLE || flag;
						}
					}
				}
				if (flag)
				{
					return true;
				}
				if (mLastTarget != null && mLastTarget == mDeffenderTarget && IsGoodTarget(mDeffenderTarget))
				{
					return true;
				}
				if (NeedSyncPos() && mLastTarget != null)
				{
					if (!mSyncPosition)
					{
						mSelfPlayer.MoveAFK(mDeffenceX, mDeffenceY, _rel: false);
						mSyncPosition = true;
						return true;
					}
				}
				else
				{
					mSyncPosition = false;
				}
				mDeffenderTarget = FindTarget(mDeffenderTarget);
				if (mDeffenderTarget != null)
				{
					mSelfPlayer.AttackAFK(mDeffenderTarget);
					mLastTarget = mDeffenderTarget;
				}
				return true;
			}
			if (mCurMode != 0)
			{
				if (mCurTarget != null && mCurTarget is DeferredAttackTarget)
				{
					DeferredAttackTarget deferredAttackTarget = (DeferredAttackTarget)mCurTarget;
					gameObject = GetAvatar();
					if (gameObject == null)
					{
						return false;
					}
					if (gameObject.Data.GetActionWithTarget() != null)
					{
						return false;
					}
					IGameObject gameObject2 = gameObject.Data.GameObjProv.TryGet(deferredAttackTarget.mEnemyObjId);
					bool flag2 = false;
					if (gameObject2 == null || !gameObject2.Data.Visible || gameObject2.Data.Params.Health == 0)
					{
						SetMode(Mode.AGRESSIVE);
						ResetTarget(deferredAttackTarget);
					}
				}
				return false;
			}
			if (IsDoingAction(gameObject, _waitAfterSkill))
			{
				return false;
			}
			IGameObject[] array = SearchEnemies(_viewRadius);
			if (array.Length == 0)
			{
				return false;
			}
			int num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].Id == mLastTargetId)
				{
					num = j;
					break;
				}
			}
			mSelfPlayer.AttackAFK(array[num]);
			return true;
		}

		private bool IsDoingAction(IGameObject _selfAvatar, float _waitAfterSkill)
		{
			if (!_selfAvatar.Data.DoingAction)
			{
				return false;
			}
			if (_selfAvatar.Data.GetActionWithTarget() != null)
			{
				return true;
			}
			foreach (TargetedAction curAction in _selfAvatar.Data.CurActions)
			{
				if (!mSkillTypes.TryGetValue(curAction.ActionId, out var value))
				{
					Effector selfEffectorByProto = _selfAvatar.Data.GetSelfEffectorByProto(curAction.ActionId);
					if (selfEffectorByProto != null)
					{
						value = selfEffectorByProto.Child.Proto.EffectDesc.mDesc.mType;
						mSkillTypes.Add(curAction.ActionId, value);
					}
				}
				if (value == SkillType.ACTIVE)
				{
					return true;
				}
			}
			return false;
		}

		public void TryUpdateFollowing(float _waitAfterSkill)
		{
			if (!mIsInited || mCurTarget == null)
			{
				return;
			}
			if (mCurTarget is FriendFollowingTarget)
			{
				FriendFollowingTarget friendFollowingTarget = (FriendFollowingTarget)mCurTarget;
				IGameObject avatar = GetAvatar();
				if (avatar == null || IsDoingAction(avatar, _waitAfterSkill))
				{
					return;
				}
				IGameObject gameObject = avatar.Data.GameObjProv.TryGet(friendFollowingTarget.mFollowedFriendId);
				if (gameObject == null || gameObject.Data.GetFriendliness() != Friendliness.FRIEND)
				{
					AbortCurTarget();
					return;
				}
				gameObject.Data.GetPosition(out var _x, out var _y);
				MoveIfNotReached(avatar, _x, _y, 1f);
			}
			if (!(mCurTarget is PointTarget))
			{
				return;
			}
			PointTarget pointTarget = (PointTarget)mCurTarget;
			if (pointTarget.mHaveSent)
			{
				return;
			}
			IGameObject avatar2 = GetAvatar();
			if (avatar2 != null && !IsDoingAction(avatar2, _waitAfterSkill))
			{
				if (!MoveIfNotReached(avatar2, pointTarget.mAimPointX, pointTarget.mAimPointY, 1f))
				{
					AbortCurTarget();
				}
				else
				{
					pointTarget.mHaveSent = true;
				}
			}
		}

		private bool MoveIfNotReached(IGameObject _selfAvatar, float _targX, float _targY, float _eps)
		{
			_selfAvatar.Data.GetPosition(out var _x, out var _y);
			float num = _targX - _x;
			float num2 = _targY - _y;
			double num3 = Math.Sqrt(num * num + num2 * num2);
			if (num3 < (double)_eps)
			{
				return false;
			}
			mSelfPlayer.MoveAFK(_targX, _targY, _rel: false);
			return true;
		}

		private void ResetTarget(DeferredAttackTarget _targ)
		{
			Order mPostTarg = _targ.mPostTarg;
			AbortCurTarget();
			if (mPostTarg != null)
			{
				InitTarget(mPostTarg);
			}
		}

		public void Subscribe(HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			_handlerMgr.Subscribe<KillArg>(BattleCmdId.ON_KILL, null, null, OnKill);
			_handlerMgr.Subscribe<DoActionArg>(BattleCmdId.DO_ACTION, null, null, OnActionStarted);
			_handlerMgr.Subscribe<ActionArg>(BattleCmdId.ACTION, null, null, OnAction);
			_handlerMgr.Subscribe<ActionDoneArg>(BattleCmdId.ACTION_DONE, null, null, OnActionDone);
			_handlerMgr.Subscribe<SyncPacket>(BattleCmdId.SYNC, null, null, DistributeSync);
			_handlerMgr.Subscribe<OrderDoneArg>(BattleCmdId.ORDER_DONE, null, null, OnOrderDone);
			mHandlerMgr = _handlerMgr;
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		private void DistributeSync(SyncPacket _arg)
		{
			foreach (SyncData datum in _arg.Data)
			{
				if (datum.SyncType != SyncType.INVISIBLE_MODE_ENABLED)
				{
					continue;
				}
				IGameObject avatar = GetAvatar();
				if (avatar == null)
				{
					break;
				}
				if (datum.TrackingId != avatar.Id)
				{
					continue;
				}
				mSelfInvisibled = datum.GetValue<float>(0) > 0f;
				if (mSelfInvisibled)
				{
					if (mCurTarget != null && mCurTarget is DeferredAttackTarget)
					{
						DeferredAttackTarget targ = (DeferredAttackTarget)mCurTarget;
						SetMode(Mode.NEUTRAL);
						ResetTarget(targ);
					}
					else
					{
						AbortCurTarget();
					}
					SetMode(Mode.NEUTRAL);
					mCurrentActions.Clear();
					mAttackers.Clear();
				}
				else
				{
					SetMode(Mode.DEFENCE);
				}
			}
		}

		private void OnKill(KillArg _arg)
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return;
			}
			if (_arg.mVictimId == avatar.Id)
			{
				SetMode(Mode.NEUTRAL);
				mCurrentActions.Clear();
				mAttackers.Clear();
				mOrders.Clear();
				AbortCurTarget();
				mSelfInvisibled = false;
			}
			if (mCurTarget != null && _arg.mKillerId == avatar.Id && mCurTarget != null && mCurTarget is DeferredAttackTarget)
			{
				DeferredAttackTarget deferredAttackTarget = (DeferredAttackTarget)mCurTarget;
				if (deferredAttackTarget.mStarted)
				{
					SetMode(Mode.AGRESSIVE);
					ResetTarget(deferredAttackTarget);
				}
			}
		}

		private void OnActionStarted(DoActionArg _arg)
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return;
			}
			if (_arg.mActionId == avatar.Data.AttackActionId)
			{
				mSelfInvisibled = false;
				if (mCurMode != Mode.DEFENCE)
				{
					DeferredAttackTarget deferredAttackTarget = new DeferredAttackTarget();
					deferredAttackTarget.mEnemyObjId = _arg.mTargetId;
					if (mCurTarget != null)
					{
						if (mCurTarget is PointTarget || mCurTarget is FriendFollowingTarget)
						{
							deferredAttackTarget.mPostTarg = mCurTarget;
						}
						else if (mCurTarget is DeferredAttackTarget)
						{
							deferredAttackTarget.mPostTarg = ((DeferredAttackTarget)mCurTarget).mPostTarg;
						}
					}
					AbortCurTarget();
					InitTarget(deferredAttackTarget);
					SetMode(Mode.NEUTRAL);
				}
			}
			else if (!avatar.Data.IsInType(_arg.mActionId, SkillType.TOGGLE) && _arg.mActionId > 0)
			{
				mOrders.Add(_arg.mActionId);
			}
			mLastTargetId = _arg.mTargetId;
		}

		private void OnAction(ActionArg _arg)
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return;
			}
			if (mCurTarget != null && mCurTarget is DeferredAttackTarget)
			{
				DeferredAttackTarget deferredAttackTarget = (DeferredAttackTarget)mCurTarget;
				if (_arg.mObjId == avatar.Id && _arg.mActionId == avatar.Data.AttackActionId)
				{
					deferredAttackTarget.mStarted = true;
				}
			}
			if (_arg.mTargetId == avatar.Id && _arg.mActionId == avatar.Data.AttackActionId && !mAttackers.ContainsKey(_arg.mObjId))
			{
				IGameObject gameObject = avatar.Data.GameObjProv.TryGet(_arg.mObjId);
				if (gameObject != null && gameObject.Data.IsEnemy())
				{
					mAttackers[_arg.mObjId] = gameObject;
				}
			}
			if (_arg.mObjId == avatar.Id)
			{
				if (!mCurrentActions.Contains(_arg.mActionId))
				{
					mCurrentActions.Add(_arg.mActionId);
				}
				bool flag = avatar.Data.IsInType(_arg.mActionId, SkillType.TOGGLE);
				if ((avatar.Data.IsInType(_arg.mActionId, SkillType.SKILL, SkillType.ACTIVE) || _arg.mItem) && !flag)
				{
					mOldTarget = mCurTarget;
					AbortCurTarget();
				}
				if (flag && mCurTarget is PointTarget)
				{
					PointTarget pointTarget = (PointTarget)mCurTarget;
					pointTarget.mHaveSent = false;
				}
			}
		}

		private void OnActionDone(ActionDoneArg _arg)
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return;
			}
			if (mCurTarget != null && mCurTarget is DeferredAttackTarget)
			{
				DeferredAttackTarget deferredAttackTarget = (DeferredAttackTarget)mCurTarget;
				if (!deferredAttackTarget.mStarted)
				{
					return;
				}
				if (_arg.mObjId == avatar.Id && _arg.mActionId == avatar.Data.AttackActionId)
				{
					IGameObject gameObject = avatar.Data.GameObjProv.TryGet(deferredAttackTarget.mEnemyObjId);
					if (gameObject != null && gameObject.Data.Visible)
					{
						return;
					}
					SetMode(Mode.AGRESSIVE);
					ResetTarget(deferredAttackTarget);
				}
			}
			if (_arg.mObjId == avatar.Id)
			{
				if (mCurrentActions.Contains(_arg.mActionId))
				{
					mCurrentActions.Remove(_arg.mActionId);
				}
				if ((avatar.Data.IsInType(_arg.mActionId, SkillType.SKILL, SkillType.ACTIVE) || _arg.mItem) && mCurTarget == null)
				{
					mCurTarget = mOldTarget;
				}
				if (avatar.Data.IsInType(_arg.mActionId, SkillType.TOGGLE) && mCurTarget is PointTarget)
				{
					PointTarget pointTarget = (PointTarget)mCurTarget;
					pointTarget.mHaveSent = false;
				}
			}
		}

		private void OnOrderDone(OrderDoneArg _arg)
		{
			if (!mOrders.Contains(_arg.mActionId))
			{
				return;
			}
			mOrders.Remove(_arg.mActionId);
			if (mOrders.Count == 0)
			{
				IGameObject avatar = GetAvatar();
				if (avatar != null)
				{
					avatar.Data.GetPosition(out var _x, out var _y);
					SetDefencePosition(_x, _y);
				}
			}
		}

		public bool IsAttacking()
		{
			IGameObject avatar = GetAvatar();
			if (avatar == null)
			{
				return false;
			}
			int attackActionId = avatar.Data.AttackActionId;
			return avatar.Data.IsDoingAction(attackActionId);
		}
	}
}
