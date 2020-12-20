using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

namespace Log4Tanat
{
	public class LogOutputFile : LogOutput, IDisposable
	{
		private delegate string ProcInfo();

		private string mOutputPath;

		protected StreamWriter mWriter;

		private string[] mOutputNames;

		private int mStreamsCount = 2;

		private int mStreamSize = 524288;

		private int mCurrentStream;

		private int mCurrentSize;

		public void Dispose()
		{
			if (mWriter != null)
			{
				mWriter.Close();
			}
		}

		public void Init(string _outputPath)
		{
			if (string.IsNullOrEmpty(_outputPath))
			{
				throw new ArgumentNullException();
			}
			if (mWriter != null)
			{
				throw new InvalidOperationException("writer already created");
			}
			mOutputPath = _outputPath;
		}

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
			if (_params.TryGetValue("auto_flush", out value) && bool.TryParse(value, out var result2))
			{
				base.AutoFlush = result2;
			}
			if (_params.TryGetValue("thread_info", out value) && bool.TryParse(value, out var result3))
			{
				base.WriteThreadInfo = result3;
			}
			if (_params.TryGetValue("files_count", out value) && int.TryParse(value, out var result4))
			{
				mStreamsCount = result4;
			}
			if (_params.TryGetValue("file_size", out value) && int.TryParse(value, out var result5))
			{
				mStreamSize = result5 * 1024;
			}
			if (_params.TryGetValue("output", out value))
			{
				Init(value);
			}
		}

		private void Replace(ref string _str, string _pattern, ProcInfo _replacer)
		{
			if (_str.Contains(_pattern))
			{
				string newValue = _replacer();
				_str = _str.Replace(_pattern, newValue);
			}
		}

		private string GetProcName()
		{
			Process currentProcess = Process.GetCurrentProcess();
			return currentProcess.ProcessName;
		}

		private string GetProcId()
		{
			Process currentProcess = Process.GetCurrentProcess();
			return currentProcess.Id.ToString();
		}

		private string GetProcStartTime()
		{
			Process currentProcess = Process.GetCurrentProcess();
			return currentProcess.StartTime.ToString("MM_dd_HH_mm_ss");
		}

		private string GetAppDataFolder()
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			return folderPath + Path.DirectorySeparatorChar;
		}

		public override void Enable()
		{
			if (string.IsNullOrEmpty(mOutputPath))
			{
				throw new InvalidOperationException("invalid output file path");
			}
			if (mWriter != null)
			{
				throw new InvalidOperationException("already enabled");
			}
			try
			{
				Replace(ref mOutputPath, "{PROCESS_NAME}", GetProcName);
				Replace(ref mOutputPath, "{PROCESS_ID}", GetProcId);
				Replace(ref mOutputPath, "{PROCESS_START_TIME}", GetProcStartTime);
				Replace(ref mOutputPath, "{APP_DATA}", GetAppDataFolder);
				string directoryName = Path.GetDirectoryName(mOutputPath);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				InitFiles(mOutputPath);
			}
			catch (IOException)
			{
			}
			catch (SecurityException)
			{
			}
		}

		private void InitFiles(string _outputPath)
		{
			mOutputNames = new string[mStreamsCount];
			for (int i = 0; i < mStreamsCount; i++)
			{
				mOutputNames[i] = Path.GetDirectoryName(_outputPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(_outputPath) + i + Path.GetExtension(_outputPath);
			}
			mCurrentStream = 0;
			mWriter = new StreamWriter(mOutputNames[mCurrentStream], append: false, Encoding.UTF8);
		}

		public override void Disable()
		{
			Dispose();
			mWriter = null;
			FileInfo fileInfo = new FileInfo(mOutputPath);
			if (fileInfo.Exists && fileInfo.Length == 0)
			{
				fileInfo.Delete();
			}
		}

		public override void Flush()
		{
			if (mWriter != null)
			{
				mWriter.Flush();
			}
		}

		private void SwitchStream()
		{
			if (mCurrentSize >= mStreamSize)
			{
				mCurrentStream++;
				if (mCurrentStream >= mStreamsCount)
				{
					mCurrentStream = 0;
				}
				mWriter.Flush();
				mWriter.Close();
				mWriter = new StreamWriter(mOutputNames[mCurrentStream], append: false, Encoding.UTF8);
				mCurrentSize = 0;
			}
		}

		public override void Write(Log.Category _category, string _caller, string _time, string _data)
		{
			if (mWriter == null)
			{
				return;
			}
			try
			{
				SwitchStream();
				StringBuilder stringBuilder = new StringBuilder();
				if (base.WriteThreadInfo && mThreadInfoProv != null)
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
				stringBuilder.AppendLine(_data);
				mCurrentSize += stringBuilder.Length;
				mWriter.Write(stringBuilder.ToString());
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
}
