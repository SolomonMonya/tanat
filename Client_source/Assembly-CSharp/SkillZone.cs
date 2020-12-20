using TanatKernel;
using UnityEngine;

public class SkillZone
{
	private AssetLoader mLoader;

	private Projector mProj;

	private Vector2 mLastMousePos;

	private GameObject mAttachedObj;

	public float aoeRadius
	{
		get
		{
			return (!(null != mProj)) ? 0f : mProj.orthographicSize;
		}
		set
		{
			if (!(value <= 0f))
			{
				mProj.orthographicSize = value;
			}
		}
	}

	public bool used
	{
		get
		{
			return null != mProj && mProj.gameObject.active;
		}
		set
		{
			if (null != mProj)
			{
				mProj.gameObject.active = value;
			}
		}
	}

	public SkillZone()
	{
		mLoader = AssetLoader.Instance;
	}

	public void Init(string _prefab)
	{
		if (mLoader != null)
		{
			mLoader.LoadAsset(_prefab, typeof(GameObject), new Notifier<ILoadedAsset, object>(OnLoaded, null));
		}
	}

	private void OnLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			GameObject gameObject = Object.Instantiate(_asset.Asset) as GameObject;
			GameObjUtil.TrySetParent(gameObject, "/effects");
			mProj = gameObject.GetComponent<Projector>();
			mProj.orthographicSize = 3f;
			used = false;
		}
	}

	public void Destroy()
	{
		if (null != mProj)
		{
			Object.Destroy(mProj.gameObject);
		}
		mAttachedObj = null;
	}

	public void AttachToObj(GameObject _obj)
	{
		mAttachedObj = _obj;
	}

	public void DetachFromObj()
	{
		mAttachedObj = null;
	}

	public void UpdatePosition()
	{
		if (used)
		{
			if (mAttachedObj != null)
			{
				FollowGameObj(mAttachedObj);
			}
			else
			{
				FollowMouse();
			}
		}
	}

	private void FollowMouse()
	{
		Vector2 vector = Input.mousePosition;
		if (!((mLastMousePos - vector).magnitude < 0.3f))
		{
			mLastMousePos = vector;
			Ray ray = Camera.main.ScreenPointToRay(vector);
			int _mask = 0;
			GameObjUtil.InitLayerMask("lightmapped", ref _mask);
			GameObjUtil.InitLayerMask("terrain", ref _mask);
			if (Physics.Raycast(ray, out var hitInfo, 1024f, _mask))
			{
				mProj.gameObject.transform.position = hitInfo.point;
			}
		}
	}

	private void FollowGameObj(GameObject _obj)
	{
		mProj.gameObject.transform.position = _obj.transform.position;
	}
}
