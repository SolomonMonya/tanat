using System;
using UnityEngine;

public class ExportObjectData : MonoBehaviour
{
	public const string mSep = "--";

	public int mId = -1;

	public void SetId(int _id)
	{
		mId = _id;
		string text = base.gameObject.name;
		int num = text.IndexOf("--");
		if (num != -1)
		{
			text = text.Remove(num);
		}
		if (mId >= 0)
		{
			text = text + "--" + mId;
		}
		base.gameObject.name = text;
	}

	public void Restore()
	{
		int id = -1;
		string name = base.gameObject.name;
		int num = name.LastIndexOf("--");
		if (num != -1)
		{
			string s = name.Substring(num + "--".Length, name.Length - num - "--".Length);
			try
			{
				id = int.Parse(s);
			}
			catch (FormatException)
			{
			}
		}
		else
		{
			id = mId;
		}
		SetId(id);
	}

	public bool IsValid()
	{
		return mId > 0;
	}
}
