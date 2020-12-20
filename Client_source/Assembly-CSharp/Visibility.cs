using UnityEngine;

public class Visibility : MonoBehaviour
{
	public void OnEnable()
	{
		Object.Destroy(this);
	}
}
