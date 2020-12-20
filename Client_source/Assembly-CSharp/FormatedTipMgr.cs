using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class FormatedTipMgr : GuiElement
{
	public enum ParamType
	{
		NONE,
		LABEL,
		DESC_1,
		DESC_2,
		RESTRICTION,
		PRICE_BUY,
		PRICE_SELL,
		NOTE,
		MANACOST,
		PARAMS
	}

	public enum TipType
	{
		SKILL_UPGRADE,
		SKILL_ACTIVE,
		ITEM_SHOP,
		ITEM_USED
	}

	private class FormatedTip : GuiElement
	{
		private Texture2D mFrame;

		public float mTextBorder;

		public Rect mTextRect;

		public List<Param> mParams;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/misc/popup_frame1");
			mParams = new List<Param>();
		}

		public override void SetSize()
		{
			mTextBorder = 8f;
			mZoneRect = new Rect(0f, 0f, 280f, mTextBorder * 2f);
			GuiSystem.GetRectScaled(ref mZoneRect, _ignoreLowRate: true);
			if (mPos != Vector2.zero)
			{
				mZoneRect.x = mPos.x;
				mZoneRect.y = mPos.y;
			}
			else
			{
				mZoneRect.x = (float)OptionsMgr.mScreenWidth / 2f;
				mZoneRect.y = 740f * GuiSystem.mNativeYRate;
			}
			mTextBorder *= GuiSystem.mNativeYRate;
			mTextRect = new Rect(mZoneRect.x + mTextBorder, mZoneRect.y + mTextBorder, mZoneRect.width - mTextBorder * 2f, mZoneRect.height - mTextBorder * 2f);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect, 4, 4, 4, 4);
			foreach (Param mParam in mParams)
			{
				mParam.RenderElement();
			}
			GUI.contentColor = Color.white;
		}

		public void AddParam(Param _param)
		{
			if (_param != null)
			{
				_param.mDataRect.x = mTextRect.x;
				_param.mDataRect.y = mTextRect.y + mTextRect.height;
				_param.mDataRect.width = mTextRect.width;
				mZoneRect.height += _param.mDataRect.height + GetParamDataOffset(_param.mType);
				mTextRect.height = mZoneRect.height - mTextBorder * 2f;
				if (_param.mMoneyData != null)
				{
					_param.mMoneyData.SetOffset(new Vector2(_param.mDataRect.x + 110f, _param.mDataRect.y));
				}
				mParams.Add(_param);
			}
		}

		public void NormilizePos()
		{
			float num = mZoneRect.x - ((!(mZoneRect.x + mZoneRect.width > (float)OptionsMgr.mScreenWidth)) ? mZoneRect.x : ((float)OptionsMgr.mScreenWidth - mZoneRect.width));
			float num2 = mZoneRect.height + ((!(mZoneRect.y - mZoneRect.height < 0f)) ? 0f : (mZoneRect.y - mZoneRect.height));
			mZoneRect.x -= num;
			mTextRect.x -= num;
			mZoneRect.y -= num2;
			mTextRect.y -= num2;
			foreach (Param mParam in mParams)
			{
				mParam.mDataRect.x -= num;
				mParam.mDataRect.y -= num2;
				if (mParam.mMoneyData != null)
				{
					mParam.mMoneyData.SetOffset(new Vector2(0f - num, 0f - num2));
				}
			}
		}

		private float GetParamDataOffset(ParamType _paramType)
		{
			if (_paramType == ParamType.DESC_2 || _paramType == ParamType.PARAMS)
			{
				return 0f;
			}
			return mTextBorder;
		}
	}

	private class Param
	{
		public string mData;

		public MoneyRenderer mMoneyData;

		public Rect mDataRect;

		public GUIStyle mStyle;

		public ParamType mType;

		public void RenderElement()
		{
			GuiSystem.DrawString(mData, mDataRect, mStyle);
			if (mMoneyData != null)
			{
				mMoneyData.Render();
			}
		}
	}

	private FormatedTip mTip;

	private Dictionary<ParamType, Color> mTipParamsColors;

	private Dictionary<ParamType, Font> mTipParamsFonts;

	private string mManaCostText = string.Empty;

	private string mCooldownText = string.Empty;

	private string mSkillLevelText = string.Empty;

	private string mRestrictionLevelText = string.Empty;

	private string mSkillPassiveText = string.Empty;

	private string mWeaponText = string.Empty;

	private string mConsumableText = string.Empty;

	private string mQuestText = string.Empty;

	private string mMoneyBuyText = string.Empty;

	private string mMoneySellText = string.Empty;

	private string mMoneyNotSellText = string.Empty;

	private string mParamsText = string.Empty;

	public override void Init()
	{
		mTipParamsColors = new Dictionary<ParamType, Color>();
		mTipParamsColors.Add(ParamType.NONE, Color.white);
		mTipParamsColors.Add(ParamType.LABEL, Color.white);
		mTipParamsColors.Add(ParamType.DESC_1, Color.white);
		mTipParamsColors.Add(ParamType.DESC_2, Color.grey);
		mTipParamsColors.Add(ParamType.RESTRICTION, new Color(11f / 15f, 0f, 0f));
		mTipParamsColors.Add(ParamType.PRICE_BUY, new Color(82f / 85f, 69f / 85f, 67f / 255f));
		mTipParamsColors.Add(ParamType.PRICE_SELL, new Color(82f / 85f, 69f / 85f, 67f / 255f));
		mTipParamsColors.Add(ParamType.NOTE, new Color(82f / 85f, 69f / 85f, 67f / 255f));
		mTipParamsColors.Add(ParamType.MANACOST, new Color(107f / 255f, 137f / 255f, 179f / 255f));
		mTipParamsColors.Add(ParamType.PARAMS, new Color(82f / 85f, 69f / 85f, 67f / 255f));
		mTipParamsFonts = new Dictionary<ParamType, Font>();
		mTipParamsFonts.Add(ParamType.NONE, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.LABEL, Resources.Load("Fonts/Tahoma_14") as Font);
		mTipParamsFonts.Add(ParamType.DESC_1, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.DESC_2, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.RESTRICTION, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.PRICE_BUY, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.PRICE_SELL, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.NOTE, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.MANACOST, Resources.Load("Fonts/Tahoma_11") as Font);
		mTipParamsFonts.Add(ParamType.PARAMS, Resources.Load("Fonts/Tahoma_11") as Font);
		mManaCostText = GuiSystem.GetLocaleText("Mana_Cost_Text");
		mCooldownText = GuiSystem.GetLocaleText("Cooldown_Text");
		mSkillLevelText = GuiSystem.GetLocaleText("Skill_Level_Text");
		mRestrictionLevelText = GuiSystem.GetLocaleText("Skill_Restriction_Level_Text");
		mSkillPassiveText = GuiSystem.GetLocaleText("Skill_Passive_Text");
		mWeaponText = GuiSystem.GetLocaleText("Weapont_Text");
		mConsumableText = GuiSystem.GetLocaleText("Consumable_Text");
		mQuestText = GuiSystem.GetLocaleText("QUEST_ITEM_TEXT");
		mMoneyBuyText = GuiSystem.GetLocaleText("Money_Buy_Text");
		mMoneySellText = GuiSystem.GetLocaleText("Money_Sell_Text");
		mMoneyNotSellText = GuiSystem.GetLocaleText("Money_Not_Sell_Text");
		mParamsText = GuiSystem.GetLocaleText("Params_Text");
	}

	public override void RenderElement()
	{
		if (mTip != null)
		{
			mTip.RenderElement();
		}
	}

	public void Hide(int _id)
	{
		if (mTip != null && _id == mTip.mId)
		{
			mTip = null;
		}
		SetPos(Vector2.zero);
	}

	public void SetPos(Vector2 _pos)
	{
		mPos = _pos;
	}

	public void Show(Effector _effector, TipType _type, int _avatarLvl, int _skillNum, int _btnId)
	{
		if (_effector == null)
		{
			return;
		}
		mTip = new FormatedTip();
		mTip.mId = _btnId;
		mTip.mPos = mPos;
		mTip.Init();
		mTip.SetSize();
		string hotKeyStr = GetHotKeyStr(_skillNum);
		string localeText = GuiSystem.GetLocaleText(_effector.Proto.EffectDesc.mDesc.mName);
		string text = string.Empty;
		string text2 = string.Empty;
		string data = string.Empty;
		string data2 = string.Empty;
		string data3 = string.Empty;
		string data4 = string.Empty;
		string data5 = string.Empty;
		string data6 = string.Empty;
		int num = 0;
		int[] upgradeLevels = _effector.GetUpgradeLevels();
		switch (_type)
		{
		case TipType.SKILL_ACTIVE:
			num = _effector.GetLevel();
			if (num > 0)
			{
				data4 = GenerateSingleParamFormatedString(mSkillLevelText, num);
			}
			if (_effector.Proto.EffectDesc.mDesc.mType != SkillType.PASSIVE)
			{
				if (_effector.CurManaCost > 0f)
				{
					data3 = GenerateSingleParamFormatedString(mManaCostText, _effector.CurManaCost);
				}
				data6 = ((hotKeyStr.Length <= 0) ? localeText : ("[" + hotKeyStr + "] " + localeText));
			}
			else
			{
				data6 = localeText;
			}
			if (_effector.Cooldown > 0f)
			{
				data2 = GenerateSingleParamFormatedString(mCooldownText, _effector.Cooldown);
			}
			text = GuiSystem.GetLocaleText(_effector.Proto.EffectDesc.mDesc.mLongDesc);
			text = GetSkillFormatedText(text, _effector, num);
			break;
		case TipType.SKILL_UPGRADE:
		{
			num = _effector.Level;
			data4 = ((num >= upgradeLevels.Length) ? GenerateSingleParamFormatedString(mSkillLevelText, num) : GenerateSingleParamFormatedString(mSkillLevelText, num + 1));
			if (_effector.Proto.EffectDesc.mDesc.mType != SkillType.PASSIVE)
			{
				data3 = GenerateSingleParamFormatedString(mManaCostText, IntArray2ObjectArray(_effector.GetAllManaCosts()));
				data2 = GenerateSingleParamFormatedString(mCooldownText, IntArray2ObjectArray(_effector.GetAllCooldowns()));
			}
			data6 = "[Ctrl+" + hotKeyStr + "] " + localeText;
			int num2 = upgradeLevels[(num <= 0) ? num : (num - 1)];
			if (num2 + 1 > _avatarLvl)
			{
				data5 = GenerateSingleParamFormatedString(mRestrictionLevelText, num2 + 1);
			}
			string localeText2 = GuiSystem.GetLocaleText(_effector.Proto.EffectDesc.mDesc.mDesc);
			if (_effector.Proto.EffectDesc.mDesc.mType != SkillType.PARAMS)
			{
				for (int i = 0; i < upgradeLevels.Length; i++)
				{
					if (i >= num)
					{
						text = text + GenerateSingleParamFormatedString(mSkillLevelText, i + 1) + " - " + GetSkillFormatedText(localeText2, _effector, i);
						if (i < upgradeLevels.Length - 1)
						{
							text += "\n";
						}
					}
					else
					{
						text2 = text2 + GenerateSingleParamFormatedString(mSkillLevelText, i + 1) + " - " + GetSkillFormatedText(localeText2, _effector, i);
						if (i + 1 < num)
						{
							text2 += "\n";
						}
					}
				}
			}
			else
			{
				text = GetSkillFormatedText(localeText2, _effector, 0);
			}
			data = GuiSystem.GetLocaleText(_effector.Proto.EffectDesc.mDesc.mLongDesc);
			break;
		}
		}
		mTip.AddParam(GenerateParam(ParamType.LABEL, data6));
		mTip.AddParam(GenerateParam(ParamType.NOTE, data4));
		mTip.AddParam(GenerateParam(ParamType.DESC_1, data));
		mTip.AddParam(GenerateParam(ParamType.DESC_2, text2));
		mTip.AddParam(GenerateParam(ParamType.DESC_1, text));
		mTip.AddParam(GenerateParam(ParamType.MANACOST, data3));
		mTip.AddParam(GenerateParam(ParamType.DESC_1, data2));
		if (_effector.Proto.EffectDesc.mDesc.mType == SkillType.PASSIVE)
		{
			mTip.AddParam(GenerateParam(ParamType.DESC_2, mSkillPassiveText));
		}
		mTip.AddParam(GenerateParam(ParamType.RESTRICTION, data5));
		mTip.NormilizePos();
	}

	private string GetSkillFormatedText(string _text, Effector _effector, int _lvl)
	{
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		List<object> list = new List<object>();
		while (true)
		{
			num = _text.IndexOf("{", num);
			if (num == -1)
			{
				break;
			}
			num2 = _text.IndexOf("}", num);
			num++;
			empty = _text.Substring(num, num2 - num);
			flag = empty[empty.Length - 1] == '%';
			flag2 = empty[empty.Length - 1] == '^';
			flag3 = empty[empty.Length - 1] == '#';
			flag4 = empty[0] == '*';
			if (flag || flag2 || flag3)
			{
				empty = empty.Remove(empty.Length - 1);
			}
			if (flag4)
			{
				empty = empty.Remove(0, 1);
			}
			_text = _text.Remove(num, num2 - num);
			object obj = _effector.GetValue(empty, _lvl);
			float num3 = ((!flag4) ? 1f : _effector.SyncedParams.mCastStrengthCoef);
			if (!flag && !flag2 && !flag3)
			{
				obj = float.Parse(obj.ToString());
			}
			else if (flag)
			{
				obj = float.Parse(obj.ToString()) * 100f;
			}
			else if (flag2)
			{
				obj = (float.Parse(obj.ToString()) - 1f) * 100f;
			}
			else if (flag3)
			{
				obj = (1f - float.Parse(obj.ToString())) * 100f;
			}
			obj = float.Parse(obj.ToString()) * num3;
			list.Add(obj);
		}
		_text = GenerateMultiParamFormatedString(_text, list.ToArray());
		return _text;
	}

	private string GetHotKeyStr(int _skillNum)
	{
		return _skillNum switch
		{
			1 => "Q", 
			2 => "W", 
			3 => "E", 
			4 => "R", 
			0 => "T", 
			_ => string.Empty, 
		};
	}

	public void Show(BattlePrototype _item, CtrlPrototype _article, int _avaLvl, int _heroLvl, int _btnId, bool? _bought)
	{
		if (_item == null && _article == null)
		{
			return;
		}
		mTip = new FormatedTip();
		mTip.mId = _btnId;
		mTip.mPos = mPos;
		mTip.Init();
		mTip.SetSize();
		string localeText = GuiSystem.GetLocaleText(_article.Desc.mName);
		string weaponTypeStr = GetWeaponTypeStr(_item, _article);
		string localeText2 = GuiSystem.GetLocaleText(_article.Desc.mLongDesc);
		string data = string.Empty;
		if (_item == null)
		{
			if (_article.Article.mMinHeroLvl > _heroLvl)
			{
				data = GenerateSingleParamFormatedString(mRestrictionLevelText, _article.Article.mMinHeroLvl);
			}
		}
		else if (_item.Item.mBattleItemType == BattleItemType.WEARABLE)
		{
			if (_article.Article.mMinAvaLvl > _avaLvl)
			{
				data = GenerateSingleParamFormatedString(mRestrictionLevelText, _article.Article.mMinAvaLvl);
			}
		}
		else if (_article.Article.mMinHeroLvl > _heroLvl)
		{
			data = GenerateSingleParamFormatedString(mRestrictionLevelText, _article.Article.mMinHeroLvl);
		}
		mTip.AddParam(GenerateParam(ParamType.LABEL, localeText));
		mTip.AddParam(GenerateParam(ParamType.NOTE, weaponTypeStr));
		if (_article.Article.mParams.Count > 0)
		{
			mTip.AddParam(GenerateParam(ParamType.PARAMS, mParamsText));
		}
		if (_item == null)
		{
			mTip.AddParam(GenerateParam(ParamType.DESC_1, GetItemFormatedText(localeText2, _article.Article.mParams, null)));
		}
		else
		{
			mTip.AddParam(GenerateParam(ParamType.DESC_1, GetItemFormatedText(localeText2, _article.Article.mParams, _item.Coefs)));
		}
		bool cantSell = _article.Article.CheckFlag(CtrlPrototype.ArticleMask.NO_SELL);
		float value;
		if (_item != null && _article.Article.mPriceType != 2)
		{
			if (!_item.Coefs.TryGetValue("PriceCoef", out value))
			{
				value = 1f;
			}
		}
		else
		{
			value = 1f;
		}
		if (_bought.HasValue)
		{
			if (_bought == true)
			{
				mTip.AddParam(GenerateMoneyParam(ParamType.PRICE_SELL, _article.Article.mSellCost, _article.Article.mPriceType == 2, cantSell));
			}
			else
			{
				mTip.AddParam(GenerateMoneyParam(ParamType.PRICE_BUY, Mathf.RoundToInt((float)(_article.Article.mBuyCost * _article.Article.mCountCoef) * value), _article.Article.mPriceType == 2, cantSell));
			}
		}
		mTip.AddParam(GenerateParam(ParamType.RESTRICTION, data));
		mTip.NormilizePos();
	}

	public void Show(BattlePrototype _item, CtrlPrototype _article, CtrlPrototype _upgradeArticle, int _avaLvl, int _heroLvl, int _btnId, bool _bought)
	{
		if (_item == null || _article == null || _upgradeArticle == null)
		{
			return;
		}
		mTip = new FormatedTip();
		mTip.mId = _btnId;
		mTip.mPos = mPos;
		mTip.Init();
		mTip.SetSize();
		string localeText = GuiSystem.GetLocaleText(_upgradeArticle.Desc.mName);
		string weaponTypeStr = GetWeaponTypeStr(_item, _upgradeArticle);
		string localeText2 = GuiSystem.GetLocaleText(_upgradeArticle.Desc.mLongDesc);
		string data = string.Empty;
		if (_item == null)
		{
			if (_upgradeArticle.Article.mMinHeroLvl > _heroLvl)
			{
				data = GenerateSingleParamFormatedString(mRestrictionLevelText, _upgradeArticle.Article.mMinHeroLvl);
			}
		}
		else if (_item.Item.mBattleItemType == BattleItemType.WEARABLE)
		{
			if (_upgradeArticle.Article.mMinAvaLvl > _avaLvl)
			{
				data = GenerateSingleParamFormatedString(mRestrictionLevelText, _upgradeArticle.Article.mMinAvaLvl);
			}
		}
		else if (_upgradeArticle.Article.mMinHeroLvl > _heroLvl)
		{
			data = GenerateSingleParamFormatedString(mRestrictionLevelText, _upgradeArticle.Article.mMinHeroLvl);
		}
		mTip.AddParam(GenerateParam(ParamType.LABEL, localeText));
		mTip.AddParam(GenerateParam(ParamType.NOTE, weaponTypeStr));
		if (_upgradeArticle.Article.mParams.Count > 0)
		{
			mTip.AddParam(GenerateParam(ParamType.PARAMS, mParamsText));
		}
		mTip.AddParam(GenerateParam(ParamType.DESC_1, GetItemDeltaFormatedText(localeText2, _article.Article.mParams, _upgradeArticle.Article.mParams, _item.Coefs)));
		bool cantSell = _article.Article.CheckFlag(CtrlPrototype.ArticleMask.NO_SELL);
		float value;
		if (_item != null && _upgradeArticle.Article.mPriceType != 2)
		{
			if (!_item.Coefs.TryGetValue("PriceCoef", out value))
			{
				value = 1f;
			}
		}
		else
		{
			value = 1f;
		}
		if (_bought)
		{
			mTip.AddParam(GenerateMoneyParam(ParamType.PRICE_SELL, _upgradeArticle.Article.mSellCost, _upgradeArticle.Article.mPriceType == 2, cantSell));
		}
		else
		{
			mTip.AddParam(GenerateMoneyParam(ParamType.PRICE_BUY, Mathf.RoundToInt((float)(_upgradeArticle.Article.mBuyCost * _upgradeArticle.Article.mCountCoef) * value), _upgradeArticle.Article.mPriceType == 2, cantSell));
		}
		mTip.AddParam(GenerateParam(ParamType.RESTRICTION, data));
		mTip.NormilizePos();
	}

	private string GetWeaponTypeStr(BattlePrototype _battleItem, CtrlPrototype _article)
	{
		if (_battleItem != null)
		{
			return _battleItem.Item.mBattleItemType switch
			{
				BattleItemType.WEARABLE => mWeaponText, 
				BattleItemType.CONSUMABLE => mConsumableText, 
				BattleItemType.QUEST => mQuestText, 
				_ => string.Empty, 
			};
		}
		return ShopGUI.GetItemTypeLabel((ShopGUI.ItemType)_article.Article.mKindId);
	}

	private string GetItemFormatedText(string _text, Dictionary<string, AddStat> _stats, IDictionary<string, float> _add)
	{
		if (_stats == null || _stats.Count == 0)
		{
			return _text;
		}
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<object> list = new List<object>();
		while (true)
		{
			num = _text.IndexOf("{", num);
			if (num == -1)
			{
				break;
			}
			num2 = _text.IndexOf("}", num);
			num++;
			empty = _text.Substring(num, num2 - num);
			flag = empty[empty.Length - 1] == '%';
			flag2 = empty[empty.Length - 1] == '^';
			flag3 = empty[empty.Length - 1] == '#';
			if (flag || flag2 || flag3)
			{
				empty = empty.Remove(empty.Length - 1);
			}
			_text = _text.Remove(num, num2 - num);
			object obj = -1;
			if (!_stats.ContainsKey(empty))
			{
				Log.Error("Can't find param : " + empty + " for item with long desc : " + _text);
			}
			else
			{
				float num3 = ((!(_stats[empty].mAdd > 0f)) ? _stats[empty].mMul : _stats[empty].mAdd);
				if (_add != null && _add.TryGetValue(empty, out var value))
				{
					num3 *= value;
				}
				obj = num3;
			}
			if (!flag && !flag2 && !flag3)
			{
				obj = float.Parse(obj.ToString());
			}
			else if (flag)
			{
				obj = float.Parse(obj.ToString()) * 100f;
			}
			else if (flag2)
			{
				obj = (float.Parse(obj.ToString()) - 1f) * 100f;
			}
			else if (flag3)
			{
				obj = (1f - float.Parse(obj.ToString())) * 100f;
			}
			list.Add(obj);
		}
		_text = GenerateMultiParamFormatedString(_text, list.ToArray());
		return _text;
	}

	private string GetItemDeltaFormatedText(string _text, Dictionary<string, AddStat> _stats, Dictionary<string, AddStat> _upgradeStats, IDictionary<string, float> _add)
	{
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<object> list = new List<object>();
		while (true)
		{
			num = _text.IndexOf("{", num);
			if (num == -1)
			{
				break;
			}
			num2 = _text.IndexOf("}", num);
			num++;
			empty = _text.Substring(num, num2 - num);
			flag = empty[empty.Length - 1] == '%';
			flag2 = empty[empty.Length - 1] == '^';
			flag3 = empty[empty.Length - 1] == '#';
			if (flag || flag2 || flag3)
			{
				empty = empty.Remove(empty.Length - 1);
			}
			_text = _text.Remove(num, num2 - num);
			object obj = -1;
			object obj2 = -1;
			bool flag4 = false;
			AddStat value = null;
			AddStat value2 = null;
			if (!_stats.TryGetValue(empty, out value) && !_upgradeStats.TryGetValue(empty, out value))
			{
				Log.Error("Can't find param : " + empty);
			}
			else
			{
				_upgradeStats.TryGetValue(empty, out value2);
				float num3 = 0f;
				float num4 = 0f;
				if (value != null)
				{
					num3 = ((!(value.mAdd > 0f)) ? value.mMul : value.mAdd);
				}
				if (value2 != null)
				{
					num4 = ((!(value2.mAdd > 0f)) ? (value2.mMul - num3 + Mathf.Floor(num3)) : (value2.mAdd - num3));
				}
				if (_add != null && _add.TryGetValue(empty, out var value3))
				{
					num3 *= value3;
					num4 *= value3;
				}
				obj = num3;
				obj2 = num4;
				flag4 = num4 > 0f;
			}
			if (!flag && !flag2 && !flag3)
			{
				obj = float.Parse(obj.ToString());
				obj2 = float.Parse(obj2.ToString());
			}
			else if (flag)
			{
				obj = float.Parse(obj.ToString()) * 100f;
				obj2 = float.Parse(obj2.ToString()) * 100f;
			}
			else if (flag2)
			{
				obj = (float.Parse(obj.ToString()) - 1f) * 100f;
				obj2 = (float.Parse(obj2.ToString()) - 1f) * 100f;
			}
			else if (flag3)
			{
				obj = (1f - float.Parse(obj.ToString())) * 100f;
				obj2 = (1f - float.Parse(obj2.ToString())) * 100f;
			}
			list.Add(obj);
			if (flag4)
			{
				list.Add(obj2);
				_text = _text.Insert(num + 1, "(+{})");
				num += 4;
			}
		}
		_text = GenerateMultiParamFormatedString(_text, list.ToArray());
		return _text;
	}

	private TextAnchor GetTextAnchor(ParamType _paramType)
	{
		return TextAnchor.MiddleLeft;
	}

	private Param GenerateParam(ParamType _paramType, string _data)
	{
		if (mTip == null || _data == string.Empty)
		{
			return null;
		}
		Font value = null;
		Color value2 = Color.white;
		if (!mTipParamsColors.TryGetValue(_paramType, out value2))
		{
			value2 = mTipParamsColors[ParamType.NONE];
		}
		if (!mTipParamsFonts.TryGetValue(_paramType, out value))
		{
			value = mTipParamsFonts[ParamType.NONE];
		}
		Param param = new Param();
		param.mData = _data;
		param.mType = _paramType;
		param.mStyle = new GUIStyle();
		param.mStyle.font = value;
		param.mStyle.alignment = GetTextAnchor(_paramType);
		param.mStyle.normal.textColor = value2;
		param.mStyle.wordWrap = true;
		param.mDataRect = new Rect(0f, 0f, 0f, param.mStyle.CalcHeight(new GUIContent(_data), mTip.mTextRect.width));
		return param;
	}

	private Param GenerateMoneyParam(ParamType _paramType, int _money, bool _diamonds, bool _cantSell)
	{
		if (mTip == null)
		{
			return null;
		}
		Font value = null;
		Color value2 = Color.white;
		if (!mTipParamsColors.TryGetValue(_paramType, out value2))
		{
			value2 = mTipParamsColors[ParamType.NONE];
		}
		if (!mTipParamsFonts.TryGetValue(_paramType, out value))
		{
			value = mTipParamsFonts[ParamType.NONE];
		}
		Param param = new Param();
		switch (_paramType)
		{
		case ParamType.PRICE_SELL:
			if (!_diamonds && !_cantSell)
			{
				param.mData = mMoneySellText;
			}
			else
			{
				param.mData = mMoneyNotSellText;
			}
			break;
		case ParamType.PRICE_BUY:
			param.mData = mMoneyBuyText;
			break;
		}
		if (_paramType != ParamType.PRICE_SELL || (_paramType == ParamType.PRICE_SELL && !_cantSell))
		{
			param.mMoneyData = new MoneyRenderer(_renderMoneyImage: true, _diamonds);
			param.mMoneyData.SetMoney(_money);
			param.mMoneyData.SetSize(mZoneRect);
		}
		param.mType = _paramType;
		param.mStyle = new GUIStyle();
		param.mStyle.font = value;
		param.mStyle.alignment = GetTextAnchor(_paramType);
		param.mStyle.normal.textColor = value2;
		param.mStyle.wordWrap = true;
		param.mDataRect = new Rect(0f, 0f, 0f, param.mStyle.CalcHeight(new GUIContent(param.mData), mTip.mTextRect.width));
		return param;
	}

	private string GenerateSingleParamFormatedString(string _defStr, object[] _values)
	{
		if (_defStr == string.Empty || _values.Length == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		for (int i = 0; i < _values.Length; i++)
		{
			text = text + "{" + i + ":0.##}";
			if (i < _values.Length - 1)
			{
				text += "/";
			}
		}
		_defStr = _defStr.Replace("{}", text);
		return string.Format(_defStr, _values);
	}

	private string GenerateMultiParamFormatedString(string _defStr, object[] _values)
	{
		if (_defStr == string.Empty || _values.Length == 0)
		{
			return _defStr;
		}
		int startIndex = 0;
		int num = 0;
		while (true)
		{
			startIndex = _defStr.IndexOf("{", startIndex);
			if (startIndex == -1)
			{
				break;
			}
			startIndex++;
			_defStr = _defStr.Insert(startIndex, num + ":0.##");
			if (_values.Length <= num)
			{
				Log.Error("Bad string or values count for : " + _defStr);
				break;
			}
			num++;
		}
		return string.Format(_defStr, _values);
	}

	private string GenerateSingleParamFormatedString(string _defStr, object _value)
	{
		return GenerateSingleParamFormatedString(_defStr, new object[1]
		{
			_value
		});
	}

	private object[] IntArray2ObjectArray<T>(T[] _array)
	{
		object[] array = new object[_array.Length];
		for (int i = 0; i < _array.Length; i++)
		{
			array[i] = _array[i];
		}
		return array;
	}
}
