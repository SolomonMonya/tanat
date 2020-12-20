namespace Network
{
	public interface IHandler<PacketT>
	{
		bool Perform(PacketT _packet);
	}
}
