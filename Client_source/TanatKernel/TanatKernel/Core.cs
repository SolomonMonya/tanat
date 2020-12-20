using System;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class Core : IUpdatable, IBattleHolder
	{
		public delegate void BattleStartedCallback(Battle _battle);

		private UserNetData mUserNetData;

		private NetSystem mNetSystem;

		private BattleServerConnection mBattleSrv;

		private CtrlServerConnection mCtrlSrv;

		private IMapManager mMapMgr;

		private IGameObjectManager mGameObjMgr;

		private IHeroViewManager mHeroViewMgr;

		private Config mConfig;

		public BattleStartedCallback mBattleStartedCallback;

		private Battle mBattle;

		public UserNetData UserNetData => mUserNetData;

		public NetSystem NetSystem => mNetSystem;

		public BattleServerConnection BattleServer => mBattleSrv;

		public CtrlServerConnection CtrlServer => mCtrlSrv;

		public IMapManager MapManager => mMapMgr;

		public IGameObjectManager GameObjectManager => mGameObjMgr;

		public IHeroViewManager HeroViewManager => mHeroViewMgr;

		public Config Config => mConfig;

		public Battle Battle
		{
			get
			{
				if (mBattle == null)
				{
					Log.Error("try to access nonexistent battle\n" + Log.StackTrace());
				}
				return mBattle;
			}
		}

		public void SetMapManager(IMapManager _mapMgr)
		{
			mMapMgr = _mapMgr;
			mBattleSrv.SetMapManager(mMapMgr);
		}

		public void SetGameObjectManager(IGameObjectManager _gameObjMgr)
		{
			mGameObjMgr = _gameObjMgr;
		}

		public void SetHeroViewManager(IHeroViewManager _heroViewMgr)
		{
			mHeroViewMgr = _heroViewMgr;
		}

		public static string GetArg(string _key, string[] _args)
		{
			if (string.IsNullOrEmpty(_key))
			{
				throw new ArgumentException();
			}
			if (_args == null)
			{
				throw new ArgumentNullException("_args");
			}
			int num = Array.IndexOf(_args, _key);
			if (num < 0 || num >= _args.Length - 1)
			{
				return null;
			}
			return _args[num + 1];
		}

		public static string GetCommandLineArg(string _key)
		{
			return GetArg(_key, Environment.GetCommandLineArgs());
		}

		public void CreateBattle(BattlePacketManager _packetMgr, int _selfPlayerId)
		{
			if (mBattle != null)
			{
				throw new InvalidOperationException("battle already created");
			}
			Battle battle = new Battle(mGameObjMgr, mBattleSrv, mCtrlSrv, _selfPlayerId);
			if (mHeroViewMgr != null)
			{
				battle.EnableHeroView(mHeroViewMgr, mCtrlSrv.Heroes);
			}
			battle.Subscribe();
			mBattle = battle;
		}

		public bool IsBattleCreated()
		{
			return mBattle != null;
		}

		public void StartBattle()
		{
			if (mBattleStartedCallback != null)
			{
				mBattleStartedCallback(Battle);
			}
		}

		public void DisableBattle()
		{
			Battle.Disable();
		}

		public void DestroyBattle()
		{
			Battle.Clear();
			mBattle = null;
		}

		public Core(NetSystem _netSystem, Config _config, UserNetData _userData)
		{
			if (_netSystem == null)
			{
				throw new ArgumentNullException("_netSystem");
			}
			if (_config == null)
			{
				throw new ArgumentNullException("_config");
			}
			if (_userData == null)
			{
				throw new ArgumentNullException("_userData");
			}
			mNetSystem = _netSystem;
			mConfig = _config;
			mUserNetData = _userData;
			mBattleSrv = new BattleServerConnection(mNetSystem, mUserNetData, this);
			mCtrlSrv = new CtrlServerConnection(mUserNetData, mConfig, _netSystem);
			mBattleSrv.SetAttemptCount(mConfig.MicroReConnectAttemptCount);
			mBattleSrv.SetAttemptTime(mConfig.MicroReConnectAttemptTime);
		}

		public void Update()
		{
			mBattleSrv.Update();
			mCtrlSrv.Update();
		}
	}
}
