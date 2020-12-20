using System;
using System.IO;
using System.Net;
using System.Threading;
using Log4Tanat;

namespace TanatKernel
{
	public class WebTransferOperation : TransferOperation
	{
		public interface CookieReceiver
		{
			void SetCookies(CookieCollection _cookies);
		}

		public CookieReceiver mCookieReceiver;

		public NetworkCredential mProxyCredential;

		public NetworkCredential mCredential;

		public object mData;

		private CookieContainer mCookies = new CookieContainer();

		private byte[] mPostContent;

		private HttpWebRequest mRequest;

		private int mAttemptsCount = 3;

		private int mWaitTime = 1000;

		private bool mNeedReconnect = true;

		public bool mIsForbidden;

		public CookieContainer Cookies => mCookies;

		public byte[] PostContent
		{
			set
			{
				mPostContent = value;
			}
		}

		public void NeedReconnect(bool _need)
		{
			mNeedReconnect = _need;
		}

		static WebTransferOperation()
		{
			ServicePointManager.Expect100Continue = false;
		}

		public void SetReconnectSettings(int _count, int _waitTime)
		{
			mAttemptsCount = _count;
			mWaitTime = _waitTime;
		}

		public WebTransferOperation(string _uri, Stream _receiver)
			: base(_uri, _receiver)
		{
		}

		public WebTransferOperation(string _uri)
			: base(_uri)
		{
		}

		public override void Begin()
		{
			if (mRequest != null)
			{
				Log.Warning("already in use");
				return;
			}
			HttpWebRequest httpWebRequest = null;
			try
			{
				httpWebRequest = WebRequest.Create(mUri) as HttpWebRequest;
			}
			catch (NotSupportedException)
			{
				Log.Error("WebRequest not supported");
			}
			if (httpWebRequest != null)
			{
				httpWebRequest.ServicePoint.ConnectionLimit = 100;
				httpWebRequest.AllowAutoRedirect = false;
				httpWebRequest.CookieContainer = mCookies;
				if (mProxyCredential != null)
				{
					httpWebRequest.Proxy.Credentials = mProxyCredential;
				}
				if (mCredential != null)
				{
					httpWebRequest.Credentials = mCredential;
					httpWebRequest.PreAuthenticate = true;
				}
				if (mPostContent != null)
				{
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded";
					httpWebRequest.ContentLength = mPostContent.Length;
					httpWebRequest.BeginGetRequestStream(OnRequestStream, httpWebRequest);
				}
				else
				{
					httpWebRequest.Method = "GET";
					httpWebRequest.BeginGetResponse(OnWebResponse, httpWebRequest);
				}
				mRequest = httpWebRequest;
			}
		}

		private void OnRequestStream(IAsyncResult _ar)
		{
			HttpWebRequest httpWebRequest = _ar.AsyncState as HttpWebRequest;
			try
			{
				Stream stream = httpWebRequest.EndGetRequestStream(_ar);
				stream.Write(mPostContent, 0, mPostContent.Length);
				stream.Flush();
				stream.Close();
			}
			catch (WebException ex)
			{
				Log.Warning("cannot send web request (WebException): " + ex.Message);
				mNotifiers.Call(_success: false, this);
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					mIsForbidden = ex.Message.Contains("(403)");
				}
				return;
			}
			httpWebRequest.BeginGetResponse(OnWebResponse, httpWebRequest);
		}

		private void OnWebResponse(IAsyncResult _ar)
		{
			HttpWebRequest httpWebRequest = _ar.AsyncState as HttpWebRequest;
			HttpWebResponse httpWebResponse = null;
			bool flag = false;
			bool flag2 = false;
			try
			{
				httpWebResponse = httpWebRequest.EndGetResponse(_ar) as HttpWebResponse;
				if (HttpStatusCode.OK == httpWebResponse.StatusCode)
				{
					if (mCookieReceiver != null)
					{
						Uri uri = new Uri(mUri);
						httpWebResponse.Cookies = httpWebRequest.CookieContainer.GetCookies(uri);
						mCookieReceiver.SetCookies(httpWebResponse.Cookies);
					}
					Stream responseStream = httpWebResponse.GetResponseStream();
					byte[] array = new byte[1024];
					while (true)
					{
						int num = responseStream.Read(array, 0, array.Length);
						if (num >= 0)
						{
							if (num == 0)
							{
								flag = true;
								break;
							}
							mReceiver.Write(array, 0, num);
							continue;
						}
						break;
					}
				}
				else if (HttpStatusCode.Forbidden == httpWebResponse.StatusCode)
				{
					mIsForbidden = true;
				}
			}
			catch (WebException ex)
			{
				Log.Warning("cannot receive web response (WebException): " + ex.Message + "\nstatus: " + ex.Status);
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					mIsForbidden = ex.Message.Contains("(403)");
				}
				flag2 = mNeedReconnect;
			}
			catch (IOException ex2)
			{
				Log.Warning("cannot receive web response (IOException): " + ex2.Message);
			}
			finally
			{
				httpWebResponse?.Close();
			}
			if (httpWebRequest == mRequest)
			{
				mRequest = null;
			}
			else
			{
				Log.Warning("rewritten request");
			}
			if (flag2 && mAttemptsCount > 0)
			{
				mAttemptsCount--;
				Log.Info("send reconnect");
				Thread.Sleep(mWaitTime);
				mRequest = null;
				Begin();
			}
			else if (flag)
			{
				mNotifiers.Call(_success: true, this);
			}
			else
			{
				mNotifiers.Call(_success: false, this);
			}
		}

		public override void End()
		{
			if (mRequest != null)
			{
				mRequest.Abort();
			}
		}
	}
}
