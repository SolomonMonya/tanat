using System;
using TanatKernel;
using UnityEngine;

public class ShortGuiObjInfo : GuiElement
{
	public Texture2D mHpImageGood;

	public Texture2D mHpImageNorm;

	public Texture2D mHpImageBad;

	public Texture2D mManaImage;

	public Texture2D mHpImage;

	public Texture2D mFrameImage;

	public Rect mFrameRect;

	public Rect mHPRect;

	public Rect mManaRect;

	private float mWidth;

	private float mHeight;

	private bool mAvatar;

	private float mLastHealthProgr;

	private float mLastManaProgr;

	private SyncedParams mSyncReceiver;

	private Vector2 mDrawPos;

	public Vector2 DrawPos
	{
		set
		{
			if (Math.Abs(mDrawPos.x - value.x) > 1f || Math.Abs(mDrawPos.y - value.y) > 1f)
			{
				mDrawPos = value;
				mFrameRect.x = mDrawPos.x - mWidth / 2f - 2f;
				mFrameRect.y = (float)OptionsMgr.mScreenHeight - mDrawPos.y - mHeight * 2f - 2f;
				mHPRect.x = mDrawPos.x - mWidth / 2f;
				mHPRect.y = (float)OptionsMgr.mScreenHeight - mDrawPos.y - mHeight * 2f;
				mManaRect.x = mHPRect.x;
				mManaRect.y = mHPRect.y + mHeight;
			}
		}
	}

	public ShortGuiObjInfo()
	{
		mHpImageGood = GuiSystem.GetImage("Gui/misc/frame_hp_1");
		mHpImageNorm = GuiSystem.GetImage("Gui/misc/frame_hp_2");
		mHpImageBad = GuiSystem.GetImage("Gui/misc/frame_hp_3");
		mManaImage = GuiSystem.GetImage("Gui/misc/frame_mana_1");
		mHpImage = mHpImageGood;
	}

	public void Init(SyncedParams _syncReceiver, bool _avatar, Friendliness _friendliness)
	{
		if (_syncReceiver == null)
		{
			throw new ArgumentNullException("_syncReceiver");
		}
		mSyncReceiver = _syncReceiver;
		mAvatar = _avatar;
		if (mAvatar)
		{
			mWidth = 60f;
			mHeight = 5f;
		}
		else
		{
			mWidth = 45f;
			mHeight = 4f;
		}
		if (mAvatar)
		{
			switch (_friendliness)
			{
			case Friendliness.FRIEND:
				mFrameImage = GuiSystem.GetImage("Gui/ShortObjectInfo/friend_frame");
				break;
			case Friendliness.ENEMY:
				mFrameImage = GuiSystem.GetImage("Gui/ShortObjectInfo/enemy_frame");
				break;
			case Friendliness.NEUTRAL:
				mFrameImage = GuiSystem.GetImage("Gui/ShortObjectInfo/neutral_frame");
				break;
			default:
				mFrameImage = null;
				break;
			}
		}
		else
		{
			mFrameImage = GuiSystem.GetImage("Gui/ShortObjectInfo/creep_frame");
		}
		mFrameRect.width = mWidth + 4f;
		mFrameRect.height = mHeight + 4f;
		if (mAvatar)
		{
			mFrameRect.height += mHeight;
		}
		mHPRect.height = mHeight;
		mManaRect.height = mHeight;
	}

	public override void RenderElement()
	{
		if (!(mFrameImage == null) && mSyncReceiver != null && mHPRect.width != 0f)
		{
			Graphics.DrawTexture(mFrameRect, mFrameImage, 5, 5, 5, 5);
			GUI.DrawTexture(mHPRect, mHpImage);
			if (mAvatar)
			{
				GUI.DrawTexture(mManaRect, mManaImage);
			}
		}
	}

	public override void Update()
	{
		if (mSyncReceiver == null)
		{
			return;
		}
		float healthProgress = mSyncReceiver.HealthProgress;
		if (Mathf.Abs(healthProgress - mLastHealthProgr) > 0.01f)
		{
			mLastHealthProgr = healthProgress;
			if (healthProgress >= 0.7f)
			{
				mHpImage = mHpImageGood;
			}
			else if (healthProgress < 0.7f && healthProgress > 0.3f)
			{
				mHpImage = mHpImageNorm;
			}
			else
			{
				mHpImage = mHpImageBad;
			}
			mHPRect.width = mWidth * healthProgress;
		}
		if (mAvatar)
		{
			float manaProgress = mSyncReceiver.ManaProgress;
			if (Mathf.Abs(manaProgress - mLastManaProgr) > 0.01f)
			{
				mLastManaProgr = manaProgress;
				mManaRect.width = mWidth * manaProgress;
			}
		}
	}
}
