using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

public class VersionInfo
{
	public class Differences
	{
		public long mSize;

		public List<long> mSizes = new List<long>();

		private Dictionary<Type, DiffCategory> mCategories = new Dictionary<Type, DiffCategory>();

		private List<string> mIgnore = new List<string>();

		public void AddIgnores(string[] _ignores)
		{
			if (_ignores != null && _ignores.Length != 0)
			{
				mIgnore.AddRange(_ignores);
			}
		}

		public void Add(Type _categoryType, string _path, bool _new)
		{
			if (!mCategories.TryGetValue(_categoryType, out var value))
			{
				value = new DiffCategory();
				mCategories.Add(_categoryType, value);
			}
			if (_new)
			{
				value.mNew.Add(_path);
			}
			else
			{
				value.mOld.Add(_path);
			}
		}

		private XmlDocument Save(string _destPath)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement xmlElement = xmlDocument.CreateElement("merge");
			xmlDocument.AppendChild(xmlElement);
			xmlElement.SetAttribute("destPath", _destPath);
			Save(xmlElement, xmlDocument, typeof(DirData), "rd", "mkdir");
			Save(xmlElement, xmlDocument, typeof(FileData), "del", "copy");
			return xmlDocument;
		}

		public void Save(TextWriter _output, string _destPath)
		{
			XmlDocument xmlDocument = Save(_destPath);
			xmlDocument.Save(_output);
		}

		public void Save(string _fn, string _destPath)
		{
			XmlDocument xmlDocument = Save(_destPath);
			xmlDocument.Save(_fn);
		}

		private void Save(XmlElement _node, XmlDocument _doc, Type _category, string _oldName, string _newName)
		{
			if (mCategories.TryGetValue(_category, out var value))
			{
				Save(_node, _doc, value.mNew, _newName);
				Save(_node, _doc, value.mOld, _oldName);
			}
		}

		private void Save(XmlElement _node, XmlDocument _doc, List<string> _paths, string _categoryName)
		{
			XmlElement xmlElement = _doc.CreateElement(_categoryName);
			_node.AppendChild(xmlElement);
			foreach (string _path in _paths)
			{
				XmlElement xmlElement2 = _doc.CreateElement("path");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("value", _path);
			}
		}

		public bool Load(string _fn, out string _destPath)
		{
			//Discarded unreachable code: IL_009b
			_destPath = string.Empty;
			mCategories.Clear();
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(_fn);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("merge");
				if (xmlNode == null)
				{
					return false;
				}
				XmlAttribute xmlAttribute = xmlNode.Attributes["destPath"];
				if (xmlAttribute == null)
				{
					return false;
				}
				_destPath = xmlAttribute.InnerText;
				Load(xmlNode, typeof(FileData), "del", "copy");
				Load(xmlNode, typeof(DirData), "rd", "mkdir");
				return true;
			}
			catch (FileNotFoundException)
			{
			}
			catch (XmlException)
			{
			}
			return false;
		}

		private void Load(XmlNode _root, Type _category, string _oldName, string _newName)
		{
			DiffCategory diffCategory = new DiffCategory();
			Load(_root.SelectSingleNode(_oldName), diffCategory.mOld);
			Load(_root.SelectSingleNode(_newName), diffCategory.mNew);
			mCategories[_category] = diffCategory;
		}

		private void Load(XmlNode _node, List<string> _paths)
		{
			if (_node == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = _node.SelectNodes("path");
			foreach (XmlNode item in xmlNodeList)
			{
				XmlAttribute xmlAttribute = item.Attributes["value"];
				if (xmlAttribute != null)
				{
					_paths.Add(xmlAttribute.InnerText);
				}
			}
		}

		public void PrepareDirs(string _workDir)
		{
			foreach (KeyValuePair<Type, DiffCategory> mCategory in mCategories)
			{
				foreach (string item in mCategory.Value.mNew)
				{
					string path = Path.GetFullPath(_workDir + item);
					if (mCategory.Key == typeof(FileData))
					{
						path = Path.GetDirectoryName(path);
					}
					Directory.CreateDirectory(path);
				}
			}
		}

		public void Del(string _workDir)
		{
			if (mCategories.TryGetValue(typeof(DirData), out var value))
			{
				foreach (string item in value.mOld)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(_workDir + item);
					if (directoryInfo.Exists)
					{
						directoryInfo.Delete(recursive: true);
					}
				}
			}
			if (!mCategories.TryGetValue(typeof(FileData), out value))
			{
				return;
			}
			foreach (string item2 in value.mOld)
			{
				FileInfo fileInfo = new FileInfo(_workDir + item2);
				if (!mIgnore.Contains(fileInfo.Name) && fileInfo.Exists)
				{
					fileInfo.Delete();
				}
			}
		}

		public void Copy(string _workDir)
		{
			if (mCategories.TryGetValue(typeof(DirData), out var value))
			{
				foreach (string item in value.mNew)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(_workDir + item);
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
				}
			}
			if (!mCategories.TryGetValue(typeof(FileData), out value))
			{
				return;
			}
			foreach (string item2 in value.mNew)
			{
				FileInfo fileInfo = new FileInfo(item2);
				if (!mIgnore.Contains(fileInfo.Name))
				{
					fileInfo.CopyTo(_workDir + item2, overwrite: true);
				}
			}
		}

		public Queue<string> GetNewFiles()
		{
			if (mCategories.TryGetValue(typeof(FileData), out var value))
			{
				return new Queue<string>(value.mNew);
			}
			return null;
		}

		public bool IsEmpty()
		{
			foreach (KeyValuePair<Type, DiffCategory> mCategory in mCategories)
			{
				if (mCategory.Value.mNew.Count > 0 || mCategory.Value.mOld.Count > 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	private class DiffCategory
	{
		public List<string> mNew = new List<string>();

		public List<string> mOld = new List<string>();
	}

	private abstract class BaseData
	{
		private string mName;

		private DirData mParent;

		public BaseData(string _name, DirData _parent)
		{
			Init(_name, _parent);
		}

		public BaseData(XmlNode _node, DirData _parent)
		{
			if (_node == null)
			{
				throw new ArgumentException();
			}
			XmlAttribute xmlAttribute = _node.Attributes["name"];
			if (xmlAttribute == null)
			{
				throw new ArgumentException();
			}
			Init(xmlAttribute.InnerText, _parent);
		}

		private void Init(string _name, DirData _parent)
		{
			if (string.IsNullOrEmpty(_name))
			{
				throw new ArgumentException();
			}
			mName = _name;
			mParent = _parent;
			if (mParent != null)
			{
				mParent.Add(this);
			}
		}

		public string GetFullName()
		{
			string text = mName;
			if (mParent != null)
			{
				text = mParent.GetFullName() + Path.DirectorySeparatorChar + text;
			}
			return text;
		}

		protected XmlElement Save(XmlElement _parent, XmlDocument _doc, string _elemName)
		{
			XmlElement xmlElement = _doc.CreateElement(_elemName);
			_parent.AppendChild(xmlElement);
			xmlElement.SetAttribute("name", mName);
			return xmlElement;
		}

		public abstract XmlElement Save(XmlElement _parent, XmlDocument _doc);

		public bool Equals(BaseData _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (GetType() != _other.GetType())
			{
				return false;
			}
			return string.Compare(mName, _other.mName, ignoreCase: true) == 0;
		}

		public virtual void SaveDiff(bool _new, Differences _difs)
		{
			_difs.Add(GetType(), GetFullName(), _new);
		}

		public abstract void Compare(BaseData _other, Differences _difs);
	}

	private class FileData : BaseData
	{
		public byte[] mHash;

		public long mSize;

		public FileData(string _name, DirData _parent)
			: base(_name, _parent)
		{
		}

		public FileData(XmlNode _node, DirData _parent)
			: base(_node, _parent)
		{
			XmlAttribute xmlAttribute = _node.Attributes["hash"];
			if (xmlAttribute == null)
			{
				throw new ArgumentException();
			}
			int num = xmlAttribute.InnerText.Length >> 1;
			if (num != 16)
			{
				throw new ArgumentException();
			}
			mHash = new byte[num];
			char[] value = xmlAttribute.InnerText.ToCharArray();
			for (int num2 = mHash.Length - 1; num2 >= 0; num2--)
			{
				string s = new string(value, num2 << 1, 2);
				mHash[num2] = byte.Parse(s, NumberStyles.HexNumber);
			}
			XmlAttribute xmlAttribute2 = _node.Attributes["size"];
			if (xmlAttribute2 == null)
			{
				throw new ArgumentException();
			}
			mSize = long.Parse(xmlAttribute2.InnerText);
		}

		public string GetHashStr()
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = mHash;
			foreach (byte b in array)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		public bool EqualHash(FileData _other)
		{
			if (mHash == null && _other.mHash == null)
			{
				return true;
			}
			if (mHash == null || _other.mHash == null)
			{
				return false;
			}
			if (mHash.Length != _other.mHash.Length)
			{
				return false;
			}
			for (int num = mHash.Length - 1; num >= 0; num--)
			{
				if (mHash[num] != _other.mHash[num])
				{
					return false;
				}
			}
			return true;
		}

		public override XmlElement Save(XmlElement _parent, XmlDocument _doc)
		{
			if (mHash == null)
			{
				return null;
			}
			XmlElement xmlElement = Save(_parent, _doc, "file");
			string hashStr = GetHashStr();
			xmlElement.SetAttribute("hash", hashStr);
			xmlElement.SetAttribute("size", mSize.ToString());
			return xmlElement;
		}

		public override void Compare(BaseData _other, Differences _difs)
		{
			FileData fileData = _other as FileData;
			if (fileData != null && !EqualHash(fileData))
			{
				_other.SaveDiff(_new: true, _difs);
			}
		}

		public override void SaveDiff(bool _new, Differences _difs)
		{
			base.SaveDiff(_new, _difs);
			if (_new)
			{
				_difs.mSizes.Add(mSize);
				_difs.mSize += mSize;
			}
		}
	}

	private class DirData : BaseData
	{
		private List<BaseData> mContent = new List<BaseData>();

		public DirData(string _name, DirData _parent)
			: base(_name, _parent)
		{
		}

		public DirData(XmlNode _node, DirData _parent)
			: base(_node, _parent)
		{
			LoadContent(_node);
		}

		public void Add(BaseData _data)
		{
			mContent.Add(_data);
		}

		public void Clear()
		{
			mContent.Clear();
		}

		public override XmlElement Save(XmlElement _parent, XmlDocument _doc)
		{
			XmlElement xmlElement = Save(_parent, _doc, "dir");
			SaveContent(xmlElement, _doc);
			return xmlElement;
		}

		public void SaveContent(XmlElement _parent, XmlDocument _doc)
		{
			foreach (BaseData item in mContent)
			{
				item.Save(_parent, _doc);
			}
		}

		public void LoadContent(XmlNode _node)
		{
			Clear();
			for (_node = _node.FirstChild; _node != null; _node = _node.NextSibling)
			{
				switch (_node.Name)
				{
				case "dir":
					new DirData(_node, this);
					break;
				case "file":
					new FileData(_node, this);
					break;
				}
			}
		}

		public override void Compare(BaseData _other, Differences _difs)
		{
			DirData dirData = _other as DirData;
			if (dirData == null)
			{
				return;
			}
			BaseData data;
			foreach (BaseData item in mContent)
			{
				data = item;
				BaseData baseData = dirData.mContent.Find((BaseData elem) => elem.Equals(data));
				if (baseData == null)
				{
					data.SaveDiff(_new: false, _difs);
					continue;
				}
				data.Compare(baseData, _difs);
				dirData.mContent.Remove(baseData);
			}
			foreach (BaseData item2 in dirData.mContent)
			{
				item2.SaveDiff(_new: true, _difs);
			}
		}

		public override void SaveDiff(bool _new, Differences _difs)
		{
			base.SaveDiff(_new, _difs);
			if (!_new)
			{
				return;
			}
			foreach (BaseData item in mContent)
			{
				item.SaveDiff(_new: true, _difs);
			}
		}
	}

	private DirData mRoot = new DirData(".", null);

	public void ScanCurDir()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(".");
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			if (!IsHidden(fileInfo))
			{
				ScanFile(fileInfo.Name, mRoot);
			}
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		DirectoryInfo[] array2 = directories;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			if (!IsHidden(directoryInfo2))
			{
				ScanDir(directoryInfo2.Name, mRoot);
			}
		}
	}

	public void Scan(string _path)
	{
		//Discarded unreachable code: IL_0013
		bool flag;
		try
		{
			flag = IsFolder(_path);
		}
		catch (IOException)
		{
			return;
		}
		if (flag)
		{
			ScanDir(_path, mRoot);
		}
		else
		{
			ScanFile(_path, mRoot);
		}
	}

	public void Scan(string[] _paths)
	{
		if (_paths != null)
		{
			foreach (string path in _paths)
			{
				Scan(path);
			}
		}
	}

	private void ScanFile(string _name, DirData _dir)
	{
		FileData fileData = new FileData(_name, _dir);
		FileInfo fileInfo = new FileInfo(fileData.GetFullName());
		fileData.mHash = GetFileHash(fileInfo.FullName);
		fileData.mSize = fileInfo.Length;
	}

	private void ScanDir(string _name, DirData _dir)
	{
		DirData dirData = new DirData(_name, _dir);
		DirectoryInfo directoryInfo = new DirectoryInfo(dirData.GetFullName());
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			if (!IsHidden(fileInfo))
			{
				ScanFile(fileInfo.Name, dirData);
			}
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		DirectoryInfo[] array2 = directories;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			if (!IsHidden(directoryInfo2))
			{
				ScanDir(directoryInfo2.Name, dirData);
			}
		}
	}

	private bool IsFolder(string _path)
	{
		return (File.GetAttributes(_path) & FileAttributes.Directory) == FileAttributes.Directory;
	}

	private bool IsHidden(FileSystemInfo _info)
	{
		return (_info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
	}

	public byte[] GetFileHash(string _fn)
	{
		//Discarded unreachable code: IL_001c
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		FileStream fileStream;
		try
		{
			fileStream = new FileStream(_fn, FileMode.Open, FileAccess.Read);
		}
		catch (IOException)
		{
			return null;
		}
		byte[] result = mD5CryptoServiceProvider.ComputeHash(fileStream);
		fileStream.Close();
		return result;
	}

	public void Clear()
	{
		mRoot.Clear();
	}

	public void SaveContentData(string _fn)
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("content");
		xmlDocument.AppendChild(xmlElement);
		mRoot.SaveContent(xmlElement, xmlDocument);
		xmlDocument.Save(_fn);
	}

	public void LoadContentData(Stream _stream)
	{
		_stream.Flush();
		_stream.Position = 0L;
		StreamReader streamReader = new StreamReader(_stream);
		string xml = streamReader.ReadToEnd();
		ReadContentData(xml);
	}

	public void ReadContentData(string _xml)
	{
		Clear();
		if (string.IsNullOrEmpty(_xml))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(_xml);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("content");
			if (xmlNode != null)
			{
				mRoot.LoadContent(xmlNode);
			}
		}
		catch (IOException)
		{
		}
		catch (XmlException)
		{
		}
		catch (ArgumentException)
		{
		}
	}

	public Differences Compare(VersionInfo _otherVersion)
	{
		Differences differences = new Differences();
		mRoot.Compare(_otherVersion.mRoot, differences);
		return differences;
	}
}
