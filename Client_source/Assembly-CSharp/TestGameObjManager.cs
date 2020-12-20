using TanatKernel;

public class TestGameObjManager : IGameObjectManager
{
	public IGameObject CreateGameObject(int _objectId, BattlePrototype _proto)
	{
		InstanceData data = new InstanceData(_objectId);
		return new TestGameObj(_proto, data);
	}

	public void CreateGameObjectAsync(int _objectId, BattlePrototype _proto, Notifier<IGameObject, object> _notifier)
	{
		IGameObject gameObject = CreateGameObject(_objectId, _proto);
		_notifier?.Call(gameObject != null, gameObject);
	}

	public void DisableGameObject(IGameObject _obj)
	{
	}

	public void EnableGameObject(IGameObject _obj)
	{
	}

	public void DeleteGameObject(IGameObject _obj)
	{
	}

	public void DeleteAllGameObjects()
	{
	}
}
