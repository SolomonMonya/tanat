using System.Collections.Generic;

namespace TanatKernel
{
	public class GameObjectsProvider : StoreContentProvider<IGameObject>
	{
		public IEnumerable<IGameObject> VisibleObjects => ((GameObjectStore)mStore).VisibleObjects;

		public GameObjectsProvider(GameObjectStore _store)
			: base((Store<IGameObject>)_store)
		{
		}

		public bool CheckRemoving(int _id)
		{
			lock (mStore)
			{
				return ((GameObjectStore)mStore).CheckRemoving(_id);
			}
		}

		public void CompleteRemove(int _id)
		{
			lock (mStore)
			{
				((GameObjectStore)mStore).CompleteRemove(_id);
			}
		}
	}
}
