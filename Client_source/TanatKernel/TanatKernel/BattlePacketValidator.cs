using Log4Tanat;
using Network;

namespace TanatKernel
{
	internal class BattlePacketValidator : PacketValidator<BattlePacket>
	{
		public virtual bool Validate(BattlePacket _packet, out int _errorCode)
		{
			bool status = _packet.Status;
			if (status)
			{
				_errorCode = 0;
			}
			else
			{
				_errorCode = 1;
				Log.Warning(string.Concat(_packet.Id, " ", _packet.Error));
			}
			return status;
		}
	}
}
