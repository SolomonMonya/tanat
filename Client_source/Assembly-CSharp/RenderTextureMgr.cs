using UnityEngine;

[AddComponentMenu("FXs/RenderTextureMgr")]
internal class RenderTextureMgr : MonoBehaviour
{
	public GameObject[] mObjects = new GameObject[0];

	public int mTextureSize;

	public string mMaterialName = string.Empty;

	public string mShaderTextureName = string.Empty;

	public TextureWrapMode mMode = TextureWrapMode.Clamp;

	private RenderTexture mTexture;

	public void OnEnable()
	{
		if (null == base.camera)
		{
			return;
		}
		mTexture = new RenderTexture(mTextureSize, mTextureSize, 24);
		mTexture.isPowerOfTwo = true;
		mTexture.useMipMap = false;
		mTexture.wrapMode = mMode;
		mTexture.Create();
		base.camera.targetTexture = mTexture;
		base.camera.aspect = 1f;
		Renderer renderer = null;
		Projector projector = null;
		Material material = null;
		GameObject[] array = mObjects;
		foreach (GameObject gameObject in array)
		{
			renderer = gameObject.GetComponent<Renderer>();
			projector = gameObject.GetComponent<Projector>();
			material = null;
			if (null != projector)
			{
				material = projector.material;
				TrySetTexture(ref material);
			}
			if (!(null != renderer))
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material2 in sharedMaterials)
			{
				material = material2;
				if (TrySetTexture(ref material))
				{
					break;
				}
			}
		}
	}

	public void OnDisable()
	{
		if (!(null == mTexture))
		{
			mTexture.Release();
			Object.Destroy(mTexture);
			mTexture = null;
		}
	}

	private bool TrySetTexture(ref Material _mat)
	{
		if (null == _mat)
		{
			return false;
		}
		if (_mat.name == mMaterialName)
		{
			_mat.SetTexture(mShaderTextureName, mTexture);
			return true;
		}
		return false;
	}
}
