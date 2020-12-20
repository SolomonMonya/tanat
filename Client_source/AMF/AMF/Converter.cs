using System.IO;

namespace AMF
{
	internal abstract class Converter<T> : IConverter
	{
		public long Encode(Variable _var, Stream _output)
		{
			T val = _var.Cast<T>();
			long position = _output.Position;
			EncodeValue(val, _output);
			return _output.Position - position;
		}

		public Variable Decode(Stream _input)
		{
			T val = DecodeValue(_input);
			return new Variable(val);
		}

		public abstract void EncodeValue(T _val, Stream _output);

		public abstract T DecodeValue(Stream _input);
	}
}
