namespace TanatKernel
{
	public interface IReconnectChecker
	{
		void AskForReconnect(float _timer, Notifier<IReconnectChecker, object> _notifier);
	}
}
