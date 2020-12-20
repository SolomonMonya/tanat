using System;

namespace TanatKernel
{
	public interface IGuiView
	{
		void Ask(Type _listenerType, int _textId, object _textData, Notifier<IGuiView, object> _notifier);

		void Inform(Type _listenerType, int _textId, object _textData);

		void Skip(Type _listenerType);
	}
}
