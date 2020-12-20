using UnityEngine;

[AddComponentMenu("FXs/PrefabTimeSpawn")]
public class PrefabTimeSpawn : MonoBehaviour
{
	public GameObject m_Prefab;

	public float m_SpawnTime = 1f;

	private float m_Time;

	public float m_SpawnRadius;

	public float m_Randomize;

	public bool mGround;

	public bool m_UseParentTransorm;

	private void Update()
	{
		m_Time += Time.deltaTime;
		if (!(m_Time >= m_SpawnTime))
		{
			return;
		}
		if (m_UseParentTransorm)
		{
			Object.Instantiate(m_Prefab, base.transform.position, base.transform.rotation);
			m_Time = 0f;
			return;
		}
		Quaternion rotation = Random.rotation;
		rotation.y = base.transform.rotation.y;
		rotation.x = base.transform.rotation.x;
		Vector3 vector = new Vector3(Random.Range(0f - m_SpawnRadius, m_SpawnRadius), 0f, Random.Range(0f - m_SpawnRadius, m_SpawnRadius));
		if (mGround)
		{
			if (Physics.Raycast(new Ray(base.transform.position + vector + Vector3.up * 1024f, Vector3.down), out var hitInfo, 2048f, 1024) && hitInfo.collider.gameObject != null)
			{
				vector.y = hitInfo.point.y;
			}
		}
		else
		{
			vector.y = base.transform.position.y;
		}
		GameObject gameObject = Object.Instantiate(m_Prefab) as GameObject;
		GameObjUtil.TrySetParent(gameObject, "/effects");
		vector.x += base.transform.position.x;
		vector.z += base.transform.position.z;
		float num = Random.Range(0f - m_Randomize, m_Randomize);
		gameObject.transform.position = vector;
		gameObject.transform.Rotate(new Vector3(num, Random.Range(0f - m_Randomize, m_Randomize), num));
		m_Time = 0f;
	}
}
