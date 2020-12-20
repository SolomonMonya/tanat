using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CSMainMenu : GuiElement
{
	private class ShortHeroInfo : GuiElement
	{
		public OnPlayerCallback mOnPlayer;

		public Texture2D mFrameImage;

		public Texture2D mFrameImageBg;

		public Texture2D mExpImage;

		public Texture2D mLeaderImage;

		public Texture2D mFaceImage;

		public int mPlayerId;

		public string mName;

		public string mLevel;

		public string mOnlineText;

		public string mOfflineText;

		public Rect mNameRect;

		public Rect mLevelRect;

		public Rect mExpRect;

		public Rect mFaceRect;

		public Rect mStatusRect;

		public bool mLeader;

		public bool mSelfHero;

		public bool mOnline;

		private Rect mCurExpRect;

		private string mExpString;

		private bool mInited;

		public bool Inited
		{
			get
			{
				return mInited;
			}
			set
			{
				mInited = value;
			}
		}

		public ShortHeroInfo()
		{
			mPlayerId = -1;
			mName = string.Empty;
			mLevel = string.Empty;
			mExpString = string.Empty;
			mLeader = false;
			mSelfHero = false;
			mInited = false;
			mOnline = true;
		}

		public override void Uninit()
		{
			mPlayerId = -1;
			mName = string.Empty;
			mLevel = string.Empty;
			mExpString = string.Empty;
			mLeader = false;
			mSelfHero = false;
			mInited = false;
			mOnline = true;
		}

		public void SetData(int _exp, int _maxExp)
		{
			mCurExpRect = new Rect(mExpRect);
			mExpString = _exp + "/" + _maxExp;
			mCurExpRect.width *= (float)_exp / (float)_maxExp;
		}

		public override void RenderElement()
		{
			if (mInited)
			{
				GuiSystem.DrawImage(mFrameImageBg, mZoneRect);
				if (mSelfHero)
				{
					GuiSystem.DrawImage(mExpImage, mCurExpRect);
				}
				GuiSystem.DrawImage(mFaceImage, mFaceRect);
				if (mLeader)
				{
					GuiSystem.DrawImage(mLeaderImage, mFaceRect);
				}
				GuiSystem.DrawImage(mFrameImage, mZoneRect);
				if (mSelfHero)
				{
					GuiSystem.DrawString(mExpString, mExpRect, "middle_center");
				}
				GuiSystem.DrawString(mName, mNameRect, "middle_center");
				GuiSystem.DrawString(mLevel, mLevelRect, "middle_center");
				if (mOnline)
				{
					GuiSystem.DrawString(mOnlineText, mStatusRect, "middle_center");
				}
				else
				{
					GuiSystem.DrawString(mOfflineText, mStatusRect, "middle_center");
				}
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mInited)
			{
				if (_curEvent.type == EventType.MouseUp && _curEvent.button == 1 && mZoneRect.Contains(_curEvent.mousePosition) && mOnPlayer != null)
				{
					mOnPlayer(mPlayerId, _curEvent.mousePosition);
				}
				base.CheckEvent(_curEvent);
			}
		}
	}

	public delegate void VoidCallback();

	public delegate void OnPlayerCallback(object _playerId, Vector2 _pos);

	private Texture2D mFrame1;

	private Texture2D mFrame2;

	private Texture2D mFaceFrame1;

	private Texture2D mFaceFrame2;

	private Texture2D mFaceFrame1Bg;

	private Texture2D mFaceFrame2Bg;

	private Texture2D mExpImage;

	private Texture2D mCrownImage1;

	private Texture2D mCrownImage2;

	private Rect mFrame2Rect = default(Rect);

	private List<GuiButton> mButtons = new List<GuiButton>();

	private List<ShortHeroInfo> mGroupInfo;

	private ShortHeroInfo mSelfHeroInfo;

	private ShortHeroInfo mNotherHeroInfo;

	public VoidCallback mOnShowWorldMap;

	public VoidCallback mShowBattleMenuCallback;

	public VoidCallback mShowHeroInfoCallback;

	public VoidCallback mShowShopCallback;

	public VoidCallback mShowShopRealCallback;

	public VoidCallback mShowBagCallback;

	public VoidCallback mShowBankCallback;

	public VoidCallback mShowNPCCallback;

	public VoidCallback mShowClanCallback;

	public OnPlayerCallback mOnPlayer;

	private Group mGroup;

	private SelfPlayer mSelfPlayer;

	private BuffRenderer mBuffRenderer = new BuffRenderer();

	private Dictionary<int, Group.Member> mInvalidPlayers;

	private Texture2D mWorldMapFrame;

	private Rect mWorldMapFrameRect;

	private GuiButton mWorldMapButton;

	public override void Init()
	{
		mInvalidPlayers = new Dictionary<int, Group.Member>();
		mFrame1 = GuiSystem.GetImage("Gui/CSMainMenu/frame1");
		mFrame2 = GuiSystem.GetImage("Gui/misc/background2");
		mFaceFrame1 = GuiSystem.GetImage("Gui/CSMainMenu/frame2");
		mFaceFrame2 = GuiSystem.GetImage("Gui/CSMainMenu/frame3");
		mFaceFrame1Bg = GuiSystem.GetImage("Gui/CSMainMenu/frame2_1");
		mFaceFrame2Bg = GuiSystem.GetImage("Gui/CSMainMenu/frame3_1");
		mCrownImage1 = GuiSystem.GetImage("Gui/CSMainMenu/crown_1");
		mCrownImage2 = GuiSystem.GetImage("Gui/CSMainMenu/crown_2");
		mExpImage = GuiSystem.GetImage("Gui/misc/frame_exp_1");
		mWorldMapFrame = GuiSystem.GetImage("Gui/CSMainMenu/frame4");
		mWorldMapButton = GuiSystem.CreateButton("Gui/CSMainMenu/button_11_norm", "Gui/CSMainMenu/button_11_over", "Gui/CSMainMenu/button_11_press", string.Empty, string.Empty);
		mWorldMapButton.mElementId = "WORLD_MAP_BUTTON";
		GuiButton guiButton = mWorldMapButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mWorldMapButton.Init();
		mSelfHeroInfo = new ShortHeroInfo();
		mSelfHeroInfo.mFrameImage = mFaceFrame1;
		mSelfHeroInfo.mFrameImageBg = mFaceFrame1Bg;
		mSelfHeroInfo.mExpImage = mExpImage;
		mSelfHeroInfo.mSelfHero = true;
		mSelfHeroInfo.mLeaderImage = mCrownImage1;
		mSelfHeroInfo.mOnlineText = GuiSystem.GetLocaleText("Online_Text");
		mSelfHeroInfo.mOfflineText = GuiSystem.GetLocaleText("Offline_Text");
		mSelfHeroInfo.mOnPlayer = OnPlayerClick;
		mNotherHeroInfo = new ShortHeroInfo();
		mNotherHeroInfo.mFrameImage = mFaceFrame1;
		mNotherHeroInfo.mFrameImageBg = mFaceFrame1Bg;
		mNotherHeroInfo.mExpImage = mExpImage;
		mNotherHeroInfo.mSelfHero = true;
		mNotherHeroInfo.mLeaderImage = mCrownImage1;
		mNotherHeroInfo.mOnlineText = GuiSystem.GetLocaleText("Online_Text");
		mNotherHeroInfo.mOfflineText = GuiSystem.GetLocaleText("Offline_Text");
		mNotherHeroInfo.mOnPlayer = OnPlayerClick;
		mBuffRenderer.mSize = 40f;
		mBuffRenderer.mStartX = 40f;
		mBuffRenderer.mStartY = -50f;
		mBuffRenderer.mOffsetX = 45f;
		mBuffRenderer.mOffsetY = -45f;
		InitGroupInfo();
		InitButtons();
		OnGroupChanged(mGroup);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(576f, 0f, mFrame1.width, mFrame1.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		float num = (float)OptionsMgr.mScreenWidth - 771f * GuiSystem.mYRate;
		mZoneRect.x += (num - mZoneRect.width) / 2f;
		mZoneRect.y = (float)OptionsMgr.mScreenHeight - mZoneRect.height;
		mWorldMapFrameRect = new Rect(0f, 0f, mWorldMapFrame.width, mWorldMapFrame.height);
		GuiSystem.GetRectScaled(ref mWorldMapFrameRect);
		mWorldMapFrameRect.x = ((float)OptionsMgr.mScreenWidth - mWorldMapFrameRect.width) / 2f;
		mFrame2Rect = new Rect(0f, 0f, mFrame2.width, mFrame2.height);
		GuiSystem.GetRectScaled(ref mFrame2Rect, _ignoreLowRate: true);
		mFrame2Rect.x = ((float)OptionsMgr.mScreenWidth - mFrame2Rect.width) / 2f;
		mFrame2Rect.y = (float)OptionsMgr.mScreenHeight - mFrame2Rect.height;
		mButtons[0].mZoneRect = new Rect(168f, 22f, 179f, 46f);
		mButtons[1].mZoneRect = new Rect(416f, 9f, 58f, 59f);
		mButtons[2].mZoneRect = new Rect(39f, 9f, 58f, 59f);
		mWorldMapButton.mZoneRect = new Rect(87f, 1f, 46f, 46f);
		GuiSystem.SetChildRect(mWorldMapFrameRect, ref mWorldMapButton.mZoneRect);
		mSelfHeroInfo.mZoneRect = new Rect(0f, 0f, mFaceFrame1.width, mFaceFrame1.height);
		GuiSystem.GetRectScaled(ref mSelfHeroInfo.mZoneRect);
		mSelfHeroInfo.mNameRect = new Rect(70f, 8f, 99f, 11f);
		mSelfHeroInfo.mLevelRect = new Rect(61f, 54f, 22f, 22f);
		mSelfHeroInfo.mExpRect = new Rect(75f, 29f, 130f, 16f);
		mSelfHeroInfo.mFaceRect = new Rect(12f, 12f, 50f, 50f);
		mSelfHeroInfo.mStatusRect = new Rect(86f, 46f, 101f, 12f);
		GuiSystem.SetChildRect(mSelfHeroInfo.mZoneRect, ref mSelfHeroInfo.mNameRect);
		GuiSystem.SetChildRect(mSelfHeroInfo.mZoneRect, ref mSelfHeroInfo.mLevelRect);
		GuiSystem.SetChildRect(mSelfHeroInfo.mZoneRect, ref mSelfHeroInfo.mExpRect);
		GuiSystem.SetChildRect(mSelfHeroInfo.mZoneRect, ref mSelfHeroInfo.mFaceRect);
		GuiSystem.SetChildRect(mSelfHeroInfo.mZoneRect, ref mSelfHeroInfo.mStatusRect);
		mNotherHeroInfo.mZoneRect = new Rect(mFaceFrame1.width + 20, 0f, mFaceFrame1.width, mFaceFrame1.height);
		GuiSystem.GetRectScaled(ref mNotherHeroInfo.mZoneRect);
		mNotherHeroInfo.mNameRect = new Rect(70f, 8f, 99f, 11f);
		mNotherHeroInfo.mLevelRect = new Rect(61f, 54f, 22f, 22f);
		mNotherHeroInfo.mExpRect = new Rect(75f, 29f, 130f, 16f);
		mNotherHeroInfo.mFaceRect = new Rect(12f, 12f, 50f, 50f);
		mNotherHeroInfo.mStatusRect = new Rect(86f, 46f, 101f, 12f);
		GuiSystem.SetChildRect(mNotherHeroInfo.mZoneRect, ref mNotherHeroInfo.mNameRect);
		GuiSystem.SetChildRect(mNotherHeroInfo.mZoneRect, ref mNotherHeroInfo.mLevelRect);
		GuiSystem.SetChildRect(mNotherHeroInfo.mZoneRect, ref mNotherHeroInfo.mExpRect);
		GuiSystem.SetChildRect(mNotherHeroInfo.mZoneRect, ref mNotherHeroInfo.mFaceRect);
		GuiSystem.SetChildRect(mNotherHeroInfo.mZoneRect, ref mNotherHeroInfo.mStatusRect);
		SetGroupInfoSize();
		for (int i = 3; i < 10; i++)
		{
			mButtons[i].mZoneRect = new Rect(54 + (i - 3) * 58, 104f, 57f, 66f);
		}
		foreach (GuiButton mButton in mButtons)
		{
			GuiSystem.SetChildRect(mZoneRect, ref mButton.mZoneRect);
		}
		PopupInfo.AddTip(this, "TIP_TEXT6", mButtons[0].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT7", mButtons[1].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT8", mButtons[2].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT9", mButtons[3].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT10", mButtons[4].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT11", mButtons[5].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT12", mButtons[8].mZoneRect);
		PopupInfo.AddTip(this, "TIP_TEXT13", mButtons[9].mZoneRect);
		mBuffRenderer.mBaseRect = mButtons[2].mZoneRect;
		mBuffRenderer.SetBuffSize();
	}

	private void InitGroupInfo()
	{
		mGroupInfo = new List<ShortHeroInfo>();
		ShortHeroInfo shortHeroInfo = null;
		for (int i = 0; i < 9; i++)
		{
			shortHeroInfo = new ShortHeroInfo();
			shortHeroInfo.mFrameImage = mFaceFrame2;
			shortHeroInfo.mFrameImageBg = mFaceFrame2Bg;
			shortHeroInfo.mLeaderImage = mCrownImage2;
			shortHeroInfo.mOnlineText = GuiSystem.GetLocaleText("Online_Text");
			shortHeroInfo.mOfflineText = GuiSystem.GetLocaleText("Offline_Text");
			shortHeroInfo.mOnPlayer = OnPlayerClick;
			mGroupInfo.Add(shortHeroInfo);
		}
	}

	private void SetGroupInfoSize()
	{
		ShortHeroInfo shortHeroInfo = null;
		int i = 0;
		for (int count = mGroupInfo.Count; i < count; i++)
		{
			shortHeroInfo = mGroupInfo[i];
			shortHeroInfo.mZoneRect = new Rect(10f, 80 + i * mFaceFrame2.height, mFaceFrame2.width, mFaceFrame2.height);
			GuiSystem.GetRectScaled(ref shortHeroInfo.mZoneRect);
			shortHeroInfo.mNameRect = new Rect(53f, 18f, 97f, 11f);
			shortHeroInfo.mLevelRect = new Rect(45f, 40f, 16f, 16f);
			shortHeroInfo.mFaceRect = new Rect(8f, 8f, 38f, 38f);
			shortHeroInfo.mStatusRect = new Rect(63f, 38f, 88f, 12f);
			GuiSystem.SetChildRect(shortHeroInfo.mZoneRect, ref shortHeroInfo.mNameRect);
			GuiSystem.SetChildRect(shortHeroInfo.mZoneRect, ref shortHeroInfo.mLevelRect);
			GuiSystem.SetChildRect(shortHeroInfo.mZoneRect, ref shortHeroInfo.mFaceRect);
			GuiSystem.SetChildRect(shortHeroInfo.mZoneRect, ref shortHeroInfo.mStatusRect);
		}
	}

	private void InitButtons()
	{
		GuiButton guiButton = null;
		guiButton = GuiSystem.CreateButton("Gui/CSMainMenu/button_1_norm", "Gui/CSMainMenu/button_1_over", "Gui/CSMainMenu/button_1_press", string.Empty, string.Empty);
		guiButton.mElementId = "BUTTON_BATTLE";
		GuiButton guiButton2 = guiButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		guiButton.RegisterAction(UserActionType.BUTTON_START_BATTLE);
		mButtons.Add(guiButton);
		guiButton = GuiSystem.CreateButton("Gui/CSMainMenu/button_2_norm", "Gui/CSMainMenu/button_2_over", "Gui/CSMainMenu/button_2_press", string.Empty, string.Empty);
		guiButton.mElementId = "BUTTON_INVENTORY";
		GuiButton guiButton3 = guiButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		guiButton.RegisterAction(UserActionType.BAG_CLICK, "LOG_CS");
		mButtons.Add(guiButton);
		guiButton = GuiSystem.CreateButton("Gui/CSMainMenu/button_3_norm", "Gui/CSMainMenu/button_3_over", "Gui/CSMainMenu/button_3_press", string.Empty, string.Empty);
		guiButton.mElementId = "BUTTON_HERO";
		GuiButton guiButton4 = guiButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		guiButton.RegisterAction(UserActionType.SELF_HERO_INFO, "LOG_CS");
		mButtons.Add(guiButton);
		string empty = string.Empty;
		for (int i = 4; i <= 10; i++)
		{
			empty = "Gui/CSMainMenu/button_" + i;
			guiButton = GuiSystem.CreateButton(empty + "_norm", empty + "_over", empty + "_press", string.Empty, string.Empty);
			guiButton.mElementId = "BUTTON_" + i;
			GuiButton guiButton5 = guiButton;
			guiButton5.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton5.mOnMouseUp, new OnMouseUp(OnButton));
			guiButton.mLocked = i > 6 && i < 9;
			guiButton.RegisterAction((UserActionType)(2000 + i), "LOG_CS");
			mButtons.Add(guiButton);
		}
		foreach (GuiButton mButton in mButtons)
		{
			mButton.Init();
			AddTutorialElement(mButton);
		}
	}

	public void ShowUserInfo(Player _player)
	{
		mNotherHeroInfo.Inited = false;
		if (_player != null && mSelfPlayer != null && _player.Id != mSelfPlayer.PlayerId)
		{
			Hero hero = _player.Hero;
			mNotherHeroInfo.mPlayerId = _player.Id;
			mNotherHeroInfo.mName = _player.Name;
			mNotherHeroInfo.mLevel = hero.GameInfo.mLevel.ToString();
			mNotherHeroInfo.SetData(hero.GameInfo.mExp, hero.GameInfo.mNextExp);
			int mRace = hero.View.mRace;
			if (mRace != -1 && mRace != 0)
			{
				string file = "Gui/HeroInfo/race_icon_" + mRace + "_" + (hero.View.mGender ? 1 : 0) + "_big";
				mNotherHeroInfo.mFaceImage = GuiSystem.GetImage(file);
				mNotherHeroInfo.Inited = true;
			}
		}
	}

	public override void Update()
	{
		if (mSelfHeroInfo == null || mSelfPlayer == null || mSelfPlayer.Player == null)
		{
			return;
		}
		Player player = mSelfPlayer.Player;
		Hero hero = player.Hero;
		mSelfHeroInfo.mPlayerId = mSelfPlayer.PlayerId;
		mSelfHeroInfo.mName = player.Name;
		mSelfHeroInfo.mLevel = hero.GameInfo.mLevel.ToString();
		mSelfHeroInfo.SetData(hero.GameInfo.mExp, hero.GameInfo.mNextExp);
		int mRace = hero.View.mRace;
		if (mRace != -1 && mRace != 0)
		{
			string file = "Gui/HeroInfo/race_icon_" + mRace + "_" + (hero.View.mGender ? 1 : 0) + "_big";
			mSelfHeroInfo.mFaceImage = GuiSystem.GetImage(file);
			mSelfHeroInfo.Inited = true;
		}
		if (mInvalidPlayers.Count > 0)
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, Group.Member> mInvalidPlayer in mInvalidPlayers)
			{
				ShortHeroInfo shortHeroInfo = mGroupInfo[mInvalidPlayer.Key];
				shortHeroInfo.Inited = InitPlayerInfo(shortHeroInfo, mInvalidPlayer.Value);
				if (shortHeroInfo.Inited)
				{
					list.Add(mInvalidPlayer.Key);
				}
			}
			foreach (int item in list)
			{
				mInvalidPlayers.Remove(item);
			}
		}
		mBuffRenderer.Update();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame2, mFrame2Rect);
		GuiSystem.DrawImage(mFrame1, mZoneRect);
		GuiSystem.DrawImage(mWorldMapFrame, mWorldMapFrameRect);
		if (mSelfHeroInfo != null)
		{
			mSelfHeroInfo.RenderElement();
		}
		if (mNotherHeroInfo != null)
		{
			mNotherHeroInfo.RenderElement();
		}
		foreach (ShortHeroInfo item in mGroupInfo)
		{
			item.RenderElement();
		}
		foreach (GuiButton mButton in mButtons)
		{
			mButton.RenderElement();
		}
		mWorldMapButton.RenderElement();
		mBuffRenderer.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (GuiButton mButton in mButtons)
		{
			mButton.CheckEvent(_curEvent);
		}
		if (mSelfHeroInfo != null)
		{
			mSelfHeroInfo.CheckEvent(_curEvent);
		}
		if (mNotherHeroInfo != null)
		{
			mNotherHeroInfo.CheckEvent(_curEvent);
		}
		foreach (ShortHeroInfo item in mGroupInfo)
		{
			item.CheckEvent(_curEvent);
		}
		if (_curEvent.type == EventType.KeyUp && GuiSystem.InputIsFree() && _curEvent.keyCode == KeyCode.I)
		{
			string comment = string.Format("{0} ({1})", GuiSystem.GetLocaleText("LOG_CS"), GuiSystem.GetLocaleText("LOG_HOTKEY"));
			UserLog.AddAction(UserActionType.BAG_CLICK, comment);
			if (mShowBagCallback != null)
			{
				mShowBagCallback();
			}
		}
		mWorldMapButton.CheckEvent(_curEvent);
		mBuffRenderer.CheckEvent(_curEvent);
	}

	public void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "BUTTON_BATTLE" && _buttonId == 0)
		{
			if (mShowBattleMenuCallback != null)
			{
				mShowBattleMenuCallback();
			}
		}
		else if (_sender.mElementId == "BUTTON_INVENTORY" && _buttonId == 0)
		{
			if (mShowBagCallback != null)
			{
				mShowBagCallback();
			}
		}
		else if (_sender.mElementId == "BUTTON_HERO" && _buttonId == 0)
		{
			if (mShowHeroInfoCallback != null)
			{
				mShowHeroInfoCallback();
			}
		}
		else if ("BUTTON_4" == _sender.mElementId && _buttonId == 0)
		{
			if (mShowShopCallback != null)
			{
				mShowShopCallback();
			}
		}
		else if ("BUTTON_5" == _sender.mElementId && _buttonId == 0)
		{
			if (mShowShopRealCallback != null)
			{
				mShowShopRealCallback();
			}
		}
		else if ("BUTTON_6" == _sender.mElementId && _buttonId == 0)
		{
			if (mShowBankCallback != null)
			{
				mShowBankCallback();
			}
		}
		else if ("BUTTON_9" == _sender.mElementId && _buttonId == 0)
		{
			if (mShowNPCCallback != null)
			{
				mShowNPCCallback();
			}
		}
		else if ("BUTTON_10" == _sender.mElementId && _buttonId == 0)
		{
			if (mShowClanCallback != null)
			{
				mShowClanCallback();
			}
		}
		else if (_sender.mElementId == "WORLD_MAP_BUTTON" && _buttonId == 0 && mOnShowWorldMap != null)
		{
			mOnShowWorldMap();
		}
	}

	public void SetData(Group _group, SelfPlayer _selfPlayer, IStoreContentProvider<CtrlPrototype> _prov, FormatedTipMgr _formatedTipMgr, HeroStore _heroStore)
	{
		if (_group == null)
		{
			throw new ArgumentNullException("_group");
		}
		if (_selfPlayer == null)
		{
			throw new ArgumentNullException("_selfPlayer");
		}
		if (_prov == null)
		{
			throw new ArgumentNullException("_prov");
		}
		if (_formatedTipMgr == null)
		{
			throw new ArgumentNullException("_formatedTipMgr");
		}
		if (_heroStore == null)
		{
			throw new ArgumentNullException("_heroStore");
		}
		mGroup = _group;
		mGroup.SubscribeChanged(OnGroupChanged);
		mSelfPlayer = _selfPlayer;
		mBuffRenderer.SetData(_selfPlayer.Player.Hero, _prov, _formatedTipMgr, _heroStore);
	}

	public void Clean()
	{
		if (mGroup != null)
		{
			mGroup.UnsubscribeChanged(OnGroupChanged);
			mGroup = null;
		}
		mBuffRenderer.Clean();
	}

	private void OnGroupChanged(Group _group)
	{
		ReinitMembers();
	}

	private void ReinitMembers()
	{
		CleanGroup();
		mInvalidPlayers.Clear();
		if (mGroup.IsEmpty)
		{
			mSelfHeroInfo.mLeader = false;
			return;
		}
		int num = 0;
		foreach (Group.Member member in mGroup.Members)
		{
			ShortHeroInfo shortHeroInfo = mGroupInfo[num];
			shortHeroInfo.mOnline = member.IsOnline;
			if (member.Id == mSelfPlayer.PlayerId)
			{
				mSelfHeroInfo.mLeader = member.IsLeader;
				continue;
			}
			shortHeroInfo.mPlayerId = member.Id;
			shortHeroInfo.mLeader = member.IsLeader;
			shortHeroInfo.mName = member.Name;
			shortHeroInfo.Inited = InitPlayerInfo(shortHeroInfo, member);
			if (!shortHeroInfo.Inited)
			{
				mInvalidPlayers.Add(num, member);
			}
			num++;
		}
	}

	private bool InitPlayerInfo(ShortHeroInfo _info, Group.Member _member)
	{
		Hero hero = _member.Hero;
		if (hero == null)
		{
			return false;
		}
		if (hero.GameInfo.mLevel == -1)
		{
			return false;
		}
		if (hero.View.mRace == -1)
		{
			return false;
		}
		_info.mLevel = hero.GameInfo.mLevel.ToString();
		string file = "Gui/HeroInfo/race_icon_" + hero.View.mRace + "_" + (hero.View.mGender ? 1 : 0) + "_small";
		_info.mFaceImage = GuiSystem.GetImage(file);
		return true;
	}

	private void CleanGroup()
	{
		foreach (ShortHeroInfo item in mGroupInfo)
		{
			item.Uninit();
		}
	}

	private void OnPlayerClick(object _playerId, Vector2 _pos)
	{
		if (mOnPlayer != null)
		{
			mOnPlayer(_playerId, _pos);
		}
	}
}
