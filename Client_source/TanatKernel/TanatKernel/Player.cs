using System;
using System.Collections.Generic;
using Log4Tanat;

namespace TanatKernel
{
	public class Player : IStorable
	{
		public class RespTimeData
		{
			private float mRespTime;

			private Player mPlayer;

			public RespTimeData(float _respTime, Player _player)
			{
				if (_player == null)
				{
					throw new ArgumentNullException("_player");
				}
				mRespTime = _respTime;
				mPlayer = _player;
			}

			public int GetTimeToRespawn()
			{
				float time = mPlayer.Time;
				return (int)(mRespTime - time + 1f);
			}
		}

		public class RespCostData
		{
			private int mCost;

			private Currency mCurrency;

			public int Cost => mCost;

			public Currency Currency => mCurrency;

			public RespCostData(int _cost, Currency _currency)
			{
				mCost = _cost;
				mCurrency = _currency;
			}
		}

		private int mId;

		private string mName;

		private int mTeam;

		private bool mIsSelf;

		private int mLevel;

		private bool mIsOnline;

		private int mAvatarObjId = -1;

		private bool mAvatarBinded;

		private IStoreContentProvider<IGameObject> mGameObjProv;

		private AvatarData mAvatarData;

		private IStoreContentProvider<Hero> mHeroProv;

		public int mKillsWithoutDeath;

		public Queue<float> mKillTime = new Queue<float>();

		private int mDeathsCnt;

		private int mKillsCnt;

		private int mAssistsCnt;

		private List<int> mActiveItems = new List<int>();

		private RespTimeData mRespTimeData;

		private RespCostData mRespCostData;

		private float mLastRespTime;

		private BattleTimer mTimer;

		public int Id => mId;

		public string Name => mName;

		public int Team => mTeam;

		public bool IsSelf => mIsSelf;

		public int Level
		{
			get
			{
				return mLevel;
			}
			set
			{
				if (mLevel != value)
				{
					Log.Notice("player " + mId + " level " + value);
				}
				mLevel = value;
				if (mLevel < 0)
				{
					Log.Warning("player " + mId + " level " + value + " < 0");
				}
			}
		}

		public bool IsOnline
		{
			get
			{
				return mIsOnline;
			}
			set
			{
				mIsOnline = value;
			}
		}

		public bool IsAvatarBinded => mAvatarBinded;

		public int AvatarObjId => mAvatarObjId;

		public AvatarData AvatarData => mAvatarData;

		public IGameObject Avatar
		{
			get
			{
				if (!mAvatarBinded)
				{
					return null;
				}
				if (mGameObjProv == null)
				{
					Log.Warning("null object provider\n" + Log.StackTrace());
					return null;
				}
				return mGameObjProv.Get(mAvatarObjId);
			}
		}

		public Hero Hero
		{
			get
			{
				if (mHeroProv == null)
				{
					Log.Warning("cannot get hero by player id without hero provider");
					return null;
				}
				return mHeroProv.Get(mId);
			}
		}

		public int DeathsCount
		{
			get
			{
				return mDeathsCnt;
			}
			set
			{
				if (value >= 0)
				{
					mDeathsCnt = value;
				}
				else
				{
					Log.Warning("invalid deaths count: " + value);
				}
			}
		}

		public int KillsCount
		{
			get
			{
				return mKillsCnt;
			}
			set
			{
				if (value >= 0)
				{
					mKillsCnt = value;
				}
				else
				{
					Log.Warning("invalid kills count: " + value);
				}
			}
		}

		public int AssistsCount
		{
			get
			{
				return mAssistsCnt;
			}
			set
			{
				mAssistsCnt = value;
			}
		}

		public IEnumerable<int> ActiveItems => mActiveItems;

		public RespTimeData RespTime => mRespTimeData;

		public RespCostData RespCost => mRespCostData;

		public float LastRespawnTime => mLastRespTime;

		public float Time
		{
			get
			{
				if (mTimer == null)
				{
					Log.Warning(mId + " doesn't have a timer");
					return 0f;
				}
				return mTimer.Time;
			}
		}

		public Player(int _id, string _name, int _team, bool _isSelf)
		{
			mId = _id;
			mName = _name;
			mTeam = _team;
			mIsSelf = _isSelf;
		}

		public void SetAvatarData(AvatarData _avatarData)
		{
			mAvatarData = _avatarData;
		}

		public void BindAvatar(int _avatarObjId, IStoreContentProvider<IGameObject> _gameObjProv)
		{
			mAvatarObjId = _avatarObjId;
			Log.Notice("player: " + mId + ", avatar: " + mAvatarObjId);
			mGameObjProv = _gameObjProv;
			mAvatarBinded = true;
		}

		public void UnbindAvatar()
		{
			mActiveItems.Clear();
			mAvatarBinded = false;
			mAvatarObjId = -1;
			Log.Notice("player: " + mId + ", avatar unbinded");
		}

		public void SetHeroProvider(IStoreContentProvider<Hero> _heroProv)
		{
			if (_heroProv == null)
			{
				throw new ArgumentNullException("_heroProv");
			}
			mHeroProv = _heroProv;
		}

		public void Equip(int _itemProtoId)
		{
			if (!mAvatarBinded)
			{
				Log.Warning("avatar not binded");
			}
			else
			{
				mActiveItems.Add(_itemProtoId);
			}
		}

		public void Unequip(int _itemProtoId)
		{
			if (!mAvatarBinded)
			{
				Log.Warning("avatar not binded");
			}
			else
			{
				mActiveItems.Remove(_itemProtoId);
			}
		}

		public void SetRespTimeData(float _respTime)
		{
			mRespTimeData = new RespTimeData(_respTime, this);
		}

		public void SetRespCostData(int _respCost, Currency _currency)
		{
			mRespCostData = new RespCostData(_respCost, _currency);
		}

		public void PerformRespawn()
		{
			mRespTimeData = null;
			mRespCostData = null;
			mLastRespTime = Time;
		}

		public void SetTimer(BattleTimer _timer)
		{
			if (_timer == null)
			{
				throw new ArgumentNullException("_timer");
			}
			mTimer = _timer;
		}
	}
}
