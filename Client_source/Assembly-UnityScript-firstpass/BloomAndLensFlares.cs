using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Bloom and Lens Flares (3.4)")]
[ExecuteInEditMode]
public class BloomAndLensFlares : PostEffectsBase
{
	public TweakMode34 tweakMode;

	public BloomScreenBlendMode screenBlendMode;

	public float sepBlurSpread;

	public float useSrcAlphaAsMask;

	public float bloomIntensity;

	public float bloomThreshhold;

	public int bloomBlurIterations;

	public bool lensflares;

	public int hollywoodFlareBlurIterations;

	public LensflareStyle34 lensflareMode;

	public float hollyStretchWidth;

	public float lensflareIntensity;

	public float lensflareThreshhold;

	public Color flareColorA;

	public Color flareColorB;

	public Color flareColorC;

	public Color flareColorD;

	public float blurWidth;

	public Shader lensFlareShader;

	private Material lensFlareMaterial;

	public Shader vignetteShader;

	private Material vignetteMaterial;

	public Shader separableBlurShader;

	private Material separableBlurMaterial;

	public Shader addBrightStuffOneOneShader;

	private Material addBrightStuffBlendOneOneMaterial;

	public Shader screenBlendShader;

	private Material screenBlend;

	public Shader hollywoodFlaresShader;

	private Material hollywoodFlaresMaterial;

	public Shader brightPassFilterShader;

	private Material brightPassFilterMaterial;

	public BloomAndLensFlares()
	{
		screenBlendMode = BloomScreenBlendMode.Screen;
		sepBlurSpread = 1.5f;
		useSrcAlphaAsMask = 0.5f;
		bloomIntensity = 1f;
		bloomThreshhold = 0.5f;
		bloomBlurIterations = 2;
		hollywoodFlareBlurIterations = 2;
		lensflareMode = LensflareStyle34.Anamorphic;
		hollyStretchWidth = 3.5f;
		lensflareIntensity = 1f;
		lensflareThreshhold = 0.3f;
		flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
		flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
		flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
		flareColorD = new Color(0.8f, 0.4f, 0f, 0.75f);
		blurWidth = 1f;
	}

	public override void Start()
	{
		CreateMaterials();
		CheckSupport(needDepth: false);
	}

	public override void CreateMaterials()
	{
		screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, screenBlend);
		lensFlareMaterial = CheckShaderAndCreateMaterial(lensFlareShader, lensFlareMaterial);
		vignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, vignetteMaterial);
		separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, separableBlurMaterial);
		addBrightStuffBlendOneOneMaterial = CheckShaderAndCreateMaterial(addBrightStuffOneOneShader, addBrightStuffBlendOneOneMaterial);
		hollywoodFlaresMaterial = CheckShaderAndCreateMaterial(hollywoodFlaresShader, hollywoodFlaresMaterial);
		brightPassFilterMaterial = CheckShaderAndCreateMaterial(brightPassFilterShader, brightPassFilterMaterial);
	}

	public override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		RenderTexture temporary = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
		RenderTexture temporary3 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
		RenderTexture temporary4 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
		float num = 1f * (float)source.width / (1f * (float)source.height);
		float num2 = 0.001953125f;
		Graphics.Blit(source, temporary, screenBlend, 2);
		Graphics.Blit(temporary, temporary2, screenBlend, 2);
		RenderTexture.ReleaseTemporary(temporary);
		BrightFilter(bloomThreshhold, useSrcAlphaAsMask, temporary2, temporary3);
		if (bloomBlurIterations < 1)
		{
			bloomBlurIterations = 1;
		}
		checked
		{
			for (int i = 0; i < bloomBlurIterations; i++)
			{
				float num3 = (float)bloomBlurIterations * 1f * sepBlurSpread;
				separableBlurMaterial.SetVector("offsets", new Vector4(0f, num3 * num2, 0f, 0f));
				Graphics.Blit((i != 0) ? temporary2 : temporary3, temporary4, separableBlurMaterial);
				separableBlurMaterial.SetVector("offsets", new Vector4(num3 / num * num2, 0f, 0f, 0f));
				Graphics.Blit(temporary4, temporary2, separableBlurMaterial);
			}
			if (lensflares)
			{
				if (lensflareMode == LensflareStyle34.Ghosting)
				{
					BrightFilter(lensflareThreshhold, 0f, temporary3, temporary4);
					separableBlurMaterial.SetVector("offsets", new Vector4(0f, 2f / (1f * (float)temporary2.height), 0f, 0f));
					Graphics.Blit(temporary4, temporary3, separableBlurMaterial);
					separableBlurMaterial.SetVector("offsets", new Vector4(2f / (1f * (float)temporary2.width), 0f, 0f, 0f));
					Graphics.Blit(temporary3, temporary4, separableBlurMaterial);
					Vignette(0.975f, temporary4, temporary3);
					BlendFlares(temporary3, temporary2);
				}
				else
				{
					hollywoodFlaresMaterial.SetVector("_Threshhold", new Vector4(lensflareThreshhold, 1f / (1f - lensflareThreshhold), 0f, 0f));
					hollywoodFlaresMaterial.SetVector("tintColor", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * flareColorA.a * lensflareIntensity);
					Graphics.Blit(temporary4, temporary3, hollywoodFlaresMaterial, 2);
					Graphics.Blit(temporary3, temporary4, hollywoodFlaresMaterial, 3);
					hollywoodFlaresMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / num * num2, 0f, 0f, 0f));
					hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth);
					Graphics.Blit(temporary4, temporary3, hollywoodFlaresMaterial, 1);
					hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth * 2f);
					Graphics.Blit(temporary3, temporary4, hollywoodFlaresMaterial, 1);
					hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth * 4f);
					Graphics.Blit(temporary4, temporary3, hollywoodFlaresMaterial, 1);
					if (lensflareMode == LensflareStyle34.Anamorphic)
					{
						for (int j = 0; j < hollywoodFlareBlurIterations; j++)
						{
							separableBlurMaterial.SetVector("offsets", new Vector4(hollyStretchWidth * 2f / num * num2, 0f, 0f, 0f));
							Graphics.Blit(temporary3, temporary4, separableBlurMaterial);
							separableBlurMaterial.SetVector("offsets", new Vector4(hollyStretchWidth * 2f / num * num2, 0f, 0f, 0f));
							Graphics.Blit(temporary4, temporary3, separableBlurMaterial);
						}
						AddTo(1f, temporary3, temporary2);
					}
					else
					{
						for (int k = 0; k < hollywoodFlareBlurIterations; k++)
						{
							separableBlurMaterial.SetVector("offsets", new Vector4(hollyStretchWidth * 2f / num * num2, 0f, 0f, 0f));
							Graphics.Blit(temporary3, temporary4, separableBlurMaterial);
							separableBlurMaterial.SetVector("offsets", new Vector4(hollyStretchWidth * 2f / num * num2, 0f, 0f, 0f));
							Graphics.Blit(temporary4, temporary3, separableBlurMaterial);
						}
						Vignette(1f, temporary3, temporary4);
						BlendFlares(temporary4, temporary3);
						AddTo(1f, temporary3, temporary2);
					}
				}
			}
			screenBlend.SetFloat("_Intensity", bloomIntensity);
			screenBlend.SetTexture("_ColorBuffer", source);
		}
		Graphics.Blit(temporary2, destination, screenBlend, (int)screenBlendMode);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
		RenderTexture.ReleaseTemporary(temporary4);
	}

	private void AddTo(float intensity, RenderTexture from, RenderTexture to)
	{
		addBrightStuffBlendOneOneMaterial.SetFloat("intensity", intensity);
		Graphics.Blit(from, to, addBrightStuffBlendOneOneMaterial);
	}

	private void BlendFlares(RenderTexture from, RenderTexture to)
	{
		lensFlareMaterial.SetVector("colorA", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * lensflareIntensity);
		lensFlareMaterial.SetVector("colorB", new Vector4(flareColorB.r, flareColorB.g, flareColorB.b, flareColorB.a) * lensflareIntensity);
		lensFlareMaterial.SetVector("colorC", new Vector4(flareColorC.r, flareColorC.g, flareColorC.b, flareColorC.a) * lensflareIntensity);
		lensFlareMaterial.SetVector("colorD", new Vector4(flareColorD.r, flareColorD.g, flareColorD.b, flareColorD.a) * lensflareIntensity);
		Graphics.Blit(from, to, lensFlareMaterial);
	}

	private void BrightFilter(float thresh, float useAlphaAsMask, RenderTexture from, RenderTexture to)
	{
		brightPassFilterMaterial.SetVector("threshhold", new Vector4(thresh, 1f / (1f - thresh), 0f, 0f));
		brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", useAlphaAsMask);
		Graphics.Blit(from, to, brightPassFilterMaterial);
	}

	private void Vignette(float amount, RenderTexture from, RenderTexture to)
	{
		vignetteMaterial.SetFloat("vignetteIntensity", amount);
		Graphics.Blit(from, to, vignetteMaterial);
	}

	public override void Main()
	{
	}
}
