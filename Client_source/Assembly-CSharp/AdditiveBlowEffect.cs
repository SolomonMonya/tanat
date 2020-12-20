using UnityEngine;

[AddComponentMenu("FXs/AdditiveBlowEffect")]
public class AdditiveBlowEffect : MonoBehaviour
{
	public string mColorName = "_TintColor";

	public Color colorStart = new Color(0.1f, 0.1f, 0.1f, 0.5f);

	public Color colorEnd = new Color(1f, 1f, 1f, 1f);

	public float duration = 2f;

	private float curDuration;

	public float randomDuration;

	private void Start()
	{
		curDuration = duration;
	}

	private void Update()
	{
		float num = Mathf.PingPong(Time.time, curDuration) / curDuration;
		if (num < 0.05f)
		{
			curDuration = duration + Random.Range(0f - randomDuration, randomDuration);
		}
		base.renderer.material.SetColor(mColorName, Color.Lerp(colorStart, colorEnd, num));
	}
}
