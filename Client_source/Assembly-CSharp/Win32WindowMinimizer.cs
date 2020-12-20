using System.Runtime.InteropServices;
using System.Threading;

public class Win32WindowMinimizer
{
	private const int SW_SHOWMINIMIZED = 2;

	[DllImport("user32")]
	private static extern int ShowWindow(int hwnd, int nCmdShow);

	[DllImport("user32")]
	private static extern int GetActiveWindow();

	[DllImport("user32")]
	private static extern int GetWindowThreadProcessId(int hWnd, int processId);

	public void MinimizeIfActive()
	{
		int activeWindow = GetActiveWindow();
		int windowThreadProcessId = GetWindowThreadProcessId(activeWindow, 0);
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		if (windowThreadProcessId == managedThreadId)
		{
			ShowWindow(activeWindow, 2);
		}
	}
}
