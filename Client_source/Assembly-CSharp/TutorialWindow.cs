using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class TutorialWindow : GuiElement
{
	public enum HintType
	{
		NONE,
		ROUND,
		ARROW_DOWN,
		ARROW_RIGHT,
		RECT_SELECTION,
		RECT_SELECTION2,
		NPC_DIALOG,
		BUILDING_BLINK,
		SHOW_PLAYER_LIST
	}

	private class TutorialHint : GuiElement
	{
		private TutorialMgr.Hint mData;

		private GuiElement mTargetElement;

		private Texture2D mHintImg;

		private Rect mHintDrawRect;

		private Rect mHintFrameRect;

		private int mMaxFrameCount;

		private int mCurFrame;

		private Color mCurColor = Color.gray;

		public override void Init()
		{
			if (mData != null)
			{
				Debug.Log("Gui/Tutorial/" + mData.mHintType.ToString().ToLower());
				mHintImg = GuiSystem.GetImage("Gui/Tutorial/" + mData.mHintType.ToString().ToLower());
			}
		}

		public override void SetSize()
		{
			if (mData == null)
			{
				return;
			}
			if (mData.mHintType == HintType.RECT_SELECTION || mData.mHintType == HintType.ROUND || mData.mHintType == HintType.RECT_SELECTION2)
			{
				if (mTargetElement == null)
				{
					Log.Error("Bad target element in tutorial");
					return;
				}
				GuiButton guiButton = mTargetElement as GuiButton;
				if (guiButton == null)
				{
					mHintDrawRect = mTargetElement.mZoneRect;
				}
				else if (guiButton.mIconRect.width == 0f || guiButton.mIconRect.height == 0f)
				{
					mHintDrawRect = mTargetElement.mZoneRect;
				}
				else
				{
					mHintDrawRect = guiButton.mIconRect;
				}
			}
			else if (mData.mHintType == HintType.ARROW_DOWN || mData.mHintType == HintType.ARROW_RIGHT)
			{
				float num = ((mData.mHintType != HintType.ARROW_DOWN) ? 270 : 164);
				float num2 = ((mData.mHintType != HintType.ARROW_DOWN) ? 165 : 258);
				mHintDrawRect = new Rect(0f, 0f, num * GuiSystem.mYRate, num2 * GuiSystem.mYRate);
				if (mTargetElement == null)
				{
					mHintDrawRect.x = mData.mPosition.x * GuiSystem.mYRate;
					mHintDrawRect.y = mData.mPosition.y * GuiSystem.mYRate;
				}
				else if (mData.mHintType == HintType.ARROW_RIGHT)
				{
					mHintDrawRect.x = mTargetElement.mZoneRect.x - mHintDrawRect.width;
					mHintDrawRect.y = mTargetElement.mZoneRect.y + mTargetElement.mZoneRect.width / 2f - mHintDrawRect.height / 2f;
				}
				else
				{
					mHintDrawRect.x = mTargetElement.mZoneRect.x + mTargetElement.mZoneRect.width / 2f - mHintDrawRect.width / 2f;
					mHintDrawRect.y = mTargetElement.mZoneRect.y - mHintDrawRect.height;
				}
			}
		}

		public override void Update()
		{
			if (mData.mHintType != 0)
			{
				if (mData.mHintType == HintType.RECT_SELECTION || mData.mHintType == HintType.RECT_SELECTION2)
				{
					mCurColor.a = Mathf.PingPong(Time.time / 2f, 0.3f);
				}
				else if (Time.frameCount % 3 == 0)
				{
					mCurFrame++;
					mCurFrame = ((mCurFrame < mMaxFrameCount) ? mCurFrame : 0);
					SetFrameRect();
				}
			}
		}

		public override void RenderElement()
		{
			if (mData.mHintType == HintType.RECT_SELECTION || mData.mHintType == HintType.RECT_SELECTION2)
			{
				GuiSystem.DrawImage(mHintImg, mHintDrawRect, mCurColor);
			}
			else
			{
				GuiSystem.DrawImage(mHintImg, mHintDrawRect, mHintFrameRect);
			}
		}

		public void SetData(TutorialMgr.Hint _data, GuiElement _targetElement)
		{
			mData = _data;
			mTargetElement = _targetElement;
			if (mData.mHintType == HintType.ARROW_DOWN || mData.mHintType == HintType.ARROW_RIGHT)
			{
				mMaxFrameCount = 6;
			}
			else if (mData.mHintType == HintType.ROUND)
			{
				mMaxFrameCount = 8;
			}
			else
			{
				mMaxFrameCount = 0;
			}
		}

		private void SetFrameRect()
		{
			float num = 1f / (float)mMaxFrameCount;
			if (mData.mHintType == HintType.ROUND || mData.mHintType == HintType.ARROW_RIGHT)
			{
				mHintFrameRect = new Rect(0f, (float)mCurFrame * num, 1f, num);
			}
			else if (mData.mHintType == HintType.ARROW_DOWN)
			{
				mHintFrameRect = new Rect((float)mCurFrame * num, 0f, num, 1f);
			}
		}
	}

	public delegate void BlockDone(int _blockNum);

	public delegate void BattleStarted();

	public delegate void TeleportAction(int _location, int _x, int _y);

	public delegate void NPCUpdateAction();

	public delegate void ShowListAction();

	public delegate void ChangeLocationAction(int _location);

	public delegate void OnTutorialDone(bool _cancelled);

	private Texture2D mFrame;

	private GuiButton mNextButton;

	private GuiButton mSkipButton;

	private OkDialog mOkDialog;

	private YesNoDialog mYesNoDialog;

	private QuestMenu mQuestWnd;

	private NpcStore mNpcStore;

	private SelfHero mSelfHero;

	private FightHelper mFightSender;

	private bool mInfoBlock = true;

	private string mText;

	private Rect mFrameRect;

	private Rect mTextRect;

	private List<TutorialHint> mHints;

	private TutorialMgr.Block mCurBlock;

	private int mCurBlockNum;

	public List<TutorialMgr.Block> mLinear;

	public List<TutorialMgr.Block> mNoneLinear;

	public BlockDone mBlockDone;

	public BattleStarted mOnStarted;

	public TeleportAction mTeleportRequest;

	public NPCUpdateAction mOnNpcUpdate;

	public ShowListAction mOnShowPlayerList;

	public ChangeLocationAction mChangeLocationRequest;

	public OnTutorialDone mOnTutorialDone;

	public override void Init()
	{
		mFrame = GuiSystem.GetImage("Gui/Tutorial/frame1");
		mHints = new List<TutorialHint>();
		mNextButton = GuiSystem.CreateButton("Gui/Tutorial/button1-norm", "Gui/Tutorial/button1-over", "Gui/Tutorial/button1-press", string.Empty, string.Empty);
		mNextButton.mElementId = "NEXT_BUTTON";
		GuiButton guiButton = mNextButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mNextButton.mLabel = GuiSystem.GetLocaleText("Select_Button_Name");
		mNextButton.RegisterAction(UserActionType.TUTORIAL_NEXT_BUTTON);
		mNextButton.Init();
		mSkipButton = GuiSystem.CreateButton("Gui/Tutorial/button2-norm", "Gui/Tutorial/button2-over", "Gui/Tutorial/button2-press", string.Empty, string.Empty);
		mSkipButton.mElementId = "SKIP_BUTTON";
		GuiButton guiButton2 = mSkipButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mSkipButton.mLabel = GuiSystem.GetLocaleText("Skip_Button_Name");
		mSkipButton.RegisterAction(UserActionType.TUTORIAL_CLOSE_BUTTON);
		mSkipButton.Init();
		AddTutorialElement(mNextButton.mElementId, mNextButton);
		AddTutorialElement(mSkipButton.mElementId, mSkipButton);
	}

	public override void SetSize()
	{
		mFrameRect = new Rect(10f, 500f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mFrameRect);
		mTextRect = new Rect(45f, 35f, mFrame.width - 90, mFrame.height - 120);
		GuiSystem.SetChildRect(mFrameRect, ref mTextRect);
		mNextButton.mZoneRect = new Rect(mFrame.width - 155, mFrame.height - 70, 120f, 46f);
		GuiSystem.SetChildRect(mFrameRect, ref mNextButton.mZoneRect);
		mSkipButton.mZoneRect = new Rect(35f, mFrame.height - 70, 154f, 46f);
		GuiSystem.SetChildRect(mFrameRect, ref mSkipButton.mZoneRect);
		foreach (TutorialHint mHint in mHints)
		{
			mHint.SetSize();
		}
	}

	public override void Update()
	{
		foreach (TutorialHint mHint in mHints)
		{
			mHint.Update();
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mFrameRect);
		if (mInfoBlock)
		{
			mNextButton.RenderElement();
		}
		mSkipButton.RenderElement();
		GUI.contentColor = new Color(47f / 256f, 17f / 128f, 15f / 128f, 1f);
		GuiSystem.DrawString(mText, mTextRect, "upper_left_12");
		GUI.contentColor = Color.white;
		foreach (TutorialHint mHint in mHints)
		{
			mHint.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mInfoBlock)
		{
			mNextButton.CheckEvent(_curEvent);
		}
		mSkipButton.CheckEvent(_curEvent);
	}

	public void SetData(OkDialog _okDialog, YesNoDialog _yesNoDialog, QuestMenu _questWnd, NpcStore _npcStore, SelfHero _self, FightHelper _sender)
	{
		mOkDialog = _okDialog;
		mYesNoDialog = _yesNoDialog;
		mQuestWnd = _questWnd;
		mNpcStore = _npcStore;
		mSelfHero = _self;
		mFightSender = _sender;
	}

	private void ChangeLocation(TutorialMgr.BlockAction _blockAction)
	{
		if (mChangeLocationRequest == null)
		{
			return;
		}
		foreach (TutorialMgr.UIelement mUIelement in _blockAction.mUIelements)
		{
			if (!int.TryParse(mUIelement.mParentId, out var result))
			{
				Log.Error("Wrong value for race: " + mUIelement.mParentId);
				break;
			}
			if (mSelfHero.Hero.View.mRace == result)
			{
				mChangeLocationRequest(mUIelement.mKeyId);
			}
		}
	}

	private void Teleport(TutorialMgr.BlockAction _blockAction)
	{
		if (mTeleportRequest == null)
		{
			return;
		}
		foreach (TutorialMgr.UIelement mUIelement in _blockAction.mUIelements)
		{
			string[] array = mUIelement.mParentId.Trim().Split(',');
			if (array.Length == 2)
			{
				if (!int.TryParse(array[0], out var result))
				{
					Log.Error("Wrong value for position x element: " + array[0]);
					break;
				}
				if (!int.TryParse(array[1], out var result2))
				{
					Log.Error("Wrong value for position y element: " + array[1]);
					break;
				}
				mTeleportRequest(mUIelement.mKeyId, result, result2);
			}
		}
	}

	private void StartBattle(TutorialMgr.UIelement _info)
	{
		if (mOnStarted != null && int.TryParse(_info.mElementId, out var result))
		{
			mOnStarted();
			mFightSender.TutorialHuntReady(result, _info.mKeyId);
		}
	}

	private void ShowQuest(TutorialMgr.UIelement _info)
	{
		if (mQuestWnd != null)
		{
			INpc npc = mNpcStore.Get(_info.mKeyId);
			if (npc != null)
			{
				QuestStore.Quest quest = new QuestStore.Quest(_info.mKeyId);
				quest.mName = _info.mParentId;
				quest.mStartDesc = _info.mElementId;
				mQuestWnd.SetData(npc, quest, null, mSelfHero, null);
				mQuestWnd.SetFakeStatus();
				mQuestWnd.SetActive(_active: true);
			}
		}
	}

	public void SetTutorialData(TutorialMgr.TutorialSet _set)
	{
		if (_set != null)
		{
			mLinear = _set.mLinear;
			mNoneLinear = _set.mNoneLinear;
		}
	}

	public void TutorialStart()
	{
		SetActive(_active: true);
		TryShowNextBlock();
	}

	public void TutorialDone()
	{
		SetActive(_active: false);
		ClearBlockEvents(mCurBlock);
		if (mHints != null)
		{
			mHints.Clear();
		}
		mText = string.Empty;
		mCurBlock = null;
		mCurBlockNum = 0;
		mLinear = null;
		mNoneLinear = null;
	}

	public void TutorialFinished()
	{
		mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_TUTORIAL_FINISHED"));
	}

	public bool OnTutorialEvent(TutorialMgr.ActionType _eventType)
	{
		if (CheckCurBlockEvent(_eventType, null, 0))
		{
			TryShowNextBlock();
			return true;
		}
		return false;
	}

	public bool OnTutorialEvent(TutorialMgr.ActionType _eventType, int _id)
	{
		if (CheckCurBlockEvent(_eventType, null, _id))
		{
			TryShowNextBlock();
			return true;
		}
		return false;
	}

	private void TryShowNextBlock()
	{
		if (mLinear != null)
		{
			ClearBlockEvents(mCurBlock);
			if (mCurBlockNum != 0 && mBlockDone != null)
			{
				mBlockDone(mCurBlockNum);
			}
			if (mLinear.Count <= mCurBlockNum && mOnTutorialDone != null)
			{
				mOnTutorialDone(_cancelled: false);
				return;
			}
			mHints.Clear();
			mCurBlock = mLinear[mCurBlockNum];
			mCurBlockNum++;
			InitBlock(mCurBlock);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_sender.mElementId == "NEXT_BUTTON" && _buttonId == 0)
		{
			TryShowNextBlock();
		}
		else if (_sender.mElementId == "SKIP_BUTTON" && _buttonId == 0 && mYesNoDialog != null)
		{
			mYesNoDialog.SetData(GuiSystem.GetLocaleText("GUI_TUTORIAL_SKIP_QUESTION"), "Ok_Button_Name", "Cancel_Button_Name", OnSkipTutorial);
		}
	}

	private void OnTutorialButton(GuiElement _button, int _elementId)
	{
		if (CheckCurBlockEvent(TutorialMgr.ActionType.ON_BUTTON, _button, 0))
		{
			TryShowNextBlock();
		}
	}

	private void OnSkipTutorial(bool _yes)
	{
		if (_yes)
		{
			if (mOnNpcUpdate != null)
			{
				mOnNpcUpdate();
			}
			if (mOkDialog != null)
			{
				mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_TUTORIAL_ON_SKIP_ACCEPT"));
			}
			if (mOnTutorialDone != null)
			{
				mOnTutorialDone(_cancelled: true);
			}
		}
	}

	private void InitBlock(TutorialMgr.Block _block)
	{
		if (_block != null)
		{
			mInfoBlock = _block.mAction == null;
			mText = GuiSystem.GetLocaleText(_block.mTextId);
			InitBlockHints(_block);
			InitBlockEvents(_block);
		}
	}

	private void InitBlockHints(TutorialMgr.Block _block)
	{
		TutorialHint tutorialHint = null;
		GuiElement guiElement = null;
		foreach (TutorialMgr.Hint mHint in _block.mHints)
		{
			if (mHint.mHintType == HintType.SHOW_PLAYER_LIST && mOnShowPlayerList != null)
			{
				mOnShowPlayerList();
			}
			if (mHint.mHintType == HintType.NPC_DIALOG)
			{
				ShowQuest(mHint.mElement);
			}
			else if (mHint.mHintType == HintType.BUILDING_BLINK)
			{
				Blink[] array = UnityEngine.Object.FindObjectsOfType(typeof(Blink)) as Blink[];
				Blink[] array2 = array;
				foreach (Blink blink in array2)
				{
					if (blink.mId == mHint.mElement.mKeyId)
					{
						blink.mOff = false;
					}
				}
				BuildingSelector[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(BuildingSelector)) as BuildingSelector[];
				BuildingSelector[] array4 = array3;
				foreach (BuildingSelector buildingSelector in array4)
				{
					if (buildingSelector.mBuilding == (Building)mHint.mElement.mKeyId)
					{
						buildingSelector.mOff = true;
					}
				}
			}
			else
			{
				guiElement = TutorialMgr.GetGuiElement(mHint.mElement);
				tutorialHint = new TutorialHint();
				tutorialHint.SetData(mHint, guiElement);
				tutorialHint.Init();
				mHints.Add(tutorialHint);
			}
		}
		foreach (TutorialHint mHint2 in mHints)
		{
			mHint2.SetSize();
		}
	}

	private void InitBlockEvents(TutorialMgr.Block _block)
	{
		if (_block == null || _block.mAction == null)
		{
			return;
		}
		if (_block.mAction.mActionType == TutorialMgr.ActionType.BATTLE_START)
		{
			TryShowNextBlock();
			StartBattle(_block.mAction.mUIelements[0]);
			return;
		}
		if (_block.mAction.mActionType == TutorialMgr.ActionType.ON_NPC_CLICK)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(QuestMark));
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object @object in array2)
			{
				QuestMark questMark = @object as QuestMark;
				if (!(questMark != null))
				{
					continue;
				}
				foreach (TutorialMgr.UIelement mUIelement in _block.mAction.mUIelements)
				{
					INpc npc = mNpcStore.TryGet(mUIelement.mKeyId);
					if (npc != null && mUIelement.mKeyId == questMark.mNpcId)
					{
						questMark.mExclamationMark.SetActiveRecursively(state: true);
						questMark.mQuestionMark.SetActiveRecursively(state: false);
					}
				}
			}
		}
		if (_block.mAction.mActionType == TutorialMgr.ActionType.TELEPORT)
		{
			TryShowNextBlock();
			Teleport(_block.mAction);
		}
		else if (_block.mAction.mActionType == TutorialMgr.ActionType.CHANGE_LOCATION)
		{
			TryShowNextBlock();
			ChangeLocation(_block.mAction);
		}
		else if (_block.mAction.mActionType == TutorialMgr.ActionType.ON_BUTTON)
		{
			foreach (TutorialMgr.UIelement mUIelement2 in _block.mAction.mUIelements)
			{
				GuiElement guiElement = TutorialMgr.GetGuiElement(mUIelement2);
				if (guiElement == null)
				{
					Log.Error("Try to get bad element in tutorial");
					break;
				}
				guiElement.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiElement.mOnMouseUp, new OnMouseUp(OnTutorialButton));
			}
		}
		else
		{
			if (_block.mAction.mActionType != TutorialMgr.ActionType.ON_BUILDING_CLICK)
			{
				return;
			}
			foreach (TutorialMgr.UIelement mUIelement3 in _block.mAction.mUIelements)
			{
				if (mUIelement3.mKeyId <= 0)
				{
					GuiElement guiElement2 = TutorialMgr.GetGuiElement(mUIelement3);
					if (guiElement2 == null)
					{
						Log.Error("Try to get bad element in tutorial");
						break;
					}
					guiElement2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiElement2.mOnMouseUp, new OnMouseUp(OnTutorialButton));
				}
			}
		}
	}

	private void ClearBlockEvents(TutorialMgr.Block _block)
	{
		if (_block == null || _block.mAction == null)
		{
			return;
		}
		if (_block.mAction.mActionType == TutorialMgr.ActionType.ON_BUTTON)
		{
			foreach (TutorialMgr.UIelement mUIelement in _block.mAction.mUIelements)
			{
				GuiElement guiElement = TutorialMgr.GetGuiElement(mUIelement);
				if (guiElement == null)
				{
					Log.Error("Try to get bad element in tutorial");
					break;
				}
				guiElement.mOnMouseUp = (OnMouseUp)Delegate.Remove(guiElement.mOnMouseUp, new OnMouseUp(OnTutorialButton));
			}
		}
		else
		{
			if (_block.mAction.mActionType != TutorialMgr.ActionType.ON_BUILDING_CLICK)
			{
				return;
			}
			foreach (TutorialMgr.UIelement mUIelement2 in _block.mAction.mUIelements)
			{
				if (mUIelement2.mKeyId <= 0)
				{
					GuiElement guiElement2 = TutorialMgr.GetGuiElement(mUIelement2);
					if (guiElement2 == null)
					{
						Log.Error("Try to get bad element in tutorial");
						break;
					}
					guiElement2.mOnMouseUp = (OnMouseUp)Delegate.Remove(guiElement2.mOnMouseUp, new OnMouseUp(OnTutorialButton));
				}
			}
		}
	}

	private bool CheckCurBlockEvent(TutorialMgr.ActionType _eventType, GuiElement _element, int _id)
	{
		if (mCurBlock == null || mCurBlock.mAction == null)
		{
			return false;
		}
		if (mCurBlock.mAction.mActionType != _eventType && _eventType != TutorialMgr.ActionType.ON_BUTTON)
		{
			return false;
		}
		foreach (TutorialMgr.UIelement mUIelement in mCurBlock.mAction.mUIelements)
		{
			GuiElement guiElement = TutorialMgr.GetGuiElement(mUIelement);
			if (guiElement == _element)
			{
				UserLog.AddAction(UserActionType.TUTORIAL_TRIGGER, (int)_eventType, GuiSystem.GetLocaleText("TUTOR_ACTION_" + _eventType));
				return true;
			}
		}
		if (_eventType == TutorialMgr.ActionType.ON_NPC_CLICK || _eventType == TutorialMgr.ActionType.ON_BUILDING_CLICK)
		{
			foreach (TutorialMgr.UIelement mUIelement2 in mCurBlock.mAction.mUIelements)
			{
				if (mUIelement2.mKeyId == _id)
				{
					UserLog.AddAction(UserActionType.TUTORIAL_TRIGGER, (int)_eventType, GuiSystem.GetLocaleText("TUTOR_ACTION_" + _eventType));
					if (_eventType == TutorialMgr.ActionType.ON_NPC_CLICK && mOnNpcUpdate != null)
					{
						mOnNpcUpdate();
					}
					return true;
				}
			}
			return false;
		}
		UserLog.AddAction(UserActionType.TUTORIAL_TRIGGER, (int)_eventType, GuiSystem.GetLocaleText("TUTOR_ACTION_" + _eventType));
		return true;
	}
}
