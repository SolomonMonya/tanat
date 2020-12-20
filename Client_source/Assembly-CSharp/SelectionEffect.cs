using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/SelectionEffect")]
public class SelectionEffect : MonoBehaviour
{
	public void Init(float _size, Vector3 _parentScale)
	{
		base.gameObject.layer = LayerMask.NameToLayer("Selection");
		if (_parentScale == Vector3.zero)
		{
			Log.Error("Bad parent scale");
			return;
		}
		float value = 0.15f + 0.4f / _size;
		base.renderer.material.SetFloat("_YTile", value);
		base.transform.localScale = new Vector3(_size, _size, _size) * 0.25f / _parentScale.x;
		base.transform.localPosition = new Vector3(0f, 0.3f / _parentScale.x, 0f);
		base.transform.eulerAngles = Vector3.zero;
	}

	public void SetColor(Color _color)
	{
		base.renderer.material.SetColor("_Color", _color);
	}
}
