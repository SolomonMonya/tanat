using UnityEngine;

public class SelectionTest : MonoBehaviour
{
	public int m_iterations = 3;

	public float m_blurSpread = 0.6f;

	private Shader mShader;

	private GameObject mShaderCameraObj;

	private GameObject mBlurCameraObj;

	private RenderTexture mRenderTex1;

	private RenderTexture mRenderTex2;

	private Material mMaterial;

	public void Start()
	{
		mShader = Shader.Find("Hidden/ImageSelection");
		Camera camera = null;
		Camera camera2 = null;
		mShaderCameraObj = new GameObject("ShaderCamera");
		mShaderCameraObj.transform.parent = base.transform;
		camera = mShaderCameraObj.AddComponent<Camera>();
		camera.enabled = false;
		mBlurCameraObj = new GameObject("BlurCamera");
		mBlurCameraObj.transform.parent = base.transform;
		camera2 = mBlurCameraObj.AddComponent<Camera>();
		camera2.enabled = false;
		BlurEffect blurEffect = mBlurCameraObj.AddComponent<BlurEffect>();
		blurEffect.blurSpread = m_blurSpread;
		blurEffect.iterations = m_iterations;
		CreateTextures();
	}

	public void ReinitTextures()
	{
		CreateTextures();
		Camera camera = null;
		Camera camera2 = null;
		camera = mShaderCameraObj.GetComponent<Camera>();
		camera.targetTexture = mRenderTex1;
		camera2 = mBlurCameraObj.GetComponent<Camera>();
		camera2.targetTexture = mRenderTex2;
	}

	public void Init()
	{
		Camera camera = null;
		Camera camera2 = null;
		camera = mShaderCameraObj.GetComponent<Camera>();
		camera.CopyFrom(base.camera);
		camera.backgroundColor = Color.clear;
		camera.clearFlags = CameraClearFlags.Color;
		camera.targetTexture = mRenderTex1;
		camera.cullingMask = 2883584;
		camera2 = mBlurCameraObj.GetComponent<Camera>();
		camera2.CopyFrom(base.camera);
		camera2.backgroundColor = Color.clear;
		camera2.clearFlags = CameraClearFlags.Color;
		camera2.targetTexture = mRenderTex2;
		camera2.cullingMask = 2883584;
	}

	public void OnDestroy()
	{
		mRenderTex1.Release();
		mRenderTex2.Release();
	}

	public void OnPostRender()
	{
		Camera camera = mShaderCameraObj.camera;
		camera.Render();
		Camera camera2 = mBlurCameraObj.camera;
		camera2.Render();
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (mMaterial == null)
		{
			Graphics.Blit(source, destination);
		}
		else
		{
			Graphics.Blit(source, destination, mMaterial);
		}
	}

	public void CreateTextures()
	{
		if (mRenderTex1 != null)
		{
			mRenderTex1.Release();
		}
		if (mRenderTex2 != null)
		{
			mRenderTex2.Release();
		}
		mRenderTex1 = new RenderTexture(OptionsMgr.mScreenWidth, OptionsMgr.mScreenHeight, 0);
		mRenderTex1.Create();
		mRenderTex2 = new RenderTexture(OptionsMgr.mScreenWidth, OptionsMgr.mScreenHeight, 0);
		mRenderTex2.Create();
		if (mMaterial == null)
		{
			mMaterial = new Material(mShader);
		}
		mMaterial.SetTexture("_shaderCamera", mRenderTex1);
		mMaterial.SetTexture("_blurCamera", mRenderTex2);
	}
}
