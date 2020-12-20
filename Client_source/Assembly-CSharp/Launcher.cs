using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class Launcher : MonoBehaviour, IDownloaderCreator, ITransferOperationCreator
{
	private class OnUpdateData
	{
		public Updater.CheckVersionResult mResult;

		public string mLocalVersion = string.Empty;

		public string mLastVersion = string.Empty;
	}

	public string mVersion;

	public string mMergeDataFN = "merge_data.xml";

	public string mStatsProgFN = "emgcisc.exe";

	private string mUpdateAddr;

	private List<string> mIgnores;

	private bool mMerge;

	private TanatApp mApp;

	private ScreenManager mScreenMgr;

	private UpdateWindow mUpdWnd;

	private List<Downloader> mCreatedDownloaders;

	private OnUpdateData mOnUpdateData;

	private Updater mUpdater;

	private Mutex mMutex;

	private long mFullSize;

	private long mDownloadedSize;

	private long mBytesReceived;

	private double mBytesReceivedTime;

	private float mAimSpeed;

	private float mRemainingTime;

	private float mTimeCalcTime;

	private float mProgressSpeed;

	private float mProgressLim;

	private readonly string mWaitCloseMutexName = "Global\\TanatOnlineUpdate";

	public void Launch(TanatApp _app, Config _config)
	{
		if (_app == null)
		{
			throw new ArgumentNullException("_app");
		}
		if (_config == null)
		{
			throw new ArgumentNullException("_config");
		}
		mApp = _app;
		mIgnores = new List<string>();
		mCreatedDownloaders = new List<Downloader>();
		mUpdateAddr = _config.AutoupdateAddr;
		mIgnores.AddRange(_config.AutoupdateIgnores);
		mMutex = new Mutex(initiallyOwned: true, mWaitCloseMutexName);
		if (File.Exists(mMergeDataFN))
		{
			Log.Debug("MergeData exist : " + mMergeDataFN);
			mMerge = true;
			OptionsMgr.mEnabled = false;
			return;
		}
		Log.Debug("Creating updater");
		mUpdater = new Updater();
		Updater updater = mUpdater;
		updater.mVersionCallback = (Updater.VersionCallback)Delegate.Combine(updater.mVersionCallback, new Updater.VersionCallback(OnVersionChecked));
		Updater updater2 = mUpdater;
		updater2.mDownloadFailedCallback = (Updater.DownloadFailedCallback)Delegate.Combine(updater2.mDownloadFailedCallback, new Updater.DownloadFailedCallback(OnDownloadFailed));
		mUpdater.CheckVersion(this, mUpdateAddr, mVersion);
		mScreenMgr = GameObjUtil.FindObjectOfType<ScreenManager>();
		mScreenMgr.Init(_config, GameObjUtil.FindObjectOfType<TaskQueue>());
		new GameObject("cameras");
		CameraMgr.CreateCamera("LauncherCamera");
		GuiSystem.mGuiSystem.SetCurGuiSet("loading_screen");
		mUpdWnd = GuiSystem.mGuiSystem.GetGuiElement<UpdateWindow>("update_screen", "UPDATE_WINDOW");
		mUpdWnd.CheckVersion(_checking: true);
	}

	public void InitApplication()
	{
		Log.Debug("InitApplication");
		if (mMutex != null)
		{
			mMutex.ReleaseMutex();
			mMutex = null;
		}
		mUpdater = null;
		mOnUpdateData = null;
		mApp.Init();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnVersionChecked(Updater.CheckVersionResult _result, string _localVersionId, string _lastVersionId)
	{
		if (mOnUpdateData != null)
		{
			Log.Error("OnVersionChecked mOnUpdateData already exist");
			return;
		}
		Log.Debug("Check version result: " + _result);
		mOnUpdateData = new OnUpdateData();
		mOnUpdateData.mResult = _result;
		mOnUpdateData.mLocalVersion = _localVersionId;
		mOnUpdateData.mLastVersion = _lastVersionId;
	}

	private void StartVersionUpdate()
	{
		if (mOnUpdateData != null && mUpdater != null)
		{
			Log.Debug("StartVersionUpdate");
			switch (mOnUpdateData.mResult)
			{
			case Updater.CheckVersionResult.IDENTICAL:
				Log.Debug("update not required");
				mUpdater.CleanTemp(mOnUpdateData.mLastVersion);
				CleanStatsProg();
				InitApplication();
				break;
			case Updater.CheckVersionResult.ERROR:
			{
				Log.Error("update error");
				InitApplication();
				LoginScreen loginScreen2 = mScreenMgr.Holder.GetScreen(ScreenType.LOGIN) as LoginScreen;
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["{ADDR}"] = mUpdateAddr;
				loginScreen2.ShowDeferredPopup("GUI_UPD_SRV_UNAVAILABLE", dictionary);
				break;
			}
			case Updater.CheckVersionResult.FORBIDDEN:
			{
				Log.Debug("update forbidden");
				InitApplication();
				LoginScreen loginScreen = mScreenMgr.Holder.GetScreen(ScreenType.LOGIN) as LoginScreen;
				loginScreen.IsUpdating = true;
				loginScreen.ShowDeferredPopup("GUI_UPD_SRV_FORBIDDEN", null);
				break;
			}
			case Updater.CheckVersionResult.DIFFERENT:
			{
				GuiSystem.mGuiSystem.SetCurGuiSet("update_screen");
				mUpdWnd.SetActive(_active: true);
				mUpdWnd.mLocalVersion = mOnUpdateData.mLocalVersion;
				mUpdWnd.mLastVersion = mOnUpdateData.mLastVersion;
				mUpdater.AddRestartRequired(mUpdater.GetSelfFileName());
				mUpdater.AddRestartRequired(Application.dataPath);
				Updater updater = mUpdater;
				updater.mPrepareCompleteCallback = (Updater.PrepareCompleteCallback)Delegate.Combine(updater.mPrepareCompleteCallback, new Updater.PrepareCompleteCallback(OnPrepared));
				Updater updater2 = mUpdater;
				updater2.mDownloadStarted = (Updater.DownloadStarted)Delegate.Combine(updater2.mDownloadStarted, new Updater.DownloadStarted(OnDownloadStarted));
				UpdateWindow updateWindow = mUpdWnd;
				updateWindow.mCancelUpdateCallback = (UpdateWindow.CancelUpdateCallback)Delegate.Combine(updateWindow.mCancelUpdateCallback, new UpdateWindow.CancelUpdateCallback(CancelUpdate));
				string workingPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
				mUpdater.Prepare(this, this, mUpdateAddr, workingPath, mOnUpdateData.mLastVersion);
				break;
			}
			}
			mOnUpdateData = null;
		}
	}

	private void OnDownloadFailed()
	{
		Log.Error("download failed");
		mUpdWnd.ShowError();
	}

	public TransferOperation CreateTransferOperation(string _uri, Stream _receiver)
	{
		return new WebTransferOperation(_uri, _receiver);
	}

	public TransferOperationGroup CreateTransferOperationGroup()
	{
		return new TransferOperationGroup();
	}

	public Downloader CreateDownloader(IEnumerable<Downloader.Task> _tasks)
	{
		Downloader downloader = new Downloader(_tasks);
		downloader.mOnProgressChanged = (Downloader.OnProgressChanged)Delegate.Combine(downloader.mOnProgressChanged, new Downloader.OnProgressChanged(OnDownloadProgressChanged));
		downloader.mOnStartDownload = (Downloader.OnStartDownload)Delegate.Combine(downloader.mOnStartDownload, new Downloader.OnStartDownload(OnStartDownloadFile));
		mCreatedDownloaders.Add(downloader);
		return downloader;
	}

	private void CancelDownloaders()
	{
		foreach (Downloader mCreatedDownloader in mCreatedDownloaders)
		{
			mCreatedDownloader.Cancel();
		}
		mCreatedDownloaders.Clear();
	}

	public void OnApplicationQuit()
	{
		Log.Debug("Launcher OnApplicationQuit");
		mOnUpdateData = null;
		mUpdater = null;
		CancelDownloaders();
	}

	public void Update()
	{
		//Discarded unreachable code: IL_00c3
		StartVersionUpdate();
		if (mMerge)
		{
			bool flag = false;
			try
			{
				Process currentProcess = Process.GetCurrentProcess();
				Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
				flag = processesByName != null && processesByName.Length == 1;
			}
			catch (SystemException ex)
			{
				Log.Error(ex.Message);
				flag = true;
			}
			if (flag)
			{
				Log.Debug("Restart. Mutex status: " + (mMutex != null));
				mMerge = false;
				Updater updater = new Updater();
				string _destPath;
				try
				{
					updater.Merge(mMergeDataFN, mIgnores.ToArray(), out _destPath);
				}
				catch (UnauthorizedAccessException ex2)
				{
					Log.Error(ex2.Message);
					ShowError("GUI_UPD_PERMISSION_DENIED");
					return;
				}
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = _destPath + "TanatSwitcher.exe";
				processStartInfo.WorkingDirectory = _destPath;
				processStartInfo.Arguments = processStartInfo.Arguments + "\"{" + mWaitCloseMutexName + "}";
				string arguments = processStartInfo.Arguments;
				processStartInfo.Arguments = arguments + "{" + _destPath + updater.GetSelfFileName() + "}";
				processStartInfo.Arguments = processStartInfo.Arguments + "{" + _destPath + "}\"";
				Log.Debug("Restart TanatSwitcher Arguments : " + processStartInfo.Arguments);
				try
				{
					Process.Start(processStartInfo);
				}
				catch (Exception ex3)
				{
					Log.Debug("Process.Start Exception : " + ex3.ToString() + "\n");
				}
				Application.Quit();
			}
		}
		if (mUpdWnd == null || !mUpdWnd.Active)
		{
			return;
		}
		float num = mAimSpeed - mUpdWnd.mSpeed;
		float num2 = Mathf.Abs(num);
		if (num2 > 0.001f)
		{
			if (num2 > 1f)
			{
				mUpdWnd.mSpeed = mAimSpeed;
			}
			else
			{
				float num3 = 0.05f * Time.deltaTime;
				if (num > num3)
				{
					num = num3;
				}
				else if (num < 0f - num3)
				{
					num = 0f - num3;
				}
				mUpdWnd.mSpeed += num;
			}
		}
		float num4 = Time.realtimeSinceStartup - mTimeCalcTime;
		mUpdWnd.mTime = (int)(mRemainingTime - num4);
		if (mUpdWnd.mTime < 0)
		{
			mUpdWnd.mTime = 0;
		}
		float num5 = mUpdWnd.Progress + Time.deltaTime * mProgressSpeed;
		if (num5 > mProgressLim)
		{
			num5 = mProgressLim;
		}
		mUpdWnd.Progress = num5;
	}

	private void CleanStatsProg()
	{
		Log.Debug("CleanStatsProg");
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string text = folderPath;
		folderPath = text + Path.DirectorySeparatorChar + "TanatOnline" + Path.DirectorySeparatorChar + mStatsProgFN;
		if (File.Exists(folderPath))
		{
			try
			{
				File.Delete(folderPath);
			}
			catch (IOException)
			{
				Log.Warning("cannot delete stats prog");
			}
		}
	}

	private void OnPrepared(VersionInfo.Differences _difs, string _tempPath, string _workingPath, string _selfFileName)
	{
		Log.Debug("update preparing complete. Mutex status : " + (mMutex != null));
		if (_difs.IsEmpty())
		{
			InitApplication();
			return;
		}
		_difs.Save(_tempPath + mMergeDataFN, _workingPath);
		if (!Application.isEditor)
		{
			bool flag = false;
			if (IsUACEnabledOS())
			{
				flag = true;
			}
			if (flag)
			{
				Log.Debug("runas");
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = _tempPath + "TanatSwitcher.exe";
			processStartInfo.WorkingDirectory = _tempPath;
			processStartInfo.Arguments = processStartInfo.Arguments + "\"{" + mWaitCloseMutexName + "}";
			string arguments = processStartInfo.Arguments;
			processStartInfo.Arguments = arguments + "{" + _tempPath + _selfFileName + "}";
			processStartInfo.Arguments = processStartInfo.Arguments + "{" + _tempPath + "}";
			processStartInfo.Arguments = processStartInfo.Arguments + "{" + (flag ? 1 : 0) + "}\"";
			Log.Debug("OnPrepared TanatSwitcher Arguments : " + processStartInfo.Arguments);
			try
			{
				Process.Start(processStartInfo);
			}
			catch (Exception ex)
			{
				Log.Debug("Process.Start Exception : " + ex.ToString() + "\n");
			}
			Application.Quit();
		}
	}

	private bool IsUACEnabledOS()
	{
		int major = Environment.OSVersion.Version.Major;
		Log.Debug("OS version: " + major);
		return major >= 6;
	}

	private void CancelUpdate()
	{
		Log.Debug("CancelUpdate");
		Application.Quit();
	}

	private void OnDownloadStarted(long _difsSize)
	{
		mUpdWnd.CheckVersion(_checking: false);
		mFullSize = _difsSize;
		mUpdWnd.mFullSize = Utils.BytesToMB(_difsSize);
		mUpdWnd.mSpeed = 0f;
		mUpdWnd.mTime = 0;
	}

	private void OnStartDownloadFile(long _fileSize)
	{
		float num = (mProgressLim = (float)(mDownloadedSize + _fileSize) / (float)mFullSize);
	}

	private void OnDownloadProgressChanged(long _bytesReceived, double _time)
	{
		if (mFullSize != 0L && _bytesReceived != 0L && !(_time <= 0.0))
		{
			mDownloadedSize += _bytesReceived;
			float progress = (float)mDownloadedSize / (float)mFullSize;
			mUpdWnd.Progress = progress;
			float mTransferedSize = Utils.BytesToMB(mDownloadedSize);
			mUpdWnd.mTransferedSize = mTransferedSize;
			mBytesReceived += _bytesReceived;
			mBytesReceivedTime += _time;
			if (mBytesReceivedTime > 2.0)
			{
				double num = (double)mBytesReceived / mBytesReceivedTime;
				mAimSpeed = Utils.BytesToMB((long)num);
				long num2 = mFullSize - mDownloadedSize;
				mRemainingTime = (float)((double)num2 / num);
				mTimeCalcTime = Time.realtimeSinceStartup;
				mBytesReceived = 0L;
				mBytesReceivedTime = 0.0;
			}
			float num3 = (float)_bytesReceived / (float)mFullSize;
			mProgressSpeed = (float)((double)num3 / _time);
		}
	}

	private void ShowError(string _textId)
	{
		GuiSystem.mGuiSystem.SetCurGuiSet("update_screen");
		OkDialog guiElement = GuiSystem.mGuiSystem.GetGuiElement<OkDialog>("update_screen", "OK_DAILOG");
		string localeText = GuiSystem.GetLocaleText(_textId);
		OkDialog.OnAnswer callback = delegate
		{
			Application.Quit();
		};
		guiElement.SetData(localeText, callback);
	}
}
