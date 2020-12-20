using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class AvatarInfoWindow : GuiElement
{
	public enum ParamTypes
	{
		Health,
		HealthRegen,
		DamageMin,
		DamageMax,
		AttackSpeed,
		Mana,
		ManaRegen,
		PhysArmor,
		AntiPhysArmor,
		MagicArmor,
		AntiMagicArmor,
		CritChance,
		CritSize,
		AntiCritChance,
		DodgeChance,
		AntiDodgeChance,
		BlockChance,
		BlockSize,
		AntiBlock
	}

	private Texture2D mFrameImage;

	private SyncedParams mSyncReceiver;

	private Dictionary<string, AvaParamData> mAvaParamData;

	public bool mInited;

	public override void Init()
	{
		mFrameImage = GuiSystem.GetImage("Gui/AvatarInfo/frame1");
		InitParamData();
	}

	public override void SetSize()
	{
		mZoneRect.y = 578f;
		mZoneRect.width = mFrameImage.width;
		mZoneRect.height = mFrameImage.height;
		GuiSystem.GetRectScaled(ref mZoneRect, _ignoreLowRate: false);
		mZoneRect.x = ((float)OptionsMgr.mScreenWidth - mZoneRect.width) / 2f;
		Color mColor = new Color(208f / 255f, 113f / 255f, 21f / 85f);
		Color mColor2 = new Color(16f / 51f, 158f / 255f, 193f / 255f);
		Color mColor3 = new Color(169f / 255f, 52f / 255f, 46f / 255f);
		foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
		{
			switch (mAvaParamDatum.Key)
			{
			case "DamageMin":
				mAvaParamDatum.Value.mDrawRect = new Rect(85f, 38f, 99f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "AttackSpeed":
				mAvaParamDatum.Value.mDrawRect = new Rect(85f, 53f, 99f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "HealthRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(267f, 38f, 99f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "ManaRegen":
				mAvaParamDatum.Value.mDrawRect = new Rect(267f, 53f, 99f, 12f);
				mAvaParamDatum.Value.mColor = mColor;
				mAvaParamDatum.Value.mFormat = "middle_center";
				break;
			case "PhysArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(145f, 102f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiPhysArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(273f, 102f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "MagicArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(145f, 118f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiMagicArmor":
				mAvaParamDatum.Value.mDrawRect = new Rect(273f, 118f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "AntiCritChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(145f, 134f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "CritChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(273f, 134f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "DodgeChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(145f, 149f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiDodgeChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(273f, 149f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			case "BlockChance":
				mAvaParamDatum.Value.mDrawRect = new Rect(145f, 164f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor2;
				break;
			case "AntiBlock":
				mAvaParamDatum.Value.mDrawRect = new Rect(273f, 164f, 90f, 12f);
				mAvaParamDatum.Value.mColor = mColor3;
				break;
			}
			GuiSystem.SetChildRect(mZoneRect, ref mAvaParamDatum.Value.mDrawRect);
		}
	}

	public override void RenderElement()
	{
		if (null != mFrameImage)
		{
			GuiSystem.DrawImage(mFrameImage, mZoneRect, 4, 4, 4, 4);
		}
		foreach (KeyValuePair<string, AvaParamData> mAvaParamDatum in mAvaParamData)
		{
			GUI.contentColor = mAvaParamDatum.Value.mColor;
			GuiSystem.DrawString(mAvaParamDatum.Value.mValue, mAvaParamDatum.Value.mDrawRect, mAvaParamDatum.Value.mFormat);
		}
		GUI.contentColor = Color.white;
	}

	public override void Update()
	{
		SetAvatarParamData();
	}

	public void SetData(SyncedParams _syncReceiver)
	{
		if (_syncReceiver != null && !mInited)
		{
			mSyncReceiver = _syncReceiver;
			mInited = true;
		}
	}

	public void SetAvatarParamData()
	{
		if (mSyncReceiver != null)
		{
			mAvaParamData["DamageMin"].mValue = GetStringParamValue("DamageMin");
			mAvaParamData["AttackSpeed"].mValue = GetStringParamValue("AttackSpeed");
			mAvaParamData["HealthRegen"].mValue = GetStringParamValue("HealthRegen");
			mAvaParamData["ManaRegen"].mValue = GetStringParamValue("ManaRegen");
			mAvaParamData["PhysArmor"].mValue = GetStringParamValue("PhysArmor");
			mAvaParamData["AntiPhysArmor"].mValue = GetStringParamValue("AntiPhysArmor");
			mAvaParamData["MagicArmor"].mValue = GetStringParamValue("MagicArmor");
			mAvaParamData["AntiMagicArmor"].mValue = GetStringParamValue("AntiMagicArmor");
			mAvaParamData["CritChance"].mValue = GetStringParamValue("CritChance");
			mAvaParamData["AntiCritChance"].mValue = GetStringParamValue("AntiCritChance");
			mAvaParamData["DodgeChance"].mValue = GetStringParamValue("DodgeChance");
			mAvaParamData["AntiDodgeChance"].mValue = GetStringParamValue("AntiDodgeChance");
			mAvaParamData["BlockChance"].mValue = GetStringParamValue("BlockChance");
			mAvaParamData["AntiBlock"].mValue = GetStringParamValue("AntiBlock");
		}
	}

	private string GetStringParamValue(string _param)
	{
		string result = string.Empty;
		switch (_param)
		{
		case "DamageMin":
			result = Mathf.RoundToInt(mSyncReceiver.mDamageMin) + "-" + Mathf.RoundToInt(mSyncReceiver.mDamageMax);
			break;
		case "HealthRegen":
			result = mSyncReceiver.mHealthRegen.ToString("0.##");
			break;
		case "ManaRegen":
			result = mSyncReceiver.mManaRegen.ToString("0.##");
			break;
		case "AttackSpeed":
			result = mSyncReceiver.mAttackSpeed.ToString("0.##");
			break;
		case "PhysArmor":
			result = "+" + (mSyncReceiver.mArmorPhys * 100f).ToString("0.##") + "%";
			break;
		case "MagicArmor":
			result = "+" + (mSyncReceiver.mArmorMagic * 100f).ToString("0.##") + "%";
			break;
		case "CritChance":
			result = "+" + (mSyncReceiver.mCritChance * 100f).ToString("0.##") + "%(x" + mSyncReceiver.mCritValue.ToString("0.##") + ")";
			break;
		case "DodgeChance":
			result = "+" + (mSyncReceiver.mDodge * 100f).ToString("0.##") + "%";
			break;
		case "BlockChance":
			result = "+" + (mSyncReceiver.mBlockChance * 100f).ToString("0.##") + "%(" + mSyncReceiver.mBlockValue.ToString("0.##") + ")";
			break;
		case "AntiCritChance":
			result = "+" + (mSyncReceiver.mAntiCritChance * 100f).ToString("0.##") + "%";
			break;
		case "AntiDodgeChance":
			result = "+" + (mSyncReceiver.mAntiDodge * 100f).ToString("0.##") + "%";
			break;
		case "AntiBlock":
			result = "+" + (mSyncReceiver.mAntiBlockChance * 100f).ToString("0.##") + "%";
			break;
		case "AntiPhysArmor":
			result = "+" + (mSyncReceiver.mAntiArmorPhys * 100f).ToString("0.##") + "%";
			break;
		case "AntiMagicArmor":
			result = "+" + (mSyncReceiver.mAntiArmorMagic * 100f).ToString("0.##") + "%";
			break;
		}
		return result;
	}

	private void InitParamData()
	{
		mAvaParamData = new Dictionary<string, AvaParamData>();
		string[] names = Enum.GetNames(typeof(ParamTypes));
		foreach (string key in names)
		{
			mAvaParamData.Add(key, new AvaParamData());
		}
	}
}
