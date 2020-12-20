using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class CompensationMenu : GuiElement, EscapeListener
{
	private enum TabType
	{
		NONE,
		SHORT_INFO,
		FULL_INFO
	}

	private class ShortInfo : GuiElement
	{
		private MapType mMapType;

		private Texture2D mFrame;

		private Texture2D mRaitingImg;

		private MoneyRenderer mMoneyRenderer;

		private string mRewardText;

		private string mHonorText;

		private string mExpText;

		private string mRaitingText;

		private string mMoneyText;

		private string mYourRewardText;

		private string mExpValueText;

		private string mHonorValueText;

		private string mRaitingValueText;

		private Rect mRewardTextRect;

		private Rect mHonorTextRect;

		private Rect mExpTextRect;

		private Rect mRaitingTextRect;

		private Rect mMoneyTextRect;

		private Rect mYourRewardTextRect;

		private Rect mExpValueRect;

		private Rect mHonorValueRect;

		private Rect mRaitingValueRect;

		private Rect mRaitingImgRect;

		private FormatedTipMgr mFormatedTipMgr;

		private CtrlPrototype mItemData;

		private GuiButton mItemButton;

		private string mItemCntText;

		private Rect mItemCntTextRect;

		public override void Init()
		{
			mRewardText = GuiSystem.GetLocaleText("Reward_Battle_Label_Text");
			mHonorText = GuiSystem.GetLocaleText("Honor_Text");
			mExpText = GuiSystem.GetLocaleText("Exp_Text");
			mRaitingText = GuiSystem.GetLocaleText("Raiting_Text");
			mMoneyText = GuiSystem.GetLocaleText("Money_Text");
			mYourRewardText = GuiSystem.GetLocaleText("Yout_Reward_Text");
			mHonorText = mHonorText.Replace("{HONOR}", string.Empty);
			mExpText = mExpText.Replace("{EXP}", string.Empty);
			mRaitingText = mRaitingText.Replace("{RAITING}({DELTA_RAITING})", string.Empty);
			mMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, _diamonds: false);
			mItemButton = new GuiButton();
			GuiButton guiButton = mItemButton;
			guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton guiButton2 = mItemButton;
			guiButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			mItemButton.Init();
		}

		public override void SetSize()
		{
			mRewardTextRect = new Rect(17f, 13f, 479f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mRewardTextRect);
			mHonorTextRect = new Rect(11f, 34f, 246f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mHonorTextRect);
			mRaitingTextRect = new Rect(11f, 82f, 246f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingTextRect);
			mMoneyTextRect = new Rect(11f, 106f, 246f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mMoneyTextRect);
			mHonorValueRect = new Rect(260f, 34f, 246f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mHonorValueRect);
			mRaitingValueRect = new Rect(260f, 82f, 246f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingValueRect);
			mRaitingImgRect = new Rect(330f, 85f, 15f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingImgRect);
			mMoneyRenderer.SetSize(mZoneRect);
			mMoneyRenderer.SetOffset(new Vector2(260f, 108f) * GuiSystem.mYRate);
			SetMiscDataRects(mMapType);
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mItemData != null)
			{
				mItemButton.CheckEvent(_curEvent);
			}
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mRewardText, mRewardTextRect, "label");
			GuiSystem.DrawString(mYourRewardText, mYourRewardTextRect, "middle_right");
			GuiSystem.DrawString(mExpText, mExpTextRect, "middle_right");
			GuiSystem.DrawString(mExpValueText, mExpValueRect, "middle_left");
			GuiSystem.DrawString(mItemCntText, mItemCntTextRect, "middle_left");
			if (mMapType != MapType.HUNT)
			{
				GuiSystem.DrawString(mHonorText, mHonorTextRect, "middle_right");
				GuiSystem.DrawString(mHonorValueText, mHonorValueRect, "middle_left");
				GuiSystem.DrawString(mRaitingText, mRaitingTextRect, "middle_right");
				GuiSystem.DrawString(mRaitingValueText, mRaitingValueRect, "middle_left");
				if (mRaitingImg != null)
				{
					GuiSystem.DrawImage(mRaitingImg, mRaitingImgRect);
				}
				GuiSystem.DrawString(mMoneyText, mMoneyTextRect, "middle_right");
				mMoneyRenderer.Render();
			}
			if (mItemData != null)
			{
				mItemButton.RenderElement();
			}
		}

		public void SetData(BattleEndData _data, IStoreContentProvider<CtrlPrototype> _prototypes, FormatedTipMgr _tipMg, MapType _mapType)
		{
			mMapType = _mapType;
			mFormatedTipMgr = _tipMg;
			if (mMapType == MapType.HUNT)
			{
				mFrame = GuiSystem.GetImage("Gui/CompensationMenu/New/frame3");
			}
			else
			{
				mFrame = GuiSystem.GetImage("Gui/CompensationMenu/New/frame2");
			}
			mHonorValueText = _data.mHonor.ToString();
			mExpValueText = (_data.mAvatarKillsExp + _data.mAvatarLevelExp + _data.mAssistsExp + _data.mBattleExp).ToString();
			mRaitingValueText = _data.mNewRating.ToString();
			int num = _data.mNewRating - _data.mOldRating;
			if (num != 0)
			{
				mRaitingImg = ((num < 0) ? GuiSystem.GetImage("Gui/misc/arrow_0") : GuiSystem.GetImage("Gui/misc/arrow_1"));
				mRaitingValueText = mRaitingValueText + "(" + ((num < 0) ? num.ToString() : ("+" + num)) + ")";
			}
			else
			{
				mRaitingImg = null;
			}
			mMoneyRenderer.SetMoney(_data.mBattleMoney);
			int rewardItemId = GetRewardItemId(_data.mItemsCount);
			if (rewardItemId != -1)
			{
				mItemData = _prototypes.TryGet(rewardItemId);
				mItemCntText = GuiSystem.GetLocaleText("Items_Count_For_Reward_Text");
				mItemCntText = mItemCntText.Replace("{COUNT}", _data.mItemsCount.ToString());
				mItemButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mItemData.Desc.mIcon);
				mItemButton.mId = mItemData.Id;
			}
			else
			{
				mItemData = null;
				mItemCntText = GuiSystem.GetLocaleText("Items_Not_Enought_Text");
				mItemButton.mIconImg = null;
				mItemButton.mId = -1;
			}
			SetMiscDataRects(mMapType);
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Show(null, mItemData, 999, 999, _sender.UId, true);
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(_sender.UId);
			}
		}

		private int GetRewardItemId(int _itemsCnt)
		{
			if (_itemsCnt >= 4 && _itemsCnt < 8)
			{
				return 318;
			}
			if (_itemsCnt >= 8 && _itemsCnt < 12)
			{
				return 319;
			}
			if (_itemsCnt >= 12 && _itemsCnt < 16)
			{
				return 320;
			}
			if (_itemsCnt >= 16)
			{
				return 321;
			}
			return -1;
		}

		private void SetMiscDataRects(MapType _mapType)
		{
			if (_mapType == MapType.HUNT)
			{
				mExpTextRect = new Rect(11f, 34f, 246f, 20f);
				GuiSystem.SetChildRect(mZoneRect, ref mExpTextRect);
				mExpValueRect = new Rect(260f, 34f, 246f, 20f);
				GuiSystem.SetChildRect(mZoneRect, ref mExpValueRect);
				mYourRewardTextRect = new Rect(11f, 64f, 246f, 47f);
				GuiSystem.SetChildRect(mZoneRect, ref mYourRewardTextRect);
				mItemCntTextRect = new Rect(321f, 64f, 176f, 47f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemCntTextRect);
				mItemButton.mZoneRect = new Rect(270f, 71f, 37f, 38f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemButton.mZoneRect);
			}
			else
			{
				mExpTextRect = new Rect(11f, 58f, 246f, 20f);
				GuiSystem.SetChildRect(mZoneRect, ref mExpTextRect);
				mExpValueRect = new Rect(260f, 58f, 246f, 20f);
				GuiSystem.SetChildRect(mZoneRect, ref mExpValueRect);
				mYourRewardTextRect = new Rect(11f, 139f, 246f, 47f);
				GuiSystem.SetChildRect(mZoneRect, ref mYourRewardTextRect);
				mItemCntTextRect = new Rect(321f, 139f, 176f, 47f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemCntTextRect);
				mItemButton.mZoneRect = new Rect(270f, 144f, 37f, 38f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemButton.mZoneRect);
			}
		}
	}

	private class FullInfo : GuiElement
	{
		private class RewardItem : GuiElement
		{
			private FormatedTipMgr mFormatedTipMgr;

			private CtrlPrototype mItemData;

			private GuiButton mItemButton;

			private Rect mTextRect;

			private string mText;

			public override void Init()
			{
				mItemButton = new GuiButton();
				GuiButton guiButton = mItemButton;
				guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton guiButton2 = mItemButton;
				guiButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				mItemButton.Init();
				mText = GuiSystem.GetLocaleText("Items_Count_For_Reward_Text");
			}

			public override void SetSize()
			{
				mTextRect = new Rect(50f, 1f, 81f, 57f);
				GuiSystem.SetChildRect(mZoneRect, ref mTextRect);
				mItemButton.mZoneRect = new Rect(8f, 11f, 37f, 38f);
				GuiSystem.SetChildRect(mZoneRect, ref mItemButton.mZoneRect);
			}

			public override void RenderElement()
			{
				mItemButton.RenderElement();
				GuiSystem.DrawString(mText, mTextRect, "middle_center");
			}

			public override void CheckEvent(Event _curEvent)
			{
				mItemButton.CheckEvent(_curEvent);
			}

			public void SetData(CtrlPrototype _data, FormatedTipMgr _tipMg, int _cnt)
			{
				if (_data != null)
				{
					mItemData = _data;
					mFormatedTipMgr = _tipMg;
					mItemButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + _data.Desc.mIcon);
					mItemButton.mId = _data.Id;
					mText = mText.Replace("{COUNT}", _cnt.ToString());
				}
			}

			private void OnItemMouseEnter(GuiElement _sender)
			{
				if (mFormatedTipMgr != null)
				{
					mFormatedTipMgr.Show(null, mItemData, 999, 999, _sender.UId, true);
				}
			}

			private void OnItemMouseLeave(GuiElement _sender)
			{
				if (mFormatedTipMgr != null)
				{
					mFormatedTipMgr.Hide(_sender.UId);
				}
			}
		}

		private Texture2D mFrame;

		private Texture2D mFrameSelect;

		private Texture2D mRaitingImg;

		private MoneyRenderer mMoneyRenderer;

		private string mBattleResultText;

		private string mBattleRewardText;

		private string mKillsRewardText;

		private string mKillsFullRewardText;

		private string mKillsResultText;

		private string mHonorText;

		private string mHonorResultText;

		private string mRaitingText;

		private string mRaitingResultText;

		private string mMoneyText;

		private string mAvatarRewardText;

		private string mAvatarFullRewardText;

		private string mAvatarResultText;

		private string mExpText;

		private string mBattleExpText;

		private string mKillsExpText;

		private string mAvatarExpText;

		private string mItemsCollectedText;

		private string mItemsCollectedResultText;

		private string mYourRewardText;

		private string mRewardLabelText;

		private Rect mBattleResultTextRect;

		private Rect mBattleRewardTextRect;

		private Rect mKillsRewardTextRect;

		private Rect mKillsResultTextRect;

		private Rect mAvatarRewardTextRect;

		private Rect mAvatarResultTextRect;

		private Rect mBattleExpTextRect;

		private Rect mKillsExpTextRect;

		private Rect mAvatarExpTextRect;

		private Rect mHonorResultTextRect;

		private Rect mRaitingResultTextRect;

		private Rect mRaitingImgRect;

		private Rect mMoneyTextRect;

		private Rect mItemsCollectedResultTextRect;

		private Rect mFrameSelectRect;

		private Rect mYourRewardTextRect;

		private Rect mRewardLabelTextRect;

		private List<RewardItem> mRewardItems;

		private int mRewardNum;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/CompensationMenu/New/frame4");
			mFrameSelect = GuiSystem.GetImage("Gui/CompensationMenu/New/frame5");
			mBattleRewardText = GuiSystem.GetLocaleText("Battle_Reward_Text");
			mKillsRewardText = GuiSystem.GetLocaleText("Kills_Reward_Text");
			mAvatarRewardText = GuiSystem.GetLocaleText("Avatar_Reward_Text");
			mKillsFullRewardText = GuiSystem.GetLocaleText("Kills_Full_Reward_Text");
			mAvatarFullRewardText = GuiSystem.GetLocaleText("Avatar_Full_Reward_Text");
			mExpText = GuiSystem.GetLocaleText("Exp_Text");
			mHonorText = GuiSystem.GetLocaleText("Honor_Text");
			mRaitingText = GuiSystem.GetLocaleText("Raiting_Text");
			mMoneyText = GuiSystem.GetLocaleText("Money_Text");
			mItemsCollectedText = GuiSystem.GetLocaleText("Items_Collected_Text");
			mYourRewardText = GuiSystem.GetLocaleText("Yout_Reward_Text");
			mRewardLabelText = GuiSystem.GetLocaleText("Reward_Items_Label_Text");
			mMoneyRenderer = new MoneyRenderer(_renderMoneyImage: true, _diamonds: false);
			mRewardNum = -1;
			InitRewardItems();
		}

		public override void SetSize()
		{
			mBattleResultTextRect = new Rect(5f, 46f, 172f, 58f);
			GuiSystem.SetChildRect(mZoneRect, ref mBattleResultTextRect);
			mBattleRewardTextRect = new Rect(5f, 5f, 172f, 41f);
			GuiSystem.SetChildRect(mZoneRect, ref mBattleRewardTextRect);
			mKillsRewardTextRect = new Rect(187f, 5f, 172f, 41f);
			GuiSystem.SetChildRect(mZoneRect, ref mKillsRewardTextRect);
			mAvatarRewardTextRect = new Rect(369f, 5f, 172f, 41f);
			GuiSystem.SetChildRect(mZoneRect, ref mAvatarRewardTextRect);
			mKillsResultTextRect = new Rect(187f, 46f, 172f, 58f);
			GuiSystem.SetChildRect(mZoneRect, ref mKillsResultTextRect);
			mAvatarResultTextRect = new Rect(369f, 46f, 172f, 58f);
			GuiSystem.SetChildRect(mZoneRect, ref mAvatarResultTextRect);
			mBattleExpTextRect = new Rect(5f, 130f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mBattleExpTextRect);
			mKillsExpTextRect = new Rect(187f, 130f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mKillsExpTextRect);
			mAvatarExpTextRect = new Rect(369f, 109f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mAvatarExpTextRect);
			mHonorResultTextRect = new Rect(187f, 109f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mHonorResultTextRect);
			mRaitingResultTextRect = new Rect(5f, 109f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingResultTextRect);
			mRaitingImgRect = new Rect(160f, 110f, 15f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mRaitingImgRect);
			mMoneyTextRect = new Rect(10f, 151f, 172f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mMoneyTextRect);
			mItemsCollectedResultTextRect = new Rect(5f, 208f, 536f, 41f);
			GuiSystem.SetChildRect(mZoneRect, ref mItemsCollectedResultTextRect);
			mRewardLabelTextRect = new Rect(5f, 185f, 536f, 16f);
			GuiSystem.SetChildRect(mZoneRect, ref mRewardLabelTextRect);
			mMoneyRenderer.SetSize(mZoneRect);
			mMoneyRenderer.SetOffset(new Vector2(70f, 151f) * GuiSystem.mYRate);
			SetRewardItemsSize();
			SetFrameSelectRect(mRewardNum);
		}

		public override void CheckEvent(Event _curEvent)
		{
			foreach (RewardItem mRewardItem in mRewardItems)
			{
				mRewardItem.CheckEvent(_curEvent);
			}
			base.CheckEvent(_curEvent);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawString(mBattleRewardText, mBattleRewardTextRect, "middle_center");
			GuiSystem.DrawString(mBattleResultText, mBattleResultTextRect, "middle_center");
			GuiSystem.DrawString(mBattleExpText, mBattleExpTextRect, "middle_center");
			GuiSystem.DrawString(mRaitingResultText, mRaitingResultTextRect, "middle_center");
			GuiSystem.DrawString(mMoneyText, mMoneyTextRect, "middle_left");
			GuiSystem.DrawImage(mRaitingImg, mRaitingImgRect);
			mMoneyRenderer.Render();
			GuiSystem.DrawString(mKillsRewardText, mKillsRewardTextRect, "middle_center");
			GuiSystem.DrawString(mKillsResultText, mKillsResultTextRect, "middle_center");
			GuiSystem.DrawString(mKillsExpText, mKillsExpTextRect, "middle_center");
			GuiSystem.DrawString(mHonorResultText, mHonorResultTextRect, "middle_center");
			GuiSystem.DrawString(mAvatarRewardText, mAvatarRewardTextRect, "middle_center");
			GuiSystem.DrawString(mAvatarResultText, mAvatarResultTextRect, "middle_center");
			GuiSystem.DrawString(mAvatarExpText, mAvatarExpTextRect, "middle_center");
			GuiSystem.DrawString(mRewardLabelText, mRewardLabelTextRect, "label");
			GuiSystem.DrawString(mItemsCollectedResultText, mItemsCollectedResultTextRect, "middle_center");
			foreach (RewardItem mRewardItem in mRewardItems)
			{
				mRewardItem.RenderElement();
			}
			if (mRewardNum != -1)
			{
				GuiSystem.DrawImage(mFrameSelect, mFrameSelectRect);
				GuiSystem.DrawString(mYourRewardText, mYourRewardTextRect, "middle_center");
			}
		}

		public void SetData(BattleEndData _data, IStoreContentProvider<CtrlPrototype> _prototypes, FormatedTipMgr _tipMg)
		{
			if (_data == null || _prototypes == null)
			{
				return;
			}
			mBattleResultText = ((_data.mFinishType != 1) ? GuiSystem.GetLocaleText("Battle_Lose_Text") : GuiSystem.GetLocaleText("Battle_Win_Text"));
			mKillsResultText = mKillsFullRewardText;
			mKillsResultText = mKillsResultText.Replace("{KILLS}", _data.mKill.ToString());
			mKillsResultText = mKillsResultText.Replace("{ASSISTS}", _data.mKillAssist.ToString());
			mAvatarResultText = mAvatarFullRewardText;
			mAvatarResultText = mAvatarResultText.Replace("{LEVEL}", (_data.mAvaLvl + 1).ToString());
			mAvatarExpText = (mKillsExpText = (mBattleExpText = mExpText));
			mBattleExpText = mBattleExpText.Replace("{EXP}", _data.mBattleExp.ToString());
			mKillsExpText = mKillsExpText.Replace("{EXP}", (_data.mAvatarKillsExp + _data.mAssistsExp).ToString());
			mAvatarExpText = mAvatarExpText.Replace("{EXP}", _data.mAvatarLevelExp.ToString());
			mHonorResultText = mHonorText;
			mHonorResultText = mHonorResultText.Replace("{HONOR}", _data.mHonor.ToString());
			mRaitingResultText = mRaitingText;
			mRaitingResultText = mRaitingResultText.Replace("{RAITING}", _data.mNewRating.ToString());
			int num = _data.mNewRating - _data.mOldRating;
			if (num != 0)
			{
				mRaitingImg = ((num < 0) ? GuiSystem.GetImage("Gui/misc/arrow_0") : GuiSystem.GetImage("Gui/misc/arrow_1"));
				mRaitingResultText = mRaitingResultText.Replace("{DELTA_RAITING}", (num < 0) ? num.ToString() : ("+" + num));
			}
			else
			{
				mRaitingImg = null;
				mRaitingResultText = mRaitingResultText.Replace("({DELTA_RAITING})", string.Empty);
			}
			mMoneyRenderer.SetMoney(_data.mBattleMoney);
			mItemsCollectedResultText = mItemsCollectedText;
			mItemsCollectedResultText = mItemsCollectedResultText.Replace("{COUNT}", _data.mItemsCount.ToString());
			if (_data.mItemsCount < 4)
			{
				mRewardNum = -1;
			}
			else if (_data.mItemsCount >= 4 && _data.mItemsCount < 8)
			{
				mRewardNum = 0;
			}
			else if (_data.mItemsCount >= 8 && _data.mItemsCount < 12)
			{
				mRewardNum = 1;
			}
			else if (_data.mItemsCount >= 12 && _data.mItemsCount < 16)
			{
				mRewardNum = 2;
			}
			else if (_data.mItemsCount >= 16)
			{
				mRewardNum = 3;
			}
			if (mRewardNum == -1)
			{
				mItemsCollectedResultText += "\n";
				mItemsCollectedResultText += GuiSystem.GetLocaleText("Items_Not_Enought_Text");
			}
			SetFrameSelectRect(mRewardNum);
			int num2 = 0;
			CtrlPrototype ctrlPrototype = null;
			foreach (RewardItem mRewardItem in mRewardItems)
			{
				ctrlPrototype = _prototypes.TryGet(318 + num2);
				mRewardItem.SetData(ctrlPrototype, _tipMg, 4 + num2 * 4);
				num2++;
			}
		}

		private void InitRewardItems()
		{
			mRewardItems = new List<RewardItem>();
			RewardItem rewardItem = null;
			for (int i = 4; i <= 16; i += 4)
			{
				rewardItem = new RewardItem();
				rewardItem.Init();
				mRewardItems.Add(rewardItem);
			}
		}

		private void SetRewardItemsSize()
		{
			int num = 0;
			foreach (RewardItem mRewardItem in mRewardItems)
			{
				mRewardItem.mZoneRect = new Rect(6 + num * 134, 270f, 132f, 59f);
				GuiSystem.SetChildRect(mZoneRect, ref mRewardItem.mZoneRect);
				mRewardItem.SetSize();
				num++;
			}
		}

		private void SetFrameSelectRect(int _rewardNum)
		{
			mFrameSelectRect = new Rect(6 + _rewardNum * 134, 249f, 132f, 80f);
			GuiSystem.SetChildRect(mZoneRect, ref mFrameSelectRect);
			mYourRewardTextRect = new Rect(1f, 1f, 130f, 19f);
			GuiSystem.SetChildRect(mFrameSelectRect, ref mYourRewardTextRect);
		}
	}

	private Texture2D mFrame1;

	private GuiButton mCloseButton1;

	private GuiButton mCloseButton2;

	private GuiButton mTabButton1;

	private GuiButton mTabButton2;

	private TabType mCurTabType;

	private FullInfo mFullInfo;

	private ShortInfo mShortInfo;

	private string mCompensationLabelText;

	private Rect mCompensationLabelTextRect;

	private Texture2D mResultFrame;

	private Rect mResultFrameRect;

	private MapType mMapType;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(275, this);
		mFrame1 = GuiSystem.GetImage("Gui/CompensationMenu/New/frame1");
		mCloseButton1 = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton1.mElementId = "CLOSE_BUTTON_1";
		GuiButton guiButton = mCloseButton1;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton1.Init();
		AddTutorialElement(mCloseButton1);
		mCloseButton2 = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mCloseButton2.mElementId = "CLOSE_BUTTON_2";
		GuiButton guiButton2 = mCloseButton2;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton2.mLabel = GuiSystem.GetLocaleText("Close_Button_Name");
		mCloseButton2.Init();
		AddTutorialElement(mCloseButton2);
		mTabButton1 = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mTabButton1.mId = 1;
		mTabButton1.mElementId = "TAB_BUTTON";
		GuiButton guiButton3 = mTabButton1;
		guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
		mTabButton1.mLabel = GuiSystem.GetLocaleText("Short_Info_Text");
		mTabButton1.Init();
		mTabButton2 = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mTabButton2.mId = 2;
		mTabButton2.mElementId = "TAB_BUTTON";
		GuiButton guiButton4 = mTabButton2;
		guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
		mTabButton2.mLabel = GuiSystem.GetLocaleText("Full_Info_Text");
		mTabButton2.Init();
		mFullInfo = new FullInfo();
		mFullInfo.Init();
		mShortInfo = new ShortInfo();
		mShortInfo.Init();
		mCompensationLabelText = GuiSystem.GetLocaleText("Compensation_Label_Text");
		SetCurTab(TabType.SHORT_INFO);
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 100f, mFrame1.width, mFrame1.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton1.mZoneRect = new Rect(562f, 6f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton1.mZoneRect);
		mCloseButton2.mZoneRect = new Rect(244f, 468f, 119f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton2.mZoneRect);
		mTabButton1.mZoneRect = new Rect(14f, 40f, 207f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mTabButton1.mZoneRect);
		mTabButton2.mZoneRect = new Rect(210f, 40f, 207f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mTabButton2.mZoneRect);
		mFullInfo.mZoneRect = new Rect(26f, 71f, 546f, 336f);
		GuiSystem.SetChildRect(mZoneRect, ref mFullInfo.mZoneRect);
		mFullInfo.SetSize();
		mShortInfo.mZoneRect = new Rect(41f, 204f, 513f, 210f);
		GuiSystem.SetChildRect(mZoneRect, ref mShortInfo.mZoneRect);
		mShortInfo.SetSize();
		mCompensationLabelTextRect = new Rect(26f, 10f, 536f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mCompensationLabelTextRect);
		mResultFrameRect = new Rect(36f, 82f, 523f, 117f);
		GuiSystem.SetChildRect(mZoneRect, ref mResultFrameRect);
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton1.CheckEvent(_curEvent);
		mCloseButton2.CheckEvent(_curEvent);
		mTabButton1.CheckEvent(_curEvent);
		if (mMapType != MapType.HUNT)
		{
			mTabButton2.CheckEvent(_curEvent);
		}
		if (mCurTabType == TabType.FULL_INFO)
		{
			mFullInfo.CheckEvent(_curEvent);
		}
		else if (mCurTabType == TabType.SHORT_INFO)
		{
			mShortInfo.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame1, mZoneRect);
		GuiSystem.DrawString(mCompensationLabelText, mCompensationLabelTextRect, "label");
		mCloseButton1.RenderElement();
		mCloseButton2.RenderElement();
		mTabButton1.RenderElement();
		if (mMapType != MapType.HUNT)
		{
			mTabButton2.RenderElement();
		}
		if (mCurTabType == TabType.FULL_INFO)
		{
			mFullInfo.RenderElement();
		}
		else if (mCurTabType == TabType.SHORT_INFO)
		{
			GuiSystem.DrawImage(mResultFrame, mResultFrameRect);
			mShortInfo.RenderElement();
		}
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void SetData(BattleEndData _data, IStoreContentProvider<CtrlPrototype> _prototypes, FormatedTipMgr _tipMg, int _race, MapType _mapType)
	{
		mMapType = _mapType;
		mShortInfo.SetData(_data, _prototypes, _tipMg, mMapType);
		SetCurTab(TabType.SHORT_INFO);
		bool flag = _data.mFinishType == 1;
		if (mMapType == MapType.HUNT)
		{
			mResultFrame = GuiSystem.GetImage("Gui/CompensationMenu/new/battle_hunt");
			return;
		}
		mResultFrame = GuiSystem.GetImage("Gui/CompensationMenu/new/battle_" + _race + "_" + (flag ? 1 : 0));
		mFullInfo.SetData(_data, _prototypes, _tipMg);
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ((_sender.mElementId == "CLOSE_BUTTON_1" || _sender.mElementId == "CLOSE_BUTTON_2") && _buttonId == 0)
		{
			Close();
		}
		else if (_sender.mElementId == "TAB_BUTTON")
		{
			SetCurTab((TabType)_sender.mId);
		}
	}

	private void SetCurTab(TabType _type)
	{
		mCurTabType = _type;
		mTabButton1.Pressed = mTabButton1.mId == (int)mCurTabType;
		mTabButton2.Pressed = mTabButton2.mId == (int)mCurTabType;
	}
}
