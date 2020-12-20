using TanatKernel;
using UnityEngine;

public class NickInfo : MonoBehaviour
{
	public float mYOffset = 5f;

	public Color mColor = Color.white;

	private bool mInited;

	private string mNick = string.Empty;

	private string mClanTag = string.Empty;

	private ObjTextInfo mObjTextInfo;

	private GameData mGameData;

	private HeroGameInfo mHeroData;

	public bool TryInit()
	{
		GameData component = GetComponent<GameData>();
		if (component == null)
		{
			return false;
		}
		if (!component.Data.IsPlayerBinded)
		{
			return false;
		}
		if (component.Data.TryGetPlayer == null)
		{
			return false;
		}
		mInited = true;
		mGameData = component;
		mNick = mGameData.Data.TryGetPlayer.Name;
		mClanTag = string.Empty;
		CreateObjText();
		return true;
	}

	private bool TrySetHeroData()
	{
		if (mGameData == null)
		{
			return false;
		}
		Player tryGetPlayer = mGameData.Data.TryGetPlayer;
		if (tryGetPlayer == null)
		{
			OnDisable();
			mGameData = null;
			return false;
		}
		if (tryGetPlayer.Hero == null)
		{
			return false;
		}
		mHeroData = tryGetPlayer.Hero.GameInfo;
		return mHeroData != null;
	}

	public void Update()
	{
		if ((mInited || TryInit()) && TrySetHeroData())
		{
			if (mClanTag != mHeroData.mClanTag)
			{
				mClanTag = mHeroData.mClanTag;
				CreateObjText();
			}
			if (!mObjTextInfo.Active)
			{
				CreateObjText();
			}
			Vector3 vector = base.gameObject.transform.position;
			vector.y += mYOffset;
			vector = Camera.main.WorldToScreenPoint(vector);
			mObjTextInfo.SetPos(vector);
		}
	}

	public void OnDisable()
	{
		mGameData = null;
		if (mObjTextInfo != null)
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Remove(mObjTextInfo);
			mObjTextInfo = null;
		}
	}

	private void CreateObjText()
	{
		if (GuiSystem.mGuiSystem.mLowLevelElements.Contains(mObjTextInfo))
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Remove(mObjTextInfo);
		}
		string data = ((!string.IsNullOrEmpty(mClanTag)) ? ("[" + mClanTag + "]" + mNick) : mNick);
		mObjTextInfo = new ObjTextInfo(data, mColor);
		mObjTextInfo.Init();
		GuiSystem.mGuiSystem.mLowLevelElements.Add(mObjTextInfo);
	}
}
