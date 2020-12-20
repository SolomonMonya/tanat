using System;
using AMF;

namespace TanatKernel
{
	public class CtrlPacket : NetPacket<Enum>
	{
		private int mStatus = -1;

		private int mError = -1;

		public int Status => mStatus;

		public int Error => mError;

		public CtrlPacket(Enum _cmd, MixedArray _args)
		{
			if (_cmd == null)
			{
				throw new ArgumentNullException();
			}
			mId = _cmd;
			mArguments = _args;
			if (_args.Associative.TryGetValue("status", out var value))
			{
				mStatus = value;
			}
			if (_args.Associative.TryGetValue("error", out value) && value.ValueType == typeof(int))
			{
				mError = value;
			}
		}

		private MixedArray BaseSerialize()
		{
			CtrlCmdId.CallPath callPath = CtrlCmdId.ToCallPath(mId);
			MixedArray mixedArray = new MixedArray();
			mixedArray["object"] = callPath.Obj;
			mixedArray["action"] = callPath.Act;
			if (mArguments != null)
			{
				mixedArray["params"] = mArguments;
			}
			return mixedArray;
		}

		public override Variable Serialize()
		{
			return BaseSerialize();
		}

		public Variable SerializeWithSession(string _uid, string _key, uint _packetNumber)
		{
			MixedArray mixedArray = BaseSerialize();
			mixedArray["sess_uid"] = _uid;
			mixedArray["sess_key"] = _key;
			mixedArray["counter"] = _packetNumber;
			return mixedArray;
		}
	}
}
