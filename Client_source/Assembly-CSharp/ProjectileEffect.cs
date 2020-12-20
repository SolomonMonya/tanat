using UnityEngine;

public class ProjectileEffect : MonoBehaviour
{
	public int mProjectileEffectNum = -1;

	public void Start()
	{
		if (mProjectileEffectNum == -1)
		{
			Object.Destroy(this);
		}
	}
}
