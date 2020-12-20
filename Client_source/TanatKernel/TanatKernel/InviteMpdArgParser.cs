using System;
using AMF;

namespace TanatKernel
{
	internal class InviteMpdArgParser : CtrlPacketArgParser<InviteMpdArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref InviteMpdArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mTime = DateTime.Now;
			if (mixedArray.Associative.Count == 1)
			{
				int num = SafeGet(mixedArray, "time", -1);
				if (num > 0)
				{
					_arg.mTime = DateTime.Now.AddSeconds(num);
				}
			}
			else
			{
				_arg.mName = SafeGet(mixedArray, "nick", "");
				_arg.mClanName = SafeGet(mixedArray, "clan_name", "");
			}
		}
	}
}
