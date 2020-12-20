using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UnityEngine
{
	public sealed class WWW : IDisposable
	{
		private IntPtr wwwWrapper;

		public Dictionary<string, string> responseHeaders
		{
			get
			{
				if (!isDone)
				{
					throw new UnityException("WWW is not finished downloading yet");
				}
				return ParseHTTPHeaderString(responseHeadersString);
			}
		}

		private string responseHeadersString
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string text
		{
			get
			{
				if (!isDone)
				{
					throw new UnityException("WWW is not ready downloading yet");
				}
				byte[] bytes = this.bytes;
				Encoding encoding = null;
				string value = null;
				if (responseHeaders.TryGetValue("CONTENT-ENCODING", out value))
				{
					try
					{
						encoding = Encoding.GetEncoding(value);
					}
					catch (Exception)
					{
					}
				}
				if (encoding == null)
				{
					encoding = Encoding.UTF8;
				}
				return encoding.GetString(bytes);
			}
		}

		[Obsolete("Please use WWW.text instead")]
		public string data => text;

		public byte[] bytes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int size
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string error
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Texture2D texture
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AudioClip audioClip => GetAudioClip(threeD: true);

		public MovieTexture movie
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool isDone
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float progress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float uploadProgress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete(".oggVorbis accessor is deprecated, use .audioClip or GetAudioClip() instead.")]
		public AudioClip oggVorbis
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string url
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AssetBundle assetBundle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ThreadPriority threadPriority
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public WWW(string url)
			: this(url, null, null)
		{
		}

		public WWW(string url, WWWForm form)
		{
			Hashtable headers = form.headers;
			string[] array = null;
			if (headers != null)
			{
				array = new string[headers.Count * 2];
				int num = 0;
				foreach (DictionaryEntry item in headers)
				{
					array[num++] = item.Key.ToString();
					array[num++] = item.Value.ToString();
				}
			}
			InitWWW(url, form.data, array);
		}

		public WWW(string url, byte[] postData)
			: this(url, postData, null)
		{
		}

		public WWW(string url, byte[] postData, Hashtable headers)
		{
			string[] array = null;
			if (headers != null)
			{
				array = new string[headers.Count * 2];
				int num = 0;
				foreach (DictionaryEntry header in headers)
				{
					array[num++] = header.Key.ToString();
					array[num++] = header.Value.ToString();
				}
			}
			InitWWW(url, postData, array);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern WWW(string url, int version);

		internal static Dictionary<string, string> ParseHTTPHeaderString(string input)
		{
			if (input == null)
			{
				throw new ArgumentException("input was null to ParseHTTPHeaderString");
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			StringReader stringReader = new StringReader(input);
			while (true)
			{
				string text = stringReader.ReadLine();
				if (text == null)
				{
					break;
				}
				int num = text.IndexOf(": ");
				if (num != -1)
				{
					string key = text.Substring(0, num).ToUpper();
					string text3 = (dictionary[key] = text.Substring(num + 2));
				}
			}
			return dictionary;
		}

		public void Dispose()
		{
			DestroyWWW(cancel: true);
		}

		~WWW()
		{
			DestroyWWW(cancel: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void DestroyWWW(bool cancel);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void InitWWW(string url, byte[] postData, string[] iHeaders);

		public static string EscapeURL(string s)
		{
			Encoding uTF = Encoding.UTF8;
			return EscapeURL(s, uTF);
		}

		public static string EscapeURL(string s, Encoding e)
		{
			if (s == null)
			{
				return null;
			}
			if (s == string.Empty)
			{
				return string.Empty;
			}
			return WWWTranscoder.URLEncode(s, e);
		}

		public static string UnEscapeURL(string s)
		{
			Encoding uTF = Encoding.UTF8;
			return UnEscapeURL(s, uTF);
		}

		public static string UnEscapeURL(string s, Encoding e)
		{
			if (s == null)
			{
				return null;
			}
			if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
			{
				return s;
			}
			return WWWTranscoder.URLDecode(s, e);
		}

		public AudioClip GetAudioClip(bool threeD)
		{
			return GetAudioClip(threeD, stream: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern AudioClip GetAudioClip(bool threeD, bool stream);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void LoadImageIntoTexture(Texture2D tex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[Obsolete("All blocking WWW functions have been deprecated, please use one of the asynchronous functions instead.", true)]
		[WrapperlessIcall]
		public static extern string GetURL(string url);

		[Obsolete("All blocking WWW functions have been deprecated, please use one of the asynchronous functions instead.", true)]
		public static Texture2D GetTextureFromURL(string url)
		{
			return new WWW(url).texture;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void LoadUnityWeb();

		public static WWW LoadFromCacheOrDownload(string url, int version)
		{
			return new WWW(url, version);
		}
	}
}
