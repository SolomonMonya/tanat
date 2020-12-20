using TanatKernel;
using UnityEngine;

public class TouchableDetector : MonoBehaviour
{
	private BattleServerConnection mBattleSrv;

	public void Init(BattleServerConnection _battleSrv)
	{
		mBattleSrv = _battleSrv;
	}

	public void OnSelect()
	{
		if (mBattleSrv != null)
		{
			GameData component = base.gameObject.GetComponent<GameData>();
			if (!(component == null))
			{
				mBattleSrv.SendUse(component.Id);
			}
		}
	}
}
