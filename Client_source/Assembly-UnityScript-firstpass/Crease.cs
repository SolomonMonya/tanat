using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Crease")]
public class Crease : PostEffectsBase
{
	public float intensity;

	public int softness;

	public float spread;

	public Shader blurShader;

	private Material blurMaterial;

	public Shader depthFetchShader;

	private Material depthFetchMaterial;

	public Shader creaseApplyShader;

	private Material creaseApplyMaterial;

	public Crease()
	{
		intensity = 0.5f;
		softness = 1;
		spread = 1f;
	}

	public override void CreateMaterials()
	{
		blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);
		depthFetchMaterial = CheckShaderAndCreateMaterial(depthFetchShader, depthFetchMaterial);
		creaseApplyMaterial = CheckShaderAndCreateMaterial(creaseApplyShader, creaseApplyMaterial);
	}

	public override void Start()
	{
		CreateMaterials();
		CheckSupport(needDepth: true);
	}

	public override void OnEnable()
	{
		camera.depthTextureMode |= DepthTextureMode.Depth;
	}

	public override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		float num = 1f * (float)source.width / (1f * (float)source.height);
		float num2 = 0.001953125f;
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0);
		RenderTexture temporary3 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0);
		Graphics.Blit(source, temporary, depthFetchMaterial);
		Graphics.Blit(temporary, temporary2);
		for (int i = 0; i < softness; i = checked(i + 1))
		{
			blurMaterial.SetVector("offsets", new Vector4(0f, spread * num2, 0f, 0f));
			Graphics.Blit(temporary2, temporary3, blurMaterial);
			blurMaterial.SetVector("offsets", new Vector4(spread * num2 / num, 0f, 0f, 0f));
			Graphics.Blit(temporary3, temporary2, blurMaterial);
		}
		creaseApplyMaterial.SetTexture("_HrDepthTex", temporary);
		creaseApplyMaterial.SetTexture("_LrDepthTex", temporary2);
		creaseApplyMaterial.SetFloat("intensity", intensity);
		Graphics.Blit(source, destination, creaseApplyMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	public override void Main()
	{
	}
}
