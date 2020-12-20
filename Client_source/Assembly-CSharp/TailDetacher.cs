using System.Collections.Generic;
using UnityEngine;

public class TailDetacher : MonoBehaviour
{
	private string mTailPrefix = "TailPhys_";

	public void Start()
	{
		DetachTails(base.gameObject.transform, base.gameObject.transform.parent);
	}

	private void DetachTails(Transform _trans, Transform _dest)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform _tran in _trans)
		{
			if (_tran.gameObject.name.StartsWith(mTailPrefix))
			{
				list.Add(_tran);
			}
			DetachTails(_tran, _dest);
		}
		foreach (Transform item in list)
		{
			item.parent = _dest;
		}
	}
}
