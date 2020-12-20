using System.IO;
using UnityEngine;

public class RenderTextureSave : MonoBehaviour
{
	public RenderTexture curTex;

	private void OnGUI()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			RenderTexture.active = curTex;
			Texture2D texture2D = new Texture2D(curTex.width, curTex.height);
			texture2D.ReadPixels(new Rect(0f, 0f, curTex.width, curTex.height), 0, 0);
			texture2D.Apply();
			File.WriteAllBytes(Application.dataPath + "/../SavedRenderTexture.png", texture2D.EncodeToPNG());
		}
	}
}
