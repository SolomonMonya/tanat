using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class MapJoinQueueMenu : GuiElement
{
	private class MapJoinRequest : GuiElement
	{
		public GuiButton mCancelButton;

		private Texture2D mFrame;

		private Texture2D mFrameHided;

		private string mName;

		private string mTime;

		private string mTimeText;

		private Rect mNameRect;

		private Rect mTimeRect;

		private IJoinedQueue mData;

		private bool mHide;

		public override void Init()
		{
			if (mData != null)
			{
				mFrame = GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame3");
				mFrameHided = GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame4");
				mTimeText = GuiSystem.GetLocaleText("Waiting_Time_Text");
				mCancelButton = GuiSystem.CreateButton("Gui/MapJoinQueueMenu/button_3_norm", "Gui/MapJoinQueueMenu/button_3_over", "Gui/MapJoinQueueMenu/button_3_press", string.Empty, GuiSystem.GetLocaleText("Cancel_Button_Name"));
				mCancelButton.mId = mData.MapData.mId;
				mCancelButton.mElementId = "CANCEL_REQUEST_BUTTON";
				mCancelButton.Init();
				SetName();
			}
		}

		public override void SetSize()
		{
			if (mHide)
			{
				mNameRect = new Rect(4f, 3f, 169f, 20f);
				mTimeRect = new Rect(4f, 24f, 169f, 20f);
			}
			else
			{
				mNameRect = new Rect(4f, 3f, 346f, 20f);
				mTimeRect = new Rect(24f, 24f, 306f, 20f);
			}
			mCancelButton.mZoneRect = new Rect(120f, 52f, 114f, 40f);
			GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
		}

		public override void RenderElement()
		{
			if (mHide)
			{
				GuiSystem.DrawImage(mFrameHided, mZoneRect);
				GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
				GuiSystem.DrawString(mName, mNameRect, "middle_center_11_bold");
				GuiSystem.DrawString(mTime, mTimeRect, "middle_center_11_bold");
				GUI.contentColor = Color.white;
			}
			else
			{
				GuiSystem.DrawImage(mFrame, mZoneRect);
				GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
				GuiSystem.DrawString(mName, mNameRect, "middle_center_11_bold");
				GuiSystem.DrawString(mTimeText, mTimeRect, "middle_left_11_bold");
				GuiSystem.DrawString(mTime, mTimeRect, "middle_right_11_bold");
				GUI.contentColor = Color.white;
				mCancelButton.RenderElement();
			}
		}

		public override void CheckEvent(Event _curEvent)
		{
			mCancelButton.CheckEvent(_curEvent);
		}

		public override void Update()
		{
			mTime = string.Empty;
			mTime = FormatTime(DateTime.Now - mData.StartTime);
		}

		public void SetData(IJoinedQueue _data)
		{
			mData = _data;
		}

		public void Hide(bool _hide)
		{
			mHide = _hide;
			SetName();
		}

		private string FormatTime(TimeSpan _dt)
		{
			return _dt.Minutes.ToString("0#") + ":" + _dt.Seconds.ToString("0#");
		}

		private void SetName()
		{
			if (mHide)
			{
				mName = GuiSystem.GetLocaleText(((MapType)mData.MapData.mType).ToString() + "_Text");
			}
			else
			{
				mName = GuiSystem.GetLocaleText(((MapType)mData.MapData.mType).ToString() + "_Text") + " - " + GuiSystem.GetLocaleText(mData.MapData.mName);
			}
		}
	}

	private class MapJoinAccept : GuiElement
	{
		public GuiButton mJoinButton;

		public float mTimeToStart;

		private Texture2D mFrame;

		private string mName;

		private string mTime;

		private string mTimeText;

		private Rect mNameRect;

		private Rect mTimeRect;

		private IJoinedQueue mData;

		public override void Init()
		{
			if (mData != null)
			{
				mFrame = GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame3");
				mTimeText = GuiSystem.GetLocaleText("Connect_Time_Text");
				mJoinButton = GuiSystem.CreateButton("Gui/MapJoinQueueMenu/button_3_norm", "Gui/MapJoinQueueMenu/button_3_over", "Gui/MapJoinQueueMenu/button_3_press", string.Empty, GuiSystem.GetLocaleText("Begin_Text"));
				mJoinButton.mId = mData.MapData.mId;
				mJoinButton.mElementId = "JOIN_REQUEST_BUTTON";
				mJoinButton.RegisterAction(UserActionType.BUTTON_ACCEPT_BATTLE);
				mJoinButton.Init();
				mName = GuiSystem.GetLocaleText(((MapType)mData.MapData.mType).ToString() + "_Text") + " - " + GuiSystem.GetLocaleText(mData.MapData.mName);
			}
		}

		public override void SetSize()
		{
			mNameRect = new Rect(4f, 3f, 346f, 20f);
			mTimeRect = new Rect(24f, 24f, 306f, 20f);
			mJoinButton.mZoneRect = new Rect(120f, 52f, 114f, 40f);
			GuiSystem.SetChildRect(mZoneRect, ref mJoinButton.mZoneRect);
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
			GuiSystem.DrawString(mName, mNameRect, "middle_center_11_bold");
			GuiSystem.DrawString(mTime, mTimeRect, "middle_center_11_bold");
			GUI.contentColor = Color.white;
			mJoinButton.RenderElement();
		}

		public override void CheckEvent(Event _curEvent)
		{
			mJoinButton.CheckEvent(_curEvent);
		}

		public override void Update()
		{
			mTimeToStart -= Time.deltaTime;
			if (mTimeToStart < 0f)
			{
				mTimeToStart = 0f;
			}
			mTime = string.Format(mTimeText, Mathf.FloorToInt(mTimeToStart));
		}

		public void SetData(IJoinedQueue _data)
		{
			mData = _data;
		}

		public IJoinedQueue GetQueue()
		{
			return mData;
		}
	}

	private class CastleJoinRequest : GuiElement
	{
		public delegate void TimeUp(CastleJoinRequest _arg);

		private Texture2D mFrame;

		private Texture2D mFrameHided;

		private string mName;

		private string mTime;

		private string mRound;

		private string mEndWaiting;

		private string mEndWaitingShort;

		private string mWaiting;

		private string mStartWait;

		private Rect mRoundRect;

		private Rect mNameRect;

		private Rect mTimeRect;

		private CastleStartBattleInfoArg mData;

		private bool mHide;

		public TimeUp mOnTimeUp;

		public CastleJoinRequest(CastleStartBattleInfoArg _data)
		{
			mData = _data;
		}

		public override void Init()
		{
			if (mData != null)
			{
				mFrame = GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame3");
				mFrameHided = GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame4");
				mWaiting = GuiSystem.GetLocaleText("Castle_Waiting_Time_Text");
				mEndWaiting = GuiSystem.GetLocaleText("Castle_End_Waiting_Time_Text");
				mEndWaitingShort = GuiSystem.GetLocaleText("Castle_End_Waiting_Time_Short_Text");
				mRound = GuiSystem.GetLocaleText("Castle_Stage_Text");
				mStartWait = GuiSystem.GetLocaleText("Castle_Start_Text");
				if (mData.mType == CastleStartBattleInfoArg.WaitType.FIVE_MINUTES)
				{
					string empty = string.Empty;
					mRound = string.Format(arg1: (mData.mStage > 1) ? $"(1/{mData.mStage})" : ((mData.mStage != 1) ? GuiSystem.GetLocaleText("Castle_War_Text") : GuiSystem.GetLocaleText("Castle_Final_Text")), format: mRound, arg0: mData.mRound);
				}
				SetName();
			}
		}

		public override void SetSize()
		{
			if (mHide)
			{
				mNameRect = new Rect(4f, 3f, 169f, 20f);
				if (mData.mType == CastleStartBattleInfoArg.WaitType.OWNER || mData.mType == CastleStartBattleInfoArg.WaitType.TWO_HOURS)
				{
					mTimeRect = new Rect(4f, 24f, 169f, 20f);
				}
				else if (mData.mType == CastleStartBattleInfoArg.WaitType.FIVE_MINUTES)
				{
					mRoundRect = new Rect(21f, 44f, 189f, 16f);
					mTimeRect = new Rect(4f, 24f, 169f, 20f);
				}
			}
			else
			{
				mNameRect = new Rect(4f, 3f, 346f, 20f);
				if (mData.mType == CastleStartBattleInfoArg.WaitType.OWNER || mData.mType == CastleStartBattleInfoArg.WaitType.TWO_HOURS)
				{
					mTimeRect = new Rect(24f, 24f, 306f, 20f);
				}
				else if (mData.mType == CastleStartBattleInfoArg.WaitType.FIVE_MINUTES)
				{
					mRoundRect = new Rect(24f, 24f, 306f, 20f);
					mTimeRect = new Rect(24f, 44f, 306f, 20f);
				}
			}
			GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
			GuiSystem.SetChildRect(mZoneRect, ref mTimeRect);
			GuiSystem.SetChildRect(mZoneRect, ref mRoundRect);
		}

		public override void RenderElement()
		{
			if (mHide)
			{
				GuiSystem.DrawImage(mFrameHided, mZoneRect);
				GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
				GuiSystem.DrawString(mName, mNameRect, "middle_center");
				if (mData.mType == CastleStartBattleInfoArg.WaitType.OWNER)
				{
					GuiSystem.DrawString(mEndWaitingShort, mTimeRect, "middle_center");
				}
				else if (mData.mType == CastleStartBattleInfoArg.WaitType.TWO_HOURS)
				{
					GuiSystem.DrawString(mTime, mTimeRect, "middle_center_11_bold");
				}
				else if (mData.mType == CastleStartBattleInfoArg.WaitType.FIVE_MINUTES)
				{
					GuiSystem.DrawString(mTime, mTimeRect, "middle_center_11_bold");
				}
				GUI.contentColor = Color.white;
				return;
			}
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
			GuiSystem.DrawString(mName, mNameRect, "middle_center_11_bold");
			if (mData.mType == CastleStartBattleInfoArg.WaitType.OWNER)
			{
				GuiSystem.DrawString(mEndWaiting, mTimeRect, "middle_center_11_bold");
			}
			else if (mData.mType == CastleStartBattleInfoArg.WaitType.TWO_HOURS)
			{
				GuiSystem.DrawString(mWaiting, mTimeRect, "middle_left_11_bold");
				GuiSystem.DrawString(mTime, mTimeRect, "middle_right_11_bold");
			}
			else if (mData.mType == CastleStartBattleInfoArg.WaitType.FIVE_MINUTES)
			{
				GuiSystem.DrawString(mRound, mRoundRect, "middle_center_11_bold");
				GuiSystem.DrawString(mStartWait, mTimeRect, "middle_left_11_bold");
				GuiSystem.DrawString(mTime, mTimeRect, "middle_right_11_bold");
			}
			GUI.contentColor = Color.white;
		}

		public override void Update()
		{
			mTime = string.Empty;
			mTime = FormatTime(mData.mStartTime - DateTime.Now);
		}

		public void SetData(CastleStartBattleInfoArg _data)
		{
			mData = _data;
		}

		public void Hide(bool _hide)
		{
			mHide = _hide;
			SetName();
		}

		private string FormatTime(TimeSpan _dt)
		{
			if (_dt.TotalSeconds <= 0.0)
			{
				if (mOnTimeUp != null && mData.mType != CastleStartBattleInfoArg.WaitType.OWNER)
				{
					mOnTimeUp(this);
				}
				return "00:00";
			}
			if (_dt.Hours > 0)
			{
				return _dt.Hours.ToString("0#") + ":" + _dt.Minutes.ToString("0#") + ":" + _dt.Seconds.ToString("0#");
			}
			return _dt.Minutes.ToString("0#") + ":" + _dt.Seconds.ToString("0#");
		}

		private void SetName()
		{
			if (mHide)
			{
				mName = GuiSystem.GetLocaleText(MapType.CW_DOTA.ToString() + "_Text");
			}
			else
			{
				mName = GuiSystem.GetLocaleText(MapType.CW_DOTA.ToString() + "_Text") + " - " + GuiSystem.GetLocaleText(mData.mCastleName);
			}
		}
	}

	public delegate void DesertCallback(int _queueId);

	public delegate void VoidCallback();

	public DesertCallback mDesertCallback;

	public VoidCallback mAcceptJoinCallback;

	private GuiButton mHideButton;

	private Texture2D mFrame;

	private bool mHide;

	private Dictionary<int, MapJoinRequest> mMapJoinRequests = new Dictionary<int, MapJoinRequest>();

	private List<CastleJoinRequest> mCastleJoinRequests = new List<CastleJoinRequest>();

	private List<CastleJoinRequest> mToDelete;

	private MapJoinAccept mMapJoinAccept;

	private YesNoDialog mYesNoDialog;

	public int mBanTimer;

	public override void Init()
	{
		mHide = false;
		mMapJoinRequests = new Dictionary<int, MapJoinRequest>();
		mCastleJoinRequests = new List<CastleJoinRequest>();
		mToDelete = new List<CastleJoinRequest>();
		mHideButton = new GuiButton();
		mHideButton.mElementId = "HIDE_BUTTON";
		GuiButton guiButton = mHideButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mHideButton.Init();
		SetHide(mHide);
	}

	public override void SetSize()
	{
		SetAllSizes();
	}

	public void SetZoneSize()
	{
		int num = ((!mHide) ? 97 : 52);
		int num2 = ((mMapJoinAccept != null) ? 1 : 0) + mCastleJoinRequests.Count + mMapJoinRequests.Count;
		mZoneRect = new Rect(0f, 450f, mFrame.width, num * num2 + 22);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width;
		mHideButton.mZoneRect = new Rect(mFrame.width - 32, 0f, 32f, 32f);
		GuiSystem.SetChildRect(mZoneRect, ref mHideButton.mZoneRect);
	}

	public override void RenderElement()
	{
		if (mMapJoinRequests.Count == 0 && mMapJoinAccept == null && mCastleJoinRequests.Count == 0)
		{
			return;
		}
		if (mMapJoinAccept != null)
		{
			mMapJoinAccept.RenderElement();
		}
		foreach (KeyValuePair<int, MapJoinRequest> mMapJoinRequest in mMapJoinRequests)
		{
			mMapJoinRequest.Value.RenderElement();
		}
		foreach (CastleJoinRequest mCastleJoinRequest in mCastleJoinRequests)
		{
			mCastleJoinRequest.RenderElement();
		}
		GuiSystem.DrawImage(mFrame, mZoneRect, 32, 32, 32, 32);
		mHideButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mMapJoinRequests.Count == 0 && mMapJoinAccept == null && mCastleJoinRequests.Count == 0)
		{
			return;
		}
		mHideButton.CheckEvent(_curEvent);
		if (mMapJoinAccept != null)
		{
			mMapJoinAccept.CheckEvent(_curEvent);
			return;
		}
		foreach (KeyValuePair<int, MapJoinRequest> mMapJoinRequest in mMapJoinRequests)
		{
			mMapJoinRequest.Value.CheckEvent(_curEvent);
		}
	}

	public override void Update()
	{
		if (mMapJoinAccept != null)
		{
			mMapJoinAccept.Update();
		}
		else
		{
			foreach (KeyValuePair<int, MapJoinRequest> mMapJoinRequest in mMapJoinRequests)
			{
				mMapJoinRequest.Value.Update();
			}
		}
		foreach (CastleJoinRequest mCastleJoinRequest in mCastleJoinRequests)
		{
			mCastleJoinRequest.Update();
		}
		foreach (CastleJoinRequest item in mToDelete)
		{
			mCastleJoinRequests.Remove(item);
		}
	}

	public void UpdateJoinedQueues(IEnumerable<IJoinedQueue> _queues)
	{
		mMapJoinRequests.Clear();
		if (mMapJoinAccept != null)
		{
			return;
		}
		foreach (IJoinedQueue _queue in _queues)
		{
			AddMapJoinRequestInList(_queue);
		}
		SetAllSizes();
	}

	public void AddMapJoinRequest(IJoinedQueue _data)
	{
		AddMapJoinRequestInList(_data);
		SetAllSizes();
	}

	public void RemoveJoinRequest(int _id)
	{
		if (mMapJoinRequests.ContainsKey(_id))
		{
			mMapJoinRequests.Remove(_id);
		}
		SetAllSizes();
	}

	public void AskJoin(IJoinedQueue _queue, int _waitTimeSec)
	{
		if (_queue != null)
		{
			mMapJoinRequests.Clear();
			mHideButton.mLocked = true;
			mMapJoinAccept = new MapJoinAccept();
			mMapJoinAccept.mTimeToStart = _waitTimeSec;
			mMapJoinAccept.mZoneRect = default(Rect);
			mMapJoinAccept.SetData(_queue);
			mMapJoinAccept.Init();
			GuiButton mJoinButton = mMapJoinAccept.mJoinButton;
			mJoinButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mJoinButton.mOnMouseUp, new OnMouseUp(OnButton));
			SetHide(_hide: false);
			string localeText = GuiSystem.GetLocaleText(string.Concat((MapType)_queue.MapData.mType, "_Text"));
			string localeText2 = GuiSystem.GetLocaleText(_queue.MapData.mName);
			string text = string.Format(GuiSystem.GetLocaleText("GUI_ACCEPT_BATTLE_INVITE"), localeText + " - " + localeText2);
			mYesNoDialog.SetData(text, "Begin_Text", "Deposit_Text", OnJoinAnswer);
		}
	}

	public void DeclineAskJoin(IJoinedQueue _queue)
	{
		if (mMapJoinAccept != null && mMapJoinAccept.GetQueue().MapData.mId == _queue.MapData.mId)
		{
			mMapJoinAccept = null;
			mHideButton.mLocked = false;
		}
	}

	public void ClearRequests()
	{
		mMapJoinRequests.Clear();
		mCastleJoinRequests.Clear();
		if (mMapJoinAccept != null)
		{
			DeclineAskJoin(mMapJoinAccept.GetQueue());
		}
	}

	public void Clean()
	{
		ClearRequests();
	}

	public void SetData(YesNoDialog _yesNoDialog)
	{
		mYesNoDialog = _yesNoDialog;
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "HIDE_BUTTON")
		{
			SetHide(!mHide);
		}
		else if (_sender.mElementId == "CANCEL_REQUEST_BUTTON")
		{
			if (mDesertCallback != null)
			{
				mDesertCallback(_sender.mId);
			}
		}
		else if (_sender.mElementId == "JOIN_REQUEST_BUTTON" && mAcceptJoinCallback != null)
		{
			mAcceptJoinCallback();
		}
	}

	private void OnJoinAnswer(bool _result)
	{
		if (_result)
		{
			UserLog.AddAction(UserActionType.BUTTON_ACCEPT_BATTLE);
		}
		else
		{
			UserLog.AddAction(UserActionType.BUTTON_DECLINE_BATTLE);
		}
		if (_result && mAcceptJoinCallback != null)
		{
			mAcceptJoinCallback();
		}
	}

	private void SetHide(bool _hide)
	{
		mHide = _hide;
		mFrame = ((!mHide) ? GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame1") : GuiSystem.GetImage("Gui/MapJoinQueueMenu/frame2"));
		string text = "Gui/MapJoinQueueMenu/button_" + ((!_hide) ? "1" : "2");
		mHideButton.mNormImg = GuiSystem.GetImage(text + "_norm");
		mHideButton.mOverImg = GuiSystem.GetImage(text + "_over");
		mHideButton.mPressImg = GuiSystem.GetImage(text + "_press");
		mHideButton.SetCurBtnImage();
		foreach (KeyValuePair<int, MapJoinRequest> mMapJoinRequest in mMapJoinRequests)
		{
			mMapJoinRequest.Value.Hide(_hide);
		}
		foreach (CastleJoinRequest mCastleJoinRequest in mCastleJoinRequests)
		{
			mCastleJoinRequest.Hide(_hide);
		}
		SetAllSizes();
	}

	private void AddMapJoinRequestInList(IJoinedQueue _data)
	{
		MapJoinRequest mapJoinRequest = new MapJoinRequest();
		mapJoinRequest.SetData(_data);
		mapJoinRequest.Init();
		GuiButton mCancelButton = mapJoinRequest.mCancelButton;
		mCancelButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(mCancelButton.mOnMouseUp, new OnMouseUp(OnButton));
		mapJoinRequest.Hide(mHide);
		mMapJoinRequests[_data.MapData.mId] = mapJoinRequest;
	}

	public void SetCastleJoinRequest(CastleStartBattleInfoArg _data)
	{
		mCastleJoinRequests.Clear();
		if (_data != null)
		{
			CastleJoinRequest castleJoinRequest = new CastleJoinRequest(_data);
			castleJoinRequest.Init();
			castleJoinRequest.Hide(mHide);
			castleJoinRequest.mOnTimeUp = (CastleJoinRequest.TimeUp)Delegate.Combine(castleJoinRequest.mOnTimeUp, new CastleJoinRequest.TimeUp(OnTimeUp));
			mCastleJoinRequests.Add(castleJoinRequest);
			SetAllSizes();
		}
	}

	private void OnTimeUp(CastleJoinRequest _arg)
	{
		mToDelete.Add(_arg);
	}

	private void SetMapJoinRequestsSize()
	{
		int num = ((mMapJoinAccept != null) ? 1 : 0);
		int num2 = ((!mHide) ? 354 : 177);
		int num3 = ((!mHide) ? 97 : 52);
		foreach (KeyValuePair<int, MapJoinRequest> mMapJoinRequest in mMapJoinRequests)
		{
			if (mHide)
			{
				mMapJoinRequest.Value.mZoneRect = new Rect(5f, 11 + num * num3, num2, num3);
			}
			else
			{
				mMapJoinRequest.Value.mZoneRect = new Rect(5f, 11 + num * num3, num2, num3);
			}
			GuiSystem.SetChildRect(mZoneRect, ref mMapJoinRequest.Value.mZoneRect);
			mMapJoinRequest.Value.SetSize();
			num++;
		}
		foreach (CastleJoinRequest mCastleJoinRequest in mCastleJoinRequests)
		{
			if (mHide)
			{
				mCastleJoinRequest.mZoneRect = new Rect(5f, 11 + num * num3, num2, num3);
			}
			else
			{
				mCastleJoinRequest.mZoneRect = new Rect(5f, 11 + num * num3, num2, num3);
			}
			GuiSystem.SetChildRect(mZoneRect, ref mCastleJoinRequest.mZoneRect);
			mCastleJoinRequest.SetSize();
			num++;
		}
	}

	private void SetMapAcceptSize()
	{
		if (mMapJoinAccept != null)
		{
			mMapJoinAccept.mZoneRect = new Rect(5f, 11f, 354f, 97f);
			GuiSystem.SetChildRect(mZoneRect, ref mMapJoinAccept.mZoneRect);
			mMapJoinAccept.SetSize();
		}
	}

	private void SetAllSizes()
	{
		SetZoneSize();
		SetMapAcceptSize();
		SetMapJoinRequestsSize();
	}
}
