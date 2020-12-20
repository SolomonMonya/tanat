using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Log4Tanat
{
	public static class Log
	{
		public enum Category
		{
			info,
			debug,
			notice,
			warning,
			significant,
			error
		}

		private static List<LogOutput> mOutputs = new List<LogOutput>();

		public static LogOutput CreateOutput(string _className)
		{
			if (string.IsNullOrEmpty(_className))
			{
				return null;
			}
			Type type = Type.GetType("Log4Tanat." + _className);
			if (type == null)
			{
				return null;
			}
			if (!type.IsSubclassOf(typeof(LogOutput)))
			{
				return null;
			}
			return (LogOutput)Activator.CreateInstance(type);
		}

		public static void Enable(LogOutput _output)
		{
			if (_output != null)
			{
				_output.Enable();
				lock (mOutputs)
				{
					mOutputs.Add(_output);
				}
			}
		}

		public static void Enable<T>(int _minLevel) where T : LogOutput, new()
		{
			LogOutput logOutput = new T();
			logOutput.SetMinLevel(_minLevel);
			Enable(logOutput);
		}

		public static void Enable<T>() where T : LogOutput, new()
		{
			Enable<T>(0);
		}

		public static void Disable<T>()
		{
			List<LogOutput> list = new List<LogOutput>();
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					if (mOutput.GetType() == typeof(T))
					{
						mOutput.Disable();
						list.Add(mOutput);
					}
				}
				foreach (LogOutput item in list)
				{
					mOutputs.Remove(item);
				}
			}
		}

		public static void DisableAll()
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					mOutput.Disable();
				}
				mOutputs.Clear();
			}
		}

		public static T GetOutput<T>() where T : LogOutput
		{
			lock (mOutputs)
			{
				return (T)mOutputs.Find((LogOutput i) => i.GetType() == typeof(T));
			}
		}

		public static void Flush()
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					mOutput.Flush();
				}
			}
		}

		private static void Write(LogOutput _output, Category _category, LogGen _lg, ref object _data)
		{
			if (!_output.CanWrite(_category))
			{
				return;
			}
			string time = DateTime.Now.ToString("HH:mm:ss:fff", CultureInfo.InvariantCulture);
			string caller = "";
			StackFrame stackFrame = new StackFrame();
			StackTrace stackTrace = new StackTrace();
			int i = 0;
			for (int frameCount = stackTrace.FrameCount; i < frameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				if (method.DeclaringType != stackFrame.GetMethod().DeclaringType)
				{
					caller = string.Concat(method.DeclaringType, ".", method.Name);
					break;
				}
			}
			if (_data == null)
			{
				if (_lg != null)
				{
					_data = _lg();
				}
				if (_data == null)
				{
					_data = "null";
				}
			}
			_output.Write(_category, caller, time, _data.ToString());
			if (_output.AutoFlush)
			{
				_output.Flush();
			}
		}

		public static void Exception(Exception _e)
		{
			if (_e != null)
			{
				Error(() => _e.GetType().FullName + " " + _e.Message + "\n" + _e.StackTrace);
			}
		}

		public static string StackTrace()
		{
			StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
			return "call stack:\n" + stackTrace.ToString();
		}

		public static void Info(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.info, _lg, ref _data);
				}
			}
		}

		public static void Info(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.info, null, ref _data);
				}
			}
		}

		public static void Debug(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.debug, _lg, ref _data);
				}
			}
		}

		public static void Debug(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.debug, null, ref _data);
				}
			}
		}

		public static void Notice(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.notice, _lg, ref _data);
				}
			}
		}

		public static void Notice(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.notice, null, ref _data);
				}
			}
		}

		public static void Warning(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.warning, _lg, ref _data);
				}
			}
		}

		public static void Warning(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.warning, null, ref _data);
				}
			}
		}

		public static void Significant(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.significant, _lg, ref _data);
				}
			}
		}

		public static void Significant(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.significant, null, ref _data);
				}
			}
		}

		public static void Error(LogGen _lg)
		{
			object _data = null;
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.error, _lg, ref _data);
					mOutput.Flush();
				}
			}
		}

		public static void Error(object _data)
		{
			lock (mOutputs)
			{
				foreach (LogOutput mOutput in mOutputs)
				{
					Write(mOutput, Category.error, null, ref _data);
					mOutput.Flush();
				}
			}
		}
	}
}
