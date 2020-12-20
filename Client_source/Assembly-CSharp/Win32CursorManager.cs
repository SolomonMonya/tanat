using System.Runtime.InteropServices;
using Log4Tanat;

public class Win32CursorManager
{
	private int mLastCursorId = -1;

	public int LastCursorId => mLastCursorId;

	[DllImport("Cursor")]
	private static extern int initCursors(int _cursorsCnt);

	[DllImport("Cursor")]
	private static extern void releaseCursors();

	[DllImport("Cursor")]
	private static extern int loadCursor(string _path, int _id);

	[DllImport("Cursor")]
	private static extern int useCursor(int _id);

	public void Init(int _cursorsCnt)
	{
		int num = initCursors(_cursorsCnt);
		if (num != 0)
		{
			Log.Warning("initCursors: " + num);
		}
	}

	public void Release()
	{
		releaseCursors();
	}

	public void Load(string _path, int _cursorId)
	{
		int num = loadCursor(_path, _cursorId);
		if (num != 0)
		{
			Log.Warning("loadCursor: " + num);
		}
	}

	public void Use(int _cursorId)
	{
		int num = useCursor(_cursorId);
		if (num != 0 && num != 1)
		{
			Log.Warning("useCursor: " + num);
		}
		mLastCursorId = _cursorId;
	}
}
