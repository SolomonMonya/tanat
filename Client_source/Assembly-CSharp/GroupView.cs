using System;
using TanatKernel;

public class GroupView : IGuiView
{
	private YesNoDialog mYesNoDialogWnd;

	private OkDialog mOkDialogWnd;

	public GroupView(YesNoDialog _yesNoDialog, OkDialog _okDialog)
	{
		if (_yesNoDialog == null)
		{
			throw new ArgumentNullException("_yesNoDialog");
		}
		if (_okDialog == null)
		{
			throw new ArgumentNullException("_okDialog");
		}
		mYesNoDialogWnd = _yesNoDialog;
		mOkDialogWnd = _okDialog;
	}

	public void Ask(Type _listenerType, int _textId, object _textData, Notifier<IGuiView, object> _notifier)
	{
		if (_listenerType == typeof(Group))
		{
			string text = "EMPTY!";
			switch (_textId)
			{
			case 0:
			{
				text = GuiSystem.GetLocaleText("GUI_GROUP_JOIN_REQUEST");
				string newValue2 = (string)_textData;
				text = text.Replace("{NAME}", newValue2);
				break;
			}
			case 1:
			{
				text = GuiSystem.GetLocaleText("GUI_GROUP_ADD_REQUEST");
				string newValue = (string)_textData;
				text = text.Replace("{NAME}", newValue);
				break;
			}
			}
			YesNoDialog.OnAnswer callback = delegate(bool _answer)
			{
				_notifier.Call(_answer, this);
			};
			mYesNoDialogWnd.SetData(text, "YES_TEXT", "NO_TEXT", callback);
		}
	}

	public void Inform(Type _listenerType, int _textId, object _textData)
	{
		if (_listenerType == typeof(Group))
		{
			string text = "EMPTY!";
			text = GuiSystem.GetLocaleText("GUI_" + (Group.Text)_textId);
			mOkDialogWnd.SetData(text);
		}
	}

	public void Skip(Type _listenerType)
	{
		if (_listenerType == typeof(Group))
		{
		}
	}
}
