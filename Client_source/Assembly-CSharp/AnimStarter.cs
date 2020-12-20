using UnityEngine;

public class AnimStarter : MonoBehaviour
{
	private Animation mAnim;

	public void OnEnable()
	{
		mAnim = base.gameObject.GetComponent<Animation>();
		if (!(mAnim == null))
		{
			PlayAnim();
		}
	}

	public void Update()
	{
		if (!(mAnim == null) && !mAnim.isPlaying)
		{
			PlayAnim();
		}
	}

	private void PlayAnim()
	{
		foreach (AnimationState item in mAnim)
		{
			item.wrapMode = WrapMode.ClampForever;
		}
		mAnim.Play();
	}
}
