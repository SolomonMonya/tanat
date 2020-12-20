namespace TanatKernel
{
	public interface IGameObjectManager
	{
		IGameObject CreateGameObject(int _objectId, BattlePrototype _proto);

		void CreateGameObjectAsync(int _objectId, BattlePrototype _proto, Notifier<IGameObject, object> _notifier);

		void DisableGameObject(IGameObject _obj);

		void EnableGameObject(IGameObject _obj);

		void DeleteGameObject(IGameObject _obj);

		void DeleteAllGameObjects();
	}
}
