namespace TanatKernel
{
	public interface BasePosPredictor<VectorT>
	{
		VectorT GetPosition(float _time);

		VectorT GetVelocity(float _time);

		void Adjust(float _clientTime, float _servTime, VectorT _pos, VectorT _vel);

		float CorrectPosition(float _time, ref VectorT _pos);

		void Reset();
	}
}
