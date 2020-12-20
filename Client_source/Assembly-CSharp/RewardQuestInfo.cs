using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class RewardQuestInfo : GuiElement, EscapeListener
{
	public GuiButton mRewardItem;

	public Rect mExpStringRect;

	public Rect mRewardDescRect;

	public Rect mRewardStringRect;

	public MoneyRenderer mRewardMoney;

	private string mRewardItemCount;

	private string mExpString;

	private string mRewardDesc;

	private string mRewardString;

	private CtrlPrototype mRewardItemArticul;

	private FormatedTipMgr mFormatedTipMgr;

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
		GuiSystem.mGuiInputMgr.AddEscapeListener(225, this);
		mRewardItem = new GuiButton();
		GuiButton guiButton = mRewardItem;
		guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
		GuiButton guiButton2 = mRewardItem;
		guiButton2.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton2.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
		mRewardItem.Init();
		mRewardString = GuiSystem.GetLocaleText("Quest_Reward_Text");
	}

	public override void SetSize()
	{
		GuiSystem.SetChildRect(mZoneRect, ref mExpStringRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRewardDescRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRewardStringRect);
		GuiSystem.SetChildRect(mZoneRect, ref mRewardItem.mZoneRect);
	}

	public override void RenderElement()
	{
		GuiSystem.DrawString(mRewardString, mRewardStringRect, "middle_center");
		if (mRewardDesc != string.Empty)
		{
			GuiSystem.DrawString(mRewardDesc, mRewardDescRect, "middle_left");
		}
		if (mExpString != string.Empty)
		{
			GuiSystem.DrawString(mExpString, mExpStringRect, "middle_left");
		}
		if (mRewardMoney != null)
		{
			mRewardMoney.Render();
		}
		mRewardItem.RenderElement();
		GuiSystem.DrawString(mRewardItemCount, mRewardItem.mZoneRect, "lower_right");
	}

	public override void CheckEvent(Event _curEvent)
	{
		mRewardItem.CheckEvent(_curEvent);
	}

	public void SetData(IQuest _questData, SelfHero _selfHero, FormatedTipMgr _tipMg, bool _pvp)
	{
		if (_questData == null || _selfHero == null)
		{
			return;
		}
		mFormatedTipMgr = _tipMg;
		Hero hero = _selfHero.Hero;
		if (!string.IsNullOrEmpty(_questData.RewardDesc))
		{
			mRewardDesc = GuiSystem.GetLocaleText(_questData.RewardDesc);
		}
		if (_questData.Money > 0)
		{
			mRewardMoney = new MoneyRenderer(_renderMoneyImage: true, _questData.MoneyCurrency == Currency.REAL);
			mRewardMoney.SetMoney(_questData.Money);
		}
		if (_questData.Exp != 0)
		{
			mExpString = _questData.Exp + " " + GuiSystem.GetLocaleText("Quest_Reward_Exp_Text");
		}
		mRewardItemCount = string.Empty;
		mRewardItemArticul = null;
		mRewardItem.mIconImg = null;
		ICollection<IQuestReward> collection = null;
		if (_pvp)
		{
			collection = _questData.Rewards;
		}
		else if (hero != null)
		{
			ICollection<IQuestReward> collection2;
			if (hero.View.mRace == 1)
			{
				ICollection<IQuestReward> rewardsHuman = _questData.RewardsHuman;
				collection2 = rewardsHuman;
			}
			else
			{
				collection2 = _questData.RewardsElf;
			}
			collection = collection2;
		}
		if (collection == null)
		{
			return;
		}
		using IEnumerator<IQuestReward> enumerator = collection.GetEnumerator();
		if (enumerator.MoveNext())
		{
			IQuestReward current = enumerator.Current;
			if (current != null && current.ArticleProto != null && current.ArticleProto.Desc != null)
			{
				mRewardItemArticul = current.ArticleProto;
				mRewardItem.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + current.ArticleProto.Desc.mIcon);
				mRewardItem.mId = current.ArticleProto.Id;
				mRewardItemCount = current.Count.ToString();
			}
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && mRewardItemArticul != null)
		{
			mFormatedTipMgr.Show(null, mRewardItemArticul, 999, 999, _sender.UId, true);
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
