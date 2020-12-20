using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ObserverInput : IPlayerControl
{
	private class BattleObjectComparer : IComparer<RaycastHit>
	{
		private Vector3 camPos = Camera.main.transform.position;

		public int Compare(RaycastHit _obj1, RaycastHit _obj2)
		{
			float magnitude = (camPos - _obj1.point).magnitude;
			float magnitude2 = (camPos - _obj2.point).magnitude;
			if (magnitude < magnitude2)
			{
				return -1;
			}
			if (magnitude > magnitude2)
			{
				return 1;
			}
			return 0;
		}
	}

	public delegate void SelectionChangedCallback(int _objId);

	public SelectionChangedCallback mSelectionChangedCallback;

	private IStoreContentProvider<IGameObject> mGameObjProv;

	private CommonInput mCommonInput;

	private int mSelectedObjId = -1;

	private int mMarkedObjId = -1;

	public int SelectedObjId => mSelectedObjId;

	public bool HasSelectedObj => mSelectedObjId != -1;

	public int MarkedObjectId => mMarkedObjId;

	public bool HasMarkedObj => mMarkedObjId != -1;

	public SelfPlayer SelfPlayer => null;

	public void Init(IStoreContentProvider<IGameObject> _gameObjProv)
	{
		if (_gameObjProv == null)
		{
			throw new ArgumentNullException("_gameObjProv");
		}
		mGameObjProv = _gameObjProv;
		mCommonInput = GameObjUtil.FindObjectOfType<CommonInput>();
		if (mCommonInput != null)
		{
			mCommonInput.SubscribeMouseClick(OnMouseClickLeft, OnMouseClickRight);
		}
	}

	public void Uninit()
	{
		if (mCommonInput != null)
		{
			mCommonInput.UnsubscribeMouseClick(OnMouseClickLeft, OnMouseClickRight);
			mCommonInput = null;
		}
		mSelectionChangedCallback = null;
		UnselectCurrent();
		mGameObjProv = null;
	}

	private void OnMouseClickLeft(List<RaycastHit> _hits, EventType _type)
	{
		Vector3 _point = Vector3.zero;
		GameObject clickedObject = GetClickedObject(_hits, ref _point);
		UpdateSelection(clickedObject);
	}

	private void OnMouseClickRight(List<RaycastHit> _hits, EventType _type)
	{
	}

	private GameObject GetClickedObject(List<RaycastHit> _hits, ref Vector3 _point)
	{
		if (_hits.Count == 0)
		{
			return null;
		}
		BattleObjectComparer comparer = new BattleObjectComparer();
		_hits.Sort(comparer);
		_point = _hits[0].point;
		return _hits[0].collider.gameObject;
	}

	private bool VisualizeSelection(GameData _gd)
	{
		VisualEffectOptions component = _gd.gameObject.GetComponent<VisualEffectOptions>();
		if (component == null)
		{
			return false;
		}
		if (_gd.Proto.Projectile != null || _gd.Proto.Shop != null)
		{
			return false;
		}
		component.ShowSelection(_self: false, _gd.Data.GetFriendliness());
		return true;
	}

	private void HideSelection(GameData _gd)
	{
		VisualEffectOptions component = _gd.GetComponent<VisualEffectOptions>();
		if (!(component == null))
		{
			component.HideSelection();
		}
	}

	public IGameObject GetSelectedObj()
	{
		object result;
		if (mSelectedObjId != -1)
		{
			IGameObject gameObject = mGameObjProv.Get(mSelectedObjId);
			result = gameObject;
		}
		else
		{
			result = null;
		}
		return (IGameObject)result;
	}

	public bool SetSelection(int _objId, bool _send)
	{
		int num = mSelectedObjId;
		mSelectedObjId = _objId;
		GameData gameData = null;
		if (mSelectedObjId != -1)
		{
			gameData = mGameObjProv.Get(mSelectedObjId) as GameData;
			if (gameData != null)
			{
				gameData.gameObject.SendMessage("OnSelect", null, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				mSelectedObjId = -1;
			}
		}
		if (num != mSelectedObjId)
		{
			if (num != -1)
			{
				GameData gameData2 = mGameObjProv.TryGet(num) as GameData;
				if (gameData2 != null)
				{
					HideSelection(gameData2);
				}
			}
			if (mSelectedObjId != -1 && !VisualizeSelection(gameData))
			{
				mSelectedObjId = -1;
			}
			if (mSelectionChangedCallback != null)
			{
				mSelectionChangedCallback(mSelectedObjId);
			}
			return true;
		}
		return false;
	}

	public void UnselectCurrent()
	{
		if (mSelectedObjId != -1)
		{
			GameData gameData = mGameObjProv.TryGet(mSelectedObjId) as GameData;
			if (gameData != null)
			{
				HideSelection(gameData);
			}
			mSelectedObjId = -1;
		}
	}

	public GameData UpdateSelection(GameObject _go, out bool _changed)
	{
		if (_go == null)
		{
			_changed = SetSelection(-1, _send: true);
			return null;
		}
		GameData component = _go.GetComponent<GameData>();
		if (component != null && mGameObjProv.Exists(component.Id) && component.Data.Relevant && component.Data.Visible)
		{
			_changed = SetSelection(component.Id, _send: true);
			return component;
		}
		_changed = SetSelection(-1, _send: true);
		return null;
	}

	public GameData UpdateSelection(GameObject _go)
	{
		bool _changed;
		return UpdateSelection(_go, out _changed);
	}

	public IGameObject GetMardedObj()
	{
		object result;
		if (mMarkedObjId != -1)
		{
			IGameObject gameObject = mGameObjProv.Get(mMarkedObjId);
			result = gameObject;
		}
		else
		{
			result = null;
		}
		return (IGameObject)result;
	}

	public GameObject GetMarkedGameObject()
	{
		GameData gameData = GetMardedObj() as GameData;
		return (!(null != gameData)) ? null : gameData.gameObject;
	}

	public void Mark(GameObject _go)
	{
		UnmarkCurrent();
		GameData component = _go.GetComponent<GameData>();
		if (!(component == null) && VisualizeSelection(component))
		{
			mMarkedObjId = component.Id;
		}
	}

	public void UnmarkCurrent()
	{
		if (mMarkedObjId == -1)
		{
			return;
		}
		if (mMarkedObjId != mSelectedObjId)
		{
			GameData gameData = mGameObjProv.TryGet(mMarkedObjId) as GameData;
			if (gameData != null)
			{
				HideSelection(gameData);
			}
		}
		mMarkedObjId = -1;
	}

	public bool IsValid()
	{
		return false;
	}

	public void PlaySound(SoundEmiter.SoundType _type)
	{
	}

	public bool RemoveActiveAbility()
	{
		return true;
	}
}
