using System.Text;

namespace TanatKernel
{
	public class Frame<VectorT>
	{
		public float mClientTime;

		public float mServerTime;

		public VectorT mPos;

		public VectorT mVel;

		public int mNum = -1;

		public Frame()
		{
		}

		public Frame(float _clientTime, float _servTime, VectorT _pos, VectorT _vel)
		{
			mClientTime = _clientTime;
			mServerTime = _servTime;
			mPos = _pos;
			mVel = _vel;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(mNum.ToString());
			stringBuilder.AppendLine("client time " + mClientTime);
			stringBuilder.AppendLine("server time " + mServerTime);
			stringBuilder.AppendLine("position " + mPos);
			stringBuilder.AppendLine("velocity " + mVel);
			return stringBuilder.ToString();
		}
	}
}
