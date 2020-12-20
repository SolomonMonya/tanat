using System;
using System.Collections.Generic;
using AMF;
using TanatKernel;
using UnityEngine;

public class PlayerControl : IPlayerControl
{
	private abstract class ActiveAbility
	{
		protected int mTargetMask;

		protected float mAoeRadius;

		protected float mDistance;

		public int TargetMask => mTargetMask;

		public float AoeRadius => mAoeRadius;

		public float Distance => mDistance;

		public abstract bool Use(SelfPlayer _selfPlayer, IGameObject _target, float _x, float _y);
	}

	private class ActiveSkill : ActiveAbility
	{
		private Effector mSkill;

		public Effector Skill => mSkill;

		public ActiveSkill(Effector _skill)
		{
			mSkill = _skill;
			mTargetMask = _skill.GetLeveledValue("target");
			if (TargetValidator.IsPointTarget(mTargetMask))
			{
				mAoeRadius = _skill.GetLeveledValue("aoeRadius");
			}
			mDistance = _skill.GetLeveledValue("distance");
		}

		public override bool Use(SelfPlayer _selfPlayer, IGameObject _target, float _x, float _y)
		{
			return _selfPlayer.UseSkill(mSkill, mTargetMask, _target, _x, _y);
		}
	}

	private class ActiveItem : ActiveAbility
	{
		private BattleThing mItem;

		public ActiveItem(BattleThing _item, BattlePrototype.PEffectDesc _props)
		{
			mItem = _item;
			InitTarget(_props);
			if (TargetValidator.IsPointTarget(mTargetMask))
			{
				InitAoeRadius(_props, 0);
			}
		}

		private void InitTarget(BattlePrototype.PEffectDesc _props)
		{
			_props.mAttribs.TryGetValue("target", out var value);
			if (value != null)
			{
				mTargetMask = value;
			}
		}

		private void InitAoeRadius(BattlePrototype.PEffectDesc _props, int _level)
		{
			_props.mAttribs.TryGetValue("aoeRadius", out var value);
			if (value == null)
			{
				return;
			}
			if (value.ValueType == typeof(MixedArray))
			{
				MixedArray mixedArray = value;
				int num = _level - 1;
				if (num >= 0 && num < mixedArray.Dense.Count)
				{
					mAoeRadius = mixedArray.Dense[num];
				}
			}
			else
			{
				mAoeRadius = value;
			}
		}

		public override bool Use(SelfPlayer _selfPlayer, IGameObject _target, float _x, float _y)
		{
			if (TargetValidator.IsNoneTarget(mTargetMask))
			{
				_selfPlayer.UseItem(mItem.Id);
				return true;
			}
			return _selfPlayer.UseItem(mItem, mTargetMask, _target, _x, _y);
		}
	}

	public delegate void SelectionChangedCallback(int _objId);

	public SelectionChangedCallback mSelectionChangedCallback;

	private bool mFriendTarget;

	private bool mUsableTarget;

	private SelfPlayer mSelfPlayer;

	private IStoreContentProvider<IGameObject> mGameObjProv;

	private IStoreContentProvider<BattlePrototype> mBattleProtoProv;

	private VisualEffectsMgr mVisualEffectsMgr;

	private Dictionary<SoundEmiter.SoundType, List<SoundEmiter.SoundElement>> mSounds = new Dictionary<SoundEmiter.SoundType, List<SoundEmiter.SoundElement>>();

	private Dictionary<SoundEmiter.SoundType, int> mProbability = new Dictionary<SoundEmiter.SoundType, int>();

	private Dictionary<SoundEmiter.SoundType, int> mMaxPriority = new Dictionary<SoundEmiter.SoundType, int>();

	private Dictionary<string, int> mSkillMaxPriority = new Dictionary<string, int>();

	private List<AudioClip> mAudio = new List<AudioClip>();

	private System.Random mRnd = new System.Random();

	private Dictionary<SoundEmiter.SoundType, DateTime> mLastSound = new Dictionary<SoundEmiter.SoundType, DateTime>();

	private GameObject mEmitter;

	private int mSelectedObjId = -1;

	private int mMarkedObjId = -1;

	private bool mForceAttack;

	private ActiveAbility mActiveAbility;

	private SkillZone mSkillZone;

	private SkillZone mSkillZoneLine;

	private bool mAoeViewEnabled;

	private GameObject Emitter
	{
		get
		{
			if (mEmitter == null)
			{
				mEmitter = SoundSystem.GetAudioListenerObj();
			}
			return mEmitter;
		}
	}

	public SelfPlayer SelfPlayer => mSelfPlayer;

	public bool IsFriendTarget => mFriendTarget;

	public int SelectedObjId => mSelectedObjId;

	public bool HasSelectedObj => mSelectedObjId != -1;

	public int MarkedObjectId => mMarkedObjId;

	public bool HasMarkedObj => mMarkedObjId != -1;

	public bool ForceAttackMode
	{
		get
		{
			return mForceAttack;
		}
		set
		{
			mForceAttack = value;
		}
	}

	public bool HasActiveAbility => mActiveAbility != null;

	public bool HasActiveSkill => mActiveAbility is ActiveSkill;

	public void Init(SelfPlayer _selfPlayer, IStoreContentProvider<IGameObject> _gameObjProv, IStoreContentProvider<BattlePrototype> _battleProtoProv, VisualEffectsMgr _visualEffectsMgr)
	{
		if (_selfPlayer == null)
		{
			throw new ArgumentNullException("_selfPlayer");
		}
		if (_gameObjProv == null)
		{
			throw new ArgumentNullException("_gameObjProv");
		}
		if (_battleProtoProv == null)
		{
			throw new ArgumentNullException("_battleProtoProv");
		}
		if (_visualEffectsMgr == null)
		{
			throw new ArgumentNullException("_visualEffectsMgr");
		}
		mSelfPlayer = _selfPlayer;
		mGameObjProv = _gameObjProv;
		mBattleProtoProv = _battleProtoProv;
		mVisualEffectsMgr = _visualEffectsMgr;
		mSelfPlayer.SubscribeInvisible(OnInvisible);
		GameData gameData = mGameObjProv.Get(mSelfPlayer.Player.AvatarObjId) as GameData;
		if (gameData != null)
		{
			SoundEmiter component = gameData.GetComponent<SoundEmiter>();
			foreach (SoundEmiter.SoundSet mSoundSet in component.mSoundSets)
			{
				mSounds[mSoundSet.mSoundType] = mSoundSet.mElements;
				mProbability[mSoundSet.mSoundType] = mSoundSet.mProbability;
				foreach (SoundEmiter.SoundElement mElement in mSoundSet.mElements)
				{
					if (!mMaxPriority.ContainsKey(mSoundSet.mSoundType))
					{
						mMaxPriority[mSoundSet.mSoundType] = 0;
					}
					Dictionary<SoundEmiter.SoundType, int> dictionary;
					Dictionary<SoundEmiter.SoundType, int> dictionary2 = (dictionary = mMaxPriority);
					SoundEmiter.SoundType mSoundType;
					SoundEmiter.SoundType key = (mSoundType = mSoundSet.mSoundType);
					int num = dictionary[mSoundType];
					dictionary2[key] = num + mElement.mSoundPriority;
				}
				if (mSoundSet.mSoundType != SoundEmiter.SoundType.SKILL_USE)
				{
					continue;
				}
				foreach (SoundEmiter.SoundElement mElement2 in mSoundSet.mElements)
				{
					if (!mSkillMaxPriority.ContainsKey(mElement2.mSkillId))
					{
						mSkillMaxPriority[mElement2.mSkillId] = 0;
					}
					Dictionary<string, int> dictionary3;
					Dictionary<string, int> dictionary4 = (dictionary3 = mSkillMaxPriority);
					string mSkillId;
					string key2 = (mSkillId = mElement2.mSkillId);
					int num = dictionary3[mSkillId];
					dictionary4[key2] = num + mElement2.mSoundPriority;
				}
			}
		}
		foreach (int value in Enum.GetValues(typeof(SoundEmiter.SoundType)))
		{
			mLastSound[(SoundEmiter.SoundType)value] = DateTime.MinValue;
		}
	}

	public void Uninit()
	{
		if (mSelfPlayer != null)
		{
			mSelfPlayer.UnsubscribeInvisible(OnInvisible);
		}
		mSelectionChangedCallback = null;
		RemoveActiveAbility();
		mSkillZone = null;
		mSkillZoneLine = null;
		UnmarkCurrent();
		UnselectCurrent();
		mSelfPlayer = null;
		mGameObjProv = null;
		mBattleProtoProv = null;
		mVisualEffectsMgr = null;
	}

	public bool IsValid()
	{
		return mSelfPlayer != null;
	}

	private void OnInvisible(int _objId)
	{
		if (mSelfPlayer != null && mSelfPlayer.Player != null)
		{
			IGameObject avatar = mSelfPlayer.Player.Avatar;
			if (avatar != null && avatar.Id == _objId)
			{
				RemoveActiveAbility();
			}
		}
	}

	private bool VisualizeSelection(GameData _gd)
	{
		if (mSelfPlayer == null)
		{
			return false;
		}
		mUsableTarget = _gd.gameObject.GetComponent<TouchableDetector>() != null;
		if (!mUsableTarget)
		{
			mUsableTarget = _gd.gameObject.GetComponent<DropContent>() != null;
		}
		VisualEffectOptions component = _gd.gameObject.GetComponent<VisualEffectOptions>();
		if (component == null)
		{
			return false;
		}
		if (_gd.Proto.Projectile != null || _gd.Proto.Shop != null)
		{
			return false;
		}
		Player player = mSelfPlayer.Player;
		if (player == null)
		{
			return false;
		}
		bool flag = player.AvatarObjId == _gd.Id;
		if (!flag)
		{
			component.ShowSelection(flag, _gd.Data.GetFriendliness());
		}
		mFriendTarget = !_gd.Data.IsEnemy();
		return true;
	}

	private void HideSelection(GameData _gd)
	{
		VisualEffectOptions component = _gd.GetComponent<VisualEffectOptions>();
		if (!(component == null) && mSelfPlayer.Player.AvatarObjId != _gd.Id)
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

	public bool SetSelection(int _objId, bool _showSelection)
	{
		int num = mSelectedObjId;
		mSelectedObjId = _objId;
		if (mSelectionChangedCallback != null && _showSelection)
		{
			mSelectionChangedCallback(mSelectedObjId);
		}
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

	public GameData GetGD(GameObject _go)
	{
		if (_go == null)
		{
			return null;
		}
		GameData component = _go.GetComponent<GameData>();
		if (component != null && mGameObjProv.Exists(component.Id) && component.Data.Relevant && component.Data.Visible)
		{
			return component;
		}
		return null;
	}

	private GameData UpdateSelection(GameObject _go, out bool _changed, bool _send)
	{
		if (_go == null)
		{
			_changed = SetSelection(-1, _send);
			return null;
		}
		GameData component = _go.GetComponent<GameData>();
		if (component != null && mGameObjProv.Exists(component.Id) && component.Data.Relevant && component.Data.Visible)
		{
			_changed = SetSelection(component.Id, _send);
			return component;
		}
		_changed = SetSelection(-1, _send);
		return null;
	}

	public GameData UpdateSelection(GameObject _go)
	{
		bool _changed;
		return UpdateSelection(_go, out _changed, _send: true);
	}

	public void PlaySound(SoundEmiter.SoundType _type)
	{
		if (OptionsMgr.mPlayAvatarVoices && !((DateTime.Now - mLastSound[_type]).TotalSeconds < 10.0))
		{
			SoundSystem.Sound sound = GetSound(_type);
			if (sound != null)
			{
				PlaySound(_type, sound);
				mLastSound[_type] = DateTime.Now;
			}
		}
	}

	public void PlaySound(SoundEmiter.SoundType _type, string _skillName)
	{
		if (OptionsMgr.mPlayAvatarVoices && !((DateTime.Now - mLastSound[_type]).TotalSeconds < 10.0))
		{
			SoundSystem.Sound sound = GetSound(_type, _skillName);
			if (sound != null)
			{
				PlaySound(_type, sound);
				mLastSound[_type] = DateTime.Now;
			}
		}
	}

	private void PlaySound(SoundEmiter.SoundType _type, SoundSystem.Sound _snd)
	{
		if (mAudio.Count > 0)
		{
			SoundEmiter component = Emitter.GetComponent<SoundEmiter>();
			AudioSource[] audio = component.Audio;
			foreach (AudioSource audioSource in audio)
			{
				if (mAudio.Contains(audioSource.clip) && audioSource.isPlaying)
				{
					return;
				}
			}
		}
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>();
		notifier.mData = _snd;
		notifier.mCallback = OnClipReady;
		SoundSystem.Instance.GetClip(_snd.GetName(), _snd.GetPath(), notifier);
	}

	private void OnClipReady(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			SoundSystem.Sound snd = (SoundSystem.Sound)_data;
			AudioClip item = _asset.Asset as AudioClip;
			if (!mAudio.Contains(item))
			{
				mAudio.Add(item);
			}
			SoundSystem.Instance.PlaySound(snd, Emitter);
		}
	}

	private SoundSystem.Sound GetSound(SoundEmiter.SoundType _type)
	{
		if (!mSounds.ContainsKey(_type) || mSounds[_type].Count == 0)
		{
			return null;
		}
		int num = mRnd.Next(100);
		if (mProbability[_type] < num)
		{
			return null;
		}
		SoundEmiter.SoundElement soundElement = mSounds[_type][0];
		num = mRnd.Next(mMaxPriority[_type]);
		for (int i = 0; i < mSounds[_type].Count; i++)
		{
			if (num <= mSounds[_type][i].mSoundPriority)
			{
				soundElement = mSounds[_type][i];
				break;
			}
			num -= mSounds[_type][i].mSoundPriority;
		}
		if (soundElement == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(soundElement.mSoundName))
		{
			return null;
		}
		SoundSystem.Sound sound = new SoundSystem.Sound();
		sound.mName = soundElement.mSoundName;
		sound.mPath = soundElement.mSoundPath;
		sound.mOptions = soundElement.mOptions;
		return sound;
	}

	private SoundSystem.Sound GetSound(SoundEmiter.SoundType _type, string _skillName)
	{
		if (!mSounds.ContainsKey(_type) || mSounds[_type].Count == 0)
		{
			return null;
		}
		if (!mSkillMaxPriority.ContainsKey(_skillName))
		{
			return null;
		}
		int num = mRnd.Next(100);
		if (mProbability[_type] < num)
		{
			return null;
		}
		SoundEmiter.SoundElement soundElement = null;
		num = mRnd.Next(mSkillMaxPriority[_skillName]);
		for (int i = 0; i < mSounds[_type].Count; i++)
		{
			if (mSounds[_type][i].mSkillId == _skillName)
			{
				if (soundElement == null)
				{
					soundElement = mSounds[_type][i];
				}
				if (num <= mSounds[_type][i].mSoundPriority)
				{
					soundElement = mSounds[_type][i];
					break;
				}
				num -= mSounds[_type][i].mSoundPriority;
			}
		}
		if (soundElement == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(soundElement.mSoundName))
		{
			return null;
		}
		SoundSystem.Sound sound = new SoundSystem.Sound();
		sound.mName = soundElement.mSoundName;
		sound.mPath = soundElement.mSoundPath;
		sound.mOptions = soundElement.mOptions;
		return sound;
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
		if (mMarkedObjId != -1)
		{
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
		mUsableTarget = false;
	}

	public void Move(Vector3 _point)
	{
		PlaySound(SoundEmiter.SoundType.RUN);
		mSelfPlayer.Move(_point.x, _point.z);
	}

	public void Stop(Vector3 _point, bool _stop, bool _playEffect)
	{
		if (_playEffect)
		{
			mVisualEffectsMgr.PlayEffect("VFX_TargetIndicator", _point);
		}
		mSelfPlayer.Stop(_stop);
	}

	public bool RemoveForceAttack()
	{
		if (mForceAttack)
		{
			mForceAttack = false;
			return true;
		}
		return false;
	}

	public bool ForceAttack(IGameObject _obj)
	{
		if (!mForceAttack)
		{
			return false;
		}
		mSelfPlayer.Attack(_obj);
		mForceAttack = false;
		return true;
	}

	public bool Attack(IGameObject _obj)
	{
		if (_obj.Data != null && _obj.Data.IsEnemy())
		{
			PlaySound(SoundEmiter.SoundType.ATTACK);
		}
		return mSelfPlayer.Attack(_obj);
	}

	public void SetSkillZone(SkillZone _skillZone)
	{
		mSkillZone = _skillZone;
	}

	public void SetSkillZoneLine(SkillZone _skillZoneLine)
	{
		mSkillZoneLine = _skillZoneLine;
	}

	public void TryUseActiveAbility(float _x, float _y)
	{
		if (mActiveAbility != null)
		{
			IGameObject obj = null;
			if (HasSelectedObj)
			{
				obj = GetSelectedObj();
			}
			TryUseActiveAbility(_x, _y, obj);
		}
	}

	public void TryUseActiveAbilityByPoint(float _x, float _y)
	{
		TryUseActiveAbility(_x, _y, null);
	}

	private void TryUseActiveAbility(float _x, float _y, IGameObject _obj)
	{
		if (mActiveAbility != null && mActiveAbility.Use(mSelfPlayer, _obj, _x, _y))
		{
			ActiveSkill activeSkill = mActiveAbility as ActiveSkill;
			if (activeSkill != null)
			{
				PlaySound(SoundEmiter.SoundType.SKILL_USE, activeSkill.Skill.Parent.Proto.EffectDesc.mDesc.mName);
			}
			RemoveActiveAbility();
		}
	}

	public bool RemoveActiveAbility()
	{
		if (mActiveAbility != null)
		{
			mActiveAbility = null;
			if (mSkillZone != null)
			{
				mSkillZone.used = false;
			}
			HideAoeView();
			return true;
		}
		return false;
	}

	private Effector GetSelfSkillByProto(int _protoId)
	{
		Effector result = null;
		Effector[] avails = mSelfPlayer.GetAvails();
		Effector[] array = avails;
		foreach (Effector effector in array)
		{
			if (effector.Parent.Proto.Id == _protoId)
			{
				result = effector;
				break;
			}
		}
		return result;
	}

	public void SetActiveSkillByProto(int _protoId)
	{
		RemoveActiveAbility();
		RemoveForceAttack();
		Effector selfSkillByProto = GetSelfSkillByProto(_protoId);
		if (selfSkillByProto != null)
		{
			mActiveAbility = new ActiveSkill(selfSkillByProto);
			ActivateAbility();
		}
	}

	private ActiveItem CreateActiveItem(BattleThing _item)
	{
		BattlePrototype battleProto = _item.BattleProto;
		if (battleProto.Tool == null)
		{
			return null;
		}
		if (battleProto.Tool.mAction == 0)
		{
			return null;
		}
		BattlePrototype battlePrototype = mBattleProtoProv.Get(battleProto.Tool.mAction);
		if (battlePrototype == null)
		{
			return null;
		}
		if (battlePrototype.EffectDesc == null)
		{
			return null;
		}
		return new ActiveItem(_item, battlePrototype.EffectDesc);
	}

	public void SetActiveItem(BattleThing _item)
	{
		if (_item == null)
		{
			throw new ArgumentNullException("_item");
		}
		RemoveActiveAbility();
		RemoveForceAttack();
		mActiveAbility = CreateActiveItem(_item);
		if (mActiveAbility != null)
		{
			ActivateAbility();
		}
	}

	private void ActivateAbility()
	{
		if (mActiveAbility == null)
		{
			return;
		}
		if (TargetValidator.IsNoneTarget(mActiveAbility.TargetMask))
		{
			ActiveSkill activeSkill = mActiveAbility as ActiveSkill;
			if (activeSkill != null)
			{
				PlaySound(SoundEmiter.SoundType.SKILL_USE, activeSkill.Skill.Parent.Proto.EffectDesc.mDesc.mName);
			}
			mActiveAbility.Use(mSelfPlayer, null, 0f, 0f);
			RemoveActiveAbility();
		}
		else if (TargetValidator.IsPointTarget(mActiveAbility.TargetMask) && mSkillZone != null && mActiveAbility.AoeRadius > 0f)
		{
			mSkillZone.aoeRadius = mActiveAbility.AoeRadius;
			mSkillZone.used = true;
		}
	}

	public Effector GetActiveSkill()
	{
		return (mActiveAbility as ActiveSkill)?.Skill;
	}

	public void ShowSkillAoeView(int _skillProtoId)
	{
		Effector selfSkillByProto = GetSelfSkillByProto(_skillProtoId);
		if (selfSkillByProto != null)
		{
			ActiveSkill actAbil = new ActiveSkill(selfSkillByProto);
			ShowAoeView(actAbil);
		}
	}

	public void ShowItemAoeView(BattleThing _item)
	{
		ActiveItem activeItem = CreateActiveItem(_item);
		if (activeItem != null)
		{
			ShowAoeView(activeItem);
		}
	}

	private void ShowAoeView(ActiveAbility _actAbil)
	{
		if (mSkillZoneLine != null && !mSkillZoneLine.used && _actAbil.Distance > 0f && _actAbil.Distance < 25f)
		{
			mSkillZoneLine.aoeRadius = _actAbil.Distance;
			mSkillZoneLine.used = true;
			GameData gameData = mSelfPlayer.Player.Avatar as GameData;
			mSkillZoneLine.AttachToObj(gameData.gameObject);
			mAoeViewEnabled = true;
		}
	}

	public void HideAoeView()
	{
		if (mAoeViewEnabled && !HasActiveAbility)
		{
			mSkillZoneLine.DetachFromObj();
			mSkillZoneLine.used = false;
			mAoeViewEnabled = false;
		}
	}

	public CursorType GetBattleCursorType()
	{
		if (mUsableTarget)
		{
			return CursorType.USE;
		}
		if (HasActiveAbility)
		{
			if (((uint)mActiveAbility.TargetMask & (true ? 1u : 0u)) != 0)
			{
				if (mFriendTarget)
				{
					return CursorType.SKILL_POSITIVE;
				}
				return CursorType.SKILL_POSITIVE_BANNED;
			}
			if (mFriendTarget)
			{
				return CursorType.SKILL_NEGATIVE_BANNED;
			}
			return CursorType.SKILL_NEGATIVE;
		}
		if (mForceAttack)
		{
			if (mMarkedObjId == -1)
			{
				return CursorType.SKILL;
			}
			return CursorType.SKILL_NEGATIVE;
		}
		if (mMarkedObjId == -1)
		{
			return CursorType.ARROW;
		}
		if (mFriendTarget)
		{
			return CursorType.ARROW_FRIEND;
		}
		return CursorType.ARROW_ENEMY;
	}

	public CursorType GetCSCursorType()
	{
		if (mUsableTarget)
		{
			return CursorType.USE;
		}
		return CursorType.ARROW;
	}
}
