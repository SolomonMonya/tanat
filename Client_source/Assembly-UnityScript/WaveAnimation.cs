using System;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class WaveAnimation : MonoBehaviour
{
	public GameObject[] siblings;

	public int index;

	public float offset;

	public float slideMin;

	public float slideMax;

	public float slideSpeed;

	public float slideSharpness;

	public float scaleMin;

	public float scaleMax;

	public float scaleSpeed;

	public float scaleSharpness;

	public float fadeSpeed;

	public Vector3 baseScroll;

	public float baseRotation;

	public Vector3 baseScale;

	private Material theMaterial;

	private float slide;

	private float slideInertia;

	private float scale;

	private float scaleInertia;

	private Vector3 basePos;

	private Vector3 texScale;

	private float lastSlide;

	private float fade;

	private Color color;

	private Color fadeColor;

	public WaveAnimation original;

	public WaveAnimation()
	{
		siblings = new GameObject[0];
		slideMin = -0.1f;
		slideMax = 0.4f;
		slideSpeed = 0.5f;
		slideSharpness = 1f;
		scaleMin = 1f;
		scaleMax = 0.4f;
		scaleSpeed = 0.5f;
		scaleSharpness = 0.5f;
		baseScroll = new Vector3(0.1f, 0f, 0.3547f);
		baseScale = new Vector3(10f, 10f, 10f);
		fade = 1f;
	}

	public override void Start()
	{
		CheckHWSupport();
		UnityScript.Lang.Array array = new UnityScript.Lang.Array();
		array = GetComponents(typeof(WaveAnimation));
		if (array.length == 1 && original == null)
		{
			original = this;
		}
		int i = 0;
		GameObject[] array2 = siblings;
		for (int length = array2.Length; i < length; i = checked(i + 1))
		{
			AddCopy(array2[i], original, copy: false);
		}
		if (array.length < Extensions.get_length((System.Array)renderer.materials))
		{
			AddCopy(gameObject, original, copy: true);
		}
		theMaterial = renderer.materials[index];
		color = theMaterial.GetColor("_Color");
		fadeColor = color;
		fadeColor.a = 0f;
		texScale = theMaterial.GetTextureScale("_MainTex");
	}

	private void CheckHWSupport()
	{
		bool isSupported = renderer.sharedMaterial.shader.isSupported;
		int i = 0;
		GameObject[] array = siblings;
		for (int length = array.Length; i < length; i = checked(i + 1))
		{
			array[i].renderer.enabled = isSupported;
		}
		renderer.enabled = isSupported;
	}

	public override void Update()
	{
		CheckHWSupport();
		slideInertia = Mathf.Lerp(slideInertia, Mathf.PingPong(Time.time * scaleSpeed + offset, 1f), slideSharpness * Time.deltaTime);
		slide = Mathf.Lerp(slide, slideInertia, slideSharpness * Time.deltaTime);
		theMaterial.SetTextureOffset("_MainTex", new Vector3((float)index * 0.35f, Mathf.Lerp(slideMin, slideMax, slide) * 2f, 0f));
		theMaterial.SetTextureOffset("_Cutout", new Vector3((float)index * 0.79f, Mathf.Lerp(slideMin, slideMax, slide) / 2f, 0f));
		fade = Mathf.Lerp(fade, (!(slide - lastSlide <= 0f)) ? 1 : 0, Time.deltaTime * fadeSpeed);
		lastSlide = slide;
		theMaterial.SetColor("_Color", Color.Lerp(fadeColor, color, fade));
		scaleInertia = Mathf.Lerp(scaleInertia, Mathf.PingPong(Time.time * scaleSpeed + offset, 1f), scaleSharpness * Time.deltaTime);
		scale = Mathf.Lerp(scale, scaleInertia, scaleSharpness * Time.deltaTime);
		theMaterial.SetTextureScale("_MainTex", new Vector3(texScale.x, Mathf.Lerp(scaleMin, scaleMax, scale), texScale.z));
		basePos += baseScroll * Time.deltaTime;
		Vector3 s = new Vector3(1f / baseScale.x, 1f / baseScale.y, 1f / baseScale.z);
		Matrix4x4 matrix = Matrix4x4.TRS(basePos, Quaternion.Euler(baseRotation, 90f, 90f), s);
		theMaterial.SetMatrix("_WavesBaseMatrix", matrix);
	}

	public override void AddCopy(GameObject ob, WaveAnimation original, bool copy)
	{
		WaveAnimation waveAnimation = (WaveAnimation)ob.AddComponent(typeof(WaveAnimation));
		waveAnimation.original = original;
		if (copy)
		{
			waveAnimation.index = checked(index + 1);
		}
		else
		{
			waveAnimation.index = index;
		}
		waveAnimation.offset = original.offset + 2f / UnityBuiltins.parseFloat(Extensions.get_length((System.Array)renderer.materials));
		waveAnimation.slideMin = original.slideMin;
		waveAnimation.slideMax = original.slideMax;
		waveAnimation.slideSpeed = original.slideSpeed + UnityEngine.Random.Range((0f - original.slideSpeed) / 5f, original.slideSpeed / 5f);
		waveAnimation.slideSharpness = original.slideSharpness + UnityEngine.Random.Range((0f - original.slideSharpness) / 5f, original.slideSharpness / 5f);
		waveAnimation.scaleMin = original.scaleMin;
		waveAnimation.scaleMax = original.scaleMax;
		waveAnimation.scaleSpeed = original.scaleSpeed + UnityEngine.Random.Range((0f - original.scaleSpeed) / 5f, original.scaleSpeed / 5f);
		waveAnimation.scaleSharpness = original.scaleSharpness + UnityEngine.Random.Range((0f - original.scaleSharpness) / 5f, original.scaleSharpness / 5f);
		waveAnimation.fadeSpeed = original.fadeSpeed;
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere.y = 0f;
		waveAnimation.baseScroll = onUnitSphere.normalized * original.baseScroll.magnitude;
		waveAnimation.baseRotation = UnityEngine.Random.Range(0, 360);
		waveAnimation.baseScale = original.baseScale * UnityEngine.Random.Range(0.8f, 1.2f);
	}

	public override void Main()
	{
	}
}
