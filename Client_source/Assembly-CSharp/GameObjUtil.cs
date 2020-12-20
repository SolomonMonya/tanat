using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public static class GameObjUtil
{
	public class CreateObjsTask : TaskQueue.Task
	{
		private List<string> mObjsToCreate;

		public CreateObjsTask(IEnumerable<string> _objNames, Notifier<TaskQueue.ITask, object> _notifier)
			: base(_notifier)
		{
			mObjsToCreate = new List<string>(_objNames);
		}

		public override void Begin()
		{
			foreach (string item in mObjsToCreate)
			{
				if (GetGameObject(item) == null)
				{
					new GameObject(item);
				}
			}
		}
	}

	public class DeleteObjsTask : TaskQueue.Task
	{
		private List<string> mObjsToDelete;

		public DeleteObjsTask(IEnumerable<string> _objNames, Notifier<TaskQueue.ITask, object> _notifier)
			: base(_notifier)
		{
			mObjsToDelete = new List<string>(_objNames);
		}

		public override void Begin()
		{
			foreach (string item in mObjsToDelete)
			{
				DestroyGameObject(item);
			}
		}
	}

	public static GameObject GetGameObject(string _name)
	{
		return GameObject.Find(_name);
	}

	public static void DestroyGameObject(string _name)
	{
		GameObject _go = GetGameObject(_name);
		DestroyGameObject(ref _go);
	}

	public static void DestroyGameObject(ref GameObject _go)
	{
		DestroyGameObject(_go);
		_go = null;
	}

	public static void DestroyGameObject(GameObject _go)
	{
		if (!(_go == null))
		{
			_go.transform.parent = null;
			_go.name += "_deleted";
			_go.SetActiveRecursively(state: false);
			Object.Destroy(_go);
		}
	}

	public static bool TrySetParent(GameObject _go, string _parentName)
	{
		if (null == _go)
		{
			return false;
		}
		GameObject gameObject = GetGameObject(_parentName);
		if (null == gameObject)
		{
			return false;
		}
		_go.transform.parent = gameObject.transform;
		return true;
	}

	public static T FindObjectOfType<T>() where T : Component
	{
		T val = TryFindObjectOfType<T>();
		if ((Object)val == (Object)null)
		{
			Log.Warning("cannot find object of type " + typeof(T).FullName);
		}
		return val;
	}

	public static T TryFindObjectOfType<T>() where T : Component
	{
		return Object.FindObjectOfType(typeof(T)) as T;
	}

	public static T GetComponentInChildren<T>(GameObject _go) where T : Component
	{
		if (_go == null)
		{
			return (T)null;
		}
		T component = _go.GetComponent<T>();
		if ((Object)component != (Object)null)
		{
			return component;
		}
		foreach (Transform item in _go.transform)
		{
			component = GetComponentInChildren<T>(item.gameObject);
			if ((Object)component != (Object)null)
			{
				return component;
			}
		}
		return (T)null;
	}

	public static void InitLayerMask(string _layer, ref int _mask)
	{
		_mask |= 1 << (LayerMask.NameToLayer(_layer) & 0x1F);
	}
}
