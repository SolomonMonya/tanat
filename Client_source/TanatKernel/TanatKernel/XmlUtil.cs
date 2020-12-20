using System;
using System.Collections.Generic;
using System.Xml;
using Log4Tanat;

namespace TanatKernel
{
	public static class XmlUtil
	{
		public static string SafeReadText(string _attrName, XmlNode _node)
		{
			if (_node == null)
			{
				return "";
			}
			if (_node.Attributes == null)
			{
				return "";
			}
			XmlAttribute xmlAttribute = _node.Attributes[_attrName];
			if (xmlAttribute == null)
			{
				return "";
			}
			return xmlAttribute.InnerText;
		}

		public static int SafeReadInt(string _attrName, XmlNode _node)
		{
			string s = SafeReadText(_attrName, _node);
			int result = 0;
			try
			{
				result = int.Parse(s);
				return result;
			}
			catch (FormatException)
			{
				return result;
			}
			catch (OverflowException)
			{
				return result;
			}
		}

		public static float SafeReadFloat(string _attrName, XmlNode _node)
		{
			string s = SafeReadText(_attrName, _node);
			float result = 0f;
			try
			{
				result = float.Parse(s);
				return result;
			}
			catch (FormatException)
			{
				return result;
			}
		}

		public static bool SafeReadBool(string _attrName, XmlNode _node)
		{
			string text = SafeReadText(_attrName, _node);
			return text.Equals("true", StringComparison.OrdinalIgnoreCase);
		}

		public static bool SafeReadNumBool(string _attrName, XmlNode _node)
		{
			string text = SafeReadText(_attrName, _node);
			return text != "0";
		}

		public static List<string> SafeReadTextList(string _attrName, XmlNode _node)
		{
			return SafeReadTextList(_attrName, _node, ';');
		}

		public static List<string> SafeReadTextList(string _attrName, XmlNode _node, char _sep)
		{
			string text = SafeReadText(_attrName, _node);
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(_sep);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!string.IsNullOrEmpty(text2))
					{
						list.Add(text2);
					}
				}
			}
			return list;
		}

		public static string[] SafeReadTextArray(string _attrName, XmlNode _node)
		{
			List<string> list = SafeReadTextList(_attrName, _node);
			return list.ToArray();
		}

		public static List<int> SafeReadIntList(string _attrName, XmlNode _node)
		{
			List<int> list = new List<int>();
			List<string> list2 = SafeReadTextList(_attrName, _node);
			try
			{
				foreach (string item2 in list2)
				{
					int item = int.Parse(item2);
					list.Add(item);
				}
				return list;
			}
			catch (FormatException ex)
			{
				Log.Warning(ex.Message);
				list.Clear();
				return list;
			}
		}

		public static int[] SafeReadIntArray(string _attrName, XmlNode _node)
		{
			List<int> list = SafeReadIntList(_attrName, _node);
			return list.ToArray();
		}

		public static List<float> SafeReadFloatList(string _attrName, XmlNode _node)
		{
			return SafeReadFloatList(_attrName, _node, ';');
		}

		public static List<float> SafeReadFloatList(string _attrName, XmlNode _node, char _sep)
		{
			List<float> list = new List<float>();
			List<string> list2 = SafeReadTextList(_attrName, _node, _sep);
			try
			{
				foreach (string item2 in list2)
				{
					float item = float.Parse(item2);
					list.Add(item);
				}
				return list;
			}
			catch (FormatException ex)
			{
				Log.Warning(ex.Message);
				list.Clear();
				return list;
			}
		}

		public static float[] SafeReadFloatArray(string _attrName, XmlNode _node)
		{
			List<float> list = SafeReadFloatList(_attrName, _node);
			return list.ToArray();
		}

		public static T SafeReadEnum<T>(string _attrName, XmlNode _node)
		{
			string value = SafeReadText(_attrName, _node);
			object obj = null;
			try
			{
				obj = Enum.Parse(typeof(T), value);
			}
			catch (ArgumentException)
			{
			}
			if (obj == null)
			{
				return default(T);
			}
			return (T)obj;
		}

		public static List<int> StringToIntList(string _string, char _symb)
		{
			List<int> list = new List<int>();
			string text = "";
			int i = 0;
			for (int length = _string.Length; i < length; i++)
			{
				if (_symb != _string[i])
				{
					text += _string[i];
				}
				if (_symb == _string[i] || i == length - 1)
				{
					list.Add(int.Parse(text));
					text = "";
				}
			}
			return list;
		}
	}
}
