namespace Network
{
	public interface ArgumentParser<PacketT, ArgT>
	{
		bool Parse(PacketT _packet, out ArgT _arg);
	}
}
