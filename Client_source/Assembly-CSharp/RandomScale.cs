using UnityEngine;

[AddComponentMenu("Object Scripts/Transform Scripts/Random Scale")]
[ExecuteInEditMode]
public class RandomScale : MonoBehaviour
{
	public float scaleCoef = 1f;

	public float scaleSpeed = 1f;

	public float speedVar = 1f;

	private Vector3 baseScale;

	private float prevScale;

	private float scaleVel;

	private float scaleStart = 1f;

	private float scaleEnd = -1f;

	private bool mVisible;

	private void Start()
	{
		baseScale = base.gameObject.transform.localScale;
		prevScale = 1f;
		mVisible = false;
	}

	private void Init()
	{
		prevScale = 0f;
		scaleVel = 0f;
		scaleStart = 1f;
		scaleEnd = -1f;
		prevScale = 1f;
	}

	private void Update()
	{
		if (!mVisible)
		{
			return;
		}
		float num = prevScale + scaleVel * (Time.time - scaleStart);
		if (Time.time > scaleEnd)
		{
			prevScale = num;
			float num2 = 1f + scaleCoef * (1f - 2f * Random.value);
			scaleVel = scaleSpeed + speedVar * (1f - 2f * Random.value);
			scaleStart = Time.time;
			scaleEnd = scaleStart + Mathf.Abs(num2 - num) / scaleVel;
			if (num2 < num)
			{
				scaleVel = 0f - scaleVel;
			}
		}
		base.gameObject.transform.localScale = baseScale * num;
	}

	private void OnBecameVisible()
	{
		mVisible = true;
	}

	private void OnBecameInvisible()
	{
		mVisible = false;
		Init();
	}
}
