using Log4Tanat;
using UnityEngine;

public class UnityLogOutput : LogOutput
{
	public override void Write(Log.Category _category, string _caller, string _time, string _data)
	{
		Debug.Log(string.Concat(_category, " | ", _data));
	}
}
