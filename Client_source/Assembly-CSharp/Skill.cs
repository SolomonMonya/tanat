using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class Skill
{
	public class EffectParams
	{
		public BattleData.EffectHolder mEffect;

		public int mOwnerObjId = -1;

		public int mTargetObjId = -1;

		public Vector3 mTargetPos;

		public List<int> mHandles = new List<int>();

		public GameObject mOwnerObj;

		public GameObject mTargetObj;
	}

	public static void StartEffects(bool _done, EffectParams _params, IStoreContentProvider<IGameObject> _gameObjProv)
	{
		if (_params.mEffect == null)
		{
			return;
		}
		VisualEffectsMgr instance = VisualEffectsMgr.Instance;
		SoundSystem instance2 = SoundSystem.Instance;
		if (_gameObjProv != null)
		{
			if (_params.mOwnerObjId != -1 && _params.mOwnerObj == null)
			{
				IGameObject gameObject = _gameObjProv.TryGet(_params.mOwnerObjId);
				if (gameObject != null)
				{
					_params.mOwnerObj = (gameObject as GameData).gameObject;
				}
			}
			if (_params.mTargetObjId != -1 && _params.mTargetObj == null)
			{
				IGameObject gameObject2 = _gameObjProv.TryGet(_params.mTargetObjId);
				if (gameObject2 != null)
				{
					_params.mTargetObj = (gameObject2 as GameData).gameObject;
				}
			}
		}
		if (_params.mEffect.mPreventDisable && _params.mOwnerObj != null)
		{
			PreventDisable component = _params.mOwnerObj.GetComponent<PreventDisable>();
			if (component != null)
			{
				component.mEnabled = true;
			}
		}
		if (_params.mEffect.mGFX != null)
		{
			BattleData.GraphicsEffect[] mGFX = _params.mEffect.mGFX;
			foreach (BattleData.GraphicsEffect graphicsEffect in mGFX)
			{
				if (graphicsEffect.mOnDone != _done)
				{
					continue;
				}
				int num = -1;
				switch (graphicsEffect.mTarget)
				{
				case FxSkillTarget.SELF:
					if (_params.mOwnerObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new GameObject[1]
						{
							_params.mOwnerObj
						});
					}
					break;
				case FxSkillTarget.SELF_TO_TARGETPOS:
					if (_params.mOwnerObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new Vector3[2]
						{
							_params.mOwnerObj.transform.position,
							_params.mTargetPos
						});
					}
					break;
				case FxSkillTarget.SELF_TO_TARGET:
					if (_params.mOwnerObj != null && _params.mTargetObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new GameObject[2]
						{
							_params.mOwnerObj,
							_params.mTargetObj
						});
					}
					break;
				case FxSkillTarget.TARGET:
					if (_params.mTargetObjId == -1)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new Vector3[1]
						{
							_params.mTargetPos
						});
					}
					else if (_params.mTargetObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new GameObject[1]
						{
							_params.mTargetObj
						});
					}
					break;
				case FxSkillTarget.TARGETPOS:
					if (_params.mTargetObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new Vector3[1]
						{
							_params.mTargetObj.transform.position
						});
					}
					break;
				case FxSkillTarget.SELFPOS:
					if (_params.mOwnerObj != null)
					{
						num = instance.PlayEffect(graphicsEffect.mName, graphicsEffect.mTime, 1f, new Vector3[1]
						{
							_params.mOwnerObj.transform.position
						});
					}
					break;
				default:
					continue;
				}
				if (num != -1 && graphicsEffect.mStopOnDone)
				{
					_params.mHandles.Add(num);
				}
			}
		}
		if (_params.mEffect.mSFX != null)
		{
			BattleData.SoundEffect[] mSFX = _params.mEffect.mSFX;
			foreach (BattleData.SoundEffect soundEffect in mSFX)
			{
				if (soundEffect.mOnDone != _done)
				{
					continue;
				}
				SoundSystem.Sound sound = new SoundSystem.Sound();
				sound.mName = soundEffect.mName;
				sound.mPath = soundEffect.mPath;
				sound.mOptions = soundEffect.mOptions;
				if (soundEffect.mTarget == FxSkillTarget.TARGET)
				{
					if (_params.mTargetObj != null)
					{
						instance2.PlaySoundEffect(sound, soundEffect.mTime, _params.mTargetObj);
					}
				}
				else if (_params.mOwnerObj != null)
				{
					instance2.PlaySoundEffect(sound, soundEffect.mTime, _params.mOwnerObj);
				}
			}
		}
		if (_done || string.IsNullOrEmpty(_params.mEffect.mAnimation) || !(_params.mOwnerObj != null))
		{
			return;
		}
		AnimationExt componentInChildren = _params.mOwnerObj.GetComponentInChildren<AnimationExt>();
		if (componentInChildren != null)
		{
			if (_params.mEffect.mLoopAnimation)
			{
				componentInChildren.SetWrapMode(_params.mEffect.mAnimation, WrapMode.Loop);
			}
			componentInChildren.PlayAnimation(_params.mEffect.mAnimation);
		}
	}

	public static void StopEffects(EffectParams _params, IStoreContentProvider<IGameObject> _gameObjProv)
	{
		if (_params.mEffect == null)
		{
			return;
		}
		VisualEffectsMgr instance = VisualEffectsMgr.Instance;
		foreach (int mHandle in _params.mHandles)
		{
			instance.StopEffect(mHandle);
		}
		_params.mHandles.Clear();
		if (string.IsNullOrEmpty(_params.mEffect.mAnimation) || !_params.mEffect.mLoopAnimation)
		{
			return;
		}
		if (_gameObjProv != null && _params.mOwnerObjId != -1 && _params.mOwnerObj == null)
		{
			IGameObject gameObject = _gameObjProv.TryGet(_params.mOwnerObjId);
			if (gameObject != null)
			{
				_params.mOwnerObj = (gameObject as GameData).gameObject;
			}
		}
		if (_params.mOwnerObj != null)
		{
			AnimationExt componentInChildren = _params.mOwnerObj.GetComponentInChildren<AnimationExt>();
			componentInChildren.StopAnimation(_params.mEffect.mAnimation);
		}
	}
}
