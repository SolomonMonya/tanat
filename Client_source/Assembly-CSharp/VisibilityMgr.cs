using UnityEngine;

public class VisibilityMgr : MonoBehaviour
{
	private int mVisible;

	private bool mRelevant;

	private bool mActivated = true;

	private int mFxLayer;

	private int mFogLayer;

	private int mSelectionLayer;

	private int mIgnoreRaycastLayer;

	private GameData mGameData;

	public void Awake()
	{
		mFogLayer = LayerMask.NameToLayer("Fog");
		mFxLayer = LayerMask.NameToLayer("Fx");
		mIgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		mSelectionLayer = LayerMask.NameToLayer("Selection");
	}

	public bool IsShowed()
	{
		return mVisible > 0;
	}

	public bool IsActivated()
	{
		return mActivated;
	}

	public void MakeVisible()
	{
		mVisible++;
		if (mActivated)
		{
			mRelevant = false;
		}
	}

	public void MakeInvisible()
	{
		mVisible--;
		mRelevant = false;
	}

	public void OnBecameVisible()
	{
		MakeVisible();
	}

	public void OnBecameInvisible()
	{
		MakeInvisible();
	}

	public void Update()
	{
		if (mGameData == null)
		{
			mGameData = GetComponent<GameData>();
		}
		if (!mRelevant)
		{
			UpdateState();
			mRelevant = true;
		}
	}

	public void Activate()
	{
		mActivated = true;
		base.gameObject.active = false;
		base.gameObject.active = true;
	}

	public void Deactivate()
	{
		mActivated = false;
		SetActiveChilds(_active: false, base.gameObject);
	}

	private void UpdateState()
	{
		SetActiveChilds(IsShowed(), base.gameObject);
	}

	private void SetActiveChilds(bool _active, GameObject _go)
	{
		foreach (Transform item in _go.transform)
		{
			SetActive(_active, item.gameObject);
		}
	}

	private bool IsFunctional()
	{
		if (mGameData == null)
		{
			return false;
		}
		if (mGameData.Data == null)
		{
			return false;
		}
		return mGameData.Data != null;
	}

	private void SetActive(bool _active, GameObject _go)
	{
		if (_go.layer == mSelectionLayer)
		{
			return;
		}
		if (_go.layer != mFogLayer && _go.layer != mFxLayer && _go.layer != mIgnoreRaycastLayer)
		{
			if (_go.animation != null && mGameData != null)
			{
				_go.active = (_active ? _active : IsFunctional());
			}
			else
			{
				_go.active = _active;
			}
		}
		foreach (Transform item in _go.transform)
		{
			SetActive(_active, item.gameObject);
		}
	}

	public static bool IsVisible(GameObject _obj)
	{
		VisibilityMgr component = _obj.GetComponent<VisibilityMgr>();
		return !(component != null) || component.IsShowed();
	}
}
