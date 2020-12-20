namespace TanatKernel
{
	internal class UserLeaveInfoArgParser : CtrlPacketArgParser<UserLeaveInfoArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref UserLeaveInfoArg _arg)
		{
			_arg.mCurrentKarma = SafeGet<int>(_packet, "current_karma");
			_arg.mNewKarma = SafeGet<int>(_packet, "new_karma");
			_arg.mLabels = SafeGet<int>(_packet, "labels");
			_arg.mLabelsLimit = SafeGet<int>(_packet, "labels_limit");
			_arg.mTime = SafeGet<int>(_packet, "time");
		}
	}
}
