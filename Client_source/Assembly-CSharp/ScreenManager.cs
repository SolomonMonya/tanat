using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

[AddComponentMenu("Tanat/Main/ScreenManager")]
public class ScreenManager : MonoBehaviour
{
	public interface IEventListener
	{
		void CheckEvents(Event _curEvent);
	}

	public class Cursor
	{
		private Texture2D mImage;

		private Vector2 mOffset;

		private Rect mBounds;

		public Cursor(string _name, float _offsetX, float _offsetY)
		{
			mImage = GuiSystem.GetImage("Gui/Cursors/" + _name);
			mOffset.x = _offsetX;
			mOffset.y = _offsetY;
			mBounds.width = mImage.width;
			mBounds.height = mImage.height;
		}

		public void Render(Vector2 _mousePos)
		{
			mBounds.x = _mousePos.x + mOffset.x;
			mBounds.y = (float)Screen.height - _mousePos.y + mOffset.y;
			Graphics.DrawTexture(mBounds, mImage);
		}
	}

	private ScreenHolder mHolder;

	private GuiSystem mGuiSys;

	private LogScreen mLogScreen;

	private FpsCounter mFpsCntr;

	private Win32CursorManager mWinCursorMgr;

	private List<IEventListener> mEventListeners;

	private bool mInited;

	private Cursor mCursor;

	public ScreenHolder Holder => mHolder;

	public GuiSystem Gui => mGuiSys;

	public Win32CursorManager WinCursorMgr => mWinCursorMgr;

	public void Init(Config _config, TaskQueue _taskQueue)
	{
		if (_config == null)
		{
			throw new ArgumentNullException("_config");
		}
		if (mInited)
		{
			return;
		}
		mGuiSys = GameObjUtil.FindObjectOfType<GuiSystem>();
		if (mGuiSys == null)
		{
			Log.Error("cannot find GuiSystem");
			return;
		}
		GuiSystem.mLocaleState = _config.LocaleState;
		mGuiSys.SetData(_taskQueue);
		mGuiSys.Init();
		mGuiSys.SetCurGuiSet("null");
		mLogScreen = GameObjUtil.FindObjectOfType<LogScreen>();
		mFpsCntr = GameObjUtil.FindObjectOfType<FpsCounter>();
		mHolder = new ScreenHolder();
		mEventListeners = new List<IEventListener>();
		if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && _config.WinCursors)
		{
			mWinCursorMgr = new Win32CursorManager();
			Log.Debug("Win32CursorManager created");
			mWinCursorMgr.Init(9);
			mWinCursorMgr.Load("./Assets/cursors/arrow.cur", 0);
			mWinCursorMgr.Load("./Assets/cursors/arrow_enemy.cur", 1);
			mWinCursorMgr.Load("./Assets/cursors/arrow_friend.cur", 2);
			mWinCursorMgr.Load("./Assets/cursors/skill.cur", 3);
			mWinCursorMgr.Load("./Assets/cursors/skill_negative.cur", 4);
			mWinCursorMgr.Load("./Assets/cursors/skill_negative_banned.cur", 5);
			mWinCursorMgr.Load("./Assets/cursors/skill_positive.cur", 6);
			mWinCursorMgr.Load("./Assets/cursors/skill_positive_banned.cur", 7);
			mWinCursorMgr.Load("./Assets/cursors/use.cur", 8);
			mWinCursorMgr.Use(0);
		}
		mInited = true;
	}

	public void OnApplicationQuit()
	{
		if (mWinCursorMgr != null)
		{
			mWinCursorMgr.Release();
		}
		OptionsMgr.SaveOptions();
	}

	public void Update()
	{
		//Discarded unreachable code: IL_0030
		if (mInited)
		{
			try
			{
				mHolder.PerformScreenSwapping();
				mGuiSys.UpdateGUI();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				throw ex;
			}
		}
	}

	public void OnGUI()
	{
		//Discarded unreachable code: IL_00d0
		if (!mInited)
		{
			return;
		}
		if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.F3)
		{
			if (Event.current.control)
			{
				mLogScreen.enabled = !mLogScreen.enabled;
			}
			else
			{
				mFpsCntr.enabled = !mFpsCntr.enabled;
			}
		}
		try
		{
			mGuiSys.OnGui();
			foreach (IEventListener mEventListener in mEventListeners)
			{
				mEventListener.CheckEvents(Event.current);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
		if (Event.current.type == EventType.Repaint)
		{
			if (mLogScreen.enabled)
			{
				mLogScreen.Render();
			}
			if (mFpsCntr.enabled)
			{
				mFpsCntr.Render();
			}
			if (mCursor != null)
			{
				mCursor.Render(Input.mousePosition);
			}
			if (mWinCursorMgr != null && Application.isEditor)
			{
				mWinCursorMgr.Use(mWinCursorMgr.LastCursorId);
			}
		}
	}

	public void SetCursor(Cursor _cursor)
	{
		if (mCursor == null && _cursor != null)
		{
			Screen.showCursor = false;
		}
		if (mCursor != null && _cursor == null)
		{
			Screen.showCursor = true;
		}
		mCursor = _cursor;
	}

	public void AddEventListener(IEventListener _listener)
	{
		if (_listener == null)
		{
			throw new ArgumentNullException("_listener");
		}
		if (!mInited)
		{
			Log.Warning("not inited");
		}
		else if (mEventListeners.Contains(_listener))
		{
			Log.Warning(_listener.GetType().FullName + " already added");
		}
		else
		{
			mEventListeners.Add(_listener);
		}
	}

	public void RemoveEventListener(IEventListener _listener)
	{
		if (_listener == null)
		{
			throw new ArgumentNullException("_listener");
		}
		if (!mInited)
		{
			Log.Warning("not inited");
		}
		else
		{
			mEventListeners.Remove(_listener);
		}
	}

	public void RemoveAllEventListeners()
	{
		if (!mInited)
		{
			Log.Warning("not inited");
		}
		else
		{
			mEventListeners.Clear();
		}
	}
}
