using System;
using System.IO;

namespace AMF
{
	internal class DoubleConverter : Converter<double>
	{
		public override void EncodeValue(double _val, Stream _output)
		{
			byte[] bytes = BitConverter.GetBytes(_val);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			_output.Write(bytes, 0, bytes.Length);
		}

		public override double DecodeValue(Stream _input)
		{
			byte[] array = new byte[8];
			int num = _input.Read(array, 0, array.Length);
			if (num == array.Length)
			{
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(array);
				}
				return BitConverter.ToDouble(array, 0);
			}
			return 0.0;
		}
	}
}
