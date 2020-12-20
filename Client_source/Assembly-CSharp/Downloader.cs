using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Log4Tanat;
using TanatKernel;

public class Downloader : IDisposable
{
	public class Task
	{
		public Uri mSrc;

		public string mDest;

		public long mFileSize;
	}

	public delegate void OnStartDownload(long _fileSize);

	public delegate void OnProgressChanged(long _bytesReceived, double _time);

	public Notifier<Downloader, object>.Group mCompleteNotifiers = new Notifier<Downloader, object>.Group();

	public OnStartDownload mOnStartDownload;

	public OnProgressChanged mOnProgressChanged;

	private WebClient mClient = new WebClient();

	private Queue<Task> mTasks;

	private int mErrorsCount;

	private Task mPrevious;

	public Downloader(IEnumerable<Task> _tasks)
	{
		if (_tasks == null)
		{
			throw new ArgumentNullException();
		}
		mTasks = new Queue<Task>(_tasks);
		mClient.DownloadFileCompleted += OnDownloadFileCompleted;
		mClient.DownloadProgressChanged += OnDownloadProgressChanged;
	}

	public void Dispose()
	{
		mClient.Dispose();
	}

	public void Begin()
	{
		Thread thread = new Thread(BeginNext);
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.BelowNormal;
		thread.Start();
	}

	private void BeginNext()
	{
		Task task = null;
		lock (mTasks)
		{
			if (mTasks.Count > 0)
			{
				task = mTasks.Dequeue();
			}
		}
		if (task == null)
		{
			mCompleteNotifiers.Call(_success: true, this);
			return;
		}
		Log.Debug("start downloading " + task.mSrc.AbsoluteUri);
		if (mOnStartDownload != null)
		{
			mOnStartDownload(task.mFileSize);
		}
		DateTime now = DateTime.Now;
		bool flag = false;
		try
		{
			mClient.DownloadFile(task.mSrc, task.mDest);
			TimeSpan timeSpan = DateTime.Now.Subtract(now);
			try
			{
				FileInfo fileInfo = new FileInfo(task.mDest);
				if (fileInfo.Length != task.mFileSize)
				{
					flag = true;
					Log.Error("Wrong size. Re downloading. Downloaded : " + fileInfo.Length);
					mTasks.Enqueue(task);
				}
			}
			catch (Exception)
			{
			}
			Log.Debug("done");
		}
		catch (Exception ex2)
		{
			flag = true;
			Log.Error(ex2.Message);
			lock (mTasks)
			{
				mTasks.Enqueue(task);
			}
			if (mPrevious != null)
			{
				try
				{
					FileInfo fileInfo2 = new FileInfo(mPrevious.mDest);
					if (fileInfo2.Length != mPrevious.mFileSize)
					{
						mTasks.Enqueue(mPrevious);
					}
					mPrevious = null;
				}
				catch (Exception)
				{
					mTasks.Enqueue(mPrevious);
				}
			}
		}
		long bytesReceived = task.mFileSize;
		if (flag)
		{
			mErrorsCount++;
			bytesReceived = 0L;
		}
		else
		{
			mPrevious = task;
			mErrorsCount = 0;
		}
		if (mErrorsCount == 3)
		{
			mCompleteNotifiers.Call(_success: false, this);
			return;
		}
		if (mOnProgressChanged != null)
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(now);
			mOnProgressChanged(bytesReceived, timeSpan.TotalSeconds);
		}
		BeginNext();
	}

	public void Cancel()
	{
		lock (mTasks)
		{
			mTasks.Clear();
		}
	}

	private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
	{
	}

	private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
	{
		if (e.Error != null)
		{
			Log.Error(e.Error.Message);
			mCompleteNotifiers.Call(_success: false, this);
		}
		else
		{
			BeginNext();
		}
	}
}
