using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Edge Detection (Geometry)")]
[ExecuteInEditMode]
public class EdgeDetectEffectNormals : PostEffectsBase
{
	public EdgeDetectMode mode;

	public float sensitivityDepth;

	public float sensitivityNormals;

	public float edgesOnly;

	public Color edgesOnlyBgColor;

	public Shader edgeDetectShader;

	private Material edgeDetectMaterial;

	public EdgeDetectEffectNormals()
	{
		mode = EdgeDetectMode.Thin;
		sensitivityDepth = 1f;
		sensitivityNormals = 1f;
		edgesOnlyBgColor = Color.white;
	}

	public override void CreateMaterials()
	{
		edgeDetectMaterial = CheckShaderAndCreateMaterial(edgeDetectShader, edgeDetectMaterial);
	}

	public override void Start()
	{
		CreateMaterials();
		CheckSupport(needDepth: true);
	}

	public override void OnEnable()
	{
		camera.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	public override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		Vector2 vector = new Vector2(sensitivityDepth, sensitivityNormals);
		source.filterMode = FilterMode.Point;
		edgeDetectMaterial.SetVector("sensitivity", new Vector4(vector.x, vector.y, 1f, vector.y));
		edgeDetectMaterial.SetFloat("_BgFade", edgesOnly);
		Vector4 vector2 = edgesOnlyBgColor;
		edgeDetectMaterial.SetVector("_BgColor", vector2);
		if (mode == EdgeDetectMode.Thin)
		{
			Graphics.Blit(source, destination, edgeDetectMaterial, 0);
		}
		else
		{
			Graphics.Blit(source, destination, edgeDetectMaterial, 1);
		}
	}

	public override void Main()
	{
	}
}
