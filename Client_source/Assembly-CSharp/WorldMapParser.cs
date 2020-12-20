using System;
using System.Collections.Generic;
using System.Xml;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class WorldMapParser
{
	public List<WorldMapMenu.MapElement> GetMapElements(string _xml)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(_xml);
		return TutorialMgr.ParseList(xmlDocument.SelectSingleNode("WorldMap"), MapElementParser);
	}

	private WorldMapMenu.MapElement MapElementParser(XmlNode _node)
	{
		if (_node == null)
		{
			return null;
		}
		WorldMapMenu.MapElement mapElement = new WorldMapMenu.MapElement();
		XmlNode xmlNode = _node.SelectSingleNode("Id");
		if (xmlNode == null)
		{
			Log.Error("Wrong value for id.");
			return null;
		}
		mapElement.mId = int.Parse(xmlNode.InnerText.Trim());
		XmlNode xmlNode2 = _node.SelectSingleNode("Location");
		if (xmlNode2 == null)
		{
			mapElement.mLocation = null;
		}
		else
		{
			mapElement.mLocation = (Location)(int)Enum.Parse(typeof(Location), xmlNode2.InnerText.Trim(), ignoreCase: true);
		}
		XmlNode xmlNode3 = _node.SelectSingleNode("Type");
		if (xmlNode3 == null)
		{
			mapElement.mMapType = null;
		}
		else
		{
			mapElement.mMapType = (MapType)(int)Enum.Parse(typeof(MapType), xmlNode3.InnerText.Trim(), ignoreCase: true);
		}
		XmlNode xmlNode4 = _node.SelectSingleNode("Position");
		if (xmlNode4 != null)
		{
			string[] array = xmlNode4.InnerText.Trim().Split(',');
			if (array.Length == 2)
			{
				if (!TutorialMgr.TryParse(array[0], out var _result))
				{
					Log.Error("Wrong value for position x element: " + array[0]);
				}
				if (!TutorialMgr.TryParse(array[1], out var _result2))
				{
					Log.Error("Wrong value for position y element: " + array[1]);
				}
				mapElement.mPos = new Vector2(_result, _result2);
			}
			else
			{
				Log.Error("Wrong value for position element: " + xmlNode4.InnerText.Trim());
			}
			return mapElement;
		}
		Log.Error("No value for position.");
		return null;
	}
}
