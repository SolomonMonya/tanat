using System;
using System.IO;

namespace AMF
{
	internal class ByteArrayConverter : DynamicLenConverter<byte[]>
	{
		public ByteArrayConverter(Converter<int> _lenConv)
			: base(_lenConv)
		{
		}

		public override void EncodeValue(byte[] _val, Stream _output)
		{
			int val = (_val.Length << 1) | 1;
			mLengthConverter.EncodeValue(val, _output);
			_output.Write(_val, 0, _val.Length);
		}

		public override byte[] DecodeValue(Stream _input)
		{
			int num = mLengthConverter.DecodeValue(_input);
			if ((num & 1) == 0)
			{
				throw new NotImplementedException("AMF reference to byte[]");
			}
			num >>= 1;
			byte[] array = new byte[num];
			_input.Read(array, 0, array.Length);
			return array;
		}
	}
}
