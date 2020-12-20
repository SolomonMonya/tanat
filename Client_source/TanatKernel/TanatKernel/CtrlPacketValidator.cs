using Log4Tanat;
using Network;

namespace TanatKernel
{
	internal class CtrlPacketValidator : PacketValidator<CtrlPacket>
	{
		public virtual bool Validate(CtrlPacket _packet, out int _errorCode)
		{
			_errorCode = -1;
			bool flag = _packet.Status == 100;
			if (!flag)
			{
				_errorCode = _packet.Error;
				Log.Warning(string.Concat(_packet.Id, " not valid, status: ", _packet.Status, "\n", _packet.Arguments.ToString()));
			}
			return flag;
		}
	}
}
