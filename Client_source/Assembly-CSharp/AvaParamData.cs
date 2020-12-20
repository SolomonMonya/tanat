using UnityEngine;

public class AvaParamData
{
	public string mValue;

	public string mFormat;

	public Color mColor;

	public string mAdd;

	public string mAddFormat;

	public Color mAddColor;

	public Rect mDrawRect;

	public AvaParamData()
	{
		mColor = Color.white;
		mAddColor = Color.white;
		mValue = string.Empty;
		mAdd = string.Empty;
		mFormat = "middle_left";
		mAddFormat = "middle_right";
	}

	public void Clear()
	{
		mValue = string.Empty;
		mAdd = string.Empty;
	}
}
