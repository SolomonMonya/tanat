using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Object Scripts/Transform Scripts/Billboard")]
public class Billboard : MonoBehaviour
{
	private bool mVisible;

	public bool mAlways;

	private void Start()
	{
		mVisible = false;
	}

	private void Update()
	{
		if ((mVisible || mAlways) && (bool)Camera.main)
		{
			base.gameObject.transform.LookAt(Camera.main.transform);
		}
	}

	private void OnBecameVisible()
	{
		mVisible = true;
	}

	private void OnBecameInvisible()
	{
		mVisible = false;
	}
}
