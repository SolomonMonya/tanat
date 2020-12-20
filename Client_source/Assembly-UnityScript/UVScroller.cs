using System;
using UnityEngine;

[Serializable]
public class UVScroller : MonoBehaviour
{
	public float scrollSpeed;

	public UVScroller()
	{
		scrollSpeed = 0.1f;
	}

	public override void Update()
	{
		float y = Time.time * scrollSpeed;
		renderer.material.SetTextureOffset("_BumpMap", new Vector2(0f, y));
		renderer.material.SetTextureOffset("_MainTex", new Vector2(0f, y));
		if (renderer.materials.Length > 1)
		{
			Material material = renderer.materials[1];
			if ((bool)material)
			{
				material.SetTextureOffset("_MainTex", new Vector2(0f, y));
			}
		}
	}

	public override void Main()
	{
	}
}
