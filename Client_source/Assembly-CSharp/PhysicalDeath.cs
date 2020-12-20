using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Death Behaviour/physical death")]
public class PhysicalDeath : DeathBehaviour
{
	public float mMaxDieTime = 1.8f;

	private float mStartDieTime;

	private Dictionary<GameObject, Material[]> mCatchedMaterials;

	private bool mStartDie;

	private VisualEffectOptions mVisualEffectOptions;

	private WarFogObject mWarFogObject;

	private List<GameObject> mPhysicalElements;

	public GameObject[] mDisableParts;

	public void Awake()
	{
		mWarFogObject = GetComponent<WarFogObject>();
		mVisualEffectOptions = GetComponent<VisualEffectOptions>();
		mPhysicalElements = new List<GameObject>();
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody rigidbody in array)
		{
			rigidbody.gameObject.active = false;
			mPhysicalElements.Add(rigidbody.gameObject);
		}
	}

	public void OnEnable()
	{
		SetPhysicsEnabled(_enabled: false);
	}

	public void Update()
	{
		if (IsDone())
		{
			return;
		}
		if (!VisibilityMgr.IsVisible(base.gameObject))
		{
			Done();
			return;
		}
		float num = Time.realtimeSinceStartup - mStartDieTime;
		float num2 = 1f - num / mMaxDieTime;
		if (num2 < 0f)
		{
			if (mWarFogObject != null)
			{
				mWarFogObject.ToFog();
			}
			num2 = 0f;
			Done();
		}
		foreach (GameObject key in mCatchedMaterials.Keys)
		{
			Material[] materials = key.renderer.materials;
			Material[] array = materials;
			foreach (Material material in array)
			{
				Color color = material.color;
				color.a = num2;
				material.color = color;
			}
		}
	}

	private void SetPhysicsEnabled(bool _enabled)
	{
		if (mPhysicalElements == null)
		{
			return;
		}
		foreach (GameObject mPhysicalElement in mPhysicalElements)
		{
			mPhysicalElement.active = _enabled;
			if (mPhysicalElement.collider != null)
			{
				mPhysicalElement.collider.isTrigger = !_enabled;
			}
			if (mPhysicalElement.rigidbody != null)
			{
				mPhysicalElement.rigidbody.useGravity = _enabled;
			}
		}
	}

	protected override void StartDie()
	{
		mStartDie = true;
		GameObject[] array = mDisableParts;
		foreach (GameObject gameObject in array)
		{
			gameObject.active = false;
		}
		VisualEffectOptions component = GetComponent<VisualEffectOptions>();
		mCatchedMaterials = component.GetDefaultMaterials();
		foreach (KeyValuePair<GameObject, Material[]> mCatchedMaterial in mCatchedMaterials)
		{
			Material[] materials = mCatchedMaterial.Key.renderer.materials;
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				material.shader = Shader.Find("Tanat/Fx/DeathShader");
			}
		}
		mStartDieTime = Time.realtimeSinceStartup;
		SetPhysicsEnabled(_enabled: true);
	}

	public override void Reborn()
	{
		if (mStartDie)
		{
			GameObject[] array = mDisableParts;
			foreach (GameObject gameObject in array)
			{
				gameObject.active = true;
			}
			mStartDie = false;
			SetPhysicsEnabled(_enabled: false);
			mVisualEffectOptions.SetDefaultMaterials();
			if (mWarFogObject != null)
			{
				mWarFogObject.FromFog();
			}
			base.Reborn();
		}
	}
}
