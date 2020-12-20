using UnityEngine;

public class StaticTextField : GuiElement
{
	public string mData = string.Empty;

	public int mLength;

	public bool mPassword;

	public string mStyleId = string.Empty;

	public bool mAutoFocus;

	private GUIStyle mStyle;

	public override void Uninit()
	{
		mStyle = null;
	}

	public override void OnInput()
	{
		if (mStyleId.Length > 0 && GUI.skin.textField.name != mStyleId)
		{
			GUIStyle style = GUI.skin.GetStyle(mStyleId);
			mStyle = new GUIStyle(style);
			GUI.skin.textField = mStyle;
		}
		GUI.SetNextControlName(mElementId);
		GUI.enabled = !mLocked;
		if (!mPassword)
		{
			mData = GUI.TextField(mZoneRect, mData, mLength);
		}
		else
		{
			mData = GUI.PasswordField(mZoneRect, mData, '*', mLength);
		}
		GUI.enabled = true;
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mLocked)
		{
			return;
		}
		if (_curEvent.type == EventType.KeyUp && mAutoFocus && _curEvent.keyCode == KeyCode.Return)
		{
			if (GUI.GetNameOfFocusedControl() != mElementId)
			{
				GUI.FocusControl(mElementId);
			}
			else if (GUI.GetNameOfFocusedControl() == mElementId)
			{
				GuiSystem.UpdateFocus();
			}
		}
		if (mZoneRect.Contains(_curEvent.mousePosition))
		{
			if (_curEvent.type == EventType.MouseDown && GUI.GetNameOfFocusedControl() != mElementId)
			{
				GUI.FocusControl(mElementId);
			}
		}
		else if (_curEvent.type == EventType.MouseDown && GUI.GetNameOfFocusedControl() == mElementId)
		{
			GuiSystem.UpdateFocus();
		}
	}
}
