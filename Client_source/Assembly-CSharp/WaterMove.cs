using UnityEngine;

[AddComponentMenu("FXs/WaterMove")]
public class WaterMove : MonoBehaviour
{
	private float mWaveScale;

	private Vector4 mWaveSpeed = Vector4.zero;

	private void Start()
	{
		mWaveSpeed = base.renderer.material.GetVector("WaveSpeed");
		mWaveScale = base.renderer.material.GetFloat("_WaveScale");
	}

	private void Update()
	{
		float num = Time.time / 20f;
		Vector4 vector = mWaveSpeed * (num * mWaveScale);
		float x = Mathf.Repeat(vector.x, 1f);
		float y = Mathf.Repeat(vector.y, 1f);
		float z = Mathf.Repeat(vector.z, 1f);
		float w = Mathf.Repeat(vector.w, 1f);
		Vector4 vector2 = new Vector4(x, y, z, w);
		base.renderer.material.SetVector("_WaveOffset", vector2);
	}
}
