using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class VisualEffectViewer : MonoBehaviour
{
	private Camera mCamera;

	private List<Object> mObjects = new List<Object>();

	private List<Object> mEffects = new List<Object>();

	private int mCurObjectNum = -1;

	private string mCurAnim = string.Empty;

	private string mCurEffect = string.Empty;

	private GameObject mCurObject;

	private Vector2 mScrollObjectPosition = Vector2.zero;

	private Vector2 mScrollEffectPosition = Vector2.zero;

	private List<string> mAnimNames = new List<string>();

	private Skill.EffectParams mCurEffectParams;

	private Font mFnt;

	public void Start()
	{
		Config config = new Config();
		config.ApplyThreadSettings();
		config.Load(AssetLoader.ReadText("configs/config"));
		config.InitLog();
		config.CreateLocaleState();
		config.LocaleState.LoadFromXml(AssetLoader.ReadText(config.LocalePath));
		TaskQueue taskQueue = GameObjUtil.FindObjectOfType<TaskQueue>();
		AssetLoader assetLoader = new AssetLoader(taskQueue);
		assetLoader.LoadAssetsDataFromFile(config.DataDir + "resources.xml");
		SoundSystem.Instance.Init();
		GuiSystem guiSystem = GameObjUtil.FindObjectOfType<GuiSystem>();
		GuiSystem.mLocaleState = config.LocaleState;
		guiSystem.Init();
		Object original = Resources.Load("Fonts/Tahoma_11");
		mFnt = Object.Instantiate(original) as Font;
		mCamera = Camera.main;
		mCamera.transform.eulerAngles = new Vector3(48.5f, 44f, 0f);
		mCamera.fieldOfView = 42f;
		Vector3 normalized = new Vector3(-8f, 15.5f, -9f).normalized;
		mCamera.transform.position = Vector3.zero + normalized * 20f;
		InitObjects();
		InitAvailableEffects();
	}

	public void OnGUI()
	{
		if (Event.current.control)
		{
			if (Event.current.type == EventType.MouseDrag && null != mCurObject)
			{
				Vector3 eulerAngles = mCurObject.transform.eulerAngles;
				eulerAngles.y -= Event.current.delta.x;
				mCurObject.transform.eulerAngles = eulerAngles;
				mCamera.transform.RotateAround(mCurObject.transform.position, mCamera.transform.right, Event.current.delta.y);
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f)
			{
				mCamera.transform.Translate(mCamera.transform.forward * axis, Space.World);
			}
		}
		mScrollObjectPosition = GUI.BeginScrollView(new Rect(10f, 80f, 220f, 400f), mScrollObjectPosition, new Rect(0f, 0f, 200f, mObjects.Count * 20));
		int i = 0;
		for (int count = mObjects.Count; i < count; i++)
		{
			if (GUI.Button(new Rect(0f, i * 20, 200f, 20f), mObjects[i].name))
			{
				mCurObjectNum = i;
				InitCurObject();
			}
		}
		GUI.EndScrollView();
		if (GUI.Button(new Rect(10f, 40f, 150f, 20f), "remove object"))
		{
			if (mCurEffectParams != null)
			{
				Skill.StopEffects(mCurEffectParams, null);
				mCurEffectParams = null;
			}
			GameObjUtil.DestroyGameObject(ref mCurObject);
		}
		if (GUI.Button(new Rect(180f, 40f, 150f, 20f), "create target"))
		{
			CreateTarget();
		}
		GUI.BeginScrollView(new Rect(10f, 550f, 220f, mAnimNames.Count * 20), Vector2.zero, new Rect(0f, 0f, 200f, mAnimNames.Count * 20));
		int j = 0;
		for (int count2 = mAnimNames.Count; j < count2; j++)
		{
			if (GUI.Button(new Rect(0f, j * 20, 200f, 20f), mAnimNames[j]))
			{
				mCurAnim = mAnimNames[j];
				PlayCurObjAnim(mCurAnim);
			}
		}
		GUI.EndScrollView();
		BattleData.EffectHolder[] mEffectsOptions = VisualEffectsMgr.Instance.mEffectsOptions;
		mScrollEffectPosition = GUI.BeginScrollView(new Rect(Screen.width - 270, 10f, 270f, 560f), mScrollEffectPosition, new Rect(0f, 0f, 250f, mEffectsOptions.Length * 20));
		for (int k = 0; k < mEffectsOptions.Length; k++)
		{
			string text = GuiSystem.mLocaleState.GetText("IDS_" + mEffectsOptions[k].mName + "_Name");
			GUIStyle button = GUI.skin.button;
			button.font = mFnt;
			button.alignment = TextAnchor.MiddleLeft;
			if (GUI.Button(new Rect(0f, k * 20, 250f, 20f), mEffectsOptions[k].mName + " " + text, button) && (bool)mCurObject)
			{
				BattleData.EffectHolder effectHolder = VisualEffectsMgr.Instance.GetEffectHolder(mEffectsOptions[k].mName);
				mCurEffectParams = new Skill.EffectParams();
				mCurEffectParams.mEffect = effectHolder;
				mCurEffectParams.mOwnerObj = mCurObject;
				mCurEffectParams.mTargetObj = GameObject.Find("/target");
				Skill.StartEffects(_done: false, mCurEffectParams, null);
			}
		}
		GUI.EndScrollView();
		if (GUI.Button(new Rect(Screen.width - 270, 620f, 150f, 20f), "stop effect"))
		{
			Skill.StartEffects(_done: true, mCurEffectParams, null);
			Skill.StopEffects(mCurEffectParams, null);
			mCurEffectParams = null;
		}
	}

	private Object[] GetAllObjs(string _path)
	{
		return new Object[0];
	}

	private void InitObjects()
	{
		InitObjGroup("Avatars");
		InitObjGroup("Mobs");
		InitObjGroup("Bosses");
		InitObjGroup("Creeps\\Elf");
		InitObjGroup("Creeps\\Human");
	}

	private void InitObjGroup(string _groupName)
	{
		Object[] allObjs = GetAllObjs("Assets\\Content\\Characters\\" + _groupName);
		for (int i = 0; i < allObjs.Length; i++)
		{
			mObjects.Add(allObjs[i]);
		}
	}

	private void InitCurObject()
	{
		if (mCurObjectNum != -1)
		{
			if (mCurObject != null)
			{
				GameObjUtil.DestroyGameObject(ref mCurObject);
			}
			mCurObject = Object.Instantiate(mObjects[mCurObjectNum]) as GameObject;
			GameData gameData = mCurObject.AddComponent<GameData>();
			gameData.InitAnimation();
			NetSyncTransform component = mCurObject.GetComponent<NetSyncTransform>();
			if ((bool)component)
			{
				component.enabled = false;
			}
			mCurObject.transform.position = Vector3.zero;
			mCurObject.transform.Rotate(0f, 180f, 0f, Space.World);
			InitAvailableAnims();
			InitAvailableEffects();
		}
	}

	private void CreateTarget()
	{
		if (mCurObjectNum == -1)
		{
			return;
		}
		GameObject _go = GameObject.Find("/target");
		if (_go != null)
		{
			GameObjUtil.DestroyGameObject(ref _go);
		}
		if (!(mCurObject == null))
		{
			_go = Object.Instantiate(mObjects[mCurObjectNum]) as GameObject;
			_go.name = "target";
			NetSyncTransform component = _go.GetComponent<NetSyncTransform>();
			if ((bool)component)
			{
				component.enabled = false;
			}
			_go.transform.position = new Vector3(0f, 0f, 10f);
			_go.transform.Rotate(0f, 180f, 0f, Space.World);
		}
	}

	private void InitAvailableAnims()
	{
		mAnimNames.Clear();
		if (mCurObject == null)
		{
			return;
		}
		AnimationExt componentInChildren = mCurObject.GetComponentInChildren<AnimationExt>();
		if (!(componentInChildren == null))
		{
			componentInChildren.mAutoActions = false;
			AnimationExt.Anim[] mAnimations = componentInChildren.mAnimations;
			foreach (AnimationExt.Anim anim in mAnimations)
			{
				mAnimNames.Add(anim.mName);
			}
		}
	}

	private void PlayCurObjAnim(string _anim)
	{
		if (!(null == mCurObject))
		{
			AnimationExt componentInChildren = mCurObject.GetComponentInChildren<AnimationExt>();
			componentInChildren.PlayAnimation(_anim);
		}
	}

	private void InitAvailableEffects()
	{
		string path = "Prefabs/FXs";
		Object[] array = Resources.LoadAll(path, typeof(GameObject));
		for (int i = 0; i < array.Length; i++)
		{
			mEffects.Add(array[i]);
		}
	}

	private void PlayEffect(string _effectName)
	{
		if (null != mCurObject)
		{
			GameObject[] targets = new GameObject[1]
			{
				mCurObject
			};
			VisualEffectsMgr.Instance.PlayEffect(_effectName, targets);
		}
	}

	private void PlayAll()
	{
		PlayCurObjAnim(mCurAnim);
		PlayEffect(mCurEffect);
	}
}
