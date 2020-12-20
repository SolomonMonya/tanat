using TanatKernel;
using UnityEngine;

public class ShortObjectInfo : MonoBehaviour
{
	public float mYOffset = 5f;

	private GameData mGameData;

	private Transform mTrans;

	private string mElementId;

	private ShortGuiObjInfo mShortGuiObjInfo;

	private TipMgr mTipMgr;

	private int mCurTipId = -1;

	private bool mTipEnabled;

	private void InitData()
	{
		mTrans = base.gameObject.transform;
		mGameData = base.gameObject.GetComponent<GameData>();
		if (mGameData == null)
		{
			Object.Destroy(this);
			return;
		}
		string curGuiSetId = GuiSystem.mGuiSystem.GetCurGuiSetId();
		if (mTipMgr == null)
		{
			mTipMgr = GuiSystem.mGuiSystem.GetGuiElement<TipMgr>(curGuiSetId, "TIP_MGR");
		}
		InitGuiObjInfo();
	}

	private void InitGuiObjInfo()
	{
		bool flag = mGameData.Proto.Avatar != null;
		bool flag2 = mGameData.Data.IsPlayerBinded && mGameData.Data.Player.IsSelf;
		mShortGuiObjInfo = new ShortGuiObjInfo();
		mShortGuiObjInfo.Init(mGameData.Data.Params, flag, mGameData.Data.GetFriendliness());
		mShortGuiObjInfo.SetActive(_active: false);
		mElementId = ((!flag) ? "NOT_AVATAR" : "AVATAR");
		mShortGuiObjInfo.mElementId = mElementId;
		int count = GuiSystem.mGuiSystem.mLowLevelElements.Count;
		if (flag && flag2)
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Insert(count, mShortGuiObjInfo);
		}
		else if (flag && !flag2)
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Insert((count != 0) ? (count - 1) : 0, mShortGuiObjInfo);
		}
		else
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Insert(0, mShortGuiObjInfo);
		}
	}

	public void Update()
	{
		if (mGameData == null)
		{
			InitData();
		}
		else if (!mGameData.Data.Relevant || !mGameData.Data.Visible)
		{
			OnDisable();
		}
		else if (mShortGuiObjInfo.mElementId == "AVATAR" && (OptionsMgr.mShowAvatarHealthBar || GuiSystem.mRenderLowLevelElements))
		{
			UpdatePos();
		}
		else if (mShortGuiObjInfo.mElementId == "NOT_AVATAR" && (OptionsMgr.mShowOtherHealthBar || GuiSystem.mRenderLowLevelElements))
		{
			UpdatePos();
		}
		else if (mShortGuiObjInfo.mElementId == string.Empty)
		{
			UpdatePos();
		}
	}

	private void UpdatePos()
	{
		mShortGuiObjInfo.DrawPos = GetGuiPos();
		if (!mShortGuiObjInfo.Active)
		{
			mShortGuiObjInfo.SetActive(_active: true);
		}
	}

	private Vector3 GetGuiPos()
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return Vector3.zero;
		}
		if (mTrans == null)
		{
			return Vector3.zero;
		}
		Vector3 position = mTrans.position;
		position.y += mYOffset;
		return main.WorldToScreenPoint(position);
	}

	public void OnMouseEnter()
	{
		if (mTipEnabled && mTipMgr != null && !(mGameData == null))
		{
			if (mGameData.Data.IsPlayerBinded)
			{
				Player player = mGameData.Data.Player;
				string name = player.Name;
				string data = GuiSystem.GetLocaleText("Level_Text") + " : " + (mGameData.Data.Level + 1);
				mCurTipId = mTipMgr.ShowTip(GetGuiPos(), name, data);
			}
			if (mShortGuiObjInfo != null)
			{
				mShortGuiObjInfo.mElementId = string.Empty;
			}
		}
	}

	public void OnMouseOver()
	{
		if (mCurTipId != -1)
		{
			mTipMgr.SetTipPos(mCurTipId, GetGuiPos());
		}
	}

	public void OnMouseExit()
	{
		HideTip();
		if (mShortGuiObjInfo != null)
		{
			mShortGuiObjInfo.mElementId = mElementId;
		}
	}

	private void HideTip()
	{
		if (mCurTipId != -1)
		{
			mTipMgr.HideTip(mCurTipId);
			mCurTipId = -1;
		}
	}

	public void DisableTip()
	{
		HideTip();
		mTipEnabled = false;
	}

	public void OnDestroy()
	{
		if (mShortGuiObjInfo != null)
		{
			GuiSystem.mGuiSystem.mLowLevelElements.Remove(mShortGuiObjInfo);
		}
		DisableTip();
	}

	public void OnEnable()
	{
		if (mShortGuiObjInfo != null)
		{
			mShortGuiObjInfo.mElementId = mElementId;
			mShortGuiObjInfo.SetActive(_active: true);
			UpdatePos();
		}
		mTipEnabled = true;
	}

	public void OnDisable()
	{
		HideTip();
		if (mShortGuiObjInfo != null)
		{
			mShortGuiObjInfo.SetActive(_active: false);
		}
	}

	public void OnBecameVisible()
	{
		if (!base.enabled)
		{
			base.enabled = true;
		}
	}

	public void OnBecameInvisible()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
	}
}
