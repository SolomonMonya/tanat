namespace TanatKernel
{
	public interface IPacketNumManager
	{
		void Register(BattlePacket _packet, out int _num);

		bool Unregister(int _num, out BattlePacket _request);
	}
}
