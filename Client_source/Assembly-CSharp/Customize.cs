using UnityEngine;

public class Customize : MonoBehaviour
{
	private Transform[] mMaxBones;

	private Matrix4x4[] mMaxBinds;

	private SkinnedMeshRenderer mRenderer;

	public void Awake()
	{
		mRenderer = GetComponent<SkinnedMeshRenderer>();
		mMaxBones = mRenderer.bones;
		if (null != mRenderer.sharedMesh)
		{
			mMaxBinds = mRenderer.sharedMesh.bindposes;
		}
	}

	public void SetMesh(Mesh _mesh)
	{
		if (null == mRenderer)
		{
			return;
		}
		if (null == _mesh)
		{
			mRenderer.sharedMesh = _mesh;
			return;
		}
		Transform[] array = new Transform[_mesh.bindposes.Length];
		int i = 0;
		for (int num = _mesh.bindposes.Length; i < num; i++)
		{
			int j = 0;
			for (int num2 = mMaxBinds.Length; j < num2; j++)
			{
				if (_mesh.bindposes[i] == mMaxBinds[j])
				{
					array[i] = mMaxBones[j];
					break;
				}
			}
		}
		mRenderer.bones = array;
		mRenderer.sharedMesh = _mesh;
	}
}
