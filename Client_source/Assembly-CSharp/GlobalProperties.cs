using UnityEngine;

[ExecuteInEditMode]
public class GlobalProperties : MonoBehaviour
{
	public FogMode _FogMode = FogMode.ExponentialSquared;

	public bool _Fog;

	public float _FogNear = 30f;

	public float _FogFar = 100f;

	public float _FogDensity;

	public Color _FogColor = Color.grey;

	public Color _AbientLight = Color.white;

	public Color CharacterAmbient = Color.white;

	public Color CharacterEmission = Color.black;

	private void OnEnable()
	{
		RenderSettings.fogColor = _FogColor;
		RenderSettings.fogMode = _FogMode;
		RenderSettings.ambientLight = _AbientLight;
		RenderSettings.fog = _Fog;
		Shader.SetGlobalColor("_CEmiss", CharacterEmission);
		Shader.SetGlobalColor("_CAmbient", CharacterAmbient);
		RenderSettings.fog = _Fog;
		RenderSettings.fogDensity = _FogDensity;
		RenderSettings.fogStartDistance = _FogNear;
		RenderSettings.fogEndDistance = _FogFar;
	}
}
