using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("Tanat/Main/TaskQueue")]
public class TaskQueue : MonoBehaviour, IProgressProvider
{
	public interface ITask
	{
		void Begin();

		void Update();

		bool IsDone();

		void End();
	}

	public class Task : ITask
	{
		private Notifier<ITask, object> mNotifier;

		protected bool mBegined;

		public Task(Notifier<ITask, object> _notifier)
		{
			mNotifier = _notifier;
		}

		public Task()
		{
		}

		public virtual void Begin()
		{
			mBegined = true;
		}

		public virtual void Update()
		{
		}

		public virtual bool IsDone()
		{
			return true;
		}

		public virtual void End()
		{
			if (mNotifier != null)
			{
				mNotifier.Call(_success: true, this);
			}
		}
	}

	private List<ITask> mTasks = new List<ITask>();

	private List<ITask> mTasksToBegin = new List<ITask>();

	private List<ITask> mTasksToRemove = new List<ITask>();

	private int mBeginTaskCount;

	public void AddTask(ITask _task)
	{
		if (_task == null)
		{
			throw new ArgumentNullException("_task");
		}
		lock (mTasksToBegin)
		{
			mTasksToBegin.Add(_task);
		}
	}

	public void Update()
	{
		CheckTasksDone();
		BeginTasks();
	}

	private void BeginTasks()
	{
		if (mTasksToBegin.Count == 0)
		{
			return;
		}
		lock (mTasksToBegin)
		{
			mTasks.AddRange(mTasksToBegin);
			mTasksToBegin.Clear();
		}
		foreach (ITask mTask in mTasks)
		{
			mTask.Begin();
		}
	}

	private void CheckTasksDone()
	{
		if (mTasks.Count == 0)
		{
			return;
		}
		foreach (ITask mTask in mTasks)
		{
			try
			{
				mTask.Update();
				if (mTask.IsDone())
				{
					mTasksToRemove.Add(mTask);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception in TaskQueue Update : " + ex.ToString());
			}
		}
		foreach (ITask item in mTasksToRemove)
		{
			item.End();
			mTasks.Remove(item);
		}
		mTasksToRemove.Clear();
	}

	public float GetProgress()
	{
		int count = mTasks.Count;
		if (count == 0)
		{
			return 1f;
		}
		if (count > mBeginTaskCount)
		{
			mBeginTaskCount = count;
		}
		return 1f - (float)count / (float)mBeginTaskCount;
	}

	public void BeginProgress()
	{
		mBeginTaskCount = ((mTasks.Count <= 0) ? 1 : mTasks.Count);
	}

	public void EndProgress()
	{
		mBeginTaskCount = 0;
	}
}
