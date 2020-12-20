using System.IO;

namespace TanatKernel
{
	public interface TransferOperationCreator
	{
		TransferOperation CreateTransferOperation(string _uri, Stream _receiver);

		TransferOperationGroup CreateTransferOperationGroup();
	}
}
