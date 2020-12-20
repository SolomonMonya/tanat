using UnityEngine;

[AddComponentMenu("FXs/RandomTimeOffset")]
public class RandomTimeOffset : MonoBehaviour
{
	private void Start()
	{
		foreach (AnimationState item in base.animation)
		{
			item.normalizedTime = Random.value;
			item.speed = 0.85f + Random.value * 0.3f;
		}
		Object.Destroy(this);
	}
}
