using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class QuestProgressMenu : GuiElement
{
	private class QuestProgressPlate : GuiElement
	{
		private BattleTimer mTimer;

		private Texture2D mFrame;

		private string mName = string.Empty;

		private string mProgress = string.Empty;

		private string mTimerStr;

		private Rect mTimerRect;

		private Rect mNameRect;

		private Rect mProgressRect;

		private Color mCurColor;

		private Color mNormColor;

		private Color mOverColor;

		private float mEndTime;

		private bool mUpdated;

		private Texture2D mFXImage;

		private int mFrameNum;

		private Rect mFXRect;

		public QuestProgressPlate(string _name, BattleTimer _timer)
		{
			mName = _name;
			mTimer = _timer;
			mUpdated = false;
		}

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/QuestProgressMenu/frame");
			mFXImage = GuiSystem.GetImage("Gui/QuestProgressMenu/progress_fx");
			mNormColor = new Color(181f / 255f, 72f / 85f, 0.882352948f);
			mOverColor = new Color(1f, 226f / 255f, 35f / 51f);
		}

		public override void SetSize()
		{
			mNameRect = new Rect(0f, 0f, mFrame.width, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			mProgressRect = new Rect(0f, 16f, mFrame.width, mFrame.height - 16);
			GuiSystem.SetChildRect(mZoneRect, ref mProgressRect);
			mTimerRect = new Rect(mFrame.width - 60, 0f, 60f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mTimerRect);
		}

		public override void Update()
		{
			float num = mEndTime - mTimer.Time;
			if (num >= 0f)
			{
				int num2 = (int)(num / 60f);
				int num3 = (int)(num - (float)(num2 * 60));
				if (num2 > 0 || num3 > 0)
				{
					mTimerStr = num2.ToString("0#") + ":" + num3.ToString("0#");
				}
				else
				{
					mTimerStr = string.Empty;
				}
			}
			if (mUpdated && mFrameNum < 18)
			{
				mFrameNum++;
				if (mFrameNum == 18)
				{
					mUpdated = false;
					mFrameNum = 0;
				}
				float num4 = 0.055555556f;
				mFXRect = new Rect(0f, (float)mFrameNum * num4, 1f, num4);
			}
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			if (mUpdated)
			{
				GuiSystem.DrawImage(mFXImage, mZoneRect, mFXRect);
			}
			GUI.contentColor = mCurColor;
			GuiSystem.DrawString(mName, mNameRect, "middle_center");
			GuiSystem.DrawString(mProgress, mProgressRect, "middle_left");
			GuiSystem.DrawString(mTimerStr, mTimerRect, "middle_center");
			GUI.contentColor = Color.white;
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mZoneRect.Contains(_curEvent.mousePosition))
			{
				mCurColor = mOverColor;
			}
			else
			{
				mCurColor = mNormColor;
			}
			base.CheckEvent(_curEvent);
		}

		public void AddProgress(string _desc, int _curVal, int _maxVal, float _endTime, bool _showCur, bool _showMax)
		{
			mEndTime = _endTime;
			mProgress += _desc;
			if (_showCur && _showMax)
			{
				string text = mProgress;
				mProgress = text + " " + _curVal + "/" + _maxVal;
			}
			else if (_showCur)
			{
				mProgress = mProgress + " " + _curVal;
			}
			else if (_showMax)
			{
				mProgress = mProgress + " " + _showMax;
			}
			mProgress += "\t";
		}

		public void Updated()
		{
			mUpdated = true;
		}
	}

	public delegate void ShowQuestInfoCallback(int _questId, bool _pvp);

	public ShowQuestInfoCallback mOnShowQuestInfo;

	private IStoreContentProvider<ISelfQuest> mSelfQuests;

	private IStoreContentProvider<ISelfPvpQuest> mSelfPvPQuests;

	private List<QuestProgressPlate> mQuestProgressPlates;

	private BattleTimer mTimer;

	public override void Init()
	{
		mQuestProgressPlates = new List<QuestProgressPlate>();
	}

	public override void SetSize()
	{
		SetProgressPlateSize();
	}

	public override void Update()
	{
		foreach (QuestProgressPlate mQuestProgressPlate in mQuestProgressPlates)
		{
			mQuestProgressPlate.Update();
		}
	}

	public override void RenderElement()
	{
		foreach (QuestProgressPlate mQuestProgressPlate in mQuestProgressPlates)
		{
			mQuestProgressPlate.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (QuestProgressPlate mQuestProgressPlate in mQuestProgressPlates)
		{
			mQuestProgressPlate.CheckEvent(_curEvent);
		}
	}

	public void SetData(IStoreContentProvider<ISelfQuest> _selfQuests, IStoreContentProvider<ISelfPvpQuest> _selfPvPQuests, BattleTimer _timer)
	{
		mSelfQuests = _selfQuests;
		mSelfPvPQuests = _selfPvPQuests;
		mTimer = _timer;
	}

	public void UpdateInfo(IList<int> _updatedQuest)
	{
		mQuestProgressPlates.Clear();
		QuestProgressPlate questProgressPlate = null;
		foreach (ISelfPvpQuest item in mSelfPvPQuests.Content)
		{
			questProgressPlate = new QuestProgressPlate(GuiSystem.GetLocaleText(item.Quest.Name), mTimer);
			questProgressPlate.mId = item.Id;
			questProgressPlate.mElementId = "PVP_QUEST_INFO";
			questProgressPlate.Init();
			QuestProgressPlate questProgressPlate2 = questProgressPlate;
			questProgressPlate2.mOnMouseUp = (OnMouseUp)Delegate.Combine(questProgressPlate2.mOnMouseUp, new OnMouseUp(OnButton));
			questProgressPlate.AddProgress(GuiSystem.GetLocaleText(item.Quest.InProgressDesc), item.State, item.Limit, item.EndTime, item.Quest.ShowCur, item.Quest.ShowMax);
			if (_updatedQuest == null || _updatedQuest.Contains(item.Id))
			{
				questProgressPlate.Updated();
			}
			mQuestProgressPlates.Add(questProgressPlate);
		}
		ISelfQuest selfQuest = null;
		foreach (int mActiveQuest in OptionsMgr.mActiveQuests)
		{
			if (mQuestProgressPlates.Count == 3)
			{
				break;
			}
			selfQuest = mSelfQuests.TryGet(mActiveQuest);
			if (selfQuest == null || (selfQuest.Status != QuestStatus.IN_PROGRESS && selfQuest.Status != QuestStatus.DONE))
			{
				continue;
			}
			questProgressPlate = new QuestProgressPlate(GuiSystem.GetLocaleText(selfQuest.Quest.Name), mTimer);
			questProgressPlate.mId = selfQuest.Id;
			questProgressPlate.mElementId = "PVE_QUEST_INFO";
			questProgressPlate.Init();
			QuestProgressPlate questProgressPlate3 = questProgressPlate;
			questProgressPlate3.mOnMouseUp = (OnMouseUp)Delegate.Combine(questProgressPlate3.mOnMouseUp, new OnMouseUp(OnButton));
			foreach (IQuestProgress item2 in selfQuest.Progress)
			{
				questProgressPlate.AddProgress(GuiSystem.GetLocaleText(item2.Desc), item2.CurVal, item2.MaxVal, 0f, selfQuest.Quest.ShowCur, selfQuest.Quest.ShowMax);
			}
			if (_updatedQuest == null || _updatedQuest.Contains(mActiveQuest))
			{
				questProgressPlate.Updated();
			}
			mQuestProgressPlates.Add(questProgressPlate);
		}
		SetProgressPlateSize();
	}

	private QuestProgressPlate GetProgressPlateById(int _questId)
	{
		foreach (QuestProgressPlate mQuestProgressPlate in mQuestProgressPlates)
		{
			if (mQuestProgressPlate.mId == _questId)
			{
				return mQuestProgressPlate;
			}
		}
		return null;
	}

	private void SetProgressPlateSize()
	{
		int num = 0;
		int num2 = 740;
		int count = mQuestProgressPlates.Count;
		foreach (QuestProgressPlate mQuestProgressPlate in mQuestProgressPlates)
		{
			mQuestProgressPlate.mZoneRect = new Rect(0f, num2 - (count - num) * 60, 394f, 53f);
			GuiSystem.GetRectScaled(ref mQuestProgressPlate.mZoneRect);
			mQuestProgressPlate.SetSize();
			num++;
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && mOnShowQuestInfo != null)
		{
			mOnShowQuestInfo(_sender.mId, _sender.mElementId == "PVP_QUEST_INFO");
		}
	}
}
