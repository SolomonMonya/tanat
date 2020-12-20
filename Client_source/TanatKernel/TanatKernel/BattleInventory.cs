using System;
using System.Collections.Generic;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class BattleInventory : Store<BattleThing>
	{
		public delegate void ChangedCallback(BattleInventory _inventory);

		private class CooldownData
		{
			public double mStartTime;

			public double mEndTime;
		}

		public ChangedCallback mChangedCallback;

		private Battle mBattle;

		private SelfPlayer mSelfPlayer;

		private IStoreContentProvider<BattlePrototype> mBattleProtoProv;

		private IStoreContentProvider<CtrlPrototype> mCtrlProtoProv;

		private HandlerManager<BattlePacket, BattleCmdId> mHandlerMgr;

		private Dictionary<int, CooldownData> mGroupCooldowns = new Dictionary<int, CooldownData>();

		private Dictionary<int, CooldownData> mTypeCooldowns = new Dictionary<int, CooldownData>();

		public BattleInventory(Battle _battle, SelfPlayer _selfPlayer, IStoreContentProvider<BattlePrototype> _battleProtoProv, IStoreContentProvider<CtrlPrototype> _ctrlProtoProv)
			: base("BattleInventory")
		{
			if (_selfPlayer == null)
			{
				throw new ArgumentNullException("_selfPlayer");
			}
			if (_battleProtoProv == null)
			{
				throw new ArgumentNullException("_battleProtoProv");
			}
			if (_ctrlProtoProv == null)
			{
				throw new ArgumentNullException("_ctrlProtoProv");
			}
			mBattle = _battle;
			mSelfPlayer = _selfPlayer;
			mBattleProtoProv = _battleProtoProv;
			mCtrlProtoProv = _ctrlProtoProv;
		}

		public void Subscribe(HandlerManager<BattlePacket, BattleCmdId> _handlerMgr)
		{
			if (_handlerMgr == null)
			{
				throw new ArgumentNullException("_handlerMgr");
			}
			mHandlerMgr = _handlerMgr;
			_handlerMgr.Subscribe<AddToInventoryArg>(BattleCmdId.ADD_TO_INVENTORY, null, null, OnAddToInventory);
			_handlerMgr.Subscribe<RemoveFromInventoryArg>(BattleCmdId.REM_FROM_INVENTORY, null, null, OnRemoveFromInventory);
			_handlerMgr.Subscribe<ActionArg>(BattleCmdId.ACTION, null, null, OnAction);
			_handlerMgr.Subscribe<ActionDoneArg>(BattleCmdId.ACTION_DONE, null, null, OnActionDone);
		}

		public void Unsubscribe()
		{
			if (mHandlerMgr != null)
			{
				mHandlerMgr.Unsubscribe(this);
				mHandlerMgr = null;
			}
		}

		private void OnAddToInventory(AddToInventoryArg _arg)
		{
			Log.Info("item " + _arg.mItemObjId + " proto " + _arg.mItemPrototypeId + " count " + _arg.mCount);
			if (_arg.mCount <= 0)
			{
				Log.Warning("items count " + _arg.mCount + " <= 0");
				return;
			}
			BattleThing battleThing = TryGet(_arg.mItemObjId);
			if (battleThing == null)
			{
				BattlePrototype battlePrototype = mBattleProtoProv.Get(_arg.mItemPrototypeId);
				if (battlePrototype != null)
				{
					battleThing = new BattleThing(_arg.mItemObjId, battlePrototype, mCtrlProtoProv);
					battleThing.Add(_arg.mCount - battleThing.Count);
					Add(battleThing);
				}
			}
			else
			{
				battleThing.Add(_arg.mCount);
			}
			if (mChangedCallback != null)
			{
				mChangedCallback(this);
			}
		}

		private void OnRemoveFromInventory(RemoveFromInventoryArg _arg)
		{
			BattleThing battleThing = Get(_arg.mItemObjId);
			if (battleThing != null)
			{
				battleThing.Remove(_arg.mCount);
				if (battleThing.IsEmpty)
				{
					Remove(battleThing.Id);
				}
				if (mChangedCallback != null)
				{
					mChangedCallback(this);
				}
			}
		}

		public double GetCooldownProgress(int _itemId)
		{
			BattleThing battleThing = TryGet(_itemId);
			if (battleThing != null)
			{
				return GetCooldownProgress(battleThing.CtrlProto);
			}
			return -1.0;
		}

		public double GetCooldownProgress(CtrlPrototype _item)
		{
			if (_item == null)
			{
				return -1.0;
			}
			CooldownData value = null;
			double num = 0.0;
			double num2 = 0.0;
			double totalSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
			if (mTypeCooldowns.TryGetValue(_item.Id, out value))
			{
				num = value.mEndTime - totalSeconds;
				num2 = value.mEndTime - value.mStartTime;
			}
			int[] mConsumableGroups = _item.Article.mConsumableGroups;
			if (mConsumableGroups != null)
			{
				int[] array = mConsumableGroups;
				foreach (int key in array)
				{
					if (mGroupCooldowns.TryGetValue(key, out value))
					{
						double num3 = value.mEndTime - totalSeconds;
						if (num3 > num)
						{
							num = num3;
							num2 = value.mEndTime - value.mStartTime;
						}
					}
				}
			}
			if (!(num > 0.0))
			{
				return -1.0;
			}
			return num / num2;
		}

		private void OnAction(ActionArg _arg)
		{
		}

		private void OnActionDone(ActionDoneArg _arg)
		{
			if (mSelfPlayer == null || mSelfPlayer.Player == null || !_arg.mItem || mSelfPlayer.Player.AvatarObjId != _arg.mObjId)
			{
				return;
			}
			CtrlPrototype ctrlPrototype = mCtrlProtoProv.Get(_arg.mActionId);
			if (ctrlPrototype == null)
			{
				return;
			}
			float num = _arg.mCooldown - mBattle.Time;
			if (num > 0f)
			{
				CtrlPrototype ctrlPrototype2 = null;
				if (ctrlPrototype.Article.mInlayId > 0)
				{
					ctrlPrototype2 = mCtrlProtoProv.Get(ctrlPrototype.Article.mInlayId);
				}
				CooldownData cooldownData = new CooldownData();
				cooldownData.mStartTime = DateTime.Now.TimeOfDay.TotalSeconds;
				cooldownData.mEndTime = cooldownData.mStartTime + (double)num;
				mTypeCooldowns[ctrlPrototype.Id] = cooldownData;
				if (ctrlPrototype2 != null)
				{
					mTypeCooldowns[ctrlPrototype2.Id] = cooldownData;
				}
				int[] mConsumableGroups = ctrlPrototype.Article.mConsumableGroups;
				int[] array = mConsumableGroups;
				foreach (int key in array)
				{
					mGroupCooldowns[key] = cooldownData;
				}
			}
		}
	}
}
