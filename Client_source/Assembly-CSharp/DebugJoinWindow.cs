using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class DebugJoinWindow : GuiElement
{
	public delegate void JoinCallback(BattleServerData _battleSrvData, BattleMapData _mapData);

	public delegate void BackCallback();

	public JoinCallback mJoinCallback;

	public BackCallback mBackCallback;

	private GuiButton mJoinBtn;

	private GuiButton mBackBtn;

	private StaticTextField mHostTF;

	private StaticTextField mPortTF;

	private StaticTextField mBattleTF;

	private StaticTextField mMapTF;

	private StaticTextField mTeamTF;

	private StaticTextField mAvatarTF;

	private StaticTextField mSceneTF;

	private MixedArray mAvaParams = new MixedArray();

	private int mSelectedAvaParam;

	private StaticTextField mParamTF;

	private StaticTextField mAddParamValTF;

	private StaticTextField mMulParamValTF;

	private GuiButton mAddParamBtn;

	private GuiButton mDelParamBtn;

	public DebugJoinWindow()
	{
		mPos.x = GuiSystem.mBaseScreenWidth / 2f;
		mPos.y = GuiSystem.mBaseScreenHeight / 2f;
		mZoneRect = new Rect(0f, -225f, 0f, 0f);
		GuiSystem.RecalculateRect(mPos, ref mZoneRect);
	}

	public override void Init()
	{
		mHostTF = new StaticTextField();
		mHostTF.mElementId = "TEXT_FIELD_IP";
		mHostTF.mPos = mPos;
		mHostTF.mZoneRect = new Rect(0f, 0f, 200f, 30f);
		mHostTF.mLength = 50;
		mHostTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mHostTF.mZoneRect);
		mPortTF = new StaticTextField();
		mPortTF.mElementId = "TEXT_FIELD_PORT";
		mPortTF.mPos = mPos;
		mPortTF.mZoneRect = new Rect(0f, 50f, 200f, 30f);
		mPortTF.mLength = 50;
		mPortTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mPortTF.mZoneRect);
		mBattleTF = new StaticTextField();
		mBattleTF.mElementId = "TEXT_FIELD_BATTLE";
		mBattleTF.mPos = mPos;
		mBattleTF.mZoneRect = new Rect(0f, 100f, 200f, 30f);
		mBattleTF.mLength = 10;
		mBattleTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mBattleTF.mZoneRect);
		mMapTF = new StaticTextField();
		mMapTF.mElementId = "TEXT_FIELD_BATTLE";
		mMapTF.mPos = mPos;
		mMapTF.mZoneRect = new Rect(0f, 150f, 200f, 30f);
		mMapTF.mLength = 10;
		mMapTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mMapTF.mZoneRect);
		mTeamTF = new StaticTextField();
		mTeamTF.mElementId = "TEXT_FIELD_TEAM";
		mTeamTF.mPos = mPos;
		mTeamTF.mZoneRect = new Rect(0f, 200f, 200f, 30f);
		mTeamTF.mLength = 5;
		mTeamTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mTeamTF.mZoneRect);
		mAvatarTF = new StaticTextField();
		mAvatarTF.mElementId = "TEXT_FIELD_AVATAR";
		mAvatarTF.mPos = mPos;
		mAvatarTF.mZoneRect = new Rect(0f, 250f, 200f, 30f);
		mAvatarTF.mLength = 50;
		mAvatarTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mAvatarTF.mZoneRect);
		mSceneTF = new StaticTextField();
		mSceneTF.mElementId = "TEXT_FIELD_SCENE";
		mSceneTF.mPos = mPos;
		mSceneTF.mZoneRect = new Rect(0f, 300f, 200f, 30f);
		mSceneTF.mLength = 50;
		mSceneTF.mStyleId = "text_field_1";
		GuiSystem.SetChildRect(mZoneRect, ref mSceneTF.mZoneRect);
		mJoinBtn = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
		mJoinBtn.mElementId = "BUTTON_JOIN";
		mJoinBtn.mLabel = "join";
		GuiButton guiButton = mJoinBtn;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnJoinBtnUp));
		mJoinBtn.mZoneRect = new Rect(0f, 520f, 200f, 30f);
		GuiSystem.SetChildRect(mZoneRect, ref mJoinBtn.mZoneRect);
		mJoinBtn.Init();
		mBackBtn = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
		mBackBtn.mElementId = "BUTTON_BACK";
		mBackBtn.mLabel = "back";
		GuiButton guiButton2 = mBackBtn;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnBackBtnUp));
		mBackBtn.mZoneRect = new Rect(0f, 570f, 200f, 30f);
		GuiSystem.SetChildRect(mZoneRect, ref mBackBtn.mZoneRect);
		mBackBtn.Init();
		mHostTF.mData = "localhost";
		mPortTF.mData = "3005";
		mBattleTF.mData = "0";
		mMapTF.mData = "-1";
		mTeamTF.mData = "1";
		mAvatarTF.mData = "Avtr_HK_ShinDalar";
		mSceneTF.mData = "map_5_0";
		if (PlayerPrefs.HasKey("server_ip"))
		{
			mHostTF.mData = PlayerPrefs.GetString("server_ip");
		}
		if (PlayerPrefs.HasKey("server_port"))
		{
			mPortTF.mData = PlayerPrefs.GetString("server_port");
		}
		if (PlayerPrefs.HasKey("server_battle"))
		{
			mBattleTF.mData = PlayerPrefs.GetString("server_battle");
		}
		if (PlayerPrefs.HasKey("server_map"))
		{
			mMapTF.mData = PlayerPrefs.GetString("server_map");
		}
		if (PlayerPrefs.HasKey("server_team"))
		{
			mTeamTF.mData = PlayerPrefs.GetString("server_team");
		}
		if (PlayerPrefs.HasKey("server_avatar"))
		{
			mAvatarTF.mData = PlayerPrefs.GetString("server_avatar");
		}
		if (PlayerPrefs.HasKey("ctrl_scene"))
		{
			mSceneTF.mData = PlayerPrefs.GetString("ctrl_scene");
		}
		mParamTF = new StaticTextField();
		mParamTF.mElementId = "TEXT_AVA_PARAM";
		mParamTF.mPos = mPos;
		mParamTF.mZoneRect = new Rect(-600f, 100f, 80f, 20f);
		mParamTF.mLength = 30;
		GuiSystem.SetChildRect(mZoneRect, ref mParamTF.mZoneRect);
		mAddParamValTF = new StaticTextField();
		mAddParamValTF.mElementId = "TEXT_AVA_ADD_PARAM_VAL";
		mAddParamValTF.mPos = mPos;
		mAddParamValTF.mZoneRect = new Rect(-500f, 100f, 30f, 20f);
		mAddParamValTF.mLength = 10;
		GuiSystem.SetChildRect(mZoneRect, ref mAddParamValTF.mZoneRect);
		mMulParamValTF = new StaticTextField();
		mMulParamValTF.mElementId = "TEXT_AVA_MUL_PARAM_VAL";
		mMulParamValTF.mPos = mPos;
		mMulParamValTF.mZoneRect = new Rect(-500f, 150f, 30f, 20f);
		mMulParamValTF.mLength = 10;
		GuiSystem.SetChildRect(mZoneRect, ref mMulParamValTF.mZoneRect);
		mAddParamBtn = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
		mAddParamBtn.mElementId = "BUTTON_ADD_AVA_PARAM";
		mAddParamBtn.mLabel = "add";
		GuiButton guiButton3 = mAddParamBtn;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(AddParam));
		mAddParamBtn.mZoneRect = new Rect(-450f, 100f, 50f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mAddParamBtn.mZoneRect);
		mAddParamBtn.Init();
		mDelParamBtn = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
		mDelParamBtn.mElementId = "BUTTON_DEL_AVA_PARAM";
		mDelParamBtn.mLabel = "del";
		GuiButton guiButton4 = mDelParamBtn;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(DelParam));
		mDelParamBtn.mZoneRect = new Rect(-450f, 150f, 50f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mDelParamBtn.mZoneRect);
		mDelParamBtn.Init();
	}

	public override void Uninit()
	{
		if (mHostTF != null)
		{
			mHostTF.Uninit();
		}
		if (mPortTF != null)
		{
			mPortTF.Uninit();
		}
		if (mBattleTF != null)
		{
			mBattleTF.Uninit();
		}
		if (mMapTF != null)
		{
			mMapTF.Uninit();
		}
		if (mTeamTF != null)
		{
			mTeamTF.Uninit();
		}
		if (mAvatarTF != null)
		{
			mAvatarTF.Uninit();
		}
		if (mSceneTF != null)
		{
			mSceneTF.Uninit();
		}
	}

	public override void RenderElement()
	{
		GUI.contentColor = Color.black;
		GuiSystem.DrawString("Battle server", new Rect(mHostTF.mZoneRect.x - 125f, mHostTF.mZoneRect.y, 100f, mHostTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Battle port", new Rect(mPortTF.mZoneRect.x - 125f, mPortTF.mZoneRect.y, 100f, mPortTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Battle id", new Rect(mBattleTF.mZoneRect.x - 125f, mBattleTF.mZoneRect.y, 100f, mBattleTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Map id", new Rect(mMapTF.mZoneRect.x - 125f, mMapTF.mZoneRect.y, 100f, mMapTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Team", new Rect(mTeamTF.mZoneRect.x - 125f, mTeamTF.mZoneRect.y, 100f, mTeamTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Avatar", new Rect(mAvatarTF.mZoneRect.x - 125f, mAvatarTF.mZoneRect.y, 100f, mAvatarTF.mZoneRect.height), "middle_left");
		GuiSystem.DrawString("Scene", new Rect(mSceneTF.mZoneRect.x - 125f, mSceneTF.mZoneRect.y, 100f, mSceneTF.mZoneRect.height), "middle_left");
		GUI.contentColor = Color.white;
		mJoinBtn.RenderElement();
		mBackBtn.RenderElement();
		mAddParamBtn.RenderElement();
		mDelParamBtn.RenderElement();
		Rect _rect = new Rect(0f, 0f, 40f, 40f);
		Vector2 pos = new Vector2(GuiSystem.mBaseScreenWidth - 40f, GuiSystem.mBaseScreenHeight - 40f);
		GuiSystem.RecalculateRect(pos, ref _rect);
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Variable> item in mAvaParams.Associative)
		{
			MixedArray mixedArray = item.Value;
			string text = mixedArray["ADD"].Value.ToString();
			string text2 = mixedArray["MUL"].Value.ToString();
			list.Add(item.Key + " " + text + " " + text2);
		}
		mSelectedAvaParam = GUI.SelectionGrid(new Rect(50f, 450f, 150f, list.Count * 20), mSelectedAvaParam, list.ToArray(), 1);
	}

	public override void OnInput()
	{
		mHostTF.OnInput();
		mPortTF.OnInput();
		mBattleTF.OnInput();
		mMapTF.OnInput();
		mTeamTF.OnInput();
		mAvatarTF.OnInput();
		mSceneTF.OnInput();
		mParamTF.OnInput();
		mAddParamValTF.OnInput();
		mMulParamValTF.OnInput();
		mAddParamBtn.OnInput();
		mDelParamBtn.OnInput();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mJoinBtn.CheckEvent(_curEvent);
		mBackBtn.CheckEvent(_curEvent);
		mAddParamBtn.CheckEvent(_curEvent);
		mDelParamBtn.CheckEvent(_curEvent);
	}

	private void SaveFields()
	{
		try
		{
			PlayerPrefs.SetString("server_ip", mHostTF.mData);
			PlayerPrefs.SetString("server_port", mPortTF.mData);
			PlayerPrefs.SetString("server_battle", mBattleTF.mData);
			PlayerPrefs.SetString("server_map", mMapTF.mData);
			PlayerPrefs.SetString("server_team", mTeamTF.mData);
			PlayerPrefs.SetString("server_avatar", mAvatarTF.mData);
			PlayerPrefs.SetString("ctrl_scene", mSceneTF.mData);
		}
		catch (PlayerPrefsException ex)
		{
			Log.Error("failed save parameters. exception detected " + ex.ToString());
		}
	}

	private void OnJoinBtnUp(GuiElement _sender, int _buttonId)
	{
		SaveFields();
		if (mJoinCallback != null)
		{
			BattleServerData battleServerData = new BattleServerData();
			battleServerData.mHost = mHostTF.mData;
			battleServerData.mPorts = XmlUtil.StringToIntList(mPortTF.mData, ',').ToArray();
			BattleServerData.DebugJoinData debugJoinData = (battleServerData.mDebugJoin = new BattleServerData.DebugJoinData());
			debugJoinData.mAvatarPrefab = mAvatarTF.mData;
			debugJoinData.mBattleId = int.Parse(mBattleTF.mData);
			debugJoinData.mMapId = int.Parse(mMapTF.mData);
			debugJoinData.mTeam = int.Parse(mTeamTF.mData);
			debugJoinData.mAvatarParams = mAvaParams;
			BattleMapData battleMapData = new BattleMapData();
			battleMapData.mMapName = mSceneTF.mData;
			mJoinCallback(battleServerData, battleMapData);
		}
	}

	private void OnBackBtnUp(GuiElement _sender, int _buttonId)
	{
		SaveFields();
		if (mBackCallback != null)
		{
			mBackCallback();
		}
	}

	private void AddParam(GuiElement _sender, int _buttonId)
	{
		//Discarded unreachable code: IL_007f
		string mData = mParamTF.mData;
		string mData2 = mAddParamValTF.mData;
		string mData3 = mMulParamValTF.mData;
		if (!string.IsNullOrEmpty(mData))
		{
			mParamTF.mData = (mAddParamValTF.mData = (mMulParamValTF.mData = string.Empty));
			int num = 0;
			int num2 = 0;
			try
			{
				num = int.Parse(mData2);
				num2 = int.Parse(mData3);
			}
			catch (FormatException)
			{
				return;
			}
			MixedArray mixedArray = new MixedArray();
			mixedArray["ADD"] = num;
			mixedArray["MUL"] = num2;
			mAvaParams[mData] = mixedArray;
		}
	}

	private void DelParam(GuiElement _sender, int _buttonId)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Variable> item in mAvaParams.Associative)
		{
			list.Add(item.Key);
		}
		if (mSelectedAvaParam >= 0 && mSelectedAvaParam < list.Count)
		{
			string key = list[mSelectedAvaParam];
			mAvaParams.Associative.Remove(key);
		}
	}
}
