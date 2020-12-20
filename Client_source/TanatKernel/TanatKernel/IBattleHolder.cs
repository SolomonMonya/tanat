namespace TanatKernel
{
	public interface IBattleHolder
	{
		void CreateBattle(BattlePacketManager _packetMgr, int _selfPlayerId);

		bool IsBattleCreated();

		void StartBattle();

		void DisableBattle();

		void DestroyBattle();
	}
}
