using System;
using System.Collections.Generic;
using TanatKernel;

public class ClanView : IGuiView
{
	private YesNoDialog mYesNoDialogWnd;

	private OkDialog mOkDialogWnd;

	public ClanView(YesNoDialog _yesNoDialog, OkDialog _okDialog)
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
		if (_listenerType == typeof(Clan))
		{
			string text = GetText((Clan.Text)_textId, _textData);
			YesNoDialog.OnAnswer callback = delegate(bool _answer)
			{
				_notifier.Call(_answer, this);
			};
			mYesNoDialogWnd.SetData(text, "YES_TEXT", "NO_TEXT", callback);
		}
	}

	public void Inform(Type _listenerType, int _textId, object _textData)
	{
		if (_listenerType == typeof(Clan))
		{
			mOkDialogWnd.SetData(GetText((Clan.Text)_textId, _textData));
		}
	}

	private string GetText(Clan.Text _text, object _textData)
	{
		string text = "EMPTY!";
		text = GuiSystem.GetLocaleText("GUI_" + _text);
		if (_textData != null)
		{
			Dictionary<string, string> dictionary = _textData as Dictionary<string, string>;
			if (dictionary != null)
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					text = text.Replace(item.Key, item.Value);
				}
				return text;
			}
		}
		return text;
	}

	public void Skip(Type _listenerType)
	{
		if (_listenerType == typeof(Clan))
		{
		}
	}
}
