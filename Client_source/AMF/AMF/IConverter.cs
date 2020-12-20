using System.IO;

namespace AMF
{
	internal interface IConverter
	{
		long Encode(Variable _var, Stream _output);

		Variable Decode(Stream _input);
	}
}
