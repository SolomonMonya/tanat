using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class ObjectInfo : GuiElement
{
	public Texture2D mFrameImage;

	public Texture2D mHealthTileImg1;

	public Texture2D mHealthTileImg2;

	public Texture2D mHealthTileImg3;

	private int mObjId = -1;

	private Texture2D mObjIcon;

	private Rect mIconRect;

	private string mLevel;

	private Rect mLevelRect;

	private string mName;

	private Rect mNameRect;

	private Texture2D mCurHPImage;

	private Rect mHPRect;

	private Rect mBaseHPRect;

	private Texture2D mCurManaImage;

	private Rect mManaRect;

	private Rect mBaseManaRect;

	private IGameObject mObject;

	private SyncedParams mSyncReceiver;

	private IStoreContentProvider<IGameObject> mGameObjProv;

	public override void Init()
	{
		mFrameImage = GuiSystem.GetImage("Gui/ObjectInfo/object_info_frame1");
		mHealthTileImg1 = GuiSystem.GetImage("Gui/misc/frame_hp_1");
		mHealthTileImg2 = GuiSystem.GetImage("Gui/misc/frame_hp_2");
		mHealthTileImg3 = GuiSystem.GetImage("Gui/misc/frame_hp_3");
		mCurManaImage = GuiSystem.GetImage("Gui/misc/frame_mana_1");
	}

	public override void SetSize()
	{
		mZoneRect.x = 0f;
		mZoneRect.y = 0f;
		mZoneRect.width = mFrameImage.width;
		mZoneRect.height = mFrameImage.height;
		GuiSystem.RecalculateRect(mPos, ref mZoneRect);
		mIconRect = new Rect(10f, 13f, 54f, 54f);
		mLevelRect = new Rect(63f, 54f, 22f, 22f);
		mNameRect = new Rect(64f, 3f, 98f, 17f);
		mBaseHPRect = new Rect(72f, 25f, 96f, 12f);
		mBaseManaRect = new Rect(72f, 41f, 96f, 12f);
		GuiSystem.SetChildRect(mZoneRect, ref mIconRect);
		GuiSystem.SetChildRect(mZoneRect, ref mLevelRect);
		GuiSystem.SetChildRect(mZoneRect, ref mNameRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBaseHPRect);
		GuiSystem.SetChildRect(mZoneRect, ref mBaseManaRect);
	}

	public override void CheckEvent(Event _curEvent)
	{
		base.CheckEvent(_curEvent);
	}

	public override void Update()
	{
		if (mObject != null)
		{
			if (!mObject.Data.Visible || !mObject.Data.Relevant)
			{
				SetObj(-1);
				return;
			}
			InitHP();
			InitMana();
			mLevel = (mObject.Data.Level + 1).ToString();
		}
	}

	public override void RenderElement()
	{
		if (!MainInfoWindow.mHidden && mObject != null)
		{
			if (mObjIcon != null)
			{
				GuiSystem.DrawImage(mObjIcon, mIconRect);
			}
			if (mCurHPImage != null)
			{
				GuiSystem.DrawImage(mCurHPImage, mHPRect);
				GuiSystem.DrawString(mSyncReceiver.Health + "/" + mSyncReceiver.MaxHealth, mBaseHPRect, "middle_center");
			}
			if (mCurManaImage != null)
			{
				GuiSystem.DrawImage(mCurManaImage, mManaRect);
				GuiSystem.DrawString(mSyncReceiver.Mana + "/" + mSyncReceiver.MaxMana, mBaseManaRect, "middle_center");
			}
			GuiSystem.DrawImage(mFrameImage, mZoneRect);
			GuiSystem.DrawString(mLevel, mLevelRect, "middle_center");
			GuiSystem.DrawString(mName, mNameRect, "middle_center");
		}
	}

	public void SetObj(int _objId)
	{
		if (_objId != mObjId)
		{
			mObjId = _objId;
			if (mObjId != -1)
			{
				InitData();
			}
		}
		SetActive(_objId != -1);
	}

	public int GetObjId()
	{
		return mObjId;
	}

	private void InitData()
	{
		if (mGameObjProv == null)
		{
			return;
		}
		mObject = mGameObjProv.Get(mObjId);
		if (mObject == null)
		{
			Log.Error("IGameObject is null");
			return;
		}
		if (mObject.Data.IsPlayerBinded)
		{
			mName = mObject.Data.Player.Name;
		}
		else
		{
			if (mObject.Proto.Desc == null)
			{
				mObjId = -1;
				mObject = null;
				SetActive(_active: false);
				return;
			}
			mName = GuiSystem.GetLocaleText(mObject.Proto.Desc.mName);
		}
		mSyncReceiver = mObject.Data.Params;
		mObjIcon = GuiSystem.GetImage(mObject.Proto.Desc.mIcon + "_03");
	}

	private void InitHP()
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
		mHPRect = new Rect(72f, 27f, 96f * healthProgress, 12f);
		GuiSystem.SetChildRect(mZoneRect, ref mHPRect);
	}

	private void InitMana()
	{
		float manaProgress = mSyncReceiver.ManaProgress;
		mManaRect = new Rect(72f, 42f, 96f * manaProgress, 12f);
		GuiSystem.SetChildRect(mZoneRect, ref mManaRect);
	}

	public void SetData(IStoreContentProvider<IGameObject> _gameObjProv)
	{
		mGameObjProv = _gameObjProv;
	}

	public void Clear()
	{
		SetObj(-1);
		mGameObjProv = null;
	}
}
