using System;
using TanatKernel;
using UnityEngine;

public class GameData : MonoBehaviour, IGameObject, InstanceData.IPosSyncListener, IStorable
{
	public enum MapSymbol
	{
		UNDEFINED,
		UNIT,
		AVATAR,
		BUILDING,
		SHOP
	}

	private BattlePrototype mProto;

	private InstanceData mData;

	private NetSyncTransform mNetTransform;

	private AnimationExt mAnimation;

	private MapSymbol mMapSmb;

	public int Id => mData.Id;

	public BattlePrototype Proto => mProto;

	public InstanceData Data => mData;

	public NetSyncTransform NetTransform => mNetTransform;

	public AnimationExt Animation => mAnimation;

	public MapSymbol Symbol => mMapSmb;

	public void Init(BattlePrototype _proto, InstanceData _data)
	{
		mProto = _proto;
		mData = _data;
		mNetTransform = base.gameObject.GetComponent<NetSyncTransform>();
		if (mNetTransform != null)
		{
			mData.AddPosSyncListener(mNetTransform);
		}
		else
		{
			mData.AddPosSyncListener(this);
		}
		InitAnimation();
	}

	public void InitAnimation()
	{
		mAnimation = GameObjUtil.GetComponentInChildren<AnimationExt>(base.gameObject);
		if (mAnimation != null)
		{
			mAnimation.SetGameData(this);
		}
	}

	public void InitMapSymbol()
	{
		mMapSmb = MapSymbol.UNDEFINED;
		if (mProto.Destructible == null)
		{
			if (mProto.Shop != null)
			{
				mMapSmb = MapSymbol.SHOP;
			}
			else if (mProto.Avatar != null)
			{
				mMapSmb = MapSymbol.AVATAR;
			}
			return;
		}
		mMapSmb = MapSymbol.UNIT;
		if (mProto.Avatar != null)
		{
			mMapSmb = MapSymbol.AVATAR;
		}
		else if (mProto.Building != null)
		{
			if (mProto.Shop != null)
			{
				mMapSmb = MapSymbol.SHOP;
			}
			else
			{
				mMapSmb = MapSymbol.BUILDING;
			}
		}
	}

	public void ProcessNetInput(InstanceData _data)
	{
		SyncedParams @params = _data.Params;
		float mX = @params.mX;
		float mY = @params.mY;
		float y = HeightMap.GetY(mX, mY);
		Vector3 position = new Vector3(mX, y, mY);
		base.gameObject.transform.position = position;
		base.gameObject.transform.right = Vector3.right;
		float num = (0f - @params.mAngle) * 57.29578f;
		base.gameObject.transform.Rotate(Vector3.up * num, Space.World);
	}

	public void ResetSyncData(bool _onVisible)
	{
	}

	public void InstantResetSyncData()
	{
	}

	public static void GetExp(BattlePrototype _proto, int _level, out float _prevLvlExp, out float _nextLvlExp)
	{
		if (_proto == null)
		{
			throw new ArgumentNullException("_proto");
		}
		BattlePrototype.PExperiencer experiencer = _proto.Experiencer;
		if (experiencer == null || experiencer.mLevelsXP == null)
		{
			_prevLvlExp = (_nextLvlExp = 0f);
			return;
		}
		int _level2 = _level;
		ClampLvl(ref _level2, experiencer.mLevelsXP.Length);
		_nextLvlExp = experiencer.mLevelsXP[_level2];
		if (_level == 0)
		{
			_prevLvlExp = 0f;
			return;
		}
		int _level3 = _level2 - 1;
		ClampLvl(ref _level3, experiencer.mLevelsXP.Length);
		_prevLvlExp = experiencer.mLevelsXP[_level3];
	}

	private static void ClampLvl(ref int _level, int _lim)
	{
		if (_level < 0)
		{
			_level = 0;
		}
		else if (_level >= _lim)
		{
			_level = _lim - 1;
		}
	}
}
