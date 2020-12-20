using UnityEngine;

[AddComponentMenu("FXs/AnimatedSkinRenderer")]
public class AnimatedSkinRenderer : MonoBehaviour
{
	public Material mMaterial;

	private void Start()
	{
		SkinnedMeshRenderer[] componentsInChildren = base.transform.parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in array)
		{
			if (!(skinnedMeshRenderer2.transform.parent.gameObject.GetComponent<AnimatedSkinRenderer>() != null))
			{
				skinnedMeshRenderer = Object.Instantiate(skinnedMeshRenderer2) as SkinnedMeshRenderer;
				skinnedMeshRenderer.transform.parent = base.transform;
				skinnedMeshRenderer.material = mMaterial;
				for (int j = 0; j < skinnedMeshRenderer.materials.Length; j++)
				{
					skinnedMeshRenderer.materials[j] = mMaterial;
				}
			}
		}
	}
}
