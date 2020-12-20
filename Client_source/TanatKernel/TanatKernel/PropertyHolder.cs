using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class PropertyHolder
	{
		private Dictionary<Type, Dictionary<int, object>> mContent = new Dictionary<Type, Dictionary<int, object>>();

		private List<int> mAvailableIds = new List<int>();

		public IEnumerable<int> AvailableIds => mAvailableIds;

		public T GetProperty<T>(int _protoId) where T : class
		{
			if (!mContent.TryGetValue(typeof(T), out var value))
			{
				return null;
			}
			value.TryGetValue(_protoId, out var value2);
			return value2 as T;
		}

		public void Clear()
		{
			mContent.Clear();
		}

		private void SaveProperty<T>(T _prop, int _id)
		{
			if (!mContent.TryGetValue(typeof(T), out var value))
			{
				value = new Dictionary<int, object>();
				mContent.Add(typeof(T), value);
			}
			value[_id] = _prop;
			if (!mAvailableIds.Contains(_id))
			{
				mAvailableIds.Add(_id);
			}
		}

		public void RetrieveProperties(int _protoId, string _xml)
		{
			if (string.IsNullOrEmpty(_xml))
			{
				Log.Warning("empty xml string");
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(_xml);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("Proto");
				if (xmlNode != null)
				{
					RetrieveProperty<BattlePrototype.PBattleDesc>("PDesc", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PPrefab>("PPrefab", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PTouchable>("PTouchable", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PItemContainer>("PItemContainer", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PBuilding>("PBuilding", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PShop>("PShop", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PShopSeller>("PShopSeller", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PItem>("PItem", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PAvatar>("PAvatar", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PDestructible>("PDestructible", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PCaster>("PCaster", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PExperiencer>("PExperiencer", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PProjectile>("PProjectile", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PEffectDesc>("PEffectDesc", xmlNode, _protoId);
					RetrieveProperty<BattlePrototype.PTool>("PTool", xmlNode, _protoId);
				}
			}
			catch (XmlException ex)
			{
				Log.Error("invalid prototype xml: " + ex.Message);
			}
		}

		private void RetrieveProperty<T>(string _nodeName, XmlNode _root, int _id) where T : IXmlLoadable, new()
		{
			XmlNode xmlNode = _root.SelectSingleNode(_nodeName);
			if (xmlNode != null)
			{
				T prop = new T();
				prop.Load(xmlNode);
				SaveProperty(prop, _id);
			}
		}

		public void RetrieveProperties(Stream _stream)
		{
			if (_stream == null)
			{
				throw new ArgumentNullException("_stream");
			}
			Formatter formatter = new Formatter();
			_stream.Position = 0L;
			Variable variable = formatter.Deserialize(_stream);
			if (variable == null || variable.ValueType != typeof(MixedArray))
			{
				Log.Warning("invalid root amf element");
				return;
			}
			MixedArray mixedArray = variable;
			foreach (Variable item in mixedArray.Dense)
			{
				if (item.ValueType == typeof(MixedArray))
				{
					try
					{
						RetrieveProperty<CtrlPrototype.PCtrlDesc>(item);
						RetrieveProperty<CtrlPrototype.PArticle>(item);
						RetrieveProperty<CtrlPrototype.PPrefab>(item);
					}
					catch (KeyNotFoundException)
					{
					}
				}
			}
		}

		private void RetrieveProperty<T>(MixedArray _data) where T : IAmfLoadable, new()
		{
			int id = _data["id"];
			T prop = new T();
			prop.Load(_data);
			SaveProperty(prop, id);
		}
	}
}
