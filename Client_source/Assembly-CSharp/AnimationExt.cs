using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class AnimationExt : MonoBehaviour
{
	public enum State
	{
		idle,
		run,
		attack,
		cast,
		other,
		death
	}

	[Serializable]
	public enum AnimRestrictionType
	{
		NONE,
		ATTACK_SPEED,
		MOVE_SPEED
	}

	[Serializable]
	public class Anim : Utils.Named
	{
		[Serializable]
		public class Callback
		{
			public float mTime;

			public string mFunc;

			public string mData;

			public float mProbability;
		}

		[Serializable]
		public class AnimRestriction
		{
			public AnimRestrictionType mType;

			public float mMinValue;

			public float mMaxValue;
		}

		private float mPrevCheckTime;

		public State mState;

		public float mWeight;

		public Callback[] mCallbacks;

		public AnimRestriction[] mRestictions;

		public void CheckCallbacks(Animation _animSys, GameObject _go)
		{
			if (mCallbacks == null || mCallbacks.Length == 0)
			{
				return;
			}
			AnimationState animationState = _animSys[mName];
			if (animationState == null)
			{
				Log.Error(_animSys.gameObject.name + " has error with anim callback for : " + mName);
			}
			else
			{
				if (animationState.weight == 0f)
				{
					return;
				}
				float num = animationState.time % animationState.length;
				Callback[] array = mCallbacks;
				foreach (Callback callback in array)
				{
					if (mPrevCheckTime > num)
					{
						mPrevCheckTime = 0f;
					}
					if (num > callback.mTime && mPrevCheckTime <= callback.mTime)
					{
						bool flag = false;
						if (callback.mProbability >= 1f || callback.mProbability >= UnityEngine.Random.value)
						{
							_go.SendMessage(callback.mFunc, callback.mData, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				mPrevCheckTime = num;
			}
		}
	}

	private class AnimationAction
	{
		private int mCurVariant;

		private float mTimeOffset = -1f;

		private float mChangeAnimTime;

		private List<Anim> mVariants;

		private List<Anim> mAvailableVariants;

		private AnimationExt mAnimationExt;

		public AnimationAction(AnimationExt _animExt)
		{
			mAnimationExt = _animExt;
		}

		public void AddVariant(Anim _anim)
		{
			if (mVariants == null)
			{
				mVariants = new List<Anim>();
			}
			mVariants.Add(_anim);
			mCurVariant = GenerateAnimNum();
		}

		public string SelectAnimation(Animation _animSys)
		{
			if (mAvailableVariants == null)
			{
				return string.Empty;
			}
			string mName = mAvailableVariants[mCurVariant].mName;
			AnimationState animationState = _animSys[mName];
			if (animationState == null)
			{
				Log.Error("null animation " + mName + " at " + _animSys.gameObject.name);
				return string.Empty;
			}
			if (mChangeAnimTime > 0f)
			{
				float num = Time.time - mChangeAnimTime;
				animationState.time += num;
				mChangeAnimTime = 0f;
			}
			if (animationState.time + Time.deltaTime > animationState.length && animationState.wrapMode != WrapMode.Loop)
			{
				int num2 = GenerateAnimNum();
				if (num2 != mCurVariant)
				{
					mChangeAnimTime = Time.time;
				}
				else
				{
					int num3 = Mathf.FloorToInt(animationState.time / animationState.length);
					animationState.time -= (float)num3 * animationState.length;
				}
				mCurVariant = num2;
			}
			return mAvailableVariants[mCurVariant].mName;
		}

		private List<Anim> GetAvailableVariants()
		{
			if (mAnimationExt == null)
			{
				return null;
			}
			List<Anim> list = new List<Anim>();
			foreach (Anim mVariant in mVariants)
			{
				if (mVariant.mRestictions == null)
				{
					list.Add(mVariant);
					continue;
				}
				int num = 0;
				Anim.AnimRestriction[] mRestictions = mVariant.mRestictions;
				foreach (Anim.AnimRestriction animRestriction in mRestictions)
				{
					switch (animRestriction.mType)
					{
					case AnimRestrictionType.NONE:
						num++;
						break;
					case AnimRestrictionType.MOVE_SPEED:
						if (mAnimationExt.mRunSpeed >= animRestriction.mMinValue && mAnimationExt.mRunSpeed <= animRestriction.mMaxValue)
						{
							num++;
						}
						break;
					case AnimRestrictionType.ATTACK_SPEED:
						if (mAnimationExt.mAttackSpeed >= animRestriction.mMinValue && mAnimationExt.mAttackSpeed <= animRestriction.mMaxValue)
						{
							num++;
						}
						break;
					}
				}
				if (num == mVariant.mRestictions.Length)
				{
					list.Add(mVariant);
				}
			}
			return list;
		}

		private int GenerateAnimNum()
		{
			mAvailableVariants = GetAvailableVariants();
			if (mAvailableVariants == null)
			{
				return 0;
			}
			float num = 0f;
			foreach (Anim mAvailableVariant in mAvailableVariants)
			{
				num += mAvailableVariant.mWeight;
			}
			if (num > 0f)
			{
				float num2 = UnityEngine.Random.Range(0f, num);
				num = 0f;
				int i = 0;
				for (int count = mAvailableVariants.Count; i < count; i++)
				{
					num += mAvailableVariants[i].mWeight;
					if (num2 <= num)
					{
						return i;
					}
				}
			}
			return UnityEngine.Random.Range(0, mAvailableVariants.Count);
		}

		public void OffsetTimeOnce(float _time)
		{
			mTimeOffset = _time;
		}

		public void Play(Animation _animSys, bool _blending, float _crossFadeLength)
		{
			string text = SelectAnimation(_animSys);
			if (mTimeOffset > 0f)
			{
				AnimationState animationState = _animSys[text];
				float num2 = (animationState.time = mTimeOffset % animationState.length);
				mTimeOffset = -1f;
			}
			Play(_animSys, text, _blending, _crossFadeLength);
		}

		public static void Play(Animation _animSys, string _animName, bool _blending, float _crossFadeLength)
		{
			if (_animName != null && _animName.Length != 0)
			{
				if (_blending)
				{
					_animSys.CrossFade(_animName, _crossFadeLength, PlayMode.StopSameLayer);
				}
				else if (!_animSys.IsPlaying(_animName))
				{
					_animSys.Play(_animName);
				}
			}
		}

		public void SetSpeed(Animation _animSys, float _speed, ref float _resultAnimSpeed)
		{
			if (mVariants == null)
			{
				return;
			}
			string empty = string.Empty;
			AnimationState animationState = null;
			foreach (Anim mVariant in mVariants)
			{
				empty = mVariant.mName;
				animationState = _animSys[empty];
				if (animationState == null)
				{
					Log.Error("null animation " + empty);
					break;
				}
				animationState.speed = animationState.length * _speed;
				_resultAnimSpeed = animationState.speed;
			}
		}

		public void SetConfiguredSpeed(Animation _animSys, float _speed, float _confSpeed, ref float _resultAnimSpeed)
		{
			if (mVariants == null)
			{
				return;
			}
			string empty = string.Empty;
			AnimationState animationState = null;
			foreach (Anim mVariant in mVariants)
			{
				empty = mVariant.mName;
				animationState = _animSys[empty];
				if (animationState == null)
				{
					Log.Error("null animation " + empty);
					break;
				}
				float num = _speed / (_confSpeed * animationState.length);
				animationState.speed = num * animationState.length;
				_resultAnimSpeed = animationState.speed;
			}
		}

		public float GetCurAnimLength(Animation _animSys)
		{
			if (mAvailableVariants == null)
			{
				return 0f;
			}
			string mName = mAvailableVariants[mCurVariant].mName;
			AnimationState animationState = _animSys[mName];
			if (animationState == null)
			{
				Log.Error("null animation " + mName);
				return 0f;
			}
			return animationState.length;
		}

		public bool IsPlaying(Animation _animSys)
		{
			if (mVariants == null)
			{
				return false;
			}
			foreach (Anim mVariant in mVariants)
			{
				if (_animSys.IsPlaying(mVariant.mName))
				{
					AnimationState animationState = _animSys[mVariant.mName];
					if (animationState.wrapMode != WrapMode.ClampForever || !(animationState.time >= animationState.length))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void Reset(Animation _animSys)
		{
			if (mVariants == null)
			{
				return;
			}
			foreach (Anim mVariant in mVariants)
			{
				AnimationState animationState = _animSys[mVariant.mName];
				if (!(animationState == null))
				{
					animationState.weight = 0f;
					animationState.enabled = false;
					animationState.speed = 1f;
					animationState.time = 0f;
				}
			}
		}
	}

	public float mCrossFadeLength = 0.2f;

	public Anim[] mAnimations;

	public float mRunSpeed = 1f;

	public bool mAutoActions = true;

	public bool mUseBlending = true;

	private Animation mAnimation;

	private AnimationAction[] mActions;

	private float mAttackSpeed;

	private bool mAttackState;

	private float mCurAnimSpeed = 6f;

	private GameData mGameData;

	public void Start()
	{
		mActions = new AnimationAction[6];
		for (int i = 0; i < mActions.Length; i++)
		{
			mActions[i] = new AnimationAction(this);
		}
		Anim[] array = mAnimations;
		foreach (Anim anim in array)
		{
			mActions[(int)anim.mState].AddVariant(anim);
		}
	}

	public void OnBecameVisible()
	{
		if (!base.enabled)
		{
			base.enabled = true;
		}
	}

	public void OnBecameInvisible()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
	}

	public void OnEnable()
	{
		InitAnimations();
	}

	public void SetGameData(GameData _gd)
	{
		mGameData = _gd;
	}

	public void Reset()
	{
		if (mAnimation == null)
		{
			return;
		}
		mAutoActions = true;
		mAnimation.Stop();
		foreach (AnimationState item in mAnimation)
		{
			item.weight = 0f;
			item.speed = 1f;
			item.time = 0f;
			item.enabled = true;
		}
	}

	private void InitAnimations()
	{
		if (mAnimation != null)
		{
			return;
		}
		mAnimation = base.gameObject.GetComponentInChildren<Animation>();
		if (mAnimation == null)
		{
			Log.Warning("AnimationExt in " + base.gameObject.name + " needs Animation component");
			return;
		}
		Anim[] array = mAnimations;
		foreach (Anim anim in array)
		{
			AnimationState animationState = mAnimation[anim.mName];
			if (animationState == null)
			{
				Log.Warning("AnimationState " + anim.mName + " does not exists in " + base.gameObject.name);
				break;
			}
			if (mUseBlending)
			{
				switch (anim.mState)
				{
				case State.idle:
					animationState.layer = 0;
					animationState.wrapMode = WrapMode.Once;
					break;
				case State.run:
					animationState.layer = 0;
					animationState.wrapMode = WrapMode.Once;
					break;
				case State.attack:
					animationState.layer = 1;
					animationState.wrapMode = WrapMode.Once;
					break;
				case State.cast:
					animationState.layer = 2;
					animationState.wrapMode = WrapMode.Once;
					break;
				case State.other:
					animationState.layer = 3;
					animationState.wrapMode = WrapMode.Once;
					break;
				case State.death:
					animationState.layer = 4;
					animationState.wrapMode = WrapMode.Once;
					break;
				}
			}
			else
			{
				animationState.wrapMode = WrapMode.Once;
			}
		}
	}

	public bool IsValid()
	{
		if (mGameData == null)
		{
			return false;
		}
		if (mAnimation == null)
		{
			return false;
		}
		return mAnimation.enabled && mActions != null;
	}

	public static AnimationState SafeGetAnimationState(Animation _animSys, ref string _name)
	{
		foreach (AnimationState _animSy in _animSys)
		{
			if (_name.Equals(_animSy.name, StringComparison.OrdinalIgnoreCase))
			{
				_name = _animSy.name;
				return _animSy;
			}
		}
		return null;
	}

	public void Update()
	{
		if (!IsValid())
		{
			return;
		}
		if (mAutoActions)
		{
			if (mUseBlending)
			{
				ActWithBlending();
			}
			else
			{
				ActWithoutBlending();
			}
		}
		Anim[] array = mAnimations;
		foreach (Anim anim in array)
		{
			anim.CheckCallbacks(mAnimation, base.gameObject);
		}
	}

	private void ActWithBlending()
	{
		bool flag = false;
		NetSyncTransform netTransform = mGameData.NetTransform;
		if (netTransform != null && netTransform.enabled)
		{
			flag = netTransform.IsMoving();
		}
		bool flag2 = false;
		float num = 0f;
		TargetedAction targetedAction = null;
		if (mGameData.Data.DoingAction)
		{
			targetedAction = mGameData.Data.GetAction(mGameData.Data.AttackActionId);
		}
		if (targetedAction != null && targetedAction.HasTarget)
		{
			float time = mGameData.Data.Time;
			num = time - targetedAction.StartTime;
			flag2 = num >= 0f;
		}
		if (flag)
		{
			if (!flag2)
			{
				mActions[2].Reset(mAnimation);
				SyncedParams @params = mGameData.Data.Params;
				if (@params != null)
				{
					mActions[1].SetConfiguredSpeed(mAnimation, @params.mSpeed, mRunSpeed, ref mCurAnimSpeed);
				}
			}
			PlayAction(State.run);
		}
		else if (!flag2)
		{
			PlayAction(State.idle);
			mCurAnimSpeed = 1f;
		}
		if (flag2)
		{
			SyncedParams params2 = mGameData.Data.Params;
			if (!mAttackState)
			{
				mActions[2].OffsetTimeOnce(num);
				if (params2 != null)
				{
					SetAttackSpeed(params2.mAttackSpeed);
				}
			}
			else if (params2 != null && mAttackSpeed != params2.mAttackSpeed)
			{
				SetAttackSpeed(params2.mAttackSpeed);
			}
			PlayAction(State.attack);
		}
		mAttackState = flag2;
	}

	private void SetAttackSpeed(float _speed)
	{
		mAttackSpeed = _speed;
		mActions[2].SetSpeed(mAnimation, mAttackSpeed, ref mCurAnimSpeed);
	}

	private void ActWithoutBlending()
	{
		bool flag = false;
		NetSyncTransform netTransform = mGameData.NetTransform;
		if (netTransform != null && netTransform.enabled)
		{
			flag = netTransform.IsMoving();
		}
		if (flag)
		{
			mActions[1].SetSpeed(mAnimation, mRunSpeed, ref mCurAnimSpeed);
			PlayAction(State.run);
			return;
		}
		bool flag2 = false;
		TargetedAction targetedAction = null;
		if (mGameData.Data.DoingAction)
		{
			targetedAction = mGameData.Data.GetAction(mGameData.Data.AttackActionId);
		}
		if (targetedAction != null)
		{
			flag2 = targetedAction.HasTarget;
		}
		if (flag2)
		{
			PlayAction(State.attack);
		}
		else
		{
			PlayAction(State.idle);
		}
	}

	public void PlayAction(State _state)
	{
		if (!IsValid())
		{
			return;
		}
		if (_state == State.death)
		{
			foreach (AnimationState item in mAnimation)
			{
				item.enabled = false;
				item.wrapMode = WrapMode.Once;
				item.time = 0f;
			}
			mAutoActions = false;
		}
		mActions[(int)_state].Play(mAnimation, mUseBlending, mCrossFadeLength);
	}

	public void PlayAnimation(string _animName)
	{
		if (!IsValid())
		{
			return;
		}
		AnimationState animationState = SafeGetAnimationState(mAnimation, ref _animName);
		if (animationState == null)
		{
			for (int i = 0; i <= 5; i++)
			{
				State state = (State)i;
				if (_animName.Equals(state.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					PlayAction(state);
				}
			}
		}
		else
		{
			AnimationAction.Play(mAnimation, _animName, _blending: true, mCrossFadeLength);
		}
	}

	public void SetWrapMode(string _animName, WrapMode _wm)
	{
		if (IsValid())
		{
			AnimationState animationState = SafeGetAnimationState(mAnimation, ref _animName);
			if (!(animationState == null))
			{
				animationState.wrapMode = _wm;
			}
		}
	}

	public void StopAnimation(string _animName)
	{
		if (IsValid())
		{
			AnimationState animationState = SafeGetAnimationState(mAnimation, ref _animName);
			if (!(animationState == null))
			{
				animationState.enabled = false;
				animationState.time = 0f;
				animationState.wrapMode = WrapMode.Once;
			}
		}
	}

	public float GetCurAnimationLength(State _state)
	{
		if (!IsValid())
		{
			return 0f;
		}
		return mActions[(int)_state].GetCurAnimLength(mAnimation);
	}

	public bool IsPlaying(State _state)
	{
		if (!IsValid())
		{
			return false;
		}
		if (!VisibilityMgr.IsVisible(base.gameObject))
		{
			return false;
		}
		return mActions[(int)_state].IsPlaying(mAnimation);
	}

	public void PlayEffect(string _effectName)
	{
		GameObject[] targets = new GameObject[1]
		{
			base.gameObject
		};
		VisualEffectsMgr.Instance.PlayEffect(_effectName, 0f, mCurAnimSpeed, targets);
	}

	public void PlayTargetedEffect(string _effectName)
	{
		if (mGameData == null)
		{
			return;
		}
		TargetedAction actionWithTarget = mGameData.Data.GetActionWithTarget();
		if (actionWithTarget != null)
		{
			IGameObject gameObject = mGameData.Data.GameObjProv.Get(actionWithTarget.TargetId);
			if (gameObject != null)
			{
				GameObject[] targets = new GameObject[2]
				{
					base.gameObject,
					(gameObject as GameData).gameObject
				};
				VisualEffectsMgr.Instance.PlayEffect(_effectName, targets);
			}
		}
	}
}
