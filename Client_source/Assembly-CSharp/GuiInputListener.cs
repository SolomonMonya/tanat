using UnityEngine;

public class GuiInputListener : GuiElement
{
	protected GuiInputMgr.ButtonDragFlags mDragType;

	public virtual bool AddDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt, int _num)
	{
		return false;
	}

	public virtual void RemoveDraggableButton(GuiInputMgr.DraggableButton _btn, int _cnt)
	{
	}

	public virtual int GetZoneNum(Vector2 _mousePos)
	{
		return -1;
	}

	public GuiInputMgr.ButtonDragFlags GetListenerType()
	{
		return mDragType;
	}
}
