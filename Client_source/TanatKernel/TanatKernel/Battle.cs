using System;
using System.Collections.Generic;
using System.Xml;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class Battle
	{
		private struct GameInfo
		{
			public int mTimeLimit;

			public int mFragLimit;
		}

		private class AvaBindingData
		{
			public int mPlayerId;

			public int mAvatarId;

			public int mLevel;
		}

		private class UpdateHeroArgs
		{
			public int mPlayerId;

			public int mObjId;
		}

		private PlayerStore mPlayers;

		private SelfPlayer mSelfPlayer;

		private PropertyHolder mPropHolder = new PropertyHolder();

		private Store<BattlePrototype> mPrototypes = new Store<BattlePrototype>("BattlePrototypes");

		private Dictionary<int, int> mArticleToProto = new Dictionary<int, int>();

		private GameObjectStore mGameObjects = new GameObjectStore();

		private EffectorStore mEffectors = new EffectorStore();

		private BattleTimer mTimer = new BattleTimer();

		private TeamRecognizer mTeamRecognizer = new TeamRecognizer();

		private GameInfo mGameInfo;

		private IHeroViewManager mHeroViewMgr;

		private HeroStore mHeroes;

		private List<IGameObjectInitializer> mInitializers = new List<IGameObjectInitializer>();

		private Notifier<Battle, object>.Group mDestroyNotifiers = new Notifier<Battle, object>.Group();

		private ShortPlayerInfo mShortSelfPlayerInfo;

		private Dictionary<int, AvaBindingData> mAvaBindingData = new Dictionary<int, AvaBindingData>();

		private Dictionary<string, float> mCoefs = new Dictionary<string, float>();

		private IGameObjectManager mGameObjMgr;

		private BattleServerConnection mBattleSrv;

		private CtrlServerConnection mCtrlSrv;

		private List<SetAvatarArg> mAvatarsArgs = new List<SetAvatarArg>();

		public bool mLoaded;

		private bool mSelfInited;

		private bool mEntered;

		private IStoreContentProvider<Player> mPlayerProv;

		private IStoreContentProvider<BattlePrototype> mProtoProv;

		private GameObjectsProvider mGameObjProv;

		private IStoreContentProvider<Effector> mEffectorProv;

		private float mSelfRespawnTime;

		public SelfPlayer SelfPlayer => mSelfPlayer;

		public PlayerStore PlayerStore => mPlayers;

		public float Time => mTimer.Time;

		public IDictionary<string, float> Coefs => mCoefs;

		public IDictionary<int, int> ArticleToProto => mArticleToProto;

		public Battle(IGameObjectManager _gameObjMgr, BattleServerConnection _battleSrv, CtrlServerConnection _ctrlSrv, int _selfPlayerId)
		{
			if (_gameObjMgr == null)
			{
				throw new ArgumentNullException("_gameObjMgr");
			}
			if (_battleSrv == null)
			{
				throw new ArgumentNullException("_battleSrv");
			}
			if (_ctrlSrv == null)
			{
				throw new ArgumentNullException("_ctrlSrv");
			}
			mGameObjMgr = _gameObjMgr;
			mBattleSrv = _battleSrv;
			mCtrlSrv = _ctrlSrv;
			mPlayers = new PlayerStore(_selfPlayerId, _ctrlSrv.Heroes, GetTimer(), mCtrlSrv.GetCtrlAvatarStore());
			mSelfPlayer = new SelfPlayer(_selfPlayerId, this, mBattleSrv, GetPlayerProvider(), GetEffectorProvider(), mCtrlSrv.GetQuestProvider());
			mSelfPlayer.InitInventory(GetPrototypeProvider(), mCtrlSrv.GetPrototypeProvider());
			mBattleSrv.SetTimer(GetTimer());
		}

		public void Clear()
		{
			mPlayers.Clear();
			mPropHolder.Clear();
			mPrototypes.Clear();
			mGameObjects.Clear();
			mEffectors.Clear();
			mInitializers.Clear();
			mGameObjMgr.DeleteAllGameObjects();
			mAvaBindingData.Clear();
			mCoefs.Clear();
			mDestroyNotifiers.Call(_success: true, this);
		}

		public void Disable()
		{
			Unsubscribe();
		}

		public void Start()
		{
		}

		public BattleTimer GetTimer()
		{
			return mTimer;
		}

		public TeamRecognizer GetTeamRecognizer()
		{
			return mTeamRecognizer;
		}

		public ShortPlayerInfo GetSelfPlayerHistory()
		{
			return mShortSelfPlayerInfo;
		}

		public IStoreContentProvider<Player> GetPlayerProvider()
		{
			if (mPlayerProv == null)
			{
				mPlayerProv = new StoreContentProvider<Player>(mPlayers);
			}
			return mPlayerProv;
		}

		public IStoreContentProvider<BattlePrototype> GetPrototypeProvider()
		{
			if (mProtoProv == null)
			{
				mProtoProv = new StoreContentProvider<BattlePrototype>(mPrototypes);
			}
			return mProtoProv;
		}

		public GameObjectsProvider GetGameObjProvider()
		{
			if (mGameObjProv == null)
			{
				mGameObjProv = new GameObjectsProvider(mGameObjects);
			}
			return mGameObjProv;
		}

		public IStoreContentProvider<Effector> GetEffectorProvider()
		{
			if (mEffectorProv == null)
			{
				mEffectorProv = new StoreContentProvider<Effector>(mEffectors);
			}
			return mEffectorProv;
		}

		public void EnableHeroView(IHeroViewManager _viewMgr, HeroStore _heroes)
		{
			if (_viewMgr == null)
			{
				throw new ArgumentNullException("_viewMgr");
			}
			if (_heroes == null)
			{
				throw new ArgumentNullException("_heroes");
			}
			mHeroViewMgr = _viewMgr;
			mHeroes = _heroes;
		}

		public void Subscribe()
		{
			BattlePacketManager packetMgr = mBattleSrv.PacketMgr;
			packetMgr.mVisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(packetMgr.mVisibleCallback, new BattlePacketManager.SetObjStateCallback(OnVisible));
			packetMgr.mInvisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(packetMgr.mInvisibleCallback, new BattlePacketManager.SetObjStateCallback(OnInvisible));
			packetMgr.mRelevantCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(packetMgr.mRelevantCallback, new BattlePacketManager.SetObjStateCallback(OnRelevant));
			packetMgr.mUnrelevantCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Combine(packetMgr.mUnrelevantCallback, new BattlePacketManager.SetObjStateCallback(OnUnrelevant));
			packetMgr.mSyncCallback = (BattlePacketManager.SyncCallback)Delegate.Combine(packetMgr.mSyncCallback, new BattlePacketManager.SyncCallback(OnSync));
			HandlerManager<BattlePacket, BattleCmdId> handlerMgr = packetMgr.HandlerMgr;
			handlerMgr.Subscribe<ServerTimeArg>(BattleCmdId.GET_TIME, null, null, OnServerTime);
			handlerMgr.Subscribe<GameDataArg>(BattleCmdId.GAME_DATA, null, null, OnGameData);
			handlerMgr.Subscribe<ProrotypeInfoArg>(BattleCmdId.PROTOTYPE_INFO, null, null, OnPrototypeInfo);
			handlerMgr.Subscribe<CreateObjectArg>(BattleCmdId.CREATE_OBJECT, null, null, OnCreateObject);
			handlerMgr.Subscribe<DeleteObjectArg>(BattleCmdId.DELETE_OBJECT, null, null, OnDeleteObject);
			handlerMgr.Subscribe<SetAvatarArg>(BattleCmdId.SET_AVATAR, null, null, OnSetAvatar);
			handlerMgr.Subscribe<ActionArg>(BattleCmdId.ACTION, null, null, OnAction);
			handlerMgr.Subscribe<ActionDoneArg>(BattleCmdId.ACTION_DONE, null, null, OnActionDone);
			handlerMgr.Subscribe<AddEffectorArg>(BattleCmdId.ADD_EFFECTOR, null, null, OnAddEffector);
			handlerMgr.Subscribe<RemoveAffectorArg>(BattleCmdId.REMOVE_EFFECTOR, null, null, OnRemoveEffector);
			handlerMgr.Subscribe<LevelUpArg>(BattleCmdId.LEVEL_UP, null, null, OnLevelUp);
			handlerMgr.Subscribe<ItemEquipedArg>(BattleCmdId.ITEM_EQUIP, null, null, OnItemEquiped);
			handlerMgr.Subscribe<BattleEndArg>(BattleCmdId.BATTLE_END, null, null, OnBattleEnd);
			handlerMgr.Subscribe(BattleCmdId.ENTER, Entered, null);
			mPlayers.Subscribe(handlerMgr);
			mSelfPlayer.Subscribe(handlerMgr);
			if (mHeroes != null)
			{
				HeroStore heroStore = mHeroes;
				heroStore.mNewItemDressedCallback = (HeroStore.NewItemDressedCallback)Delegate.Combine(heroStore.mNewItemDressedCallback, new HeroStore.NewItemDressedCallback(OnNewItemDressed));
				HeroStore heroStore2 = mHeroes;
				heroStore2.mItemUndressedCallback = (HeroStore.ItemUndressedCallback)Delegate.Combine(heroStore2.mItemUndressedCallback, new HeroStore.ItemUndressedCallback(OnItemUndressed));
			}
		}

		private void Unsubscribe()
		{
			mPlayers.Unsubscribe();
			mSelfPlayer.Unsubscribe();
			mBattleSrv.PacketMgr.HandlerMgr.Unsubscribe(this);
			BattlePacketManager packetMgr = mBattleSrv.PacketMgr;
			packetMgr.mVisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(packetMgr.mVisibleCallback, new BattlePacketManager.SetObjStateCallback(OnVisible));
			BattlePacketManager packetMgr2 = mBattleSrv.PacketMgr;
			packetMgr2.mInvisibleCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(packetMgr2.mInvisibleCallback, new BattlePacketManager.SetObjStateCallback(OnInvisible));
			BattlePacketManager packetMgr3 = mBattleSrv.PacketMgr;
			packetMgr3.mRelevantCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(packetMgr3.mRelevantCallback, new BattlePacketManager.SetObjStateCallback(OnRelevant));
			BattlePacketManager packetMgr4 = mBattleSrv.PacketMgr;
			packetMgr4.mUnrelevantCallback = (BattlePacketManager.SetObjStateCallback)Delegate.Remove(packetMgr4.mUnrelevantCallback, new BattlePacketManager.SetObjStateCallback(OnUnrelevant));
			BattlePacketManager packetMgr5 = mBattleSrv.PacketMgr;
			packetMgr5.mSyncCallback = (BattlePacketManager.SyncCallback)Delegate.Remove(packetMgr5.mSyncCallback, new BattlePacketManager.SyncCallback(OnSync));
			mCtrlSrv.EntryPoint.HandlerMgr.Unsubscribe(this);
			if (mHeroes != null)
			{
				HeroStore heroStore = mHeroes;
				heroStore.mNewItemDressedCallback = (HeroStore.NewItemDressedCallback)Delegate.Remove(heroStore.mNewItemDressedCallback, new HeroStore.NewItemDressedCallback(OnNewItemDressed));
				HeroStore heroStore2 = mHeroes;
				heroStore2.mItemUndressedCallback = (HeroStore.ItemUndressedCallback)Delegate.Remove(heroStore2.mItemUndressedCallback, new HeroStore.ItemUndressedCallback(OnItemUndressed));
			}
		}

		private void OnServerTime(ServerTimeArg _arg)
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(_arg.mReceivingTime);
			float num = _arg.mPing * 0.5f;
			float num2 = num + (float)timeSpan.TotalSeconds;
			float ping = mBattleSrv.PacketMgr.Ping;
			Log.Debug("OnServerTime avg ping : " + ping + " cur ping : " + _arg.mPing);
			if (_arg.mPing > ping * 2f + 0.1f)
			{
				Log.Debug("skip time sync (avg ping = " + ping + ", cur ping = " + _arg.mPing + ")");
			}
			else
			{
				mTimer.Sync(_arg.mTime + num2);
			}
		}

		private void OnGameData(GameDataArg _arg)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(_arg.mGameDataXml);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("root");
				if (xmlNode == null)
				{
					return;
				}
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("battle");
				if (xmlNode2 != null)
				{
					XmlAttribute xmlAttribute = xmlNode2.Attributes["time_limit"];
					if (xmlAttribute != null)
					{
						int.TryParse(xmlAttribute.InnerText, out mGameInfo.mTimeLimit);
					}
					xmlAttribute = xmlNode2.Attributes["frag_limit"];
					if (xmlAttribute != null)
					{
						int.TryParse(xmlAttribute.InnerText, out mGameInfo.mFragLimit);
					}
				}
				XmlNode xmlNode3 = xmlNode.SelectSingleNode("Teams");
				if (xmlNode3 != null)
				{
					mTeamRecognizer.SetAllNeutrals(XmlUtil.SafeReadBool("value", xmlNode3));
					for (XmlNode xmlNode4 = xmlNode3.FirstChild; xmlNode4 != null; xmlNode4 = xmlNode4.NextSibling)
					{
						List<int> teams = XmlUtil.SafeReadIntList("value", xmlNode4);
						if (xmlNode4.Name == "Neutral")
						{
							mTeamRecognizer.AddTeams(teams, _isNeutral: true);
						}
						else if (xmlNode4.Name == "Hostile")
						{
							mTeamRecognizer.AddTeams(teams, _isNeutral: false);
						}
					}
				}
				XmlNode xmlNode5 = xmlNode.SelectSingleNode("coefs");
				if (xmlNode5 != null)
				{
					for (XmlNode xmlNode6 = xmlNode5.FirstChild; xmlNode6 != null; xmlNode6 = xmlNode6.NextSibling)
					{
						string name = xmlNode6.Name;
						float value = XmlUtil.SafeReadFloat("value", xmlNode6);
						mCoefs[name] = value;
					}
				}
			}
			catch (XmlException)
			{
			}
		}

		private void OnPrototypeInfo(ProrotypeInfoArg _arg)
		{
			Log.Info(() => "proto: " + _arg.mProrotypeId + "\n" + _arg.mInfo);
			mPropHolder.RetrieveProperties(_arg.mProrotypeId, _arg.mInfo);
			BattlePrototype battlePrototype = new BattlePrototype(_arg.mProrotypeId, mPropHolder, mCoefs);
			lock (mPrototypes)
			{
				mPrototypes.Add(battlePrototype);
			}
			if (battlePrototype.Item != null)
			{
				mArticleToProto[battlePrototype.Item.mArticle] = battlePrototype.Id;
			}
		}

		private void OnCreateObject(CreateObjectArg _arg)
		{
			if (mGameObjects.Exists(_arg.mObjId))
			{
				Log.Error("object " + _arg.mObjId + " already exists");
				return;
			}
			BattlePrototype battlePrototype = mPrototypes.Get(_arg.mPrototypeId);
			if (battlePrototype != null)
			{
				if (battlePrototype.Prefab == null)
				{
					Log.Error("cannot create object by prototype without prefab");
					return;
				}
				mBattleSrv.PacketMgr.Suspend();
				Notifier<IGameObject, object> notifier = new Notifier<IGameObject, object>(OnGameObjectCreated, null);
				mGameObjMgr.CreateGameObjectAsync(_arg.mObjId, battlePrototype, notifier);
			}
		}

		private void OnDeleteObject(DeleteObjectArg _arg)
		{
			IGameObject gameObject = mGameObjects.Get(_arg.mObjId);
			if (gameObject != null)
			{
				mGameObjects.Remove(_arg.mObjId);
				mGameObjMgr.DeleteGameObject(gameObject);
			}
		}

		private void OnRelevant(int _objId)
		{
			IGameObject gameObject = mGameObjects.Get(_objId);
			if (gameObject != null)
			{
				gameObject.Data.Relevant = true;
			}
		}

		private void OnUnrelevant(int _objId)
		{
			IGameObject gameObject = mGameObjects.Get(_objId);
			if (gameObject != null)
			{
				gameObject.Data.Relevant = false;
			}
		}

		private void OnVisible(int _objId)
		{
			bool _prevVisible;
			IGameObject gameObject = mGameObjects.SetVisibility(_objId, _visible: true, out _prevVisible);
			if (gameObject != null && !_prevVisible)
			{
				mGameObjMgr.EnableGameObject(gameObject);
			}
		}

		private void OnInvisible(int _objId)
		{
			bool _prevVisible;
			IGameObject gameObject = mGameObjects.SetVisibility(_objId, _visible: false, out _prevVisible);
			if (gameObject == null)
			{
				return;
			}
			foreach (int effectorId in gameObject.Data.EffectorIds)
			{
				mEffectors.Remove(effectorId);
			}
			gameObject.Data.RemoveAllEffectors();
			gameObject.Data.StopAllActions();
			if (_prevVisible)
			{
				mGameObjMgr.DisableGameObject(gameObject);
			}
		}

		private void OnSync(SyncData _data, float _time)
		{
			mGameObjects.Get(_data.TrackingId)?.Data.Sync(_data, _time);
		}

		private void OnSetAvatar(SetAvatarArg _arg)
		{
			mAvatarsArgs.Add(_arg);
			if (_arg.mPlayerId == mSelfPlayer.PlayerId && mGameObjects.Exists(_arg.mObjId))
			{
				mSelfInited = true;
			}
			PerformAvatars();
		}

		private void Entered()
		{
			mEntered = true;
			PerformAvatars();
		}

		private void PerformAvatars()
		{
			if (!mLoaded && (!mSelfInited || !mEntered))
			{
				return;
			}
			List<Player> list = new List<Player>();
			List<IGameObject> list2 = new List<IGameObject>();
			List<int> list3 = new List<int>();
			foreach (SetAvatarArg mAvatarsArg in mAvatarsArgs)
			{
				if (mAvatarsArg.mPlayerId == mSelfPlayer.PlayerId)
				{
					mSelfPlayer.SkillPoints = mAvatarsArg.mSkillPoints;
				}
				Player player = mPlayers.Get(mAvatarsArg.mPlayerId);
				if (player == null)
				{
					return;
				}
				if (mAvatarsArg.mObjId == -1)
				{
					player.UnbindAvatar();
					continue;
				}
				IGameObject gameObject = mGameObjects.TryGet(mAvatarsArg.mObjId);
				if (gameObject != null)
				{
					list.Add(player);
					list2.Add(gameObject);
					list3.Add(mAvatarsArg.mLevel);
				}
				else
				{
					AvaBindingData avaBindingData = new AvaBindingData();
					avaBindingData.mPlayerId = mAvatarsArg.mPlayerId;
					avaBindingData.mAvatarId = mAvatarsArg.mObjId;
					avaBindingData.mLevel = mAvatarsArg.mLevel;
					mAvaBindingData[mAvatarsArg.mObjId] = avaBindingData;
				}
			}
			BindPlayerAvatars(list, list2, list3);
			mAvatarsArgs.Clear();
			mLoaded = true;
		}

		private void BindPlayerAvatar(Player _player, IGameObject _avatar, int _level)
		{
			_player.BindAvatar(_avatar.Id, GetGameObjProvider());
			_avatar.Data.BindPlayer(_player.Id, GetPlayerProvider());
			_avatar.Data.Level = _level;
			if (_player.Id == mSelfPlayer.PlayerId)
			{
				mSelfPlayer.Init(_avatar);
				mTeamRecognizer.SetSelfTeam(mSelfPlayer.Player.Team);
			}
			if (_avatar.Proto.Prefab.mValue == "Hero")
			{
				if (mHeroViewMgr != null)
				{
					mHeroes.UpdateGameInfo(_player.Id, null);
					UpdateHeroArgs updateHeroArgs = new UpdateHeroArgs();
					updateHeroArgs.mPlayerId = _player.Id;
					updateHeroArgs.mObjId = _avatar.Id;
					mHeroes.UpdateHeroData(_player.Id, new Notifier<Hero, object>(OnHeroDataUpdated, updateHeroArgs));
				}
			}
			else if (_player.IsSelf)
			{
				mShortSelfPlayerInfo = new ShortPlayerInfo();
				mShortSelfPlayerInfo.mId = _player.Id;
				mShortSelfPlayerInfo.mName = _player.Name;
				mShortSelfPlayerInfo.mIsSelf = _player.IsSelf;
				mShortSelfPlayerInfo.mIcon = _avatar.Proto.Desc.mIcon;
			}
		}

		private void BindPlayerAvatars(List<Player> _player, List<IGameObject> _avatar, List<int> _level)
		{
			List<int> list = new List<int>();
			List<Notifier<Hero, object>> list2 = new List<Notifier<Hero, object>>();
			List<Notifier<Hero, object>> list3 = new List<Notifier<Hero, object>>();
			for (int i = 0; i < _player.Count; i++)
			{
				_player[i].BindAvatar(_avatar[i].Id, GetGameObjProvider());
				_avatar[i].Data.BindPlayer(_player[i].Id, GetPlayerProvider());
				_avatar[i].Data.Level = _level[i];
				if (_player[i].Id == mSelfPlayer.PlayerId)
				{
					mSelfPlayer.Init(_avatar[i]);
					mTeamRecognizer.SetSelfTeam(mSelfPlayer.Player.Team);
				}
				if (_avatar[i].Proto.Prefab.mValue == "Hero")
				{
					if (mHeroViewMgr != null)
					{
						UpdateHeroArgs updateHeroArgs = new UpdateHeroArgs();
						updateHeroArgs.mPlayerId = _player[i].Id;
						updateHeroArgs.mObjId = _avatar[i].Id;
						list.Add(_player[i].Id);
						list2.Add(new Notifier<Hero, object>(OnHeroDataUpdatedList, updateHeroArgs));
						list3.Add(new Notifier<Hero, object>(OnDressedItemsUpdated, updateHeroArgs));
					}
				}
				else if (_player[i].IsSelf)
				{
					mShortSelfPlayerInfo = new ShortPlayerInfo();
					mShortSelfPlayerInfo.mId = _player[i].Id;
					mShortSelfPlayerInfo.mName = _player[i].Name;
					mShortSelfPlayerInfo.mIsSelf = _player[i].IsSelf;
					mShortSelfPlayerInfo.mIcon = _avatar[i].Proto.Desc.mIcon;
				}
			}
			mHeroes.UpdateHeroesInfo(list, list2, list3, !mLoaded, mSelfPlayer.PlayerId);
		}

		private void OnHeroDataUpdated(bool _success, Hero _hero, object _data)
		{
			if (_success)
			{
				UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
				IGameObject gameObject = mGameObjects.Get(updateHeroArgs.mObjId);
				if (gameObject != null)
				{
					Notifier<IHeroViewManager, object> notifier = new Notifier<IHeroViewManager, object>();
					notifier.mCallback = (Notifier<IHeroViewManager, object>.Callback)Delegate.Combine(notifier.mCallback, new Notifier<IHeroViewManager, object>.Callback(OnHeroViewDone));
					notifier.mData = updateHeroArgs;
					mHeroViewMgr.SetHeroView(gameObject, _hero.View, notifier);
				}
			}
		}

		private void OnHeroViewDone(bool _success, IHeroViewManager _mgr, object _data)
		{
			if (_success)
			{
				UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
				mHeroes.UpdateDressedItems(updateHeroArgs.mPlayerId, new Notifier<Hero, object>(OnDressedItemsUpdated, updateHeroArgs));
			}
		}

		private void OnDressedItemsUpdated(bool _success, Hero _hero, object _data)
		{
			if (_success)
			{
				UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
				IGameObject gameObject = mGameObjects.Get(updateHeroArgs.mObjId);
				if (gameObject != null)
				{
					mHeroViewMgr.SetHeroViewItems(gameObject, _hero.DressedItems);
				}
			}
		}

		private void OnHeroDataUpdatedList(bool _success, Hero _hero, object _data)
		{
			if (_success)
			{
				UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
				IGameObject gameObject = mGameObjects.TryGet(updateHeroArgs.mObjId);
				if (gameObject != null)
				{
					Notifier<IHeroViewManager, object> notifier = new Notifier<IHeroViewManager, object>();
					notifier.mCallback = (Notifier<IHeroViewManager, object>.Callback)Delegate.Combine(notifier.mCallback, new Notifier<IHeroViewManager, object>.Callback(OnDressedItemsUpdatedList));
					notifier.mData = updateHeroArgs;
					mHeroViewMgr.SetHeroView(gameObject, _hero.View, notifier);
				}
			}
		}

		private void OnDressedItemsUpdatedList(bool _success, IHeroViewManager _mgr, object _data)
		{
			if (!_success)
			{
				return;
			}
			UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
			IGameObject gameObject = mGameObjects.Get(updateHeroArgs.mObjId);
			Hero hero = mHeroes.Get(updateHeroArgs.mPlayerId);
			if (gameObject != null)
			{
				mHeroViewMgr.SetHeroViewItems(gameObject, hero.DressedItems);
				if (mSelfPlayer.PlayerId == updateHeroArgs.mPlayerId)
				{
					mSelfRespawnTime = mTimer.Time;
					PlayerStore.Get(updateHeroArgs.mPlayerId).PerformRespawn();
				}
				else if (mSelfRespawnTime > 0f && mSelfRespawnTime + 2f < mTimer.Time)
				{
					PlayerStore.Get(updateHeroArgs.mPlayerId).PerformRespawn();
				}
			}
		}

		private void OnHeroViewDoneList(bool _success, IHeroViewManager _mgr, object _data)
		{
			if (_success)
			{
				UpdateHeroArgs updateHeroArgs = _data as UpdateHeroArgs;
				mHeroes.UpdateDressedItemsList(updateHeroArgs.mPlayerId);
			}
		}

		private void OnAction(ActionArg _arg)
		{
			mGameObjects.Get(_arg.mObjId)?.Data.DoAction(_arg.mActionId, _arg.mTargetId, _arg.mStartTime);
		}

		private void OnActionDone(ActionDoneArg _arg)
		{
			mGameObjects.Get(_arg.mObjId)?.Data.StopAction(_arg.mActionId);
		}

		private void OnAddEffector(AddEffectorArg _arg)
		{
			if (!mGameObjects.Exists(_arg.mOwnerId))
			{
				Log.Error("owner " + _arg.mOwnerId + " not exists");
				return;
			}
			BattlePrototype battlePrototype = mPrototypes.Get(_arg.mEffectorProtoId);
			if (battlePrototype == null || battlePrototype.EffectDesc == null)
			{
				Log.Error("invalid prototype " + _arg.mEffectorProtoId);
				return;
			}
			IGameObject gameObject = mGameObjects.Get(_arg.mOwnerId);
			Effector effector = new Effector(_arg.mEffectorId, battlePrototype, _arg.mArgs, gameObject.Data.Params, _arg.mOwnerId);
			mEffectors.Add(effector);
			if (_arg.mParentId != -1)
			{
				mEffectors.Bind(_arg.mParentId, effector.Id);
			}
			gameObject.Data.AddEffector(_arg.mEffectorId);
		}

		private void OnRemoveEffector(RemoveAffectorArg _arg)
		{
			Effector effector = mEffectors.Get(_arg.mEffectorId);
			if (effector != null)
			{
				mEffectors.Remove(_arg.mEffectorId);
				mGameObjects.Get(effector.OwnerId)?.Data.RemoveEffector(_arg.mEffectorId);
			}
		}

		private void OnLevelUp(LevelUpArg _arg)
		{
			IGameObject gameObject = mGameObjects.Get(_arg.mObjId);
			if (gameObject != null)
			{
				gameObject.Data.Level = _arg.mLevel;
			}
		}

		private void OnItemEquiped(ItemEquipedArg _arg)
		{
			Log.Debug(_arg.mIsEquiped + " " + _arg.mItemObjId + " " + _arg.mObjId + " " + _arg.mItemProtoId);
			IGameObject gameObject = mGameObjects.Get(_arg.mObjId);
			if (gameObject == null)
			{
				return;
			}
			if (!gameObject.Data.IsPlayerBinded)
			{
				Log.Error("object " + gameObject.Id + " is not an avatar");
				return;
			}
			Player player = gameObject.Data.Player;
			if (_arg.mIsEquiped)
			{
				player.Equip(_arg.mItemProtoId);
			}
			else
			{
				player.Unequip(_arg.mItemProtoId);
			}
		}

		private void OnBattleEnd(BattleEndArg _arg)
		{
			mCtrlSrv.SendGetFightLog(mBattleSrv.BattleId, -1);
		}

		public void AddGameObjInitializer(IGameObjectInitializer _initializer)
		{
			if (_initializer == null)
			{
				throw new ArgumentNullException("_initializer");
			}
			mInitializers.Add(_initializer);
		}

		public void RemoveGameObjInitializer(IGameObjectInitializer _initializer)
		{
			mInitializers.Remove(_initializer);
		}

		private void OnGameObjectCreated(bool _success, IGameObject _go, object _data)
		{
			if (_success)
			{
				_go.Data.SetTimer(GetTimer());
				_go.Data.SetEffectorProvider(GetEffectorProvider());
				_go.Data.SetTeamRecognizer(GetTeamRecognizer());
				_go.Data.SetGameObjectProvider(GetGameObjProvider());
				foreach (IGameObjectInitializer mInitializer in mInitializers)
				{
					mInitializer.InitGameObject(_go);
				}
				lock (mGameObjects)
				{
					mGameObjects.Add(_go);
				}
				if (mAvaBindingData.TryGetValue(_go.Id, out var value))
				{
					mAvaBindingData.Remove(_go.Id);
					Player player = mPlayers.Get(value.mPlayerId);
					if (player != null)
					{
						BindPlayerAvatar(player, _go, value.mLevel);
					}
				}
			}
			mBattleSrv.PacketMgr.Resume();
		}

		private void OnNewItemDressed(Hero _hero, int _slot)
		{
			CtrlThing itemAtSlot = _hero.GetItemAtSlot(_slot);
			if (itemAtSlot == null)
			{
				Log.Warning("hero " + _hero.Id + " slot " + _slot + " is empty");
			}
			else if (mHeroViewMgr != null)
			{
				Player player = mPlayers.Get(_hero.Id);
				if (player != null)
				{
					mHeroViewMgr.SetHeroViewItem(player.Avatar, itemAtSlot);
				}
			}
		}

		private void OnItemUndressed(Hero _hero, int _slot, CtrlThing _item)
		{
			if (mHeroViewMgr != null)
			{
				Player player = mPlayers.Get(_hero.Id);
				if (player != null)
				{
					mHeroViewMgr.RemoveHeroViewItem(player.Avatar, _item);
				}
			}
		}

		public void SubscribeOnDestory(Notifier<Battle, object> _notifier)
		{
			mDestroyNotifiers.Add(_notifier);
		}
	}
}
