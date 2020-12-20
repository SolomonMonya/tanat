using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerOld : MonoBehaviour
{
	[Serializable]
	public class MiniMapConfig : Utils.Named
	{
		public Vector2 mMapOffset;

		public float mScaleFactor = 1f;

		public float mRotFactor;

		public string mMiniMapImage;

		public float mMapSize;

		public bool mCamRotated;
	}

	public delegate void CameraLoadedCallback(GameObject _camGameObject);

	public CameraLoadedCallback mCameraLoadedCallback;

	public string mScenesLocation = "data/scenes/";

	public List<string> mOldStyleScenes;

	public MiniMapConfig[] mMimiMapConfigs;
}
