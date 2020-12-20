namespace Network
{
	public interface PacketValidator<PacketT>
	{
		bool Validate(PacketT _packet, out int _errorCode);
	}
}
