using System.Collections;
using Log4Tanat;
using UnityEngine;

public class ModCombiner : MonoBehaviour
{
	public Mod[] mStartMods;

	private SortedList mListMods = new SortedList();

	private void Start()
	{
		Mod[] array = mStartMods;
		foreach (Mod mod in array)
		{
			AttachMod(mod);
		}
	}

	private void Update()
	{
		if (Input.GetButton("Jump"))
		{
			RemoveMod("test");
		}
	}

	public Mod[] GetStartMods()
	{
		return mStartMods;
	}

	private void AttachMod(Mod _mod)
	{
		Log.Notice(_mod.mId);
		Mod.Part[] mParts = _mod.mParts;
		foreach (Mod.Part part in mParts)
		{
			if (null == part.mSkinnedMesh)
			{
				Log.Error("null skinned mesh");
				return;
			}
			GameObject gameObject = base.transform.Find(part.mSkinnedMesh.name).gameObject;
			SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)gameObject.GetComponent("SkinnedMeshRenderer");
			skinnedMeshRenderer.sharedMesh = part.mMesh;
			skinnedMeshRenderer.material.SetTexture("_Diffuse" + part.mLayer, part.mDiffuse);
			skinnedMeshRenderer.material.SetTexture("_Tint" + part.mLayer, part.mTint);
			skinnedMeshRenderer.material.SetColor("_ColorAdditive" + part.mLayer, part.mTintAdditive);
			skinnedMeshRenderer.material.SetColor("_ColorMultiply" + part.mLayer, part.mTintMultiply);
		}
		mListMods.Add(_mod.mId, _mod);
	}

	private void RemoveMod(string _id)
	{
		Log.Notice(_id);
		if (!mListMods.Contains(_id))
		{
			Log.Error("this mod not present in character environment");
			return;
		}
		Mod mod = (Mod)mListMods[_id];
		Mod.Part[] mParts = mod.mParts;
		foreach (Mod.Part part in mParts)
		{
			GameObject gameObject = base.transform.Find(part.mSkinnedMesh.name).gameObject;
			SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)gameObject.GetComponent("SkinnedMeshRenderer");
			skinnedMeshRenderer.material.SetTexture("_Diffuse" + part.mLayer, null);
			skinnedMeshRenderer.material.SetTexture("_Tint" + part.mLayer, null);
			skinnedMeshRenderer.material.SetColor("_ColorAdditive" + part.mLayer, Color.black);
			skinnedMeshRenderer.material.SetColor("_ColorMultiply" + part.mLayer, Color.white);
			int num = 1;
			for (int j = 0; j < 4; j++)
			{
				if (null != skinnedMeshRenderer.material.GetTexture("_Diffuse" + (j + 1)) || null != skinnedMeshRenderer.material.GetTexture("_Tint" + (j + 1)))
				{
					num = 0;
					break;
				}
			}
			if (num == 1)
			{
				skinnedMeshRenderer.sharedMesh = null;
			}
		}
		mListMods.Remove(_id);
	}
}
