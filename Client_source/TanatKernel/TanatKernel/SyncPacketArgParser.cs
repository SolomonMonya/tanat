using System;

namespace TanatKernel
{
	internal class SyncPacketArgParser : BattlePacketArgParser<SyncPacket>
	{
		private TrackingIdManager mTrackingIdMgr;

		public SyncPacketArgParser(TrackingIdManager _trackingIdMgr)
		{
			if (_trackingIdMgr == null)
			{
				throw new ArgumentNullException();
			}
			mTrackingIdMgr = _trackingIdMgr;
		}

		protected override void ParseArg(BattlePacket _packet, ref SyncPacket _arg)
		{
			byte[] buffer = SafeGet(_packet, "data", new byte[0]);
			_arg.Parse(buffer, mTrackingIdMgr);
		}
	}
}
