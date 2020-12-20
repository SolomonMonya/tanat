using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class RewardInfo : GuiElement, EscapeListener
{
	private class RewardElement
	{
		public GuiButton mItem;

		private int mCount;

		public Rect mCountRect;

		public Rect mDayRect;

		public Color mCountColor;

		private string mDayText;

		private string mCountText;

		public int Day
		{
			set
			{
				mDayText = string.Format("{0} {1}", value, GuiSystem.GetLocaleText("DAY_TEXT"));
			}
		}

		public int Count
		{
			set
			{
				mCount = value;
				mCountText = mCount.ToString();
			}
		}

		public void DrawReward()
		{
			mItem.RenderElement();
			if (mCount > 0)
			{
				GUI.contentColor = mCountColor;
				GuiSystem.DrawString(mCountText, mCountRect, "middle_center_11_bold");
			}
			GUI.contentColor = new Color(47f / 255f, 142f / (339f * (float)Math.PI), 0.117647059f);
			GuiSystem.DrawString(mDayText, mDayRect, "middle_center");
		}

		public void CheckEvent(Event _curEvent)
		{
			mItem.CheckEvent(_curEvent);
		}
	}

	private Texture2D mFrame;

	private GuiButton mCloseButton;

	private GuiButton mCancelButton;

	private Rect mCurrentRect;

	private Rect mMainRect;

	private Rect mMainRect2;

	private Rect mCaptionRect;

	private Rect mCongRect;

	private string mCurrentText;

	private string mMainText;

	private string mMainText2;

	private string mCaptionText;

	private string mCongText;

	private List<RewardElement> mItems = new List<RewardElement>();

	private RewardElement mCurrent;

	private IStoreContentProvider<CtrlPrototype> mProtoProvider;

	private FormatedTipMgr mFormatedTipMgr;

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
		mFrame = GuiSystem.GetImage("Gui/DailyReward/Frame_1");
		GuiSystem.mGuiInputMgr.AddEscapeListener(550, this);
		mCloseButton = GuiSystem.CreateButton("Gui/SelectGameMenu/button_3_norm", "Gui/SelectGameMenu/button_3_over", "Gui/SelectGameMenu/button_3_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE_BUTTON";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		AddTutorialElement(mCloseButton.mElementId, mCloseButton);
		mCancelButton = GuiSystem.CreateButton("Gui/SelectGameMenu/button_2_norm", "Gui/SelectGameMenu/button_2_over", "Gui/SelectGameMenu/button_2_press", string.Empty, string.Empty);
		mCancelButton.mElementId = "CANCEL_BUTTON";
		GuiButton guiButton2 = mCancelButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mCancelButton.mLabel = GuiSystem.GetLocaleText("Close_Button_Name");
		mCancelButton.mNormColor = new Color(48f / 85f, 191f / 255f, 194f / 255f);
		mCancelButton.mLabelStyle = "middle_center_18";
		mCancelButton.Init();
		AddTutorialElement(mCancelButton.mElementId, mCancelButton);
		mCurrentText = GuiSystem.GetLocaleText("GUI_CURRENT_TEXT");
		mMainText = GuiSystem.GetLocaleText("GUI_REWARD_TEXT");
		mMainText2 = GuiSystem.GetLocaleText("GUI_REWARD_TEXT2");
		mCaptionText = GuiSystem.GetLocaleText("GUI_REWARD_CAPTION");
		mCongText = GuiSystem.GetLocaleText("GUI_REWARD_CONG");
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 100f, mFrame.width, mFrame.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(455f, 3f, 32f, 32f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mCancelButton.mZoneRect = new Rect(146f, 433f, 200f, 50f);
		GuiSystem.SetChildRect(mZoneRect, ref mCancelButton.mZoneRect);
		mCurrentRect = new Rect(45f, 83f, 405f, 20f);
		mMainRect = new Rect(45f, 235f, 405f, 20f);
		mMainRect2 = new Rect(45f, 250f, 405f, 20f);
		mCaptionRect = new Rect(45f, 7f, 405f, 25f);
		mCongRect = new Rect(45f, 60f, 405f, 25f);
		GuiSystem.SetChildRect(mZoneRect, ref mCurrentRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMainRect);
		GuiSystem.SetChildRect(mZoneRect, ref mMainRect2);
		GuiSystem.SetChildRect(mZoneRect, ref mCaptionRect);
		GuiSystem.SetChildRect(mZoneRect, ref mCongRect);
		SetItemsSize();
	}

	private void SetItemsSize()
	{
		if (mItems.Count == 0)
		{
			return;
		}
		for (int i = 0; i < mItems.Count; i++)
		{
			if (mItems[i].mItem != null)
			{
				mItems[i].mItem.mZoneRect = new Rect(33 + i * mItems[i].mItem.mCurImg.width, 285f, mItems[i].mItem.mCurImg.width, mItems[i].mItem.mCurImg.height);
				GuiSystem.SetChildRect(mZoneRect, ref mItems[i].mItem.mZoneRect);
				mItems[i].mCountRect = new Rect(53 + i * mItems[i].mItem.mCurImg.width, 378f, 40f, 20f);
				mItems[i].mDayRect = new Rect(25 + i * mItems[i].mItem.mCurImg.width, 295f, 100f, 20f);
				GuiSystem.SetChildRect(mZoneRect, ref mItems[i].mCountRect);
				GuiSystem.SetChildRect(mZoneRect, ref mItems[i].mDayRect);
			}
		}
		if (mCurrent != null)
		{
			mCurrent.mItem.mZoneRect = new Rect(206f, 100f, mCurrent.mItem.mCurImg.width, mCurrent.mItem.mCurImg.height);
			GuiSystem.SetChildRect(mZoneRect, ref mCurrent.mItem.mZoneRect);
			mCurrent.mCountRect = new Rect(232f, 193f, 30f, 20f);
			mCurrent.mDayRect = new Rect(197f, 110f, 100f, 20f);
			GuiSystem.SetChildRect(mZoneRect, ref mCurrent.mCountRect);
			GuiSystem.SetChildRect(mZoneRect, ref mCurrent.mDayRect);
		}
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame, mZoneRect);
		GUI.contentColor = Color.white;
		GUI.contentColor = new Color(41f / 85f, 32f / 51f, 57f / 85f);
		GuiSystem.DrawString(mCaptionText, mCaptionRect, "label_bold");
		GUI.contentColor = new Color(101f / 255f, 23f / 255f, 11f / 255f);
		GuiSystem.DrawString(mCongText, mCongRect, "label");
		GUI.contentColor = new Color(42f / 85f, 43f / 255f, 2f / 255f);
		GuiSystem.DrawString(mCurrentText, mCurrentRect, "middle_center_11_bold");
		GuiSystem.DrawString(mMainText, mMainRect, "middle_center_11_bold");
		GuiSystem.DrawString(mMainText2, mMainRect2, "middle_center_11_bold");
		mCloseButton.RenderElement();
		mCancelButton.RenderElement();
		for (int i = 0; i < mItems.Count; i++)
		{
			mItems[i].DrawReward();
		}
		mCurrent.DrawReward();
		GUI.contentColor = Color.white;
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		mCancelButton.CheckEvent(_curEvent);
		base.CheckEvent(_curEvent);
	}

	public void Open()
	{
		SetActive(_active: true);
	}

	public void Close()
	{
		SetActive(_active: false);
	}

	public void SetData(IStoreContentProvider<CtrlPrototype> _provider, FormatedTipMgr _formatedTipMgr)
	{
		if (_provider == null)
		{
			throw new ArgumentNullException("_provider");
		}
		if (_formatedTipMgr == null)
		{
			throw new ArgumentNullException("_formatedTipMgr");
		}
		mFormatedTipMgr = _formatedTipMgr;
		mProtoProvider = _provider;
	}

	public void SetData(List<OneDayReward> _data, int _current)
	{
		mCurrent = new RewardElement();
		mCurrent.Count = _data[_current].mCount;
		mCurrent.Day = _current + 1;
		mCurrent.mCountColor = new Color(179f / 255f, 14f / 15f, 1f);
		if (_current == _data.Count - 1)
		{
			mCurrent.Count = 0;
		}
		mCurrent.mItem = CreateItemBtn(_data[_current].mItemId);
		mItems.Clear();
		for (int i = 0; i < _data.Count; i++)
		{
			OneDayReward oneDayReward = _data[i];
			RewardElement rewardElement = new RewardElement();
			rewardElement.Count = oneDayReward.mCount;
			if (i == _data.Count - 1)
			{
				rewardElement.Count = 0;
			}
			rewardElement.Day = i + 1;
			rewardElement.mItem = CreateItemBtn(oneDayReward.mItemId);
			if (rewardElement.mItem != null)
			{
				Texture2D image;
				Texture2D mIconImg;
				if (i < _current)
				{
					image = GuiSystem.GetImage("Gui/DailyReward/frame4");
					mIconImg = GuiSystem.GetImage("Gui/DailyReward/diamond_2");
					rewardElement.mCountColor = new Color(0.5882353f, 197f / 255f, 0.8235294f);
				}
				else if (i > _current)
				{
					rewardElement.mCountColor = new Color(172f / 255f, 167f / 255f, 154f / 255f);
					image = GuiSystem.GetImage("Gui/DailyReward/frame2");
					mIconImg = ((i != _data.Count - 1) ? GuiSystem.GetImage("Gui/DailyReward/diamond_3") : GuiSystem.GetImage("Gui/DailyReward/box3"));
				}
				else
				{
					image = GuiSystem.GetImage("Gui/DailyReward/frame3");
					mIconImg = ((i != _data.Count - 1) ? GuiSystem.GetImage("Gui/DailyReward/diamond_1") : GuiSystem.GetImage("Gui/DailyReward/box1"));
					mCurrent.mItem.mCurImg = image;
					mCurrent.mItem.mIconImg = mIconImg;
					rewardElement.mCountColor = new Color(179f / 255f, 14f / 15f, 1f);
				}
				rewardElement.mItem.mCurImg = image;
				rewardElement.mItem.mIconImg = mIconImg;
				mItems.Add(rewardElement);
			}
		}
		SetItemsSize();
	}

	private GuiButton CreateItemBtn(int _protoId)
	{
		GuiButton guiButton = new GuiButton();
		guiButton.mElementId = "REWARD_BUTTON";
		guiButton.mId = _protoId;
		if (guiButton.mIconImg == null)
		{
			guiButton.mIconImg = GuiSystem.GetImage("Gui/misc/star");
		}
		guiButton.Init();
		return guiButton;
	}

	private void OnTipMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && !(_sender.mElementId != "REWARD_BUTTON"))
		{
			CtrlPrototype article = mProtoProvider.Get(_sender.mId);
			mFormatedTipMgr.Show(null, article, 20, 75, _sender.UId, null);
		}
	}

	private void OnTipMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if ((_sender.mElementId == "CLOSE_BUTTON" || _sender.mElementId == "CANCEL_BUTTON") && _buttonId == 0)
		{
			Close();
		}
	}
}
