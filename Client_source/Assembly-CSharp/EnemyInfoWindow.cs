using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class EnemyInfoWindow : GuiElement, EscapeListener
{
	private enum ParamGroupType
	{
		NONE,
		ATTACK,
		DEFENCE,
		REGEN,
		MOVE_SPEED
	}

	private int mPlayerObjId = -1;

	private GameData mGameData;

	private SyncedParams mSyncReceiver;

	private Texture2D mFrameImage1;

	private Texture2D mFrameImage2;

	private Rect mFrameImage2Rect;

	private Texture2D mCurHPImage;

	private Rect mHPRect;

	private Rect mHPRectBase;

	private Texture2D mCurManaImage;

	private Rect mManaRect;

	private Rect mManaRectBase;

	public Texture2D mHealthTileImg1;

	public Texture2D mHealthTileImg2;

	public Texture2D mHealthTileImg3;

	private Texture2D mObjIcon;

	private Rect mIconRect;

	private Rect mLevelRect;

	private string mName;

	private Rect mNameRect;

	private Rect mDamageRect;

	private string mDamage;

	private Rect mArmorRect;

	private string mArmor;

	private Rect mRegenRect;

	private string mRegen;

	private Rect mSpeedRect;

	private string mSpeed;

	private string mHP;

	private string mMana;

	private GuiButton mCloseButton;

	private List<GuiButton> mItems;

	private Dictionary<ParamGroupType, Rect> mParamZoneRects;

	private bool mHasTip;

	private BuffRenderer mBuffRenderer = new BuffRenderer();

	private FormatedTipMgr mFormatedTipMgr;

	private IStoreContentProvider<IGameObject> mGameObjProv;

	private IStoreContentProvider<BattlePrototype> mBattleItemData;

	private IStoreContentProvider<CtrlPrototype> mCtrlItemData;

	private PopupInfo mPopupInfoWnd;

	private void InitZoneRects()
	{
		mParamZoneRects.Add(ParamGroupType.ATTACK, default(Rect));
		mParamZoneRects.Add(ParamGroupType.DEFENCE, default(Rect));
		mParamZoneRects.Add(ParamGroupType.REGEN, default(Rect));
		mParamZoneRects.Add(ParamGroupType.MOVE_SPEED, default(Rect));
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
		GuiSystem.mGuiInputMgr.AddEscapeListener(550, this);
		mFrameImage1 = GuiSystem.GetImage("Gui/EnemyInfo/frame1");
		mFrameImage2 = GuiSystem.GetImage("Gui/EnemyInfo/frame2");
		mHealthTileImg1 = GuiSystem.GetImage("Gui/misc/frame_hp_1");
		mHealthTileImg2 = GuiSystem.GetImage("Gui/misc/frame_hp_2");
		mHealthTileImg3 = GuiSystem.GetImage("Gui/misc/frame_hp_3");
		mCurManaImage = GuiSystem.GetImage("Gui/misc/frame_mana_1");
		mItems = new List<GuiButton>();
		mParamZoneRects = new Dictionary<ParamGroupType, Rect>();
		mCloseButton = new GuiButton();
		mCloseButton.mElementId = "ENEMY_INFO_CLOSE_BUTTON";
		mCloseButton.mNormImg = GuiSystem.GetImage("Gui/misc/button_10_norm");
		mCloseButton.mOverImg = GuiSystem.GetImage("Gui/misc/button_10_over");
		mCloseButton.mPressImg = GuiSystem.GetImage("Gui/misc/button_10_press");
		GuiButton guiButton = mCloseButton;
		guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnCloseButton));
		mCloseButton.Init();
		InitZoneRects();
	}

	public override void SetSize()
	{
		mZoneRect = new Rect(0f, 560f, mFrameImage1.width, mFrameImage1.height);
		GuiSystem.GetRectScaled(ref mZoneRect);
		mZoneRect.x = (float)OptionsMgr.mScreenWidth - mZoneRect.width;
		mCloseButton.mZoneRect = new Rect(208f, 5f, 26f, 26f);
		GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		mFrameImage2Rect = new Rect(7f, 7f, mFrameImage2.width, mFrameImage2.height);
		GuiSystem.SetChildRect(mZoneRect, ref mFrameImage2Rect);
		mIconRect = new Rect(11f, 26f, 42f, 42f);
		GuiSystem.SetChildRect(mZoneRect, ref mIconRect);
		mLevelRect = new Rect(7f, 5f, 23f, 23f);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
		mNameRect = new Rect(40f, 8f, 163f, 16f);
		GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
		mHPRectBase = new Rect(54f, 33f, 168f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref mHPRectBase);
		mManaRectBase = new Rect(54f, 49f, 168f, 13f);
		GuiSystem.SetChildRect(mZoneRect, ref mManaRectBase);
		mDamageRect = new Rect(37f, 81f, 68f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mDamageRect);
		mArmorRect = new Rect(37f, 107f, 68f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mArmorRect);
		mRegenRect = new Rect(37f, 133f, 68f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mRegenRect);
		mSpeedRect = new Rect(37f, 159f, 68f, 14f);
		GuiSystem.SetChildRect(mZoneRect, ref mSpeedRect);
		Rect _rect = new Rect(17f, 78f, 20f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		mParamZoneRects[ParamGroupType.ATTACK] = _rect;
		_rect = new Rect(17f, 104f, 20f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		mParamZoneRects[ParamGroupType.DEFENCE] = _rect;
		_rect = new Rect(17f, 130f, 20f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		mParamZoneRects[ParamGroupType.REGEN] = _rect;
		_rect = new Rect(17f, 156f, 20f, 20f);
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		mParamZoneRects[ParamGroupType.MOVE_SPEED] = _rect;
		for (int i = 0; i < mItems.Count; i++)
		{
			mItems[i].mZoneRect = GetItemRect(i);
		}
		mBuffRenderer.mSize = 30f;
		mBuffRenderer.mOffsetX = 35f;
		mBuffRenderer.mOffsetY = -35f;
		mBuffRenderer.mStartX = 50f;
		mBuffRenderer.mStartY = -35f;
		mBuffRenderer.mRowSize = 4f;
		mBuffRenderer.mBaseRect = mZoneRect;
		mBuffRenderer.SetBuffSize();
	}

	public override void RenderElement()
	{
		if (MainInfoWindow.mHidden || null == mGameData || mSyncReceiver == null)
		{
			return;
		}
		GuiSystem.DrawImage(mFrameImage1, mZoneRect);
		GuiSystem.DrawImage(mObjIcon, mIconRect);
		GuiSystem.DrawImage(mCurHPImage, mHPRect);
		GuiSystem.DrawImage(mCurManaImage, mManaRect);
		GuiSystem.DrawImage(mFrameImage2, mFrameImage2Rect);
		GuiSystem.DrawString(mMana, mManaRectBase, "middle_center");
		GuiSystem.DrawString(mHP, mHPRectBase, "middle_center");
		GuiSystem.DrawString(mName, mNameRect, "middle_center");
		GuiSystem.DrawString((mGameData.Data.Level + 1).ToString(), mLevelRect, "middle_center");
		GUI.contentColor = Color.red;
		GuiSystem.DrawString(mDamage, mDamageRect, "middle_center");
		GUI.contentColor = new Color(103f / 255f, 227f / 255f, 223f / 255f);
		GuiSystem.DrawString(mArmor, mArmorRect, "middle_center");
		GUI.contentColor = new Color(13f / 15f, 22f / 51f, 23f / 255f);
		GuiSystem.DrawString(mRegen, mRegenRect, "middle_center");
		GUI.contentColor = new Color(1f, 226f / 255f, 35f / 51f);
		GuiSystem.DrawString(mSpeed, mSpeedRect, "middle_center");
		GUI.contentColor = Color.white;
		foreach (GuiButton mItem in mItems)
		{
			mItem.RenderElement();
		}
		mBuffRenderer.RenderElement();
		mCloseButton.RenderElement();
	}

	public override void Update()
	{
		InitHP();
		InitMana();
		InitAttributes();
		mBuffRenderer.Update();
	}

	public override void CheckEvent(Event _curEvent)
	{
		mCloseButton.CheckEvent(_curEvent);
		foreach (GuiButton mItem in mItems)
		{
			mItem.CheckEvent(_curEvent);
		}
		bool flag = false;
		foreach (KeyValuePair<ParamGroupType, Rect> mParamZoneRect in mParamZoneRects)
		{
			if (mParamZoneRect.Value.Contains(_curEvent.mousePosition))
			{
				flag = true;
				mHasTip = true;
				ShowParamsTip(mParamZoneRect.Key);
				break;
			}
		}
		if (!flag && mHasTip)
		{
			ShowParamsTip(ParamGroupType.NONE);
			mHasTip = false;
		}
		mBuffRenderer.CheckEvent(_curEvent);
	}

	public override void Uninit()
	{
		mGameData = null;
		mSyncReceiver = null;
		mObjIcon = null;
		SetActive(_active: false);
		mPlayerObjId = -1;
		mName = string.Empty;
	}

	public bool SetEnemy(GameData _enemy)
	{
		Uninit();
		if (null == _enemy)
		{
			return false;
		}
		if (!_enemy.Data.IsPlayerBinded)
		{
			return false;
		}
		mGameData = _enemy;
		mSyncReceiver = mGameData.Data.Params;
		mObjIcon = GuiSystem.GetImage(mGameData.Proto.Desc.mIcon + "_03");
		Player player = mGameData.Data.Player;
		mPlayerObjId = player.AvatarObjId;
		mName = player.Name;
		mBuffRenderer.SetAvatarData(mGameData.Data);
		mBuffRenderer.SetHero(player.Hero);
		List<Effector> list = new List<Effector>();
		mGameData.Data.GetEffectorsByType(SkillType.BUFF, list);
		foreach (Effector item in list)
		{
			AddBuff(item.Id, item.Proto, item.OwnerId);
		}
		InitItems(player.ActiveItems);
		SetActive(_active: true);
		return true;
	}

	public bool SetEnemy(int _enemyId)
	{
		IGameObject gameObject = mGameObjProv.TryGet(_enemyId);
		return SetEnemy(gameObject as GameData);
	}

	public void HideIfCurEnemy(GameData _gd)
	{
		if (mGameData == _gd)
		{
			SetEnemy(null);
			SetActive(_active: false);
		}
	}

	public void ReinitItems(Player _playerInfo)
	{
		if (_playerInfo.AvatarObjId == mPlayerObjId)
		{
			InitItems(_playerInfo.ActiveItems);
		}
	}

	public void AddBuff(int _buffId, BattlePrototype _data, int _objId)
	{
		if (mPlayerObjId == _objId)
		{
			if (mBuffRenderer.ContainsBattleBuff(_buffId))
			{
				Log.Error("buff with id : " + _buffId + " already exist");
			}
			else
			{
				mBuffRenderer.AddBattleBuff(_buffId, _data);
			}
		}
	}

	public void RemoveBuff(int _buffId)
	{
		if (mBuffRenderer.ContainsBattleBuff(_buffId))
		{
			mBuffRenderer.RemoveBattleBuff(_buffId);
		}
	}

	private void OnItemMouseEnter(GuiElement _sender)
	{
		if (mFormatedTipMgr == null || mGameData == null)
		{
			return;
		}
		BattlePrototype battlePrototype = mBattleItemData.TryGet(_sender.mId);
		if (battlePrototype != null)
		{
			CtrlPrototype ctrlPrototype = mCtrlItemData.TryGet(battlePrototype.Item.mArticle);
			if (ctrlPrototype != null)
			{
				mFormatedTipMgr.Show(battlePrototype, ctrlPrototype, mGameData.Data.Level + 1, -1, _sender.UId, false);
			}
		}
	}

	private void OnTipMouseLeave(GuiElement _sender)
	{
		if (mFormatedTipMgr != null)
		{
			mFormatedTipMgr.Hide(_sender.UId);
		}
	}

	private void ShowParamsTip(ParamGroupType _groupType)
	{
		if (_groupType == ParamGroupType.NONE)
		{
			mPopupInfoWnd.Hide();
		}
		else
		{
			mPopupInfoWnd.ShowInfo(GetParamText(_groupType));
		}
	}

	private string GetParamText(ParamGroupType _groupType)
	{
		string text = string.Empty;
		switch (_groupType)
		{
		case ParamGroupType.ATTACK:
		{
			string text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ATTACK_TEXT") + ": " + mSyncReceiver.mDamageMin + "-" + mSyncReceiver.mDamageMax + "\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ATTACK_SPEED_TEXT") + ": " + mSyncReceiver.mAttackSpeed.ToString("0.##") + "\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("CRIT_TEXT") + ": " + (mSyncReceiver.mCritChance * 100f).ToString("0.##") + "%(x" + mSyncReceiver.mCritValue.ToString("0.##") + ")\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ANTIDODGE_CHANCE_TEXT") + ": " + (mSyncReceiver.mAntiDodge * 100f).ToString("0.##") + "%\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ATTACK_RANGE") + ": " + mSyncReceiver.mAttackRange.ToString("0.##") + "\n";
			break;
		}
		case ParamGroupType.DEFENCE:
		{
			string text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ARMOR_PHYS_TEXT") + ": " + (mSyncReceiver.mArmorPhys * 100f).ToString("0.##") + "%\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ARMOR_MAGIC_TEXT") + ": " + (mSyncReceiver.mArmorMagic * 100f).ToString("0.##") + "%\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("BLOCK_TEXT") + ": " + (mSyncReceiver.mBlockChance * 100f).ToString("0.##") + "%(" + mSyncReceiver.mBlockValue.ToString("0.##") + ")\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("DODGE_CHANCE_TEXT") + ": " + (mSyncReceiver.mDodge * 100f).ToString("0.##") + "%\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("ANTICRIT_CHANCE_TEXT") + ": " + (mSyncReceiver.mAntiCritChance * 100f).ToString("0.##") + "%\n";
			break;
		}
		case ParamGroupType.REGEN:
		{
			string text2 = text;
			text = text2 + GuiSystem.GetLocaleText("REGEN_TEXT") + " " + GuiSystem.GetLocaleText("HEALTH_REGEN_TEXT") + ": " + mSyncReceiver.mHealthRegen.ToString("0.##") + "\n";
			text2 = text;
			text = text2 + GuiSystem.GetLocaleText("REGEN_TEXT") + " " + GuiSystem.GetLocaleText("MANA_REGEN_TEXT") + ": " + mSyncReceiver.mManaRegen.ToString("0.##") + "\n";
			break;
		}
		case ParamGroupType.MOVE_SPEED:
		{
			string text2 = text;
			text = text2 + GuiSystem.GetLocaleText("MOVE_SPEED_TEXT") + ": " + mSyncReceiver.mSpeed.ToString("0.##") + "\n";
			break;
		}
		}
		return text;
	}

	private void InitItems(IEnumerable<int> _items)
	{
		mItems.Clear();
		foreach (int _item in _items)
		{
			BattlePrototype itemData = mBattleItemData.Get(_item);
			AddItem(_item, itemData);
		}
	}

	private void AddItem(int itemId, BattlePrototype _itemData)
	{
		if (_itemData != null)
		{
			GuiButton guiButton = new GuiButton();
			guiButton.mId = itemId;
			guiButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + _itemData.Desc.mIcon);
			guiButton.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			guiButton.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton.mOnMouseLeave, new OnMouseLeave(OnTipMouseLeave));
			guiButton.mZoneRect = GetItemRect(mItems.Count);
			guiButton.Init();
			mItems.Add(guiButton);
		}
	}

	private Rect GetItemRect(int _slotNum)
	{
		Rect _rect = new Rect(0f, 0f, 46f, 46f);
		switch (_slotNum)
		{
		case 0:
			_rect.x = 119f;
			_rect.y = 78f;
			break;
		case 1:
			_rect.x = 177f;
			_rect.y = 78f;
			break;
		case 2:
			_rect.x = 148f;
			_rect.y = 136f;
			break;
		}
		GuiSystem.SetChildRect(mZoneRect, ref _rect);
		return _rect;
	}

	private void InitHP()
	{
		if (mSyncReceiver.MaxHealth != 0)
		{
			float healthProgress = mSyncReceiver.HealthProgress;
			if (healthProgress >= 0.7f)
			{
				mCurHPImage = mHealthTileImg1;
			}
			else if (healthProgress < 0.7f && healthProgress > 0.3f)
			{
				mCurHPImage = mHealthTileImg2;
			}
			else
			{
				mCurHPImage = mHealthTileImg3;
			}
			mHPRect = mHPRectBase;
			mHPRect.width *= healthProgress;
		}
	}

	private void InitMana()
	{
		if (mSyncReceiver.MaxMana != 0)
		{
			float manaProgress = mSyncReceiver.ManaProgress;
			mManaRect = mManaRectBase;
			mManaRect.width *= manaProgress;
		}
	}

	private void InitAttributes()
	{
		mDamage = Mathf.RoundToInt(mSyncReceiver.mDamageMin) + "-" + Mathf.RoundToInt(mSyncReceiver.mDamageMax);
		mArmor = (mSyncReceiver.mArmorPhys * 100f).ToString("0.##") + "/" + (mSyncReceiver.mArmorMagic * 100f).ToString("0.##");
		mRegen = mSyncReceiver.mHealthRegen.ToString("0.##") + "/" + mSyncReceiver.mManaRegen.ToString("0.##");
		mSpeed = mSyncReceiver.mSpeed.ToString("0.##");
		mHP = mSyncReceiver.Health.ToString("0.##") + "/" + mSyncReceiver.MaxHealth.ToString("0.##");
		mMana = mSyncReceiver.Mana.ToString("0.##") + "/" + mSyncReceiver.MaxMana.ToString("0.##");
	}

	private void OnCloseButton(GuiElement _sender, int _buttonId)
	{
		if (_buttonId == 0)
		{
			SetActive(_active: false);
		}
	}

	public void SetData(IStoreContentProvider<IGameObject> _gameObjProv, IStoreContentProvider<CtrlPrototype> _ctrlItemData, IStoreContentProvider<BattlePrototype> _battleItemData, PopupInfo _popupInfo, HeroStore _heroStore, FormatedTipMgr _tipMgr)
	{
		mGameObjProv = _gameObjProv;
		mBattleItemData = _battleItemData;
		mCtrlItemData = _ctrlItemData;
		mPopupInfoWnd = _popupInfo;
		mFormatedTipMgr = _tipMgr;
		mBuffRenderer.SetData(_ctrlItemData, _heroStore);
		mBuffRenderer.SetFormater(mFormatedTipMgr);
	}

	public void Clear()
	{
		mGameObjProv = null;
		mBattleItemData = null;
		mCtrlItemData = null;
		mFormatedTipMgr = null;
		mPopupInfoWnd = null;
	}
}
