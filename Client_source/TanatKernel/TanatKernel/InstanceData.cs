using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Log4Tanat;

namespace TanatKernel
{
	public class InstanceData : IStorable, IMovable
	{
		public delegate void NextSyncCallback(SyncType _syncType, InstanceData _data);

		public interface IPosSyncListener
		{
			void ProcessNetInput(InstanceData _data);

			void ResetSyncData(bool _onVisible);

			void InstantResetSyncData();
		}

		private class CooldownData
		{
			public double mStartTime;

			public double mEndTime;
		}

		public delegate void EffectorsChangedCallback(InstanceData _data, int _effectorId);

		private int mId;

		private bool mVisible;

		private bool mRelevant;

		private bool mRelevantEnabled;

		private Dictionary<SyncType, NextSyncCallback> mNextSyncCallbacks = new Dictionary<SyncType, NextSyncCallback>();

		private List<IPosSyncListener> mPosSyncListeners = new List<IPosSyncListener>();

		private SyncedParams mParams;

		private int mLevel;

		private bool mPlayerBinded;

		private int mPlayerId;

		private IStoreContentProvider<Player> mPlayerProv;

		private Dictionary<int, TargetedAction> mCurActions = new Dictionary<int, TargetedAction>();

		private Dictionary<int, CooldownData> mActionColldown = new Dictionary<int, CooldownData>();

		private EffectorsChangedCallback mEffectorsChangedCallback = delegate
		{
		};

		private IStoreContentProvider<Effector> mEffectorProv;

		private ICollection<int> mEffectors = new List<int>();

		private int mAttackActionId = -1;

		private BattleTimer mTimer;

		public float mSpeed;

		private TeamRecognizer mTeamRecognizer;

		private GameObjectsProvider mGameObjProv;

		[CompilerGenerated]
		private static EffectorsChangedCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		public int Id => mId;

		public bool Visible
		{
			get
			{
				return mVisible;
			}
			set
			{
				if (mVisible != value && !mRelevantEnabled)
				{
					foreach (IPosSyncListener mPosSyncListener in mPosSyncListeners)
					{
						if (mVisible)
						{
							mPosSyncListener.ResetSyncData(_onVisible: true);
						}
						else
						{
							mPosSyncListener.InstantResetSyncData();
						}
					}
				}
				bool flag = mVisible;
				mVisible = value;
				Log.Info(mId + " visibility from " + flag + " to " + mVisible);
			}
		}

		public bool Relevant
		{
			get
			{
				return mRelevant;
			}
			set
			{
				if (!mRelevant && value)
				{
					mRelevantEnabled = true;
				}
				mRelevant = value;
			}
		}

		public SyncedParams Params
		{
			get
			{
				if (mParams == null)
				{
					mParams = new SyncedParams();
				}
				return mParams;
			}
		}

		public int Level
		{
			get
			{
				return mLevel;
			}
			set
			{
				mLevel = value;
				if (mLevel < 0)
				{
					Log.Warning(mId + " level " + mLevel + " < 0");
				}
			}
		}

		public bool IsPlayerBinded => mPlayerBinded;

		public Player Player
		{
			get
			{
				if (!mPlayerBinded)
				{
					Log.Warning("there is no binded with " + mId + " player");
					return null;
				}
				return mPlayerProv.Get(mPlayerId);
			}
		}

		public Player TryGetPlayer
		{
			get
			{
				if (!mPlayerBinded)
				{
					return null;
				}
				return mPlayerProv.TryGet(mPlayerId);
			}
		}

		public bool DoingAction => mCurActions.Count > 0;

		public IEnumerable<TargetedAction> CurActions => mCurActions.Values;

		public IEnumerable<int> EffectorIds => mEffectors;

		public Effector[] Effectors
		{
			get
			{
				if (mEffectorProv == null)
				{
					Log.Warning("cannot get effectors without effector provider");
					return new Effector[0];
				}
				List<Effector> list = new List<Effector>();
				foreach (int mEffector in mEffectors)
				{
					Effector effector = mEffectorProv.Get(mEffector);
					if (effector != null)
					{
						list.Add(effector);
					}
				}
				return list.ToArray();
			}
		}

		public int AttackActionId
		{
			get
			{
				if (mAttackActionId == -1)
				{
					List<Effector> list = new List<Effector>();
					GetEffectorsByType(SkillType.ATTACK, list);
					if (list.Count > 0)
					{
						mAttackActionId = list[0].Proto.Id;
					}
					else
					{
						Log.Warning("cannot find attack effector at " + mId);
					}
				}
				return mAttackActionId;
			}
		}

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

		public GameObjectsProvider GameObjProv
		{
			get
			{
				if (mGameObjProv == null)
				{
					Log.Warning("game object provider is null at " + mId);
				}
				return mGameObjProv;
			}
		}

		public InstanceData(int _id)
		{
			mId = _id;
		}

		public void Sync(SyncData _syncData, float _time)
		{
			Params.Update(_syncData, _time);
			if (mNextSyncCallbacks.TryGetValue(_syncData.SyncType, out var value))
			{
				mNextSyncCallbacks.Remove(_syncData.SyncType);
				value(_syncData.SyncType, this);
			}
			if (_syncData.SyncType != SyncType.POSITION && _syncData.SyncType != SyncType.POS_ANGLE)
			{
				return;
			}
			foreach (IPosSyncListener mPosSyncListener in mPosSyncListeners)
			{
				if (mRelevantEnabled)
				{
					mPosSyncListener.ResetSyncData(_onVisible: false);
				}
				mPosSyncListener.ProcessNetInput(this);
			}
			mRelevantEnabled = false;
		}

		public void SubscribeNextSyncCallback(SyncType _syncType, NextSyncCallback _callback)
		{
			mNextSyncCallbacks.TryGetValue(_syncType, out var value);
			value = (NextSyncCallback)Delegate.Combine(value, _callback);
			mNextSyncCallbacks[_syncType] = value;
		}

		public void AddPosSyncListener(IPosSyncListener _listener)
		{
			if (_listener == null)
			{
				throw new ArgumentNullException("_listener");
			}
			mPosSyncListeners.Add(_listener);
		}

		public void LevelUp()
		{
			mLevel++;
			Log.Debug(mId + " level: " + mLevel);
		}

		public void BindPlayer(int _playerId, IStoreContentProvider<Player> _playerProv)
		{
			if (_playerProv == null)
			{
				throw new NullReferenceException("_playerProv");
			}
			mPlayerProv = _playerProv;
			mPlayerId = _playerId;
			mPlayerBinded = true;
		}

		public void UnbindPlayer()
		{
			mPlayerBinded = false;
		}

		public void AddActionCooldown(int _id, double _startTime, double _endTime)
		{
			CooldownData cooldownData = new CooldownData();
			cooldownData.mStartTime = _startTime;
			cooldownData.mEndTime = _endTime;
			if (mActionColldown.ContainsKey(_id))
			{
				mActionColldown[_id] = cooldownData;
			}
			else
			{
				mActionColldown.Add(_id, cooldownData);
			}
		}

		public double GetCooldownProgress(int _id)
		{
			if (!mActionColldown.TryGetValue(_id, out var value))
			{
				return -1.0;
			}
			double totalSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
			double num = value.mEndTime - totalSeconds;
			double num2 = value.mEndTime - value.mStartTime;
			if (num <= 0.0)
			{
				mActionColldown.Remove(_id);
				return -1.0;
			}
			return num / num2;
		}

		public void DoAction(int _actionId, int _targetId, float _startTime)
		{
			if (IsDoingAction(_actionId))
			{
				Log.Error("action " + _actionId + " already added");
				return;
			}
			TargetedAction targetedAction = new TargetedAction(_actionId, _targetId, _startTime);
			mCurActions.Add(targetedAction.ActionId, targetedAction);
		}

		public void StopAction(int _actionId)
		{
			mCurActions.Remove(_actionId);
		}

		public void StopAllActions()
		{
			mCurActions.Clear();
		}

		public bool IsDoingAction(int _actionId)
		{
			return mCurActions.ContainsKey(_actionId);
		}

		public TargetedAction GetActionWithTarget()
		{
			foreach (TargetedAction value in mCurActions.Values)
			{
				if (value.HasTarget)
				{
					return value;
				}
			}
			return null;
		}

		public TargetedAction GetAction(int _actionId)
		{
			mCurActions.TryGetValue(_actionId, out var value);
			return value;
		}

		public TargetedAction[] GetActionsByTarget(int _targetId)
		{
			List<TargetedAction> list = new List<TargetedAction>();
			foreach (TargetedAction value in mCurActions.Values)
			{
				if (value.TargetId == _targetId)
				{
					list.Add(value);
				}
			}
			return list.ToArray();
		}

		public void SetEffectorProvider(IStoreContentProvider<Effector> _effectorProv)
		{
			if (_effectorProv == null)
			{
				throw new ArgumentNullException("_effectorProv");
			}
			mEffectorProv = _effectorProv;
		}

		public void AddEffector(int _effectorId)
		{
			if (mEffectors.Contains(_effectorId))
			{
				Log.Warning("secondary adding effector " + _effectorId + " to " + mId);
			}
			else
			{
				mEffectors.Add(_effectorId);
				mEffectorsChangedCallback(this, _effectorId);
			}
		}

		public void RemoveEffector(int _effectorId)
		{
			mEffectors.Remove(_effectorId);
			mEffectorsChangedCallback(this, -_effectorId);
		}

		public void RemoveAllEffectors()
		{
			foreach (int mEffector in mEffectors)
			{
				mEffectorsChangedCallback(this, -mEffector);
			}
			mEffectors.Clear();
		}

		public void SubscribeEffectorsChanged(EffectorsChangedCallback _callback)
		{
			mEffectorsChangedCallback = (EffectorsChangedCallback)Delegate.Combine(mEffectorsChangedCallback, _callback);
		}

		public void UnsubscribeEffectorsChanged(EffectorsChangedCallback _callback)
		{
			mEffectorsChangedCallback = (EffectorsChangedCallback)Delegate.Remove(mEffectorsChangedCallback, _callback);
		}

		public void GetEffectorsByType(SkillType _type, ICollection<Effector> _effectors)
		{
			if (_effectors == null)
			{
				throw new ArgumentNullException("_effectors");
			}
			if (mEffectorProv == null)
			{
				Log.Warning("cannot get effectors without effector provider");
				return;
			}
			foreach (int mEffector in mEffectors)
			{
				Effector effector = mEffectorProv.Get(mEffector);
				if (effector != null && effector.SkillType == _type)
				{
					_effectors.Add(effector);
				}
			}
		}

		public Effector GetSelfEffector(int _effectorId)
		{
			if (!mEffectors.Contains(_effectorId))
			{
				Log.Warning("cannot find effector " + _effectorId + " at " + mId);
				return null;
			}
			if (mEffectorProv == null)
			{
				Log.Warning("cannot get effector without effector provider");
				return null;
			}
			return mEffectorProv.Get(_effectorId);
		}

		public Effector GetSelfEffectorByProto(int _protoId)
		{
			if (mEffectorProv == null)
			{
				Log.Warning("cannot get effector without effector provider");
				return null;
			}
			foreach (int mEffector in mEffectors)
			{
				Effector effector = mEffectorProv.Get(mEffector);
				if (effector != null && effector.Proto.Id == _protoId)
				{
					return effector;
				}
			}
			return null;
		}

		public bool IsInType(int _action, params SkillType[] skillTypes)
		{
			List<Effector> list = new List<Effector>();
			foreach (SkillType type in skillTypes)
			{
				GetEffectorsByType(type, list);
			}
			foreach (Effector item in list)
			{
				if (item.Parent != null && item.Parent.Proto.Id == _action)
				{
					return true;
				}
			}
			return false;
		}

		public void SetTimer(BattleTimer _timer)
		{
			if (_timer == null)
			{
				throw new ArgumentNullException("_timer");
			}
			mTimer = _timer;
		}

		public void GetPosition(out float _x, out float _y)
		{
			Params.GetPosition(Time, out _x, out _y);
		}

		public void SetTeamRecognizer(TeamRecognizer _teamRecognizer)
		{
			if (_teamRecognizer == null)
			{
				throw new ArgumentNullException("_teamRecognizer");
			}
			mTeamRecognizer = _teamRecognizer;
		}

		public Friendliness GetFriendliness()
		{
			if (mTeamRecognizer == null)
			{
				Log.Warning(mId + " doesn't have a TeamRecognizer");
				return Friendliness.UNKNOWN;
			}
			return mTeamRecognizer.GetFriendliness(Params);
		}

		public bool IsFriend()
		{
			return GetFriendliness() == Friendliness.FRIEND;
		}

		public bool IsEnemy()
		{
			return GetFriendliness() == Friendliness.ENEMY;
		}

		public void SetGameObjectProvider(GameObjectsProvider _gameObjProv)
		{
			if (_gameObjProv == null)
			{
				throw new ArgumentNullException("_gameObjProv");
			}
			mGameObjProv = _gameObjProv;
		}
	}
}
