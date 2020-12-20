using UnityEngine;

public class FunctionalObjectData : MonoBehaviour
{
	public SceneObjectType mType;

	public int mTeam = -1;

	public string mName = string.Empty;

	public string mPrototype = string.Empty;

	public Vector2 mAreaSize = Vector2.zero;

	public Vector2 mPatrolCenter = Vector2.zero;

	public string mInstanceName = string.Empty;

	public void Start()
	{
		Object.Destroy(base.gameObject);
	}
}
