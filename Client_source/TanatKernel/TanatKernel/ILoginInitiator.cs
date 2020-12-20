namespace TanatKernel
{
	public interface ILoginInitiator
	{
		void StartLogin(string _email, string _password, Notifier<LoginPerformer, object> _notifier);
	}
}
