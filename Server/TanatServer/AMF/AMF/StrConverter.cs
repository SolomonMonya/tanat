using System.IO;
using System.Text;

namespace AMF
{
	internal class StrConverter : DynamicLenConverter<string>
	{
		private StringRefTable mRefTable;

		public StrConverter(Converter<int> _lenConv, StringRefTable _refTable)
			: base(_lenConv)
		{
			mRefTable = _refTable;
		}

		public override void EncodeValue(string _val, Stream _output)
		{
			int num = -1;
			if (mRefTable != null)
			{
				num = mRefTable.GetStrId(_val);
			}
			if (num >= 0)
			{
				num <<= 1;
				mLengthConverter.EncodeValue(num, _output);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(_val);
			int val = (bytes.Length << 1) | 1;
			mLengthConverter.EncodeValue(val, _output);
			_output.Write(bytes, 0, bytes.Length);
			if (mRefTable != null)
			{
				mRefTable.AddStr(_val);
			}
		}

		public override string DecodeValue(Stream _input)
		{
			int num = mLengthConverter.Decode(_input);
			string text = "";
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				num >>= 1;
				if (num == 0)
				{
					return "";
				}
				if (num < 0)
				{
					return null;
				}
				byte[] array = new byte[num];
				_input.Read(array, 0, array.Length);
				text = Encoding.UTF8.GetString(array);
				if (mRefTable != null)
				{
					mRefTable.AddStr(text);
				}
			}
			else
			{
				int id = num >> 1;
				text = mRefTable.GetStr(id);
			}
			return text;
		}
	}
}
