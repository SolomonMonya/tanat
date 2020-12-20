using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Log4Tanat;
using TanatKernel;

public class Updater
{
	public enum CheckVersionResult
	{
		NONE,
		ERROR,
		IDENTICAL,
		DIFFERENT,
		FORBIDDEN
	}

	private class MergeData
	{
		public string mTempPath;

		public string mWorkingPath;

		public string mDownloadAddr;

		public string mSelfFileName;

		public VersionInfo.Differences mDifs;

		public string mLastVersionId;

		public ITransferOperationCreator mTransOpCreator;

		public IDownloaderCreator mDownloaderCreator;

		public MemoryStream mContentInfo;

		public bool IsTest()
		{
			return mLastVersionId == "test";
		}
	}

	public delegate void PrepareCompleteCallback(VersionInfo.Differences _difs, string _tempPath, string _workingPath, string _selfFileName);

	public delegate void DownloadFailedCallback();

	public delegate void VersionCallback(CheckVersionResult _result, string _localVersionId, string _lastVersionId);

	public delegate void DownloadStarted(long _difsSize);

	public PrepareCompleteCallback mPrepareCompleteCallback;

	public DownloadFailedCallback mDownloadFailedCallback;

	public VersionCallback mVersionCallback;

	public DownloadStarted mDownloadStarted;

	private List<FileInfo> mRestartRequiredFiles = new List<FileInfo>();

	public void CheckVersion(ITransferOperationCreator _transOpCreator, string _addr, string _localVersion)
	{
		Log.Debug("begin checking version");
		Uri uri = GetUri(_addr, "version");
		TransferOperation transferOperation = _transOpCreator.CreateTransferOperation(uri.AbsoluteUri, new MemoryStream());
		(transferOperation as WebTransferOperation)?.NeedReconnect(_need: false);
		transferOperation.mNotifiers.Add(new Notifier<TransferOperation, object>(OnVersionDownloadComplete, _localVersion));
		transferOperation.Begin();
	}

	private void OnVersionDownloadComplete(bool _success, TransferOperation _op, object _data)
	{
		CheckVersionResult result = CheckVersionResult.ERROR;
		string text = null;
		string text2 = _data.ToString();
		if (_success)
		{
			Stream receiver = _op.Receiver;
			if (receiver.Length > 0)
			{
				receiver.Position = 0L;
				StreamReader streamReader = new StreamReader(receiver, Encoding.ASCII);
				text = streamReader.ReadLine();
				result = ((!(text == text2)) ? CheckVersionResult.DIFFERENT : CheckVersionResult.IDENTICAL);
			}
			receiver.Close();
		}
		else
		{
			WebTransferOperation webTransferOperation = _op as WebTransferOperation;
			if (webTransferOperation != null && webTransferOperation.mIsForbidden)
			{
				Log.Debug("Forbiden");
				result = CheckVersionResult.FORBIDDEN;
			}
		}
		if (mVersionCallback != null)
		{
			mVersionCallback(result, text2, text);
		}
	}

	public void Prepare(ITransferOperationCreator _transOpCreator, IDownloaderCreator _downloaderCreator, string _downloadAddr, string _workingPath, string _lastVersionId)
	{
		MergeData mergeData = new MergeData();
		mergeData.mSelfFileName = GetSelfFileName();
		mergeData.mWorkingPath = _workingPath;
		mergeData.mDownloadAddr = _downloadAddr;
		mergeData.mTransOpCreator = _transOpCreator;
		mergeData.mDownloaderCreator = _downloaderCreator;
		mergeData.mLastVersionId = _lastVersionId;
		mergeData.mContentInfo = new MemoryStream();
		Uri uri = GetUri(_downloadAddr, "tanat_content.xml");
		TransferOperation transferOperation = _transOpCreator.CreateTransferOperation(uri.AbsoluteUri, mergeData.mContentInfo);
		transferOperation.mNotifiers.Add(new Notifier<TransferOperation, object>(OnContentInfoDownloaded, mergeData));
		transferOperation.Begin();
	}

	private void OnContentInfoDownloaded(bool _success, TransferOperation _op, object _data)
	{
		if (_success)
		{
			Prepare(_data as MergeData);
		}
		else
		{
			OnFailed();
		}
	}

	private void OnDownloaded(bool _success, Downloader _downloader, object _data)
	{
		_downloader.Dispose();
		if (_success)
		{
			if (mPrepareCompleteCallback != null)
			{
				MergeData mergeData = _data as MergeData;
				mPrepareCompleteCallback(mergeData.mDifs, mergeData.mTempPath, mergeData.mWorkingPath, mergeData.mSelfFileName);
			}
		}
		else
		{
			OnFailed();
		}
	}

	private void OnFailed()
	{
		if (mDownloadFailedCallback != null)
		{
			mDownloadFailedCallback();
		}
	}

	private void Prepare(MergeData _data)
	{
		VersionInfo versionInfo = new VersionInfo();
		versionInfo.LoadContentData(_data.mContentInfo);
		_data.mContentInfo.Close();
		_data.mContentInfo = null;
		VersionInfo versionInfo2 = new VersionInfo();
		if (!_data.IsTest())
		{
			versionInfo2.ScanCurDir();
		}
		_data.mTempPath = GetTempPath(_data.mLastVersionId);
		_data.mDifs = versionInfo2.Compare(versionInfo);
		PrepareDirs(_data);
		CopyRequared(_data);
		StartDownload(_data);
	}

	private string GetTempPath(string _versionId)
	{
		string text = "update_Tanat_" + _versionId;
		return Path.GetTempPath() + text + Path.DirectorySeparatorChar;
	}

	public void CleanTemp(string _versionId)
	{
		Log.Debug("CleanTemp : " + _versionId);
		string tempPath = GetTempPath(_versionId);
		try
		{
			Directory.Delete(tempPath, recursive: true);
		}
		catch (Exception ex)
		{
			Log.Debug("CleanTemp Exception : " + ex.ToString());
		}
	}

	private void PrepareDirs(MergeData _data)
	{
		Log.Debug("PrepareDirs : " + _data.mLastVersionId);
		CleanTemp(_data.mLastVersionId);
		Directory.CreateDirectory(_data.mTempPath);
		_data.mDifs.PrepareDirs(_data.mTempPath);
	}

	private void CopyRequared(MergeData _data)
	{
		if (_data.IsTest())
		{
			return;
		}
		string text = new DirectoryInfo(".").FullName.ToUpperInvariant();
		foreach (FileInfo mRestartRequiredFile in mRestartRequiredFiles)
		{
			string text2 = string.Empty;
			bool flag = false;
			for (DirectoryInfo directoryInfo = mRestartRequiredFile.Directory; directoryInfo != null; directoryInfo = directoryInfo.Parent)
			{
				if (directoryInfo.FullName.ToUpperInvariant() == text)
				{
					flag = true;
					break;
				}
				text2 = directoryInfo.Name + Path.DirectorySeparatorChar + text2;
			}
			if (flag)
			{
				string text3 = _data.mTempPath + text2;
				if (!Directory.Exists(text3))
				{
					Directory.CreateDirectory(text3);
				}
				mRestartRequiredFile.CopyTo(text3 + mRestartRequiredFile.Name, overwrite: true);
			}
		}
	}

	private void StartDownload(MergeData _data)
	{
		Queue<string> newFiles = _data.mDifs.GetNewFiles();
		if (newFiles == null || newFiles.Count == 0)
		{
			OnDownloaded(_success: true, null, _data);
			return;
		}
		Queue<Downloader.Task> queue = new Queue<Downloader.Task>();
		int num = 0;
		foreach (string item in newFiles)
		{
			Downloader.Task task = new Downloader.Task();
			task.mSrc = GetUri(_data.mDownloadAddr, item);
			task.mDest = _data.mTempPath + ((!item.StartsWith(".\\")) ? item : item.Substring(2));
			task.mFileSize = _data.mDifs.mSizes[num++];
			queue.Enqueue(task);
			Log.Debug("enqueue " + item + " - " + task.mFileSize);
		}
		Downloader downloader = _data.mDownloaderCreator.CreateDownloader(queue);
		downloader.mCompleteNotifiers.Add(new Notifier<Downloader, object>(OnDownloaded, _data));
		downloader.Begin();
		if (mDownloadStarted != null)
		{
			mDownloadStarted(_data.mDifs.mSize);
		}
	}

	public bool Merge(string _mergeDataFN, string[] _ignores, out string _destPath)
	{
		VersionInfo.Differences differences = new VersionInfo.Differences();
		differences.AddIgnores(_ignores);
		if (!differences.Load(_mergeDataFN, out _destPath))
		{
			return false;
		}
		if (!Directory.Exists(_destPath))
		{
			return false;
		}
		differences.Del(_destPath);
		differences.Copy(_destPath);
		return true;
	}

	public void AddRestartRequired(string _name)
	{
		FileInfo fileInfo = new FileInfo(_name);
		if (fileInfo.Exists)
		{
			mRestartRequiredFiles.Add(fileInfo);
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(_name);
		if (directoryInfo.Exists)
		{
			AddRestartRequiredDir(directoryInfo);
		}
	}

	private void AddRestartRequiredDir(DirectoryInfo _dir)
	{
		FileInfo[] files = _dir.GetFiles();
		mRestartRequiredFiles.AddRange(files);
		DirectoryInfo[] directories = _dir.GetDirectories();
		DirectoryInfo[] array = directories;
		foreach (DirectoryInfo dir in array)
		{
			AddRestartRequiredDir(dir);
		}
	}

	public string GetSelfFileFullName()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		return commandLineArgs[0];
	}

	public string GetSelfFileName()
	{
		string selfFileFullName = GetSelfFileFullName();
		return Path.GetFileName(selfFileFullName);
	}

	private Uri GetUri(string _addr, string _fn)
	{
		if (_fn.StartsWith(".\\"))
		{
			_fn = _fn.Substring(2);
		}
		return new Uri(_addr + _fn.Replace('\\', '/'));
	}
}
