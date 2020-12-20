using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class Shop : MonoBehaviour
{
	public float mRadius = 15f;

	private bool mOpened;

	private static int mOpenedShop = -1;

	private ShopMenu mWnd;

	private IStoreContentProvider<BattlePrototype> mProtoProv;

	private IStoreContentProvider<CtrlPrototype> mCtrlProtoProv;

	private PlayerControl mPlayerCtrl;

	private GameData mGameData;

	public void Init(ShopMenu _wnd, IStoreContentProvider<BattlePrototype> _protoProv, IStoreContentProvider<CtrlPrototype> _ctrlPrototypes, PlayerControl _playerCtrl)
	{
		mWnd = _wnd;
		mProtoProv = _protoProv;
		mCtrlProtoProv = _ctrlPrototypes;
		mPlayerCtrl = _playerCtrl;
		mGameData = base.gameObject.GetComponent<GameData>();
	}

	public void OnSelect()
	{
		if (mWnd == null)
		{
			Log.Warning("uninited shop");
		}
		else if (CheckDistance())
		{
			mWnd.SetData(mGameData.Id, mGameData.Proto, mProtoProv, mCtrlProtoProv, mPlayerCtrl.SelfPlayer, mPlayerCtrl.SelfPlayer, mPlayerCtrl);
			mWnd.SetActive(_active: true);
			mOpened = true;
			mOpenedShop = GetInstanceID();
		}
	}

	private bool CheckDistance()
	{
		Player player = mPlayerCtrl.SelfPlayer.Player;
		if (!player.IsAvatarBinded)
		{
			return false;
		}
		IMovable data = player.Avatar.Data;
		data.GetPosition(out var _x, out var _y);
		Vector3 position = base.gameObject.transform.position;
		_x -= position.x;
		_y -= position.z;
		float num = Mathf.Sqrt(_x * _x + _y * _y);
		return num < mRadius;
	}

	private void Update()
	{
		if (mOpened && !CheckDistance())
		{
			Close();
		}
	}

	public void Close()
	{
		mWnd.SetActive(_active: false);
		mOpened = false;
		mOpenedShop = -1;
	}

	public void OnDisable()
	{
		if (GetInstanceID() == mOpenedShop)
		{
			Close();
		}
	}
}
