using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CastleRequestMenu : GuiElement, EscapeListener
{
	private class ClanMemberComparer : IComparer<CastleRequestMember>
	{
		public int Compare(CastleRequestMember _obj1, CastleRequestMember _obj2)
		{
			int num = ((int)_obj2.Role).CompareTo((int)_obj1.Role);
			if (num == 0)
			{
				return _obj1.Name.CompareTo(_obj2.Name);
			}
			return num;
		}
	}

	private class CastleRequestMember : GuiElement
	{
		public delegate void OnSwitchMember(int _id);

		private Texture2D mFrame;

		private GuiButton mButton;

		private string mName;

		private Rect mPlaceRect;

		private Rect mNameRect;

		private bool mQueue;

		private Clan.ClanMember mMemberData;

		private string mClanTag;

		private int mPlace;

		public OnSwitchMember mOnSwitchMember;

		public Clan.Role Role => (mMemberData != null) ? mMemberData.Role : ((Clan.Role)0);

		public string Name => mName;

		public override void Init()
		{
			if (mMemberData != null)
			{
				mName = "[" + mClanTag + "]" + mMemberData.Name;
				if (mQueue)
				{
					mButton = GuiSystem.CreateButton("Gui/CastleRequestMenu/button_1_norm", "Gui/CastleRequestMenu/button_1_over", "Gui/CastleRequestMenu/button_1_press", string.Empty, string.Empty);
					mButton.mElementId = "ARROW_BTN";
					GuiButton guiButton = mButton;
					guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
					mButton.Init();
				}
				else if (mPlace == -1)
				{
					mFrame = GuiSystem.GetImage("Gui/CastleRequestMenu/frame4");
				}
				else if (mPlace == 0)
				{
					mFrame = GuiSystem.GetImage("Gui/CastleRequestMenu/frame2");
					mButton = GuiSystem.CreateButton("Gui/CastleRequestMenu/button_2_norm", "Gui/CastleRequestMenu/button_2_over", "Gui/CastleRequestMenu/button_2_press", string.Empty, string.Empty);
					mButton.mElementId = "ARROW_BTN";
					GuiButton guiButton2 = mButton;
					guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
					mButton.Init();
				}
				else if (mPlace >= 1 && mPlace <= 12)
				{
					mFrame = GuiSystem.GetImage("Gui/CastleRequestMenu/frame3");
				}
			}
		}

		public override void SetSize()
		{
			mNameRect = new Rect(28f, -1f, 240f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			if (mQueue)
			{
				mButton.mZoneRect = new Rect(273f, 1f, 16f, 16f);
				mPlaceRect = new Rect(1f, 1f, 16f, 16f);
				GuiSystem.SetChildRect(mZoneRect, ref mPlaceRect);
			}
			else if (mButton != null)
			{
				mButton.mZoneRect = new Rect(1f, 1f, 16f, 16f);
			}
			if (mButton != null)
			{
				GuiSystem.SetChildRect(mZoneRect, ref mButton.mZoneRect);
			}
		}

		public override void RenderElement()
		{
			if (mFrame != null)
			{
				GuiSystem.DrawImage(mFrame, mZoneRect);
			}
			GuiSystem.DrawString(mName, mNameRect, "middle_left");
			if (mQueue)
			{
				GuiSystem.DrawString(mPlace.ToString(), mPlaceRect, "middle_center");
			}
			if (mButton != null)
			{
				mButton.RenderElement();
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mButton != null)
			{
				mButton.CheckEvent(_curEvent);
			}
		}

		public void SetData(bool _queue, Clan.ClanMember _memberData, string _clanTag, int _place)
		{
			mQueue = _queue;
			mMemberData = _memberData;
			mClanTag = _clanTag;
			mPlace = _place;
		}

		public int GetUserId()
		{
			if (mMemberData == null)
			{
				return -1;
			}
			return mMemberData.Id;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "ARROW_BTN" && _buttonId == 0 && mOnSwitchMember != null && mMemberData != null)
			{
				mOnSwitchMember(mMemberData.Id);
			}
		}
	}

	public delegate void CancelRequest(int _castleId);

	public delegate void AcceptFighters(int _castleId, Dictionary<int, int> _fighters);

	public delegate void OnClanInfo(int _clanId);

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private GuiButton mAcceptButton;

	private GuiButton mCancelButton;

	private string mLabel;

	private Rect mLabelRect;

	private CastleInfo mCastleInfo;

	private bool mEdit;

	private string mClanTag;

	private GuiButton mCastleClanControlButton;

	private string mCastleClanLevel;

	private string mCastleBattleStartTimeText;

	private string mCastleClanControlText;

	private string mCastleBattleStartTime;

	private string mCastleClanControl;

	private Rect mCastleClanLevelRect;

	private Rect mCastleBattleStartTimeTextRect;

	private Rect mCastleClanControlTextRect;

	private Rect mCastleBattleStartTimeRect;

	private Rect mCastleClanControlRect;

	private string mCastleMakeRequestText;

	private string mClanText;

	private string mBaseText;

	private string mReserveText;

	private Rect mCastleMakeRequestTextRect;

	private Rect mClanTextRect;

	private Rect mBaseTextRect;

	private Rect mReserveTextRect;

	private YesNoDialog mYesNoDialog;

	private OkDialog mOkDialog;

	private List<CastleRequestMember> mClanMembers;

	private Dictionary<int, CastleRequestMember> mTeamMembers;

	private Dictionary<int, List<Clan.ClanMember>> mClanData;

	private Dictionary<int, KeyValuePair<int, Clan.ClanMember>> mAllMembers;

	private bool mMembersChanged;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private Rect mDrawRect;

	public CancelRequest mCancelRequest;

	public AcceptFighters mAcceptFighters;

	public OnClanInfo mOnClanInfo;

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			Close();
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(390, this);
		mFrame = GuiSystem.GetImage("Gui/CastleRequestMenu/frame1");
		mStartScrollOffset = 224f;
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mAcceptButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mAcceptButton.mElementId = "ACCEPT_BUTTON";
		GuiButton guiButton2 = mAcceptButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mAcceptButton.mLabel = GuiSystem.GetLocaleText("Castle_Request_Accept");
		mAcceptButton.Init();
		mCancelButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "CANCEL_BUTTON";
		GuiButton guiButton3 = mCancelButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.Init();
		mCastleClanControlButton = new GuiButton();
		mCastleClanControlButton.mElementId = "CASTLE_CLAN_CONTROL_BUTTON";
		GuiButton guiButton4 = mCastleClanControlButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mCastleClanControlButton.mOverColor = Color.yellow;
		mCastleClanControlButton.mLabelStyle = "middle_left";
		mCastleClanControlButton.Init();
		mCastleBattleStartTimeText = GuiSystem.GetLocaleText("Castle_Battle_Start_Time");
		mCastleClanControlText = GuiSystem.GetLocaleText("Castle_Clan_Control");
		mCastleMakeRequestText = GuiSystem.GetLocaleText("Castle_Make_Request_Text");
		mClanText = GuiSystem.GetLocaleText("Castle_Request_Clan_Text");
		mBaseText = GuiSystem.GetLocaleText("Castle_Request_Base_Text");
		mReserveText = GuiSystem.GetLocaleText("Castle_Request_Reserve_Text");
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		mTeamMembers = new Dictionary<int, CastleRequestMember>();
		mClanMembers = new List<CastleRequestMember>();
		mAllMembers = new Dictionary<int, KeyValuePair<int, Clan.ClanMember>>();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(661f, 8f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mAcceptButton.mZoneRect = new Rect(32f, 488f, 154f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mAcceptButton.mZoneRect);
		mCancelButton.mZoneRect = new Rect(509f, 488f, 154f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		mLabelRect = new Rect(28f, 8f, 644f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		mCastleClanLevelRect = new Rect(254f, 58f, 188f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanLevelRect);
		mCastleBattleStartTimeRect = new Rect(297f, 111f, 158f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleBattleStartTimeRect);
		mCastleClanControlRect = new Rect(303f, 142f, 360f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanControlRect);
		mCastleClanControlButton.mZoneRect = mCastleClanControlRect;
		mCastleBattleStartTimeTextRect = new Rect(8f, 111f, 265f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleBattleStartTimeTextRect);
		mCastleClanControlTextRect = new Rect(8f, 142f, 265f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleClanControlTextRect);
		mCastleMakeRequestTextRect = new Rect(38f, 184f, 620f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mCastleMakeRequestTextRect);
		mClanTextRect = new Rect(339f, 212f, 296f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mClanTextRect);
		mBaseTextRect = new Rect(37f, 212f, 296f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mBaseTextRect);
		mReserveTextRect = new Rect(37f, 401f, 296f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref mReserveTextRect);
		mScrollbar.mZoneRect = new Rect(639f, 210f, 22f, 251f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mDrawRect = new Rect(342f, 234f, 292f, 224f);
		GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
		SetTeamMembersSize();
		SetClanMembersSize();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		GuiSystem.DrawString(mCastleMakeRequestText, mCastleMakeRequestTextRect, "label");
		GuiSystem.DrawString(mClanText, mClanTextRect, "label");
		GuiSystem.DrawString(mBaseText, mBaseTextRect, "label");
		GuiSystem.DrawString(mReserveText, mReserveTextRect, "label");
		GuiSystem.DrawString(mCastleBattleStartTimeText, mCastleBattleStartTimeTextRect, "middle_right");
		GuiSystem.DrawString(mCastleBattleStartTime, mCastleBattleStartTimeRect, "middle_center");
		GuiSystem.DrawString(mCastleClanLevel, mCastleClanLevelRect, "middle_center");
		GuiSystem.DrawString(mCastleClanControlText, mCastleClanControlTextRect, "middle_right");
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.RenderElement();
		}
		else
		{
			GuiSystem.DrawString(mCastleClanControl, mCastleClanControlRect, "middle_left");
		}
		foreach (CastleRequestMember mClanMember in mClanMembers)
		{
			if (mClanMember.Active)
			{
				mClanMember.RenderElement();
			}
		}
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			mTeamMember.Value.RenderElement();
		}
		mCloseButton.RenderElement();
		mAcceptButton.RenderElement();
		mCancelButton.RenderElement();
		mScrollbar.RenderElement();
	}

	public override void OnInput()
	{
		mScrollbar.OnInput();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mAcceptButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		if (string.IsNullOrEmpty(mCastleClanControl))
		{
			mCastleClanControlButton.CheckEvent(_curEvent);
		}
		foreach (CastleRequestMember mClanMember in mClanMembers)
		{
			if (mClanMember.Active)
			{
				mClanMember.CheckEvent(_curEvent);
			}
		}
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			mTeamMember.Value.CheckEvent(_curEvent);
		}
		mScrollbar.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void Update()
	{
		if (mMembersChanged)
		{
			InitMembers();
		}
	}

	public override void Uninit()
	{
		mCastleInfo = null;
		mYesNoDialog = null;
		mOkDialog = null;
		mClanData = null;
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			CastleRequestMember value = mTeamMember.Value;
			value.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Remove(value.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
		}
		foreach (CastleRequestMember mClanMember in mClanMembers)
		{
			mClanMember.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Remove(mClanMember.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
		}
		mTeamMembers.Clear();
		mClanMembers.Clear();
		mAllMembers.Clear();
	}

	public void Close()
	{
		SetActive(_active: false);
		mCastleInfo = null;
		mCastleClanControl = string.Empty;
	}

	public void Open()
	{
		SetActive(_active: true);
		InitData();
	}

	public void SetData(YesNoDialog _yesNoDialog, OkDialog _okDialog)
	{
		mYesNoDialog = _yesNoDialog;
		mOkDialog = _okDialog;
	}

	public void SetCastleData(CastleInfo _castleData)
	{
		if (_castleData != null)
		{
			mCastleInfo = _castleData;
		}
	}

	public void SetFighters(Dictionary<int, List<Clan.ClanMember>> _clanData, string _clanTag, bool _canLeave)
	{
		mClanData = _clanData;
		mClanTag = _clanTag;
		mEdit = _canLeave;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "ACCEPT_BUTTON" && _buttonId == 0)
		{
			SendAcceptFighters();
		}
		else if (_sender.mElementId == "CANCEL_BUTTON" && _buttonId == 0)
		{
			if (mEdit)
			{
				YesNoDialog.OnAnswer callback = delegate(bool _yes)
				{
					if (_yes && mCancelRequest != null && mCastleInfo != null)
					{
						mCancelRequest(mCastleInfo.mId);
					}
				};
				mYesNoDialog.SetData(GuiSystem.GetLocaleText("Castle_Request_Cancel_Question"), "YES_TEXT", "CANCEL_TEXT", callback);
			}
			else
			{
				Close();
			}
		}
		else if (_sender.mElementId == "CASTLE_CLAN_CONTROL_BUTTON" && _buttonId == 0)
		{
			OnGetClanInfo(_sender.mId);
		}
	}

	private void InitData()
	{
		if (mCastleInfo != null && mClanData != null)
		{
			mScrollOffset = 0f;
			mScrollbar.Refresh();
			mLabel = GuiSystem.GetLocaleText(mCastleInfo.mName);
			mCastleClanLevel = GuiSystem.GetLocaleText("Castle_Clan_Level");
			string text = mCastleClanLevel;
			mCastleClanLevel = text + " " + mCastleInfo.mLevelMin + "-" + mCastleInfo.mLevelMax;
			mCastleBattleStartTime = mCastleInfo.mStartTime.ToString();
			if (string.IsNullOrEmpty(mCastleInfo.mOwnerName))
			{
				mCastleClanControl = GuiSystem.GetLocaleText("Castle_Clan_No_Control_Text");
				mCastleClanControlButton.mLabel = string.Empty;
				mCastleClanControlButton.mId = 0;
			}
			else
			{
				mCastleClanControl = string.Empty;
				mCastleClanControlButton.mLabel = mCastleInfo.mOwnerName;
				mCastleClanControlButton.mId = mCastleInfo.mOwnerId;
			}
			mCancelButton.mLabel = ((!mEdit) ? GuiSystem.GetLocaleText("Close_Button_Name") : GuiSystem.GetLocaleText("Castle_Request_Cancel"));
			InitAllMembers();
			InitMembers();
		}
	}

	private void InitAllMembers()
	{
		if (mClanData == null)
		{
			return;
		}
		mAllMembers.Clear();
		foreach (KeyValuePair<int, List<Clan.ClanMember>> mClanDatum in mClanData)
		{
			foreach (Clan.ClanMember item in mClanDatum.Value)
			{
				mAllMembers.Add(item.Id, new KeyValuePair<int, Clan.ClanMember>(mClanDatum.Key, item));
			}
		}
	}

	private void InitMembers()
	{
		if (mClanData == null)
		{
			return;
		}
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			CastleRequestMember value = mTeamMember.Value;
			value.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Remove(value.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
		}
		foreach (CastleRequestMember mClanMember in mClanMembers)
		{
			mClanMember.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Remove(mClanMember.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
		}
		mTeamMembers.Clear();
		mClanMembers.Clear();
		foreach (KeyValuePair<int, KeyValuePair<int, Clan.ClanMember>> mAllMember in mAllMembers)
		{
			if (mAllMember.Value.Key > 0)
			{
				AddTeamMember(mAllMember.Value.Key, mAllMember.Value.Value);
			}
			AddClanMember(mAllMember.Value.Key, mAllMember.Value.Value);
		}
		SortClanMembers();
		SetTeamMembersSize();
		SetClanMembersSize();
		mMembersChanged = false;
	}

	private void AddTeamMember(int _place, Clan.ClanMember _memberData)
	{
		if (_place >= 1 && _place <= 12 && _memberData != null)
		{
			CastleRequestMember castleRequestMember = new CastleRequestMember();
			castleRequestMember.SetData(_queue: true, _memberData, mClanTag, _place);
			castleRequestMember.Init();
			castleRequestMember.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Combine(castleRequestMember.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
			mTeamMembers.Add(_place, castleRequestMember);
		}
	}

	private void AddClanMember(int _place, Clan.ClanMember _memberData)
	{
		if (_place >= -1 && _place <= 12 && _memberData != null)
		{
			CastleRequestMember castleRequestMember = new CastleRequestMember();
			castleRequestMember.SetData(_queue: false, _memberData, mClanTag, _place);
			castleRequestMember.Init();
			castleRequestMember.mOnSwitchMember = (CastleRequestMember.OnSwitchMember)Delegate.Combine(castleRequestMember.mOnSwitchMember, new CastleRequestMember.OnSwitchMember(SwitchMember));
			mClanMembers.Add(castleRequestMember);
		}
	}

	private void SetTeamMembersSize()
	{
		if (mCastleInfo == null)
		{
			return;
		}
		int num = 39;
		int num2 = 234;
		int num3 = 17;
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			Rect _rect = ((mTeamMember.Key > mCastleInfo.mFightersMin) ? new Rect(num, num2 + mTeamMember.Key * num3, 290f, 18f) : new Rect(num, num2 + (mTeamMember.Key - 1) * num3, 290f, 18f));
			GuiSystem.SetChildRect(mZoneRect, ref _rect);
			mTeamMember.Value.mZoneRect = _rect;
			mTeamMember.Value.SetSize();
		}
	}

	private void SortClanMembers()
	{
		ClanMemberComparer comparer = new ClanMemberComparer();
		mClanMembers.Sort(comparer);
	}

	private void SetClanMembersSize()
	{
		int num = 342;
		int num2 = 234;
		int num3 = 17;
		int num4 = 0;
		int num5 = 0;
		foreach (CastleRequestMember mClanMember in mClanMembers)
		{
			mClanMember.mZoneRect = new Rect(num, (float)(num2 + num4 * num3) - mScrollOffset, 290f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mClanMember.mZoneRect);
			mClanMember.SetSize();
			mClanMember.SetActive(!(mClanMember.mZoneRect.y + 1f < mDrawRect.y) && !(mClanMember.mZoneRect.y + mClanMember.mZoneRect.height > mDrawRect.y + mDrawRect.height + 1f));
			num5 += num3;
			num4++;
		}
		mScrollbar.SetData(mStartScrollOffset, num5);
	}

	private void SwitchMember(int _id)
	{
		if (!mAllMembers.TryGetValue(_id, out var value))
		{
			return;
		}
		int num = value.Key;
		if (num > -1)
		{
			switch (num)
			{
			case 0:
				num = GetFreeTeamPlace();
				break;
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
				num = 0;
				break;
			}
			mAllMembers[_id] = new KeyValuePair<int, Clan.ClanMember>(num, value.Value);
			mMembersChanged = true;
		}
	}

	private int GetFreeTeamPlace()
	{
		if (mCastleInfo == null)
		{
			return 0;
		}
		for (int i = 1; i <= mCastleInfo.mFightersMin; i++)
		{
			if (!mTeamMembers.ContainsKey(i))
			{
				return i;
			}
		}
		if (!mTeamMembers.ContainsKey(11))
		{
			return 11;
		}
		if (!mTeamMembers.ContainsKey(12))
		{
			return 12;
		}
		return 0;
	}

	private void SendAcceptFighters()
	{
		if (mAcceptFighters == null || !ValidateTeam())
		{
			return;
		}
		Dictionary<int, int> curFighters = new Dictionary<int, int>();
		int num = -1;
		foreach (KeyValuePair<int, CastleRequestMember> mTeamMember in mTeamMembers)
		{
			num = mTeamMember.Value.GetUserId();
			if (num != -1)
			{
				curFighters.Add(num, mTeamMember.Key);
			}
		}
		if (mCastleInfo.mSelfOwner)
		{
			mAcceptFighters(mCastleInfo.mId, curFighters);
			return;
		}
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			if (_yes)
			{
				mAcceptFighters(mCastleInfo.mId, curFighters);
			}
		};
		string localeText = GuiSystem.GetLocaleText("GUI_CASTLE_ENTER_WARNING");
		mYesNoDialog.SetData(localeText, "Accept2_Button_Name", "Cancel_Button_Name", callback);
	}

	private bool ValidateTeam()
	{
		if (mCastleInfo == null || mOkDialog == null)
		{
			return false;
		}
		for (int i = 1; i <= mCastleInfo.mFightersMin; i++)
		{
			if (!mTeamMembers.ContainsKey(i))
			{
				string localeText = GuiSystem.GetLocaleText("Castle_Request_Not_Enought_Members");
				localeText = localeText.Replace("{COUNT}", mCastleInfo.mFightersMin.ToString());
				mOkDialog.SetData(localeText);
				return false;
			}
		}
		return true;
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetClanMembersSize();
		}
	}

	private void OnGetClanInfo(int _clanId)
	{
		if (mOnClanInfo != null)
		{
			mOnClanInfo(_clanId);
		}
	}
}
