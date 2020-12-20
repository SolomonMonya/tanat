using System;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class BattlePacket : NetPacket<BattleCmdId>, NetSystem.ITimeRegisterer
	{
		private bool mStatus = true;

		private string mError;

		private int mNum = -1;

		private BattlePacket mRequest;

		private DateTime mSendingTime;

		private DateTime mReceivingTime;

		private bool mRegSend;

		private bool mRegRecv;

		public bool Status => mStatus;

		public string Error => mError;

		public int Num => mNum;

		public BattlePacket Request => mRequest;

		public DateTime ReceivingTime => mReceivingTime;

		public BattlePacket(BattleCmdId _id, MixedArray _args, IPacketNumManager _numMgr)
		{
			if (_numMgr == null || _args == null)
			{
				throw new ArgumentNullException();
			}
			mId = _id;
			mArguments = _args;
			_numMgr.Register(this, out mNum);
		}

		public BattlePacket(Variable _content, IPacketNumManager _numMgr)
		{
			if (_content == null || _numMgr == null)
			{
				throw new ArgumentNullException();
			}
			if (typeof(MixedArray) != _content.ValueType)
			{
				throw new NetSystemException("invalid content type: " + _content.ValueType);
			}
			MixedArray mixedArray = _content;
			if (!mixedArray.Associative.TryGetValue("cmdId", out var value) || value.ValueType != typeof(int))
			{
				throw new NetSystemException("invalid command id");
			}
			mId = (BattleCmdId)value.Cast<int>();
			if (mixedArray.Associative.TryGetValue("status", out var value2))
			{
				mStatus = value2;
			}
			if (!mStatus && mixedArray.Associative.TryGetValue("error", out var value3))
			{
				mError = value3;
			}
			if (mixedArray.Associative.TryGetValue("requestId", out var value4))
			{
				mNum = value4;
				_numMgr.Unregister(mNum, out mRequest);
			}
			if (mixedArray.Associative.TryGetValue("arguments", out var value5) && value5.ValueType == typeof(MixedArray))
			{
				mArguments = value5;
			}
		}

		public override Variable Serialize()
		{
			MixedArray mixedArray = new MixedArray();
			mixedArray["arguments"] = mArguments;
			mixedArray["cmdId"] = (int)mId;
			mixedArray["requestId"] = mNum;
			return mixedArray;
		}

		public void RegisterSendingTime()
		{
			mSendingTime = DateTime.Now;
			mRegSend = true;
		}

		public void RegisterReceivingTime()
		{
			mReceivingTime = DateTime.Now;
			mRegRecv = true;
		}

		public bool HasRequest()
		{
			return mRequest != null;
		}

		public float GetPing()
		{
			if (!HasRequest())
			{
				Log.Error("no request");
				return 0f;
			}
			if (!mRegRecv || !mRequest.mRegSend)
			{
				Log.Error("invalid time registration state: " + mRegRecv + " " + mRequest.mRegSend);
				return 0f;
			}
			return (float)mReceivingTime.Subtract(mRequest.mSendingTime).TotalSeconds;
		}

		public float GetTransferTime()
		{
			float ping = GetPing();
			return ping * 0.5f;
		}
	}
}
