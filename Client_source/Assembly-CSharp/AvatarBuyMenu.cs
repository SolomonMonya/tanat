using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class AvatarBuyMenu : GuiElement
{
	private class BuyParam
	{
		public string mText;

		public string mValue;

		public Color mTextColor;

		public Rect mDrawRect;
	}

	private enum BuyAvatarParams
	{
		LEVEL = 1,
		HERO_RATING,
		WINS,
		LOSE,
		AVATARKILLS,
		DEATHS,
		ASSISTS,
		FIGHTS,
		TIME_HUNT,
		TIME_DOTA,
		CREEPKILLS,
		ARTIFACT
	}

	public delegate void Buy(Dictionary<int, int> _basket);

	private Texture2D mFrame1Image;

	private GuiButton mCloseButton;

	private GuiButton mBuyButton;

	private string mGetText;

	private Rect mGetTextRect;

	private string mBuyText;

	private Rect mBuyTextRect;

	private List<BuyParam> mBuyParams;

	private AvatarData mAvatarData;

	private MapAvatarData mMapAvatarData;

	private CtrlPrototype mItemArticle;

	private Rect mMoneyRect;

	private Rect mMoneyValueRect;

	private Rect mMoneyImageRect;

	private string mMoneyString;

	private Texture2D mMoneyImage;

	public Buy mBuy;

	private IRealMoneyHolder mMoneyHolder;

	private bool mCantBuy;

	public override void Init()
	{
		mFrame1Image = GuiSystem.GetImage("Gui/AvatarBuyMenu/frame1");
		mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
		mCloseButton.mElementId = "CLOSE";
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
		mCloseButton.Init();
		mBuyButton = GuiSystem.CreateButton("Gui/misc/button_1_norm", "Gui/misc/button_1_over", "Gui/misc/button_1_press", string.Empty, string.Empty);
		mBuyButton.mElementId = "BUY";
		GuiButton guiButton2 = mBuyButton;
		guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
		mBuyButton.mLabel = GuiSystem.GetLocaleText("SHOP_BUTTON_BUY_TEXT");
		mBuyButton.Init();
		mGetText = GuiSystem.GetLocaleText("Get_Text");
		mBuyText = GuiSystem.GetLocaleText("Buy_Avatar_Text");
		mBuyParams = new List<BuyParam>();
		mCantBuy = false;
	}

	public override void Uninit()
	{
		mBuyParams.Clear();
		mAvatarData = null;
		mMapAvatarData = null;
		mCantBuy = false;
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 57f, 820f, 383f);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		mCloseButton.mZoneRect = new Rect(588f, 64f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mBuyButton.mZoneRect = new Rect(353f, 267f, 116f, 28f);
		GuiSystem.SetChildRect(mZoneRect, ref mBuyButton.mZoneRect);
		mGetTextRect = new Rect(353f, 102f, 116f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mGetTextRect);
		mBuyTextRect = new Rect(283f, 210f, 256f, 24f);
		GuiSystem.SetChildRect(mZoneRect, ref mBuyTextRect);
		mMoneyRect = new Rect(353f, 242f, 116f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyRect);
		mMoneyImageRect = new Rect(383f, 245f, 17f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyImageRect);
		mMoneyValueRect = new Rect(403f, 243f, 50f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mMoneyValueRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawImage(mFrame1Image, mZoneRect);
		GuiSystem.DrawString(mGetText, mGetTextRect, "middle_center");
		if (mBuyButton.mLocked)
		{
			GUI.contentColor = Color.red;
		}
		if (mCantBuy)
		{
			GuiSystem.DrawString(mMoneyString, mMoneyRect, "middle_center");
		}
		else
		{
			GuiSystem.DrawString(mBuyText, mBuyTextRect, "middle_center");
			GuiSystem.DrawString(mMoneyString, mMoneyValueRect, "middle_left");
			GuiSystem.DrawImage(mMoneyImage, mMoneyImageRect);
			mBuyButton.RenderElement();
		}
		foreach (BuyParam mBuyParam in mBuyParams)
		{
			GUI.contentColor = mBuyParam.mTextColor;
			GuiSystem.DrawString(mBuyParam.mText, mBuyParam.mDrawRect, "middle_left");
			GuiSystem.DrawString(mBuyParam.mValue, mBuyParam.mDrawRect, "middle_right");
		}
		GUI.contentColor = Color.white;
		mCloseButton.RenderElement();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		if (!mCantBuy)
		{
			mBuyButton.CheckEvent(_curEvent);
		}
		base.CheckEvent(_curEvent);
	}

	public void SetData(AvatarData _data, MapAvatarData _mapData, IRealMoneyHolder _moneyHolder, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes)
	{
		mMoneyHolder = _moneyHolder;
		mAvatarData = _data;
		mMapAvatarData = _mapData;
		mItemArticle = ((mAvatarData.mArticleId <= 0) ? null : _ctrlPrototypes.Get(mAvatarData.mArticleId));
		mBuyParams.Clear();
		mBuyButton.mLocked = !GetEnoughtMoney();
		InitAvatarBuyParams();
		InitCost();
	}

	public void Clean()
	{
		mMoneyHolder = null;
		mAvatarData = null;
		mItemArticle = null;
		mMapAvatarData = null;
	}

	private bool GetEnoughtMoney()
	{
		if (mItemArticle == null)
		{
			return false;
		}
		if (mItemArticle.Article == null)
		{
			return false;
		}
		if (mItemArticle.Article.mPriceType == 1)
		{
			return mItemArticle.Article.mBuyCost <= mMoneyHolder.VirtualMoney;
		}
		if (mItemArticle.Article.mPriceType == 2)
		{
			return mItemArticle.Article.mBuyCost <= mMoneyHolder.DiamondMoney;
		}
		return false;
	}

	private void InitAvatarBuyParams()
	{
		if (mMapAvatarData == null)
		{
			return;
		}
		BuyParam buyParam = null;
		int num = 0;
		int num2 = 0;
		if (string.IsNullOrEmpty(mAvatarData.mDescRestriction))
		{
			foreach (AvatarRestriction mRestriction in mMapAvatarData.mRestrictions)
			{
				num = mBuyParams.Count % 2;
				num2 = Mathf.FloorToInt(mBuyParams.Count / 2);
				buyParam = new BuyParam();
				buyParam.mText = GuiSystem.GetLocaleText("Avatar_Param_Text_" + mRestriction.mType);
				buyParam.mValue = mRestriction.mValue;
				buyParam.mTextColor = ((!mRestriction.mAllow) ? Color.red : Color.green);
				buyParam.mDrawRect = new Rect(264 + num * 165, 128 + num2 * 17, 135f, 14f);
				GuiSystem.SetChildRect(mZoneRect, ref buyParam.mDrawRect);
				mBuyParams.Add(buyParam);
			}
		}
		else
		{
			num = mBuyParams.Count % 2;
			num2 = Mathf.FloorToInt(mBuyParams.Count / 2);
			buyParam = new BuyParam();
			buyParam.mText = GuiSystem.GetLocaleText("Avatar_Param_Text_" + mAvatarData.mDescRestriction);
			buyParam.mTextColor = Color.red;
			buyParam.mDrawRect = new Rect(264 + num * 165, 148 + num2 * 17, 305f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref buyParam.mDrawRect);
			mBuyParams.Add(buyParam);
		}
	}

	private void InitCost()
	{
		if (mCantBuy = mItemArticle == null || mItemArticle.Article == null || !string.IsNullOrEmpty(mAvatarData.mDescRestriction))
		{
			mMoneyString = GuiSystem.GetLocaleText("Cant_Buy_Avatar_Text");
		}
		else if (mItemArticle.Article.mPriceType == 1)
		{
			mMoneyImage = GuiSystem.GetImage("Gui/misc/gold");
			mMoneyString = (mItemArticle.Article.mBuyCost / 10000).ToString();
		}
		else if (mItemArticle.Article.mPriceType == 2)
		{
			mMoneyImage = GuiSystem.GetImage("Gui/misc/diamond");
			mMoneyString = (mItemArticle.Article.mBuyCost / 100).ToString();
		}
	}

	private void OnButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0 && _sender.mElementId == "CLOSE")
		{
			SetActive(_active: false);
		}
		if (_buttonId == 0 && _sender.mElementId == "BUY" && mBuy != null)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			dictionary.Add(mAvatarData.mArticleId, 1);
			mBuy(dictionary);
			SetActive(_active: false);
		}
	}
}
