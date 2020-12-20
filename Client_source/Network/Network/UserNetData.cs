using System;
using System.Net;
using Log4Tanat;

namespace Network
{
	public class UserNetData
	{
		private NetworkCredential mUserCredential;

		private string mEMail;

		private int mUserId;

		private string mClientVersion;

		private int mUserFlags;

		private bool mInited;

		private NetworkCredential mProxyCredential;

		public NetworkCredential Credential => mUserCredential;

		public int UserFlags
		{
			get
			{
				return mUserFlags;
			}
			set
			{
				mUserFlags = value;
			}
		}

		public string UserName => mUserCredential.UserName;

		public string Password => mUserCredential.Password;

		public int UserId
		{
			get
			{
				return mUserId;
			}
			set
			{
				mUserId = value;
				Log.Notice(mUserId);
				mInited = true;
			}
		}

		public string EMail => mEMail;

		public bool Inited => mInited;

		public string ClientVersion
		{
			get
			{
				return mClientVersion;
			}
			set
			{
				mClientVersion = value;
			}
		}

		public bool IsProxyEnabled => null != mProxyCredential;

		public NetworkCredential ProxyCredential => mProxyCredential;

		public UserNetData(string _email, string _pass)
		{
			if (_email == null)
			{
				throw new ArgumentNullException("_email");
			}
			if (_pass == null)
			{
				throw new ArgumentNullException("_pass");
			}
			mEMail = _email;
			int num = mEMail.IndexOf('@');
			string userName = ((num != -1) ? mEMail.Substring(0, num) : mEMail);
			mUserCredential = new NetworkCredential(userName, _pass);
		}

		public void UpdateUserName(string _userName)
		{
			if (string.IsNullOrEmpty(_userName))
			{
				throw new ArgumentNullException("_userName");
			}
			mUserCredential.UserName = _userName;
		}

		public void EnableProxy(string _userName, string _pass)
		{
			mProxyCredential = new NetworkCredential(_userName, _pass);
		}

		public void DisableProxy()
		{
			mProxyCredential = null;
		}
	}
}
