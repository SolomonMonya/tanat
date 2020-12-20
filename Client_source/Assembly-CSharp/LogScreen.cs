using System.Collections.Generic;
using System.Text;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("Tanat/Info/LogScreen")]
public class LogScreen : MonoBehaviour
{
	private class ScreenLogOutput : LogOutput
	{
		public LogScreen mScreenLog;

		private bool mInited;

		public void Init(LogScreen _screen)
		{
			mScreenLog = _screen;
			mInited = mScreenLog != null;
		}

		public override void Write(Log.Category _category, string _caller, string _time, string _data)
		{
			if (mInited)
			{
				mScreenLog.AddLine(_caller + " " + _data, _category);
			}
		}
	}

	private struct Line
	{
		public string mData;

		public Color mTxtColor;

		public Color mBgColor;
	}

	public Texture2D mBackground;

	private Queue<Line> mLines = new Queue<Line>();

	private object mColorsSync = new object();

	private Dictionary<Log.Category, Color[]> mColors;

	private string mLastError;

	private int mErrorsNum;

	private Vector2 mPos;

	private float mLineHeight;

	public void Start()
	{
		mPos = new Vector2(4f, 4f);
		mLineHeight = 16f;
		InitColors();
		InitOutput();
	}

	private void InitColors()
	{
		mColors = new Dictionary<Log.Category, Color[]>();
		mColors[Log.Category.info] = new Color[2]
		{
			new Color(0.7058824f, 40f / 51f, 0.8235294f),
			new Color(8f / 255f, 4f / 85f, 16f / 255f)
		};
		mColors[Log.Category.debug] = new Color[2]
		{
			new Color(38f / 51f, 38f / 85f, 37f / 255f),
			new Color(8f / 255f, 4f / 85f, 16f / 255f)
		};
		mColors[Log.Category.notice] = new Color[2]
		{
			new Color(0.3529412f, 20f / 51f, 0.4117647f),
			new Color(8f / 255f, 4f / 85f, 16f / 255f)
		};
		mColors[Log.Category.warning] = new Color[2]
		{
			new Color(26f / 255f, 12f / 85f, 14f / 85f),
			new Color(1f, 0.6f, 0.2f)
		};
		mColors[Log.Category.significant] = new Color[2]
		{
			new Color(1f, 1f, 1f),
			new Color(8f / 255f, 4f / 85f, 16f / 255f)
		};
		mColors[Log.Category.error] = new Color[2]
		{
			new Color(26f / 255f, 12f / 85f, 14f / 85f),
			new Color(1f, 0f, 0f)
		};
	}

	public void InitOutput()
	{
		ScreenLogOutput output = Log.GetOutput<ScreenLogOutput>();
		if (output == null)
		{
			Log.Enable<ScreenLogOutput>();
			output = Log.GetOutput<ScreenLogOutput>();
			output.Init(this);
			output.SetMinLevel(3);
		}
	}

	public void AddLine(string _data, Log.Category _style)
	{
		if (this == null)
		{
			return;
		}
		if (_style == Log.Category.error)
		{
			mLastError = _data;
			mErrorsNum++;
		}
		if (!base.enabled)
		{
			return;
		}
		Line item = default(Line);
		item.mData = _data;
		lock (mColorsSync)
		{
			Color[] array = mColors[_style];
			item.mTxtColor = array[0];
			item.mBgColor = array[1];
		}
		lock (mLines)
		{
			mLines.Enqueue(item);
			while (mLines.Count > 3)
			{
				mLines.Dequeue();
			}
		}
	}

	public void Render()
	{
		GUI.color = Color.black;
		GuiSystem.DrawImage(mBackground, new Rect(mPos.x, mPos.y, (float)Screen.width - mPos.x, mLineHeight * 3f));
		GUI.color = Color.red;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("log:");
		if (mErrorsNum > 0)
		{
			stringBuilder.Append(" errors = ");
			stringBuilder.Append(mErrorsNum);
			stringBuilder.Append(", last = " + mLastError);
		}
		else
		{
			stringBuilder.Append(" no errors");
		}
		GuiSystem.DrawString(stringBuilder.ToString(), new Rect(mPos.x + 5f, mPos.y, (float)Screen.width - mPos.x, mLineHeight), "log");
		float num = mPos.y + mLineHeight;
		lock (mLines)
		{
			foreach (Line mLine in mLines)
			{
				GUI.color = mLine.mBgColor;
				GuiSystem.DrawImage(mBackground, new Rect(mPos.x, num, (float)Screen.width - mPos.x, mLineHeight));
				GUI.color = mLine.mTxtColor;
				GuiSystem.DrawString(mLine.mData, new Rect(mPos.x + 5f, num, (float)Screen.width - mPos.x, mLineHeight), "log");
				num += mLineHeight;
			}
		}
	}
}
