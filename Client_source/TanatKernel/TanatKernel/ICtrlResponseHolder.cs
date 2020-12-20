namespace TanatKernel
{
	public interface ICtrlResponseHolder
	{
		void AddResponse(CtrlPacket _packet);

		void SetSessionKey(string _key);
	}
}
