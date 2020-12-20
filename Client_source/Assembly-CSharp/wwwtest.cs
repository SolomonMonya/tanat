using UnityEngine;

public class wwwtest : MonoBehaviour
{
	private WWW mLoader;

	private AssetBundle mSceneBundle;

	private AsyncOperation mAsyncOp;

	private string mLastScene = string.Empty;

	private bool mLoaded;

	private void Update()
	{
		if (mLoader != null && mLoader.isDone && mLastScene != string.Empty)
		{
			if (string.IsNullOrEmpty(mLoader.error))
			{
				mSceneBundle = mLoader.assetBundle;
				mAsyncOp = Application.LoadLevelAdditiveAsync(mLastScene);
			}
			else
			{
				Debug.Log("mLoader.error : " + mLoader.error);
			}
			mLastScene = string.Empty;
		}
		if (mAsyncOp != null && mAsyncOp.isDone && !mLoaded)
		{
			mLoaded = true;
			mAsyncOp = null;
		}
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Create main_menu"))
		{
			LoadScene("main_menu");
		}
		if (GUILayout.Button("Create CS_Human"))
		{
			LoadScene("cs_human");
		}
		if (GUILayout.Button("Create CS_Elf"))
		{
			LoadScene("cs_elf");
		}
		if (GUILayout.Button("Create map_0_0"))
		{
			LoadScene("map_0_0");
		}
		if (GUILayout.Button("Create map_1_0"))
		{
			LoadScene("map_1_0");
		}
		if (GUILayout.Button("Create map_4_0"))
		{
			LoadScene("map_4_0");
		}
		if (GUILayout.Button("Create map_4_1"))
		{
			LoadScene("map_4_1");
		}
		if (GUILayout.Button("Create map_4_2"))
		{
			LoadScene("map_4_2");
		}
		if (GUILayout.Button("Clear") && mLoaded)
		{
			mLoaded = false;
			mSceneBundle.Unload(unloadAllLoadedObjects: true);
			mSceneBundle = null;
			mLoader.Dispose();
			mLoader = null;
			Object.Destroy(GameObject.Find("level"));
			Resources.UnloadUnusedAssets();
		}
	}

	private void LoadScene(string _scene)
	{
		mLastScene = _scene;
		mLoader = new WWW("file://C:\\TanatOnline 3\\data\\scenes\\" + mLastScene + ".unity3d");
	}
}
