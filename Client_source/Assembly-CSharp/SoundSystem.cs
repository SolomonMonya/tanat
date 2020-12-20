using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
	public enum SoundType
	{
		sound,
		music,
		gui
	}

	[Serializable]
	public class SoundOptions
	{
		public float mVolume = 1f;

		public float mWaitLoadingTime = 2f;

		public bool mReplacePlaying;

		private bool mUsePann;

		private float mPannValue;

		public bool UsePann
		{
			get
			{
				return mUsePann;
			}
			set
			{
				mUsePann = value;
			}
		}

		public float PannValue
		{
			get
			{
				return mPannValue;
			}
			set
			{
				mPannValue = value;
			}
		}
	}

	[Serializable]
	public class Sound : Utils.Named, Utils.PlaceDescriptor
	{
		public string mPath;

		public SoundOptions mOptions;

		private string mPathCache;

		public string GetName()
		{
			return mName;
		}

		public string GetPath()
		{
			if (mPathCache == null)
			{
				mPathCache = "sound/" + mPath + "/";
			}
			return mPathCache;
		}

		public virtual float GetFinalVolume()
		{
			return GetFinalSoundVolume(mOptions.mVolume, SoundType.sound);
		}
	}

	[Serializable]
	public class GuiSound : Sound
	{
		public override float GetFinalVolume()
		{
			return GetFinalSoundVolume(mOptions.mVolume, SoundType.gui);
		}
	}

	[Serializable]
	public class Music : Utils.Named, Utils.PlaceDescriptor
	{
		public float mVolume = 1f;

		public bool mLoop = true;

		public string GetName()
		{
			return mName;
		}

		public string GetPath()
		{
			return "music/";
		}

		public float GetFinalVolume()
		{
			return GetFinalSoundVolume(mVolume, SoundType.music);
		}
	}

	public class PlayMusicTask : TaskQueue.Task
	{
		private Music[] mMusicToPlay;

		public PlayMusicTask(Music[] _music, Notifier<TaskQueue.ITask, object> _notifier)
			: base(_notifier)
		{
			mMusicToPlay = new Music[_music.Length];
			Array.Copy(_music, mMusicToPlay, _music.Length);
		}

		public override void Begin()
		{
			if (!mBegined)
			{
				base.Begin();
				Music[] array = mMusicToPlay;
				foreach (Music music in array)
				{
					Instance.PlayMusic(music);
				}
			}
		}
	}

	public Sound[] mSoundEvents;

	private AssetLoader mLoader;

	private static SoundSystem mInstance;

	public static float mMinDistance = 1f;

	public static float mMaxDistance = 60f;

	private Dictionary<string, float> mMusicVolumes = new Dictionary<string, float>();

	public static SoundSystem Instance => mInstance;

	public void Init()
	{
		mLoader = AssetLoader.Instance;
	}

	public void GetClip(string _clipName, string _clipPath, Notifier<ILoadedAsset, object> _notifier)
	{
		string assetName = "Assets/" + _clipPath + _clipName + ".ogg";
		if (mLoader != null)
		{
			mLoader.LoadAsset(assetName, typeof(AudioClip), _notifier);
		}
	}

	public static GameObject GetAudioListenerObj()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(AudioListener));
		return (array.Length <= 0) ? null : (array[0] as AudioListener).gameObject;
	}

	public static int GetPlayingSounds(GameObject _go, ICollection<AudioSource> _sounds)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		if (_sounds == null)
		{
			throw new ArgumentNullException("_sounds");
		}
		AudioSource[] components = _go.GetComponents<AudioSource>();
		AudioSource[] array = components;
		foreach (AudioSource item in array)
		{
			_sounds.Add(item);
		}
		return components.Length;
	}

	public void OnEnable()
	{
		mInstance = this;
	}

	public void OnDisable()
	{
		mInstance = null;
	}

	public static void ApplyOptions(AudioSource _audio, float _volume, SoundOptions _options)
	{
		_audio.volume = _volume;
		_audio.minDistance = mMinDistance;
		_audio.maxDistance = mMaxDistance;
		_audio.rolloffMode = AudioRolloffMode.Linear;
		_audio.pitch = 1f;
		_audio.loop = false;
		_audio.playOnAwake = false;
		if (_options.UsePann)
		{
			_audio.panLevel = _options.PannValue;
		}
	}

	public static float GetFinalSoundVolume(float _volume, SoundType _sndType)
	{
		_volume *= OptionsMgr.mBaseVolume;
		switch (_sndType)
		{
		case SoundType.sound:
			_volume *= OptionsMgr.mSoundVolume;
			break;
		case SoundType.music:
			_volume *= OptionsMgr.mMusicVolume;
			break;
		case SoundType.gui:
			_volume *= OptionsMgr.mGuiVolume;
			break;
		}
		_volume = Mathf.Clamp01(_volume);
		return _volume;
	}

	public void PlayMusic(Music _music)
	{
		if (_music == null)
		{
			throw new ArgumentNullException("_music");
		}
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>();
		notifier.mData = _music;
		notifier.mCallback = OnMusicClipReady;
		GetClip(_music.GetName(), _music.GetPath(), notifier);
	}

	private void OnMusicClipReady(bool _success, ILoadedAsset _asset, object _data)
	{
		if (!_success)
		{
			return;
		}
		AudioClip audioClip = _asset.Asset as AudioClip;
		GameObject audioListenerObj = GetAudioListenerObj();
		if (audioListenerObj == null)
		{
			return;
		}
		AudioSource[] components = audioListenerObj.GetComponents<AudioSource>();
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource.clip == audioClip)
			{
				if (!audioSource.isPlaying)
				{
					audioSource.Play();
				}
				return;
			}
		}
		Music music = _data as Music;
		AudioSource audioSource2 = audioListenerObj.AddComponent<AudioSource>();
		audioClip.name = music.mName;
		audioSource2.clip = audioClip;
		audioSource2.volume = music.GetFinalVolume();
		audioSource2.loop = music.mLoop;
		audioSource2.playOnAwake = false;
		audioSource2.panLevel = 0f;
		audioSource2.Play();
		mMusicVolumes[audioSource2.clip.name] = music.mVolume;
	}

	private IEnumerable<AudioSource> GetPlayingMusic()
	{
		List<AudioSource> list = new List<AudioSource>();
		AudioListener[] array = UnityEngine.Object.FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
		AudioListener[] array2 = array;
		foreach (AudioListener audioListener in array2)
		{
			GetPlayingSounds(audioListener.gameObject, list);
		}
		return list;
	}

	public void StopAllMusic()
	{
		IEnumerable<AudioSource> playingMusic = GetPlayingMusic();
		foreach (AudioSource item in playingMusic)
		{
			item.Stop();
		}
	}

	public void SetMusicVolume(float _volume)
	{
		if (mMusicVolumes == null)
		{
			return;
		}
		IEnumerable<AudioSource> playingMusic = GetPlayingMusic();
		foreach (AudioSource item in playingMusic)
		{
			if (!(item == null) && !(item.clip == null) && mMusicVolumes.TryGetValue(item.clip.name, out var value))
			{
				value = (item.volume = GetFinalSoundVolume(value, SoundType.music));
			}
		}
	}

	public void PlaySound(Sound _snd, GameObject _go)
	{
		if (_snd == null)
		{
			throw new ArgumentNullException("_snd");
		}
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		SoundEmiter component = _go.GetComponent<SoundEmiter>();
		if (component == null)
		{
			Log.Warning(_go.name + " has no sound emiter");
		}
		else
		{
			component.Play(_snd);
		}
	}

	public void PlayGuiSound(GuiSound _snd)
	{
		if (_snd == null)
		{
			throw new ArgumentNullException("_snd");
		}
		GameObject audioListenerObj = GetAudioListenerObj();
		if (!(audioListenerObj == null))
		{
			SoundEmiter component = audioListenerObj.GetComponent<SoundEmiter>();
			if (component == null)
			{
				Log.Warning(audioListenerObj.name + " has no sound emiter");
			}
			else
			{
				component.Play(_snd);
			}
		}
	}

	public void PlaySoundEffect(Sound _snd, float _dt, GameObject _go)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		SoundEmiter component = _go.GetComponent<SoundEmiter>();
		if (component == null)
		{
			Log.Warning(_go.name + " has no sound emiter");
		}
		else if (_dt > 0f)
		{
			component.DeferredPlay(_snd, _dt);
		}
		else
		{
			component.Play(_snd);
		}
	}

	public void PlaySoundEvent(string _soundEventName, GameObject _go)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		Sound element = Utils.GetElement(mSoundEvents, _soundEventName);
		if (element == null)
		{
			Log.Warning("cannot find sound event " + _soundEventName);
		}
		else
		{
			PlaySound(element, _go);
		}
	}

	public void PlaySoundEvent(string _soundEventName)
	{
		GameObject cam = Camera.main.gameObject;
		if (cam == null)
		{
			return;
		}
		Sound snd = Utils.GetElement(mSoundEvents, _soundEventName);
		if (snd == null)
		{
			Log.Warning("cannot find sound event " + _soundEventName);
			return;
		}
		Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>();
		notifier.mCallback = delegate(bool _success, ILoadedAsset _asset, object _data)
		{
			if (_success)
			{
				AudioClip clip = _asset.Asset as AudioClip;
				AudioSource freeAudioSrouce = GetFreeAudioSrouce(cam);
				InitAndPlay(freeAudioSrouce, clip, snd);
			}
		};
		GetClip(snd.GetName(), snd.GetPath(), notifier);
	}

	public static void InitAndPlay(AudioSource _audio, AudioClip _clip, Sound _snd)
	{
		if (_audio == null)
		{
			throw new ArgumentNullException("_audio");
		}
		if (_clip == null)
		{
			throw new ArgumentNullException("_clip");
		}
		if (_snd == null)
		{
			throw new ArgumentNullException("_snd");
		}
		_audio.clip = _clip;
		ApplyOptions(_audio, _snd.GetFinalVolume(), _snd.mOptions);
		_audio.Play();
	}

	public static AudioSource GetFreeAudioSrouce(GameObject _go)
	{
		if (_go == null)
		{
			throw new ArgumentNullException("_go");
		}
		AudioSource[] components = _go.GetComponents<AudioSource>();
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource.clip == null || !audioSource.isPlaying)
			{
				return audioSource;
			}
		}
		return _go.AddComponent<AudioSource>();
	}
}
