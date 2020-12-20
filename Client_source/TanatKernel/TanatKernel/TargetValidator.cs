using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class TargetValidator
	{
		private delegate bool CheckFunc(IGameObject _target);

		private Dictionary<SkillTarget, CheckFunc> mCheckFuncs = new Dictionary<SkillTarget, CheckFunc>();

		public TargetValidator()
		{
			mCheckFuncs[SkillTarget.FRIEND] = CheckFriend;
			mCheckFuncs[SkillTarget.NOT_FRIEND] = CheckNotFriend;
			mCheckFuncs[SkillTarget.ENEMY] = CheckEnemy;
			mCheckFuncs[SkillTarget.NOT_ENEMY] = CheckNotEnemy;
			mCheckFuncs[SkillTarget.BUILDING] = CheckBuilding;
			mCheckFuncs[SkillTarget.NOT_BUILDING] = CheckNotBuilding;
			mCheckFuncs[SkillTarget.OBJECT] = CheckObject;
			mCheckFuncs[SkillTarget.NOT_OBJECT] = CheckNotObject;
			mCheckFuncs[SkillTarget.SELF] = CheckSelf;
			mCheckFuncs[SkillTarget.NOT_SELF] = CheckNotSelf;
		}

		public bool IsValidTarget(int _mask, IGameObject _target)
		{
			foreach (SkillTarget value2 in Enum.GetValues(typeof(SkillTarget)))
			{
				int num = (int)value2;
				if ((num & _mask) != 0 && mCheckFuncs.TryGetValue(value2, out var value) && !value(_target))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsPointTarget(int _mask)
		{
			return (_mask & 0x100) != 0;
		}

		public static bool IsNoneTarget(int _mask)
		{
			return _mask == 0;
		}

		private bool CheckFriend(IGameObject _target)
		{
			return _target?.Data.IsFriend() ?? false;
		}

		private bool CheckNotFriend(IGameObject _target)
		{
			if (_target == null)
			{
				return false;
			}
			Friendliness friendliness = _target.Data.GetFriendliness();
			if (friendliness == Friendliness.UNKNOWN)
			{
				return false;
			}
			return friendliness != Friendliness.FRIEND;
		}

		private bool CheckEnemy(IGameObject _target)
		{
			return _target?.Data.IsEnemy() ?? false;
		}

		private bool CheckNotEnemy(IGameObject _target)
		{
			if (_target == null)
			{
				return false;
			}
			Friendliness friendliness = _target.Data.GetFriendliness();
			if (friendliness == Friendliness.UNKNOWN)
			{
				return false;
			}
			return friendliness != Friendliness.ENEMY;
		}

		private bool CheckBuilding(IGameObject _target)
		{
			if (_target != null)
			{
				return _target.Proto.Building != null;
			}
			return false;
		}

		private bool CheckNotBuilding(IGameObject _target)
		{
			if (_target != null)
			{
				return _target.Proto.Building == null;
			}
			return false;
		}

		private bool CheckObject(IGameObject _target)
		{
			return _target != null;
		}

		private bool CheckNotObject(IGameObject _target)
		{
			return _target == null;
		}

		private bool CheckSelf(IGameObject _target)
		{
			if (_target == null)
			{
				return false;
			}
			if (!_target.Data.IsPlayerBinded)
			{
				return false;
			}
			return _target.Data.Player.IsSelf;
		}

		private bool CheckNotSelf(IGameObject _target)
		{
			if (_target == null)
			{
				return false;
			}
			if (!_target.Data.IsPlayerBinded)
			{
				return true;
			}
			return !_target.Data.Player.IsSelf;
		}
	}
}
