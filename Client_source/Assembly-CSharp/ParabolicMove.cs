using UnityEngine;

public class ParabolicMove : MonoBehaviour
{
	public static Vector3 GetParabolicPosByTime(Vector3 _startPos, Vector3 _startSpeed, float _gravity, float _curTime)
	{
		Vector3 zero = Vector3.zero;
		zero.x = _startPos.x + _startSpeed.x * _curTime;
		zero.z = _startPos.z + _startSpeed.z * _curTime;
		zero.y = _startPos.y + (_startSpeed.y * _curTime - _gravity * Mathf.Pow(_curTime, 2f) / 2f);
		return zero;
	}

	public static Vector3 GetParabolicPosByLength(Vector3 _startPos, Vector3 _endPos, Vector3 _abc, float _curLength)
	{
		if (_startPos == _endPos)
		{
			return _endPos;
		}
		Vector3 pos = _startPos + (_endPos - _startPos).normalized * _curLength;
		return GetParabolicPosByLength(pos, _abc);
	}

	public static Vector3 GetParabolicPosByLength(Vector3 _pos, Vector3 _abc)
	{
		if (_abc != Vector3.zero)
		{
			float num = (_pos.x + _pos.z) / 2f;
			_pos.y = _abc.x * num * num + _abc.y * num + _abc.z;
		}
		return _pos;
	}

	public static Vector3 GetParabolicABC(Vector3 _p1, Vector3 _p2, Vector3 _p3)
	{
		float num = (_p1.x + _p1.z) / 2f;
		float y = _p1.y;
		float num2 = (_p2.x + _p2.z) / 2f;
		float y2 = _p2.y;
		float num3 = (_p3.x + _p3.z) / 2f;
		float y3 = _p3.y;
		float num4 = num2 - num;
		float num5 = 0.01f;
		if (Mathf.Abs(num4) >= num5)
		{
			float num6 = (y3 - (num3 * (y2 - y) + num2 * y - num * y2) / num4) / (num3 * (num3 - num - num2) + num * num2);
			float y4 = (y2 - y) / num4 - num6 * (num + num2);
			float z = (num2 * y - num * y2) / num4 + num6 * num * num2;
			return new Vector3(num6, y4, z);
		}
		return Vector3.zero;
	}

	public static Vector3 GetParabolicABC(Vector3 _startPos, Vector3 _endPos, float _maxHeight, float _heightDistance)
	{
		Vector3 parabolicUpPos = GetParabolicUpPos(_startPos, _endPos, _maxHeight, _heightDistance);
		return GetParabolicABC(_startPos, parabolicUpPos, _endPos);
	}

	public static Vector3 GetParabolicUpPos(Vector3 _startPos, Vector3 _endPos, float _maxHeight, float _heightDistance)
	{
		Vector3 vector = _endPos - _startPos;
		vector.Normalize();
		float num = Vector3.Distance(_startPos, _endPos);
		float num2 = num / _heightDistance;
		_maxHeight *= num2;
		Vector3 vector2 = Vector3.Cross(Vector3.Cross(vector, Vector3.up), vector);
		return _startPos + vector2 * _maxHeight + vector * (num / 2f);
	}
}
