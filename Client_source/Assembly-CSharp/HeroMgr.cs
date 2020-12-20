using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class HeroMgr : MonoBehaviour
{
	[Serializable]
	public class HeroCustomize
	{
		public List<HeroCustomizeData> mFaces;

		public List<HeroCustomizeData> mHairs;
	}

	[Serializable]
	public class HeroCustomizeData
	{
		public Mesh mItemMesh;

		public Texture mItemTexture1;

		public Texture mItemTexture2;
	}

	[Serializable]
	public class HeroColorData
	{
		public Color mSkinColor;

		public Color mHairColor;
	}

	public class CreateHeroData
	{
		public GameObject mParentObj;

		public HeroView mParams;

		public OnHeroLoaded mOnHeroLoaded;
	}

	public delegate void OnHeroLoaded(GameObject _hero, object _data);

	public HeroCustomize mElfMCustomize;

	public HeroCustomize mHumanMCustomize;

	public HeroCustomize mElfFCustomize;

	public HeroCustomize mHumanFCustomize;

	public List<HeroColorData> mHumanColors;

	public List<HeroColorData> mElfColors;

	private AssetBundle mItemsAssetBundle;

	private AssetLoader mLoader;

	public Color GetCustomizeColor(HeroRace _race, CustomType _type, int _value)
	{
		List<HeroColorData> list = ((_race != HeroRace.HUMAN) ? mElfColors : mHumanColors);
		if (list.Count <= _value)
		{
			Log.Error("non existing human hero color - " + _value);
			return Color.clear;
		}
		if (_type == CustomType.HAIR_COLOR)
		{
			return list[_value].mHairColor;
		}
		return list[_value].mSkinColor;
	}

	public HeroCustomizeData GetCustomizeFace(HeroRace _race, bool _gender, int _num)
	{
		HeroCustomize customize = GetCustomize(_race, _gender);
		return customize.mFaces[_num];
	}

	public HeroCustomizeData GetCustomizeHair(HeroRace _race, bool _gender, int _num)
	{
		HeroCustomize customize = GetCustomize(_race, _gender);
		return customize.mHairs[_num];
	}

	public HeroCustomizeData GetDefaultItem(HeroRace _race, bool _gender, CustomType _type)
	{
		string text = GenerateDefaultItemId(_race, _gender, _type);
		string texId = text;
		return GetItemById(text, texId);
	}

	public HeroCustomizeData GetItemById(string _id, bool _gender, ref CustomType _type)
	{
		string text = GenerateItemId(_id, _gender);
		string texId = GenerateTextureId(_id);
		_type = GetCustomTypeByItemId(text);
		return GetItemById(text, texId);
	}

	private HeroCustomizeData GetItemById(string _itemId, string _texId)
	{
		if (mItemsAssetBundle == null)
		{
			Log.Error("Items not loaded");
			return null;
		}
		Texture2D texture2D = mItemsAssetBundle.Load(_texId, typeof(Texture2D)) as Texture2D;
		UnityEngine.Object[] array = mItemsAssetBundle.LoadAll(typeof(Mesh));
		Mesh mItemMesh = null;
		UnityEngine.Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Mesh mesh = (Mesh)array2[i];
			if (string.Compare(mesh.name, _itemId) == 0)
			{
				mItemMesh = mesh;
				break;
			}
		}
		HeroCustomizeData heroCustomizeData = new HeroCustomizeData();
		heroCustomizeData.mItemMesh = mItemMesh;
		heroCustomizeData.mItemTexture1 = GetItemSkinTexture(_itemId);
		if (heroCustomizeData.mItemTexture1 != null)
		{
			heroCustomizeData.mItemTexture2 = texture2D;
		}
		else
		{
			heroCustomizeData.mItemTexture1 = texture2D;
		}
		return heroCustomizeData;
	}

	public Texture2D GetItemSkinTexture(string _itemId)
	{
		if (mItemsAssetBundle == null)
		{
			Log.Error("Items not loaded");
			return null;
		}
		string defaultItemIdByItemId = GetDefaultItemIdByItemId(_itemId);
		return mItemsAssetBundle.Load(defaultItemIdByItemId, typeof(Texture2D)) as Texture2D;
	}

	public static CustomType GetCustomTypeByItemId(string _id)
	{
		int num = _id.LastIndexOf('_');
		num++;
		return _id.Substring(num, _id.Length - num) switch
		{
			"Body" => CustomType.BODY, 
			"Foots" => CustomType.FOOTS, 
			"Hands" => CustomType.HANDS, 
			"Legs" => CustomType.LEGS, 
			"Belt" => CustomType.BELT, 
			"Shoulders" => CustomType.SHOULDERS, 
			"Helm" => CustomType.HELM, 
			_ => CustomType.NONE, 
		};
	}

	public static string GenerateItemId(string _id, bool _gender)
	{
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		char[] array = _id.ToCharArray();
		foreach (char c in array)
		{
			if (c == '_')
			{
				num2++;
				if (num2 == 2)
				{
					return _id.Substring(0, num) + ((!_gender) ? "_F" : "_M") + _id.Substring(num, _id.Length - num);
				}
			}
			num++;
		}
		return empty;
	}

	public static string GenerateTextureId(string _id)
	{
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		char[] array = _id.ToCharArray();
		foreach (char c in array)
		{
			if (c == '_')
			{
				num2++;
				if (num2 == 2)
				{
					return _id.Substring(0, num) + "_M" + _id.Substring(num, _id.Length - num);
				}
			}
			num++;
		}
		return empty;
	}

	public static string GenerateDefaultItemId(HeroRace _race, bool _gender, CustomType _type)
	{
		string text = "Set_";
		text += ((_race != HeroRace.HUMAN) ? "Elf_" : "Human_");
		text += ((!_gender) ? "F_Base_" : "M_Base_");
		return _type switch
		{
			CustomType.BODY => text += "Body", 
			CustomType.FOOTS => text += "Foots", 
			CustomType.HANDS => text += "Hands", 
			CustomType.LEGS => text += "Legs", 
			_ => string.Empty, 
		};
	}

	public static string GetDefaultItemIdByItemId(string _itemId)
	{
		int num = 0;
		int num2 = 0;
		string text = string.Empty;
		char[] array = _itemId.ToCharArray();
		foreach (char c in array)
		{
			if (c == '_')
			{
				num++;
			}
			if (num == 3)
			{
				text = _itemId.Substring(0, num2 - 1);
				text += "Base";
			}
			if (num == 5)
			{
				text += _itemId.Substring(num2, _itemId.Length - num2);
				break;
			}
			num2++;
		}
		return text;
	}

	public HeroCustomize GetCustomize(HeroRace _race, bool _gender)
	{
		if (_race == HeroRace.HUMAN)
		{
			if (_gender)
			{
				return mHumanMCustomize;
			}
			return mHumanFCustomize;
		}
		if (_gender)
		{
			return mElfMCustomize;
		}
		return mElfFCustomize;
	}

	public static int GetParamValue(HeroView _params, CustomType _type)
	{
		return _type switch
		{
			CustomType.FACE => _params.mFace, 
			CustomType.HAIR => _params.mHair, 
			CustomType.SPECIALITY => _params.mDistMark, 
			CustomType.HAIR_COLOR => _params.mHairColor, 
			CustomType.SKIN_COLOR => _params.mSkinColor, 
			_ => -1, 
		};
	}

	public void CreateHero(HeroRace _race, bool _gender, CreateHeroData _data)
	{
		string empty = string.Empty;
		empty = ((_race != HeroRace.HUMAN) ? (empty + "Elf_") : (empty + "Human_"));
		empty = ((!_gender) ? (empty + "F") : (empty + "M"));
		if (mLoader != null)
		{
			mLoader.LoadAsset(empty, typeof(GameObject), new Notifier<ILoadedAsset, object>(OnHeroLoad, _data));
		}
	}

	public void OnHeroLoad(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			CreateHeroData createHeroData = _data as CreateHeroData;
			GameObject gameObject = UnityEngine.Object.Instantiate(_asset.Asset) as GameObject;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.eulerAngles = Vector3.zero;
			if (createHeroData.mOnHeroLoaded != null)
			{
				createHeroData.mOnHeroLoaded(gameObject, _data);
			}
		}
	}

	public void OnItemsLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (!_success)
		{
			Log.Error("Can't load hero items");
			return;
		}
		mItemsAssetBundle = _asset.Asset as AssetBundle;
		if (mLoader != null)
		{
			mLoader.KeepAlive(_asset);
		}
	}

	public void Init(Config _config)
	{
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>();
		notifier.mCallback = OnItemsLoaded;
		mLoader = AssetLoader.Instance;
		if (mLoader != null)
		{
			mLoader.LoadAsset(_config.DataDir + "Characters/Heroes/Hero.unity3d", typeof(AssetBundle), notifier);
		}
	}
}
