using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

namespace Log4Tanat
{
	public class LogOutputHtml : LogOutputFile
	{
		public string mTemplatePath;

		public void Init(string _outputPath, string _templatePath)
		{
			Init(_outputPath);
			mTemplatePath = _templatePath;
		}

		public override void Init(IDictionary<string, string> _params)
		{
			base.Init(_params);
			_params.TryGetValue("template", out mTemplatePath);
		}

		public override void Enable()
		{
			if (string.IsNullOrEmpty(mTemplatePath))
			{
				throw new InvalidOperationException("invalid html template file path");
			}
			base.Enable();
			if (mWriter != null)
			{
				try
				{
					string value = File.ReadAllText(mTemplatePath);
					mWriter.Write(value);
				}
				catch (IOException)
				{
				}
				catch (SecurityException)
				{
				}
			}
		}

		public override void Write(Log.Category _category, string _caller, string _time, string _data)
		{
			if (mWriter != null)
			{
				_data = EscapeTxt(_data);
				try
				{
					mWriter.Write("<script type=\"text/javascript\">log(\"");
					mWriter.Write(_category);
					mWriter.Write("\", \"");
					mWriter.Write(_time);
					mWriter.Write("\", \"");
					mWriter.Write(_caller);
					mWriter.Write("\", \"");
					mWriter.Write(_data);
					mWriter.WriteLine("\")</script>");
				}
				catch (IOException)
				{
					Disable();
				}
				catch (ObjectDisposedException)
				{
					Disable();
				}
			}
		}

		private string EscapeTxt(string _txt)
		{
			if (string.IsNullOrEmpty(_txt))
			{
				return _txt;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in _txt)
			{
				if (char.IsLetterOrDigit(c) || c == ' ')
				{
					stringBuilder.Append(c);
					continue;
				}
				stringBuilder.Append("\\u");
				stringBuilder.AppendFormat("{0:x4}", (int)c);
			}
			return stringBuilder.ToString();
		}
	}
}
