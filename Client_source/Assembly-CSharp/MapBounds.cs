using UnityEngine;

public class MapBounds : MonoBehaviour
{
	public Rect mBounds;

	public void CorrectPosition(ref Vector3 _pos)
	{
		if (_pos.x < mBounds.x)
		{
			_pos.x = mBounds.x;
		}
		else
		{
			float num = mBounds.x + mBounds.width;
			if (_pos.x > num)
			{
				_pos.x = num;
			}
		}
		if (_pos.z < 0f - mBounds.y)
		{
			_pos.z = 0f - mBounds.y;
			return;
		}
		float num2 = mBounds.y + mBounds.height;
		if (_pos.z > num2)
		{
			_pos.z = num2;
		}
	}
}
