using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	public class TradeSender
	{
		private CtrlEntryPoint mEntryPoint;

		public TradeSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void Start(int _userId)
		{
			mEntryPoint.Send(CtrlCmdId.trade.start, new NamedVar("user_id", _userId));
		}

		public void Answare(bool _answer)
		{
			mEntryPoint.Send(CtrlCmdId.trade.start_answer, new NamedVar("answer", _answer));
		}

		public void Ready(int _money, IDictionary<int, int> _items)
		{
			if (_items == null)
			{
				throw new ArgumentNullException("_items");
			}
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> _item in _items)
			{
				mixedArray[_item.Key.ToString()] = _item.Value;
			}
			mEntryPoint.Send(CtrlCmdId.trade.ready, new NamedVar("items", mixedArray), new NamedVar("money", _money));
		}

		public void NotReady()
		{
			mEntryPoint.Send(CtrlCmdId.trade.not_ready);
		}

		public void Cancel()
		{
			mEntryPoint.Send(CtrlCmdId.trade.cancel);
		}

		public void Confirm()
		{
			mEntryPoint.Send(CtrlCmdId.trade.confirm);
		}

		public void UpdateBag()
		{
			mEntryPoint.Send(CtrlCmdId.user.bag);
		}
	}
}
