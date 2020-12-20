using System.Collections.Generic;

namespace TanatKernel
{
	public interface IHeroViewManager
	{
		void SetHeroView(IGameObject _go, HeroView _view, Notifier<IHeroViewManager, object> _notifier);

		void SetHeroViewItem(IGameObject _go, CtrlThing _item);

		void SetHeroViewItems(IGameObject _go, IEnumerable<CtrlThing> _items);

		void RemoveHeroViewItem(IGameObject _go, CtrlThing _item);
	}
}
