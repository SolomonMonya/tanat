using System;
using System.Collections.Generic;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class HeroStore : Store<Hero>
	{
		public delegate void NewItemDressedCallback(Hero _hero, int _slot);

		public delegate void ItemUndressedCallback(Hero _hero, int _slot, CtrlThing _item);

		public delegate void Action();

		public NewItemDressedCallback mNewItemDressedCallback;

		public ItemUndressedCallback mItemUndressedCallback;

		private CtrlServerConnection mCtrlSrv;

		private IStoreContentProvider<CtrlPrototype> mCtrlPrototypes;

		public Dictionary<int, DateTime> mGlobalBuffs = new Dictionary<int, DateTime>();

		public Action mGlobalBuffsReloaded;

		private Dictionary<int, Hero> mHeroesWait = new Dictionary<int, Hero>();

		private Dictionary<int, Notifier<Hero, object>.Group> mHeroDataNotifiers = new Dictionary<int, Notifier<Hero, object>.Group>();

		private Dictionary<int, Notifier<Hero, object>.Group> mGameInfoNotifiers = new Dictionary<int, Notifier<Hero, object>.Group>();

		private Dictionary<int, Notifier<Hero, object>.Group> mPersonalInfoNotifiers = new Dictionary<int, Notifier<Hero, object>.Group>();

		private Dictionary<int, Notifier<Hero, object>.Group> mDressedItemsNotifiers = new Dictionary<int, Notifier<Hero, object>.Group>();

		private IStoreContentProvider<Hero> mContentProv;

		public HeroStore(CtrlServerConnection _ctrlSrv)
			: base("Heroes")
		{
			if (_ctrlSrv == null)
			{
				throw new ArgumentNullException("_ctrlSrv");
			}
			mCtrlSrv = _ctrlSrv;
			mCtrlPrototypes = mCtrlSrv.GetPrototypeProvider();
		}

		public void AddByPlayerId(int _playerId)
		{
			if (!Exists(_playerId))
			{
				Hero obj = new Hero(_playerId);
				Add(obj);
			}
		}

		private void TryAddNotifier(int _heroId, Dictionary<int, Notifier<Hero, object>.Group> _groups, Notifier<Hero, object> _notifier)
		{
			if (_notifier != null)
			{
				if (!_groups.TryGetValue(_heroId, out var value))
				{
					value = new Notifier<Hero, object>.Group();
					_groups.Add(_heroId, value);
				}
				value.Add(_notifier);
			}
		}

		private bool TryNotify(bool _success, Hero _hero, Dictionary<int, Notifier<Hero, object>.Group> _groups)
		{
			if (!_groups.TryGetValue(_hero.Id, out var value))
			{
				return false;
			}
			value.Call(_success, _hero);
			return true;
		}

		public void UpdateHeroData(int _heroId, Notifier<Hero, object> _notifier)
		{
			if (!Exists(_heroId))
			{
				Log.Warning("hero " + _heroId + " not exists");
			}
			TryAddNotifier(_heroId, mHeroDataNotifiers, _notifier);
			mCtrlSrv.HeroSender.DataRequest(_heroId);
		}

		public void UpdateGameInfo(int _heroId, Notifier<Hero, object> _notifier)
		{
			if (!Exists(_heroId))
			{
				Log.Warning("hero " + _heroId + " not exists");
			}
			TryAddNotifier(_heroId, mGameInfoNotifiers, _notifier);
			mCtrlSrv.HeroSender.GameInfoRequest(_heroId);
		}

		public void UpdateHeroesInfo(List<int> _heroId, List<Notifier<Hero, object>> _notifier, List<Notifier<Hero, object>> _wearNotifier, bool _send, int _selfId)
		{
			for (int i = 0; i < _heroId.Count; i++)
			{
				if (!Exists(_heroId[i]))
				{
					Log.Warning("hero " + _heroId[i] + " not exists");
				}
				TryAddNotifier(_heroId[i], mHeroDataNotifiers, _notifier[i]);
				TryAddNotifier(_heroId[i], mDressedItemsNotifiers, _wearNotifier[i]);
				if (!_send && mHeroesWait.ContainsKey(_heroId[i]))
				{
					TryNotify(_success: true, mHeroesWait[_heroId[i]], mHeroDataNotifiers);
					TryNotify(_success: true, mHeroesWait[_heroId[i]], mGameInfoNotifiers);
					mHeroesWait.Remove(_heroId[i]);
				}
			}
			if (_heroId.Contains(_selfId))
			{
				_heroId.Remove(_selfId);
			}
			if (_send)
			{
				mCtrlSrv.HeroSender.GameHeroesRequest(_heroId);
			}
		}

		public void UpdatePersonalInfo(int _heroId, Notifier<Hero, object> _notifier)
		{
			if (!Exists(_heroId))
			{
				Log.Warning("hero " + _heroId + " not exists");
			}
			TryAddNotifier(_heroId, mPersonalInfoNotifiers, _notifier);
			mCtrlSrv.HeroSender.PersonalDetailsRequest(_heroId);
		}

		public void UpdateDressedItems(int _heroId, Notifier<Hero, object> _notifier)
		{
			if (!Exists(_heroId))
			{
				Log.Warning("hero " + _heroId + " not exists");
			}
			TryAddNotifier(_heroId, mDressedItemsNotifiers, _notifier);
			mCtrlSrv.HeroSender.DressedItemsRequest(_heroId);
		}

		public void UpdateDressedItemsList(int _heroId)
		{
			if (!Exists(_heroId))
			{
				Log.Warning("hero " + _heroId + " not exists");
			}
			Hero hero = Get(_heroId);
			TryNotify(_success: true, hero, mDressedItemsNotifiers);
		}

		public void Subscribe()
		{
			HandlerManager<CtrlPacket, Enum> handlerMgr = mCtrlSrv.EntryPoint.HandlerMgr;
			handlerMgr.Subscribe<HeroDataArg>(CtrlCmdId.hero.get_data, null, null, OnHeroData);
			handlerMgr.Subscribe<HeroDataListArg>(CtrlCmdId.hero.get_data_list, null, null, OnHeroDataList);
			handlerMgr.Subscribe<HeroDataListMpdArg>(CtrlCmdId.hero.get_data_list_mpd, null, null, OnHeroDataListBroadCast);
			handlerMgr.Subscribe<HeroDataArg>(CtrlCmdId.common.hero_conf, null, null, OnSelfHeroData);
			handlerMgr.Subscribe<HeroGameInfoArg>(CtrlCmdId.user.game_info, null, null, OnHeroGameInfo);
			handlerMgr.Subscribe<HeroBuffsUpdateMpdArg>(CtrlCmdId.user.update_buffs_mpd, null, null, OnHeroBuffsUpdate);
			handlerMgr.Subscribe<GlobalBuffsUpdateMpdArg>(CtrlCmdId.user.update_global_buffs_mpd, null, null, OnGlobalBuffsUpdate);
			handlerMgr.Subscribe<UserPersonalInfoArg>(CtrlCmdId.user.personal_details, null, null, OnPersonalInfo);
			handlerMgr.Subscribe<DressedItemsArg>(CtrlCmdId.user.dressed_items, null, null, OnDressedItems);
			handlerMgr.Subscribe<NewDressedItemArg>(CtrlCmdId.user.dress_mpd, null, null, OnDressedMpd);
			handlerMgr.Subscribe<UndressedItemArg>(CtrlCmdId.user.undress_mpd, null, null, OnUndressedMpd);
			handlerMgr.Subscribe<HeroesInfoUpdateArg>(CtrlCmdId.user.update_info_mpd, null, null, OnUpdateHeroesInfo);
		}

		public void Unsubscribe()
		{
			mCtrlSrv.EntryPoint.HandlerMgr.Unsubscribe(this);
		}

		public override void Remove(int _id)
		{
			base.Remove(_id);
			if (mHeroesWait.ContainsKey(_id))
			{
				mHeroesWait.Remove(_id);
			}
		}

		public override void Clear()
		{
			base.Clear();
			mHeroesWait.Clear();
			mHeroDataNotifiers.Clear();
			mGameInfoNotifiers.Clear();
			mPersonalInfoNotifiers.Clear();
			mDressedItemsNotifiers.Clear();
		}

		private void OnHeroDataListBroadCast(HeroDataListMpdArg _arg)
		{
			List<Hero> list = PerformHeroData(_arg.mData);
			foreach (Hero item in list)
			{
				mHeroesWait[item.Id] = item;
			}
		}

		private void OnHeroDataList(HeroDataListArg _arg)
		{
			PerformHeroData(_arg);
		}

		private List<Hero> PerformHeroData(HeroDataListArg _arg)
		{
			List<Hero> list = new List<Hero>();
			foreach (HeroDataListArg.HeroDataItem mItem in _arg.mItems)
			{
				if (!mItem.mPersExists)
				{
					continue;
				}
				Hero hero = TryGet(mItem.mHeroId);
				if (hero == null)
				{
					continue;
				}
				hero.SetView(mItem.mView);
				hero.GameInfo.mLevel = mItem.mLevel;
				hero.GameInfo.mExp = mItem.mExp;
				hero.GameInfo.mNextExp = mItem.mNextExp;
				hero.GameInfo.mRating = mItem.mRating;
				hero.SetClanInfo(mItem.mClanId, mItem.mClanTag);
				hero.UndressAll();
				foreach (DressedItemsArg.DressedItem mItem2 in mItem.mItems)
				{
					CtrlPrototype ctrlPrototype = mCtrlPrototypes.Get(mItem2.mArticleId);
					if (ctrlPrototype != null)
					{
						if (ctrlPrototype.Article == null)
						{
							Log.Warning("invalid article " + ctrlPrototype.Id);
							continue;
						}
						CtrlThing ctrlThing = new CtrlThing(mItem2.mId, ctrlPrototype);
						ctrlThing.Add(mItem2.mCount - ctrlThing.Count);
						hero.Dress(mItem2.mSlot, ctrlThing);
					}
				}
				if (!TryNotify(_success: true, hero, mHeroDataNotifiers))
				{
					Log.Debug("Cant find notifier for " + hero.Id);
					list.Add(hero);
				}
				TryNotify(_success: true, hero, mGameInfoNotifiers);
			}
			return list;
		}

		private void OnHeroData(HeroDataArg _arg)
		{
			if (_arg.mPersExists)
			{
				Hero hero = Get(_arg.mHeroId);
				if (hero != null)
				{
					hero.SetView(_arg.mView);
					TryNotify(_arg.mPersExists, hero, mHeroDataNotifiers);
				}
			}
		}

		private void OnSelfHeroData(HeroDataArg _arg)
		{
			Hero hero = new Hero(_arg.mHeroId);
			hero.SetView(_arg.mView);
			Add(hero);
		}

		private void OnHeroGameInfo(HeroGameInfoArg _arg)
		{
			Hero hero = Get(_arg.mHeroId);
			if (hero != null)
			{
				hero.SetGameInfo(_arg.mInfo);
				TryNotify(_success: true, hero, mGameInfoNotifiers);
			}
		}

		private void OnHeroBuffsUpdate(HeroBuffsUpdateMpdArg _args)
		{
			Hero hero = mCtrlSrv.SelfHero.Hero;
			hero.SetBuffs(_args.mBuffs);
		}

		private void OnGlobalBuffsUpdate(GlobalBuffsUpdateMpdArg _args)
		{
			mGlobalBuffs = _args.mBuffs;
			if (mGlobalBuffsReloaded != null)
			{
				mGlobalBuffsReloaded();
			}
		}

		private void OnUpdateHeroesInfo(HeroesInfoUpdateArg _arg)
		{
			foreach (KeyValuePair<int, HeroGameInfo> item in _arg.mInfo)
			{
				TryGet(item.Key)?.UpdateGameInfo(item.Value);
			}
		}

		private void OnPersonalInfo(UserPersonalInfoArg _arg)
		{
			Hero hero = Get(_arg.mHeroId);
			if (hero != null)
			{
				hero.SetPersonalInfo(_arg.mInfo);
				TryNotify(_success: true, hero, mPersonalInfoNotifiers);
			}
		}

		private void OnDressedItems(DressedItemsArg _arg)
		{
			Hero hero = Get(_arg.mHeroId);
			if (hero == null)
			{
				return;
			}
			hero.UndressAll();
			foreach (DressedItemsArg.DressedItem mItem in _arg.mItems)
			{
				CtrlPrototype ctrlPrototype = mCtrlPrototypes.Get(mItem.mArticleId);
				if (ctrlPrototype != null)
				{
					if (ctrlPrototype.Article == null)
					{
						Log.Warning("invalid article " + ctrlPrototype.Id);
						continue;
					}
					CtrlThing ctrlThing = new CtrlThing(mItem.mId, ctrlPrototype);
					ctrlThing.Add(mItem.mCount - ctrlThing.Count);
					hero.Dress(mItem.mSlot, ctrlThing);
				}
			}
			TryNotify(_success: true, hero, mDressedItemsNotifiers);
		}

		private void OnDressedMpd(NewDressedItemArg _arg)
		{
			Hero hero = Get(_arg.mHeroId);
			if (hero == null)
			{
				return;
			}
			CtrlPrototype ctrlPrototype = mCtrlPrototypes.Get(_arg.mArticleId);
			if (ctrlPrototype == null)
			{
				return;
			}
			if (ctrlPrototype.Article == null)
			{
				Log.Warning("invalid article " + ctrlPrototype.Id);
				return;
			}
			CtrlThing item = new CtrlThing(-1, ctrlPrototype);
			hero.Dress(_arg.mSlot, item);
			if (mNewItemDressedCallback != null)
			{
				mNewItemDressedCallback(hero, _arg.mSlot);
			}
		}

		private void OnUndressedMpd(UndressedItemArg _arg)
		{
			Hero hero = Get(_arg.mHeroId);
			if (hero != null)
			{
				CtrlThing item = hero.Undress(_arg.mSlot);
				if (mItemUndressedCallback != null)
				{
					mItemUndressedCallback(hero, _arg.mSlot, item);
				}
			}
		}

		public IStoreContentProvider<Hero> GetContentProvider()
		{
			if (mContentProv == null)
			{
				mContentProv = new StoreContentProvider<Hero>(this);
			}
			return mContentProv;
		}
	}
}
