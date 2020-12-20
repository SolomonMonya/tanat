using System.IO;
using TanatKernel;

public interface ITransferOperationCreator
{
	TransferOperation CreateTransferOperation(string _uri, Stream _receiver);
}
