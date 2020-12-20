using System;
using TanatKernel;

public class PopupEvent
{
	private PopupInfo mWnd;

	private IStoreContentProvider<BattlePrototype> mBattleProtoProv;

	private IStoreContentProvider<CtrlPrototype> mCtrlProtoProv;

	private IStoreContentProvider<BattleThing> mBattleInventory;

	private IStoreContentProvider<CtrlThing> mCtrlInventory;

	private PlayerControl mPlayerCtrl;

	private static PopupEvent mInstance;

	public static void Init(PopupInfo _wnd, IStoreContentProvider<BattlePrototype> _battleProtoProv, IStoreContentProvider<CtrlPrototype> _ctrlProtoProv, IStoreContentProvider<BattleThing> _battleInventory, IStoreContentProvider<CtrlThing> _ctrlInventory, PlayerControl _playerCtrl)
	{
		if (_wnd == null)
		{
			throw new ArgumentNullException("_wnd");
		}
		if (_battleProtoProv == null)
		{
			throw new ArgumentNullException("_battleProtoProv");
		}
		if (_ctrlProtoProv == null)
		{
			throw new ArgumentNullException("_ctrlProtoProv");
		}
		mInstance = new PopupEvent();
		mInstance.mWnd = _wnd;
		mInstance.mBattleProtoProv = _battleProtoProv;
		mInstance.mCtrlProtoProv = _ctrlProtoProv;
		mInstance.mBattleInventory = _battleInventory;
		mInstance.mCtrlInventory = _ctrlInventory;
		mInstance.mPlayerCtrl = _playerCtrl;
	}

	public static void Destroy()
	{
		mInstance = null;
	}

	public static void ShowBattlePopup(int _protoId)
	{
		if (mInstance != null)
		{
			BattlePrototype battlePrototype = mInstance.mBattleProtoProv.TryGet(_protoId);
			if (battlePrototype != null)
			{
				mInstance.mWnd.ShowDesc(battlePrototype);
			}
		}
	}

	public static void ShowCtrlPopup(int _protoId)
	{
		if (mInstance != null)
		{
			CtrlPrototype ctrlPrototype = mInstance.mCtrlProtoProv.TryGet(_protoId);
			if (ctrlPrototype != null)
			{
				mInstance.mWnd.ShowDesc(ctrlPrototype);
			}
		}
	}

	public static void OnHintGuiMouseOver(GuiElement _sender)
	{
		if (mInstance != null)
		{
			mInstance.mWnd.ShowHint(_sender.mElementId + "-" + _sender.mId);
		}
	}

	public static void OnShowAOE(GuiElement _sender)
	{
		if (mInstance.mPlayerCtrl != null)
		{
			mInstance.mPlayerCtrl.ShowSkillAoeView(_sender.mId);
		}
	}

	public static void OnHideAOE()
	{
		if (mInstance.mPlayerCtrl != null)
		{
			mInstance.mPlayerCtrl.HideAoeView();
		}
	}

	public static void OnHintItemMouseOver(GuiElement _sender)
	{
		if (mInstance != null && mInstance.mBattleInventory != null)
		{
			BattleThing battleThing = mInstance.mBattleInventory.TryGet(_sender.mId);
			if (battleThing != null)
			{
				mInstance.mWnd.ShowDesc(battleThing.BattleProto);
			}
			if (mInstance.mPlayerCtrl != null)
			{
				mInstance.mPlayerCtrl.ShowItemAoeView(battleThing);
			}
		}
	}

	public static void OnHintCtrlProtoMouseOver(GuiElement _sender)
	{
		ShowCtrlPopup(_sender.mId);
	}

	public static void OnHintCtrlItemMouseOver(GuiElement _sender)
	{
		if (mInstance != null && mInstance.mCtrlInventory != null)
		{
			CtrlThing ctrlThing = mInstance.mCtrlInventory.TryGet(_sender.mId);
			if (ctrlThing != null)
			{
				mInstance.mWnd.ShowDesc(ctrlThing.CtrlProto);
			}
		}
	}

	public static void OnMouseLeave(GuiElement _sender)
	{
		if (mInstance != null)
		{
			mInstance.mWnd.Hide();
			OnHideAOE();
		}
	}
}
