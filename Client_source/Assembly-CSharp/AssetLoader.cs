using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class AssetLoader
{
	private class PackInfo
	{
		public string mFileName;

		public string mPath;
	}

	private class LoadedAsset : ILoadedAsset
	{
		public int mCacheId = -1;

		private Type mType;

		private UnityEngine.Object mAsset;

		public Type AssetType => mType;

		public UnityEngine.Object Asset => mAsset;

		public LoadedAsset(Type _assetType, UnityEngine.Object _asset)
		{
			if (_assetType == null)
			{
				throw new ArgumentNullException("_assetType");
			}
			if (_asset == null)
			{
				throw new ArgumentNullException("_asset");
			}
			mType = _assetType;
			mAsset = _asset;
		}

		public LoadedAsset(Type _assetType, WWW _loader)
		{
			if (_loader == null)
			{
				throw new ArgumentNullException("_loader");
			}
			if (_assetType == null)
			{
				throw new ArgumentNullException("_assetType");
			}
			mType = _assetType;
			if (_assetType == typeof(AssetBundle))
			{
				mAsset = _loader.assetBundle;
			}
			else if (_assetType == typeof(AudioClip))
			{
				mAsset = _loader.audioClip;
			}
			else if (_assetType == typeof(Texture2D))
			{
				mAsset = _loader.texture;
			}
		}

		public LoadedAsset(Type _assetType, AssetBundle _bundle, string _assetName)
		{
			if (_assetType == null)
			{
				throw new ArgumentNullException("_assetType");
			}
			if (_bundle == null)
			{
				throw new ArgumentNullException("_bundle");
			}
			if (string.IsNullOrEmpty(_assetName))
			{
				throw new ArgumentNullException("_assetName");
			}
			mType = _assetType;
			mAsset = _bundle.Load(_assetName, _assetType);
		}
	}

	private class AssetsHolder
	{
		public List<int> mKeepAlive = new List<int>();

		private Dictionary<string, ILoadedAsset> mLoaded = new Dictionary<string, ILoadedAsset>();

		private int mNextCacheId;

		public void Save(ILoadedAsset _asset, Uri _uri)
		{
			LoadedAsset loadedAsset = _asset as LoadedAsset;
			if (loadedAsset != null)
			{
				loadedAsset.mCacheId = mNextCacheId++;
			}
			try
			{
				mLoaded.Add(_uri.ToString(), _asset);
			}
			catch (ArgumentException)
			{
				Log.Error("secondary resource addition: " + _uri);
			}
		}

		public ILoadedAsset TryGet(Uri _uri)
		{
			mLoaded.TryGetValue(_uri.ToString(), out var value);
			return value;
		}

		public void UnloadAll()
		{
			Dictionary<string, ILoadedAsset> dictionary = new Dictionary<string, ILoadedAsset>();
			foreach (KeyValuePair<string, ILoadedAsset> item in mLoaded)
			{
				LoadedAsset loadedAsset = item.Value as LoadedAsset;
				if (loadedAsset != null && mKeepAlive.Contains(loadedAsset.mCacheId))
				{
					dictionary.Add(item.Key, item.Value);
					continue;
				}
				AssetBundle assetBundle = item.Value.Asset as AssetBundle;
				if (assetBundle != null)
				{
					assetBundle.Unload(unloadAllLoadedObjects: true);
				}
				else if (item.Value.AssetType != typeof(GameObject))
				{
					UnityEngine.Object.Destroy(item.Value.Asset);
				}
			}
			mLoaded.Clear();
			mLoaded = dictionary;
		}
	}

	private abstract class BaseLoadTask : TaskQueue.ITask
	{
		protected ILoadedAsset mAsset;

		protected Uri mPath;

		protected Type mAssetType;

		protected AssetsHolder mHolder;

		protected bool mBegined;

		protected Notifier<ILoadedAsset, object>.Group mNotifiers = new Notifier<ILoadedAsset, object>.Group();

		private Dictionary<string, BaseLoadTask> mInProgressTasks;

		public ThreadPriority mPriority = ThreadPriority.Normal;

		public BaseLoadTask(Uri _path, Type _assetType, AssetsHolder _holder, Notifier<ILoadedAsset, object> _notifier, Dictionary<string, BaseLoadTask> _inProgressTasks)
		{
			mPath = _path;
			mAssetType = _assetType;
			mHolder = _holder;
			mInProgressTasks = _inProgressTasks;
			AddNotifier(_notifier);
		}

		public void AddNotifier(Notifier<ILoadedAsset, object> _notifier)
		{
			mNotifiers.Add(_notifier);
		}

		protected void Complete()
		{
			mInProgressTasks.Remove(mPath.ToString());
		}

		public void Update()
		{
		}

		public virtual void Begin()
		{
			mBegined = true;
		}

		public abstract bool IsDone();

		public abstract void End();
	}

	private class LoadTask : BaseLoadTask
	{
		private WWW mLoader;

		public LoadTask(Uri _path, Type _assetType, AssetsHolder _holder, Notifier<ILoadedAsset, object> _notifier, Dictionary<string, BaseLoadTask> _inProgressTasks)
			: base(_path, _assetType, _holder, _notifier, _inProgressTasks)
		{
		}

		public override void Begin()
		{
			if (mLoader == null)
			{
				mLoader = new WWW(mPath.AbsoluteUri);
				mLoader.threadPriority = mPriority;
			}
		}

		public override bool IsDone()
		{
			return mLoader == null || mLoader.isDone;
		}

		public override void End()
		{
			bool flag;
			if (mLoader != null)
			{
				flag = string.IsNullOrEmpty(mLoader.error);
				if (flag)
				{
					mAsset = new LoadedAsset(mAssetType, mLoader);
					mHolder.Save(mAsset, mPath);
				}
				else
				{
					Log.Error("loading error: " + mLoader.error);
				}
				mLoader.Dispose();
				mLoader = null;
			}
			else
			{
				flag = mAsset != null;
			}
			Complete();
			try
			{
				mNotifiers.Call(flag, mAsset);
			}
			catch (Exception ex)
			{
				Log.Error("On Resource Loaded Notifier Exception : " + ex.Message);
			}
		}
	}

	private class UnpackData
	{
		public string mAssetName;

		public Type mAssetType;

		public Notifier<ILoadedAsset, object> mUserNotifier;
	}

	private Dictionary<Type, Dictionary<string, PackInfo>> mAssetsData = new Dictionary<Type, Dictionary<string, PackInfo>>();

	private TaskQueue mTaskQueue;

	private AssetsHolder mHolder = new AssetsHolder();

	private Dictionary<string, BaseLoadTask> mInProgressTasks = new Dictionary<string, BaseLoadTask>();

	private static AssetLoader mInstance;

	public static AssetLoader Instance => mInstance;

	public AssetLoader(TaskQueue _taskQueue)
	{
		if (_taskQueue == null)
		{
			throw new ArgumentNullException("_taskQueue");
		}
		mTaskQueue = _taskQueue;
		mInstance = this;
	}

	public void LoadAssetsDataFromFile(string _fn)
	{
		//Discarded unreachable code: IL_0044
		mAssetsData.Clear();
		if (File.Exists(_fn))
		{
			XmlDocument xmlDocument;
			try
			{
				xmlDocument = new XmlDocument();
				xmlDocument.Load(_fn);
			}
			catch (XmlException ex)
			{
				Log.Warning("error while parsing resources data: " + ex.Message);
				return;
			}
			LoadAssetsData(xmlDocument);
		}
	}

	private void LoadAssetsData(XmlDocument _doc)
	{
		try
		{
			XmlNode xmlNode = _doc.SelectSingleNode("Resources");
			if (xmlNode == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = xmlNode.SelectNodes("Bundle");
			foreach (XmlNode item in xmlNodeList)
			{
				if (!item.HasChildNodes)
				{
					continue;
				}
				PackInfo packInfo = new PackInfo();
				packInfo.mFileName = XmlUtil.SafeReadText("name", item);
				packInfo.mPath = XmlUtil.SafeReadText("path", item);
				for (XmlNode xmlNode3 = item.FirstChild; xmlNode3 != null; xmlNode3 = xmlNode3.NextSibling)
				{
					Type type = null;
					switch (xmlNode3.Name)
					{
					case "Prefab":
						type = typeof(GameObject);
						break;
					case "Texture2D":
						type = typeof(Texture2D);
						break;
					case "Material":
						type = typeof(Material);
						break;
					default:
						Log.Warning("unsupported type " + xmlNode3.Name);
						continue;
					}
					if (!mAssetsData.TryGetValue(type, out var value))
					{
						value = new Dictionary<string, PackInfo>();
						mAssetsData.Add(type, value);
					}
					string text = XmlUtil.SafeReadText("name", xmlNode3);
					if (value.ContainsKey(text))
					{
						Log.Warning("asset " + text + " already exists");
					}
					else
					{
						value.Add(text, packInfo);
					}
				}
			}
		}
		catch (XmlException ex)
		{
			Log.Warning("error while parsing resources data: " + ex.Message);
		}
	}

	private string GetPackedPath(PackInfo _pack, string _prefabName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("file://");
		if (_pack.mPath.StartsWith("data"))
		{
			stringBuilder.Append("Assets/Content");
			stringBuilder.Append(_pack.mPath.Remove(0, "data".Length));
		}
		else
		{
			stringBuilder.Append(_pack.mPath);
		}
		int startIndex = _pack.mFileName.LastIndexOf(".unity3d");
		stringBuilder.Append(_pack.mFileName.Remove(startIndex));
		stringBuilder.Append("/Prefabs/");
		stringBuilder.Append(_prefabName);
		stringBuilder.Append(".prefab");
		return stringBuilder.ToString();
	}

	private PackInfo GetPackInfo(string _assetName, Type _assetType)
	{
		if (!mAssetsData.TryGetValue(_assetType, out var value))
		{
			return null;
		}
		value.TryGetValue(_assetName, out var value2);
		return value2;
	}

	private ILoadedAsset GetAssetFromCache(string _assetName, Type _assetType)
	{
		//Discarded unreachable code: IL_0088
		PackInfo packInfo = GetPackInfo(_assetName, _assetType);
		string uriString;
		if (packInfo != null)
		{
			if (Application.isEditor && _assetType == typeof(GameObject))
			{
				uriString = GetPackedPath(packInfo, _assetName);
			}
			else
			{
				uriString = packInfo.mPath + packInfo.mFileName;
				uriString = Path.GetFullPath(uriString);
			}
		}
		else
		{
			uriString = _assetName;
			uriString = Path.GetFullPath(uriString);
		}
		Uri uri;
		try
		{
			uri = new Uri(uriString);
		}
		catch (UriFormatException ex)
		{
			Log.Error("invalid path: " + ex.Message);
			return null;
		}
		ILoadedAsset loadedAsset = mHolder.TryGet(uri);
		if (loadedAsset == null)
		{
			return null;
		}
		if (packInfo != null && _assetType != typeof(AssetBundle) && loadedAsset.AssetType == typeof(AssetBundle))
		{
			AssetBundle assetBundle = loadedAsset.Asset as AssetBundle;
			if (assetBundle.Contains(_assetName))
			{
				loadedAsset = new LoadedAsset(_assetType, assetBundle, _assetName);
			}
			else
			{
				Log.Error("bundle doesn't contain " + _assetName);
				loadedAsset = null;
			}
		}
		return loadedAsset;
	}

	public void LoadAsset(string _assetName, Type _assetType, Notifier<ILoadedAsset, object> _notifier)
	{
		ILoadedAsset assetFromCache = GetAssetFromCache(_assetName, _assetType);
		if (assetFromCache != null)
		{
			_notifier?.mCallback(_success: true, assetFromCache, _notifier.mData);
		}
		else
		{
			LoadAsset(_assetName, _assetType, _notifier, ThreadPriority.Normal);
		}
	}

	public void LoadAsset(string _assetName, Type _assetType, Notifier<ILoadedAsset, object> _notifier, ThreadPriority _priority)
	{
		//Discarded unreachable code: IL_00fc
		if (string.IsNullOrEmpty(_assetName))
		{
			throw new ArgumentNullException("_assetName");
		}
		if (_assetType == null)
		{
			throw new ArgumentNullException("_assetType");
		}
		PackInfo packInfo = GetPackInfo(_assetName, _assetType);
		string uriString;
		if (packInfo != null)
		{
			if (Application.isEditor && _assetType == typeof(GameObject))
			{
				uriString = GetPackedPath(packInfo, _assetName);
			}
			else
			{
				if (_assetType != typeof(AssetBundle))
				{
					UnpackData unpackData = new UnpackData();
					unpackData.mAssetName = _assetName;
					unpackData.mAssetType = _assetType;
					unpackData.mUserNotifier = _notifier;
					Notifier<ILoadedAsset, object> notifier = new Notifier<ILoadedAsset, object>(OnBundleLoaded, unpackData);
					_assetType = typeof(AssetBundle);
					_notifier = notifier;
				}
				uriString = packInfo.mPath + packInfo.mFileName;
				uriString = Path.GetFullPath(uriString);
			}
		}
		else
		{
			uriString = _assetName;
			uriString = Path.GetFullPath(uriString);
		}
		Uri uri;
		try
		{
			uri = new Uri(uriString);
		}
		catch (UriFormatException ex)
		{
			Log.Error("invalid path: " + ex.Message);
			return;
		}
		if (mInProgressTasks.TryGetValue(uri.ToString(), out var value))
		{
			value.AddNotifier(_notifier);
			return;
		}
		value = new LoadTask(uri, _assetType, mHolder, _notifier, mInProgressTasks);
		value.mPriority = _priority;
		mInProgressTasks.Add(uri.ToString(), value);
		mTaskQueue.AddTask(value);
	}

	private void OnBundleLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		UnpackData unpackData = _data as UnpackData;
		ILoadedAsset owner = null;
		if (_success)
		{
			AssetBundle assetBundle = _asset.Asset as AssetBundle;
			if (assetBundle.Contains(unpackData.mAssetName))
			{
				owner = new LoadedAsset(unpackData.mAssetType, assetBundle, unpackData.mAssetName);
			}
			else
			{
				Log.Error("bundle doesn't contain " + unpackData.mAssetName);
				_success = false;
			}
		}
		if (unpackData.mUserNotifier != null)
		{
			unpackData.mUserNotifier.Call(_success, owner);
		}
	}

	public void UnloadAll()
	{
		mHolder.UnloadAll();
	}

	public void KeepAlive(ILoadedAsset _asset)
	{
		LoadedAsset loadedAsset = _asset as LoadedAsset;
		if (loadedAsset != null)
		{
			mHolder.mKeepAlive.Add(loadedAsset.mCacheId);
		}
	}

	public static string ReadText(string _txtResourceName)
	{
		if (string.IsNullOrEmpty(_txtResourceName))
		{
			throw new ArgumentNullException("_txtResourceName");
		}
		TextAsset textAsset = Resources.Load(_txtResourceName, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			throw new ArgumentException("text resource " + _txtResourceName + " does not exist");
		}
		return textAsset.text;
	}

	public static void Clear()
	{
		mInstance = null;
	}
}
