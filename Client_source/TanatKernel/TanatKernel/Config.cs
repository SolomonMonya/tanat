using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Log4Tanat;

namespace TanatKernel
{
	public class Config : IThreadInfoProvider, ILocaleStateProvider
	{
		private string mCtrlSrvHost = "localhost";

		private int mCtrlSrvPort = 80;

		private int mCtrlPingTime = 60;

		private int mCtrlDisconnectTime = 15;

		private string mSessionPath;

		private string mItemProtoUrl;

		private string mQuestsUrl;

		private string mTasksUrl;

		private string mAvatarsUrl;

		private string mDataDir;

		private string mSrvDataDir;

		private string mSavesDir;

		private List<KeyValuePair<string, Dictionary<string, string>>> mLogConfigs;

		private string[] mAvatars = new string[0];

		private string mLocalePath;

		private string mLang;

		private LocaleState mLocale;

		private string mAutoupdateAddr;

		private string[] mAutoupdateIgnores = new string[0];

		private float mAfkKickTime;

		private float mAfkWarnTime;

		private float mAiUpdateRate;

		private float mAiViewRadius;

		private int mMicroReConnectAttemptTime;

		private int mMicroReConnectAttemptCount;

		private int mWebReConnectAttemptCount;

		private bool mWinCursors;

		private bool mSingleAppInstance;

		public string ControlServerHost => mCtrlSrvHost;

		public int ControlServerPort => mCtrlSrvPort;

		public int ControlServerPingTime => mCtrlPingTime;

		public int ControlServerDisconnectTime => mCtrlDisconnectTime;

		public string SessionPath => mSessionPath;

		public string ItemProtoUrl => mItemProtoUrl;

		public string QuestsUrl => mQuestsUrl;

		public string TasksUrl => mTasksUrl;

		public string AvatarsUrl => mAvatarsUrl;

		public string DataDir => mDataDir;

		public string SrvDataDir => mSrvDataDir;

		public string SavesDir => mSavesDir;

		public string[] Avatars => mAvatars;

		public string Lang => mLang;

		public LocaleState LocaleState => mLocale;

		public string LocalePath => mLocalePath;

		public string AutoupdateAddr => mAutoupdateAddr;

		public string[] AutoupdateIgnores => mAutoupdateIgnores;

		public float AfkKickTime => mAfkKickTime;

		public float AfkWarnTime => mAfkWarnTime;

		public float AiUpdateRate => mAiUpdateRate;

		public float AiViewRadius => mAiViewRadius;

		public int MicroReConnectAttemptTime => mMicroReConnectAttemptTime;

		public int MicroReConnectAttemptCount => mMicroReConnectAttemptCount;

		public int WebReConnectAttemptCount => mWebReConnectAttemptCount;

		public bool WinCursors => mWinCursors;

		public bool SingleAppInstance => mSingleAppInstance;

		public void ApplyThreadSettings()
		{
			CultureInfo cultureInfo = new CultureInfo("ru-RU");
			cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = cultureInfo;
			Thread.CurrentThread.CurrentUICulture = cultureInfo;
		}

		public string GetThreadInfo()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Thread.CurrentThread.ManagedThreadId);
			string name = Thread.CurrentThread.Name;
			if (!string.IsNullOrEmpty(name))
			{
				stringBuilder.Append(":");
				stringBuilder.Append(name);
			}
			return stringBuilder.ToString();
		}

		public void InitLog()
		{
			if (mLogConfigs == null)
			{
				return;
			}
			foreach (KeyValuePair<string, Dictionary<string, string>> mLogConfig in mLogConfigs)
			{
				LogOutput logOutput = Log.CreateOutput(mLogConfig.Key);
				if (logOutput != null)
				{
					logOutput.Init(mLogConfig.Value);
					if (logOutput.WriteThreadInfo)
					{
						logOutput.SetThreadInfoProvider(this);
					}
					Log.Enable(logOutput);
				}
			}
		}

		public void CreateLocaleState()
		{
			mLocale = new LocaleState(mLang);
		}

		public void LoadFromFile(string _fn)
		{
			if (string.IsNullOrEmpty(_fn))
			{
				new ArgumentNullException("_fn");
			}
			string xml = File.ReadAllText(_fn);
			Load(xml);
		}

		public void Load(string _xml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(_xml);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("tanat_config");
			if (xmlNode == null)
			{
				throw new XmlException("cannot find root element");
			}
			Load(xmlNode);
		}

		protected virtual void Load(XmlNode _root)
		{
			XmlNode xmlNode = null;
			XmlNodeList xmlNodeList = _root.SelectNodes("control_server");
			if (xmlNodeList.Count > 0)
			{
				string text = XmlUtil.SafeReadText("value", _root.SelectSingleNode("use_control_server"));
				foreach (XmlNode item4 in xmlNodeList)
				{
					string text2 = XmlUtil.SafeReadText("name", item4);
					if (text2 == text)
					{
						xmlNode = item4;
						break;
					}
				}
				if (xmlNode == null)
				{
					xmlNode = xmlNodeList[0];
				}
			}
			if (xmlNode != null)
			{
				mCtrlSrvHost = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("host"));
				mCtrlSrvPort = XmlUtil.SafeReadInt("value", xmlNode.SelectSingleNode("port"));
				mCtrlDisconnectTime = XmlUtil.SafeReadInt("value", xmlNode.SelectSingleNode("disconnect_time"));
				mCtrlPingTime = XmlUtil.SafeReadInt("value", xmlNode.SelectSingleNode("ping_time"));
				mItemProtoUrl = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("item_proto"));
				mQuestsUrl = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("quests"));
				mTasksUrl = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("tasks"));
				mAvatarsUrl = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("avatars"));
				mSessionPath = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("session"));
			}
			mDataDir = XmlUtil.SafeReadText("value", _root.SelectSingleNode("data_dir"));
			mSrvDataDir = XmlUtil.SafeReadText("value", _root.SelectSingleNode("srvdata_dir"));
			mSavesDir = XmlUtil.SafeReadText("value", _root.SelectSingleNode("saves_dir"));
			mLocalePath = XmlUtil.SafeReadText("value", _root.SelectSingleNode("locale_path"));
			mLang = XmlUtil.SafeReadText("value", _root.SelectSingleNode("lang"));
			XmlNode xmlNode3 = _root.SelectSingleNode("logs");
			if (xmlNode3 != null)
			{
				mLogConfigs = new List<KeyValuePair<string, Dictionary<string, string>>>();
				for (XmlNode xmlNode4 = xmlNode3.FirstChild; xmlNode4 != null; xmlNode4 = xmlNode4.NextSibling)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					for (XmlNode xmlNode5 = xmlNode4.FirstChild; xmlNode5 != null; xmlNode5 = xmlNode5.NextSibling)
					{
						if (xmlNode5.NodeType != XmlNodeType.Comment)
						{
							dictionary[xmlNode5.Name] = XmlUtil.SafeReadText("value", xmlNode5);
						}
					}
					KeyValuePair<string, Dictionary<string, string>> item = new KeyValuePair<string, Dictionary<string, string>>(xmlNode4.Name, dictionary);
					mLogConfigs.Add(item);
				}
			}
			XmlNode xmlNode6 = _root.SelectSingleNode("avatars");
			if (xmlNode6 != null)
			{
				Queue<string> queue = new Queue<string>();
				for (XmlNode xmlNode7 = xmlNode6.FirstChild; xmlNode7 != null; xmlNode7 = xmlNode7.NextSibling)
				{
					string item2 = XmlUtil.SafeReadText("value", xmlNode7);
					queue.Enqueue(item2);
				}
				mAvatars = queue.ToArray();
			}
			XmlNode xmlNode8 = _root.SelectSingleNode("autoupdate");
			if (xmlNode8 != null)
			{
				mAutoupdateAddr = XmlUtil.SafeReadText("value", xmlNode8.SelectSingleNode("address"));
				XmlNode xmlNode9 = xmlNode8.SelectSingleNode("ignores");
				if (xmlNode9 != null)
				{
					List<string> list = new List<string>();
					XmlNodeList xmlNodeList2 = xmlNode9.SelectNodes("file");
					foreach (XmlNode item5 in xmlNodeList2)
					{
						string item3 = XmlUtil.SafeReadText("value", item5);
						list.Add(item3);
					}
					mAutoupdateIgnores = list.ToArray();
				}
			}
			XmlNode xmlNode10 = _root.SelectSingleNode("afk");
			if (xmlNode10 != null)
			{
				mAfkKickTime = XmlUtil.SafeReadFloat("value", xmlNode10.SelectSingleNode("kick_time"));
				mAfkWarnTime = XmlUtil.SafeReadFloat("value", xmlNode10.SelectSingleNode("warning_time"));
			}
			XmlNode xmlNode11 = _root.SelectSingleNode("ai");
			if (xmlNode11 != null)
			{
				mAiUpdateRate = XmlUtil.SafeReadFloat("value", xmlNode11.SelectSingleNode("update_rate"));
				mAiViewRadius = XmlUtil.SafeReadFloat("value", xmlNode11.SelectSingleNode("view_radius"));
			}
			XmlNode xmlNode12 = _root.SelectSingleNode("microReconnect");
			if (xmlNode12 != null)
			{
				mMicroReConnectAttemptTime = XmlUtil.SafeReadInt("value", xmlNode12.SelectSingleNode("reconnect_time"));
				mMicroReConnectAttemptCount = XmlUtil.SafeReadInt("value", xmlNode12.SelectSingleNode("reconnect_attempts"));
				mWebReConnectAttemptCount = XmlUtil.SafeReadInt("value", xmlNode12.SelectSingleNode("web_reconnect_attempts"));
			}
			mWinCursors = XmlUtil.SafeReadBool("value", _root.SelectSingleNode("win_cursors"));
			mSingleAppInstance = XmlUtil.SafeReadBool("value", _root.SelectSingleNode("single_app_instance"));
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (Array.IndexOf(commandLineArgs, "-disable_cursors") != -1)
			{
				mWinCursors = false;
			}
		}
	}
}
