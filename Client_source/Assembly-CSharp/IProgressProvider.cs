public interface IProgressProvider
{
	float GetProgress();

	void BeginProgress();

	void EndProgress();
}
