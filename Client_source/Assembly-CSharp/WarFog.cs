using UnityEngine;

[AddComponentMenu("War Fog/WarFog")]
public class WarFog : MonoBehaviour
{
	private RenderTexture mFogTexture;

	private GameObject mWarFogPlane;

	private float mSize;

	public void Init(string _curSet)
	{
		mFogTexture = new RenderTexture(256, 256, 24);
		mFogTexture.isPowerOfTwo = true;
		mFogTexture.useMipMap = false;
		mFogTexture.Create();
		InitWarFogPlane();
		InitRenderCam();
		InitMiniMap(_curSet);
	}

	public void OnDisable()
	{
		if (null != mFogTexture)
		{
			mFogTexture.Release();
			mFogTexture = null;
		}
	}

	private void InitRenderCam()
	{
		if (!(null == base.camera))
		{
			mSize = HeightMap.GetTerrainSize();
			Vector3 zero = Vector3.zero;
			zero.y += 500f;
			base.camera.targetTexture = mFogTexture;
			base.camera.transform.position = zero;
			base.camera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
			base.camera.aspect = 1f;
			base.camera.orthographicSize = mSize / 2f;
			if (null != mWarFogPlane)
			{
				mWarFogPlane.transform.localScale = new Vector3(mSize, mSize, mSize);
			}
		}
	}

	private void InitWarFogPlane()
	{
		mWarFogPlane = GameObject.Find("WarFogPlane_prop01");
		if ((bool)mWarFogPlane)
		{
			MeshRenderer meshRenderer = mWarFogPlane.GetComponentInChildren(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.material.mainTexture = mFogTexture;
		}
	}

	private void InitMiniMap(string _curSet)
	{
		GuiSystem.mGuiSystem.GetGuiElement<MiniMap>(_curSet, "MINI_MAP_GAME_MENU")?.SetTexture(mFogTexture);
	}
}
