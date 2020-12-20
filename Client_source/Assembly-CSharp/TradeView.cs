using System;
using System.Collections.Generic;
using TanatKernel;

public class TradeView : IGuiView
{
	private TradeManager mTradeMgr;

	private PlayerTradeMenu mTradeWnd;

	private YesNoDialog mYesNoDialogWnd;

	private OkDialog mOkDialogWnd;

	public TradeView(PlayerTradeMenu _tradeWnd, YesNoDialog _yesNoDialog, OkDialog _okDialog)
	{
		if (_tradeWnd == null)
		{
			throw new ArgumentNullException("_tradeWnd");
		}
		if (_yesNoDialog == null)
		{
			throw new ArgumentNullException("_yesNoDialog");
		}
		if (_okDialog == null)
		{
			throw new ArgumentNullException("_okDialog");
		}
		mTradeWnd = _tradeWnd;
		mYesNoDialogWnd = _yesNoDialog;
		mOkDialogWnd = _okDialog;
	}

	public void Ask(Type _listenerType, int _textId, object _textData, Notifier<IGuiView, object> _notifier)
	{
		if (_listenerType == typeof(TradeManager))
		{
			string text = "EMPTY!";
			if (_textId == 0)
			{
				text = GuiSystem.GetLocaleText("GUI_TRADE_START_REQUEST");
				string newValue = (string)_textData;
				text = text.Replace("{NAME}", newValue);
			}
			YesNoDialog.OnAnswer callback = delegate(bool _answer)
			{
				_notifier.Call(_answer, this);
			};
			mYesNoDialogWnd.SetData(text, "YES_TEXT", "NO_TEXT", callback);
		}
	}

	public void Inform(Type _listenerType, int _textId, object _textData)
	{
		if (_listenerType == typeof(TradeManager))
		{
			string data = "EMPTY!";
			switch (_textId)
			{
			case 1:
			{
				data = GuiSystem.GetLocaleText("GUI_TRADE_START_DENIED");
				string newValue = (string)_textData;
				data = data.Replace("{NAME}", newValue);
				break;
			}
			case 2:
				data = GuiSystem.GetLocaleText("GUI_TRADE_START_ERROR");
				break;
			case 3019:
				data = GuiSystem.GetLocaleText("GUI_TRADE_START_ERROR_SELF_LVL");
				break;
			case 3020:
				data = GuiSystem.GetLocaleText("GUI_TRADE_START_ERROR_PLYER_LVL");
				break;
			case 3021:
				data = GuiSystem.GetLocaleText("GUI_TRADE_START_ERROR_PLYER_IGNORE");
				break;
			}
			mOkDialogWnd.SetData(data);
		}
	}

	public void Skip(Type _listenerType)
	{
		if (_listenerType == typeof(TradeManager))
		{
		}
	}

	private void OnStateChanged(TradeManager _tradeMgr)
	{
		if (_tradeMgr.IsSessionOpened && _tradeMgr.CheckState(TradeManager.State.STARTED))
		{
			if (!mTradeWnd.Active)
			{
				UserLog.AddAction(UserActionType.TRADE_WINDOW_OPENED);
			}
			mTradeWnd.Open();
			mTradeWnd.SetStatus(_tradeMgr.CheckState(TradeManager.State.READY_SELF));
			mTradeWnd.SetEnemyStatus(_tradeMgr.CheckState(TradeManager.State.READY_OPPONENT));
			mTradeWnd.SetTraderCost(_tradeMgr.GetOpponentMoney());
			mTradeWnd.SetEnemyItems(_tradeMgr.GetOpponentItems());
		}
		else
		{
			mTradeWnd.OnTradeSuccess();
			mTradeWnd.SetActive(_active: false);
		}
	}

	public void OpenSession(int _playerId)
	{
		if (mTradeMgr != null)
		{
			mTradeMgr.OpenSession(_playerId);
		}
	}

	public void Init(TradeManager _tradeMgr)
	{
		if (_tradeMgr == null)
		{
			throw new ArgumentNullException("_tradeMgr");
		}
		Uninit();
		mTradeMgr = _tradeMgr;
		mTradeMgr.SubscribeOnStateChanged(OnStateChanged);
		PlayerTradeMenu playerTradeMenu = mTradeWnd;
		playerTradeMenu.mReadyCallback = (PlayerTradeMenu.ReadyCallback)Delegate.Combine(playerTradeMenu.mReadyCallback, new PlayerTradeMenu.ReadyCallback(OnReady));
		PlayerTradeMenu playerTradeMenu2 = mTradeWnd;
		playerTradeMenu2.mBarterCallback = (PlayerTradeMenu.BarterCallback)Delegate.Combine(playerTradeMenu2.mBarterCallback, new PlayerTradeMenu.BarterCallback(OnConfirm));
		PlayerTradeMenu playerTradeMenu3 = mTradeWnd;
		playerTradeMenu3.mCancelCallback = (PlayerTradeMenu.CancelCallback)Delegate.Combine(playerTradeMenu3.mCancelCallback, new PlayerTradeMenu.CancelCallback(OnCancel));
	}

	public void Uninit()
	{
		if (mTradeMgr != null)
		{
			mTradeMgr.UnsubscribeOnStateChanged(OnStateChanged);
			mTradeMgr = null;
		}
		PlayerTradeMenu playerTradeMenu = mTradeWnd;
		playerTradeMenu.mReadyCallback = (PlayerTradeMenu.ReadyCallback)Delegate.Remove(playerTradeMenu.mReadyCallback, new PlayerTradeMenu.ReadyCallback(OnReady));
		PlayerTradeMenu playerTradeMenu2 = mTradeWnd;
		playerTradeMenu2.mBarterCallback = (PlayerTradeMenu.BarterCallback)Delegate.Remove(playerTradeMenu2.mBarterCallback, new PlayerTradeMenu.BarterCallback(OnConfirm));
		PlayerTradeMenu playerTradeMenu3 = mTradeWnd;
		playerTradeMenu3.mCancelCallback = (PlayerTradeMenu.CancelCallback)Delegate.Remove(playerTradeMenu3.mCancelCallback, new PlayerTradeMenu.CancelCallback(OnCancel));
	}

	private void OnReady(bool _ready, int _money, IDictionary<int, int> _items)
	{
		if (_ready)
		{
			mTradeMgr.SetSelfMoney(_money);
			mTradeMgr.SetSelfItems(_items);
			mTradeMgr.Ready();
		}
		else
		{
			mTradeMgr.NotReady();
		}
	}

	private void OnConfirm()
	{
		mTradeMgr.Confirm();
	}

	private void OnCancel()
	{
		mTradeMgr.Cancel();
	}
}
