using System;
using System.Collections.Generic;
using System.Xml;
using Log4Tanat;

namespace TanatKernel
{
	public class LocaleState
	{
		private string mLang;

		private Dictionary<string, string> mTexts = new Dictionary<string, string>();

		public string Lang => mLang;

		public LocaleState(string _lang)
		{
			if (string.IsNullOrEmpty(_lang))
			{
				Log.Error("undefined language");
			}
			mLang = _lang;
		}

		public string GetText(string _id)
		{
			if (string.IsNullOrEmpty(_id))
			{
				Log.Warning("empty string id\n" + Log.StackTrace());
				return "";
			}
			if (!mTexts.TryGetValue(_id, out var value))
			{
				return "EMPTY!";
			}
			return value;
		}

		public void LoadFromFile(string _fn)
		{
			if (string.IsNullOrEmpty(_fn))
			{
				throw new ArgumentNullException();
			}
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(_fn);
				Load(xmlDocument);
			}
			catch (XmlException ex)
			{
				Log.Error("error while loading locale: " + ex.Message);
			}
		}

		public void LoadFromXml(string _xml)
		{
			if (string.IsNullOrEmpty(_xml))
			{
				throw new ArgumentNullException();
			}
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(_xml);
				Load(xmlDocument);
			}
			catch (XmlException ex)
			{
				Log.Error("error while loading locale: " + ex.Message);
			}
		}

		private void Load(XmlDocument _doc)
		{
			try
			{
				XmlNode xmlNode = _doc.SelectSingleNode("locale");
				if (xmlNode == null)
				{
					Log.Error("cannot find locale root element");
					return;
				}
				XmlNodeList xmlNodeList = xmlNode.SelectNodes("lang");
				XmlNode xmlNode2 = null;
				foreach (XmlNode item in xmlNodeList)
				{
					string text = XmlUtil.SafeReadText("id", item);
					if (text == mLang)
					{
						xmlNode2 = item;
						break;
					}
				}
				if (xmlNode2 == null)
				{
					Log.Error("cannot find texts for " + mLang + " language");
					return;
				}
				XmlNodeList xmlNodeList2 = xmlNode2.SelectNodes("data");
				foreach (XmlNode item2 in xmlNodeList2)
				{
					string text2 = XmlUtil.SafeReadText("id", item2);
					if (string.IsNullOrEmpty(text2))
					{
						Log.Warning("locale contains empty text id");
						continue;
					}
					string value = XmlUtil.SafeReadText("value", item2);
					try
					{
						mTexts.Add(text2, value);
					}
					catch (ArgumentException)
					{
						Log.Warning("locale already contains text for id " + text2);
					}
				}
			}
			catch (XmlException ex2)
			{
				Log.Error("error while loading locale: " + ex2.Message);
			}
		}
	}
}
