using UnityEngine;

public class ObjTextInfo : GuiElement
{
	private string mData = string.Empty;

	private Color mDataColor = Color.white;

	private Rect mDrawRect = default(Rect);

	public ObjTextInfo(string _data, Color _color)
	{
		mData = _data;
		mDataColor = _color;
		mDrawRect = new Rect(0f, 0f, 120f, 10f);
	}

	public override void RenderElement()
	{
		GUI.contentColor = mDataColor;
		GuiSystem.DrawString(mData, mDrawRect, "middle_center");
		GUI.contentColor = Color.white;
	}

	public void SetPos(Vector2 _pos)
	{
		mDrawRect.x = _pos.x - mDrawRect.width / 2f;
		mDrawRect.y = (float)Screen.height - (_pos.y - mDrawRect.height / 2f);
	}
}
