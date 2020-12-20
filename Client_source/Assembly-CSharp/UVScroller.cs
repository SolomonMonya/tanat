using UnityEngine;

[AddComponentMenu("FXs/UVScroller")]
public class UVScroller : MonoBehaviour
{
	public float mScrollSpeedX = 0.1f;

	public float mScrollSpeedY = 0.1f;

	public string mTextureName = "_MainTex";

	public int[] mMaterialNums;

	private void Update()
	{
		if (base.renderer == null)
		{
			base.enabled = false;
		}
		float x = Mathf.Repeat(Time.time * mScrollSpeedX, 1f);
		float y = Mathf.Repeat(Time.time * mScrollSpeedY, 1f);
		int num = base.renderer.materials.Length;
		int[] array = mMaterialNums;
		foreach (int num2 in array)
		{
			if (num2 >= 0 && num2 < num)
			{
				Material material = base.renderer.materials[num2];
				if ((bool)material)
				{
					material.SetTextureOffset(mTextureName, new Vector2(x, y));
				}
			}
			else
			{
				Debug.LogError("Bad material num in UVScroller : " + num2 + " in : " + base.name);
			}
		}
	}
}
