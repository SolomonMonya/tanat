using Log4Tanat;
using UnityEngine;

internal class CameraMgr : MonoBehaviour
{
	public static GameObject CreateCamera(string _cameraId)
	{
		string path = "Prefabs/Cameras/" + _cameraId;
		Object original = Resources.Load(path);
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		if (GameObjUtil.TrySetParent(gameObject, "/cameras"))
		{
			gameObject.name = _cameraId;
			return gameObject;
		}
		Log.Error("Can't add camera object " + Log.StackTrace());
		Object.Destroy(gameObject);
		return null;
	}
}
