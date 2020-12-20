using UnityEngine;

public class CursorDetector : MonoBehaviour
{
	private IPlayerControl mPlayerCtrl;

	private GameData mGameData;

	public void Init(IPlayerControl _playerCtrl)
	{
		mPlayerCtrl = _playerCtrl;
		mGameData = base.gameObject.GetComponent<GameData>();
	}

	public void OnMouseEnter()
	{
		if (base.enabled && mPlayerCtrl != null)
		{
			mPlayerCtrl.Mark(base.gameObject);
		}
	}

	public void OnMouseExit()
	{
		if (base.enabled && mPlayerCtrl != null)
		{
			mPlayerCtrl.UnmarkCurrent();
		}
	}

	public void OnDisable()
	{
		if (mPlayerCtrl != null && !(mGameData == null) && mPlayerCtrl.MarkedObjectId == mGameData.Id)
		{
			mPlayerCtrl.UnmarkCurrent();
		}
	}
}
