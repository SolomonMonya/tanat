using System.Collections.Generic;

namespace TanatKernel
{
	public class GameObjectStore : Store<IGameObject>
	{
		private List<int> mRemoved = new List<int>();

		private List<IGameObject> mVisibleObjs = new List<IGameObject>();

		public IEnumerable<IGameObject> VisibleObjects => mVisibleObjs;

		public GameObjectStore()
			: base("GameObjects")
		{
		}

		public IGameObject SetVisibility(int _id, bool _visible, out bool _prevVisible)
		{
			IGameObject gameObject = Get(_id);
			if (gameObject == null)
			{
				_prevVisible = false;
				return null;
			}
			_prevVisible = gameObject.Data.Visible;
			gameObject.Data.Visible = _visible;
			if (_visible)
			{
				mVisibleObjs.Add(gameObject);
			}
			else
			{
				mVisibleObjs.Remove(gameObject);
			}
			return gameObject;
		}

		public override void Remove(int _id)
		{
			base.Remove(_id);
			mRemoved.Add(_id);
		}

		public bool CheckRemoving(int _id)
		{
			return mRemoved.Contains(_id);
		}

		public void CompleteRemove(int _id)
		{
			mRemoved.Remove(_id);
		}
	}
}
