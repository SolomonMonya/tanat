using System;
using System.Text;

namespace AMF
{
	public class Variable
	{
		private object mValue;

		public Type ValueType
		{
			get
			{
				if (mValue == null)
				{
					return null;
				}
				return mValue.GetType();
			}
		}

		public object Value => mValue;

		public Variable()
		{
		}

		public Variable(object _val)
		{
			mValue = _val;
		}

		public T Cast<T>()
		{
			if (mValue != null)
			{
				try
				{
					return (T)Convert.ChangeType(mValue, typeof(T));
				}
				catch (InvalidCastException)
				{
					object obj = null;
					if (typeof(T) == typeof(string))
					{
						obj = ((typeof(byte[]) != mValue.GetType()) ? mValue.ToString() : ByteArrToString((byte[])mValue));
					}
					else if (typeof(T) == typeof(MixedArray))
					{
						MixedArray mixedArray = new MixedArray();
						mixedArray.Dense.Add(this);
						obj = mixedArray;
					}
					else if (typeof(T) == typeof(byte[]))
					{
						byte[] array = null;
						Type type = mValue.GetType();
						if (typeof(int) == type)
						{
							array = BitConverter.GetBytes((int)mValue);
						}
						else if (typeof(double) == type)
						{
							array = BitConverter.GetBytes((double)mValue);
						}
						else if (typeof(float) == type)
						{
							array = BitConverter.GetBytes((float)mValue);
						}
						else if (typeof(bool) == type)
						{
							array = BitConverter.GetBytes((bool)mValue);
						}
						else if (typeof(string) == type)
						{
							array = Encoding.UTF8.GetBytes((string)mValue);
						}
						obj = array;
					}
					if (obj != null)
					{
						return (T)obj;
					}
				}
				catch (OverflowException)
				{
				}
			}
			return default(T);
		}

		public override string ToString()
		{
			string text = base.ToString() + " ";
			if (mValue == null)
			{
				text += "null";
			}
			else
			{
				if (mValue.GetType() != typeof(MixedArray))
				{
					text = text + mValue.GetType().FullName + " ";
				}
				if (mValue.GetType() == typeof(byte[]))
				{
					byte[] array = (byte[])mValue;
					object obj = text;
					text = string.Concat(obj, "[", array.Length, "]");
					if (array.Length > 0)
					{
						text = text + " " + ByteArrToString(array);
					}
				}
				else
				{
					text += mValue.ToString();
				}
			}
			return text;
		}

		private string ByteArrToString(byte[] _arr)
		{
			if (_arr.Length == 0)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_arr[0].ToString());
			for (int i = 1; i < _arr.Length; i++)
			{
				stringBuilder.Append(" " + _arr[i]);
			}
			return stringBuilder.ToString();
		}

		public static bool Equal(Variable _var1, Variable _var2)
		{
			if (_var1 == null || _var2 == null)
			{
				throw new ArgumentNullException();
			}
			if (_var1.ValueType != _var2.ValueType)
			{
				return false;
			}
			if (_var1.mValue == null)
			{
				return true;
			}
			return _var1.mValue.Equals(_var2.mValue);
		}

		public static implicit operator Variable(int _val)
		{
			return new Variable(_val);
		}

		public static implicit operator int(Variable _var)
		{
			return _var.Cast<int>();
		}

		public static implicit operator Variable(double _val)
		{
			return new Variable(_val);
		}

		public static implicit operator double(Variable _var)
		{
			return _var.Cast<double>();
		}

		public static implicit operator Variable(float _val)
		{
			return new Variable(_val);
		}

		public static implicit operator float(Variable _var)
		{
			return _var.Cast<float>();
		}

		public static implicit operator Variable(bool _val)
		{
			return new Variable(_val);
		}

		public static implicit operator bool(Variable _var)
		{
			return _var.Cast<bool>();
		}

		public static implicit operator Variable(string _val)
		{
			return new Variable(_val);
		}

		public static implicit operator string(Variable _var)
		{
			return _var.Cast<string>();
		}

		public static implicit operator Variable(byte[] _val)
		{
			return new Variable(_val);
		}

		public static implicit operator byte[](Variable _var)
		{
			return _var.Cast<byte[]>();
		}

		public static implicit operator Variable(MixedArray _val)
		{
			return new Variable(_val);
		}

		public static implicit operator MixedArray(Variable _var)
		{
			return _var.Cast<MixedArray>();
		}
	}
}
