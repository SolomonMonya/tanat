using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public interface IIncomingChatMsg
	{
		ChatChannel Channel
		{
			get;
		}

		string Text
		{
			get;
		}

		DateTime Time
		{
			get;
		}

		string Sender
		{
			get;
		}

		IEnumerable<string> Recipients
		{
			get;
		}
	}
}
