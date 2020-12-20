namespace UnityEngine
{
	public struct NetworkMessageInfo
	{
		private double m_TimeStamp;

		private NetworkPlayer m_Sender;

		private NetworkViewID m_ViewID;

		public double timestamp => m_TimeStamp;

		public NetworkPlayer sender => m_Sender;

		public NetworkView networkView => NetworkView.Find(m_ViewID);
	}
}
