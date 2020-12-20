using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Log4Tanat;

namespace Network
{
	public class Session
	{
		private Dictionary<string, Cookie> mCookies = new Dictionary<string, Cookie>();

		private string mSavePath;

		public bool IsValid
		{
			get
			{
				foreach (Cookie value in mCookies.Values)
				{
					if (value == null)
					{
						return false;
					}
				}
				return true;
			}
		}

		public Session(IEnumerable<string> _requiredCookieNames)
		{
			if (_requiredCookieNames == null)
			{
				throw new ArgumentNullException("_requiredCookieNames");
			}
			foreach (string _requiredCookieName in _requiredCookieNames)
			{
				mCookies.Add(_requiredCookieName, null);
			}
		}

		public void SetSavePath(string _savePath)
		{
			mSavePath = Path.GetFullPath(_savePath);
		}

		public static bool IsEqualsCookies(Cookie _first, Cookie _second)
		{
			if (_first == null || _second == null)
			{
				if (_first == null)
				{
					return _second == null;
				}
				return false;
			}
			if (_first.Name != _second.Name)
			{
				return false;
			}
			if (_first.Value != _second.Value)
			{
				return false;
			}
			if (_first.Domain != _second.Domain)
			{
				return false;
			}
			if (_first.Path != _second.Path)
			{
				return false;
			}
			return true;
		}

		public void Update(CookieCollection _incomingCookies)
		{
			if (_incomingCookies == null)
			{
				throw new ArgumentNullException("_incomingCookies");
			}
			List<Cookie> list = new List<Cookie>();
			foreach (KeyValuePair<string, Cookie> mCooky in mCookies)
			{
				Cookie cookie = _incomingCookies[mCooky.Key];
				if (cookie != null && !IsEqualsCookies(cookie, mCooky.Value))
				{
					list.Add(cookie);
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			foreach (Cookie item in list)
			{
				mCookies[item.Name] = item;
			}
			Save();
		}

		public void Update(string _key, string _value)
		{
			if (mCookies.TryGetValue(_key, out var value))
			{
				if (value == null)
				{
					value = new Cookie(_key, _value);
					mCookies[_key] = value;
				}
				else
				{
					value.Value = _value;
				}
				Save();
			}
		}

		public string GetValue(string _key)
		{
			mCookies.TryGetValue(_key, out var value);
			if (value == null)
			{
				return "";
			}
			return value.Value;
		}

		public void Distribute(CookieContainer _outcomingCookies)
		{
			if (_outcomingCookies == null)
			{
				throw new ArgumentNullException("_outcomingCookies");
			}
			foreach (Cookie value in mCookies.Values)
			{
				if (value != null)
				{
					_outcomingCookies.Add(value);
				}
			}
		}

		public void Clean()
		{
			List<string> list = new List<string>(mCookies.Keys);
			foreach (string item in list)
			{
				mCookies[item] = null;
			}
		}

		public void CleanSaved()
		{
			if (!string.IsNullOrEmpty(mSavePath) && File.Exists(mSavePath))
			{
				try
				{
					File.Delete(mSavePath);
				}
				catch (IOException ex)
				{
					Log.Warning("cannot delete session file: " + ex.Message);
				}
			}
		}

		public void Save()
		{
			if (string.IsNullOrEmpty(mSavePath) || !IsValid)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (Cookie value in mCookies.Values)
			{
				if (value != null)
				{
					string item = value.Name + "|" + value.Value + "|" + value.Path + "|" + value.Domain;
					list.Add(item);
				}
			}
			try
			{
				string directoryName = Path.GetDirectoryName(mSavePath);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllLines(mSavePath, list.ToArray());
			}
			catch (IOException ex)
			{
				Log.Error("cannot save cookies: " + ex.Message);
			}
		}

		public void TryRead()
		{
			if (string.IsNullOrEmpty(mSavePath) || !File.Exists(mSavePath))
			{
				return;
			}
			string[] array;
			try
			{
				array = File.ReadAllLines(mSavePath);
			}
			catch (IOException ex)
			{
				Log.Error("cannot read cookies: " + ex.Message);
				return;
			}
			List<Cookie> list = new List<Cookie>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(new char[1]
				{
					'|'
				}, StringSplitOptions.RemoveEmptyEntries);
				Cookie cookie;
				try
				{
					if (array3.Length == 4)
					{
						cookie = new Cookie(array3[0], array3[1], array3[2], array3[3]);
					}
					else
					{
						if (array3.Length != 2)
						{
							Log.Warning("invalid cookies file format");
							return;
						}
						cookie = new Cookie(array3[0], array3[1]);
					}
				}
				catch (CookieException ex2)
				{
					Log.Warning("cannot create cookie: " + ex2.Message);
					return;
				}
				if (mCookies.ContainsKey(cookie.Name))
				{
					list.Add(cookie);
				}
			}
			foreach (Cookie item in list)
			{
				mCookies[item.Name] = item;
			}
		}
	}
}
