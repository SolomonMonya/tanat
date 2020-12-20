namespace TanatKernel
{
	public interface IMapManager
	{
		void LoadBattleMap(BattleMapData _data, bool _isObserver, Notifier<IMapManager, object> _notifier);

		void UnloadCurrentBattleMap(Notifier<IMapManager, object> _notifier, bool _unloadRes);
	}
}
