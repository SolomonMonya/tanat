using System;
using System.Collections.Generic;
using Log4Tanat;

namespace TanatKernel
{
	public class ScreenHolder
	{
		private Dictionary<ScreenType, IScreen> mScreens = new Dictionary<ScreenType, IScreen>();

		private IScreen mCurScreen;

		private IScreen mNewScreen;

		private bool mNeedSwap;

		public void RegisterScreen(ScreenType _type, IScreen _screen)
		{
			if (_screen == null)
			{
				throw new ArgumentNullException();
			}
			if (mScreens.ContainsKey(_type))
			{
				Log.Warning(string.Concat("screen ", _type, " already added"));
			}
			else
			{
				mScreens.Add(_type, _screen);
			}
		}

		public bool IsScreenRegistered(ScreenType _type)
		{
			return mScreens.ContainsKey(_type);
		}

		public void UnregisterScreen(ScreenType _type)
		{
			if (mScreens.TryGetValue(_type, out var value))
			{
				if (value == mCurScreen)
				{
					HideCurScreen();
				}
				mScreens.Remove(_type);
			}
		}

		public void UnregisterAllScreens()
		{
			List<ScreenType> list = new List<ScreenType>(mScreens.Keys);
			foreach (ScreenType item in list)
			{
				UnregisterScreen(item);
			}
		}

		public IScreen ShowScreen(ScreenType _type)
		{
			if (!mScreens.TryGetValue(_type, out var value))
			{
				Log.Warning("cannot find screen " + _type);
			}
			mNewScreen = value;
			mNeedSwap = true;
			Log.Info("new screen: " + _type);
			return mNewScreen;
		}

		public IScreen GetScreen(ScreenType _type)
		{
			mScreens.TryGetValue(_type, out var value);
			return value;
		}

		public void HideCurScreen()
		{
			mNewScreen = null;
			mNeedSwap = true;
		}

		public void PerformScreenSwapping()
		{
			if (!mNeedSwap)
			{
				return;
			}
			if (mCurScreen != null)
			{
				try
				{
					mCurScreen.Hide();
				}
				finally
				{
					mCurScreen = null;
				}
			}
			if (mNewScreen != null)
			{
				try
				{
					mNewScreen.Show();
					Log.Info("new screen showed");
					mCurScreen = mNewScreen;
				}
				finally
				{
					mNewScreen = null;
				}
			}
			mNeedSwap = false;
		}

		public bool IsCurScreen(ScreenType _type)
		{
			if (mCurScreen == null)
			{
				return false;
			}
			foreach (KeyValuePair<ScreenType, IScreen> mScreen in mScreens)
			{
				if (mScreen.Value == mCurScreen)
				{
					return mScreen.Key == _type;
				}
			}
			return false;
		}
	}
}
