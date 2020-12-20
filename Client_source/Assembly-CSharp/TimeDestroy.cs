using UnityEngine;

public class TimeDestroy : MonoBehaviour
{
	public float m_TimeToDestroy = 1f;

	private void Start()
	{
		Object.Destroy(base.gameObject, m_TimeToDestroy);
	}
}
