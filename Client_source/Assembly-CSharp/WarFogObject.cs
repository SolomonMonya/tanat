using TanatKernel;
using UnityEngine;

[AddComponentMenu("War Fog/WarFogObject")]
public class WarFogObject : MonoBehaviour
{
	public float mToFogTime = 2f;

	private float mCurToFogTime;

	private bool mInited;

	private bool mVisible = true;

	private bool mDisable;

	private bool mToFog;

	private float mRadius;

	private float mRadiusK = 8f;

	private InstanceData mObjData;

	private GameObject mFogZoneObject;

	private static string mWarFogZoneFx = "VFX_WarFogZone_prop01";

	private void Update()
	{
		if (mDisable || !mVisible)
		{
			return;
		}
		if (!mInited)
		{
			if (mObjData == null)
			{
				GameData component = base.gameObject.GetComponent<GameData>();
				if (component != null)
				{
					mObjData = component.Data;
					if (component.Proto == null || component.Proto.Shop != null)
					{
						Object.Destroy(this);
					}
				}
				return;
			}
			Friendliness friendliness = mObjData.GetFriendliness();
			float mViewRadius = mObjData.Params.mViewRadius;
			string curGuiSetId = GuiSystem.mGuiSystem.GetCurGuiSetId();
			if (mViewRadius > 0f && (friendliness != 0 || curGuiSetId == "observer"))
			{
				mVisible = friendliness == Friendliness.FRIEND || friendliness == Friendliness.NEUTRAL || curGuiSetId == "observer";
				mRadius = mViewRadius;
				mObjData = null;
				mInited = true;
				if (mVisible)
				{
					AssetLoader.Instance?.LoadAsset(mWarFogZoneFx, typeof(GameObject), new Notifier<ILoadedAsset, object>(OnLoaded, null));
				}
			}
		}
		else if (mToFog && mCurToFogTime > 0f)
		{
			mCurToFogTime -= Time.deltaTime;
			if (mCurToFogTime < 0f)
			{
				mCurToFogTime = 0f;
			}
			SetRadius(mRadius * (mCurToFogTime / mToFogTime));
		}
	}

	private void OnLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success && !(this == null))
		{
			mFogZoneObject = Object.Instantiate(_asset.Asset) as GameObject;
			mFogZoneObject.transform.parent = base.transform;
			mFogZoneObject.transform.localPosition = Vector3.zero;
			SetRadius(mRadius);
		}
	}

	public void ToFog()
	{
		if (mInited && !mDisable && mVisible)
		{
			mToFog = true;
			mCurToFogTime = mToFogTime;
		}
	}

	public void FromFog()
	{
		if (mInited && !mDisable && mVisible)
		{
			mToFog = false;
			SetRadius(mRadius);
		}
	}

	private void SetRadius(float _radius)
	{
		if (!(null == mFogZoneObject))
		{
			Vector3 localScale = Vector3.one * _radius * mRadiusK;
			localScale.x /= base.transform.localScale.x;
			localScale.y /= base.transform.localScale.y;
			localScale.z /= base.transform.localScale.z;
			mFogZoneObject.transform.localScale = localScale;
		}
	}
}
