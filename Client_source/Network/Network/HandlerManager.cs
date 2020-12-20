using System;
using System.Collections.Generic;
using Log4Tanat;

namespace Network
{
	public class HandlerManager<PacketT, IdT>
	{
		private Dictionary<IdT, IHandler<PacketT>> mHandlers = new Dictionary<IdT, IHandler<PacketT>>();

		private Dictionary<IdT, object> mValidators = new Dictionary<IdT, object>();

		private Dictionary<Type, object> mParsers = new Dictionary<Type, object>();

		public void Perform(PacketT _packet, IdT _id)
		{
			IHandler<PacketT> handler = GetHandler(_id);
			if (handler != null)
			{
				Log.Info(() => "performing " + _id.ToString());
				if (!handler.Perform(_packet))
				{
					Log.Warning("cannot process packet " + _id);
				}
			}
			else
			{
				Log.Warning("unregistered handler " + _id);
			}
		}

		public IHandler<PacketT> GetHandler(IdT _id)
		{
			mHandlers.TryGetValue(_id, out var value);
			return value;
		}

		public ValidationHandler<PacketT> GetValidationHandler(IdT _id)
		{
			IHandler<PacketT> handler = GetHandler(_id);
			if (handler == null)
			{
				return null;
			}
			return handler as ValidationHandler<PacketT>;
		}

		public DataHandler<PacketT, ArgT> GetDataHandler<ArgT>(IdT _id)
		{
			IHandler<PacketT> handler = GetHandler(_id);
			if (handler == null)
			{
				return null;
			}
			return handler as DataHandler<PacketT, ArgT>;
		}

		public void RegisterParser<ArgT>(ArgumentParser<PacketT, ArgT> _parser)
		{
			mParsers[typeof(ArgT)] = _parser;
		}

		public void RegisterValidator(IdT _id, PacketValidator<PacketT> _validator)
		{
			mValidators[_id] = _validator;
		}

		private void InitValidator(ValidationHandler<PacketT> _handler, IdT _id)
		{
			if (mValidators.TryGetValue(_id, out var value))
			{
				_handler.SetValidator(value as PacketValidator<PacketT>);
			}
		}

		private void InitParser<ArgT>(DataHandler<PacketT, ArgT> _handler)
		{
			if (mParsers.TryGetValue(typeof(ArgT), out var value))
			{
				_handler.SetParser(value as ArgumentParser<PacketT, ArgT>);
			}
		}

		public void RegisterValidation(IdT _id)
		{
			ValidationHandler<PacketT> validationHandler = new ValidationHandler<PacketT>();
			InitValidator(validationHandler, _id);
			mHandlers[_id] = validationHandler;
		}

		public void Register<ArgT>(IdT _id)
		{
			DataHandler<PacketT, ArgT> dataHandler = new DataHandler<PacketT, ArgT>();
			InitValidator(dataHandler, _id);
			InitParser(dataHandler);
			mHandlers[_id] = dataHandler;
		}

		public void Subscribe(IdT _id, ValidationHandler<PacketT>.Callback _success, ValidationHandler<PacketT>.ErrorCallback _fail)
		{
			ValidationHandler<PacketT> validationHandler = GetValidationHandler(_id);
			if (validationHandler == null)
			{
				Log.Error("cannot subscribe " + _id);
				return;
			}
			validationHandler.SubscribeSuccess(_success);
			validationHandler.SubscribeFail(_fail);
		}

		public void Subscribe<ArgT>(IdT _id, ValidationHandler<PacketT>.Callback _success, ValidationHandler<PacketT>.ErrorCallback _fail, DataHandler<PacketT, ArgT>.RecvDataCallback _recvData)
		{
			DataHandler<PacketT, ArgT> dataHandler = GetDataHandler<ArgT>(_id);
			if (dataHandler == null)
			{
				Log.Error("cannot subscribe " + _id);
				return;
			}
			dataHandler.SubscribeSuccess(_success);
			dataHandler.SubscribeFail(_fail);
			dataHandler.SubscribeRecvData(_recvData);
		}

		public void Unsubscribe(IdT _id, ValidationHandler<PacketT>.Callback _success, ValidationHandler<PacketT>.ErrorCallback _fail)
		{
			ValidationHandler<PacketT> validationHandler = GetValidationHandler(_id);
			if (validationHandler == null)
			{
				Log.Error("cannot subscribe " + _id);
				return;
			}
			validationHandler.UnsubscribeSuccess(_success);
			validationHandler.UnsubscribeFail(_fail);
		}

		public void Unsubscribe<ArgT>(IdT _id, ValidationHandler<PacketT>.Callback _success, ValidationHandler<PacketT>.ErrorCallback _fail, DataHandler<PacketT, ArgT>.RecvDataCallback _recvData)
		{
			DataHandler<PacketT, ArgT> dataHandler = GetDataHandler<ArgT>(_id);
			if (dataHandler == null)
			{
				Log.Error("cannot subscribe " + _id);
				return;
			}
			dataHandler.UnsubscribeSuccess(_success);
			dataHandler.UnsubscribeFail(_fail);
			dataHandler.UnsubscribeRecvData(_recvData);
		}

		public void Unsubscribe(IdT _id, object _receiver)
		{
			ValidationHandler<PacketT> validationHandler = GetValidationHandler(_id);
			if (validationHandler == null)
			{
				Log.Error("cannot unsubscribe " + _id);
			}
			else
			{
				validationHandler.Unsubscribe(_receiver);
			}
		}

		public void Unsubscribe(object _receiver)
		{
			foreach (KeyValuePair<IdT, IHandler<PacketT>> mHandler in mHandlers)
			{
				ValidationHandler<PacketT> validationHandler = mHandler.Value as ValidationHandler<PacketT>;
				validationHandler.Unsubscribe(_receiver);
			}
		}
	}
}
