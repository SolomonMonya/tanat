using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class ClanMenu : GuiElement, EscapeListener
{
	private class ClanMemberComparer : IComparer<Clan.ClanMember>
	{
		public int Compare(Clan.ClanMember _obj1, Clan.ClanMember _obj2)
		{
			if (_obj1.Location.Length > 0 && _obj2.Location.Length == 0)
			{
				return -1;
			}
			if (_obj2.Location.Length > 0 && _obj1.Location.Length == 0)
			{
				return 1;
			}
			int num = ((int)_obj2.Role).CompareTo((int)_obj1.Role);
			if (num == 0)
			{
				return _obj1.Name.CompareTo(_obj2.Name);
			}
			return num;
		}
	}

	private class InvitePlayerMenu : GuiElement
	{
		public delegate void OnInvite(string _nick);

		private Texture2D mFrame;

		private GuiButton mCloseButton;

		private GuiButton mInviteButton;

		private StaticTextField mInviteField;

		private string mLabel;

		private string mNick;

		private Rect mLabelRect;

		private Rect mNickRect;

		public OnInvite mOnInvite;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/ClanMenu/frame4");
			mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
			mCloseButton.mElementId = "CLOSE_BUTTON";
			GuiButton guiButton = mCloseButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mCloseButton.Init();
			mInviteButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
			mInviteButton.mElementId = "INVITE_BUTTON";
			GuiButton guiButton2 = mInviteButton;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
			mInviteButton.mLabel = GuiSystem.GetLocaleText("Clan_Make_Invite_Text");
			mInviteButton.Init();
			mInviteField = new StaticTextField();
			mInviteField.mElementId = "INVITE_TEXT_FIELD";
			mInviteField.mLength = 32;
			mInviteField.mStyleId = "text_field_1";
			mLabel = GuiSystem.GetLocaleText("Clan_Invite_Label_Text");
			mNick = GuiSystem.GetLocaleText("Clan_Player_Nick_Text");
		}

		public override void Uninit()
		{
			if (mInviteField != null)
			{
				mInviteField.Uninit();
			}
		}

		public override void SetSize()
		{
			mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
			mLabelRect = new Rect(322f, 168f, 192f, 18f);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			mNickRect = new Rect(311f, 202f, 225f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mNickRect);
			mCloseButton.mZoneRect = new Rect(515f, 164f, 26f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
			mInviteField.mZoneRect = new Rect(344f, 225f, 162f, 24f);
			GuiSystem.SetChildRect(mZoneRect, ref mInviteField.mZoneRect);
			mInviteButton.mZoneRect = new Rect(344f, 261f, 160f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mInviteButton.mZoneRect);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mLabel, mLabelRect, "label");
			GuiSystem.DrawString(mNick, mNickRect, "middle_center");
			mCloseButton.RenderElement();
			mInviteButton.RenderElement();
		}

		public override void OnInput()
		{
			mInviteField.OnInput();
		}

		public override void CheckEvent(Event _curEvent)
		{
			mInviteField.CheckEvent(_curEvent);
			mCloseButton.CheckEvent(_curEvent);
			mInviteButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
			{
				SetActive(_active: false);
			}
			else if (_sender.mElementId == "INVITE_BUTTON" && _buttonId == 0)
			{
				if (mOnInvite != null)
				{
					mOnInvite(mInviteField.mData);
				}
				SetActive(_active: false);
			}
		}

		public void Open()
		{
			mInviteField.mData = string.Empty;
			SetActive(_active: true);
		}
	}

	private class ClanMember : GuiElement
	{
		public delegate void OnRemoveMember(int _id);

		public delegate void OnChangeRole(int _id, int _newRole);

		public Action<int> mOnPlayerInfo;

		public GuiButton mRemoveMemberButton;

		private Clan.ClanMember mPlayerData;

		private Clan.ClanMember mSelfData;

		private Texture2D mFrame;

		private string mClanTag;

		private string mNumber;

		private GuiButton mName;

		private string mLocation;

		private string mRole;

		private Rect mNameRect;

		private Rect mLocationRect;

		private Rect mNumberRect;

		private Rect mRoleRect;

		private YesNoDialog mYesNoDialog;

		private Texture2D[] mBoxImages;

		private Texture2D mBoxZoneImage;

		private ComboBox mRoleBox;

		private Dictionary<Clan.Role, string> mRoleNames;

		public OnRemoveMember mOnRemoveMember;

		public OnChangeRole mOnChangeRole;

		public override void Init()
		{
			mRole = string.Empty;
			mLocation = string.Empty;
			mNumber = string.Empty;
			mClanTag = string.Empty;
			mFrame = GuiSystem.GetImage("Gui/ClanMenu/frame5");
			mRemoveMemberButton = GuiSystem.CreateButton("Gui/misc/button_13_norm", "Gui/misc/button_13_over", "Gui/misc/button_13_press", string.Empty, string.Empty);
			mRemoveMemberButton.mElementId = "REMOVE_MEMBER";
			GuiButton guiButton = mRemoveMemberButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mRemoveMemberButton.Init();
			mBoxImages = new Texture2D[3];
			mBoxImages[0] = GuiSystem.GetImage("Gui/misc/button_14_norm");
			mBoxImages[1] = GuiSystem.GetImage("Gui/misc/button_14_over");
			mBoxImages[2] = GuiSystem.GetImage("Gui/misc/button_14_press");
			mBoxZoneImage = GuiSystem.GetImage("Gui/misc/popup_frame1");
			mName = new GuiButton();
			mName.mElementId = "CLAN_BUTTON";
			mName.mId = 0;
			mName.mLabel = string.Empty;
			mName.mOverColor = Color.yellow;
			GuiButton guiButton2 = mName;
			guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnNameButton));
			mName.mLabelStyle = "middle_left";
			mName.Init();
			mRoleNames = new Dictionary<Clan.Role, string>();
			foreach (int value in Enum.GetValues(typeof(Clan.Role)))
			{
				Dictionary<Clan.Role, string> dictionary = mRoleNames;
				int num = value;
				dictionary.Add((Clan.Role)value, GuiSystem.GetLocaleText("Clan_Role_" + num + "_Text"));
			}
		}

		public override void SetSize()
		{
			mNumberRect = new Rect(3f, 2f, 28f, 16f);
			mNameRect = new Rect(40f, 1f, 255f, 16f);
			mLocationRect = new Rect(502f, 1f, 265f, 16f);
			mRemoveMemberButton.mZoneRect = new Rect(749f, 0f, 23f, 24f);
			mRoleRect = new Rect(302f, 3f, 195f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mRoleRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNumberRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mLocationRect);
			GuiSystem.SetChildRect(mZoneRect, ref mRemoveMemberButton.mZoneRect);
			mName.mZoneRect = mNameRect;
			SetRoleBoxSize();
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mNumber, mNumberRect, "middle_center");
			GuiSystem.DrawString(mLocation, mLocationRect, "middle_left");
			if (mRole.Length > 0)
			{
				GuiSystem.DrawString(mRole, mRoleRect, "middle_left");
			}
			if (mRemoveMemberButton.Active)
			{
				mRemoveMemberButton.RenderElement();
			}
			if (mRoleBox != null)
			{
				mRoleBox.RenderElement();
			}
			if (mName != null)
			{
				mName.RenderElement();
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mRemoveMemberButton.Active)
			{
				mRemoveMemberButton.CheckEvent(_curEvent);
			}
			if (mRoleBox != null)
			{
				mRoleBox.CheckEvent(_curEvent);
			}
			if (mName != null)
			{
				mName.CheckEvent(_curEvent);
			}
		}

		private void SetRoleBoxSize()
		{
			if (mRoleBox != null)
			{
				mRoleBox.mZoneRect = mRoleRect;
				mRoleBox.SetSize();
			}
		}

		public void SetData(int _number, Clan.ClanMember _selfData, Clan.ClanMember _playerData, string _clanTag, YesNoDialog _yesNoDialog)
		{
			mSelfData = _selfData;
			mPlayerData = _playerData;
			mClanTag = "[" + _clanTag + "]";
			mName.mLabel = mClanTag + mPlayerData.Name;
			mName.mId = _playerData.Id;
			mNumber = _number.ToString();
			mYesNoDialog = _yesNoDialog;
			if (_playerData.Location.Length > 0)
			{
				mLocation = GuiSystem.GetLocaleText(mPlayerData.Location);
			}
			else
			{
				mLocation = GuiSystem.GetLocaleText("Offline_Text");
			}
			mRemoveMemberButton.SetActive(GetCanRemove(mSelfData, mPlayerData));
			List<Clan.Role> availableChangeRoles = GetAvailableChangeRoles(mSelfData, mPlayerData);
			if (availableChangeRoles.Count > 0)
			{
				List<string> list = new List<string>();
				foreach (Clan.Role item in availableChangeRoles)
				{
					list.Add(mRoleNames[item]);
				}
				mRoleBox = new ComboBox(list, mBoxImages, mBoxZoneImage, null);
				ComboBox comboBox = mRoleBox;
				comboBox.mOnMouseDown = (OnMouseDown)Delegate.Combine(comboBox.mOnMouseDown, new OnMouseDown(OnSelectRole));
				mRoleBox.mRenderFrame = false;
				mRoleBox.mFontStyle = "middle_left";
				mRoleBox.mSelectionFrame = GuiSystem.GetImage("Gui/misc/selection");
				mRoleBox.SetSelectedData((int)(mPlayerData.Role - 1));
				mRoleBox.Init();
				SetRoleBoxSize();
			}
			else
			{
				mRole = GuiSystem.GetLocaleText("Clan_Role_" + (int)_playerData.Role + "_Text");
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "REMOVE_MEMBER" && _buttonId == 0 && mYesNoDialog != null)
			{
				string empty = string.Empty;
				if (mSelfData == mPlayerData)
				{
					empty = GuiSystem.GetLocaleText("Clan_Member_Self_Remove_Question_Text");
				}
				else
				{
					empty = GuiSystem.GetLocaleText("Clan_Member_Remove_Question_Text");
					empty = empty.Replace("[NICK]", mName.mLabel);
				}
				mYesNoDialog.SetData(empty, "Ok_Button_Name", "Cancel_Button_Name", OnRemoveButton);
			}
		}

		private void OnNameButton(GuiElement _sender, int _buttonId)
		{
			UserLog.AddAction(UserActionType.HERO_INFO, _sender.mId, (mPlayerData == null) ? string.Empty : mPlayerData.Name);
			if (mOnPlayerInfo != null)
			{
				mOnPlayerInfo(_sender.mId);
			}
		}

		private void OnRemoveButton(bool _yes)
		{
			if (_yes && mOnRemoveMember != null && mPlayerData != null)
			{
				mOnRemoveMember(mPlayerData.Id);
			}
		}

		private void OnSelectRole(GuiElement _sender, int _num)
		{
			if (mOnChangeRole != null && mPlayerData != null)
			{
				mOnChangeRole(mPlayerData.Id, _sender.mId + 1);
			}
		}
	}

	public Action<int> mOnPlayerInfo;

	private Texture2D mFrame;

	private string mLabel;

	private Rect mLabelRect;

	private GuiButton mCloseButton;

	private GuiButton mInviteButton;

	private GuiButton mRemoveClanButton;

	private GuiButton mSaveChangesButton;

	private Clan mClanData;

	private IClanInfo mClanInfoData;

	private YesNoDialog mYesNoDialog;

	private InvitePlayerMenu mInvitePlayerMenu;

	private string mLevelText;

	private string mRaitingText;

	private string mLevel;

	private string mRaiting;

	private string mMemberNumber;

	private string mMemberName;

	private string mMemberPost;

	private string mMemberLocation;

	private Rect mLevelTextRect;

	private Rect mRaitingTextRect;

	private Rect mLevelRect;

	private Rect mRaitingRect;

	private Rect mMemberNumberRect;

	private Rect mMemberNameRect;

	private Rect mMemberPostRect;

	private Rect mMemberLocationRect;

	private VerticalScrollbar mScrollbar;

	private float mScrollOffset;

	private float mStartScrollOffset;

	private Rect mDrawRect;

	private List<ClanMember> mClanMembers;

	private Dictionary<int, Clan.Role> mRoleChanges;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(510, this);
		mFrame = GuiSystem.GetImage("Gui/ClanMenu/frame2");
		mStartScrollOffset = 202f;
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mInviteButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mInviteButton.mElementId = "INVITE_BUTTON";
		GuiButton guiButton2 = mInviteButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mInviteButton.mLabel = GuiSystem.GetLocaleText("Clan_Invite_Text");
		mInviteButton.Init();
		mRemoveClanButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mRemoveClanButton.mElementId = "REMOVE_BUTTON";
		GuiButton guiButton3 = mRemoveClanButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mRemoveClanButton.mLabel = GuiSystem.GetLocaleText("Clan_Remove_Text");
		mRemoveClanButton.Init();
		mSaveChangesButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mSaveChangesButton.mElementId = "SAVE_CHANGES_BUTTON";
		GuiButton guiButton4 = mSaveChangesButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mSaveChangesButton.mLabel = GuiSystem.GetLocaleText("Clan_Save_Changes_Text");
		mSaveChangesButton.Init();
		mInvitePlayerMenu = new InvitePlayerMenu();
		mInvitePlayerMenu.Init();
		InvitePlayerMenu invitePlayerMenu = mInvitePlayerMenu;
		invitePlayerMenu.mOnInvite = (InvitePlayerMenu.OnInvite)Delegate.Combine(invitePlayerMenu.mOnInvite, new InvitePlayerMenu.OnInvite(OnInvitePlayer));
		mInvitePlayerMenu.SetActive(_active: false);
		mLevelText = GuiSystem.GetLocaleText("Clan_Level_Text");
		mRaitingText = GuiSystem.GetLocaleText("Clan_Raiting_Text");
		mMemberNumber = GuiSystem.GetLocaleText("Clan_Member_Number_Text");
		mMemberName = GuiSystem.GetLocaleText("Clan_Member_Name_Text");
		mMemberPost = GuiSystem.GetLocaleText("Clan_Member_Post_Text");
		mMemberLocation = GuiSystem.GetLocaleText("Clan_Member_Location_Text");
		mScrollbar = new VerticalScrollbar();
		mScrollbar.Init();
		VerticalScrollbar verticalScrollbar = mScrollbar;
		verticalScrollbar.mOnChangeVal = (VerticalScrollbar.OnChangeVal)Delegate.Combine(verticalScrollbar.mOnChangeVal, new VerticalScrollbar.OnChangeVal(OnScrollbar));
		mClanMembers = new List<ClanMember>();
		mRoleChanges = new Dictionary<int, Clan.Role>();
	}

	public override void Uninit()
	{
		if (mInvitePlayerMenu != null)
		{
			mInvitePlayerMenu.Uninit();
		}
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mLabelRect = new Rect(36f, 11f, 790f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
		mCloseButton.mZoneRect = new Rect(818f, 8f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mInviteButton.mZoneRect = new Rect(23f, 422f, 180f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mInviteButton.mZoneRect);
		mRemoveClanButton.mZoneRect = new Rect(661f, 422f, 180f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mRemoveClanButton.mZoneRect);
		mSaveChangesButton.mZoneRect = new Rect(345f, 366f, 180f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mSaveChangesButton.mZoneRect);
		mLevelTextRect = new Rect(342f, 48f, 120f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelTextRect);
		mRaitingTextRect = new Rect(342f, 66f, 120f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mRaitingTextRect);
		mLevelRect = new Rect(467f, 48f, 44f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
		mRaitingRect = new Rect(467f, 66f, 44f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mRaitingRect);
		mMemberNumberRect = new Rect(37f, 110f, 33f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mMemberNumberRect);
		mMemberNameRect = new Rect(72f, 110f, 260f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mMemberNameRect);
		mMemberPostRect = new Rect(334f, 110f, 198f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mMemberPostRect);
		mMemberLocationRect = new Rect(534f, 110f, 293f, 18f);
		GuiSystem.SetChildRect(mZoneRect, ref mMemberLocationRect);
		mScrollbar.mZoneRect = new Rect(802f, 139f, 22f, 198f);
		GuiSystem.SetChildRect(mZoneRect, ref mScrollbar.mZoneRect);
		mScrollbar.SetSize();
		mDrawRect = new Rect(30f, 138f, 773f, 202f);
		GuiSystem.SetChildRect(mZoneRect, ref mDrawRect);
		mInvitePlayerMenu.SetSize();
		SetMembersSize();
	}

	public override void OnInput()
	{
		if (mInvitePlayerMenu.Active)
		{
			mInvitePlayerMenu.OnInput();
		}
		else
		{
			mScrollbar.OnInput();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mInvitePlayerMenu.Active)
		{
			mInvitePlayerMenu.CheckEvent(_curEvent);
		}
		if (mClanMembers.Count > 0)
		{
			for (int i = 0; i < mClanMembers.Count; i++)
			{
				if (mClanMembers[i].Active)
				{
					mClanMembers[i].CheckEvent(_curEvent);
				}
			}
		}
		mCloseButton.CheckEvent(_curEvent);
		mRemoveClanButton.CheckEvent(_curEvent);
		mInviteButton.CheckEvent(_curEvent);
		mSaveChangesButton.CheckEvent(_curEvent);
		mScrollbar.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GuiSystem.DrawString(mLabel, mLabelRect, "label");
		mCloseButton.RenderElement();
		mRemoveClanButton.RenderElement();
		mInviteButton.RenderElement();
		mSaveChangesButton.RenderElement();
		mScrollbar.RenderElement();
		if (mClanMembers.Count > 0)
		{
			for (int num = mClanMembers.Count - 1; num >= 0; num--)
			{
				if (mClanMembers[num].Active)
				{
					mClanMembers[num].RenderElement();
				}
			}
		}
		if (mInvitePlayerMenu.Active)
		{
			mInvitePlayerMenu.RenderElement();
		}
		GuiSystem.DrawString(mLevelText, mLevelTextRect, "middle_right");
		GuiSystem.DrawString(mRaitingText, mRaitingTextRect, "middle_right");
		GuiSystem.DrawString(mLevel, mLevelRect, "middle_left");
		GuiSystem.DrawString(mRaiting, mRaitingRect, "middle_left");
		GuiSystem.DrawString(mMemberNumber, mMemberNumberRect, "label");
		GuiSystem.DrawString(mMemberName, mMemberNameRect, "label_left");
		GuiSystem.DrawString(mMemberPost, mMemberPostRect, "label_left");
		GuiSystem.DrawString(mMemberLocation, mMemberLocationRect, "label_left");
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void Open()
	{
		SetActive(_active: true);
		InitClanData();
	}

	public void SetData(Clan _clanData, YesNoDialog _yesNoDialog)
	{
		if (_clanData != null)
		{
			mClanData = _clanData;
			mClanInfoData = _clanData;
			mYesNoDialog = _yesNoDialog;
		}
	}

	public void SetData(Clan.ClanInfo _clanInfoData, YesNoDialog _yesNoDialog)
	{
		if (_clanInfoData != null)
		{
			mClanData = null;
			mClanInfoData = _clanInfoData;
			mYesNoDialog = _yesNoDialog;
		}
	}

	public void Clean()
	{
		mClanData = null;
		mYesNoDialog = null;
		mClanInfoData = null;
	}

	public static bool GetCanRemove(Clan.ClanMember _self, Clan.ClanMember _player)
	{
		if (_self == null || _player == null)
		{
			return false;
		}
		if (_self == _player)
		{
			return true;
		}
		return _self.Role > _player.Role;
	}

	public static bool GetCanChangeRole(Clan.ClanMember _self, Clan.ClanMember _player)
	{
		if (_self == null || _player == null)
		{
			return false;
		}
		if (_self == _player)
		{
			return _self.Role == Clan.Role.HEAD;
		}
		return _self.Role > _player.Role && _self.Role >= Clan.Role.DEPUTY;
	}

	public static List<Clan.Role> GetAvailableChangeRoles(Clan.ClanMember _self, Clan.ClanMember _player)
	{
		List<Clan.Role> list = new List<Clan.Role>();
		if (!GetCanChangeRole(_self, _player))
		{
			return list;
		}
		foreach (int value in Enum.GetValues(typeof(Clan.Role)))
		{
			if (value < (int)_self.Role || (value == (int)_self.Role && _self.Role == Clan.Role.HEAD))
			{
				list.Add((Clan.Role)value);
			}
		}
		return list;
	}

	private void InitClanData()
	{
		if (mClanInfoData == null)
		{
			return;
		}
		mScrollOffset = 0f;
		mScrollbar.Refresh();
		mClanMembers.Clear();
		mLabel = "[" + mClanInfoData.Tag + "]" + mClanInfoData.ClanName;
		mLevel = mClanInfoData.Level.ToString();
		mRaiting = mClanInfoData.Rating.ToString();
		ClanMember clanMember = null;
		Clan.ClanMember clanMember2 = null;
		if (mClanData != null)
		{
			clanMember2 = mClanData.SelfMember;
		}
		ClanMemberComparer comparer = new ClanMemberComparer();
		List<Clan.ClanMember> list = new List<Clan.ClanMember>();
		foreach (Clan.ClanMember member in mClanInfoData.Members)
		{
			list.Add(member);
		}
		list.Sort(comparer);
		foreach (Clan.ClanMember item in list)
		{
			clanMember = new ClanMember();
			clanMember.Init();
			clanMember.SetData(mClanMembers.Count + 1, clanMember2, item, mClanInfoData.Tag, mYesNoDialog);
			ClanMember clanMember3 = clanMember;
			clanMember3.mOnRemoveMember = (ClanMember.OnRemoveMember)Delegate.Combine(clanMember3.mOnRemoveMember, new ClanMember.OnRemoveMember(OnRemoveMember));
			ClanMember clanMember4 = clanMember;
			clanMember4.mOnChangeRole = (ClanMember.OnChangeRole)Delegate.Combine(clanMember4.mOnChangeRole, new ClanMember.OnChangeRole(OnChangeRole));
			ClanMember clanMember5 = clanMember;
			clanMember5.mOnPlayerInfo = (Action<int>)Delegate.Combine(clanMember5.mOnPlayerInfo, new Action<int>(GetPlayerInfo));
			mClanMembers.Add(clanMember);
		}
		mSaveChangesButton.mLocked = clanMember2 == null || clanMember2.Role < Clan.Role.DEPUTY;
		mRemoveClanButton.mLocked = clanMember2 == null || clanMember2.Role != Clan.Role.HEAD;
		mInviteButton.mLocked = clanMember2 == null || clanMember2.Role == Clan.Role.WARRIOR;
		SetMembersSize();
	}

	private void GetPlayerInfo(int _playerId)
	{
		if (mOnPlayerInfo != null)
		{
			mOnPlayerInfo(_playerId);
		}
	}

	private void OnScrollbar(GuiElement _sender, float _offset)
	{
		if (_offset != mScrollOffset)
		{
			mScrollOffset = _offset;
			SetMembersSize();
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "INVITE_BUTTON" && _buttonId == 0)
		{
			mInvitePlayerMenu.Open();
		}
		else if (_sender.mElementId == "REMOVE_BUTTON" && _buttonId == 0)
		{
			if (mYesNoDialog != null)
			{
				mYesNoDialog.SetData(GuiSystem.GetLocaleText("Clan_Remove_Question_Text"), "Ok_Button_Name", "Cancel_Button_Name", OnRemoveClan);
			}
		}
		else if (_sender.mElementId == "SAVE_CHANGES_BUTTON" && _buttonId == 0 && mClanData != null && mRoleChanges.Count > 0)
		{
			mClanData.ChangeRole(mRoleChanges);
			mRoleChanges.Clear();
		}
	}

	private void OnRemoveMember(int _id)
	{
		if (mClanData != null)
		{
			mClanData.RemoveUser(_id);
		}
	}

	private void OnChangeRole(int _id, int _newRole)
	{
		if (mRoleChanges.ContainsKey(_id))
		{
			mRoleChanges[_id] = (Clan.Role)_newRole;
		}
		else
		{
			mRoleChanges.Add(_id, (Clan.Role)_newRole);
		}
	}

	private void OnRemoveClan(bool _yes)
	{
		if (_yes && mClanData != null)
		{
			mClanData.RemoveClan();
		}
	}

	private void OnInvitePlayer(string _name)
	{
		if (mClanData != null)
		{
			mClanData.InviteRequest(_name);
		}
	}

	private void SetMembersSize()
	{
		int num = 30;
		int num2 = 138;
		int num3 = 0;
		int num4 = 0;
		foreach (ClanMember mClanMember in mClanMembers)
		{
			mClanMember.mZoneRect = new Rect(num, (float)(num2 + num4) - mScrollOffset, 772f, 24f);
			GuiSystem.SetChildRect(mZoneRect, ref mClanMember.mZoneRect);
			mClanMember.SetSize();
			mClanMember.SetActive(!(mClanMember.mZoneRect.y + 1f < mDrawRect.y) && !(mClanMember.mZoneRect.y + mClanMember.mZoneRect.height > mDrawRect.y + mDrawRect.height + 1f));
			num4 += 24;
			num3++;
		}
		mScrollbar.SetData(mStartScrollOffset, num4);
	}
}
