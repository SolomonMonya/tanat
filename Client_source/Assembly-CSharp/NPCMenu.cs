using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class NPCMenu : GuiElement, EscapeListener
{
	private class Quest
	{
		public GuiButton mQuestButton;

		public Texture2D mQuestStatusImage;

		public string mQuestName;

		public Rect mQuestStatusImageRect;

		public Rect mQuestNameRect;
	}

	private class NPC : GuiElement
	{
		public GuiButton mNPCButton;

		private INpc mNPCData;

		private IStoreContentProvider<ISelfQuest> mSelfQuestData;

		private QuestStatus mQuestStatus;

		private Texture2D mIcon;

		private Texture2D mStatusIcon;

		private string mName;

		private Rect mIconRect;

		private Rect mStatusIconRect;

		private Rect mNameRect;

		public NPC(INpc _data, IStoreContentProvider<ISelfQuest> _selfQuestData)
		{
			mNPCData = _data;
			mSelfQuestData = _selfQuestData;
		}

		public override void Init()
		{
			mName = GuiSystem.GetLocaleText(mNPCData.Name);
			mIcon = GuiSystem.GetImage("Gui/NPCMenu/Icons/" + mNPCData.Icon);
			mNPCButton = GuiSystem.CreateButton("Gui/NPCMenu/button_1_norm", "Gui/NPCMenu/button_1_over", "Gui/NPCMenu/button_1_press", string.Empty, string.Empty);
			mNPCButton.mElementId = "NPC_BUTTON";
			mNPCButton.mId = mId;
			mNPCButton.Init();
			mQuestStatus = GetNPCQuestStatus(mNPCData, mSelfQuestData);
			mStatusIcon = GetStatusImage(mQuestStatus);
		}

		public override void SetSize()
		{
			mNPCButton.mZoneRect = mZoneRect;
			mIconRect = new Rect(9f, 6f, 52f, 52f);
			mStatusIconRect = new Rect(258f, 24f, 11f, 18f);
			mNameRect = new Rect(65f, 19f, 180f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mIconRect);
			GuiSystem.SetChildRect(mZoneRect, ref mStatusIconRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mIcon, mIconRect);
			mNPCButton.RenderElement();
			GuiSystem.DrawImage(mStatusIcon, mStatusIconRect);
			GuiSystem.DrawString(mName, mNameRect, "middle_center");
		}

		public override void CheckEvent(Event _curEvent)
		{
			mNPCButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public void SetSelected(bool _selected)
		{
			mNPCButton.Pressed = _selected;
		}
	}

	public delegate void ShowQuestCallback(int _npcId, int _questId);

	public delegate void ShowQuestJournalCallback(int _questId);

	public ShowQuestCallback mShowQuestCallback;

	public ShowQuestJournalCallback mShowQuestJournalCallback;

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private GuiButton mQuestJournalButton;

	private SortedDictionary<int, NPC> mNPCs;

	private IStoreContentProvider<INpc> mNPCData;

	private IStoreContentProvider<IQuest> mQuestData;

	private IStoreContentProvider<ISelfQuest> mSelfQuestData;

	public int mSelectedNPC = -1;

	private string mNPCName;

	private string mNPCDesc;

	private Texture mNPCIcon;

	private Rect mNPCNameRect;

	private Rect mNPCDescRect;

	private Rect mNPCIconRect;

	private List<Quest> mQuests;

	private List<Rect> mQuestsRect;

	private Rect mPagesRect;

	private Rect mQuestStatusImageRect;

	private Rect mQuestNameRect;

	private int mMaxQuestsOnPage;

	private int mCurPage;

	private int mMaxPages;

	private string mPagesStr;

	private GuiButton mPageLeftButton;

	private GuiButton mPageRightButton;

	public static QuestStatus GetNPCQuestStatus(INpc _npcData, IStoreContentProvider<ISelfQuest> _selfQuestData)
	{
		if (_selfQuestData == null || _npcData == null)
		{
			return QuestStatus.NONE;
		}
		List<QuestStatus> list = new List<QuestStatus>();
		ISelfQuest selfQuest = null;
		int[] quests = _npcData.Quests;
		foreach (int id in quests)
		{
			selfQuest = _selfQuestData.TryGet(id);
			if (selfQuest != null)
			{
				if (selfQuest.Status == QuestStatus.DONE)
				{
					list.Add(QuestStatus.DONE);
				}
				else if (selfQuest.Status == QuestStatus.IN_PROGRESS)
				{
					list.Add(QuestStatus.IN_PROGRESS);
				}
				else if (selfQuest.Status == QuestStatus.WAIT_COOLDOWN)
				{
					list.Add(QuestStatus.EXIST);
				}
			}
			else
			{
				list.Add(QuestStatus.EXIST);
			}
		}
		if (list.Contains(QuestStatus.DONE))
		{
			return QuestStatus.DONE;
		}
		if (list.Contains(QuestStatus.IN_PROGRESS) && !list.Contains(QuestStatus.EXIST))
		{
			return QuestStatus.IN_PROGRESS;
		}
		if (list.Contains(QuestStatus.EXIST))
		{
			return QuestStatus.EXIST;
		}
		return QuestStatus.NONE;
	}

	public static Texture2D GetStatusImage(QuestStatus _status)
	{
		string text = "Gui/NPCMenu/status";
		switch (_status)
		{
		case QuestStatus.WAIT_COOLDOWN:
		case QuestStatus.EXIST:
			text += "1";
			break;
		case QuestStatus.IN_PROGRESS:
			text += "3";
			break;
		case QuestStatus.DONE:
			text += "2";
			break;
		default:
			return null;
		}
		return GuiSystem.GetImage(text);
	}

	public bool OnEscapeAction()
	{
		if (base.Active)
		{
			SetActive(_active: false);
			return true;
		}
		return false;
	}

	public override void Init()
	{
		GuiSystem.mGuiInputMgr.AddEscapeListener(350, this);
		mNPCs = new SortedDictionary<int, NPC>();
		mQuests = new List<Quest>();
		mQuestsRect = new List<Rect>();
		mMaxQuestsOnPage = 5;
		mFrame = GuiSystem.GetImage("Gui/NPCMenu/frame");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton);
		mQuestJournalButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mQuestJournalButton.mElementId = "QUEST_JOURNAL_BUTTON";
		GuiButton guiButton2 = mQuestJournalButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mQuestJournalButton.mLabel = GuiSystem.GetLocaleText("Quest_Journal_Text");
		mQuestJournalButton.RegisterAction(UserActionType.QUEST_JOURNAL_BUTTON);
		mQuestJournalButton.Init();
		mPageLeftButton = GuiSystem.CreateButton("Gui/misc/button_6_norm", "Gui/misc/button_6_over", "Gui/misc/button_6_press", string.Empty, string.Empty);
		mPageLeftButton.mElementId = "PAGE_LEFT_BUTTON";
		GuiButton guiButton3 = mPageLeftButton;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mPageLeftButton.Init();
		mPageRightButton = GuiSystem.CreateButton("Gui/misc/button_7_norm", "Gui/misc/button_7_over", "Gui/misc/button_7_press", string.Empty, string.Empty);
		mPageRightButton.mElementId = "PAGE_RIGHT_BUTTON";
		GuiButton guiButton4 = mPageRightButton;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mPageRightButton.Init();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 200f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(654f, 6f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mQuestJournalButton.mZoneRect = new Rect(262f, 471f, 165f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mQuestJournalButton.mZoneRect);
		mNPCNameRect = new Rect(396f, 55f, 243f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mNPCNameRect);
		mNPCDescRect = new Rect(354f, 100f, 283f, 114f);
		GuiSystem.SetChildRect(mZoneRect, ref mNPCDescRect);
		mNPCIconRect = new Rect(337f, 42f, 52f, 52f);
		GuiSystem.SetChildRect(mZoneRect, ref mNPCIconRect);
		mPagesRect = new Rect(464f, 423f, 70f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mPagesRect);
		mPageLeftButton.mZoneRect = new Rect(322f, 305f, 28f, 54f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageLeftButton.mZoneRect);
		mPageRightButton.mZoneRect = new Rect(643f, 305f, 28f, 54f);
		GuiSystem.SetChildRect(mZoneRect, ref mPageRightButton.mZoneRect);
		mQuestsRect.Clear();
		for (int i = 0; i < mMaxQuestsOnPage; i++)
		{
			Rect _rect = new Rect(352f, 259 + i * 31, 288f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref _rect);
			mQuestsRect.Add(_rect);
		}
		mQuestStatusImageRect = new Rect(8f, 5f, 11f, 18f);
		mQuestNameRect = new Rect(28f, 4f, 255f, 18f);
		GuiSystem.GetRectScaled(ref mQuestStatusImageRect);
		GuiSystem.GetRectScaled(ref mQuestNameRect);
		SetNPCsSize();
		SetNPCQuestsSize();
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		foreach (KeyValuePair<int, NPC> mNPC in mNPCs)
		{
			mNPC.Value.RenderElement();
		}
		for (int i = mCurPage * mMaxQuestsOnPage; i < (mCurPage + 1) * mMaxQuestsOnPage && mQuests.Count > i; i++)
		{
			mQuests[i].mQuestButton.RenderElement();
			GuiSystem.DrawImage(mQuests[i].mQuestStatusImage, mQuests[i].mQuestStatusImageRect);
			GuiSystem.DrawString(mQuests[i].mQuestName, mQuests[i].mQuestNameRect, "middle_center");
		}
		if (mSelectedNPC != -1)
		{
			GuiSystem.DrawImage(mNPCIcon, mNPCIconRect);
			GuiSystem.DrawString(mNPCName, mNPCNameRect, "middle_center");
			GuiSystem.DrawString(mNPCDesc, mNPCDescRect, "upper_left");
			GuiSystem.DrawString(mPagesStr, mPagesRect, "middle_center");
		}
		mCloseButton.RenderElement();
		mQuestJournalButton.RenderElement();
		mPageLeftButton.RenderElement();
		mPageRightButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (KeyValuePair<int, NPC> mNPC in mNPCs)
		{
			mNPC.Value.CheckEvent(_curEvent);
		}
		for (int i = mCurPage * mMaxQuestsOnPage; i < (mCurPage + 1) * mMaxQuestsOnPage && mQuests.Count > i; i++)
		{
			mQuests[i].mQuestButton.CheckEvent(_curEvent);
		}
		mCloseButton.CheckEvent(_curEvent);
		mQuestJournalButton.CheckEvent(_curEvent);
		mPageLeftButton.CheckEvent(_curEvent);
		mPageRightButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public override void SetActive(bool _active)
	{
		if (_active)
		{
			InitNPCs();
			SetNPCsSize();
			SetSelectedNPC();
		}
		base.SetActive(_active);
	}

	private void InitNPCs()
	{
		mNPCs.Clear();
		if (mNPCData == null)
		{
			return;
		}
		NPC nPC = null;
		bool flag = mSelectedNPC == -1;
		foreach (INpc item in mNPCData.Content)
		{
			if (item.NeedShow)
			{
				if ((flag && mSelectedNPC == -1) || (flag && item.Id < mSelectedNPC))
				{
					mSelectedNPC = item.Id;
				}
				nPC = new NPC(item, mSelfQuestData);
				nPC.mId = item.Id;
				nPC.Init();
				GuiButton mNPCButton = nPC.mNPCButton;
				mNPCButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mNPCButton.mOnMouseUp, new OnMouseUp(OnButton));
				mNPCs.Add(item.Id, nPC);
			}
		}
	}

	private void InitNPCQuest()
	{
		INpc npc = mNPCData.TryGet(mSelectedNPC);
		if (npc == null)
		{
			return;
		}
		mQuests.Clear();
		Quest quest = null;
		ISelfQuest selfQuest = null;
		IQuest quest2 = null;
		int[] quests = npc.Quests;
		foreach (int num in quests)
		{
			selfQuest = mSelfQuestData.TryGet(num);
			quest2 = mQuestData.TryGet(num);
			quest = new Quest();
			if (quest2 != null)
			{
				quest.mQuestName = GuiSystem.GetLocaleText(quest2.Name);
			}
			quest.mQuestStatusImage = GetStatusImage(selfQuest?.Status ?? QuestStatus.EXIST);
			quest.mQuestButton = GuiSystem.CreateButton("Gui/NPCMenu/button_2_norm", "Gui/NPCMenu/button_2_over", "Gui/NPCMenu/button_2_press", string.Empty, string.Empty);
			quest.mQuestButton.mElementId = "SELECT_QUEST_BUTTON";
			quest.mQuestButton.mId = num;
			GuiButton mQuestButton = quest.mQuestButton;
			mQuestButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mQuestButton.mOnMouseUp, new OnMouseUp(OnButton));
			quest.mQuestButton.Init();
			mQuests.Add(quest);
		}
		mMaxPages = Mathf.CeilToInt(mQuests.Count / (mMaxQuestsOnPage + 1));
		SetCurPage(0);
	}

	private void SetNPCsSize()
	{
		int num = 0;
		float num2 = 71f;
		foreach (KeyValuePair<int, NPC> mNPC in mNPCs)
		{
			mNPC.Value.mZoneRect = new Rect(23f, 44f + (float)num * num2, 280f, 66f);
			GuiSystem.SetChildRect(mZoneRect, ref mNPC.Value.mZoneRect);
			mNPC.Value.SetSize();
			num++;
		}
	}

	private void SetNPCQuestsSize()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < mQuests.Count; i++)
		{
			num = Mathf.FloorToInt((float)i / (float)mMaxQuestsOnPage);
			num2 = i - num * mMaxQuestsOnPage;
			mQuests[i].mQuestButton.mZoneRect = mQuestsRect[num2];
			mQuests[i].mQuestStatusImageRect = mQuestStatusImageRect;
			mQuests[i].mQuestNameRect = mQuestNameRect;
			mQuests[i].mQuestStatusImageRect.x += mQuests[i].mQuestButton.mZoneRect.x;
			mQuests[i].mQuestStatusImageRect.y += mQuests[i].mQuestButton.mZoneRect.y;
			mQuests[i].mQuestNameRect.x += mQuests[i].mQuestButton.mZoneRect.x;
			mQuests[i].mQuestNameRect.y += mQuests[i].mQuestButton.mZoneRect.y;
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
		{
			SetActive(_active: false);
		}
		else if (_sender.mElementId == "NPC_BUTTON" && _buttonId == 0)
		{
			mSelectedNPC = _sender.mId;
			SetSelectedNPC();
		}
		else if (_sender.mElementId == "SELECT_QUEST_BUTTON" && _buttonId == 0)
		{
			SetActive(_active: false);
			if (mShowQuestCallback != null)
			{
				mShowQuestCallback(mSelectedNPC, _sender.mId);
			}
		}
		else if (_sender.mElementId == "PAGE_LEFT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPage - 1);
		}
		else if (_sender.mElementId == "PAGE_RIGHT_BUTTON" && _buttonId == 0)
		{
			SetCurPage(mCurPage + 1);
		}
		else if (_sender.mElementId == "QUEST_JOURNAL_BUTTON" && _buttonId == 0)
		{
			SetActive(_active: false);
			if (mShowQuestJournalCallback != null)
			{
				mShowQuestJournalCallback(-1);
			}
		}
	}

	private void SetSelectedNPC()
	{
		foreach (KeyValuePair<int, NPC> mNPC in mNPCs)
		{
			mNPC.Value.SetSelected(mNPC.Key == mSelectedNPC);
		}
		if (mNPCData != null)
		{
			INpc npc = mNPCData.Get(mSelectedNPC);
			if (npc != null)
			{
				mNPCIcon = GuiSystem.GetImage("Gui/NPCMenu/Icons/" + npc.Icon);
				mNPCName = GuiSystem.GetLocaleText(npc.Name);
				mNPCDesc = GuiSystem.GetLocaleText(npc.Desc);
			}
			InitNPCQuest();
			SetNPCQuestsSize();
		}
	}

	private NPC GetNPCById(int _id)
	{
		if (mNPCs.ContainsKey(_id))
		{
			return mNPCs[_id];
		}
		return null;
	}

	private void SetCurPage(int _page)
	{
		mCurPage = _page;
		mCurPage = ((mCurPage >= 0) ? mCurPage : 0);
		mCurPage = ((mCurPage <= mMaxPages) ? mCurPage : mMaxPages);
		SetPageStr();
		SetPageButtonState();
	}

	private void SetPageButtonState()
	{
		mPageLeftButton.mLocked = mCurPage == 0;
		mPageRightButton.mLocked = mCurPage == mMaxPages;
	}

	private void SetPageStr()
	{
		mPagesStr = mCurPage + 1 + "/" + (mMaxPages + 1);
	}

	public void SetData(IStoreContentProvider<INpc> _npcs, IStoreContentProvider<IQuest> _quests, IStoreContentProvider<ISelfQuest> _selfQuests)
	{
		mNPCData = _npcs;
		mQuestData = _quests;
		mSelfQuestData = _selfQuests;
	}

	public void Clean()
	{
		mNPCData = null;
		mQuestData = null;
		mSelfQuestData = null;
	}
}
