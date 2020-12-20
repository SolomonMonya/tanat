using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class VisualBattle
{
	private Battle mBattle;

	private VisualEffectsMgr mVisualEffectsMgr;

	private IStoreContentProvider<IGameObject> mGameObjProv;

	private IPlayerControl mPlayerCtrl;

	private AriseTextMgr mAriseTextMgr;

	private KillEventManager mKillEventMgr;

	private bool mAvatarLight;

	private System.Random mRnd = new System.Random();

	private BattlePacketManager mPacketMgr;

	private PlayerStore mPlayerStore;

	private Dictionary<int, Skill.EffectParams> mEffects = new Dictionary<int, Skill.EffectParams>();

	public bool AvatarLight
	{
		set
		{
			mAvatarLight = value;
			if (mPacketMgr != null)
			{
				if (mAvatarLight)
				{
					BattlePacketManager battlePacketManager = mPacketMgr;
					battlePacketManager.mVisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(battlePacketManager.mVisibleCallback, new BattlePacketManager.SetObjStateCallback(OnLightVisibility));
				}
				else
				{
					BattlePacketManager battlePacketManager2 = mPacketMgr;
					battlePacketManager2.mVisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(battlePacketManager2.mVisibleCallback, new BattlePacketManager.SetObjStateCallback(OnLightVisibility));
				}
			}
		}
	}

	public VisualBattle(Battle _battle, KillEventManager _killEventMgr, IPlayerControl _playerCtrl)
	{
		mBattle = _battle;
		mVisualEffectsMgr = VisualEffectsMgr.Instance;
		mGameObjProv = mBattle.GetGameObjProvider();
		mPlayerCtrl = _playerCtrl;
		mKillEventMgr = _killEventMgr;
		mAriseTextMgr = new AriseTextMgr();
	}

	public void Subscribe(BattlePacketManager _packetManager, PlayerStore _plStore)
	{
		mPacketMgr = _packetManager;
		_packetManager.HandlerMgr.Subscribe<ActionArg>(BattleCmdId.ACTION, null, null, OnAction);
		_packetManager.HandlerMgr.Subscribe<ActionDoneArg>(BattleCmdId.ACTION_DONE, null, null, OnActionDone);
		_packetManager.HandlerMgr.Subscribe<EffectStartArg>(BattleCmdId.EFFECT_START, null, null, OnEffectStart);
		_packetManager.HandlerMgr.Subscribe<EffectEndArg>(BattleCmdId.EFFECT_END, null, null, OnEffectEnd);
		_packetManager.HandlerMgr.Subscribe<SetProjectileArg>(BattleCmdId.SET_PROJECTILE, null, null, OnSetProjectile);
		_packetManager.HandlerMgr.Subscribe<KillArg>(BattleCmdId.ON_KILL, null, null, OnKill);
		_packetManager.HandlerMgr.Subscribe<LevelUpArg>(BattleCmdId.LEVEL_UP, null, null, OnLevelUp);
		_packetManager.HandlerMgr.Subscribe<ReceiveHitArg>(BattleCmdId.RECEIVE_HIT, null, null, OnReceiveHit);
		_packetManager.HandlerMgr.Subscribe<SetMoneyArg>(BattleCmdId.SET_MONEY, null, null, OnSetMoney);
		_packetManager.HandlerMgr.Subscribe<DeleteObjectArg>(BattleCmdId.DELETE_OBJECT, null, null, OnDeleteObject);
		_packetManager.HandlerMgr.Subscribe<BattleEndArg>(BattleCmdId.BATTLE_END, null, null, OnBattleEnd);
		mPlayerStore = _plStore;
		mPlayerStore.SubscribeOnKill(OnPlayerKill);
	}

	public void Unsubscribe()
	{
		if (mPacketMgr != null)
		{
			mPacketMgr.HandlerMgr.Unsubscribe(this);
			if (mAvatarLight)
			{
				BattlePacketManager battlePacketManager = mPacketMgr;
				battlePacketManager.mVisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(battlePacketManager.mVisibleCallback, new BattlePacketManager.SetObjStateCallback(OnLightVisibility));
			}
			mPacketMgr = null;
		}
		if (mPlayerStore != null)
		{
			mPlayerStore.UnsubscribeOnKill(OnPlayerKill);
			mPlayerStore = null;
		}
	}

	public void TryAddLight()
	{
		if (!mAvatarLight)
		{
			return;
		}
		foreach (IGameObject item in mGameObjProv.Content)
		{
			OnLightVisibility(item);
		}
	}

	private void OnLightVisibility(int _objId)
	{
		IGameObject obj = mGameObjProv.Get(Math.Abs(_objId));
		OnLightVisibility(obj);
	}

	private void OnLightVisibility(IGameObject _obj)
	{
		if (_obj != null && _obj.Data.IsPlayerBinded)
		{
			if (_obj.Data.Params.IsTeamInited)
			{
				TryLight(_obj);
			}
			else
			{
				_obj.Data.SubscribeNextSyncCallback(SyncType.TEAM, OnTeamSync);
			}
		}
	}

	private void OnTeamSync(SyncType _syncType, InstanceData _data)
	{
		IGameObject obj = mGameObjProv.Get(_data.Id);
		TryLight(obj);
	}

	private void TryLight(IGameObject _obj)
	{
		if (_obj.Data.TryGetPlayer != null && _obj.Data.TryGetPlayer.IsSelf)
		{
			GameObject gameObject = (_obj as GameData).gameObject;
			if (gameObject.transform.FindChild("VFX_Holder_Light") == null)
			{
				VisualEffectsMgr.Instance.PlayEffect("VFX_Avatar_Light", gameObject);
			}
		}
	}

	private void OnAction(ActionArg _arg)
	{
		GameData gameData = mGameObjProv.Get(_arg.mObjId) as GameData;
		if (gameData == null)
		{
			return;
		}
		IGameObject gameObject2;
		if (mBattle.SelfPlayer.Player == null)
		{
			IGameObject gameObject = null;
			gameObject2 = gameObject;
		}
		else
		{
			gameObject2 = mBattle.SelfPlayer.Player.Avatar;
		}
		IGameObject gameObject3 = gameObject2;
		if (gameData.Data.IsEnemy() && gameObject3 != null && _arg.mTargetId == gameObject3.Id && _arg.mActionId == gameObject3.Data.AttackActionId && OptionsMgr.mShowAttackers)
		{
			VisualEffectOptions component = gameData.gameObject.GetComponent<VisualEffectOptions>();
			if (component != null)
			{
				component.SetAttackSelection(_attacker: true);
			}
		}
		NetSyncTransform netTransform = gameData.NetTransform;
		if (!(netTransform != null))
		{
			return;
		}
		if (_arg.mTargetId != -1)
		{
			IGameObject gameObject4 = mGameObjProv.Get(_arg.mTargetId);
			if (gameObject4 != null)
			{
				GameObject gameObject5 = (gameObject4 as GameData).gameObject;
				Vector3 position = gameObject5.transform.position;
				netTransform.RotateTo(position.x, position.z, _fast: true);
			}
		}
		else
		{
			if (_arg.mActionId == -1)
			{
				return;
			}
			Effector selfEffectorByProto = gameData.Data.GetSelfEffectorByProto(_arg.mActionId);
			if (selfEffectorByProto != null && selfEffectorByProto.Child != null)
			{
				int leveledValue = selfEffectorByProto.Child.GetLeveledValue("target");
				if (TargetValidator.IsPointTarget(leveledValue))
				{
					netTransform.RotateTo(_arg.mTargetX, _arg.mTargetY, _fast: true);
				}
			}
		}
	}

	private void OnActionDone(ActionDoneArg _arg)
	{
		IGameObject gameObject = mGameObjProv.Get(_arg.mObjId);
		if (gameObject == null)
		{
			return;
		}
		GameData gameData = gameObject as GameData;
		if (gameData != null)
		{
			VisualEffectOptions component = gameData.gameObject.GetComponent<VisualEffectOptions>();
			if (component != null)
			{
				component.SetAttackSelection(_attacker: false);
			}
		}
		if (_arg.mItem)
		{
			return;
		}
		float num = _arg.mCooldown - mBattle.Time;
		if (gameObject.Data.IsPlayerBinded && gameObject.Data.Player != null && gameObject.Data.Player.IsSelf)
		{
			if (num > 0f)
			{
				double totalSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
				double endTime = totalSeconds + (double)num;
				gameObject.Data.AddActionCooldown(_arg.mActionId, totalSeconds, endTime);
			}
			else
			{
				gameObject.Data.AddActionCooldown(_arg.mActionId, 0.0, 0.0);
			}
		}
	}

	private void OnEffectStart(EffectStartArg _arg)
	{
		//Discarded unreachable code: IL_00e9
		BattleData.EffectHolder effectHolder = mVisualEffectsMgr.GetEffectHolder(_arg.mFx);
		if (effectHolder == null)
		{
			Log.Warning("effect " + _arg.mEffectId + " unknown fx " + _arg.mFx);
		}
		Skill.EffectParams effectParams = new Skill.EffectParams();
		effectParams.mEffect = effectHolder;
		effectParams.mOwnerObjId = _arg.mOwnerId;
		if (_arg.mArgs != null)
		{
			if (_arg.mArgs.Associative.TryGetValue("target", out var value))
			{
				try
				{
					effectParams.mTargetObjId = value;
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("OnEffectStart fx : ", _arg.mFx, " id :  ValueType ", value.ValueType, " exception : ", ex.Message));
					return;
				}
			}
			if (_arg.mArgs.Associative.TryGetValue("targetPos", out var value2))
			{
				MixedArray mixedArray = value2;
				float x = mixedArray["x"];
				float z = mixedArray["y"];
				effectParams.mTargetPos = new Vector3(x, HeightMap.GetY(x, z), z);
			}
		}
		AddEffect(_arg.mEffectId, effectParams);
		Skill.StartEffects(_done: false, effectParams, mGameObjProv);
		effectParams.mOwnerObj = null;
		effectParams.mTargetObj = null;
	}

	private void OnEffectEnd(EffectEndArg _arg)
	{
		Skill.EffectParams effect = GetEffect(_arg.mEffectId);
		if (effect == null)
		{
			Log.Error("invalid effect " + _arg.mEffectId);
			return;
		}
		try
		{
			Skill.StopEffects(effect, mGameObjProv);
			Skill.StartEffects(_done: true, effect, mGameObjProv);
			effect.mOwnerObj = null;
			effect.mTargetObj = null;
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message);
		}
		finally
		{
			RemoveEffect(_arg.mEffectId);
		}
	}

	private void OnSetProjectile(SetProjectileArg _arg)
	{
		if (_arg.mSourceObjId == -1 || _arg.mTargetId == -1)
		{
			return;
		}
		GameData gameData = mGameObjProv.Get(_arg.mSourceObjId) as GameData;
		if (gameData == null)
		{
			return;
		}
		GameData gameData2 = mGameObjProv.Get(_arg.mTargetId) as GameData;
		if (gameData2 == null)
		{
			return;
		}
		float num = _arg.mHitTime - mBattle.Time;
		if (num > 0f)
		{
			VisualEffectOptions component = gameData.transform.gameObject.GetComponent<VisualEffectOptions>();
			if (component == null)
			{
				Log.Error("Try to shot projectile from object with no VisualEffectOptions : " + _arg.mSourceObjId);
			}
			else
			{
				component.ShotProjectile(gameData2.transform, num);
			}
		}
	}

	private void OnKill(KillArg _arg)
	{
		if (!mPlayerCtrl.IsValid())
		{
			return;
		}
		GameData gameData = mGameObjProv.Get(_arg.mVictimId) as GameData;
		if (gameData == null)
		{
			return;
		}
		int avatarObjId = mPlayerCtrl.SelfPlayer.Player.AvatarObjId;
		if (avatarObjId == _arg.mKillerId && gameData.Proto.Avatar != null)
		{
			UserLog.AddAction(UserActionType.KILL);
			mPlayerCtrl.PlaySound(SoundEmiter.SoundType.KILL);
		}
		if (avatarObjId == _arg.mVictimId)
		{
			mPlayerCtrl.SetSelection(-1, _send: true);
			mPlayerCtrl.RemoveActiveAbility();
			GameData gameData2 = mGameObjProv.Get(_arg.mKillerId) as GameData;
			if (gameData2 != null && OptionsMgr.mPlayAvatarVoices)
			{
				SoundEmiter component = gameData2.GetComponent<SoundEmiter>();
				if (component != null)
				{
					SoundEmiter.SoundSet soundSet = null;
					int num = 0;
					foreach (SoundEmiter.SoundSet mSoundSet in component.mSoundSets)
					{
						if (mSoundSet.mSoundType != SoundEmiter.SoundType.KILL)
						{
							continue;
						}
						soundSet = mSoundSet;
						foreach (SoundEmiter.SoundElement mElement in mSoundSet.mElements)
						{
							num += mElement.mSoundPriority;
						}
					}
					if (soundSet != null && soundSet.mElements.Count > 0)
					{
						int num2 = mRnd.Next(100);
						if (soundSet.mProbability >= num2)
						{
							SoundEmiter.SoundElement soundElement = soundSet.mElements[0];
							num2 = mRnd.Next(num);
							for (int i = 0; i < soundSet.mElements.Count; i++)
							{
								if (num2 <= soundSet.mElements[i].mSoundPriority)
								{
									soundElement = soundSet.mElements[i];
									break;
								}
								num2 -= soundSet.mElements[i].mSoundPriority;
							}
							SoundSystem.Sound sound = new SoundSystem.Sound();
							sound.mName = soundElement.mSoundName;
							sound.mPath = soundElement.mSoundPath;
							sound.mOptions = soundElement.mOptions;
							if (!string.IsNullOrEmpty(sound.mName))
							{
								SoundSystem.Instance.PlaySound(sound, SoundSystem.GetAudioListenerObj());
							}
						}
					}
				}
			}
		}
		if (gameData.Animation != null)
		{
			gameData.Animation.PlayAction(AnimationExt.State.death);
		}
		EffectEmiter component2 = gameData.gameObject.GetComponent<EffectEmiter>();
		if (component2 != null)
		{
			component2.WaitReborn();
		}
		DeathBehaviour.Die(gameData.gameObject);
		if (gameData.NetTransform != null)
		{
			gameData.NetTransform.enabled = false;
		}
		CursorDetector component3 = gameData.gameObject.GetComponent<CursorDetector>();
		if (component3 != null)
		{
			component3.enabled = false;
		}
		ShortObjectInfo component4 = gameData.gameObject.GetComponent<ShortObjectInfo>();
		if (component4 != null)
		{
			component4.enabled = false;
		}
		if (gameData.Proto.Avatar == null)
		{
			Collider[] array = ((!(gameData.gameObject.GetComponent<PhysicalDeath>() == null)) ? gameData.gameObject.GetComponents<Collider>() : gameData.gameObject.GetComponentsInChildren<Collider>());
			Collider[] array2 = array;
			foreach (Collider obj in array2)
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}

	private void OnPlayerKill(Player _victim, int _killerId)
	{
		if (_killerId > 0)
		{
			Player killer = mPlayerStore.Get(_killerId);
			mKillEventMgr.RegNewKill(killer, _victim, mBattle.GetTimer().Time);
		}
		else
		{
			mKillEventMgr.RegNewKill(null, _victim, mBattle.GetTimer().Time);
		}
	}

	private void OnDeleteObject(DeleteObjectArg _arg)
	{
		if (mPlayerCtrl.SelectedObjId == _arg.mObjId)
		{
			mPlayerCtrl.SetSelection(-1, _send: true);
		}
	}

	private void OnLevelUp(LevelUpArg _arg)
	{
		GameData gameData = mGameObjProv.Get(_arg.mObjId) as GameData;
		if (!(gameData == null))
		{
			EffectEmiter component = gameData.gameObject.GetComponent<EffectEmiter>();
			if (component != null)
			{
				component.PlayLevelUp();
			}
		}
	}

	private void OnReceiveHit(ReceiveHitArg _arg)
	{
		GameData gameData = mGameObjProv.Get(_arg.mVictimObjId) as GameData;
		if (gameData == null || !VisibilityMgr.IsVisible(gameData.gameObject))
		{
			return;
		}
		if (_arg.mHitParamsMask == 2 && (int)_arg.mDamage > 0)
		{
			string text = Math.Abs((int)_arg.mDamage).ToString();
			text += "!";
			mAriseTextMgr.Show(text, gameData.gameObject, AriseTextMgr.Style.DMG);
		}
		if (_arg.mDamagerObjId == -1)
		{
			return;
		}
		GameData gameData2 = mGameObjProv.Get(_arg.mDamagerObjId) as GameData;
		if (gameData2 == null)
		{
			return;
		}
		VisualEffectOptions component = gameData2.gameObject.GetComponent<VisualEffectOptions>();
		if (component != null)
		{
			string effectByDamageType = VisualEffectsMgr.Instance.GetEffectByDamageType(component.mDamageType);
			if (effectByDamageType != null && string.Empty != effectByDamageType)
			{
				VisualEffectsMgr.Instance.PlayEffect(effectByDamageType, gameData.gameObject);
			}
		}
	}

	private void OnSetMoney(SetMoneyArg _arg)
	{
		if (_arg.mFromObjId != -1)
		{
			GameData gameData = mGameObjProv.Get(_arg.mFromObjId) as GameData;
			if (!(gameData == null) && _arg.mChangedVirtual > 0)
			{
				mAriseTextMgr.Show(_arg.mChangedVirtual.ToString(), gameData.gameObject, new Vector3(0f, -0.5f, 0f), AriseTextMgr.Style.MONEY);
			}
		}
	}

	private void OnBattleEnd(BattleEndArg _arg)
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(NetSyncTransform));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			NetSyncTransform netSyncTransform = @object as NetSyncTransform;
			netSyncTransform.ForceStop();
		}
	}

	private void AddEffect(int _id, Skill.EffectParams _params)
	{
		try
		{
			mEffects.Add(_id, _params);
		}
		catch (ArgumentException)
		{
			Log.Warning("effect " + _id + " already exists");
			mEffects[_id] = _params;
		}
	}

	private Skill.EffectParams GetEffect(int _id)
	{
		mEffects.TryGetValue(_id, out var value);
		return value;
	}

	private void RemoveEffect(int _id)
	{
		mEffects.Remove(_id);
	}
}
