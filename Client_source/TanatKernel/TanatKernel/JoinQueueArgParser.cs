namespace TanatKernel
{
	internal class JoinQueueArgParser : CtrlPacketArgParser<JoinQueueArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref JoinQueueArg _arg)
		{
			_arg.mStatus = _packet.Arguments.TryGet("join_status", 0);
			if (_arg.mStatus == 0)
			{
				_arg.mRequestId = SafeGet<int>(_packet, "request_id");
				_arg.mMapId = _packet.Arguments.TryGet("map_id", -1);
			}
		}
	}
}
