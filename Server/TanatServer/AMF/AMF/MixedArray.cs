using System;
using System.Collections.Generic;
using System.Text;

namespace AMF
{
	public class MixedArray
	{
		private Dictionary<string, Variable> mAssociative = new Dictionary<string, Variable>();

		private List<Variable> mDense = new List<Variable>();

		public Dictionary<string, Variable> Associative => mAssociative;

		public Variable this[string _key]
		{
			get
			{
				return mAssociative[_key];
			}
			set
			{
				mAssociative[_key] = value;
			}
		}

		public List<Variable> Dense => mDense;

		public Variable this[int _i] => mDense[_i];

		public bool TryGet<T>(string _key, T _default, out T _val)
		{
			if (string.IsNullOrEmpty(_key))
			{
				throw new ArgumentNullException("_key");
			}
			if (mAssociative.TryGetValue(_key, out var value) && value.Value != null)
			{
				_val = value.Cast<T>();
				return true;
			}
			_val = _default;
			return false;
		}

		public T TryGet<T>(string _key, T _default)
		{
			if (string.IsNullOrEmpty(_key))
			{
				throw new ArgumentNullException("_key");
			}
			if (mAssociative.TryGetValue(_key, out var value) && value.Value != null)
			{
				return value.Cast<T>();
			}
			return _default;
		}

		public void Set(NamedVar _nv)
		{
			if (_nv == null)
			{
				throw new ArgumentNullException();
			}
			mAssociative[_nv.Name] = _nv.Var;
		}

		public void Set(NamedVar[] _nvs)
		{
			if (_nvs == null)
			{
				throw new ArgumentNullException();
			}
			foreach (NamedVar nv in _nvs)
			{
				Set(nv);
			}
		}

		public void Add(Variable _var)
		{
			if (_var == null)
			{
				throw new ArgumentNullException();
			}
			mDense.Add(_var);
		}

		public void Clear()
		{
			mAssociative.Clear();
			mDense.Clear();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.ToString());
			stringBuilder.AppendLine("{");
			stringBuilder.Append("associative[" + mAssociative.Count + "]");
			if (mAssociative.Count > 0)
			{
				stringBuilder.AppendLine(":");
				foreach (KeyValuePair<string, Variable> item in mAssociative)
				{
					stringBuilder.AppendLine(item.Key + " " + item.Value.ToString());
				}
			}
			else
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("dense[" + mDense.Count + "]");
			if (mDense.Count > 0)
			{
				stringBuilder.AppendLine(":");
				for (int i = 0; i < mDense.Count; i++)
				{
					stringBuilder.AppendLine(i + " " + mDense[i].ToString());
				}
			}
			else
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
	}
}
