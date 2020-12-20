using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	internal class ChatMsgArgParser : CtrlPacketArgParser<ChatMsgArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ChatMsgArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mMsg = SafeGet<string>(mixedArray, "msg");
			_arg.mType = SafeGet<string>(mixedArray, "type");
			_arg.mTime = SafeGet<int>(mixedArray, "ctime");
			_arg.mFrom = SafeGet<string>(mixedArray, "from");
			List<string> list = new List<string>();
			MixedArray mixedArray2 = mixedArray.TryGet("recipient_list", new MixedArray());
			foreach (Variable item in mixedArray2.Dense)
			{
				list.Add(item);
			}
			_arg.mRecipients = list.ToArray();
		}
	}
}
