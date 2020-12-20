using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Tanat
{
	public class LogOutputConsole : LogOutput
	{
		public override void Init(IDictionary<string, string> _params)
		{
			if (_params == null)
			{
				throw new ArgumentNullException();
			}
			if (_params.TryGetValue("level", out var value) && int.TryParse(value, out var result))
			{
				SetMinLevel(result);
			}
			if (_params.TryGetValue("thread_info", out value) && bool.TryParse(value, out var result2))
			{
				base.WriteThreadInfo = result2;
			}
		}

		public override void Write(Log.Category _category, string _caller, string _time, string _data)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (base.WriteThreadInfo)
			{
				string threadInfo = mThreadInfoProv.GetThreadInfo();
				stringBuilder.Append(threadInfo);
				stringBuilder.Append(" | ");
			}
			stringBuilder.Append(_time);
			stringBuilder.Append(" | ");
			stringBuilder.Append(_category);
			stringBuilder.Append(" | ");
			stringBuilder.Append(_caller);
			stringBuilder.Append(" | ");
			stringBuilder.Append(_data);
			Console.WriteLine(stringBuilder);
		}
	}
}
