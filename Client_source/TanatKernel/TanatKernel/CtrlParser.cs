using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	internal class CtrlParser
	{
		public virtual bool Begin(Stream _respContent)
		{
			if (_respContent.Length == 0)
			{
				return false;
			}
			_respContent.Position = 0L;
			return true;
		}

		public virtual void Parse(Stream _respContent, ICtrlResponseHolder _responses, Formatter _formatter)
		{
			while (true)
			{
				_formatter.ClearRefTables();
				Variable variable = _formatter.Deserialize(_respContent);
				if (variable == null)
				{
					MemoryStream memoryStream = _respContent as MemoryStream;
					if (memoryStream != null)
					{
						memoryStream.Position = 0L;
						byte[] bytes = memoryStream.ToArray();
						string @string = Encoding.ASCII.GetString(bytes);
						Log.Warning("unformatted message: " + @string);
					}
					break;
				}
				if (variable.ValueType != typeof(MixedArray))
				{
					Log.Warning("invalid root AMF value: " + variable.ToString());
					continue;
				}
				Log.Info("response: " + variable.ToString());
				AddToResponses(_responses, variable);
				if (_respContent.Position == _respContent.Length)
				{
					break;
				}
				if (PrepareNext(_respContent))
				{
					continue;
				}
				Log.Warning("invalid rest part");
				break;
			}
			_formatter.ClearRefTables();
		}

		public virtual void End(bool _success, Stream _respContent)
		{
			_respContent.Close();
		}

		protected virtual void AddToResponses(ICtrlResponseHolder _responses, MixedArray _args)
		{
			foreach (KeyValuePair<string, Variable> item in _args.Associative)
			{
				Enum @enum = ParseCmd(item.Key);
				if (@enum == null)
				{
					Log.Warning("unsupported command: " + item.Key + " " + item.Value.ToString());
					continue;
				}
				CtrlPacket packet = new CtrlPacket(@enum, item.Value);
				_responses.AddResponse(packet);
			}
		}

		protected virtual Enum ParseCmd(string _str)
		{
			return CtrlCmdId.Parse(_str);
		}

		protected virtual bool PrepareNext(Stream _respContent)
		{
			return true;
		}
	}
}
