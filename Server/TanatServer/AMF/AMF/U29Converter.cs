using System.IO;

namespace AMF
{
	internal class U29Converter : Converter<int>
	{
		public override void EncodeValue(int _val, Stream _output)
		{
			_val &= 0x1FFFFFFF;
			byte[] array;
			if (_val <= 127)
			{
				array = new byte[1]
				{
					(byte)((uint)_val & 0x7Fu)
				};
			}
			else if (_val <= 16383)
			{
				array = new byte[2]
				{
					0,
					(byte)((uint)_val & 0x7Fu)
				};
				_val >>= 7;
				array[0] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
			}
			else if (_val <= 2097151)
			{
				array = new byte[3]
				{
					0,
					0,
					(byte)((uint)_val & 0x7Fu)
				};
				_val >>= 7;
				array[1] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
				_val >>= 7;
				array[0] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
			}
			else
			{
				array = new byte[4]
				{
					0,
					0,
					0,
					(byte)((uint)_val & 0xFFu)
				};
				_val >>= 8;
				array[2] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
				_val >>= 7;
				array[1] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
				_val >>= 7;
				array[0] = (byte)(((uint)_val & 0x7Fu) | 0x80u);
			}
			_output.Write(array, 0, array.Length);
		}

		public override int DecodeValue(Stream _input)
		{
			int num = 0;
			int num2 = 0;
			int num3;
			while ((num3 = _input.ReadByte()) != -1)
			{
				if (num2 == 3)
				{
					num <<= 8;
					num |= num3 & 0xFF;
				}
				else
				{
					num <<= 7;
					num |= num3 & 0x7F;
				}
				if ((num3 & 0x80) == 0)
				{
					break;
				}
				num2++;
				if (num2 >= 4)
				{
					break;
				}
			}
			if (((uint)num & 0x10000000u) != 0)
			{
				num |= -268435456;
			}
			return num;
		}
	}
}
