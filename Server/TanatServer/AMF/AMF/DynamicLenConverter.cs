using System;

namespace AMF
{
	internal abstract class DynamicLenConverter<T> : Converter<T>
	{
		protected Converter<int> mLengthConverter;

		public DynamicLenConverter(Converter<int> _lenConv)
		{
			if (_lenConv == null)
			{
				throw new ArgumentNullException();
			}
			mLengthConverter = _lenConv;
		}
	}
}
