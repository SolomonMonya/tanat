using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/MultiTargetsEffect")]
public class MultiTargetsEffect : MonoBehaviour
{
	public bool mDone;

	public virtual void Init()
	{
	}

	public virtual bool SetTargets(List<GameObject> _targets)
	{
		foreach (GameObject _target in _targets)
		{
			if (null == _target)
			{
				Log.Error("null target object in : " + base.name);
				return false;
			}
		}
		return true;
	}

	public virtual bool SetTargets(List<Vector3> _targets)
	{
		return _targets != null;
	}
}
