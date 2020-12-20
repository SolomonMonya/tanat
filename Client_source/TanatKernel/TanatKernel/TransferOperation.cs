using System;
using System.IO;

namespace TanatKernel
{
	public abstract class TransferOperation : BaseTransferOperation<TransferOperation>
	{
		protected string mUri;

		protected Stream mReceiver;

		public string URI => mUri;

		public Stream Receiver => mReceiver;

		public TransferOperation(string _uri, Stream _receiver)
		{
			if (string.IsNullOrEmpty(_uri) || _receiver == null)
			{
				throw new ArgumentNullException();
			}
			mUri = _uri;
			mReceiver = _receiver;
		}

		public TransferOperation(string _uri)
			: this(_uri, new MemoryStream())
		{
		}
	}
}
