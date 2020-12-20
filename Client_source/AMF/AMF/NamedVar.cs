using System;

namespace AMF
{
	public class NamedVar
	{
		private string mName;

		private Variable mVar;

		public string Name => mName;

		public Variable Var => mVar;

		public NamedVar(string _name, Variable _var)
		{
			if (string.IsNullOrEmpty(_name) || _var == null)
			{
				throw new ArgumentException();
			}
			mName = _name;
			mVar = _var;
		}

		public override string ToString()
		{
			return base.ToString() + " " + mName + " " + mVar.ToString();
		}
	}
}
