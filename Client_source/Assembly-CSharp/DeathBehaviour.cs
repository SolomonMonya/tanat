using System;
using UnityEngine;

[AddComponentMenu("Death Behaviour/DeathBehaviour")]
public abstract class DeathBehaviour : MonoBehaviour
{
	private bool mDisableMode;

	private bool mDone = true;

	private GameObjManager mGameObjMgr;

	public void Init(GameObjManager _gameObjMgr)
	{
		if (_gameObjMgr == null)
		{
			throw new ArgumentNullException("_gameObjMgr");
		}
		mGameObjMgr = _gameObjMgr;
	}

	public bool IsDone()
	{
		return mDone;
	}

	public virtual void Reborn()
	{
		mDone = true;
	}

	protected abstract void StartDie();

	public void Die()
	{
		mDone = false;
		if (VisibilityMgr.IsVisible(base.gameObject))
		{
			StartDie();
		}
		else
		{
			Done();
		}
	}

	protected virtual void Done()
	{
		mDone = true;
		if (mGameObjMgr != null)
		{
			if (mDisableMode)
			{
				mGameObjMgr.TryDisable(base.gameObject);
			}
			else
			{
				mGameObjMgr.TryDelete(base.gameObject);
			}
		}
		else if (mDisableMode)
		{
			base.gameObject.SetActiveRecursively(state: false);
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public virtual bool IsForceStart()
	{
		return false;
	}

	public virtual bool EffectsDone()
	{
		bool result = true;
		VisualEffectHolder[] componentsInChildren = base.gameObject.GetComponentsInChildren<VisualEffectHolder>();
		VisualEffectHolder[] array = componentsInChildren;
		foreach (VisualEffectHolder visualEffectHolder in array)
		{
			if (visualEffectHolder.mWaitDeath)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool IsDisableMode()
	{
		return mDisableMode;
	}

	public static bool IsDone(GameObject _obj)
	{
		DeathBehaviour[] componentsInChildren = _obj.GetComponentsInChildren<DeathBehaviour>();
		DeathBehaviour[] array = componentsInChildren;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			if (!deathBehaviour.IsDone())
			{
				return false;
			}
		}
		return true;
	}

	public static void Reborn(GameObject _obj)
	{
		DeathBehaviour[] componentsInChildren = _obj.GetComponentsInChildren<DeathBehaviour>();
		DeathBehaviour[] array = componentsInChildren;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			deathBehaviour.Reborn();
		}
	}

	public static void Die(GameObject _obj)
	{
		DeathBehaviour[] componentsInChildren = _obj.GetComponentsInChildren<DeathBehaviour>();
		DeathBehaviour[] array = componentsInChildren;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			deathBehaviour.Die();
		}
	}

	public static void DieSelectively(GameObject _obj)
	{
		DeathBehaviour[] componentsInChildren = _obj.GetComponentsInChildren<DeathBehaviour>();
		bool flag = false;
		DeathBehaviour[] array = componentsInChildren;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			if (deathBehaviour.IsForceStart())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Die(_obj);
		}
	}

	public static void SetMode(GameObject _obj, bool _disableMode)
	{
		DeathBehaviour[] componentsInChildren = _obj.GetComponentsInChildren<DeathBehaviour>();
		DeathBehaviour[] array = componentsInChildren;
		foreach (DeathBehaviour deathBehaviour in array)
		{
			deathBehaviour.mDisableMode = _disableMode;
		}
	}
}
