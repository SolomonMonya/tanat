using TanatKernel;
using UnityEngine;

public class EffectEmiter : MonoBehaviour
{
	private static float mRespWaitTime = 5f;

	public string mRebornGFX = "VFX_reborn_prop01";

	public string mRebornSFX = "resurrect";

	public string mLevelUpGFX = "VFX_LevelUp_prop01";

	public string mLevelUpSFX = "interface_level_up";

	private bool mReborn = true;

	private GameData mGameData;

	public void Start()
	{
		mGameData = base.gameObject.GetComponent<GameData>();
		if (mGameData.Data == null)
		{
			mGameData = null;
		}
	}

	public void Update()
	{
		if (!mReborn || !mGameData || !mGameData.Data.IsPlayerBinded || !mGameData.Data.Visible)
		{
			return;
		}
		Player player = mGameData.Data.Player;
		if (player != null && player.RespTime == null && player.LastRespawnTime > 0f)
		{
			mReborn = false;
			if (mGameData.Data.Time - player.LastRespawnTime < mRespWaitTime)
			{
				PlayReborn();
			}
		}
	}

	private void PlayReborn()
	{
		if (base.gameObject.active)
		{
			VisualEffectsMgr.Instance.PlayEffect(mRebornGFX, base.gameObject);
			SoundSystem.Instance.PlaySoundEvent(mRebornSFX, base.gameObject);
		}
	}

	public void WaitReborn()
	{
		mReborn = true;
	}

	public void PlayLevelUp()
	{
		if (base.gameObject.active && VisibilityMgr.IsVisible(base.gameObject))
		{
			VisualEffectsMgr.Instance.PlayEffect(mLevelUpGFX, base.gameObject);
			SoundSystem.Instance.PlaySoundEvent(mLevelUpSFX, base.gameObject);
		}
	}
}
