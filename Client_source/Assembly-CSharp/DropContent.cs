using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class DropContent : MonoBehaviour
{
	private List<DroppedItem> mContent = new List<DroppedItem>();

	private BattleServerConnection mBattleSrv;

	private PlayerControl mPlayerCtrl;

	private VisualEffectsMgr mEffectMgr;

	private DropMenu mDropWnd;

	private static int mOpenedChestId;

	private string mStartedEffect;

	private int mStartedEffectId = -1;

	private GameData mGameData;

	public void Init(BattleServerConnection _battleSrv, PlayerControl _playerCtrl, VisualEffectsMgr _effectMgr)
	{
		if (_battleSrv == null)
		{
			throw new ArgumentNullException("_battleSrv");
		}
		if (_playerCtrl == null)
		{
			throw new ArgumentNullException("_playerCtrl");
		}
		if (_effectMgr == null)
		{
			throw new ArgumentNullException("_effectMgr");
		}
		mGameData = base.gameObject.GetComponent<GameData>();
		if (!(mGameData == null))
		{
			mBattleSrv = _battleSrv;
			mPlayerCtrl = _playerCtrl;
			mEffectMgr = _effectMgr;
			mBattleSrv.SendGetDropInfo(mGameData.Id);
		}
	}

	public void OnSelect()
	{
		if (mBattleSrv != null)
		{
			bool shift = Event.current.shift;
			ShowContent(shift);
		}
	}

	public void SetContent(List<DroppedItem> _items)
	{
		if (_items == null)
		{
			throw new ArgumentNullException("_items");
		}
		mContent.Clear();
		mContent.AddRange(_items);
		bool flag = false;
		int avatarObjId = mPlayerCtrl.SelfPlayer.Player.AvatarObjId;
		foreach (DroppedItem item in mContent)
		{
			if (item.mAllowed.Contains(avatarObjId))
			{
				flag = true;
				break;
			}
		}
		string text = mStartedEffect;
		if (flag)
		{
			mStartedEffect = "VFX_Drop_Chest_prop02";
		}
		else
		{
			mStartedEffect = "VFX_Drop_Chest_prop01";
		}
		if (mStartedEffectId != -1)
		{
			if (text != mStartedEffect)
			{
				mEffectMgr.StopEffect(mStartedEffectId);
				mStartedEffectId = mEffectMgr.PlayEffect(mStartedEffect, base.gameObject);
			}
		}
		else
		{
			mStartedEffectId = mEffectMgr.PlayEffect(mStartedEffect, base.gameObject);
		}
		if (IsCurWndOpened())
		{
			mDropWnd.SetData(mContent);
		}
	}

	public void SetDropWnd(DropMenu _wnd)
	{
		mDropWnd = _wnd;
	}

	public void ShowContent(bool _autoTake)
	{
		if (!(mGameData == null))
		{
			if (mDropWnd == null)
			{
				_autoTake = true;
			}
			if (_autoTake)
			{
				Log.Debug("auto take drop from " + mGameData.Id);
				AutoTake();
			}
			else if (mContent.Count > 0)
			{
				mDropWnd.SetActive(_active: true);
				mDropWnd.SetData(mContent);
				mOpenedChestId = GetInstanceID();
			}
			else
			{
				mDropWnd.Close();
				mOpenedChestId = 0;
			}
		}
	}

	private void AutoTake()
	{
		int avatarObjId = mPlayerCtrl.SelfPlayer.Player.AvatarObjId;
		foreach (DroppedItem item in mContent)
		{
			if (item.mAllowed.Contains(avatarObjId))
			{
				mPlayerCtrl.SelfPlayer.PickUp(item.mId);
				Log.Debug("auto taken item " + item.mId + " (proto " + item.mProtoId + ")");
			}
		}
	}

	private bool IsCurWndOpened()
	{
		return mDropWnd != null && mDropWnd.Active && GetInstanceID() == mOpenedChestId;
	}

	public void OnDisable()
	{
		if (IsCurWndOpened())
		{
			mDropWnd.SetActive(_active: false);
			mOpenedChestId = 0;
		}
		if (mStartedEffectId != -1)
		{
			mEffectMgr.RemoveObjectEffects(base.gameObject);
			mStartedEffectId = -1;
			mStartedEffect = null;
		}
	}

	public void OnEnable()
	{
		if (!string.IsNullOrEmpty(mStartedEffect) && mStartedEffectId == -1)
		{
			mStartedEffectId = mEffectMgr.PlayEffect(mStartedEffect, base.gameObject);
		}
	}

	public void Update()
	{
		if (!(mGameData == null))
		{
			if (!mGameData.Data.Relevant && mStartedEffectId != -1)
			{
				OnDisable();
			}
			if (mGameData.Data.Visible && mStartedEffectId == -1 && mContent.Count > 0)
			{
				SetContent(new List<DroppedItem>(mContent));
			}
		}
	}
}
