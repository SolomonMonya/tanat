using System;
using UnityEngine;

public class AvatarShadow : MonoBehaviour
{
	public int _TexSize = 512;

	private RenderTexture mTexture;

	private Material mShadowMaterial;

	private static string shadowMatString = "Shader \"Hidden/ShadowMat\" \r\n{\r\n\tSubShader \r\n\t{\r\n\t\tPass \r\n\t\t{\r\n\t\t\tZTest Greater Cull Off ZWrite Off\r\n\t\t\tSetTexture [_Dummy] { combine primary }\r\n\t\t}\r\n\t}\r\n\tFallback off\r\n}";

	private Material shadowMaterial
	{
		get
		{
			if (mShadowMaterial == null)
			{
				mShadowMaterial = new Material(shadowMatString);
				mShadowMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
				mShadowMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return mShadowMaterial;
		}
	}

	public void OnPostRender()
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < shadowMaterial.passCount; i++)
		{
			shadowMaterial.SetPass(i);
			shadowMaterial.SetColor("_Color", Color.gray);
			GL.Begin(7);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, -99.99f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, -99.99f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, -99.99f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(0f, 1f, -99.99f);
			GL.End();
		}
		GL.PopMatrix();
	}

	public void Start()
	{
		mTexture = new RenderTexture(_TexSize, _TexSize, 24);
		mTexture.isPowerOfTwo = true;
		mTexture.useMipMap = false;
		mTexture.Create();
		base.camera.targetTexture = mTexture;
		base.camera.aspect = 1f;
		Projector component = GetComponent<Projector>();
		if (null != component)
		{
			component.material.SetTexture("_ShadowTex", mTexture);
		}
	}

	private GameCamera GetGameCamera()
	{
		UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(GameCamera));
		return @object as GameCamera;
	}

	public void OnEnable()
	{
		GameCamera gameCamera = GetGameCamera();
		if (null != gameCamera)
		{
			gameCamera.mCameraMoved = (GameCamera.CameraMoved)Delegate.Combine(gameCamera.mCameraMoved, new GameCamera.CameraMoved(OnCameraMoved));
		}
	}

	public void OnDisable()
	{
		if (null != mTexture)
		{
			mTexture.Release();
		}
		GameCamera gameCamera = GetGameCamera();
		if (null != gameCamera)
		{
			gameCamera.mCameraMoved = (GameCamera.CameraMoved)Delegate.Remove(gameCamera.mCameraMoved, new GameCamera.CameraMoved(OnCameraMoved));
		}
	}

	private void OnCameraMoved(Vector3 _lookPoint, float _zoom)
	{
		float num = (_zoom - 15f) / 40f;
		float orthographicSize = 10f + num * 20f;
		Camera component = base.gameObject.GetComponent<Camera>();
		component.orthographicSize = orthographicSize;
		Projector component2 = base.gameObject.GetComponent<Projector>();
		component2.orthographicSize = orthographicSize;
		base.transform.position = _lookPoint;
	}
}
