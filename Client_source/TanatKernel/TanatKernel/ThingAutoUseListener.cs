using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class ThingAutoUseListener
	{
		private delegate bool CheckParamFunc(float _val);

		private Dictionary<string, CheckParamFunc> mCheckFuncs = new Dictionary<string, CheckParamFunc>();

		private SelfPlayer mSelfPlayer;

		private SyncedParams mAvatarParams;

		public ThingAutoUseListener(SelfPlayer _selfPlayer)
		{
			if (_selfPlayer == null)
			{
				throw new ArgumentNullException("_selfPlayer");
			}
			mSelfPlayer = _selfPlayer;
			mCheckFuncs["Health"] = CheckHealth;
			mCheckFuncs["Mana"] = CheckMana;
		}

		public bool GetAutoUse(int _itemId)
		{
			return mSelfPlayer.BattleInventory.Get(_itemId)?.AutoUse ?? false;
		}

		public bool SetAutoUse(int _itemId, bool _autoUse)
		{
			BattleThing battleThing = mSelfPlayer.BattleInventory.Get(_itemId);
			if (battleThing == null)
			{
				return false;
			}
			battleThing.AutoUse = _autoUse;
			return battleThing.AutoUse;
		}

		public bool CheckAutoUse(int _itemId)
		{
			BattleThing battleThing = mSelfPlayer.BattleInventory.TryGet(_itemId);
			if (battleThing == null)
			{
				return false;
			}
			return CheckAutoUse(battleThing);
		}

		public bool CheckAutoUse(BattleThing _item)
		{
			if (_item == null)
			{
				throw new ArgumentNullException("_item");
			}
			if (!_item.AutoUse)
			{
				return false;
			}
			if (_item.CtrlProto.Article.mCooldown > 0f)
			{
				double cooldownProgress = mSelfPlayer.BattleInventory.GetCooldownProgress(_item.CtrlProto);
				if (cooldownProgress >= 0.0 && cooldownProgress <= 1.0)
				{
					return false;
				}
			}
			Player player = mSelfPlayer.Player;
			if (player == null)
			{
				return false;
			}
			if (!player.IsAvatarBinded)
			{
				return false;
			}
			IGameObject avatar = player.Avatar;
			if (avatar == null)
			{
				return false;
			}
			mAvatarParams = avatar.Data.Params;
			bool result = false;
			foreach (KeyValuePair<string, float> mActivator in _item.CtrlProto.Article.mActivators)
			{
				if (mCheckFuncs.TryGetValue(mActivator.Key, out var value) && value(mActivator.Value))
				{
					result = true;
					mSelfPlayer.UseItem(_item.Id);
				}
			}
			return result;
		}

		private bool CheckHealth(float _val)
		{
			return mAvatarParams.Health <= (int)_val;
		}

		private bool CheckMana(float _val)
		{
			return mAvatarParams.Mana <= (int)_val;
		}
	}
}
