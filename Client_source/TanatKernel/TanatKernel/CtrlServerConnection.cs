using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class CtrlServerConnection
	{
		public interface IMapTypeDescHolder
		{
			string GetMapTypeDesc(MapType _mapType);
		}

		private class MapTypeDescHolder : IMapTypeDescHolder
		{
			public Dictionary<MapType, string> mDescs = new Dictionary<MapType, string>();

			public string GetMapTypeDesc(MapType _mapType)
			{
				if (mDescs.TryGetValue(_mapType, out var value))
				{
					return value;
				}
				return "UNDEFINED";
			}
		}

		public enum LogLevel
		{
			notice,
			warning,
			error
		}

		private Config mConfig;

		private CtrlEntryPoint mEntryPoint;

		private bool mLoggedIn;

		private bool mHeroExists;

		private bool mKeepSessionAlive;

		private GroupSender mGroupSender;

		private ClanSender mClanSender;

		private FightSender mFightSender;

		private CastleSender mCastleSender;

		private TradeSender mTradeSender;

		private ShopSender mShopSender;

		private HeroSender mHeroSender;

		private MapSender mMapSender;

		private QuestSender mQuestSender;

		private UsersListSender mUsersListSender;

		private Chat mChat;

		private HeroStore mHeroes;

		private SelfHero mSelfHero;

		private Group mGroup;

		private Clan mClan;

		private FightHelper mFightHelper;

		private CastleHelper mCastleHelper;

		private QuestStore mQuests;

		private SelfQuestStore mSelfQuests;

		private NpcStore mNpcs;

		private CtrlAvatarStore mCtrlAvatarStore;

		private PropertyHolder mProperties = new PropertyHolder();

		private IStoreContentProvider<CtrlPrototype> mProtoProv;

		public int mCurrentLocation;

		private MapTypeDescHolder mMapTypeDescs = new MapTypeDescHolder();

		private NetSystem mNetSys;

		private MpdConnection mMpdConnection;

		private bool mMapJoined;

		private BattleServerConnection mAutoConnectSrv;

		public ClanSender ClanSender => mClanSender;

		public FightSender FightSender => mFightSender;

		public CastleSender CastleSender => mCastleSender;

		public UsersListSender UsersListSender => mUsersListSender;

		public GroupSender GroupSender => mGroupSender;

		public TradeSender TradeSender => mTradeSender;

		public ShopSender ShopSender => mShopSender;

		public HeroSender HeroSender => mHeroSender;

		public MapSender MapSender => mMapSender;

		public QuestSender QuestSender => mQuestSender;

		public CtrlEntryPoint EntryPoint => mEntryPoint;

		public Chat Chat => mChat;

		public HeroStore Heroes => mHeroes;

		public SelfHero SelfHero => mSelfHero;

		public Group Group => mGroup;

		public Clan Clan => mClan;

		public FightHelper FightHelper => mFightHelper;

		public CastleHelper CastleHelper => mCastleHelper;

		public NpcStore Npcs => mNpcs;

		public QuestStore Quests => mQuests;

		public SelfQuestStore SelfQuests => mSelfQuests;

		public bool KeepSessionAlive
		{
			get
			{
				return mKeepSessionAlive;
			}
			set
			{
				mKeepSessionAlive = value;
				Log.Debug("keep session alive: " + mKeepSessionAlive);
			}
		}

		public IMapTypeDescHolder MapTypeDescs => mMapTypeDescs;

		public bool IsLoggedIn => mLoggedIn;

		public bool IsHeroExists => mHeroExists;

		public MpdConnection MpdConnection => mMpdConnection;

		public bool IsMapJoined => mMapJoined;

		public CtrlServerConnection(UserNetData _userData, Config _conf, NetSystem _netSys)
		{
			if (_userData == null)
			{
				throw new ArgumentNullException("_userData");
			}
			if (_conf == null)
			{
				throw new ArgumentNullException("_conf");
			}
			if (_netSys == null)
			{
				throw new ArgumentNullException("_netSys");
			}
			mNetSys = _netSys;
			mConfig = _conf;
			mEntryPoint = new CtrlEntryPoint(_userData);
			mEntryPoint.SetHost(_conf.ControlServerHost);
			mEntryPoint.SetPort(_conf.ControlServerPort);
			mEntryPoint.PingTime = _conf.ControlServerPingTime;
			mEntryPoint.DisconnectTime = _conf.ControlServerDisconnectTime;
			mFightSender = new FightSender(mEntryPoint);
			mClanSender = new ClanSender(mEntryPoint);
			mCastleSender = new CastleSender(mEntryPoint);
			mGroupSender = new GroupSender(mEntryPoint);
			mTradeSender = new TradeSender(mEntryPoint);
			mShopSender = new ShopSender(mEntryPoint);
			mHeroSender = new HeroSender(mEntryPoint);
			mMapSender = new MapSender(mEntryPoint);
			mQuestSender = new QuestSender(mEntryPoint);
			mUsersListSender = new UsersListSender(mEntryPoint);
			mEntryPoint.HandlerMgr.Subscribe<LoginArg>(CtrlCmdId.user.login, null, null, OnLogin);
			mEntryPoint.HandlerMgr.Subscribe<HeroDataArg>(CtrlCmdId.common.hero_conf, null, null, OnHeroData);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.hero.create, OnHeroCreated, null);
			mEntryPoint.HandlerMgr.Subscribe<ChatConfArg>(CtrlCmdId.chat.conf, null, null, OnChatConf);
			mEntryPoint.HandlerMgr.Subscribe<JoinArg>(CtrlCmdId.arena.join_request, null, null, OnMapJoined);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.arena.desert, StopWaitReconnect, null);
			mEntryPoint.HandlerMgr.Subscribe<ServerDataArg>(CtrlCmdId.common.area_conf, null, null, OnBattleServerData);
			mEntryPoint.HandlerMgr.Subscribe<ReconnectArg>(CtrlCmdId.common.reconnect, null, null, OnReconnectData);
			mEntryPoint.HandlerMgr.Subscribe<MapTypeDescsArg>(CtrlCmdId.arena.get_map_type_descs, null, null, OnMapTypeDescs);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.castle.start_request_mpd, OnCastleStart, null);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.castle.desert_battle, StopWaitReconnect, null);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.fight.in_request, OnCastleStart, null);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.hunt.join, OnCastleStart, null);
			mEntryPoint.HandlerMgr.Subscribe(CtrlCmdId.fight.desert_mpd, StopWaitReconnect, null);
			mProtoProv = new CachedCtrlPrototypeProvider(mProperties);
			mQuests = new QuestStore(mProtoProv);
			mCtrlAvatarStore = new CtrlAvatarStore();
            DownloadItemPrototypes(_conf.ItemProtoUrl);
            DownloadAvatarPrototypes(_conf.AvatarsUrl);
            DownloadQuests(_conf.QuestsUrl);
            DownloadQuests(_conf.TasksUrl);
            mChat = new Chat(this);
            mChat.Subscribe();
            mHeroes = new HeroStore(this);
            mHeroes.Subscribe();
            mSelfHero = new SelfHero(this, _userData);
            mSelfHero.Subscribe();
            mGroup = new Group(this, _userData);
            mGroup.Subscribe(mEntryPoint.HandlerMgr);
            mClan = new Clan(this, _userData);
            mClan.Subscribe(mEntryPoint.HandlerMgr);
            mFightHelper = new FightHelper(mFightSender);
            mFightHelper.Subscribe(mEntryPoint.HandlerMgr);
            mNpcs = new NpcStore(mQuestSender);
            mNpcs.Subscribe(mEntryPoint.HandlerMgr);
            mSelfQuests = new SelfQuestStore(mQuestSender, GetQuestProvider(), mNpcs);
            mSelfQuests.Subscribe(mEntryPoint.HandlerMgr);
            mCastleHelper = new CastleHelper(mCastleSender, mFightHelper, mClan);
            mCastleHelper.Subscribe(mEntryPoint.HandlerMgr);
        }

		public IStoreContentProvider<CtrlPrototype> GetPrototypeProvider()
		{
			return mProtoProv;
		}

		public CtrlAvatarStore GetCtrlAvatarStore()
		{
			return mCtrlAvatarStore;
		}

		public IStoreContentProvider<Hero> GetHeroProvider()
		{
			return new StoreContentProvider<Hero>(mHeroes);
		}

		public IStoreContentProvider<IQuest> GetQuestProvider()
		{
			return new StoreContentProvider<IQuest>(mQuests);
		}

		public IStoreContentProvider<ISelfQuest> GetSelfQuestProvider()
		{
			return new StoreContentProvider<ISelfQuest>(mSelfQuests);
		}

		public IStoreContentProvider<INpc> GetNpcProvider()
		{
			return new StoreContentProvider<INpc>(mNpcs);
		}

		public void Update()
		{
			mEntryPoint.Update();
			mEntryPoint.SendOutcomings();
		}

		public void DownloadAvatarPrototypes(string _url)
		{
			if (!string.IsNullOrEmpty(_url))
			{
				WebTransferOperation webTransferOperation = new WebTransferOperation(_url);
				webTransferOperation.SetReconnectSettings(mConfig.WebReConnectAttemptCount, mConfig.MicroReConnectAttemptTime);
				webTransferOperation.mProxyCredential = mEntryPoint.UserData.ProxyCredential;
				webTransferOperation.mNotifiers.Add(new Notifier<TransferOperation, object>(OnAvatarsDownloaded, null));
				webTransferOperation.Begin();
			}
		}

		public void OnAvatarsDownloaded(bool _success, TransferOperation _op, object _data)
		{
			if (_success)
			{
				mConfig.ApplyThreadSettings();
				mCtrlAvatarStore.Retrieve(_op.Receiver);
			}
		}

		public void Login()
		{
			SendLogin();
		}

		private void OnLogin(LoginArg _arg)
		{
			Log.Notice(mEntryPoint.Host + " logged in");
			mEntryPoint.UserData.UserId = _arg.mUserId;
			mEntryPoint.UserData.UpdateUserName(_arg.mUserName);
			mEntryPoint.UserData.UserFlags = _arg.mUserFlags;
			mEntryPoint.EnablePing();
			if (!string.IsNullOrEmpty(_arg.mSessKey))
			{
				mEntryPoint.SetSessionKey(_arg.mSessKey);
			}
			mLoggedIn = true;
			GetGlobalBuffs();
		}

		public void Logout()
		{
			mChat.Clear();
			mEntryPoint.DisablePing();
			mEntryPoint.Clean();
			if (!mKeepSessionAlive)
			{
				mEntryPoint.Session.CleanSaved();
			}
			mGroup.Clean();
			mClan.Clear();
			mFightHelper.Clear();
			mSelfQuests.Clear();
			mNpcs.Clear();
			mCastleHelper.Clear();
			if (mMpdConnection != null)
			{
				mMpdConnection.Disconnect();
				mMpdConnection = null;
			}
			mLoggedIn = false;
			Log.Notice(mEntryPoint.Host + " logged out");
		}

		private void SendLogin()
		{
			UserNetData userData = mEntryPoint.UserData;
			mEntryPoint.Send(CtrlCmdId.user.login, new NamedVar("email", userData.EMail), new NamedVar("passwd", userData.Password), new NamedVar("version", userData.ClientVersion));
		}

		private void OnHeroData(HeroDataArg _arg)
		{
			mHeroExists = _arg.mPersExists;
		}

		private void OnHeroCreated()
		{
			mHeroExists = true;
		}

		private void GetGlobalBuffs()
		{
			mEntryPoint.Send(CtrlCmdId.user.get_global_buffs);
		}

		public void GetSelfBuffs()
		{
			mEntryPoint.Send(CtrlCmdId.user.get_buffs);
		}

		private void OnChatConf(ChatConfArg _arg)
		{
			mMpdConnection = new MpdConnection(mNetSys, _arg.mUid, _arg.mSid, mEntryPoint);
			mMpdConnection.SetAttemptCount(mConfig.MicroReConnectAttemptCount);
			mMpdConnection.SetAttemptTime(mConfig.MicroReConnectAttemptTime);
			mNetSys.Connect(_arg.mMpdHost, _arg.mMpdPorts, mMpdConnection);
		}

		private void OnCastleStart()
		{
			OnMapJoined(null);
		}

		private void OnMapJoined(JoinArg _arg)
		{
			mMapJoined = true;
			if (mAutoConnectSrv != null)
			{
				mAutoConnectSrv.WaitReconnect();
			}
		}

		public void StopWaitReconnect()
		{
			mMapJoined = false;
			if (mAutoConnectSrv != null)
			{
				mAutoConnectSrv.StopWaitReconnect();
			}
		}

		private void OnMapTypeDescs(MapTypeDescsArg _arg)
		{
			mMapTypeDescs.mDescs.Clear();
			foreach (KeyValuePair<int, string> mDesc in _arg.mDescs)
			{
				if (Enum.IsDefined(typeof(MapType), mDesc.Key))
				{
					MapType key = (MapType)mDesc.Key;
					mMapTypeDescs.mDescs[key] = mDesc.Value;
				}
			}
		}

		public void SendChatMessage(string _text, string _channel, IEnumerable<string> _recipients)
		{
			if (string.IsNullOrEmpty(_text))
			{
				throw new ArgumentNullException("_text");
			}
			if (string.IsNullOrEmpty(_channel))
			{
				throw new ArgumentNullException("_channel");
			}
			_text = ValidateChatMessage(_text);
			if (string.IsNullOrEmpty(_text))
			{
				return;
			}
			MixedArray mixedArray = new MixedArray();
			if (_recipients != null)
			{
				foreach (string _recipient in _recipients)
				{
					mixedArray.Add(_recipient);
				}
			}
			mEntryPoint.Send(CtrlCmdId.chat.add, new NamedVar("type", _channel), new NamedVar("recipient_list", mixedArray), new NamedVar("message", _text));
		}

		public void SendSetGag(string _userName, int _duration)
		{
			if (string.IsNullOrEmpty(_userName))
			{
				throw new ArgumentNullException("_userName");
			}
			mEntryPoint.Send(CtrlCmdId.chat.set_gag, new NamedVar("nick", _userName), new NamedVar("duration", _duration));
		}

		private string ValidateChatMessage(string _msg)
		{
			_msg = _msg.Replace("\n", " ");
			Regex regex = new Regex("[^a-zA-Z0-9а-яА-Яё\\s\\,\\.\\:\\;\\'\"\\?\\!\\#\\@\\&\\)\\(\\*\\^\\%\\$\\~\\`\\<\\>\\{\\}\\[\\]\\-\\_\\+\\=\\-\\|\\\\\\/]", RegexOptions.IgnoreCase);
			_msg = regex.Replace(_msg, "");
			Regex regex2 = new Regex("\\s{2,}", RegexOptions.IgnoreCase);
			return regex2.Replace(_msg, " ");
		}

		public void DownloadItemPrototypes(string _url)
		{
			if (!string.IsNullOrEmpty(_url))
			{
				WebTransferOperation webTransferOperation = new WebTransferOperation(_url);
				webTransferOperation.SetReconnectSettings(mConfig.WebReConnectAttemptCount, mConfig.MicroReConnectAttemptTime);
				webTransferOperation.mProxyCredential = mEntryPoint.UserData.ProxyCredential;
				webTransferOperation.mNotifiers.Add(new Notifier<TransferOperation, object>(OnProtoDownloaded, null));
				webTransferOperation.Begin();
			}
		}

		private void OnProtoDownloaded(bool _success, TransferOperation _op, object _data)
		{
			if (_success)
			{
				mConfig.ApplyThreadSettings();
				mProperties.RetrieveProperties(_op.Receiver);
			}
		}

		public IDictionary<int, ICollection<CtrlPrototype>> GetGroupedItems()
		{
			IDictionary<int, ICollection<CtrlPrototype>> dictionary = new Dictionary<int, ICollection<CtrlPrototype>>();
			foreach (int availableId in mProperties.AvailableIds)
			{
				CtrlPrototype ctrlPrototype = mProtoProv.Get(availableId);
				if (ctrlPrototype.Article != null)
				{
					if (!dictionary.TryGetValue(ctrlPrototype.Article.mTreeId, out var value))
					{
						value = new List<CtrlPrototype>();
						dictionary.Add(ctrlPrototype.Article.mTreeId, value);
					}
					if (ctrlPrototype.Article.mTreeSlot != -1)
					{
						value.Add(ctrlPrototype);
					}
				}
			}
			return dictionary;
		}

		public void DownloadQuests(string _url)
		{
			if (!string.IsNullOrEmpty(_url))
			{
				WebTransferOperation webTransferOperation = new WebTransferOperation(_url);
				webTransferOperation.SetReconnectSettings(mConfig.WebReConnectAttemptCount, mConfig.MicroReConnectAttemptTime);
				webTransferOperation.mProxyCredential = mEntryPoint.UserData.ProxyCredential;
				webTransferOperation.mNotifiers.Add(new Notifier<TransferOperation, object>(OnQuestsDownloaded, null));
				webTransferOperation.Begin();
			}
		}

		private void OnQuestsDownloaded(bool _success, TransferOperation _op, object _data)
		{
			if (_success)
			{
				mConfig.ApplyThreadSettings();
				mQuests.Retrieve(_op.Receiver);
			}
		}

		public void EnableAutoConnect(BattleServerConnection _battleSrv)
		{
			if (_battleSrv == null)
			{
				throw new ArgumentNullException("_battleSrv");
			}
			mAutoConnectSrv = _battleSrv;
		}

		public void DisableAutoConnect()
		{
			mAutoConnectSrv = null;
		}

		private void OnBattleServerData(ServerDataArg _arg)
		{
			mCurrentLocation = _arg.mAreaId;
			UserLog.SetData(this, _arg.mNeedLog);
			if (mAutoConnectSrv != null)
			{
				BattleServerData battleServerData = new BattleServerData();
				battleServerData.mHost = _arg.mHost;
				battleServerData.mPorts = _arg.mPorts;
				battleServerData.mPasswd = _arg.mPasswd;
				BattleMapData battleMapData = new BattleMapData();
				battleMapData.mMapName = _arg.mMap;
				mAutoConnectSrv.Connect(battleServerData, battleMapData);
			}
		}

		public void SendReconnectRequest()
		{
			if (mAutoConnectSrv != null)
			{
				mAutoConnectSrv.WaitReconnect();
			}
			mEntryPoint.Send(CtrlCmdId.common.area_conf);
		}

		public void SendReconnectRequest(int _id)
		{
			if (mAutoConnectSrv != null)
			{
				mAutoConnectSrv.WaitReconnect();
			}
			mEntryPoint.Send(CtrlCmdId.common.area_conf, new NamedVar("area_id", _id));
		}

		public void SendLog(MixedArray _items)
		{
			mEntryPoint.Send(CtrlCmdId.common.user_log, new NamedVar("items", _items));
		}

		public void SendGetFightLog(int _battleId, int _battleType)
		{
			if (_battleType == -1)
			{
				mEntryPoint.Send(CtrlCmdId.fight.log, new NamedVar("fight_id", _battleId));
			}
			else
			{
				mEntryPoint.Send(CtrlCmdId.fight.log, new NamedVar("fight_id", _battleId), new NamedVar("fight_type", _battleType));
			}
		}

		public void SendGetMoney()
		{
			mEntryPoint.Send(CtrlCmdId.user.money);
		}

		public void SendBankChange(int _diamondMoney, int _goldMoney)
		{
			mEntryPoint.Send(CtrlCmdId.bank.change, new NamedVar("money_d", _diamondMoney), new NamedVar("money_g", _goldMoney));
		}

		public void SendCanReconnect()
		{
			mEntryPoint.Send(CtrlCmdId.common.can_reconnect, new NamedVar("passwd", mEntryPoint.UserData.Password), new NamedVar("version", mEntryPoint.UserData.ClientVersion));
		}

		public void SendReconnect()
		{
			mEntryPoint.Send(CtrlCmdId.common.reconnect, new NamedVar("passwd", mEntryPoint.UserData.Password), new NamedVar("version", mEntryPoint.UserData.ClientVersion));
			SendFastReconnect();
		}

		public void SendFastReconnect()
		{
			mEntryPoint.Send(CtrlCmdId.common.reconnected);
		}

		private void OnReconnectData(ReconnectArg _arg)
		{
			mHeroExists = true;
			OnLogin(_arg.mLoginArg);
			OnBattleServerData(_arg.mServerDataArg);
		}

		public void SendLog(LogLevel _lvl, string _msg)
		{
			string value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value);
			stringBuilder.Append(" | ");
			stringBuilder.Append(_msg);
			mEntryPoint.Send(CtrlCmdId.log.insert, new NamedVar("type", (int)_lvl), new NamedVar("msg", stringBuilder.ToString()));
			Log.Debug("server log: " + stringBuilder.ToString());
		}
	}
}
