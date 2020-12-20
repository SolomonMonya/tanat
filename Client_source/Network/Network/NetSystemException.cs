using System;
using Log4Tanat;

namespace Network
{
	public class NetSystemException : Exception
	{
		public NetSystemException(string _message)
			: base(_message)
		{
			Log.Exception(this);
		}
	}
}
