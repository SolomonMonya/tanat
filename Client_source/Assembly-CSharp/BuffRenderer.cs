using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class BuffRenderer : GuiElement
{
	private List<KeyValuePair<int, GuiButton>> mBuffs = new List<KeyValuePair<int, GuiButton>>();

	private Dictionary<int, DateTime> mBuffsTime = new Dictionary<int, DateTime>();

	private IStoreContentProvider<CtrlPrototype> mProtoProvider;

	private FormatedTipMgr mFormatedTipMgr;

	private HeroStore mHeroStore;

	private InstanceData mAvatarData;

	private bool mBuffsReInit;

	public Rect mBaseRect;

	private Dictionary<int, GuiButton> mBuffsBattle = new Dictionary<int, GuiButton>();

	public float mSize = 40f;

	public float mStartX;

	public float mStartY;

	public float mOffsetX = 45f;

	public float mOffsetY = -45f;

	public float mMaxCount = 16f;

	public float mRowSize = 8f;

	private Hero mHero;

	private Dictionary<int, DateTime> mConstBuffs;

	public override void Update()
	{
		if (mBuffsReInit)
		{
			CreateBuffs();
		}
		UpdateBuffTime();
	}

	public override void RenderElement()
	{
		foreach (KeyValuePair<int, GuiButton> mBuff in mBuffs)
		{
			mBuff.Value.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		foreach (KeyValuePair<int, GuiButton> mBuff in mBuffs)
		{
			mBuff.Value.CheckEvent(_curEvent);
		}
	}

	public bool ContainsBattleBuff(int _buffId)
	{
		return mBuffsBattle.ContainsKey(_buffId);
	}

	public void AddBattleBuff(int _buffId, BattlePrototype _data)
	{
		if (!mBuffsBattle.ContainsKey(_buffId))
		{
			GuiButton guiButton = CreateBattleBuffBtn(_data);
			if (guiButton != null)
			{
				mBuffsBattle.Add(_buffId, guiButton);
				mBuffsReInit = true;
			}
		}
	}

	public void RemoveBattleBuff(int _buffId)
	{
		if (!mBuffsBattle.ContainsKey(_buffId))
		{
			Log.Error("buff with id : " + _buffId + " doesn't exist");
			return;
		}
		mBuffsBattle.Remove(_buffId);
		mBuffsReInit = true;
	}

	public void SetBuffSize()
	{
		for (int i = 0; i < mBuffs.Count; i++)
		{
			SetPosition(mBuffs[i].Value, i);
		}
	}

	private void UpdateBuffTime()
	{
		List<KeyValuePair<int, GuiButton>> list = new List<KeyValuePair<int, GuiButton>>();
		foreach (KeyValuePair<int, GuiButton> mBuff in mBuffs)
		{
			if (!mBuffsTime.ContainsKey(mBuff.Key))
			{
				continue;
			}
			TimeSpan timeSpan = mBuffsTime[mBuff.Key] - DateTime.Now;
			if (timeSpan.Days > 0)
			{
				mBuff.Value.mLabel = timeSpan.Days + 1 + GuiSystem.GetLocaleText("IDS_DAY_STAY");
				continue;
			}
			if (timeSpan.Hours > 0)
			{
				mBuff.Value.mLabel = timeSpan.Hours + 1 + GuiSystem.GetLocaleText("IDS_HOUR_STAY");
				continue;
			}
			if (timeSpan.Minutes > 0)
			{
				mBuff.Value.mLabel = timeSpan.Minutes + 1 + GuiSystem.GetLocaleText("IDS_MINUTE_STAY");
				continue;
			}
			if (timeSpan.Seconds > 0)
			{
				mBuff.Value.mLabel = timeSpan.Seconds + GuiSystem.GetLocaleText("IDS_SECOND_STAY");
			}
			if (timeSpan.TotalSeconds <= 1.0)
			{
				list.Add(mBuff);
			}
		}
		foreach (KeyValuePair<int, GuiButton> item in list)
		{
			mBuffs.Remove(item);
		}
		if (list.Count > 0)
		{
			SetBuffSize();
		}
	}

	private void CreateBuffs()
	{
		Dictionary<int, GuiButton> dictionary = new Dictionary<int, GuiButton>();
		if (mHero != null)
		{
			CreateButtons(mHero.GameInfo.mBuffs, dictionary);
		}
		else
		{
			if (mConstBuffs == null)
			{
				return;
			}
			CreateButtons(mConstBuffs, dictionary);
		}
		if (mHeroStore != null && mHeroStore.mGlobalBuffs != null)
		{
			CreateButtons(mHeroStore.mGlobalBuffs, dictionary);
		}
		foreach (KeyValuePair<int, GuiButton> item in mBuffsBattle)
		{
			dictionary[item.Key] = item.Value;
		}
		List<int> current = new List<int>();
		KeyValuePair<int, GuiButton> btn;
		foreach (KeyValuePair<int, GuiButton> item2 in dictionary)
		{
			btn = item2;
			if (mBuffs.Exists((KeyValuePair<int, GuiButton> toSearch) => btn.Key == toSearch.Key))
			{
				current.Add(btn.Key);
				continue;
			}
			if (btn.Value != null)
			{
				mBuffs.Add(btn);
			}
			current.Add(btn.Key);
		}
		mBuffs.RemoveAll(delegate(KeyValuePair<int, GuiButton> toSearch)
		{
			foreach (int item3 in current)
			{
				if (item3 == toSearch.Key)
				{
					return false;
				}
			}
			return true;
		});
		SetBuffSize();
		mBuffsReInit = false;
	}

	private void CreateButtons(Dictionary<int, DateTime> _buffs, Dictionary<int, GuiButton> _result)
	{
		foreach (KeyValuePair<int, DateTime> _buff in _buffs)
		{
			mBuffsTime[_buff.Key] = _buff.Value;
			GuiButton guiButton = CreateBuffBtn(_buff.Key);
			if (guiButton != null)
			{
				_result[_buff.Key] = guiButton;
			}
		}
	}

	private void BuffsUpdated(HeroGameInfo _gameinfo)
	{
		mBuffsReInit = true;
	}

	private void OnGlobalReloaded()
	{
		mBuffsReInit = true;
	}

	private void SetPosition(GuiButton _btn, int _number)
	{
		int num = _number;
		int num2 = (int)((float)num / mRowSize);
		num = (int)((float)num - mRowSize * (float)num2);
		_btn.mZoneRect = new Rect(mStartX + mOffsetX * (float)num, mStartY + mOffsetY * (float)num2, mSize, mSize);
		GuiSystem.SetChildRect(mBaseRect, ref _btn.mZoneRect);
	}

	private GuiButton CreateBuffBtn(int _protoId)
	{
		CtrlPrototype ctrlPrototype = mProtoProvider.Get(_protoId);
		if (ctrlPrototype == null || ctrlPrototype.Desc == null)
		{
			return null;
		}
		GuiButton guiButton = CreateButton(_protoId, "Gui/Icons/Items/" + ctrlPrototype.Desc.mIcon);
		guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnBuffMouseEnter));
		guiButton.Init();
		return guiButton;
	}

	private GuiButton CreateBattleBuffBtn(BattlePrototype _data)
	{
		if (_data == null || _data.EffectDesc == null)
		{
			return null;
		}
		GuiButton guiButton = CreateButton(_data.Id, _data.EffectDesc.mDesc.mIcon);
		guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnBattleBuffMouseEnter));
		guiButton.Init();
		return guiButton;
	}

	private GuiButton CreateButton(int _id, string _icon)
	{
		GuiButton guiButton = new GuiButton();
		guiButton.mElementId = "BUFF_BUTTON";
		guiButton.mId = _id;
		guiButton.mIconImg = GuiSystem.GetImage(_icon);
		if (guiButton.mIconImg == null)
		{
			guiButton.mIconImg = GuiSystem.GetImage("Gui/misc/star");
		}
		guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnTipMouseLeave));
		guiButton.mNormColor = Color.blue;
		guiButton.mOverColor = Color.blue;
		guiButton.mPressColor = Color.blue;
		guiButton.mLabelStyle = "buff";
		return guiButton;
	}

	private void OnBattleBuffMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr == null)
		{
			return;
		}
		if (mAvatarData == null)
		{
			Log.Warning("mAvatarData is null");
			return;
		}
		Effector selfEffectorByProto = mAvatarData.GetSelfEffectorByProto(_sender.mId);
		if (selfEffectorByProto != null && _sender.mElementId == "BUFF_BUTTON")
		{
			mFormatedTipMgr.Show(selfEffectorByProto, FormatedTipMgr.TipType.SKILL_ACTIVE, mAvatarData.Level + 1, -1, _sender.UId);
		}
	}

	private void OnBuffMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr != null && !(_sender.mElementId != "BUFF_BUTTON"))
		{
			CtrlPrototype article = mProtoProvider.Get(_sender.mId);
			mFormatedTipMgr.Show(null, article, 20, 70, _sender.UId, null);
		}
	}

	private void OnTipMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	public void SetData(Hero _player, IStoreContentProvider<CtrlPrototype> _prov, FormatedTipMgr _formatedTipMgr, HeroStore _heroStore)
	{
		SetData(_prov, _formatedTipMgr, _heroStore);
		SetHero(_player);
	}

	public void SetAvatarData(InstanceData _data)
	{
		if (_data == null)
		{
			throw new ArgumentNullException("_data");
		}
		mAvatarData = _data;
	}

	public void SetData(IStoreContentProvider<CtrlPrototype> _prov, FormatedTipMgr _formatedTipMgr, HeroStore _heroStore, Dictionary<int, DateTime> _buffs)
	{
		mConstBuffs = _buffs;
		SetData(_prov, _formatedTipMgr, _heroStore);
	}

	public void SetData(IStoreContentProvider<CtrlPrototype> _prov, FormatedTipMgr _formatedTipMgr, HeroStore _heroStore)
	{
		SetData(_prov, _heroStore);
		SetFormater(_formatedTipMgr);
	}

	public void SetData(IStoreContentProvider<CtrlPrototype> _prov, HeroStore _heroStore)
	{
		if (_prov == null)
		{
			throw new ArgumentNullException("_prov");
		}
		if (_heroStore == null)
		{
			throw new ArgumentNullException("_heroStore");
		}
		mProtoProvider = _prov;
		mHeroStore = _heroStore;
		if (mHeroStore != null)
		{
			HeroStore heroStore = mHeroStore;
			heroStore.mGlobalBuffsReloaded = (HeroStore.Action)Delegate.Remove(heroStore.mGlobalBuffsReloaded, new HeroStore.Action(OnGlobalReloaded));
		}
		HeroStore heroStore2 = mHeroStore;
		heroStore2.mGlobalBuffsReloaded = (HeroStore.Action)Delegate.Combine(heroStore2.mGlobalBuffsReloaded, new HeroStore.Action(OnGlobalReloaded));
		mBuffsBattle.Clear();
		mBuffsReInit = true;
	}

	public void SetFormater(FormatedTipMgr _formatedTipMgr)
	{
		if (_formatedTipMgr == null)
		{
			throw new ArgumentNullException("_formatedTipMgr");
		}
		mFormatedTipMgr = _formatedTipMgr;
	}

	public void SetHero(Hero _player)
	{
		if (_player == null)
		{
			throw new ArgumentNullException("_player");
		}
		if (mHero != null)
		{
			Hero hero = mHero;
			hero.mBuffsUpdated = (Action<HeroGameInfo>)Delegate.Remove(hero.mBuffsUpdated, new Action<HeroGameInfo>(BuffsUpdated));
		}
		mHero = _player;
		Hero hero2 = mHero;
		hero2.mBuffsUpdated = (Action<HeroGameInfo>)Delegate.Combine(hero2.mBuffsUpdated, new Action<HeroGameInfo>(BuffsUpdated));
		mBuffsBattle.Clear();
		mBuffsReInit = true;
	}

	public void Clean()
	{
		if (mHero != null)
		{
			Hero hero = mHero;
			hero.mBuffsUpdated = (Action<HeroGameInfo>)Delegate.Remove(hero.mBuffsUpdated, new Action<HeroGameInfo>(BuffsUpdated));
		}
		mHero = null;
		if (mHeroStore != null)
		{
			HeroStore heroStore = mHeroStore;
			heroStore.mGlobalBuffsReloaded = (HeroStore.Action)Delegate.Remove(heroStore.mGlobalBuffsReloaded, new HeroStore.Action(OnGlobalReloaded));
		}
	}
}
