using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSelector : Selector
{
	public Building mBuilding;

	private List<Transform> mTargets = new List<Transform>();

	private System.Random mRnd;

	public override void Init()
	{
		mRnd = new System.Random();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.name == "TargetPoint")
			{
				mTargets.Add(item);
			}
		}
		if (mTargets.Count == 0)
		{
			mTargets.Add(base.transform);
		}
	}

	public override int CurrentValue()
	{
		return (int)mBuilding;
	}

	public Vector2 GetTargetPoint()
	{
		int index = mRnd.Next(mTargets.Count);
		double num = (double)(mRnd.Next(41) - 20) / 10.0;
		double num2 = (double)(mRnd.Next(41) - 20) / 10.0;
		return new Vector2(mTargets[index].position.x + (float)num, mTargets[index].position.z + (float)num2);
	}
}
