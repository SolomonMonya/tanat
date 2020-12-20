using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("FXs/VisualEffectOptions")]
public class VisualEffectOptions : MonoBehaviour
{
	private class SelectionData
	{
		public bool mSelf;

		public Friendliness mFriendliness;
	}

	[Serializable]
	public class EffectOptions
	{
		public string mEffect;

		public Vector3 mScale;

		public Vector3 mOffset;

		public bool mLockXRot;

		public bool mLockYRot;

		public bool mLockZRot;

		public string mBone;

		public EffectOptions()
		{
			mEffect = string.Empty;
			mScale = new Vector3(1f, 1f, 1f);
			mOffset = new Vector3(0f, 0f, 0f);
			mLockXRot = false;
			mLockYRot = false;
			mLockZRot = false;
			mBone = string.Empty;
		}
	}

	public string mDamageType = string.Empty;

	public string mDefenceType = string.Empty;

	public GameObject[] mProjectiles;

	public GameObject[] mProjectileEffects;

	public string mPojectileBone;

	public EffectOptions[] mEffectOptions;

	public static UnityEngine.Object mSelectionObjectPrefab;

	private static bool mSelectionPrefabRequested;

	private SelectionEffect mSelection;

	private Dictionary<GameObject, Material[]> mCatchedMaterials;

	private SelectionData mSelectionData;

	private float mSize;

	private bool mHiddenInited;

	private Color mHiddenColor = Color.black;

	private List<Renderer> mRenderers;

	private GameData mGameData;

	private bool mIsAttacker;

	private bool mIsSelected;

	public static void Uninit()
	{
		mSelectionObjectPrefab = null;
		mSelectionPrefabRequested = false;
	}

	public void Awake()
	{
		Validate();
		mRenderers = new List<Renderer>();
		mCatchedMaterials = new Dictionary<GameObject, Material[]>();
		CatchMaterials();
		if (base.collider != null)
		{
			mSize = base.collider.bounds.size.magnitude;
			if (mSelectionObjectPrefab == null && !mSelectionPrefabRequested)
			{
				AssetLoader.Instance?.LoadAsset("Selection_prop01", typeof(GameObject), new Notifier<ILoadedAsset, object>(OnSelectionLoaded, null));
				mSelectionPrefabRequested = true;
			}
		}
	}

	public void SetAttackSelection(bool _attacker)
	{
		if (_attacker != mIsAttacker)
		{
			mIsAttacker = _attacker;
			if (mIsAttacker && !mIsSelected)
			{
				SetSelectionColor();
			}
			else if (!mIsAttacker && !mIsSelected)
			{
				ClearSelectionColor();
			}
		}
	}

	private void SetSelectionColor()
	{
		if (mRenderers == null)
		{
			return;
		}
		if (mGameData == null)
		{
			mGameData = GetComponentInChildren<GameData>();
		}
		foreach (Renderer mRenderer in mRenderers)
		{
			if (mRenderer == null)
			{
				continue;
			}
			Material[] materials = mRenderer.materials;
			foreach (Material material in materials)
			{
				if (material != null && material.HasProperty("_OverlayColor"))
				{
					Color color = ((!mIsAttacker) ? GetEdgeColor(_self: false, Friendliness.ENEMY) : new Color(0.7f, 0.3f, 0f, 0.2f));
					if (mGameData != null && mIsSelected)
					{
						bool self = mGameData.Data.TryGetPlayer != null && mGameData.Data.TryGetPlayer.IsSelf;
						color = GetEdgeColor(self, mGameData.Data.GetFriendliness());
					}
					material.SetColor("_OverlayColor", color);
					if (material.HasProperty("_SelectSize"))
					{
						material.SetFloat("_SelectSize", 0.0027f);
					}
				}
			}
		}
	}

	private void ClearSelectionColor()
	{
		if (mRenderers == null)
		{
			return;
		}
		foreach (Renderer mRenderer in mRenderers)
		{
			if (mRenderer == null || mRenderer.gameObject == null)
			{
				continue;
			}
			Material[] materials = mRenderer.materials;
			foreach (Material material in materials)
			{
				if (material != null && material.HasProperty("_OverlayColor"))
				{
					material.SetColor("_OverlayColor", mHiddenColor);
				}
				if (material != null && material.HasProperty("_SelectSize"))
				{
					material.SetFloat("_SelectSize", 0f);
				}
			}
		}
	}

	public void OnMouseEnter()
	{
		mIsSelected = true;
		SetSelectionColor();
	}

	public void OnMouseExit()
	{
		mIsSelected = false;
		if (mIsAttacker)
		{
			SetSelectionColor();
		}
		else
		{
			ClearSelectionColor();
		}
	}

	public void Update()
	{
		if (base.collider != null)
		{
			if (mSelection == null && mSelectionObjectPrefab != null)
			{
				InitSelection();
			}
			if (mSelectionData != null && mSelection != null)
			{
				mSelection.SetColor(GetSelectionColor(mSelectionData.mSelf, mSelectionData.mFriendliness));
				mSelection.gameObject.active = true;
				mSelectionData = null;
			}
		}
		if (mHiddenInited)
		{
			return;
		}
		GameData component = GetComponent<GameData>();
		if (!(component == null) && component.Data != null)
		{
			mHiddenInited = true;
			Friendliness friendliness = component.Data.GetFriendliness();
			bool isSelf = false;
			if (component.Data.TryGetPlayer != null)
			{
				isSelf = component.Data.Player.IsSelf;
			}
			SetColors(isSelf, friendliness);
		}
	}

	private void SetColors(bool _isSelf, Friendliness _state)
	{
		mHiddenColor = GetEdgeColor(_isSelf, _state);
		foreach (Material[] value in mCatchedMaterials.Values)
		{
			Material[] array = value;
			foreach (Material material in array)
			{
				if (material != null && material.HasProperty("_OverlayColor"))
				{
					material.SetColor("_OverlayColor", mHiddenColor);
				}
			}
		}
	}

	public void ShotProjectile(Transform _end, float _targetTime)
	{
		if (mProjectiles.Length == 0)
		{
			Log.Error("Object projectile prefab is null : " + base.name);
			return;
		}
		Transform transform = base.transform;
		if (string.Empty != mPojectileBone)
		{
			transform = VisualEffectsMgr.GetObjectChildSafe(mPojectileBone, base.gameObject).transform;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(mProjectiles[UnityEngine.Random.Range(0, mProjectiles.Length)]) as GameObject;
		GameObjUtil.TrySetParent(gameObject, "/objects");
		Projectile component = gameObject.GetComponent<Projectile>();
		if (component == null)
		{
			Log.Error("Bad Projectile Obj : " + gameObject.name);
			return;
		}
		component.SetData(transform, _end, _targetTime);
		ProjectileEffect componentInChildren = GetComponentInChildren<ProjectileEffect>();
		if (componentInChildren != null && componentInChildren.mProjectileEffectNum >= 0 && componentInChildren.mProjectileEffectNum < mProjectileEffects.Length)
		{
			GameObject original = mProjectileEffects[componentInChildren.mProjectileEffectNum];
			original = UnityEngine.Object.Instantiate(original) as GameObject;
			original.transform.parent = component.transform;
			original.transform.localPosition = Vector3.zero;
			original.transform.localRotation = Quaternion.identity;
		}
	}

	public EffectOptions GetEffectOptions(string _effect)
	{
		EffectOptions[] array = mEffectOptions;
		foreach (EffectOptions effectOptions in array)
		{
			if (effectOptions.mEffect == _effect)
			{
				return effectOptions;
			}
		}
		return GetDefaultOptions();
	}

	public void SetDefaultMaterials()
	{
		foreach (KeyValuePair<GameObject, Material[]> mCatchedMaterial in mCatchedMaterials)
		{
			int i = 0;
			for (int num = mCatchedMaterial.Key.renderer.materials.Length; i < num; i++)
			{
				Material material = mCatchedMaterial.Value[i];
				mCatchedMaterial.Key.renderer.materials[i].shader = Shader.Find(material.shader.name);
				if (mCatchedMaterial.Key.renderer.materials[i].HasProperty("_Color"))
				{
					mCatchedMaterial.Key.renderer.materials[i].SetColor("_Color", material.color);
				}
			}
		}
	}

	public Dictionary<GameObject, Material[]> GetDefaultMaterials()
	{
		return mCatchedMaterials;
	}

	public void ShowSelection(bool _self, Friendliness _friendliness)
	{
		mSelectionData = new SelectionData();
		mSelectionData.mSelf = _self;
		mSelectionData.mFriendliness = _friendliness;
	}

	public void OnDisable()
	{
		HideSelection();
	}

	public void HideSelection()
	{
		if (!(mSelection == null))
		{
			mSelectionData = null;
			mSelection.gameObject.active = false;
		}
	}

	public void UpdateCatchRenderers()
	{
		CatchMaterials();
		foreach (Material[] value in mCatchedMaterials.Values)
		{
			Material[] array = value;
			foreach (Material material in array)
			{
				if (material != null && material.HasProperty("_OverlayColor"))
				{
					material.SetColor("_OverlayColor", mHiddenColor);
				}
			}
		}
	}

	private Color GetSelectionColor(bool _self, Friendliness _friendliness)
	{
		if (_self)
		{
			return new Color(0f, 0.3f, 1f, 0.35f);
		}
		return _friendliness switch
		{
			Friendliness.ENEMY => Color.red, 
			Friendliness.FRIEND => Color.green, 
			Friendliness.NEUTRAL => Color.yellow, 
			_ => Color.yellow, 
		};
	}

	private Color GetHidenColor(bool _self, Friendliness _friendliness)
	{
		if (_self)
		{
			return Color.white;
		}
		return _friendliness switch
		{
			Friendliness.ENEMY => new Color(1f, 64f / 255f, 64f / 255f, 1f), 
			Friendliness.FRIEND => new Color(22f / 255f, 0.5882353f, 91f / 255f, 1f), 
			Friendliness.NEUTRAL => Color.yellow, 
			_ => Color.yellow, 
		};
	}

	private Color GetEdgeColor(bool _self, Friendliness _friendliness)
	{
		if (_self)
		{
			return new Color(0f, 0.3f, 1f, 0.35f);
		}
		return _friendliness switch
		{
			Friendliness.ENEMY => new Color(1f, 0f, 0f, 0.3f), 
			Friendliness.FRIEND => new Color(0f, 0.4f, 0f, 0.3f), 
			Friendliness.NEUTRAL => new Color(0f, 0f, 0f, 0.3f), 
			_ => new Color(0f, 0.4f, 0f, 0.3f), 
		};
	}

	private static EffectOptions GetDefaultOptions()
	{
		return new EffectOptions();
	}

	private void CatchMaterials()
	{
		mCatchedMaterials.Clear();
		mRenderers.Clear();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
			{
				mCatchedMaterials.Add(renderer.gameObject, renderer.sharedMaterials);
				if (renderer.gameObject.layer != 16 && renderer.gameObject.layer != 11)
				{
					mRenderers.Add(renderer);
				}
			}
		}
	}

	private void OnSelectionLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success && !(mSelectionObjectPrefab != null))
		{
			mSelectionObjectPrefab = _asset.Asset;
			if (this != null)
			{
				InitSelection();
			}
		}
	}

	private void InitSelection()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(mSelectionObjectPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		gameObject.active = false;
		mSelection = gameObject.GetComponent<SelectionEffect>();
		mSelection.Init(mSize, base.transform.localScale);
	}

	private void Validate()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.castShadows = false;
			component.receiveShadows = false;
		}
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (componentInChildren != null)
		{
			componentInChildren.clip = null;
			componentInChildren.cullingType = AnimationCullingType.BasedOnRenderers;
			componentInChildren.animatePhysics = false;
			componentInChildren.playAutomatically = false;
		}
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			skinnedMeshRenderer.updateWhenOffscreen = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		Collider[] array2 = componentsInChildren2;
		foreach (Collider collider in array2)
		{
			collider.isTrigger = true;
		}
	}
}
