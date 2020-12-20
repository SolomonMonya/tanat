using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class SelfPlayer : IMoneyHolder, ICustomer
	{
		public delegate void InitedCallback(SelfPlayer _selfPlayer);

		public delegate void InventoryChangedCallback(SelfPlayer _selfPlayer);

		public InitedCallback mInitedCallback;

		public InventoryChangedCallback mInventoryChangedCallback;

		private int mId;

		private bool mInited;

		private int mAvatarState;

		private int mPetState;

		private int mVirtualMoney;

		private int mRealMoney;

		private int mSkillPoints;

		private Battle mBattle;

		private SelfPvpQuestStore mSelfPvpQuests;

		private TargetValidator mTargValidator = new TargetValidator();

		private BattleServerConnection mBattleSrv;

		private IStoreContentProvider<Player> mPlayerProv;

		private IStoreContentProvider<Effector> mAffectorProv;

		private HandlerManager<BattlePacket, BattleCmdId> mHandlerMgr;

		private DateTime mLastUseSkillTime;

		private BattleInventory mInventory;

		private StoreContentProvider<BattleThing> mItemProv;

		private ThingAutoUseListener mAutoUseListener;

		private DateTime mLastActionTime;

		public Player Player => mPlayerProv.Get(mId);

		public int PlayerId => mId;

		public bool IsInited => mInited;

		public int AvatarState => mAvatarState;

		public int PetState => mPetState;

		public SelfPvpQuestStore SelfPvpQuests => mSelfPvpQuests;

		public int SkillPoints
		{
			get
			{
				return mSkillPoints;
			}
			set
			{
				mSkillPoints = value;
				if (mSkillPoints < 0)
				{
					Log.Warning("skill points " + mSkillPoints + " < 0");
				}
				else
				{
					Log.Notice("skill points " + mSkillPoints);
				}
			}
		}

		public float UseSkillDeltaTime => (float)DateTime.Now.Subtract(mLastUseSkillTime).TotalSeconds;

		public StoreContentProvider<BattleThing> Inventory
		{
			get
			{
				if (mInventory == null)
				{
					throw new InvalidOperationException("invoke InitInventory first");
				}
				if (mItemProv == null)
				{
					mItemProv = new StoreContentProvider<BattleThing>(mInventory);
				}
				return mItemProv;
			}
		}

		public ICollection<BattleThing> Equipment
		{
			get
			{
				List<BattleThing> list = new List<BattleThing>();
				foreach (BattleThing @object in mInventory.Objects)
				{
					if (@object.Used > 0)
					{
						list.Add(@object);
					}
				}
				return list;
			}
		}

		public BattleInventory BattleInventory => mInventory;

		public DateTime LastActionTime => mLastActionTime;

		public int VirtualMoney => mVirtualMoney;

		public int RealMoney => mRealMoney;

		public TargetValidator TargValidator => mTargValidator;

		public SelfPlayer(int _id, Battle _battle, BattleServerConnection _battleSrv, IStoreContentProvider<Player> _playerProv, IStoreContentProvider<Effector> _affectorProv, IStoreContentProvider<IQuest> _questProv)
		{
			if (_battleSrv == null)
			{
				throw new ArgumentNullException("_battleSrv");
			}
			if (_playerProv == null)
			{
				throw new ArgumentNullException("_playerProv");
			}
			if (_affectorProv == null)
			{
				throw new ArgumentNullException("_affectorProv");
			}
			if (_questProv == null)
			{
				throw new ArgumentNullException("_questProv");
			}
			mBattle = _battle;
			mBattleSrv = _battleSrv;
			mPlayerProv = _playerProv;
			mAffectorProv = _affectorProv;
			mId = _id;
			mSelfPvpQuests = new SelfPvpQuestStore(_questProv);
		}

		public void Subscribe(HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			_handlerMgr.Subscribe<SetStateArg>(BattleCmdId.SET_STATE, null, null, OnSetState);
			_handlerMgr.Subscribe<SetMoneyArg>(BattleCmdId.SET_MONEY, null, null, OnSetMoney);
			_handlerMgr.Subscribe<UpdateSkillArg>(BattleCmdId.UPGRADE_SKILL, null, null, OnUpgradeSkill);
			_handlerMgr.Subscribe<LevelUpArg>(BattleCmdId.LEVEL_UP, null, null, OnLevelUp);
			_handlerMgr.Subscribe<EquipItemArg>(BattleCmdId.EQUIP_ITEM, null, null, OnEquip);
			_handlerMgr.Subscribe<BoughtItemArg>(BattleCmdId.BUY, null, null, OnBought);
			if (mInventory != null)
			{
				mInventory.Subscribe(_handlerMgr);
			}
			mSelfPvpQuests.Subscribe(_handlerMgr);
		}

		public void Unsubscribe()
		{
			mSelfPvpQuests.Unsubscribe();
			if (mInventory != null)
			{
				mInventory.Unsubscribe();
			}
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		public void SubscribeInvisible(BattlePacketManager.SetObjStateCallback _arg)
		{
			if (mBattleSrv != null)
			{
				BattlePacketManager packetMgr = mBattleSrv.PacketMgr;
				packetMgr.mInvisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(packetMgr.mInvisibleCallback, _arg);
			}
		}

		public void UnsubscribeInvisible(BattlePacketManager.SetObjStateCallback _arg)
		{
			if (mBattleSrv != null)
			{
				BattlePacketManager packetMgr = mBattleSrv.PacketMgr;
				packetMgr.mInvisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(packetMgr.mInvisibleCallback, _arg);
			}
		}

		public IStoreContentProvider<ISelfPvpQuest> GetSelfPvpQuestProvider()
		{
			return new StoreContentProvider<ISelfPvpQuest>(mSelfPvpQuests);
		}

		public void Init(IGameObject _go)
		{
			if (_go == null)
			{
				throw new ArgumentNullException("_go");
			}
			_go.Data.SubscribeNextSyncCallback(SyncType.POSITION, OnSelfAvatarFirstPosSync);
			SetState(0);
		}

		private void OnSelfAvatarFirstPosSync(SyncType _syncType, InstanceData _data)
		{
			mInited = true;
			UpdateActionTime();
			Log.Notice("self player inited");
			if (mInitedCallback != null)
			{
				mInitedCallback(this);
			}
		}

		private void OnSetState(SetStateArg _arg)
		{
			if (_arg.mIsPet)
			{
				mPetState = _arg.mStateId;
			}
			else
			{
				mAvatarState = _arg.mStateId;
			}
		}

		private void OnSetMoney(SetMoneyArg _arg)
		{
			mVirtualMoney = _arg.mVirtualMoney;
			mRealMoney = _arg.mRealMoney;
		}

		private void OnUpgradeSkill(UpdateSkillArg _arg)
		{
			SkillPoints--;
			Log.Debug("skill " + _arg.mSkillId + " upgraded");
		}

		private void OnLevelUp(LevelUpArg _arg)
		{
			Player player = Player;
			if (player != null && _arg.mObjId == player.AvatarObjId)
			{
				SkillPoints = _arg.mSkillPoints;
			}
		}

		private void OnEquip(EquipItemArg _arg)
		{
			Log.Debug("item " + _arg.mItemId);
		}

		private void OnBought(BoughtItemArg _arg)
		{
			Log.Debug("item proto: " + _arg.mItemProtoId);
		}

		public void Move(float _x, float _y)
		{
			Player player = Player;
			if (player != null && player.IsAvatarBinded)
			{
				mBattleSrv.SendMovePlayer(_x, _y);
				UpdateActionTime();
			}
		}

		public void Move(float _x, float _y, bool _rel)
		{
			Player player = Player;
			if (player != null && player.IsAvatarBinded)
			{
				mBattleSrv.SendMovePlayer(_x, _y, _rel);
				UpdateActionTime();
			}
		}

		public void Stop(bool _stop)
		{
			Player player = Player;
			if (player != null && player.IsAvatarBinded)
			{
				mBattleSrv.SendStopPlayer(_stop);
				UpdateActionTime();
			}
		}

		public void SetState(int _stateId)
		{
			mBattleSrv.SendPlayerState(_stateId, _isPet: false);
		}

		public void SetPetState(int _petStateId)
		{
			mBattleSrv.SendPlayerState(_petStateId, _isPet: true);
		}

		public bool Teleport(int _x, int _y)
		{
			Player player = Player;
			if (player == null || !player.IsAvatarBinded)
			{
				return false;
			}
			mBattleSrv.SendDoAction(player.AvatarObjId, 31, -1, _x, _y);
			return true;
		}

		private bool PerformAttack(IGameObject _go)
		{
			if (_go == null)
			{
				throw new ArgumentNullException("_go");
			}
			Player player = Player;
			if (player == null || !player.IsAvatarBinded)
			{
				return false;
			}
			if (_go.Proto.Destructible == null)
			{
				return false;
			}
			if (_go.Data.Params.Health == 0)
			{
				return false;
			}
			if (_go.Data.GetFriendliness() != Friendliness.ENEMY)
			{
				return false;
			}
			int attackActionId = player.Avatar.Data.AttackActionId;
			mBattleSrv.SendDoAction(player.AvatarObjId, attackActionId, _go.Id, 0f, 0f);
			return true;
		}

		public bool Attack(IGameObject _go)
		{
			if (!PerformAttack(_go))
			{
				return false;
			}
			UpdateActionTime();
			return true;
		}

		public void UpgradeSkill(int _skillId)
		{
			if (mSkillPoints <= 0)
			{
				Log.Warning("skill points " + mSkillPoints + " <= 0");
			}
			mBattleSrv.SendUpgradeSkill(_skillId, 1);
			UpdateActionTime();
		}

		public void SetBeacon(float _x, float _y)
		{
			mBattleSrv.SendSetBeacon(_x, _y);
		}

		public void Buy(int _shopId, int _sellerId, int _itemId, int _cnt)
		{
			mBattleSrv.SendBuy(_shopId, _sellerId, _itemId, _cnt);
			UpdateActionTime();
		}

		public void PickUp(int _itemId)
		{
			mBattleSrv.SendPickUp(_itemId);
			UpdateActionTime();
		}

		public void DropThing(int _thingId, int _count)
		{
			mBattleSrv.SendDropItem(_thingId, _count);
		}

		public void DropThingByArticle(int _articleId)
		{
			int num = -1;
			foreach (BattleThing @object in mInventory.Objects)
			{
				if (@object.CtrlProto.Id == _articleId)
				{
					num = @object.Id;
					break;
				}
			}
			if (num == -1)
			{
				Log.Warning("cannot find thing in inventory with article id " + _articleId);
			}
			else
			{
				mBattleSrv.SendDropItem(num, 1);
			}
		}

		public bool IsAvailEffector(Effector _effector)
		{
			Player player = Player;
			if (player == null || !player.IsAvatarBinded)
			{
				return false;
			}
			IGameObject avatar = player.Avatar;
			if (avatar == null)
			{
				return false;
			}
			Variable attrib = _effector.GetAttrib("levels");
			if (attrib == null || attrib.ValueType != typeof(MixedArray))
			{
				return false;
			}
			MixedArray mixedArray = attrib;
			if (_effector.Level >= mixedArray.Dense.Count)
			{
				return false;
			}
			int num = mixedArray.Dense[_effector.Level];
			if (avatar.Data.Level >= num)
			{
				return true;
			}
			return false;
		}

		public Effector[] GetUpgradeAvails()
		{
			Player player = Player;
			if (player == null || !player.IsAvatarBinded)
			{
				return new Effector[0];
			}
			IGameObject avatar = player.Avatar;
			if (avatar == null)
			{
				return new Effector[0];
			}
			List<Effector> list = new List<Effector>();
			avatar.Data.GetEffectorsByType(SkillType.SKILL, list);
			avatar.Data.GetEffectorsByType(SkillType.PARAMS, list);
			List<Effector> list2 = new List<Effector>();
			foreach (Effector item in list)
			{
				Variable attrib = item.GetAttrib("levels");
				if (attrib == null || attrib.ValueType != typeof(MixedArray))
				{
					continue;
				}
				MixedArray mixedArray = attrib;
				if (item.Level < mixedArray.Dense.Count)
				{
					int num = mixedArray.Dense[item.Level];
					if (avatar.Data.Level >= num)
					{
						list2.Add(item);
					}
				}
			}
			return list2.ToArray();
		}

		public Effector[] GetAvails()
		{
			Player player = Player;
			if (!player.IsAvatarBinded)
			{
				return new Effector[0];
			}
			IGameObject avatar = player.Avatar;
			List<Effector> list = new List<Effector>();
			avatar.Data.GetEffectorsByType(SkillType.ACTIVE, list);
			avatar.Data.GetEffectorsByType(SkillType.TOGGLE, list);
			List<Effector> list2 = new List<Effector>();
			foreach (Effector item in list)
			{
				list2.Add(item);
			}
			return list2.ToArray();
		}

		private bool DoAction(int _objId, int _actId, int _targetMask, IGameObject _target, float _x, float _y)
		{
			Player player = Player;
			if (player == null || !player.IsAvatarBinded)
			{
				return false;
			}
			if (mTargValidator.IsValidTarget(_targetMask, _target))
			{
				UpdateActionTime();
				if (TargetValidator.IsPointTarget(_targetMask))
				{
					_target?.Data.GetPosition(out _x, out _y);
					mBattleSrv.SendDoAction(_objId, _actId, -1, _x, _y);
				}
				else
				{
					int target = _target?.Id ?? (-1);
					mBattleSrv.SendDoAction(_objId, _actId, target, _x, _y);
				}
				return true;
			}
			return false;
		}

		public bool UseSkill(Effector _skill, int _targetMask, IGameObject _target, float _x, float _y)
		{
			if (_skill == null)
			{
				throw new ArgumentNullException("_skill");
			}
			int avatarObjId = Player.AvatarObjId;
			int id = _skill.Parent.Proto.Id;
			mLastUseSkillTime = DateTime.Now;
			return DoAction(avatarObjId, id, _targetMask, _target, _x, _y);
		}

		public bool UseSkill(Effector _skill, IGameObject _target, float _x, float _y)
		{
			if (_skill == null)
			{
				throw new ArgumentNullException("_skill");
			}
			Variable attrib = _skill.GetAttrib("target");
			if (attrib == null)
			{
				Log.Warning("unknown target at affector " + _skill.Id);
				return false;
			}
			return UseSkill(_skill, attrib, _target, _x, _y);
		}

		public bool UseSkill(Effector _skill)
		{
			return UseSkill(_skill, null, 0f, 0f);
		}

		public bool UseSkill(Effector _skill, IGameObject _target)
		{
			return UseSkill(_skill, _target, 0f, 0f);
		}

		public bool UseSkill(Effector _skill, float _x, float _y)
		{
			return UseSkill(_skill, null, _x, _y);
		}

		public bool UseItem(BattleThing _item, int _targetMask, IGameObject _target, float _x, float _y)
		{
			if (_item == null)
			{
				throw new ArgumentNullException("_item");
			}
			if (TargetValidator.IsNoneTarget(_targetMask))
			{
				_target = Player.Avatar;
			}
			return DoAction(_item.Id, -1, _targetMask, _target, _x, _y);
		}

		public void UseItem(int _itemId)
		{
			Player player = Player;
			if (player != null && player.IsAvatarBinded)
			{
				UpdateActionTime();
				mBattleSrv.SendDoAction(_itemId, -1, Player.AvatarObjId, 0f, 0f);
			}
		}

		public void InitInventory(IStoreContentProvider<BattlePrototype> _battleProtoProv, IStoreContentProvider<CtrlPrototype> _ctrlProtoProv)
		{
			mInventory = new BattleInventory(mBattle, this, _battleProtoProv, _ctrlProtoProv);
			BattleInventory battleInventory = mInventory;
			battleInventory.mAddCallback = (Store<BattleThing>.AddCallback)Delegate.Combine(battleInventory.mAddCallback, new Store<BattleThing>.AddCallback(OnAddToInventory));
			BattleInventory battleInventory2 = mInventory;
			battleInventory2.mChangedCallback = (BattleInventory.ChangedCallback)Delegate.Combine(battleInventory2.mChangedCallback, new BattleInventory.ChangedCallback(OnInventoryChanged));
		}

		private void OnAddToInventory(Store<BattleThing> _inventory, BattleThing _item)
		{
		}

		private void OnInventoryChanged(BattleInventory _inventory)
		{
			if (mInventoryChangedCallback != null)
			{
				mInventoryChangedCallback(this);
			}
		}

		public void SetEquiped(int _itemId)
		{
		}

		public ThingAutoUseListener GetAutoUseListener()
		{
			if (mAutoUseListener == null)
			{
				mAutoUseListener = new ThingAutoUseListener(this);
			}
			return mAutoUseListener;
		}

		private void UpdateActionTime()
		{
			mLastActionTime = DateTime.Now;
		}

		public void MoveAFK(float _x, float _y, bool _rel)
		{
			Player player = Player;
			if (player != null && player.IsAvatarBinded)
			{
				mBattleSrv.SendMovePlayer(_x, _y, _rel);
			}
		}

		public bool AttackAFK(IGameObject _go)
		{
			return PerformAttack(_go);
		}
	}
}
