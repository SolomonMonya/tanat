using System.Globalization;
using System.Xml;

namespace TanatKernel
{
	public class PossabilityMap
	{
		public struct Point
		{
			public float mX;

			public float mY;
		}

		public class Polygon
		{
			public Point[] mPoints;
		}

		public Polygon[] mPolygons;

		public void Load(string _fn)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(_fn);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("Map");
				if (xmlNode != null)
				{
					xmlNode = xmlNode.SelectSingleNode("Landscape");
					LoadObstacle(xmlNode);
				}
			}
			catch (XmlException)
			{
			}
		}

		private void LoadObstacle(XmlNode _node)
		{
			if (_node == null)
			{
				return;
			}
			_node = _node.SelectSingleNode("Obstacle");
			if (_node == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = _node.SelectNodes("Segment");
			if (xmlNodeList == null)
			{
				return;
			}
			mPolygons = new Polygon[xmlNodeList.Count];
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				mPolygons[i] = new Polygon();
				XmlNode xmlNode = xmlNodeList[i];
				XmlAttribute xmlAttribute = xmlNode.Attributes["value"];
				if (xmlAttribute == null || !(xmlAttribute.InnerText == "poly"))
				{
					continue;
				}
				_node = xmlNode.SelectSingleNode("Vertices");
				if (_node == null)
				{
					continue;
				}
				xmlAttribute = _node.Attributes["value"];
				if (xmlAttribute != null)
				{
					string[] array = xmlAttribute.InnerText.Split(';');
					mPolygons[i].mPoints = new Point[array.Length];
					for (int j = 0; j < array.Length; j++)
					{
						ref Point reference = ref mPolygons[i].mPoints[j];
						reference = String2Point(array[j]);
					}
				}
			}
		}

		private string Point2String(Point _p)
		{
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberDecimalSeparator = ".";
			return _p.mX.ToString(numberFormatInfo) + " " + _p.mY.ToString(numberFormatInfo);
		}

		private Point String2Point(string _s)
		{
			Point result = default(Point);
			string[] array = _s.Split(' ');
			if (array.Length == 2)
			{
				NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
				numberFormatInfo.NumberDecimalSeparator = ".";
				result.mX = float.Parse(array[0], numberFormatInfo);
				result.mY = float.Parse(array[1], numberFormatInfo);
			}
			return result;
		}
	}
}
