using System;
using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

internal class OptionsMgr
{
	public enum OptionsType
	{
		ALL_OPTIONS,
		SCREEN_OPTIONS,
		QUALITY_OPTIONS,
		VOLUME_OPTIONS
	}

	public delegate void OnTutorialSetChanged(int _setNum);

	public static int mScreenWidth = 0;

	public static int mScreenHeight = 0;

	public static bool mFullScreen = true;

	public static QualityLevel mQuality = QualityLevel.Good;

	public static float mBaseVolume = 1f;

	public static float mMusicVolume = 1f;

	public static float mSoundVolume = 1f;

	public static float mGuiVolume = 1f;

	public static bool mShowAvatarHealthBar = true;

	public static bool mShowOtherHealthBar = true;

	public static bool mShowDamageEffect = true;

	public static bool mShowAttackers = true;

	private static bool mScreenChanged = false;

	public static Queue<int> mActiveQuests = new Queue<int>();

	public static SortedDictionary<int, int> mActiveItems = new SortedDictionary<int, int>();

	public static bool mShopFilterLvl;

	private static float mCamPosVal;

	private static float mMinCamPosVal;

	private static float mMaxCamPosVal;

	private static GameCamera.CamMode mCamMode;

	public static float mCamSpeed;

	public static float mMinCamSpeed;

	public static float mMaxCamSpeed;

	public static float mDefaultCamSpeed;

	private static Resolution mScreenResolution;

	private static int mTutorialSetNum = 0;

	public static bool mPlayAvatarVoices = true;

	public static OnTutorialSetChanged mOnTutorialSetChanged = null;

	public static bool mEnabled = true;

	public static float CamPosVal
	{
		get
		{
			return mCamPosVal;
		}
		set
		{
			mCamPosVal = value;
			mCamPosVal = ((!(mCamPosVal > mMaxCamPosVal)) ? mCamPosVal : mMaxCamPosVal);
			mCamPosVal = ((!(mCamPosVal < mMinCamPosVal)) ? mCamPosVal : mMinCamPosVal);
		}
	}

	public static GameCamera.CamMode CamMode
	{
		get
		{
			return mCamMode;
		}
		set
		{
			if (Enum.IsDefined(typeof(GameCamera.CamMode), value))
			{
				mCamMode = value;
			}
			else
			{
				mCamMode = GameCamera.CamMode.GAME_NORMAL_CAM_MODE;
			}
		}
	}

	public static int TutorialSetNum
	{
		get
		{
			return mTutorialSetNum;
		}
		set
		{
			int num = mTutorialSetNum;
			mTutorialSetNum = value;
			if (mOnTutorialSetChanged != null && num != mTutorialSetNum)
			{
				mOnTutorialSetChanged(mTutorialSetNum);
			}
		}
	}

	private static bool IsEnabled()
	{
		return mEnabled;
	}

	public static void LoadOptions()
	{
		if (IsEnabled())
		{
			mMinCamPosVal = 25f;
			mMaxCamPosVal = 40f;
			mMinCamSpeed = 0.5f;
			mMaxCamSpeed = 3f;
			mDefaultCamSpeed = 0.85f;
			mCamSpeed = mDefaultCamSpeed;
			mTutorialSetNum = PlayerPrefs.GetInt("tutorial", 0);
			mTutorialSetNum = ((mTutorialSetNum <= 0) ? mTutorialSetNum : (-1));
			mBaseVolume = PlayerPrefs.GetFloat("base_volume", 1f);
			mMusicVolume = PlayerPrefs.GetFloat("music_volume", 1f);
			mSoundVolume = PlayerPrefs.GetFloat("sound_volume", 1f);
			mGuiVolume = PlayerPrefs.GetFloat("gui_volume", 1f);
			mPlayAvatarVoices = PlayerPrefs.GetInt("avatar_voices", 1) == 1;
			mScreenWidth = PlayerPrefs.GetInt("screen_width", 0);
			mScreenHeight = PlayerPrefs.GetInt("screen_height", 0);
			mFullScreen = PlayerPrefs.GetInt("full_screen", 1) == 1;
			int @int = PlayerPrefs.GetInt("quality", 3);
			if (Enum.IsDefined(typeof(QualityLevel), @int))
			{
				mQuality = (QualityLevel)@int;
			}
			else
			{
				mQuality = QualityLevel.Good;
			}
			mShowAvatarHealthBar = PlayerPrefs.GetInt("avatar_health_bar", 1) == 1;
			mShowOtherHealthBar = PlayerPrefs.GetInt("other_health_bar", 1) == 1;
			mShowDamageEffect = PlayerPrefs.GetInt("damage_effect", 1) == 1;
			mShopFilterLvl = PlayerPrefs.GetInt("shop_filter_lvl", 0) == 1;
			mShowAttackers = PlayerPrefs.GetInt("attacker_effect", 1) == 1;
			CamPosVal = PlayerPrefs.GetFloat("camera_value", mMaxCamPosVal);
			mCamSpeed = PlayerPrefs.GetFloat("camera_move_speed", mDefaultCamSpeed);
			mCamSpeed = ((!(mCamSpeed < mMinCamSpeed)) ? mCamSpeed : mMinCamSpeed);
			mCamSpeed = ((!(mCamSpeed > mMaxCamSpeed)) ? mCamSpeed : mMaxCamSpeed);
			CamMode = (GameCamera.CamMode)PlayerPrefs.GetInt("camera_mode", 1);
			if (Screen.fullScreen)
			{
				Log.Error("fullScreen can be at start");
			}
			mScreenResolution = Screen.currentResolution;
			Log.Debug("Max screen resolution : " + mScreenResolution.width + " x " + mScreenResolution.height);
			LoadActiveQuests();
			LoadActiveItems();
			Log.Debug("LoadOptions : " + mScreenWidth + "x" + mScreenHeight + " fs : " + mFullScreen + " q : " + mQuality);
			SetCurOptions(OptionsType.ALL_OPTIONS);
		}
	}

	public static void SaveOptions()
	{
		if (!IsEnabled() || Application.isEditor)
		{
			return;
		}
		mTutorialSetNum = ((mTutorialSetNum <= 0) ? mTutorialSetNum : (-1));
		PlayerPrefs.SetInt("tutorial", mTutorialSetNum);
		PlayerPrefs.SetFloat("base_volume", mBaseVolume);
		PlayerPrefs.SetFloat("music_volume", mMusicVolume);
		PlayerPrefs.SetFloat("sound_volume", mSoundVolume);
		PlayerPrefs.SetFloat("gui_volume", mGuiVolume);
		PlayerPrefs.SetInt("avatar_voices", mPlayAvatarVoices ? 1 : 0);
		PlayerPrefs.SetInt("screen_width", mScreenWidth);
		PlayerPrefs.SetInt("screen_height", mScreenHeight);
		PlayerPrefs.SetInt("full_screen", mFullScreen ? 1 : 0);
		PlayerPrefs.SetInt("quality", (int)mQuality);
		PlayerPrefs.SetInt("avatar_health_bar", mShowAvatarHealthBar ? 1 : 0);
		PlayerPrefs.SetInt("other_health_bar", mShowOtherHealthBar ? 1 : 0);
		PlayerPrefs.SetInt("damage_effect", mShowDamageEffect ? 1 : 0);
		PlayerPrefs.SetInt("shop_filter_lvl", mShopFilterLvl ? 1 : 0);
		PlayerPrefs.SetInt("attacker_effect", mShowAttackers ? 1 : 0);
		PlayerPrefs.SetFloat("camera_value", mCamPosVal);
		mCamSpeed = ((!(mCamSpeed < mMinCamSpeed)) ? mCamSpeed : mMinCamSpeed);
		mCamSpeed = ((!(mCamSpeed > mMaxCamSpeed)) ? mCamSpeed : mMaxCamSpeed);
		PlayerPrefs.SetFloat("camera_move_speed", mCamSpeed);
		PlayerPrefs.SetInt("camera_mode", (int)mCamMode);
		int num = 0;
		string text = string.Empty;
		while (mActiveQuests.Count > 3)
		{
			mActiveQuests.Dequeue();
		}
		foreach (int mActiveQuest in mActiveQuests)
		{
			num++;
			text += mActiveQuest;
			if (num < mActiveQuests.Count)
			{
				text += ",";
			}
		}
		PlayerPrefs.SetString("active_quests", text);
		num = 0;
		text = string.Empty;
		foreach (int value in mActiveItems.Values)
		{
			num++;
			text += value;
			if (num < mActiveItems.Count)
			{
				text += ",";
			}
		}
		PlayerPrefs.SetString("active_items", text);
	}

	public static void UpdateScreen()
	{
		if (IsEnabled() && !Application.isEditor)
		{
			if (mScreenChanged)
			{
				CheckScreenChanges();
			}
			if (mFullScreen != Screen.fullScreen && !mScreenChanged)
			{
				mFullScreen = Screen.fullScreen;
				SetScreenOptions();
			}
		}
	}

	public static void SetCurOptions(OptionsType _type)
	{
		if (IsEnabled() && !Application.isEditor)
		{
			switch (_type)
			{
			case OptionsType.SCREEN_OPTIONS:
				SetScreenOptions();
				break;
			case OptionsType.QUALITY_OPTIONS:
				SetQualityOptions();
				break;
			case OptionsType.VOLUME_OPTIONS:
				SetSoundOptions();
				break;
			case OptionsType.ALL_OPTIONS:
				SetQualityOptions();
				SetScreenOptions();
				SetSoundOptions();
				break;
			}
		}
	}

	public static bool IsQuestActive(int _id)
	{
		return mActiveQuests.Contains(_id);
	}

	public static bool IsItemSlotFree(int _num)
	{
		if (mActiveItems.TryGetValue(_num, out var value))
		{
			return value == -1;
		}
		return false;
	}

	public static int GetActiveItemNum(int _id)
	{
		foreach (KeyValuePair<int, int> mActiveItem in mActiveItems)
		{
			if (mActiveItem.Value == _id)
			{
				return mActiveItem.Key;
			}
		}
		return -1;
	}

	public static SortedDictionary<int, int> GetActiveItems()
	{
		return mActiveItems;
	}

	public static void SetActiveQuest(int _id, bool _show)
	{
		if (IsQuestActive(_id))
		{
			if (_show)
			{
				return;
			}
			int[] array = mActiveQuests.ToArray();
			mActiveQuests.Clear();
			int[] array2 = array;
			foreach (int num in array2)
			{
				if (num != _id)
				{
					mActiveQuests.Enqueue(num);
				}
			}
		}
		else if (_show)
		{
			while (mActiveQuests.Count >= 3)
			{
				mActiveQuests.Dequeue();
			}
			mActiveQuests.Enqueue(_id);
		}
	}

	public static void SetActiveItem(int _id, int _num)
	{
		if (_num >= 0 && _num < 5)
		{
			if (!mActiveItems.ContainsKey(_num))
			{
				Log.Error("Can't add active item with id : " + _id + " to slot : " + _num);
			}
			else
			{
				mActiveItems[_num] = _id;
			}
		}
	}

	public static void RemoveActiveItem(int _id)
	{
		foreach (KeyValuePair<int, int> mActiveItem in mActiveItems)
		{
			if (mActiveItem.Value == _id)
			{
				mActiveItems[mActiveItem.Key] = -1;
			}
		}
	}

	private static void LoadActiveQuests()
	{
		mActiveQuests.Clear();
		string @string = PlayerPrefs.GetString("active_quests", string.Empty);
		string[] array = @string.Split(',');
		int result = 0;
		string[] array2 = array;
		foreach (string s in array2)
		{
			if (int.TryParse(s, out result))
			{
				mActiveQuests.Enqueue(result);
				if (mActiveQuests.Count == 3)
				{
					break;
				}
			}
		}
	}

	private static void LoadActiveItems()
	{
		mActiveItems.Clear();
		for (int i = 0; i < 5; i++)
		{
			mActiveItems.Add(i, -1);
		}
		string @string = PlayerPrefs.GetString("active_items", string.Empty);
		string[] array = @string.Split(',');
		int result = 0;
		int num = 0;
		string[] array2 = array;
		foreach (string s in array2)
		{
			if (int.TryParse(s, out result))
			{
				mActiveItems[num] = result;
				num++;
				if (num == 5)
				{
					break;
				}
			}
		}
	}

	private static void CheckScreenChanges()
	{
		mScreenChanged = Screen.width != mScreenWidth || Screen.height != mScreenHeight || Screen.fullScreen != mFullScreen;
	}

	private static void SetScreenOptions()
	{
		CheckScreenOptions();
		Log.Debug("Setting Resolution 1 : " + mScreenWidth + "x" + mScreenHeight + "fs : " + mFullScreen);
		if (Screen.width != mScreenWidth || Screen.height != mScreenHeight || Screen.fullScreen != mFullScreen)
		{
			Log.Debug("Setting Resolution 2 : " + mScreenWidth + "x" + mScreenHeight + "fs : " + mFullScreen);
			Screen.SetResolution(mScreenWidth, mScreenHeight, mFullScreen);
			mScreenChanged = true;
		}
	}

	private static void SetQualityOptions()
	{
		if (!Enum.IsDefined(typeof(QualityLevel), (int)mQuality))
		{
			mQuality = QualityLevel.Good;
		}
		QualitySettings.currentLevel = mQuality;
	}

	private static void SetSoundOptions()
	{
		AudioListener.volume = mBaseVolume;
		SoundSystem instance = SoundSystem.Instance;
		if (instance != null)
		{
			instance.SetMusicVolume(mMusicVolume);
		}
	}

	public static void CheckScreenOptions()
	{
		if (Screen.resolutions.Length == 0)
		{
			Log.Error("Critical error! No any available resolutions! Close Application");
			Application.Quit();
			return;
		}
		bool flag = false;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (resolution.width == mScreenWidth && resolution.height == mScreenHeight)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Log.Warning("Trying to set not available resolution : " + mScreenWidth + "x" + mScreenHeight);
			mScreenWidth = mScreenResolution.width;
			mScreenHeight = mScreenResolution.height;
			Log.Warning("Trying : " + mScreenWidth + "x" + mScreenHeight);
		}
		if (mScreenWidth >= mScreenResolution.width || mScreenHeight >= mScreenResolution.height)
		{
			mFullScreen = true;
		}
	}
}
