using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class HeroWear : MonoBehaviour
{
	public List<string> mHairsHelms = new List<string>();

	private Dictionary<CustomType, string[]> mCustomizeParts = new Dictionary<CustomType, string[]>();

	private HeroRace mRace;

	private bool mGender;

	private HeroMgr mHeroMgr;

	private VisualEffectOptions mVisualEffectOptions;

	public HeroRace Race => mRace;

	public void OnEnable()
	{
		mHeroMgr = GameObjUtil.FindObjectOfType<HeroMgr>();
	}

	public void OnDisable()
	{
		mHeroMgr = null;
		mVisualEffectOptions = null;
	}

	public void Awake()
	{
		mCustomizeParts.Add(CustomType.HAIR, new string[1]
		{
			"Hair"
		});
		mCustomizeParts.Add(CustomType.HAIR_COLOR, new string[1]
		{
			"Hair"
		});
		mCustomizeParts.Add(CustomType.FACE, new string[1]
		{
			"Head"
		});
		mCustomizeParts.Add(CustomType.SPECIALITY, new string[1]
		{
			"Head"
		});
		mCustomizeParts.Add(CustomType.SKIN_COLOR, new string[4]
		{
			"Body",
			"Legs",
			"Hands",
			"Head"
		});
		mCustomizeParts.Add(CustomType.BODY, new string[1]
		{
			"Body"
		});
		mCustomizeParts.Add(CustomType.FOOTS, new string[1]
		{
			"Foots"
		});
		mCustomizeParts.Add(CustomType.HANDS, new string[1]
		{
			"Hands"
		});
		mCustomizeParts.Add(CustomType.LEGS, new string[1]
		{
			"Legs"
		});
		mCustomizeParts.Add(CustomType.BELT, new string[1]
		{
			"Belt"
		});
		mCustomizeParts.Add(CustomType.SHOULDERS, new string[1]
		{
			"Shoulders"
		});
		mCustomizeParts.Add(CustomType.HELM, new string[1]
		{
			"Helm"
		});
	}

	public void Start()
	{
		if (base.transform.parent != null)
		{
			mVisualEffectOptions = base.transform.parent.gameObject.GetComponent<VisualEffectOptions>();
		}
		if (mVisualEffectOptions != null)
		{
			mVisualEffectOptions.UpdateCatchRenderers();
		}
	}

	public void SetHero(HeroView _params)
	{
		if (!(mHeroMgr == null))
		{
			mRace = (HeroRace)_params.mRace;
			mGender = _params.mGender;
			HeroMgr.HeroCustomizeData heroCustomizeData = null;
			Color clear = Color.clear;
			clear = mHeroMgr.GetCustomizeColor(mRace, CustomType.SKIN_COLOR, _params.mSkinColor);
			if (clear != Color.clear)
			{
				SetHeroColor(CustomType.SKIN_COLOR, clear);
			}
			clear = mHeroMgr.GetCustomizeColor(mRace, CustomType.HAIR_COLOR, _params.mHairColor);
			if (clear != Color.clear)
			{
				SetHeroColor(CustomType.HAIR_COLOR, clear);
			}
			heroCustomizeData = mHeroMgr.GetCustomizeFace(mRace, mGender, _params.mFace);
			if (heroCustomizeData != null)
			{
				SetItem(heroCustomizeData, CustomType.FACE);
			}
			heroCustomizeData = mHeroMgr.GetCustomizeHair(mRace, mGender, _params.mHair);
			if (heroCustomizeData != null)
			{
				SetItem(heroCustomizeData, CustomType.HAIR);
			}
		}
	}

	public void SetItems(ICollection<string> _items)
	{
		CustomType _type = (CustomType)(-1);
		foreach (string _item in _items)
		{
			HeroMgr.HeroCustomizeData itemById = mHeroMgr.GetItemById(_item, mGender, ref _type);
			if (itemById != null)
			{
				SetItem(itemById, _type);
			}
		}
	}

	public void SetItem(string _item)
	{
		CustomType _type = (CustomType)(-1);
		HeroMgr.HeroCustomizeData itemById = mHeroMgr.GetItemById(_item, mGender, ref _type);
		if (itemById != null)
		{
			SetItem(itemById, _type);
		}
	}

	public void RemoveItem(string _item)
	{
		CustomType customTypeByItemId = HeroMgr.GetCustomTypeByItemId(HeroMgr.GenerateItemId(_item, mGender));
		HeroMgr.HeroCustomizeData defaultItem = mHeroMgr.GetDefaultItem(mRace, mGender, customTypeByItemId);
		SetItem(defaultItem, customTypeByItemId);
	}

	public void SetDefault(HeroRace _race, bool _gender)
	{
		if (mHeroMgr == null)
		{
			return;
		}
		for (CustomType customType = CustomType.FACE; customType <= CustomType.HELM; customType++)
		{
			switch (customType)
			{
			case CustomType.SKIN_COLOR:
			case CustomType.HAIR_COLOR:
			{
				Color customizeColor = mHeroMgr.GetCustomizeColor(_race, customType, 0);
				if (customizeColor != Color.clear)
				{
					SetHeroColor(customType, customizeColor);
				}
				break;
			}
			case CustomType.FACE:
			{
				HeroMgr.HeroCustomizeData customizeFace = mHeroMgr.GetCustomizeFace(_race, _gender, 0);
				if (customizeFace != null)
				{
					SetItem(customizeFace, customType);
				}
				break;
			}
			case CustomType.HAIR:
			{
				HeroMgr.HeroCustomizeData customizeHair = mHeroMgr.GetCustomizeHair(_race, _gender, 0);
				if (customizeHair != null)
				{
					SetItem(customizeHair, customType);
				}
				break;
			}
			default:
			{
				HeroMgr.HeroCustomizeData defaultItem = mHeroMgr.GetDefaultItem(_race, _gender, customType);
				if (defaultItem != null)
				{
					SetItem(defaultItem, customType);
				}
				break;
			}
			case CustomType.SPECIALITY:
				break;
			}
		}
	}

	private void SetHeroColor(CustomType _type, Color _color)
	{
		string[] customizePart = GetCustomizePart(_type);
		string[] array = customizePart;
		foreach (string part in array)
		{
			SkinnedMeshRenderer heroRenderer = GetHeroRenderer(part);
			if (null == heroRenderer)
			{
				Log.Error("null skinned mesh renderer");
				break;
			}
			heroRenderer.material.color = _color;
		}
	}

	private string[] GetCustomizePart(CustomType _type)
	{
		if (mCustomizeParts.ContainsKey(_type))
		{
			return mCustomizeParts[_type];
		}
		return new string[0];
	}

	public void SetItem(HeroMgr.HeroCustomizeData _customizeData, CustomType _type)
	{
		if (_customizeData == null)
		{
			return;
		}
		string[] customizePart = GetCustomizePart(_type);
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		string[] array = customizePart;
		foreach (string text in array)
		{
			skinnedMeshRenderer = GetHeroRenderer(text);
			if (null == skinnedMeshRenderer)
			{
				Log.Error("null skinned mesh renderer : " + text);
				return;
			}
			Customize component = skinnedMeshRenderer.gameObject.GetComponent<Customize>();
			if (null == component)
			{
				Log.Error("null Customize");
				return;
			}
			component.SetMesh(_customizeData.mItemMesh);
			if (null != _customizeData.mItemTexture2)
			{
				skinnedMeshRenderer.material.shader = Shader.Find("Tanat/Heroes/ModCombinerClother");
				skinnedMeshRenderer.material.SetTexture("_SkinTex", _customizeData.mItemTexture1);
				skinnedMeshRenderer.material.SetTexture("_WearTex", _customizeData.mItemTexture2);
			}
			else
			{
				skinnedMeshRenderer.material.shader = Shader.Find("Tanat/Heroes/ModCombinerSkin");
				skinnedMeshRenderer.material.SetTexture("_SkinTex", _customizeData.mItemTexture1);
			}
		}
		if (_type == CustomType.HELM)
		{
			bool enabled = _customizeData.mItemMesh == null || (_customizeData.mItemMesh != null && mHairsHelms.Contains(_customizeData.mItemMesh.name));
			customizePart = GetCustomizePart(CustomType.HAIR);
			string[] array2 = customizePart;
			foreach (string part in array2)
			{
				skinnedMeshRenderer = GetHeroRenderer(part);
				skinnedMeshRenderer.enabled = enabled;
			}
		}
		if (mVisualEffectOptions != null)
		{
			mVisualEffectOptions.UpdateCatchRenderers();
		}
	}

	private SkinnedMeshRenderer GetHeroRenderer(string _part)
	{
		string name = "WearMeshes/" + _part;
		Transform transform = base.transform.Find(name);
		if (transform == null)
		{
			Log.Error("No hero renderer with name : " + _part);
			return null;
		}
		return transform.gameObject.GetComponent<SkinnedMeshRenderer>();
	}
}
