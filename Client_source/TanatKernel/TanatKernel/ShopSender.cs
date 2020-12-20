using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	public class ShopSender
	{
		private CtrlEntryPoint mEntryPoint;

		public ShopSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void ContentRequest(ShopType _type)
		{
			switch (_type)
			{
			case ShopType.SIMPLE:
				mEntryPoint.Send(CtrlCmdId.store.list);
				break;
			case ShopType.REAL:
				mEntryPoint.Send(CtrlCmdId.store.list, new NamedVar("money_d", true));
				break;
			case ShopType.AVATAR:
				mEntryPoint.Send(CtrlCmdId.store.list, new NamedVar("avatar", true));
				break;
			}
		}

		public void ContentRequest(MapType _mapType)
		{
			mEntryPoint.Send(CtrlCmdId.store.list, new NamedVar("type", (int)_mapType));
		}

		public void Buy(IDictionary<int, int> _basket)
		{
			if (_basket == null)
			{
				throw new ArgumentNullException("_basket");
			}
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> item in _basket)
			{
				mixedArray[item.Key.ToString()] = item.Value;
			}
			mEntryPoint.Send(CtrlCmdId.store.buy, new NamedVar("basket", mixedArray));
		}

		public void Buy(IDictionary<int, int> _basket, int _mapType)
		{
			if (_basket == null)
			{
				throw new ArgumentNullException("_basket");
			}
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> item in _basket)
			{
				mixedArray[item.Key.ToString()] = item.Value;
			}
			mEntryPoint.Send(CtrlCmdId.store.buy, new NamedVar("basket", mixedArray), new NamedVar("type", _mapType));
		}

		public void Sell(IDictionary<int, int> _basket)
		{
			if (_basket == null)
			{
				throw new ArgumentNullException("_basket");
			}
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> item in _basket)
			{
				mixedArray[item.Key.ToString()] = item.Value;
			}
			mEntryPoint.Send(CtrlCmdId.store.sell, new NamedVar("basket", mixedArray));
		}
	}
}
