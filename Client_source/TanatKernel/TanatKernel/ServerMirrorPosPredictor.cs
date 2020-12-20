using System;

namespace TanatKernel
{
	public class ServerMirrorPosPredictor : BasePosPredictor<Vec2>
	{
		protected Frame<Vec2> mLast = new Frame<Vec2>();

		private int mLastFrameNum;

		public Vec2 GetPosition(float _time)
		{
			float num = _time - mLast.mServerTime;
			Vec2 result = default(Vec2);
			result.mX = mLast.mPos.mX + mLast.mVel.mX * num;
			result.mY = mLast.mPos.mY + mLast.mVel.mY * num;
			return result;
		}

		public Vec2 GetVelocity(float _time)
		{
			return mLast.mVel;
		}

		public virtual void Adjust(float _clientTime, float _servTime, Vec2 _pos, Vec2 _vel)
		{
			mLast.mClientTime = _clientTime;
			mLast.mServerTime = _servTime;
			mLast.mPos = _pos;
			mLast.mVel = _vel;
			mLast.mNum = mLastFrameNum++;
		}

		public virtual float CorrectPosition(float _time, ref Vec2 _pos)
		{
			Vec2 position = GetPosition(_time);
			Vec2 vec = default(Vec2);
			vec.mX = position.mX - _pos.mX;
			vec.mY = position.mY - _pos.mY;
			float result = (float)Math.Sqrt(vec.mX * vec.mX + vec.mY * vec.mY);
			_pos = position;
			return result;
		}

		public void Reset()
		{
			mLast.mPos = GetPosition(mLast.mClientTime);
			mLast.mServerTime = mLast.mClientTime;
		}
	}
}
