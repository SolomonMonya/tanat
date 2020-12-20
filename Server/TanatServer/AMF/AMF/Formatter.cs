using System;
using System.Collections.Generic;
using System.IO;

namespace AMF
{
	public class Formatter
	{
		private Dictionary<TypeMarker, IConverter> mConverters = new Dictionary<TypeMarker, IConverter>();

		private StringRefTable mStrRefTable = new StringRefTable();

		public Formatter()
		{
			U29Converter u29Converter = new U29Converter();
			StrConverter strConverter = new StrConverter(u29Converter, mStrRefTable);
			StrConverter value = new StrConverter(u29Converter, null);
			mConverters[TypeMarker.Integer] = u29Converter;
			mConverters[TypeMarker.Double] = new DoubleConverter();
			mConverters[TypeMarker.ByteArray] = new ByteArrayConverter(u29Converter);
			mConverters[TypeMarker.String] = strConverter;
			mConverters[TypeMarker.Xml] = value;
			mConverters[TypeMarker.XmlDoc] = value;
			mConverters[TypeMarker.Array] = new ArrayConverter(u29Converter, strConverter, this);
		}

		public long Serialize(Variable _var, Stream _output)
		{
			if (_var == null || _output == null)
			{
				throw new ArgumentNullException();
			}
			TypeMarker typeMarker = TypeMarkerRecognizer.Recognize(_var);
			_output.WriteByte((byte)typeMarker);
			long num = 1L;
			if (mConverters.TryGetValue(typeMarker, out var value))
			{
				num += value.Encode(_var, _output);
			}
			return num;
		}

		public Variable Deserialize(Stream _input)
		{
			TypeMarker typeMarker = (TypeMarker)_input.ReadByte();
			if (!mConverters.TryGetValue(typeMarker, out var value))
			{
				return typeMarker switch
				{
					TypeMarker.Undefined => new Variable(), 
					TypeMarker.Null => new Variable(), 
					TypeMarker.True => true, 
					TypeMarker.False => false, 
					_ => null, 
				};
			}
			return value.Decode(_input);
		}

		public void ClearRefTables()
		{
			mStrRefTable.Clear();
		}
	}
}
