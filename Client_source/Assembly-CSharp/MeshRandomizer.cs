using UnityEngine;

public class MeshRandomizer : MonoBehaviour
{
	public Mesh[] mMeshes;

	public void Start()
	{
		if (mMeshes.Length > 0)
		{
			MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
			if (null != component)
			{
				Mesh mesh = mMeshes[Random.Range(0, mMeshes.Length)];
				if (null != mesh)
				{
					component.mesh = mesh;
				}
			}
		}
		Object.Destroy(this);
	}
}
