using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/DirectionEffect")]
public class DirectionEffect : MultiTargetsEffect
{
	public Vector3 mOffset;

	public override bool SetTargets(List<Vector3> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count DirectionEffect.SetTargets()");
			return false;
		}
		base.transform.position = _targets[0];
		base.transform.forward = (_targets[1] + mOffset - _targets[0]).normalized;
		return true;
	}

	public override bool SetTargets(List<GameObject> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets count DirectionEffect.SetTargets()");
			return false;
		}
		base.transform.position = _targets[0].transform.position;
		base.transform.forward = (_targets[1].transform.position + mOffset - _targets[0].transform.position).normalized;
		return true;
	}
}
