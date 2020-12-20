using UnityEngine;

[AddComponentMenu("FXs/FreezeEffect")]
public class FreezeEffect : MonoBehaviour
{
	private Animation mAnimation;

	public void Start()
	{
		mAnimation = base.transform.parent.transform.gameObject.GetComponentInChildren<Animation>();
		if (mAnimation != null)
		{
			mAnimation.enabled = false;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void OnDisable()
	{
		if (mAnimation != null)
		{
			mAnimation.enabled = true;
		}
	}
}
