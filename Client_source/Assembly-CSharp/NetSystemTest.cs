using System;
using Log4Tanat;
using Network;
using TanatKernel;
using UnityEngine;

public class NetSystemTest : MonoBehaviour
{
	public double mLogFlushInterval = 5.0;

	private DateTime mLastLogFlushTime;

	private Config mConfig;

	private NetSystem mNetSys;

	private Core mCore;

	private LoginPerformer mLoginPerformer;

	public void Start()
	{
		mConfig = new Config();
		mConfig.ApplyThreadSettings();
		mConfig.Load(ReadText("configs/config"));
		mConfig.InitLog();
		Log.Debug("BEGIN");
		Log.Debug("platform: " + Application.platform);
		mNetSys = new NetSystem();
		UserNetData userNetData = new UserNetData("bot0@ittnord.ru", "bot");
		userNetData.ClientVersion = "0.22b";
		mCore = new Core(mNetSys, mConfig, userNetData);
		mCore.SetGameObjectManager(new TestGameObjManager());
		mCore.CtrlServer.EnableAutoConnect(mCore.BattleServer);
		Debug.Log("start login");
		mLoginPerformer = new LoginPerformer(mCore);
		Notifier<LoginPerformer, object> notifier = new Notifier<LoginPerformer, object>(OnLoginPerformed, null);
		mLoginPerformer.Notifiers.Add(notifier);
		mLoginPerformer.Start();
	}

	public void OnMouseDown()
	{
	}

	private void OnLoginPerformed(bool _success, LoginPerformer _loginPerformer, object _data)
	{
		Debug.Log("login status " + _success);
		mLoginPerformer = null;
	}

	public void OnApplicationQuit()
	{
		if (mNetSys != null)
		{
			mNetSys.DisconnectAll();
		}
		Log.Debug("END");
		Log.DisableAll();
	}

	public void Update()
	{
		//Discarded unreachable code: IL_003f
		try
		{
			if (mLoginPerformer != null)
			{
				mLoginPerformer.UpdateInProgressCore();
			}
			else if (mCore != null)
			{
				mCore.Update();
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
		if ((DateTime.Now - mLastLogFlushTime).TotalSeconds > mLogFlushInterval)
		{
			Log.Flush();
			mLastLogFlushTime = DateTime.Now;
		}
	}

	public static string ReadText(string _txtResourceName)
	{
		if (string.IsNullOrEmpty(_txtResourceName))
		{
			throw new ArgumentNullException("_txtResourceName");
		}
		TextAsset textAsset = Resources.Load(_txtResourceName, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			throw new ArgumentException("text resource " + _txtResourceName + " does not exist");
		}
		return textAsset.text;
	}
}
