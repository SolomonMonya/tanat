using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("FXs/InvisibilityEffect")]
public class InvisibilityEffect : MonoBehaviour
{
	public float mToAlphaTime = 0.5f;

	public float mTargetAlpha = 0.5f;

	private VisualEffectOptions mVisualEffectOptions;

	private Dictionary<GameObject, Material[]> mDefaultMaterials;

	private bool mApplied;

	public void OnEnable()
	{
		TryApply();
	}

	public void TryApply()
	{
		GameObject gameObject = base.transform.parent.parent.gameObject;
		if (gameObject == null || !VisibilityMgr.IsVisible(gameObject))
		{
			return;
		}
		mVisualEffectOptions = gameObject.GetComponent<VisualEffectOptions>();
		if (mVisualEffectOptions == null)
		{
			return;
		}
		mDefaultMaterials = mVisualEffectOptions.GetDefaultMaterials();
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (mDefaultMaterials.ContainsKey(renderer.gameObject))
			{
				int j = 0;
				for (int num = renderer.materials.Length; j < num; j++)
				{
					renderer.materials[j].shader = Shader.Find("Tanat/Fx/DeathShader");
					renderer.materials[j].color = new Color(1f, 1f, 1f, mTargetAlpha);
				}
			}
		}
		mApplied = true;
	}

	public void Update()
	{
		if (!mApplied)
		{
			TryApply();
		}
	}

	public void OnDisable()
	{
		SetDefaultVisibility();
	}

	private void SetDefaultVisibility()
	{
		if (!(mVisualEffectOptions == null))
		{
			mVisualEffectOptions.SetDefaultMaterials();
		}
	}
}
