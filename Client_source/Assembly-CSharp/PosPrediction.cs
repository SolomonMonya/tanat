using TanatKernel;
using UnityEngine;

public class PosPrediction
{
	public class ServerMirror : BasePosPredictor<Vector2>
	{
		protected Frame<Vector2> mLast = new Frame<Vector2>();

		private int mLastFrameNum;

		public Vector2 GetPosition(float _time)
		{
			float num = _time - mLast.mServerTime;
			return mLast.mPos + mLast.mVel * num;
		}

		public Vector2 GetVelocity(float _time)
		{
			return mLast.mVel;
		}

		public virtual void Adjust(float _clientTime, float _servTime, Vector2 _pos, Vector2 _vel)
		{
			mLast.mClientTime = _clientTime;
			mLast.mServerTime = _servTime;
			mLast.mPos = _pos;
			mLast.mVel = _vel;
			mLast.mNum = mLastFrameNum++;
		}

		public virtual float CorrectPosition(float _time, ref Vector2 _pos)
		{
			Vector2 position = GetPosition(_time);
			float magnitude = (position - _pos).magnitude;
			_pos = position;
			return magnitude;
		}

		public void Reset()
		{
			mLast.mPos = GetPosition(mLast.mClientTime);
			mLast.mServerTime = mLast.mClientTime;
		}
	}

	public class SmoothErrorCorrector : ServerMirror
	{
		private float mMaxSpeed;

		public override void Adjust(float _clientTime, float _servTime, Vector2 _pos, Vector2 _vel)
		{
			base.Adjust(_clientTime, _servTime, _pos, _vel);
			float magnitude = mLast.mVel.magnitude;
			if (magnitude > mMaxSpeed)
			{
				mMaxSpeed = magnitude;
			}
		}

		public override float CorrectPosition(float _time, ref Vector2 _pos)
		{
			float num = _time - mLast.mServerTime;
			if (num < 0f && 0f - num > Time.deltaTime)
			{
				return 0f;
			}
			Vector2 vector = mLast.mPos + mLast.mVel * num;
			Vector2 vector2 = vector - _pos;
			float num2 = vector2.magnitude;
			float num3 = mMaxSpeed * Time.deltaTime;
			if (num2 > num3)
			{
				vector2 /= num2;
				num2 = num3;
				vector2 *= num2;
				_pos += vector2;
			}
			else
			{
				_pos = vector;
			}
			if (Mathf.Abs(num2) < 0.001f)
			{
				mMaxSpeed = 0f;
			}
			return num2;
		}
	}
}
