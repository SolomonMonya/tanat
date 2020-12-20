using System.Collections.Generic;
using UnityEngine;

public class ColliderScript : MonoBehaviour
{
	private bool mToAlpha;

	private float mCurAlpha = 1f;

	private Shader mToAlphaShader;

	private List<Material> mMaterials;

	private Shader[] mObjectShaders;

	public float mK = 2f;

	private void Start()
	{
		base.gameObject.layer = 2;
		mToAlphaShader = Shader.Find("Tanat/Fx/DeathShader");
		mMaterials = GetMaterials();
		mObjectShaders = new Shader[mMaterials.Count];
		int i = 0;
		for (int count = mMaterials.Count; i < count; i++)
		{
			mObjectShaders[i] = mMaterials[i].shader;
		}
	}

	public void Update()
	{
		if (mToAlpha && mCurAlpha > 0f)
		{
			mCurAlpha -= Time.deltaTime * mK;
			mCurAlpha = ((!(mCurAlpha < 0f)) ? mCurAlpha : 0f);
			SetAlpha(mCurAlpha);
			if (mCurAlpha == 0f)
			{
				SetActive(_enabled: false);
			}
		}
		else if (!mToAlpha && mCurAlpha < 1f)
		{
			mCurAlpha += Time.deltaTime * mK;
			mCurAlpha = ((!(mCurAlpha > 1f)) ? mCurAlpha : 1f);
			SetAlpha(mCurAlpha);
			if (mCurAlpha == 1f)
			{
				SetStartShaders();
			}
		}
	}

	public void OnTriggerEnter(Collider _collider)
	{
		mToAlpha = true;
		SetShader(mToAlphaShader);
	}

	public void OnTriggerExit(Collider _collider)
	{
		mToAlpha = false;
		SetActive(_enabled: true);
	}

	private void SetActive(bool _enabled)
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = _enabled;
		}
	}

	private void SetShader(Shader _shader)
	{
		if (mMaterials == null || _shader == null)
		{
			return;
		}
		foreach (Material mMaterial in mMaterials)
		{
			mMaterial.shader = _shader;
		}
	}

	private void SetStartShaders()
	{
		int i = 0;
		for (int count = mMaterials.Count; i < count; i++)
		{
			mMaterials[i].shader = mObjectShaders[i];
		}
	}

	private void SetAlpha(float _alpha)
	{
		foreach (Material mMaterial in mMaterials)
		{
			Color color = mMaterial.color;
			color.a = _alpha;
			mMaterial.color = color;
		}
	}

	private List<Material> GetMaterials()
	{
		List<Material> list = new List<Material>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			foreach (Material item in materials)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
