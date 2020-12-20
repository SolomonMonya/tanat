namespace TanatKernel
{
	public abstract class BaseTransferOperation<NotifOwnerT>
	{
		public Notifier<NotifOwnerT, object>.Group mNotifiers = new Notifier<NotifOwnerT, object>.Group();

		protected long mTransferedSize;

		protected long mLastChunkSize;

		public long TransferedSize => mTransferedSize;

		public long LastChunkSize => mLastChunkSize;

		public abstract void Begin();

		public abstract void End();
	}
}
