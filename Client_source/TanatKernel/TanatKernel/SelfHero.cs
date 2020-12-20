using System;
using System.Collections.Generic;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class SelfHero : IRealMoneyHolder, IMoneyHolder
	{
		public delegate void InventoryChangedCallback(IStoreContentProvider<CtrlThing> _inventory);

		public InventoryChangedCallback mInventoryChangedCallback;

		private UserNetData mUserData;

		private Store<CtrlThing> mInventory = new Store<CtrlThing>("HeroInventory");

		private CtrlServerConnection mCtrlSrv;

		private IStoreContentProvider<Hero> mHeroProv;

		private int mMoney;

		private int mDiamondMoney;

		private IStoreContentProvider<CtrlThing> mItemProv;

		private Notifier<SelfHero, object>.Group mBuyNotifiers = new Notifier<SelfHero, object>.Group();

		private Notifier<SelfHero, object>.Group mSellNotifiers = new Notifier<SelfHero, object>.Group();

		private Dictionary<int, CtrlThing> mDressedItems = new Dictionary<int, CtrlThing>();

		private Notifier<SelfHero, object>.Group mDressNotifiers = new Notifier<SelfHero, object>.Group();

		private Notifier<int, object>.Group mDressFailNotifiers = new Notifier<int, object>.Group();

		private Notifier<SelfHero, object>.Group mUndressNotifiers = new Notifier<SelfHero, object>.Group();

		public Hero Hero => mHeroProv.Get(mUserData.UserId);

		public int VirtualMoney => mMoney;

		public int RealMoney => mDiamondMoney;

		public int DiamondMoney => mDiamondMoney;

		public float RealDiamondMoney => (float)mDiamondMoney * 0.01f;

		public IStoreContentProvider<CtrlThing> Inventory
		{
			get
			{
				if (mItemProv == null)
				{
					mItemProv = new StoreContentProvider<CtrlThing>(mInventory);
				}
				return mItemProv;
			}
		}

		public IEnumerable<CtrlThing> DressedItems => mDressedItems.Values;

		public IEnumerable<KeyValuePair<int, CtrlThing>> DressedItemsWithSlots => mDressedItems;

		public SelfHero(CtrlServerConnection _ctrlSrv, UserNetData _userData)
		{
			if (_ctrlSrv == null)
			{
				throw new ArgumentNullException("_ctrlSrv");
			}
			if (_userData == null)
			{
				throw new ArgumentNullException("_userData");
			}
			mCtrlSrv = _ctrlSrv;
			mUserData = _userData;
			mHeroProv = _ctrlSrv.Heroes.GetContentProvider();
		}

		public void Subscribe()
		{
			HandlerManager<CtrlPacket, Enum> handlerMgr = mCtrlSrv.EntryPoint.HandlerMgr;
			handlerMgr.Subscribe<UserBagArg>(CtrlCmdId.user.bag, null, null, OnBag);
			handlerMgr.Subscribe<UpdateBagArg>(CtrlCmdId.user.update_bag_mpd, null, null, OnUpdateBag);
			handlerMgr.Subscribe<DressItemArg>(CtrlCmdId.user.dress, null, OnDressFailed, OnDressed);
			handlerMgr.Subscribe(CtrlCmdId.user.undress, OnUndressed, OnUndressFailed);
			handlerMgr.Subscribe(CtrlCmdId.store.buy, OnBought, OnBuyFailed);
			handlerMgr.Subscribe(CtrlCmdId.store.sell, OnSold, OnSellFailed);
			handlerMgr.Subscribe<HeroMoneyArg>(CtrlCmdId.user.money, null, null, OnMoney);
			handlerMgr.Subscribe<HeroMoneyMpdArg>(CtrlCmdId.user.money_mpd, null, null, OnMoneyCame);
		}

		public void Unsubscribe()
		{
			mCtrlSrv.EntryPoint.HandlerMgr.Unsubscribe(this);
		}

		private CtrlThing CreateThing(int _id, int _articleId)
		{
			IStoreContentProvider<CtrlPrototype> prototypeProvider = mCtrlSrv.GetPrototypeProvider();
			CtrlPrototype ctrlPrototype = prototypeProvider.Get(_articleId);
			if (ctrlPrototype == null || ctrlPrototype.Article == null)
			{
				Log.Warning("invalid item prototype " + _articleId);
				return null;
			}
			return new CtrlThing(_id, ctrlPrototype);
		}

		private void OnBag(UserBagArg _arg)
		{
			mMoney = _arg.mUserMoney;
			mInventory.Clear();
			foreach (UserBagArg.BagItem mItem in _arg.mItems)
			{
				CtrlThing ctrlThing = CreateThing(mItem.mId, mItem.mArticleId);
				if (ctrlThing != null)
				{
					ctrlThing.Add(mItem.mCount - ctrlThing.Count);
					ctrlThing.Use(mItem.mUsed - ctrlThing.Used);
					mInventory.Add(ctrlThing);
				}
			}
			if (mInventoryChangedCallback != null)
			{
				mInventoryChangedCallback(Inventory);
			}
		}

		private void OnUpdateBag(UpdateBagArg _arg)
		{
			foreach (UpdateBagArg.UpdData mUpdItem in _arg.mUpdItems)
			{
				CtrlThing ctrlThing = mInventory.TryGet(mUpdItem.mItemId);
				if (ctrlThing == null)
				{
					ctrlThing = CreateThing(mUpdItem.mItemId, mUpdItem.mArticleId);
					if (ctrlThing == null)
					{
						continue;
					}
					mInventory.Add(ctrlThing);
				}
				if (mUpdItem.mCount == 0)
				{
					mInventory.Remove(mUpdItem.mItemId);
					continue;
				}
				ctrlThing.Add(mUpdItem.mCount - ctrlThing.Count);
				ctrlThing.Use(mUpdItem.mUsed - ctrlThing.Used);
			}
			if (mInventoryChangedCallback != null)
			{
				mInventoryChangedCallback(Inventory);
			}
		}

		private void OnDressed(DressItemArg _arg)
		{
			IStoreContentProvider<CtrlPrototype> prototypeProvider = mCtrlSrv.GetPrototypeProvider();
			CtrlPrototype ctrlPrototype = prototypeProvider.Get(_arg.mArticleId);
			if (ctrlPrototype != null)
			{
				CtrlThing value = new CtrlThing(_arg.mItemId, ctrlPrototype);
				mDressedItems[_arg.mSlot] = value;
				mDressNotifiers.Call(_success: true, this);
				mDressFailNotifiers.Call(_success: true, 0);
			}
		}

		private void OnDressFailed(int _errorCode)
		{
			mDressFailNotifiers.Call(_success: false, _errorCode);
			mDressNotifiers.Call(_success: false, this);
		}

		private void OnUndressed()
		{
			mUndressNotifiers.Call(_success: true, this);
		}

		private void OnUndressFailed(int _errorCode)
		{
			mUndressNotifiers.Call(_success: false, this);
		}

		private void OnBought()
		{
			mBuyNotifiers.Call(_success: true, this);
		}

		private void OnBuyFailed(int _errorCode)
		{
			mBuyNotifiers.Call(_success: false, this);
		}

		private void OnSold()
		{
			mSellNotifiers.Call(_success: true, this);
		}

		private void OnSellFailed(int _errorCode)
		{
			mSellNotifiers.Call(_success: false, this);
		}

		private void OnMoney(HeroMoneyArg _arg)
		{
			mMoney = _arg.mMoney;
			mDiamondMoney = _arg.mDiamondMoney;
		}

		private void OnMoneyCame(HeroMoneyMpdArg _arg)
		{
			OnMoney(_arg.mMoney);
		}

		public void Dress(int _itemId, int _slot, Notifier<SelfHero, object> _notifier, Notifier<int, object> _failNotifier)
		{
			if (_notifier != null)
			{
				mDressNotifiers.Add(_notifier);
			}
			if (_failNotifier != null)
			{
				mDressFailNotifiers.Add(_failNotifier);
			}
			mCtrlSrv.HeroSender.Dress(_itemId, _slot);
		}

		public void Undress(int _slot, Notifier<SelfHero, object> _notifier)
		{
			if (_notifier != null)
			{
				mUndressNotifiers.Add(_notifier);
			}
			mCtrlSrv.HeroSender.Undress(_slot);
		}

		public void Buy(Dictionary<int, int> _basket, Notifier<SelfHero, object> _notifier)
		{
			if (_notifier != null)
			{
				mBuyNotifiers.Add(_notifier);
			}
			mCtrlSrv.ShopSender.Buy(_basket);
		}

		public void Sell(Dictionary<int, int> _basket, Notifier<SelfHero, object> _notifier)
		{
			if (_notifier != null)
			{
				mSellNotifiers.Add(_notifier);
			}
			mCtrlSrv.ShopSender.Sell(_basket);
		}

		public void ChangeMoney(int _diamondMoney, int _goldMoney)
		{
			mCtrlSrv.SendBankChange(_diamondMoney, _goldMoney);
		}

		public void BuyFromBattle(Dictionary<int, int> _basket, int _mapType)
		{
			mCtrlSrv.ShopSender.Buy(_basket, _mapType);
		}
	}
}
