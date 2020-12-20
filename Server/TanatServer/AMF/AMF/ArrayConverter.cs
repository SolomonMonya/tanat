using System;
using System.Collections.Generic;
using System.IO;

namespace AMF
{
	internal class ArrayConverter : DynamicLenConverter<MixedArray>
	{
		private StrConverter mStrConverter;

		private Formatter mFormatter;

		public ArrayConverter(Converter<int> _lenConv, StrConverter _strConv, Formatter _formatter)
			: base(_lenConv)
		{
			if (_strConv == null || _formatter == null)
			{
				throw new ArgumentNullException();
			}
			mStrConverter = _strConv;
			mFormatter = _formatter;
		}

		public override void EncodeValue(MixedArray _val, Stream _output)
		{
			int val = (_val.Dense.Count << 1) | 1;
			mLengthConverter.EncodeValue(val, _output);
			foreach (KeyValuePair<string, Variable> item in _val.Associative)
			{
				mStrConverter.EncodeValue(item.Key, _output);
				mFormatter.Serialize(item.Value, _output);
			}
			mStrConverter.EncodeValue("", _output);
			foreach (Variable item2 in _val.Dense)
			{
				mFormatter.Serialize(item2, _output);
			}
		}

		public override MixedArray DecodeValue(Stream _input)
		{
			int num = mLengthConverter.DecodeValue(_input);
			if ((num & 1) == 0)
			{
				throw new NotImplementedException("AMF reference to Array");
			}
			num >>= 1;
			MixedArray mixedArray = new MixedArray();
			while (true)
			{
				string text = mStrConverter.DecodeValue(_input);
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				Variable variable = mFormatter.Deserialize(_input);
				if (variable == null)
				{
					break;
				}
				mixedArray[text] = variable;
			}
			for (int num2 = num; num2 > 0; num2--)
			{
				Variable var = mFormatter.Deserialize(_input);
				mixedArray.Add(var);
			}
			return mixedArray;
		}
	}
}
