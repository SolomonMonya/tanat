using UnityEngine;

[AddComponentMenu("FXs/AriseBehavior")]
public class AriseBehavior : MonoBehaviour
{
	public delegate void OnDie(AriseBehavior _obj);

	public float flySpeed = 3f;

	public float ttl = 1.5f;

	public OnDie mOnDie;

	public AriseTextMgr.Style mFontStyle;

	public void OnEnable()
	{
		base.gameObject.transform.rotation = Camera.main.gameObject.transform.rotation;
		ttl = 1.5f;
	}

	public void Update()
	{
		base.gameObject.transform.Translate(Vector3.up * flySpeed * Time.deltaTime, Space.World);
		ttl -= Time.deltaTime;
		if (ttl < 0f)
		{
			base.gameObject.SetActiveRecursively(state: false);
			if (mOnDie != null)
			{
				mOnDie(this);
			}
		}
	}
}
