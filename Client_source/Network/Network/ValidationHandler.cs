using System;
using System.Runtime.CompilerServices;

namespace Network
{
	public class ValidationHandler<PacketT> : IHandler<PacketT>
	{
		public delegate void Callback();

		public delegate void ErrorCallback(int _errorCode);

		protected Callback mSuccessCallback = delegate
		{
		};

		protected ErrorCallback mFailCallback = delegate
		{
		};

		private uint mRequestCnt;

		private uint mDoneCnt;

		private PacketValidator<PacketT> mValidator;

		[CompilerGenerated]
		private static Callback _003C_003E9__CachedAnonymousMethodDelegate2;

		[CompilerGenerated]
		private static ErrorCallback _003C_003E9__CachedAnonymousMethodDelegate3;

		public void SetValidator(PacketValidator<PacketT> _validator)
		{
			mValidator = _validator;
		}

		public uint GetRequestCnt()
		{
			return mRequestCnt;
		}

		public uint GetDoneCnt()
		{
			return mDoneCnt;
		}

		public virtual bool Perform(PacketT _packet)
		{
			mRequestCnt++;
			if (mValidator != null && !mValidator.Validate(_packet, out var _errorCode))
			{
				mFailCallback(_errorCode);
				return false;
			}
			mSuccessCallback();
			mDoneCnt++;
			return true;
		}

		public void SubscribeSuccess(Callback _success)
		{
			mSuccessCallback = (Callback)Delegate.Combine(mSuccessCallback, _success);
		}

		public void SubscribeFail(ErrorCallback _fail)
		{
			mFailCallback = (ErrorCallback)Delegate.Combine(mFailCallback, _fail);
		}

		public void UnsubscribeSuccess(Callback _success)
		{
			mSuccessCallback = (Callback)Delegate.Remove(mSuccessCallback, _success);
		}

		public void UnsubscribeFail(ErrorCallback _fail)
		{
			mFailCallback = (ErrorCallback)Delegate.Remove(mFailCallback, _fail);
		}

		public virtual void Unsubscribe(object _receiver)
		{
			Delegate[] invocationList = mSuccessCallback.GetInvocationList();
			Delegate[] array = invocationList;
			foreach (Delegate @delegate in array)
			{
				if (@delegate.Target == _receiver)
				{
					mSuccessCallback = (Callback)Delegate.Remove(mSuccessCallback, @delegate as Callback);
				}
			}
			invocationList = mFailCallback.GetInvocationList();
			Delegate[] array2 = invocationList;
			foreach (Delegate delegate2 in array2)
			{
				if (delegate2.Target == _receiver)
				{
					mFailCallback = (ErrorCallback)Delegate.Remove(mFailCallback, delegate2 as ErrorCallback);
				}
			}
		}
	}
}
