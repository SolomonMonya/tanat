using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class SoundEmiter : MonoBehaviour
{
	[Serializable]
	public class SoundElement
	{
		public string mSkillId;

		public string mSoundName;

		public string mSoundPath;

		public int mSoundPriority;

		public SoundSystem.SoundOptions mOptions;

		public SoundElement()
		{
		}

		public SoundElement(string _id, string _name, string _path, int _priority)
		{
			mSkillId = _id;
			mSoundName = _name;
			mSoundPath = _path;
			mSoundPriority = _priority;
			mOptions = new SoundSystem.SoundOptions();
		}
	}

	public enum SoundType
	{
		RUN,
		ATTACK,
		SKILL_USE,
		KILL
	}

	[Serializable]
	public class SoundSet
	{
		public SoundType mSoundType;

		public int mProbability;

		public List<SoundElement> mElements;
	}

	private class DeferredPlayData
	{
		public float mStartTime;

		public SoundSystem.Sound mSound;
	}

	public SoundSystem.Sound[] mSounds;

	public Utils.VariantsList[] mActions;

	public int mSoundsLim = 3;

	public List<SoundSet> mSoundSets = new List<SoundSet>();

	private SoundSystem mSoundSys;

	private AudioSource[] mAudio;

	private List<DeferredPlayData> mDeferredSounds = new List<DeferredPlayData>();

	public AudioSource[] Audio => mAudio;

	private void Start()
	{
		foreach (SoundSet mSoundSet in mSoundSets)
		{
			foreach (SoundElement mElement in mSoundSet.mElements)
			{
				mElement.mOptions.UsePann = true;
			}
		}
	}

	public void Init()
	{
		mSoundSys = SoundSystem.Instance;
		mAudio = new AudioSource[mSoundsLim];
		int num = mAudio.Length - 1;
		AudioSource[] components = base.gameObject.GetComponents<AudioSource>();
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (num < 0)
			{
				break;
			}
			audioSource.Stop();
			audioSource.clip = null;
			mAudio[num--] = audioSource;
		}
		while (num >= 0)
		{
			AudioSource audioSource2 = base.gameObject.AddComponent<AudioSource>();
			mAudio[num--] = audioSource2;
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
		if (mAudio == null)
		{
			Init();
		}
	}

	public void OnDisable()
	{
		try
		{
			AudioSource[] array = mAudio;
			foreach (AudioSource audioSource in array)
			{
				audioSource.Stop();
				audioSource.clip = null;
			}
		}
		catch (MissingReferenceException)
		{
		}
	}

	public void DeferredPlay(SoundSystem.Sound _snd, float _dt)
	{
		if (_snd == null)
		{
			throw new ArgumentNullException("_snd");
		}
		if (mSoundSys == null)
		{
			return;
		}
		DeferredPlayData deferredPlayData = new DeferredPlayData();
		deferredPlayData.mStartTime = Time.realtimeSinceStartup + _dt;
		deferredPlayData.mSound = _snd;
		for (int num = mDeferredSounds.Count - 1; num >= 0; num--)
		{
			if (mDeferredSounds[num] == null)
			{
				mDeferredSounds[num] = deferredPlayData;
				deferredPlayData = null;
				break;
			}
		}
		if (deferredPlayData != null)
		{
			mDeferredSounds.Add(deferredPlayData);
		}
	}

	public void Play(SoundSystem.Sound _snd)
	{
		if (!(mSoundSys == null))
		{
			if (!base.enabled)
			{
				DeferredPlay(_snd, 0f);
				return;
			}
			DeferredPlayData deferredPlayData = new DeferredPlayData();
			deferredPlayData.mStartTime = Time.realtimeSinceStartup;
			deferredPlayData.mSound = _snd;
			LoadClipAndPlay(deferredPlayData);
		}
	}

	private void LoadClipAndPlay(DeferredPlayData _d)
	{
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>();
		notifier.mData = _d;
		notifier.mCallback = OnClipReady;
		mSoundSys.GetClip(_d.mSound.GetName(), _d.mSound.GetPath(), notifier);
	}

	public void OnClipReady(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success && !(this == null) && base.enabled)
		{
			DeferredPlayData deferredPlayData = _data as DeferredPlayData;
			float num = Time.realtimeSinceStartup - deferredPlayData.mStartTime;
			if (!(num > deferredPlayData.mSound.mOptions.mWaitLoadingTime))
			{
				PlayClip(_asset.Asset as AudioClip, deferredPlayData.mSound);
			}
		}
	}

	private void PlayClip(AudioClip _clip, SoundSystem.Sound _snd)
	{
		int num = -1;
		for (int num2 = mAudio.Length - 1; num2 >= 0; num2--)
		{
			AudioSource audioSource = mAudio[num2];
			if (audioSource.isPlaying)
			{
				if (audioSource.clip == _clip)
				{
					if (_snd.mOptions.mReplacePlaying)
					{
						audioSource.Stop();
						num = num2;
						break;
					}
					return;
				}
			}
			else
			{
				num = num2;
			}
		}
		if (num != -1)
		{
			SoundSystem.InitAndPlay(mAudio[num], _clip, _snd);
		}
	}

	public void Update()
	{
		int count = mDeferredSounds.Count;
		if (count <= 0)
		{
			return;
		}
		bool flag = true;
		for (int num = count - 1; num >= 0; num--)
		{
			DeferredPlayData deferredPlayData = mDeferredSounds[num];
			if (deferredPlayData != null)
			{
				flag = false;
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (realtimeSinceStartup > deferredPlayData.mStartTime)
				{
					mDeferredSounds[num] = null;
					if (realtimeSinceStartup < deferredPlayData.mStartTime + deferredPlayData.mSound.mOptions.mWaitLoadingTime)
					{
						LoadClipAndPlay(deferredPlayData);
					}
				}
			}
		}
		if (flag)
		{
			mDeferredSounds.Clear();
		}
	}

	public void PlaySound(string _soundName)
	{
		if (base.enabled && !(mSoundSys == null))
		{
			SoundSystem.Sound element = Utils.GetElement(mSounds, _soundName);
			if (element != null)
			{
				Play(element);
			}
			else
			{
				Log.Warning("sound " + _soundName + " does not exists in " + base.gameObject.name);
			}
		}
	}

	public void PlaySoundAction(string _actionName)
	{
		if (!base.enabled || mSoundSys == null)
		{
			return;
		}
		Utils.VariantsList element = Utils.GetElement(mActions, _actionName);
		if (element != null)
		{
			int num = Utils.Select(element.mVariants);
			if (num != -1)
			{
				PlaySound(element.mVariants[num].mName);
			}
			else
			{
				Log.Warning("sound action " + _actionName + " have wrong sound options");
			}
		}
		else
		{
			PlaySound(_actionName);
		}
	}
}
