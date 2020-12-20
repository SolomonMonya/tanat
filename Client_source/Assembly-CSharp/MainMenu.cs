using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MainMenu/MainMenu")]
public class MainMenu : MonoBehaviour
{
	[Serializable]
	public class DisableSceneData
	{
		public GameObject mData;

		public MainMenuState mState;
	}

	[Serializable]
	public enum MainMenuState
	{
		BATTLE,
		SELECT_RACE,
		CUSTOMIZE_HUMAN,
		CUSTOMIZE_ELF
	}

	private enum MenuBattleState
	{
		NONE,
		BATTLE,
		IDLE
	}

	public GameObject[] mBattleObjects;

	public int mMinIdleCnt = 3;

	public int mMaxIdleCnt = 5;

	public float mBlendTime = 0.3f;

	public GameObject mBattleCamera;

	public GameObject mHeroCamera;

	public GameObject mElfCamera;

	public GameObject mHumanCamera;

	public DisableSceneData[] mDisableData;

	private MainMenuState mMainMenuState;

	private MenuBattleState mMenuBattleState;

	private int mMaxIdleCount;

	private int mCurIdleCount;

	private float mIdleTime;

	private float mBattleTime;

	private float mCurTime;

	private List<BattleObjectController> mBattleObjectControllers;

	public Action<HeroRace> mRaceSelectedCallback;

	public void Start()
	{
		mBattleObjectControllers = new List<BattleObjectController>();
		BattleObjectController battleObjectController = null;
		GameObject[] array = mBattleObjects;
		foreach (GameObject gameObject in array)
		{
			battleObjectController = gameObject.AddComponent<BattleObjectController>();
			if (!(battleObjectController == null))
			{
				battleObjectController.SetBlendTime(mBlendTime);
				if (gameObject.GetComponentInChildren<Camera>() != null)
				{
					mIdleTime = battleObjectController.GetAnimTime("Idle01");
					mBattleTime = battleObjectController.GetAnimTime("Battle01");
				}
				mBattleObjectControllers.Add(battleObjectController);
			}
		}
		mMaxIdleCount = UnityEngine.Random.Range(mMinIdleCnt, mMaxIdleCount);
		SetMenuState(MainMenuState.BATTLE);
	}

	public void OnEnable()
	{
		DisableSceneData[] array = mDisableData;
		foreach (DisableSceneData disableSceneData in array)
		{
			RaceSelector[] componentsInChildren = disableSceneData.mData.GetComponentsInChildren<RaceSelector>();
			if (componentsInChildren != null)
			{
				RaceSelector[] array2 = componentsInChildren;
				foreach (RaceSelector raceSelector in array2)
				{
					raceSelector.mSelectedCallback = (Action<HeroRace>)Delegate.Combine(raceSelector.mSelectedCallback, new Action<HeroRace>(OnRaceSelected));
				}
			}
		}
	}

	public void OnDisable()
	{
		DisableSceneData[] array = mDisableData;
		foreach (DisableSceneData disableSceneData in array)
		{
			if (disableSceneData.mData == null)
			{
				continue;
			}
			RaceSelector[] componentsInChildren = disableSceneData.mData.GetComponentsInChildren<RaceSelector>();
			if (componentsInChildren != null)
			{
				RaceSelector[] array2 = componentsInChildren;
				foreach (RaceSelector raceSelector in array2)
				{
					raceSelector.mSelectedCallback = (Action<HeroRace>)Delegate.Remove(raceSelector.mSelectedCallback, new Action<HeroRace>(OnRaceSelected));
				}
			}
		}
	}

	private void OnRaceSelected(HeroRace _race)
	{
		if (mRaceSelectedCallback != null)
		{
			mRaceSelectedCallback(_race);
		}
	}

	public void Update()
	{
		if (mMainMenuState != 0)
		{
			return;
		}
		if (mCurTime <= 0f)
		{
			mMenuBattleState = GenerateBattleState();
			mCurTime = ((mMenuBattleState != MenuBattleState.BATTLE) ? mIdleTime : mBattleTime);
			PlayBattleObjectsAnim();
			if (mMenuBattleState == MenuBattleState.BATTLE)
			{
				SoundSystem.Music music = new SoundSystem.Music();
				music.mName = "menu_sound";
				music.mLoop = false;
				SoundSystem.Instance.PlayMusic(music);
			}
		}
		mCurTime -= Time.deltaTime;
	}

	public void SetMenuState(MainMenuState _state)
	{
		mMainMenuState = _state;
		SetCurCamera(mMainMenuState);
		SetSceneData(mMainMenuState);
	}

	private void SetCurCamera(MainMenuState _state)
	{
		mBattleCamera.active = _state == MainMenuState.BATTLE;
		mHeroCamera.active = _state == MainMenuState.SELECT_RACE;
		mElfCamera.active = _state == MainMenuState.CUSTOMIZE_ELF;
		mHumanCamera.active = _state == MainMenuState.CUSTOMIZE_HUMAN;
	}

	private void SetSceneData(MainMenuState _state)
	{
		DisableSceneData[] array = mDisableData;
		foreach (DisableSceneData disableSceneData in array)
		{
			disableSceneData.mData.SetActiveRecursively(disableSceneData.mState == _state);
		}
	}

	private MenuBattleState GenerateBattleState()
	{
		if (mMenuBattleState == MenuBattleState.BATTLE)
		{
			mMaxIdleCount = UnityEngine.Random.Range(mMinIdleCnt, mMaxIdleCount);
			mCurIdleCount = 0;
		}
		if (mCurIdleCount < mMaxIdleCount)
		{
			mCurIdleCount++;
			return MenuBattleState.IDLE;
		}
		return MenuBattleState.BATTLE;
	}

	private void PlayBattleObjectsAnim()
	{
		foreach (BattleObjectController mBattleObjectController in mBattleObjectControllers)
		{
			mBattleObjectController.PlayAnim((mMenuBattleState != MenuBattleState.BATTLE) ? "Idle01" : "Battle01");
		}
	}
}
