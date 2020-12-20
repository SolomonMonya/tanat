using UnityEngine;

public class SmoothTransparency : MonoBehaviour
{
	public float mEnd;

	public float mSpeed;

	public Shader mFinalShader;

	private float mStart;

	private float mStartTime;

	public void Init()
	{
		mStart = base.gameObject.renderer.material.color.a;
		mStartTime = Time.realtimeSinceStartup;
	}

	public void Update()
	{
		Color color = base.gameObject.renderer.material.color;
		float num = Time.realtimeSinceStartup - mStartTime;
		color.a = Mathf.Lerp(mStart, mEnd, num * mSpeed);
		base.gameObject.renderer.material.color = color;
		if (color.a - 0.001f < mEnd && color.a + 0.001f > mEnd)
		{
			if (null != mFinalShader)
			{
				base.gameObject.renderer.material.shader = mFinalShader;
			}
			Object.Destroy(this);
		}
	}
}
