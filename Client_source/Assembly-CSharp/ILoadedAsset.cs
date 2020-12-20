using System;
using UnityEngine;

public interface ILoadedAsset
{
	Type AssetType
	{
		get;
	}

	UnityEngine.Object Asset
	{
		get;
	}
}
